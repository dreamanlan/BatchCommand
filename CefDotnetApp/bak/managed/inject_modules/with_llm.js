// ==UserScript==
// @name         MetaDSL Agent Bridge (With Chat)
// @version      1.1
// @description  With page <-> local WebSocket code execution loop (single model)
// @match        *://*/*
// @grant        none
// ==/UserScript==

(function () {
  'use strict';

  /* ===========================================
     Section 0  Config
     =========================================== */
  const CFG = {
    WS_URL: null,         // set by DSL via ws_start_with command
    RECONNECT_MS: 3000,
    MAX_RECONNECT: 50,
    DEBOUNCE_MS: 1200,
    RESULT_FLUSH_MS: 2000,
    POLL_MS: 3000,
    LANG_FILTER: [],
    MAX_ROUNDS: 8,
    KEEP_ROUNDS: 6,
    RESET_TIMEOUT_MS: 30000,
    // Fire freebie_context_count_down every N conversation rounds (0 disables).
    LLM_CONTEXT_COUNT_MODULO: 16,
    DEBUG: true,
    // Stream completion: required idle window after last text change.
    TEXT_STABLE_MS: 3000,
  };

  // Selectors aligned with the With page structure.
  const SEL = {
    // Container for AI messages (cached at first detection - see ST.chatListEl).
    // Fallback root used by Observer until first AI message is found.
    chatListFallback: 'body',
    // AI message anchor: each represents one round of AI reply.
    aiMsg: 'div.message.assist-message',
    codeBlock: 'pre',
    stopBtn: 'div.ask-form button.t-button--variant-base:not(.send-button)',

    // Code text inside the block.
    codeText: 'pre code',
    // Real textarea for chat input.
    inputTA: 'div.ask-form textarea.narrow-scrollbar',
    sendBtn: 'div.ask-form button.t-button--theme-primary',
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

  /** Pick AI messages whose host or visible content has a non-zero rect. */
  function pickAllVisibleMessages(root) {
    const all = (root || document).querySelectorAll(SEL.aiMsg);
    const out = [];
    for (const el of all) {
      const r = el.getBoundingClientRect();
      if (r.width > 0 && r.height > 0) {
        out.push(el);
        continue;
      }
      const content = el.querySelector('pre, code, .markdown, [class*="markdown"]');
      if (!content) continue;
      const contentRect = content.getBoundingClientRect();
      if (contentRect.width > 0 && contentRect.height > 0) out.push(el);
    }
    return out;
  }

  /** Return code blocks scoped to one AI message. */
  function getMessageCodeBlocks(aiMsgEl) {
    const blocks = aiMsgEl.querySelectorAll(SEL.codeBlock);
    if (blocks.length > 0) return blocks;
    const content = aiMsgEl.querySelector('.markdown, [class*="markdown"]') || aiMsgEl;
    // Tagged <metadsl> code rendered as plain text has no <pre>: the whole
    // message content becomes the block when a tag or marker is present.
    const raw = readCodeText(content);
    return (extractTaggedBlocks(raw).length > 0 ||
      hasExecuteMarker(normalizeCodeText(raw))) ? [content] : [];
  }

  /** Read plain text from textarea/input or a contenteditable textbox. */
  function getInputText(el) {
    if (!el) return '';
    if (el.tagName === 'TEXTAREA' || el.tagName === 'INPUT') {
      return el.value || '';
    }
    return (el.innerText || el.textContent || '').replace(/\u00a0/g, ' ');
  }

  /** Pick a visible button whose metadata explicitly identifies send/submit. */
  function pickSendButton() {
    return pickVisible(SEL.sendBtn);
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
    drainMode: false,
    sentSigs: new Map(),

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
  const T = '[MetaDSL-With]';
  const log = (...a) => CFG.DEBUG && console.log(`%c${T}`, 'color:#00bcd4;font-weight:bold', ...a);
  const warn = (...a) => console.warn(`%c${T}`, 'color:#ff9800;font-weight:bold', ...a);
  const err = (...a) => console.error(`%c${T}`, 'color:#f44336;font-weight:bold', ...a);

  /** Always return document.body (chatList caching disabled - see tryCacheChatList). */
  function getChatRoot() {
    return document.body;
  }

  /**
   * Keep observation on document.body because With replaces conversation
   * containers during navigation and response streaming.
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

  // Mirrors the main-window llm_* notifications (metadsl_monitor.js keepContext /
  // state_machine.js save_conversation_history) but keyed by agentId: each
  // single-page freebie agent reports with its fixed module identity.
  const AGENT_ID = 'with';

  function notifyContextCountDown() {
    bridgeSendNotification('freebie_context_count_down', {
      agentId: AGENT_ID,
      count: CFG.LLM_CONTEXT_COUNT_MODULO
    });
  }

  // Build the assistant text used for SQLite history persistence: like
  // aiMsgEl.textContent, but every MetaDSL code block is collapsed into a
  // fixed "[metadsl]\n...\n[/metadsl]" placeholder. Detection uses the
  // existing hasExecuteMarker heuristic on the raw code text, so non-MetaDSL
  // fenced snippets (rare in agent mode) are preserved verbatim.
  function readAssistantHistoryText(aiMsgEl) {
    if (!aiMsgEl) return '';
    const clone = aiMsgEl.cloneNode(true);
    clone.querySelectorAll(SEL.codeBlock).forEach(blk => {
      const codeEl = (SEL.codeText ? blk.querySelector(SEL.codeText) : null) || blk;
      const raw = readCodeText(codeEl);
      if (!hasExecuteMarker(raw)) return;
      const parent = blk.parentNode;
      if (!parent) return;
      const placeholder = document.createTextNode('\n[metadsl]\n...\n[/metadsl]\n');
      parent.replaceChild(placeholder, blk);
    });
    return clone.textContent || '';
  }

  function notifyConversationHistory(aiMsgEl) {
    if (!aiMsgEl) return;
    const assistantText = readAssistantHistoryText(aiMsgEl);
    bridgeSendNotification('freebie_save_conversation_history', {
      agentId: AGENT_ID,
      conversations: [{ user: ST.lastSentPrompt || '', assistant: assistantText }]
    });
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

  // DSL -> JS: proactive command (e.g. ws_start_with, send_message)
  window.onAgentCommand = function (commandJson) {
    try {
      const cmd = JSON.parse(commandJson);
      log('[bridge] onAgentCommand:', cmd.command);

      if (cmd.command === 'ws_start_with' && cmd.params && cmd.params.port) {
        CFG.WS_URL = 'ws://localhost:' + cmd.params.port;
        log(`[bridge] ws_start_with -> ${CFG.WS_URL}`);
        if (!ST.ws || ST.ws.readyState > 1) {
          ST.reconCnt = 0;
          ST.ws = wsCreate();
        }
        return;
      }
      if (cmd.command === 'ws_stop_with') {
        log('[bridge] ws_stop_with');
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
    ST.execInflight = (ST.execInflight || 0) + 1;
    return true;
  }

  /* ===========================================
     Section 5  Receive Exec Results + Flush
     =========================================== */
  function onWSMsg(text) {
    log(`[ws] recv ${text.length}B`);
    if (!text) return;
    if (ST.execInflight > 0) ST.execInflight--;
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
    const curVal = getInputText(taNow);
    if (curVal.trim() && curVal !== ST.lastAutoSentText) {
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
      if (ST.mergeNextFlush) { ST.mergeNextFlush = false; }
      if (!ST.drainMode) {
        ST.roundCount += 1;
        // Context count-down is driven by flush rounds (one exec-result
        // round-trip), not by AI message elements: those depend on DOM
        // visibility and can be detected repeatedly for a single reply.
        ST.historyRoundCount = (ST.historyRoundCount || 0) + 1;
        if (CFG.LLM_CONTEXT_COUNT_MODULO > 0 &&
            ST.historyRoundCount % CFG.LLM_CONTEXT_COUNT_MODULO === 0) {
          notifyContextCountDown();
        }
      }

      ST.lastFlushTs = now;

      if (!ST.longRunMode && ST.roundCount >= CFG.MAX_ROUNDS) {
        ST.breakerOn = true;
        warn(`[breaker] reached ${CFG.MAX_ROUNDS} rounds, auto break`);
      }

      const text = lines.join('\n');
      log('[flush] send back:', text.slice(0, 120) + '...');
      ST.lastAutoSentText = text;
      chatSend(text, true);
      if (ST.pendingResults.length > 0 && !ST.breakerOn && ST.armed) {
        ST.drainMode = true;
        scheduleFlush();
      }
    }
    if (ST.pendingResults.length === 0) { ST.drainMode = false; }
    updatePanel();
  }

  /* ===========================================
     Section 6  Breaker / Arm Controls
     =========================================== */
  function manualBreak() {
    ST.breakerOn = true;
    ST.drainMode = false;
    warn('[breaker] manual break');
    updatePanel();
  }
  function manualClear() {
    const n = ST.pendingResults.length;
    ST.pendingResults = [];
    ST.drainMode = false;
    ST.sentSigs.clear();
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
    pickAllVisibleMessages(root).forEach(msg => {
      getMessageCodeBlocks(msg).forEach(el => {
        if (!ST.processedCodes.has(el)) { ST.processedCodes.add(el); n++; }
      });
    });
    log(`[baseline] absorbed ${n} existing code blocks`);
    return n;
  }

  function armNow() {
    if (ST.armed) { log('[arm] already armed'); return; }
    ST.drainMode = false;
    ST.sentSigs.clear();
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
    panel.id = 'metadsl-with-panel';
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
    title.textContent = 'MetaDSL-With';
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
    statusLine.id = 'metadsl-with-status';
    statusLine.style.cssText = 'margin-bottom:6px;';
    body.appendChild(statusLine);

    // Button row 1: mode switches
    const row1 = document.createElement('div');
    row1.style.cssText = 'display:flex; gap:4px; margin-bottom:4px; flex-wrap:wrap;';
    const btnArm = mkBtn('arm', () => { ST.armed ? disarmNow() : armNow(); });
    const btnLong = mkBtn('long', () => {
      ST.longRunMode = !ST.longRunMode;
      log('[longrun] ' + ST.longRunMode);
      updatePanel();
    });
    const btnAuto = mkBtn('auto', () => {
      ST.autoSendOn ? stopAutoSend() : startAutoSend();
    });
    row1.appendChild(btnArm);
    row1.appendChild(btnLong);
    row1.appendChild(btnAuto);
    body.appendChild(row1);

    // Button row 2: run controls
    const row2 = document.createElement('div');
    row2.style.cssText = 'display:flex; gap:4px; margin-bottom:4px; flex-wrap:wrap;';
    const btnBreak = mkBtn('break', manualBreak);
    const btnResume = mkBtn('resume', manualResume);
    const btnClear = mkBtn('clear', manualClear);
    row2.appendChild(btnBreak);
    row2.appendChild(btnResume);
    row2.appendChild(btnClear);
    body.appendChild(row2);

    // Button row 3: history settings
    const row3 = document.createElement('div');
    row3.style.cssText = 'display:flex; gap:4px; margin-bottom:4px; flex-wrap:wrap; align-items:center;';
    const btnTrim = mkBtn('trim', () => trimHistory(CFG.KEEP_ROUNDS));
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
    row3.appendChild(keepLabel);
    row3.appendChild(btnTrim);
    body.appendChild(row3);

    // Button row 4: prompt
    const row4 = document.createElement('div');
    row4.style.cssText = 'display:flex; gap:4px; margin-bottom:4px; flex-wrap:wrap;';
    const btnPrompt = mkBtn('prompt', () => sendPrompt());
    const btnTodo = mkBtn('todo', () => sendTodo());
    row4.appendChild(btnPrompt);
    row4.appendChild(btnTodo);
    body.appendChild(row4);

    panel.appendChild(header);
    panel.appendChild(body);
    document.body.appendChild(panel);
    ST.panelEl = panel;
    updatePanel();
  }

  function mkBtn(text, onClick) {
    const b = document.createElement('button');
    b.dataset.action = text;
    b.textContent = text;
    b.style.cssText = `
      background:#3a4a6b; color:#fff; border:none; border-radius:3px;
      padding:3px 8px; cursor:pointer; font-size:10px; font-family:monospace;
    `;
    b.addEventListener('click', onClick);
    return b;
  }

  function sendPrompt() {
    let txt = '';
    try { txt = callMetaDSL('get_with_prompt', '') || ''; }
    catch (e) { log('[prompt] callMetaDSL error: ' + (e && e.message)); return; }
    if (!txt || !String(txt).trim()) { log('[prompt] empty, skip'); return; }
    chatSend(String(txt));
    log('[prompt] sent (' + String(txt).length + ' chars)');
  }

  function sendTodo() {
    let txt = '';
    try { txt = callMetaDSL('get_todo', AGENT_ID) || ''; }
    catch (e) { log('[todo] callMetaDSL error: ' + (e && e.message)); return; }
    if (!txt || !String(txt).trim()) { log('[todo] empty, skip'); return; }
    chatSend(String(txt));
    log('[todo] sent (' + String(txt).length + ' chars)');
  }

  function updatePanel() {
    if (!ST.panelEl) return;
    const status = ST.panelEl.querySelector('#metadsl-with-status');
    if (!status) return;

    const queue = ST.pendingResults.length;
    const wsText = (ST.ws && ST.ws.readyState === 1) ? 'ws' : 'ws-off';
    const modeText = ST.breakerOn ? 'BREAK' : (ST.longRunMode ? 'LONG' : 'RUN');
    const autoText = ST.autoSendOn ? 'AUTO' : 'MANUAL';
    const failText = ST.sendFailAt ? ' | FAIL' : '';
    status.style.whiteSpace = 'pre-line';
    status.textContent =
      `${ST.armed ? 'ARM' : 'IDLE'} | ${modeText} | ${autoText} | Q:${queue} | ${wsText}${failText}\n` +
      `rounds: ${ST.roundCount}/${CFG.MAX_ROUNDS}`;
    status.title = ST.sendFailAt ? ST.sendFailInfo : '';

    const setButtonState = (action, active, activeColor) => {
      const button = ST.panelEl.querySelector(`button[data-action="${action}"]`);
      if (!button) return;
      button.textContent = `${action}:${active ? 'on' : 'off'}`;
      button.style.background = active ? activeColor : '#3a4a6b';
      button.style.fontWeight = active ? 'bold' : 'normal';
    };
    setButtonState('arm', ST.armed, '#2e7d32');
    setButtonState('break', ST.breakerOn, '#c62828');
    setButtonState('long', ST.longRunMode, '#ef6c00');
    setButtonState('auto', ST.autoSendOn, '#00838f');
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
    const text = getInputText(ta).trim();
    if (!text) return;
    const btn = pickSendButton();
    if (!btn) return;
    if (btn.disabled) return;
    // Don't click if currently generating (button shows stop icon).
    if (isGenerating(btn)) return;
    log('[autosend] click send (text.len=' + text.length + ')');
    btn.click();
  }

  /** Heuristic: detect 'generating' state on send button. */
  function isGenerating(btn) {
    return !!pickVisible(SEL.stopBtn);
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

  /** Set textarea/input or contenteditable text and notify the page. */
  function setReactValue(el, value) {
    if (el.isContentEditable) {
      el.focus();
      const selection = window.getSelection();
      const range = document.createRange();
      range.selectNodeContents(el);
      selection.removeAllRanges();
      selection.addRange(range);
      let inserted = false;
      try {
        inserted = document.execCommand('insertText', false, value);
      } catch (_) { }
      if (!inserted) {
        el.textContent = value;
      }
      el.dispatchEvent(new InputEvent('input', {
        bubbles: true,
        inputType: 'insertText',
        data: value,
      }));
      el.dispatchEvent(new Event('change', { bubbles: true }));
      return;
    }
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
  function chatSend(text, isAgentResult) {
    // Remember the prompt for freebie_save_conversation_history.
    // Agent-injected messages (exec results) keep an empty user, mirroring the
    // main agent's agentCollapsed handling in page_adapter.extractNewConversations.
    ST.lastSentPrompt = isAgentResult ? '' : String(text || '');
    const ta = pickVisible(SEL.inputTA);
    if (!ta) { markSendFail('chat input not found'); return; }
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
          dump.push(`${i}:${(b.getAttribute('data-testid') || '').slice(0, 40)}|${(b.getAttribute('aria-label') || '').slice(0, 20)}|d=${b.disabled}`);
        }
        markSendFail('send button never enabled. input.len=' + getInputText(ta).length + ' btns=' + dump.join(';'));
        return;
      }
      const btn = pickSendButton();
      const ta2 = pickVisible(SEL.inputTA);
      // Wait until: send btn visible & enabled, AND the page has retained our
      // value in the visible input (defends against framework rollback).
      if (!btn || btn.disabled || !ta2 || !getInputText(ta2).trim()) {
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
    // textContent drops <br> newlines: when a renderer treats <metadsl> as an
    // HTML block, the inner fence is not parsed as markdown and lines end up
    // joined by <br> (or block elements), which would flatten the code into
    // one line and break every line-based check. Walk the DOM instead and
    // emit a newline for <br> and block boundaries.
    const BLOCK_TAGS = new Set(['P', 'DIV', 'LI', 'UL', 'OL', 'PRE', 'CODE',
      'BLOCKQUOTE', 'TABLE', 'TR', 'SECTION', 'ARTICLE', 'METADSL']);
    const parts = [];
    (function walk(node) {
      if (node.nodeType === Node.TEXT_NODE) {
        parts.push(node.nodeValue || '');
        return;
      }
      if (node.nodeType !== Node.ELEMENT_NODE) return;
      if (node.tagName === 'BR') {
        parts.push('\n');
        return;
      }
      const isBlock = BLOCK_TAGS.has(node.tagName);
      if (isBlock) parts.push('\n');
      node.childNodes.forEach(walk);
      if (isBlock) parts.push('\n');
    })(el);
    return parts.join('').replace(/\n{3,}/g, '\n\n');
  }

  const FENCE_OPEN_RE = /^\s*[`~]{3,}/;
  const FENCE_CLOSE_RE = /^\s*[`~]{3,}\s*$/;
  const TAG_OPEN_RE = /^\s*<metadsl[^>]*>\s*$/i;
  const TAG_CLOSE_RE = /^\s*<\/metadsl\s*>\s*$/i;

  /**
   * Normalize extracted code text: drop markdown fence lines and
   * <metadsl></metadsl> wrapper lines so the DSL engine only sees code.
   */
  function normalizeCodeText(code) {
    if (!code) return code;
    let lines = code.replace(/\r\n/g, '\n').split('\n');
    const trimBlank = () => {
      while (lines.length && !lines[0].trim()) lines.shift();
      while (lines.length && !lines[lines.length - 1].trim()) lines.pop();
    };
    trimBlank();
    // Strip wrapper pairs (fence or tag) from the outside in.
    while (lines.length >= 2 &&
      ((FENCE_OPEN_RE.test(lines[0]) && FENCE_CLOSE_RE.test(lines[lines.length - 1])) ||
        (TAG_OPEN_RE.test(lines[0]) && TAG_CLOSE_RE.test(lines[lines.length - 1])))) {
      lines.shift();
      lines.pop();
      trimBlank();
    }
    return lines.join('\n');
  }

  /**
   * Extract the code inside every <metadsl>...</metadsl> tag block. Matches
   * the chat room semantics (d:\AiClaw\js\metadsl_executor.js): tags are
   * case-insensitive, may carry attributes, may sit anywhere on a line (text
   * before the open tag / after the close tag is ignored), and the close tag
   * tolerates inner spaces. Inner fence markers are removed by
   * normalizeCodeText.
   */
  function extractTaggedBlocks(text) {
    const out = [];
    if (!text) return out;
    const openG = /<metadsl\b[^>]*>/gi;
    const closeG = /<\/metadsl\s*>/gi;
    let openM;
    while ((openM = openG.exec(text)) !== null) {
      closeG.lastIndex = openM.index + openM[0].length;
      const closeM = closeG.exec(text);
      if (!closeM) {
        break;  // unclosed tag: nothing more to extract
      }
      const body = text.slice(openM.index + openM[0].length, closeM.index);
      const code = normalizeCodeText(body);
      if (code && code.trim()) out.push(code);
      openG.lastIndex = closeM.index + closeM[0].length;
    }
    return out;
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
   * Double-safety completion check:
   *   1. The global stop button is no longer visible; AND
   *   2. The assistant message text is stable for TEXT_STABLE_MS.

   */
  function isMessageComplete(aiMsgEl) {
    // With is complete only after the global stop button disappears and
    // this assistant message remains text-stable for TEXT_STABLE_MS.
    if (pickVisible(SEL.stopBtn)) return false;
    const txt = aiMsgEl.textContent || '';
    const rec = ST.emptyAt.get(aiMsgEl);
    const now = Date.now();
    if (!rec) {
      ST.emptyAt.set(aiMsgEl, { text: txt, ts: now });
      return false;
    }
    if (rec.text !== txt) {
      rec.text = txt;
      rec.ts = now;
      return false;
    }
    return (now - rec.ts) >= CFG.TEXT_STABLE_MS;
  }

  /**
   * True when nothing but whitespace separates the end of block a from the
   * start of block b inside root, i.e. the two fences are rendered back to
   * back. Climbs out of each block's own wrapper first, so page chrome (copy
   * button, language label) rendered next to a code block is not mistaken for
   * LLM content sitting in the gap.
   */
  function isBlankGap(root, a, b) {
    if (a === b || a.contains(b) || b.contains(a)) return false;
    let n = a, m = b;
    while (n.parentElement && n.parentElement !== root && !n.parentElement.contains(b)) {
      n = n.parentElement;
    }
    while (m.parentElement && m.parentElement !== root && !m.parentElement.contains(a)) {
      m = m.parentElement;
    }
    if (!n.parentElement || n.parentElement !== m.parentElement) return false;
    let cur = n.nextSibling;
    while (cur && cur !== m) {
      if (cur.nodeType === Node.TEXT_NODE) {
        if ((cur.nodeValue || '').trim()) return false;
      } else if (cur.nodeType === Node.ELEMENT_NODE) {
        // img/video/hr/table carry no text but are visible content.
        if (cur.matches('img, video, hr, table')) return false;
        if ((cur.textContent || '').trim()) return false;
      }
      cur = cur.nextSibling;
    }
    return true;
  }

  /**
   * Stage 1 of code block recognition: absorb continuations.
   * A block carrying an execute marker or a <metadsl> tag opens a command
   * unit. A block without one continues the preceding unit ONLY when nothing
   * but whitespace separates it from the previous block; otherwise it belongs
   * to no command (a leading plain fence, or one separated from the unit by
   * visible text).
   * Returns { units, dropped }. The units are NOT required to be adjacent to
   * each other - joining them is stage 2.
   */
  function groupCommandUnits(root, items) {
    const units = [];
    const dropped = [];
    let cur = null, prev = null;
    items.forEach(it => {
      if (it.starts) {
        cur = [it];
        units.push(cur);
      } else if (cur && prev && isBlankGap(root, prev.blk, it.blk)) {
        cur.push(it);
      } else {
        dropped.push(it);
      }
      prev = it;
    });
    return { units: units, dropped: dropped };
  }

  /**
   * Stage 2 of code block recognition: join the commands.
   * The protocol asks for one MetaDSL block per reply, but when the model
   * sends several they are concatenated and run as a single command; fixing
   * whatever that breaks is the model's job. Stage 1 units are joined here,
   * with no adjacency requirement between them.
   */
  function mergeCommandUnits(units) {
    const merged = [];
    units.forEach(u => u.forEach(it => merged.push(it)));
    return merged;
  }

  /** Diagnostics for a recognized command: how many fences were merged. */
  function blockLog(cmdBlocks) {
    const n = cmdBlocks.length;
    return 'merged ' + n + ' code block' + (n > 1 ? 's' : '') + ' into one command';
  }

  /**
   * Extract new code blocks from an AI message element.
   * Consecutive fences with nothing but whitespace between them are merged
   * into one script; several MetaDSL fences in one reply run as one command.
   * Returns array of { el, els, code, lang, id }.
   */
  function extractBlocks(aiMsgEl) {
    const out = [];
    const items = [];
    getMessageCodeBlocks(aiMsgEl).forEach(blk => {
      if (ST.processedCodes.has(blk)) return;
      const codeEl = blk.matches('pre')
        ? (blk.querySelector('code') || blk)
        : (blk.matches('.markdown, [class*="markdown"], [data-message-author-role="assistant"]')
          ? blk
          : blk.querySelector(SEL.codeText));
      if (!codeEl) return;
      // <metadsl>...</metadsl> tagged code: extract the inner code directly.
      // Handles both a tag wrapper inside a markdown fence and a bare tag in
      // the message text.
      const raw = readCodeText(codeEl);
      const tagged = extractTaggedBlocks(raw);
      const code = tagged.length > 0 ? tagged.join('\n\n') : normalizeCodeText(raw);
      if (!code || !code.trim()) return;
      // A tag wrapper is itself the execute signal; @execute still works.
      items.push({
        blk: blk,
        codeEl: codeEl,
        code: code,
        starts: tagged.length > 0 || hasExecuteMarker(code),
      });
    });
    const scan = groupCommandUnits(aiMsgEl, items);

    // Plain blocks that continue no command are never executed, but are still
    // recorded so later scans do not revisit them.
    scan.dropped.forEach(it => ST.processedCodes.add(it.blk));

    const cmdBlocks = mergeCommandUnits(scan.units);
    if (cmdBlocks.length === 0) {
      if (items.length > 0) {
        // Diagnostic: log first non-empty line so user can spot missed markers.
        const code = items.map(it => it.code).join('\n');
        const firstLine = (code.split('\n').find(l => l.trim()) || '').slice(0, 80);
        log('[extract] no @execute marker, skip. first=' + firstLine);
      }
      return out;
    }

    const codeEl = cmdBlocks[0].codeEl;
    const code = cmdBlocks.map(it => it.code).join('\n');
    const lang = getLang(codeEl);
    // Optional language filter from CFG.LANG_FILTER (empty = no filter).
    if (CFG.LANG_FILTER.length > 0 && CFG.LANG_FILTER.indexOf(lang) < 0) {
      log('[extract] lang filtered: ' + lang);
      cmdBlocks.forEach(it => ST.processedCodes.add(it.blk));
      return out;
    }
    cmdBlocks.forEach(it => ST.processedCodes.add(it.blk));
    ST.blockSeq += 1;
    log('[extract] ' + blockLog(cmdBlocks));
    out.push({
      el: cmdBlocks[0].blk,
      els: cmdBlocks.map(it => it.blk),
      code,
      lang,
      id: 'blk' + ST.blockSeq,
    });
    return out;
  }

  /* ===========================================
     Section 12  Visual Marking
     =========================================== */
  function markVisual(block) {
    try {
      // A merged group marks every block it spans, so it is visible which
      // fences were treated as one snippet.
      const els = (block.els && block.els.length > 0) ? block.els : [block.el];
      els.forEach(el => {
        el.style.borderLeft = '4px solid #00bcd4';
        const badge = document.createElement('span');
        badge.textContent = ' [' + block.id + '] ';
        badge.style.cssText = 'background:#00bcd4;color:#fff;font-size:10px;padding:1px 4px;margin-left:4px;border-radius:2px;';
        const parent = el.parentElement;
        if (parent) parent.insertBefore(badge, el);
      });
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

    const aiMsgs = pickAllVisibleMessages(root);
    if (aiMsgs.length === 0) return;

    // Disarmed mode: rolling baseline - absorb every visible code block as
    // already processed; do not mark messages, do not send to WS.
    if (!ST.armed) {
      aiMsgs.forEach(msg => {
        const blocks = getMessageCodeBlocks(msg);
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

      // Persist every completed AI message with the prompt that produced it.
      notifyConversationHistory(msg);

      const blocks = extractBlocks(msg);
      if (blocks.length === 0) {
        // No code blocks found in a fully stable message -> mark final.
        ST.processedMsgs.add(msg);
        ST.emptyAt.delete(msg);
        return;
      }

      // Send each code block to DSL via WebSocket.
      blocks.forEach(blk => {
        const sig = String(blk.code).replace(/\r\n/g, '\n').replace(/[ \t]+$/gm, '').trim();
        const sigTs = Date.now();
        const sigPrev = ST.sentSigs.get(sig);
        if (sigPrev !== undefined && sigTs - sigPrev < 60000) {
          log('[dedupe] skip duplicate code: ' + blk.id);
          return;
        }
        ST.sentSigs.set(sig, sigTs);
        markVisual(blk);
        const ok = wsSend(blk.code);
        if (!ok) {
          ST.sentSigs.delete(sig);
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
    pickAllVisibleMessages(root).forEach(msg => {
      ST.processedMsgs.add(msg); nMsg++;
      getMessageCodeBlocks(msg).forEach(blk => {
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
    // 'with_ready' to avoid touching the DSL handler (N1 decision).
    bridgeSendNotification('with_ready', { url: location.href });
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
    // Main-agent compatible queue counters (AgentAPI.getXXXQueueCount).
    // Single-page modules scan and send immediately, so there is neither a
    // pending-operation layer nor a to-send queue: both counters are always 0.
    // Exec requests still in flight (sent, result not yet back) are tracked in
    // ST.execInflight and exposed via status().execQueue instead.
    getOperationQueueCount: () => 0,
    getSendQueueCount: () => 0,
    getReceiveQueueCount: () => ST.pendingResults.length,
    status: () => ({
      armed: ST.armed,
      breakerOn: ST.breakerOn,
      longRun: ST.longRunMode,
      autoSend: ST.autoSendOn,
      queue: ST.pendingResults.length,
      execQueue: ST.execInflight || 0,
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
