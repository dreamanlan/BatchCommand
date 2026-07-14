// ==UserScript==
// @name         MetaDSL Agent Bridge (Google AI Search)
// @version      1.0
// @description  Google AI Search page <-> local WebSocket code execution loop (single model)
// @match        *://*/*
// @grant        none
// ==/UserScript==

(function () {
  'use strict';

  /* ===========================================
     Section 0  Config
     =========================================== */
  const CFG = {
    WS_URL: null,         // set by DSL via ws_start command
    RECONNECT_MS: 3000,
    MAX_RECONNECT: 50,
    DEBOUNCE_MS: 1200,
    RESULT_FLUSH_MS: 2000,
    POLL_MS: 3000,
    LANG_FILTER: [],
    MAX_ROUNDS: 8,
    KEEP_ROUNDS: 6,
    RESET_TIMEOUT_MS: 30000,
    DEBUG: true,
    // Stream completion: required idle window after last text change.
    TEXT_STABLE_MS: 3000,
  };

  // Selectors aligned with the Google AI Search page structure.
  const SEL = {
    // Container for AI messages (cached at first detection - see ST.chatListEl).
    // Fallback root used by Observer until first AI message is found.
    chatListFallback: 'body',
    // AI message anchor: each represents one round of AI reply.
    aiMsg: '.mZJni[data-xid="VpUvz"]',
    // Code block container (also carries data-complete="true" when stream done).
    codeBlock: '.r1PmQe',
    codeBlockComplete: '.r1PmQe[data-complete="true"]',
    // Code text inside the block.
    codeText: 'pre code',
    // Real textarea for chat input.
    inputTA: 'textarea.ITIRGe[maxlength="8192"]',
    // Send button - must be precise to avoid voice-send-button twin.
    sendBtn: 'button[data-xid="input-plate-send-button"]',
  };

  /** Pick the first visible (rect non-zero) element matching selector. */
  function pickVisible(selector, root) {
    const all = (root || document).querySelectorAll(selector);
    for (const el of all) {
      const r = el.getBoundingClientRect();
      if (r.width > 0 && r.height > 0) return el;
    }
    return null;
  }

  /** Pick all visible (rect non-zero) elements matching selector as Array. */
  function pickAllVisible(selector, root) {
    const all = (root || document).querySelectorAll(selector);
    const out = [];
    for (const el of all) {
      const r = el.getBoundingClientRect();
      if (r.width > 0 && r.height > 0) out.push(el);
    }
    return out;
  }

  /* ===========================================
     Section 1  Global State (single model)
     =========================================== */
  const ST = {
    // Single WebSocket connection (no slot keying).
    ws: null,
    reconCnt: 0,
    // Pending DSL execution results awaiting flush to chat input.
    pendingResults: [],

    processedCodes: new WeakSet(),
    processedMsgs: new WeakSet(),
    paused: false,
    observer: null,
    debounceTimer: null,
    pollTimer: null,
    blockSeq: 0,
    flushTimer: null,
    // Per-message text-stability bookkeeping for empty-confirm path.
    emptyAt: new Map(),

    roundCount: 0,
    lastFlushTs: 0,
    breakerOn: false,
    panelCollapsed: false,
    panelEl: null,
    mergeNextFlush: false,

    // Recognition arming: when false, newly observed code blocks are
    // continuously absorbed into the processed sets as a rolling baseline
    // (no extraction, no WS send). Flip to true via the panel [arm] button.
    armed: false,
    chatSendLastLogTs: 0,
    sendFailAt: 0,
    sendFailInfo: '',
    // Last text auto-written to chat input by flushResults; used to
    // distinguish our own residual input from genuine user typing.
    lastAutoSentText: '',
    deferTimer: null,
    deferLastLogTs: 0,

    // Long-run mode: when true, auto-break on MAX_ROUNDS is disabled.
    longRunMode: false,

    // Auto-send mode: periodically check input box and click send if ready.
    autoSendOn: false,
    autoSendTimer: null,

    // M2: cached parent of first detected AI message, used as Observer target
    // and as chatList root. Falls back to document.body until set.
    chatListEl: null,
  };

  /* ===========================================
     Section 2  Logging
     =========================================== */
  const T = '[MetaDSL-GAI]';
  const log = (...a) => CFG.DEBUG && console.log(`%c${T}`, 'color:#00bcd4;font-weight:bold', ...a);
  const warn = (...a) => console.warn(`%c${T}`, 'color:#ff9800;font-weight:bold', ...a);
  const err = (...a) => console.error(`%c${T}`, 'color:#f44336;font-weight:bold', ...a);

  /** Always return document.body (chatList caching disabled - see tryCacheChatList). */
  function getChatRoot() {
    return document.body;
  }

  /**
   * Disabled (no-op). Previous M2 strategy cached aiMsg.parentElement, but on
   * Google AI Search that parent (.pWvJNd) is a per-message wrapper, not the
   * chat list. Observer bound there missed new AI msgs added to outer container.
   * Reverted to always-body strategy.
   */
  function tryCacheChatList() {
    // Intentionally empty.
  }

  /* ===========================================
     Section 3  Bridge (JS <-> DSL via callMetaDSL)
     =========================================== */
  const BRIDGE = {
    commandId: 0,
    callbacks: new Map(),
    nativeMode: (typeof callMetaDSL !== 'undefined'),
  };

  /**
   * Send a command to DSL (expects async response via onAgentResponse).
   */
  function bridgeSendCommand(cmd, params, cb) {
    const id = ++BRIDGE.commandId;
    if (cb) BRIDGE.callbacks.set(id, cb);
    const msg = JSON.stringify({ id, command: cmd, params: params || {} });
    if (BRIDGE.nativeMode) {
      setTimeout(() => { callMetaDSL('handle_agent_command', msg); }, 0);
    } else {
      warn('[bridge] callMetaDSL unavailable, command not sent:', cmd);
      if (cb) setTimeout(() => cb(false, null, 'native API unavailable'), 0);
    }
    return id;
  }

  /**
   * Send a notification to DSL (fire-and-forget, no response expected).
   */
  function bridgeSendNotification(type, data) {
    const msg = JSON.stringify({ type, data: data || {} });
    if (BRIDGE.nativeMode) {
      setTimeout(() => { callMetaDSL('handle_agent_notification', msg); }, 0);
    } else {
      log('[bridge] mock notification:', type);
    }
  }

  // DSL -> JS: command response callback
  window.onAgentResponse = function (responseJson) {
    try {
      const resp = JSON.parse(responseJson);
      const cb = BRIDGE.callbacks.get(resp.id);
      if (cb) {
        cb(resp.success, resp.data, resp.error);
        BRIDGE.callbacks.delete(resp.id);
      }
    } catch (e) {
      err('[bridge] response parse error', e);
    }
  };

  // DSL -> JS: proactive command (e.g. ws_start, send_message)
  window.onAgentCommand = function (commandJson) {
    try {
      const cmd = JSON.parse(commandJson);
      log('[bridge] onAgentCommand:', cmd.command);

      if (cmd.command === 'ws_start' && cmd.params && cmd.params.port) {
        CFG.WS_URL = 'ws://localhost:' + cmd.params.port;
        log(`[bridge] ws_start -> ${CFG.WS_URL}`);
        if (!ST.ws || ST.ws.readyState > 1) {
          ST.reconCnt = 0;
          ST.ws = wsCreate();
        }
        return;
      }
      if (cmd.command === 'ws_stop') {
        log('[bridge] ws_stop');
        if (ST.ws) {
          try { ST.ws.close(); } catch (_) { }
          ST.ws = null;
        }
        CFG.WS_URL = null;
        return;
      }
      if (cmd.command === 'send_message' && cmd.params && cmd.params.text) {
        ST.pendingResults.push(cmd.params.text);
        log(`[bridge] queued send_message (${cmd.params.text.length}B)`);
        scheduleFlush();
        return;
      }
      warn('[bridge] unknown command:', cmd.command);
    } catch (e) {
      err('[bridge] command parse error', e);
    }
  };

  /* ===========================================
     Section 4  WebSocket Management
     =========================================== */
  function wsCreate() {
    if (!CFG.WS_URL) {
      warn('[ws] WS_URL not set, skipping connect');
      return null;
    }
    log(`[ws] connecting -> ${CFG.WS_URL}`);
    let ws;
    try { ws = new WebSocket(CFG.WS_URL); } catch (e) {
      err('[ws] create failed', e);
      wsReconnect(); return null;
    }

    ws.onopen = () => {
      log('[ws] connected');
      ST.reconCnt = 0;
    };

    ws.onmessage = (ev) => {
      onWSMsg(typeof ev.data === 'string' ? ev.data : String(ev.data));
    };

    ws.onerror = () => err('[ws] error');
    ws.onclose = () => { warn('[ws] closed'); wsReconnect(); };
    return ws;
  }

  function wsReconnect() {
    if ((ST.reconCnt = (ST.reconCnt || 0) + 1) > CFG.MAX_RECONNECT) {
      err('[ws] reconnect limit reached'); return;
    }
    setTimeout(() => { ST.ws = wsCreate(); }, CFG.RECONNECT_MS);
  }

  function wsSend(text) {
    const ws = ST.ws;
    if (!ws || ws.readyState !== WebSocket.OPEN) {
      warn('[ws] not connected'); return false;
    }
    ws.send(text);
    return true;
  }

  /* ===========================================
     Section 5  Receive Exec Results + Flush
     =========================================== */
  function onWSMsg(text) {
    log(`[ws] recv ${text.length}B`);
    if (!text) return;
    ST.pendingResults.push(text);
    scheduleFlush();
  }

  function scheduleFlush() {
    clearTimeout(ST.flushTimer);
    ST.flushTimer = setTimeout(flushResults, CFG.RESULT_FLUSH_MS);
  }

  function totalPending() {
    return ST.pendingResults.length;
  }

  function flushResults() {
    if (ST.pendingResults.length === 0) return;

    // Not armed: keep queue, do not flush.
    if (!ST.armed) {
      log('[arm] not armed, results kept in queue');
      updatePanel();
      return;
    }

    // Breaker: keep queue, do not flush.
    if (ST.breakerOn) {
      log('[breaker] on, results kept in queue');
      updatePanel();
      return;
    }

    // User-typing guard: if chat input currently holds text that we did
    // NOT auto-write ourselves, the user is composing a message. Defer the
    // flush (keep queue intact) and retry in 2s.
    const taNow = pickVisible(SEL.inputTA);
    const curVal = taNow ? taNow.value : '';
    if (curVal && curVal !== ST.lastAutoSentText) {
      const nowTs = Date.now();
      if (nowTs - ST.deferLastLogTs > 1000) {
        ST.deferLastLogTs = nowTs;
        log('[flush] user typing, defer. ta.len=' + curVal.length);
      }
      clearTimeout(ST.deferTimer);
      ST.deferTimer = setTimeout(flushResults, 2000);
      updatePanel();
      return;
    }

    const now = Date.now();
    if (!ST.longRunMode && ST.lastFlushTs && now - ST.lastFlushTs > CFG.RESET_TIMEOUT_MS) {
      ST.roundCount = 0;
      log('[rounds] idle timeout, reset to 0');
    }

    const total = ST.pendingResults.length;
    const lines = [];

    // Queued dispatch: when more than one result is pending, only emit the
    // first one and keep the rest queued.
    if (total > 1) {
      const raw = ST.pendingResults.shift();
      lines.push('### exec result');
      lines.push('```\n' + raw.replace(/```/g, '`\u200b``').trimEnd() + '\n```');
      const remaining = total - 1;
      lines.push('');
      lines.push(`> 还有 ${remaining} 个代码结果在排队输出，不要再发新代码，回复继续即可`);
    } else {
      const raw = ST.pendingResults.shift();
      lines.push('### exec result');
      lines.push('```\n' + raw.replace(/```/g, '`\u200b``').trimEnd() + '\n```');
    }

    if (lines.length) {
      if (ST.mergeNextFlush) {
        ST.roundCount += 1;
        ST.mergeNextFlush = false;
      } else {
        ST.roundCount += 1;
      }
      ST.lastFlushTs = now;

      if (!ST.longRunMode && ST.roundCount >= CFG.MAX_ROUNDS) {
        ST.breakerOn = true;
        warn(`[breaker] reached ${CFG.MAX_ROUNDS} rounds, auto break`);
      }

      const text = lines.join('\n');
      log('[flush] send back:', text.slice(0, 120) + '...');
      ST.lastAutoSentText = text;
      chatSend(text);
    }
    updatePanel();
  }

  /* ===========================================
     Section 6  Breaker / Arm Controls
     =========================================== */
  function manualBreak() {
    ST.breakerOn = true;
    warn('[breaker] manual break');
    updatePanel();
  }
  function manualClear() {
    const n = ST.pendingResults.length;
    ST.pendingResults = [];
    log(`[breaker] queue cleared (${n})`);
    updatePanel();
  }
  function manualResume() {
    ST.breakerOn = false;
    ST.roundCount = 0;
    ST.mergeNextFlush = true;
    log('[breaker] resumed, rounds=0');
    if (ST.pendingResults.length > 0) scheduleFlush();
    updatePanel();
  }

  /** Treat all currently visible code blocks as baseline (no extract). */
  function baselineCodeOnly() {
    const root = getChatRoot();
    let n = 0;
    root.querySelectorAll(SEL.codeBlock).forEach(el => {
      if (!ST.processedCodes.has(el)) { ST.processedCodes.add(el); n++; }
    });
    log(`[baseline] absorbed ${n} existing code blocks`);
    return n;
  }

  function armNow() {
    if (ST.armed) { log('[arm] already armed'); return; }
    baselineCodeOnly();
    ST.armed = true;
    log('[arm] armed - new code blocks will be sent to WS');
    if (ST.pendingResults.length > 0) scheduleFlush();
    updatePanel();
  }

  function disarmNow() {
    if (!ST.armed) { log('[arm] already disarmed'); return; }
    ST.armed = false;
    log('[arm] disarmed - rolling baseline mode');
    updatePanel();
  }

  /* ===========================================
     Section 7  Floating Panel
     =========================================== */
  function createPanel() {
    if (ST.panelEl) return;
    const panel = document.createElement('div');
    panel.id = 'metadsl-gai-panel';
    panel.style.cssText = `
      position:fixed; top:10px; right:10px; z-index:99999;
      background:rgba(20,20,30,0.92); color:#e0e0e0;
      font:11px/1.4 monospace; border-radius:6px;
      box-shadow:0 2px 8px rgba(0,0,0,0.4);
      min-width:240px; user-select:none;
    `;

    const header = document.createElement('div');
    header.style.cssText = `
      padding:6px 10px; background:#283149; border-radius:6px 6px 0 0;
      cursor:pointer; display:flex; justify-content:space-between; align-items:center;
    `;
    const title = document.createElement('span');
    title.textContent = 'MetaDSL-GAI';
    title.style.cssText = 'font-weight:bold; color:#00bcd4;';
    const collapseBtn = document.createElement('span');
    collapseBtn.textContent = '[-]';
    collapseBtn.style.cssText = 'font-size:10px; color:#aaa;';
    header.appendChild(title);
    header.appendChild(collapseBtn);

    const body = document.createElement('div');
    body.style.cssText = 'padding:8px 10px; display:block;';

    header.addEventListener('click', () => {
      ST.panelCollapsed = !ST.panelCollapsed;
      body.style.display = ST.panelCollapsed ? 'none' : 'block';
      collapseBtn.textContent = ST.panelCollapsed ? '[+]' : '[-]';
    });

    // Status line
    const statusLine = document.createElement('div');
    statusLine.id = 'metadsl-gai-status';
    statusLine.style.cssText = 'margin-bottom:6px;';
    body.appendChild(statusLine);

    // Button row 1: arm/disarm + break/clear/resume
    const row1 = document.createElement('div');
    row1.style.cssText = 'display:flex; gap:4px; margin-bottom:4px; flex-wrap:wrap;';
    const btnArm = mkBtn('arm', () => { ST.armed ? disarmNow() : armNow(); });
    const btnBreak = mkBtn('break', manualBreak);
    const btnClear = mkBtn('clear', manualClear);
    const btnResume = mkBtn('resume', manualResume);
    row1.appendChild(btnArm);
    row1.appendChild(btnBreak);
    row1.appendChild(btnClear);
    row1.appendChild(btnResume);
    body.appendChild(row1);

    // Button row 2: trim + longrun + autosend + keep input
    const row2 = document.createElement('div');
    row2.style.cssText = 'display:flex; gap:4px; margin-bottom:4px; flex-wrap:wrap; align-items:center;';
    const btnTrim = mkBtn('trim', () => trimHistory(CFG.KEEP_ROUNDS));
    const btnLong = mkBtn('long', () => {
      ST.longRunMode = !ST.longRunMode;
      log('[longrun] ' + ST.longRunMode);
      updatePanel();
    });
    const btnAuto = mkBtn('auto', () => {
      ST.autoSendOn ? stopAutoSend() : startAutoSend();
    });
    const keepLabel = document.createElement('label');
    keepLabel.style.cssText = 'font-size:10px; display:inline-flex; align-items:center; gap:2px;';
    const keepInput = document.createElement('input');
    keepInput.type = 'number';
    keepInput.min = '1';
    keepInput.max = '99';
    keepInput.value = String(CFG.KEEP_ROUNDS);
    keepInput.style.cssText = 'width:36px; background:#111; color:#fff; border:1px solid #444; padding:1px 3px;';
    keepInput.addEventListener('change', () => {
      const v = parseInt(keepInput.value, 10);
      if (v >= 1 && v <= 99) CFG.KEEP_ROUNDS = v;
    });
    keepLabel.appendChild(document.createTextNode('keep'));
    keepLabel.appendChild(keepInput);
    row2.appendChild(btnTrim);
    row2.appendChild(btnLong);
    row2.appendChild(btnAuto);
    row2.appendChild(keepLabel);
    body.appendChild(row2);

    // Button row 3: prompt1/prompt2/prompt3
    const row3 = document.createElement('div');
    row3.style.cssText = 'display:flex; gap:4px; margin-bottom:4px; flex-wrap:wrap;';
    const btnP1 = mkBtn('prompt1', () => sendPromptN(1));
    const btnP2 = mkBtn('prompt2', () => sendPromptN(2));
    const btnP3 = mkBtn('prompt3', () => sendPromptN(3));
    row3.appendChild(btnP1);
    row3.appendChild(btnP2);
    row3.appendChild(btnP3);
    body.appendChild(row3);

    panel.appendChild(header);
    panel.appendChild(body);
    document.body.appendChild(panel);
    ST.panelEl = panel;
    updatePanel();
  }

  function mkBtn(text, onClick) {
    const b = document.createElement('button');
    b.textContent = text;
    b.style.cssText = `
      background:#3a4a6b; color:#fff; border:none; border-radius:3px;
      padding:3px 8px; cursor:pointer; font-size:10px; font-family:monospace;
    `;
    b.addEventListener('click', onClick);
    return b;
  }

  function sendPromptN(n) {
    let txt = '';
    try { txt = callMetaDSL('get_system_prompt_' + n, '') || ''; }
    catch (e) { log('[prompt' + n + '] callMetaDSL error: ' + (e && e.message)); return; }
    if (!txt || !String(txt).trim()) { log('[prompt' + n + '] empty, skip'); return; }
    chatSend(String(txt));
    log('[prompt' + n + '] sent (' + String(txt).length + ' chars)');
  }

  function updatePanel() {
    if (!ST.panelEl) return;
    const status = ST.panelEl.querySelector('#metadsl-gai-status');
    if (!status) return;

    const armTag = ST.armed
      ? '<span style="color:#4caf50;font-weight:bold;">ARM</span>'
      : '<span style="color:#999;">idle</span>';
    const brkTag = ST.breakerOn
      ? '<span style="color:#f44336;font-weight:bold;">BRK</span>'
      : (ST.longRunMode
        ? '<span style="color:#ff9800;">LONG</span>'
        : '<span style="color:#4caf50;">run</span>');
    const autoTag = ST.autoSendOn
      ? '<span style="color:#00bcd4;">AUTO</span>'
      : '';
    const queue = ST.pendingResults.length;
    const queueTag = queue > 0
      ? `<span style="color:#ff9800;">Q:${queue}</span>`
      : '<span style="color:#666;">Q:0</span>';
    const wsTag = (ST.ws && ST.ws.readyState === 1)
      ? '<span style="color:#4caf50;">ws</span>'
      : '<span style="color:#f44336;">ws-off</span>';
    const failTag = ST.sendFailAt
      ? `<span style="color:#f44336;" title="${ST.sendFailInfo}">FAIL</span>`
      : '';

    status.innerHTML = `${armTag} ${brkTag} ${autoTag} ${queueTag} ${wsTag} ${failTag}
      <br>rounds: ${ST.roundCount}/${CFG.MAX_ROUNDS}`;
  }

  /* ===========================================
     Section 8  History Trimming
     =========================================== */
  function trimHistory(keepRounds) {
    const root = getChatRoot();
    if (!root) { warn('[trim] no chat root'); return; }
    // Treat each AI message as one round; keep last N AI messages and their
    // preceding siblings up to the previous AI message.
    const aiMsgs = pickAllVisible(SEL.aiMsg, root);
    const total = aiMsgs.length;
    if (total <= keepRounds) {
      log(`[trim] nothing to do (${total} <= keep=${keepRounds})`);
      return;
    }
    const cutoffAi = aiMsgs[total - keepRounds];
    let removed = 0;
    // Walk siblings of root: remove everything before cutoffAi.
    let node = root.firstElementChild;
    while (node && node !== cutoffAi) {
      const next = node.nextElementSibling;
      // Skip removing the cutoff itself's prior user message - keep it as
      // context. Heuristic: stop at the closest user msg before cutoff.
      // For simplicity remove all earlier siblings.
      try { node.remove(); removed++; } catch (_) { }
      node = next;
    }
    log(`[trim] removed ${removed} nodes, kept last ${keepRounds} rounds`);
  }

  /* ===========================================
     Section 9  Auto-Send (input box ready -> click send)
     =========================================== */
  function startAutoSend() {
    if (ST.autoSendOn) return;
    ST.autoSendOn = true;
    ST.autoSendTimer = setInterval(autoSendTick, 2000);
    log('[autosend] on');
    updatePanel();
  }
  function stopAutoSend() {
    ST.autoSendOn = false;
    if (ST.autoSendTimer) { clearInterval(ST.autoSendTimer); ST.autoSendTimer = null; }
    log('[autosend] off');
    updatePanel();
  }
  function autoSendTick() {
    if (!ST.autoSendOn) return;
    const ta = pickVisible(SEL.inputTA);
    if (!ta) return;
    const text = (ta.value || '').trim();
    if (!text) return;
    const btn = pickVisible(SEL.sendBtn);
    if (!btn) return;
    if (btn.disabled) return;
    // Don't click if currently generating (button shows stop icon).
    if (isGenerating(btn)) return;
    log('[autosend] click send (text.len=' + text.length + ')');
    btn.click();
  }

  /** Heuristic: detect 'generating' state on send button. */
  function isGenerating(btn) {
    // Google AI page swaps the button into a 'stop' state; aria-label or
    // SVG path tends to change. Fallback: check if disabled property flips
    // back within next tick.
    const al = (btn.getAttribute('aria-label') || '').toLowerCase();
    if (al.includes('stop') || al.includes('停止')) return true;
    return false;
  }

  /* ===========================================
     Section 10  chatSend  (write to textarea + click send)
     =========================================== */
  function markSendFail(info) {
    ST.sendFailAt = Date.now();
    ST.sendFailInfo = info || '';
    err('[chatSend] FAIL:', info);
    updatePanel();
  }
  function clearSendFail() {
    if (ST.sendFailAt) {
      ST.sendFailAt = 0;
      ST.sendFailInfo = '';
      updatePanel();
    }
  }

  /** Set textarea value via React-native setter so React notices the change. */
  function setReactValue(el, value) {
    const proto = el.tagName === 'TEXTAREA'
      ? window.HTMLTextAreaElement.prototype
      : window.HTMLInputElement.prototype;
    const setter = Object.getOwnPropertyDescriptor(proto, 'value').set;
    setter.call(el, value);
    el.dispatchEvent(new Event('input', { bubbles: true }));
    el.dispatchEvent(new Event('change', { bubbles: true }));
  }

  /**
   * Write text into the chat input and click the send button when ready.
   * Uses a tick loop with 30s deadline. Detects 'generating' state on the
   * button (stop-icon swap) to know when send finished.
   */
  function chatSend(text) {
    const ta = pickVisible(SEL.inputTA);
    if (!ta) { markSendFail('input textarea not found'); return; }
    try { setReactValue(ta, text); }
    catch (e) { markSendFail('setReactValue threw: ' + e.message); return; }

    const deadline = Date.now() + 30000;
    const tick = () => {
      if (Date.now() > deadline) {
        // Dump candidate buttons to help diagnose selector drift.
        const all = document.querySelectorAll('button');
        const dump = [];
        for (let i = 0; i < Math.min(all.length, 8); i++) {
          const b = all[i];
          dump.push(`${i}:${(b.getAttribute('data-xid') || '').slice(0, 40)}|${(b.getAttribute('aria-label') || '').slice(0, 20)}|d=${b.disabled}`);
        }
        markSendFail('send button never enabled. ta.val.len=' + (ta.value || '').length + ' btns=' + dump.join(';'));
        return;
      }
      const btn = pickVisible(SEL.sendBtn);
      const ta2 = pickVisible(SEL.inputTA);
      // Wait until: send btn visible & enabled, AND React has flushed our
      // value into the visible textarea (defends against framework rollback).
      if (!btn || btn.disabled || !ta2 || ta2.value !== text) {
        setTimeout(tick, 200);
        return;
      }
      // Throttled progress log.
      const nowTs = Date.now();
      if (nowTs - ST.chatSendLastLogTs > 5000) {
        ST.chatSendLastLogTs = nowTs;
        log('[chatSend] clicking send btn');
      }
      try {
        btn.click();
        clearSendFail();
      } catch (e) {
        markSendFail('btn.click threw: ' + e.message);
      }
    };
    setTimeout(tick, 100);
  }

  /* ===========================================
     Section 11  Code Block Extraction
     =========================================== */
  const EXECUTE_MARKERS = ['// @execute', '# @execute'];

  /** Heuristic: derive lang from code element class (language-xxx). */
  function getLang(codeEl) {
    const cls = codeEl.getAttribute('class') || '';
    const m = cls.match(/language-([a-zA-Z0-9_\-]+)/);
    if (m) return m[1].toLowerCase();
    return 'metadsl';
  }

  /** Read code text robustly. */
  function readCodeText(el) {
    // Try line-span format (markdown renderers split per line).
    const lineSpans = el.querySelectorAll('.code-line');
    if (lineSpans && lineSpans.length > 0) {
      const parts = [];
      lineSpans.forEach(ls => parts.push(ls.textContent || ''));
      return parts.join('\n');
    }
    return el.textContent || '';
  }

  /** Test if code starts with one of the execute markers. */
  function hasExecuteMarker(code) {
    const lines = code.split('\n');
    for (let i = 0; i < lines.length; i++) {
      const ln = lines[i].trim();
      if (!ln) continue;
      for (const m of EXECUTE_MARKERS) {
        if (ln.startsWith(m)) return true;
      }
      // Only inspect the very first non-empty line.
      return false;
    }
    return false;
  }

  /**
   * Determine whether the AI message has finished streaming.
   * Decision beta (double-safety):
   *   1. All code blocks inside the message carry data-complete="true"; AND
   *   2. The message text has been stable for TEXT_STABLE_MS.
   */
  function isMessageComplete(aiMsgEl) {
    // 1) All code blocks marked complete.
    const allCb = aiMsgEl.querySelectorAll(SEL.codeBlock);
    const completeCb = aiMsgEl.querySelectorAll(SEL.codeBlockComplete);
    if (allCb.length > 0 && completeCb.length !== allCb.length) {
      return false;
    }
    // 2) Text-stable window.
    const txt = aiMsgEl.textContent || '';
    const rec = ST.emptyAt.get(aiMsgEl);
    const now = Date.now();
    if (!rec) {
      ST.emptyAt.set(aiMsgEl, { len: txt.length, ts: now });
      return false;
    }
    if (rec.len !== txt.length) {
      // Still changing; reset the stability window.
      rec.len = txt.length;
      rec.ts = now;
      return false;
    }
    return (now - rec.ts) >= CFG.TEXT_STABLE_MS;
  }

  /**
   * Extract new code blocks from an AI message element.
   * Returns array of { el, code, lang, id }.
   */
  function extractBlocks(aiMsgEl) {
    const out = [];
    const blocks = aiMsgEl.querySelectorAll(SEL.codeBlock);
    blocks.forEach(blk => {
      if (ST.processedCodes.has(blk)) return;
      const codeEl = blk.querySelector(SEL.codeText);
      if (!codeEl) return;
      const code = readCodeText(codeEl);
      if (!code || !code.trim()) return;
      if (!hasExecuteMarker(code)) {
        // Diagnostic: log first non-empty line so user can spot missed markers.
        const firstLine = (code.split('\n').find(l => l.trim()) || '').slice(0, 80);
        log('[extract] no @execute marker, skip. first=' + firstLine);
        ST.processedCodes.add(blk);
        return;
      }
      const lang = getLang(codeEl);
      // Optional language filter from CFG.LANG_FILTER (empty = no filter).
      if (CFG.LANG_FILTER.length > 0 && CFG.LANG_FILTER.indexOf(lang) < 0) {
        log('[extract] lang filtered: ' + lang);
        ST.processedCodes.add(blk);
        return;
      }
      ST.processedCodes.add(blk);
      ST.blockSeq += 1;
      out.push({
        el: blk,
        code,
        lang,
        id: 'blk' + ST.blockSeq,
      });
    });
    return out;
  }

  /* ===========================================
     Section 12  Visual Marking
     =========================================== */
  function markVisual(block) {
    try {
      block.el.style.borderLeft = '4px solid #00bcd4';
      const badge = document.createElement('span');
      badge.textContent = ' [' + block.id + '] ';
      badge.style.cssText = 'background:#00bcd4;color:#fff;font-size:10px;padding:1px 4px;margin-left:4px;border-radius:2px;';
      const parent = block.el.parentElement;
      if (parent) parent.insertBefore(badge, block.el);
    } catch (_) { }
  }

  /* ===========================================
     Section 13  processMessages  (main loop)
     =========================================== */
  function processMessages() {
    if (ST.paused) return;
    // Try to cache the real chatList parent now that DOM may have AI msgs.
    tryCacheChatList();

    const root = getChatRoot();
    if (!root) return;

    const aiMsgs = pickAllVisible(SEL.aiMsg, root);
    if (aiMsgs.length === 0) return;

    // Disarmed mode: rolling baseline - absorb every visible code block as
    // already processed; do not mark messages, do not send to WS.
    if (!ST.armed) {
      aiMsgs.forEach(msg => {
        const blocks = msg.querySelectorAll(SEL.codeBlock);
        blocks.forEach(blk => {
          if (!ST.processedCodes.has(blk)) ST.processedCodes.add(blk);
        });
      });
      return;
    }

    // Armed mode: process each AI message that has not yet been finalized.
    aiMsgs.forEach(msg => {
      if (ST.processedMsgs.has(msg)) return;
      if (!isMessageComplete(msg)) return;

      const blocks = extractBlocks(msg);
      if (blocks.length === 0) {
        // No code blocks found in a fully stable message -> mark final.
        ST.processedMsgs.add(msg);
        ST.emptyAt.delete(msg);
        return;
      }

      // Send each code block to DSL via WebSocket.
      blocks.forEach(blk => {
        markVisual(blk);
        const ok = wsSend(blk.code);
        if (!ok) {
          warn('[process] wsSend failed, block kept unprocessed: ' + blk.id);
        } else {
          log(`[process] sent ${blk.id} lang=${blk.lang} ${blk.code.length}B`);
        }
      });

      // Finalize this message so we don't re-extract on later mutations.
      ST.processedMsgs.add(msg);
      ST.emptyAt.delete(msg);
    });
  }

  /* ===========================================
     Section 14  markHistory  (mark existing as processed at init)
     =========================================== */
  function markHistory() {
    const root = getChatRoot();
    if (!root) return;
    let nMsg = 0, nBlk = 0;
    pickAllVisible(SEL.aiMsg, root).forEach(msg => {
      ST.processedMsgs.add(msg); nMsg++;
      msg.querySelectorAll(SEL.codeBlock).forEach(blk => {
        ST.processedCodes.add(blk); nBlk++;
      });
    });
    log(`[init] markHistory: ${nMsg} ai msgs, ${nBlk} code blocks marked as processed`);
  }

  /* ===========================================
     Section 15  Mutation Observer + Polling
     =========================================== */
  function startObserver() {
    if (ST.observer) return;
    const target = getChatRoot();
    ST.observer = new MutationObserver(() => {
      clearTimeout(ST.debounceTimer);
      ST.debounceTimer = setTimeout(processMessages, CFG.DEBOUNCE_MS);
    });
    ST.observer.observe(target, { childList: true, subtree: true });
    log('[observer] started on: ' + (target.tagName || 'body'));

    // Polling fallback in case mutations are missed.
    if (ST.pollTimer) clearInterval(ST.pollTimer);
    ST.pollTimer = setInterval(processMessages, CFG.POLL_MS);
  }

  /* ===========================================
     Section 16  init
     =========================================== */
  function init() {
    log('[init] start, url=' + location.href);
    // Cache chatList parent if any AI msg already on page.
    tryCacheChatList();
    // All currently-visible messages and code blocks are considered history.
    markHistory();
    startObserver();
    createPanel();
    // Notify DSL side that the bridge is ready. Keep the legacy name
    // 'aiclaw_ready' to avoid touching the DSL handler (N1 decision).
    bridgeSendNotification('aiclaw_ready', { url: location.href });
    log('[init] done');
  }

  /* ===========================================
     Section 17  Public API (window.MetaDSLBridge)
     =========================================== */
  window.MetaDSLBridge = {
    init,
    state: () => ST,
    config: () => CFG,
    pause: () => { ST.paused = true; log('[ctl] paused'); },
    resume: () => { ST.paused = false; log('[ctl] resumed'); processMessages(); },
    process: processMessages,
    markHistory,
    sendChat: chatSend,
    setLangFilter: (arr) => { CFG.LANG_FILTER = arr || []; },
    breakNow: manualBreak,
    clearQueue: manualClear,
    resumeNow: manualResume,
    arm: armNow,
    disarm: disarmNow,
    sendCommand: bridgeSendCommand,
    sendNotification: bridgeSendNotification,
    status: () => ({
      armed: ST.armed,
      breakerOn: ST.breakerOn,
      longRun: ST.longRunMode,
      autoSend: ST.autoSendOn,
      queue: ST.pendingResults.length,
      rounds: ST.roundCount,
      wsReady: !!(ST.ws && ST.ws.readyState === 1),
      sendFail: !!ST.sendFailAt,
      sendFailInfo: ST.sendFailInfo,
    }),
  };

  /* ===========================================
     Section 18  Boot
     =========================================== */
  if (document.readyState === 'complete') {
    setTimeout(init, 500);
  } else {
    window.addEventListener('load', () => setTimeout(init, 500));
  }

})();
