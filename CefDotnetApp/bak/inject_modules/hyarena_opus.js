// ==UserScript==
// @name         MetaDSL Agent Bridge v2
// @version      2.2
// @description  SBS multi-model chat <-> local WebSocket code execution loop (dynamic slots)
// @match        *://*/*
// @grant        none
// ==/UserScript==

(function () {
  'use strict';

  /* ===========================================
     Section 0  Config
     =========================================== */
  const CFG = {
    WS_URL: 'ws://localhost:9527',
    RECONNECT_MS: 3000,
    MAX_RECONNECT: 50,
    DEBOUNCE_MS: 1200,
    RESULT_FLUSH_MS: 2000,
    POLL_MS: 3000,
    LANG_FILTER: [],
    MAX_ROUNDS: 8,
    RESET_TIMEOUT_MS: 30000,
    DEBUG: true,
    // Minimum slot count when page selectors are not yet available.
    DEFAULT_SLOT_COUNT: 2,
    // Color palette for visual marking of code blocks per slot.
    SLOT_COLORS: ['#00bcd4', '#ff9800', '#8bc34a', '#e91e63', '#9c27b0', '#ffc107'],
    // Blind-mode identity prompt sent by the panel [identity] button.
    IDENTITY_PROMPT: '我们现在有2个模型同时参与会话，一个是Model A，一个是Model B，现在需要你们给自己取一个名字，然后我会告诉哪个是A，哪个是B，后续需要你们知道这一点。',
  };

  // Selectors aligned with the current page structure.
  const SEL = {
    chatList: '.sbs-chat-list-content',
    aiMsg: '.sbsai-item',
    userMsg: '.sbshuman-item',
    multiContent: '.multimodel-type-content',
    contentItem: '.multimodel-type-content-item',
    itemHeader: '.multimodel-type-content-item-header',
    itemHeaderName: '.multimodel-type-content-item-header-left-text .t-trigger',
    codeBlock: 'pre code',
    loadingDot: '.hy-cherry-markdown__loading-dot',
    inputTA: '.t-textarea__inner',
    sendBtn: '.hy-chat-input-send-btn',
    paramsBtn: '[data-tool-key="params"]',
    settingsCard: '.model-setting-v2__card',
    settingsClose: '.model-setting-v2-dialog .t-dialog__close-btn',
    // Top-bar model selector name (used at startup for initial slot count).
    topModelName: '.model-selector-container .model-selector__name',
    // Blind-compare mode root (present when the page is in anonymous compare mode).
    blindRoot: '.sbs-chat-content-container--blind',
  };

  /* ===========================================
     Section 1  Global State (slot-based)
     =========================================== */
  const ST = {
    // Dynamic per-slot state keyed by slotId like 'slot0', 'slot1', ...
    ws: {},               // { slot0: WebSocket, ... }
    reconCnt: {},         // { slot0: 0, ... }
    pendingResults: {},   // { slot0: [str, ...], ... }
    sysPrompt: {},        // { slot0: 'prompt text', ... }
    modelNames: {},       // { slot0: 'Hy3 preview...', ... }
    slotCount: 0,

    processedCodes: new WeakSet(),
    processedMsgs: new WeakSet(),
    paused: false,
    observer: null,
    debounceTimer: null,
    pollTimer: null,
    blockSeq: 0,
    flushTimer: null,
    emptyAt: new Map(),

    roundCount: 0,
    lastFlushTs: 0,
    breakerOn: false,
    panelCollapsed: false,
    panelEl: null,
    mergeNextFlush: false,

    // Recognition arming: when false, newly observed AI messages / code blocks
    // are continuously absorbed into the processed sets as a rolling baseline
    // (no extraction, no WS send). Flip to true via the panel [arm] button.
    armed: false,
    // Throttle diagnostic logging in chatSend tick loop.
    chatSendLastLogTs: 0,
    // Track last chat-send failure for panel badge display.
    sendFailAt: 0,
    sendFailInfo: '',
    // Last text auto-written to chat input by flushResults; used to
    // distinguish our own residual input from genuine user typing.
    lastAutoSentText: '',
    // Single-shot deferred-flush timer when user is typing.
    deferTimer: null,
    // Throttle for deferred-flush diagnostic log.
    deferLastLogTs: 0,

    // Executor slot selector: only code blocks from this slot are actually
    // sent over WebSocket for execution. Blocks from other slots are still
    // recognized and visually marked (in red), but dropped before wsSend.
    // Default is set in init() based on actual slotCount.
    executorSlot: 'slot1',
  };

  /* ===========================================
     Section 2  Logging
     =========================================== */
  const T = '[MetaDSL]';
  const log = (...a) => CFG.DEBUG && console.log(`%c${T}`, 'color:#00bcd4;font-weight:bold', ...a);
  const warn = (...a) => console.warn(`%c${T}`, 'color:#ff9800;font-weight:bold', ...a);
  const err = (...a) => console.error(`%c${T}`, 'color:#f44336;font-weight:bold', ...a);

  /* ===========================================
     Section 2.5  Slot helpers
     =========================================== */
  function slotIdOf(index) { return 'slot' + index; }
  function slotIndexOf(slotId) {
    const m = /^slot(\d+)$/.exec(slotId);
    return m ? parseInt(m[1], 10) : -1;
  }
  // Only accept canonical slot id like 'slot0', 'slot1', ...
  function normalizeSlot(key) {
    if (typeof key === 'string' && /^slot\d+$/.test(key)) return key;
    return null;
  }
  function slotColor(index) {
    return CFG.SLOT_COLORS[index % CFG.SLOT_COLORS.length];
  }
  function activeSlotIds() {
    const ids = [];
    for (let i = 0; i < ST.slotCount; i++) ids.push(slotIdOf(i));
    return ids;
  }
  function displayName(slotId) {
    const nm = ST.modelNames[slotId];
    return nm && nm.trim() ? nm.trim() : slotId;
  }
  function shortName(slotId, max) {
    const n = displayName(slotId);
    if (n.length <= max) return n;
    return n.slice(0, max) + '...';
  }

  /** Detect blind-compare mode by the root container class. */
  function isBlindMode() {
    return !!document.querySelector(SEL.blindRoot);
  }

  /** Probe initial slot count from the top model selector. Fallback to default. */
  function probeInitialSlotCount() {
    const nodes = document.querySelectorAll(SEL.topModelName);
    if (nodes && nodes.length > 0) return nodes.length;
    return CFG.DEFAULT_SLOT_COUNT;
  }

  /** Ensure internal slot state for at least `count` slots; creates WS lazily. */
  function ensureSlots(count) {
    if (count <= ST.slotCount) return;
    const topNames = document.querySelectorAll(SEL.topModelName);
    const blind = isBlindMode();
    for (let i = ST.slotCount; i < count; i++) {
      const id = slotIdOf(i);
      ST.reconCnt[id] = 0;
      ST.pendingResults[id] = [];
      ST.sysPrompt[id] = '';
      if (!ST.modelNames[id]) {
        if (blind) {
          // Blind mode: fixed placeholder names, not taken from top selector.
          ST.modelNames[id] = 'Model ' + String.fromCharCode(65 + i);
        } else {
          const nm = topNames[i] && topNames[i].textContent ? topNames[i].textContent.trim() : '';
          ST.modelNames[id] = nm || id;
        }
      }
      ST.ws[id] = wsCreate(id);
    }
    ST.slotCount = count;
    log(`[slots] expanded to ${count}`);
  }

  /* ===========================================
     Section 3  WebSocket Management
     =========================================== */
  function wsCreate(slotId) {
    log(`[${slotId}] connecting -> ${CFG.WS_URL}`);
    let ws;
    try { ws = new WebSocket(CFG.WS_URL); } catch (e) {
      err(`WS create failed [${slotId}]`, e);
      wsReconnect(slotId); return null;
    }

    ws.onopen = () => {
      log(`[${slotId}] connected`);
      ST.reconCnt[slotId] = 0;
    };

    ws.onmessage = (ev) => {
      onWSMsg(slotId, typeof ev.data === 'string' ? ev.data : String(ev.data));
    };

    ws.onerror = () => err(`[${slotId}] WS error`);
    ws.onclose = () => { warn(`[${slotId}] closed`); wsReconnect(slotId); };
    return ws;
  }

  function wsReconnect(slotId) {
    if ((ST.reconCnt[slotId] = (ST.reconCnt[slotId] || 0) + 1) > CFG.MAX_RECONNECT) {
      err(`[${slotId}] reconnect limit reached`); return;
    }
    setTimeout(() => { ST.ws[slotId] = wsCreate(slotId); }, CFG.RECONNECT_MS);
  }

  function wsSend(slotId, text) {
    const ws = ST.ws[slotId];
    if (!ws || ws.readyState !== WebSocket.OPEN) {
      warn(`[${slotId}] WS not connected`); return false;
    }
    ws.send(text);
    return true;
  }

  /* ===========================================
     Section 4  Receive Exec Results (plain text)
     =========================================== */
  function onWSMsg(slotId, text) {
    log(`[${slotId}] recv ${text.length}B`);
    if (!text) return;
    if (!ST.pendingResults[slotId]) ST.pendingResults[slotId] = [];
    ST.pendingResults[slotId].push(text);
    scheduleFlush();
  }

  function scheduleFlush() {
    clearTimeout(ST.flushTimer);
    ST.flushTimer = setTimeout(flushResults, CFG.RESULT_FLUSH_MS);
  }

  function totalPending() {
    let n = 0;
    for (const id of activeSlotIds()) n += (ST.pendingResults[id] || []).length;
    return n;
  }

  function flushResults() {
    const slots = activeSlotIds();
    const nonEmpty = slots.filter(id => (ST.pendingResults[id] || []).length > 0);
    if (nonEmpty.length === 0) return;

    // Not armed yet: keep queue, do not flush.
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
    const taNow = document.querySelector(SEL.inputTA);
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
    if (ST.lastFlushTs && now - ST.lastFlushTs > CFG.RESET_TIMEOUT_MS) {
      ST.roundCount = 0;
      log('[rounds] idle timeout, reset to 0');
    }

    // Count total pending code results across all slots.
    let total = 0;
    for (const id of slots) {
      total += (ST.pendingResults[id] || []).length;
    }

    const lines = [];
    const blind = isBlindMode();
    if (blind) {
      lines.push('> Note: blind mode, model names (Model A/B) are anonymous placeholders.');
    }

    // Queued dispatch: when more than one result is pending, only emit the
    // first one and keep the rest queued, so that the model focuses on
    // previous results one at a time instead of being flooded.
    if (total > 1) {
      let pickedSlot = null;
      for (const id of slots) {
        const bucket = ST.pendingResults[id] || [];
        if (bucket.length > 0) { pickedSlot = id; break; }
      }
      const bucket = ST.pendingResults[pickedSlot];
      const raw = bucket.shift();
      const name = displayName(pickedSlot);
      lines.push(`### ${name} exec result`);
      lines.push('```\n' + raw.replace(/```/g, '`\u200b``').trimEnd() + '\n```');
      const remaining = total - 1;
      lines.push('');
      lines.push(`> 还有 ${remaining} 个代码结果在排队输出，不要再发新代码，回复继续即可`);
    } else {
      for (const id of slots) {
        const bucket = ST.pendingResults[id] || [];
        if (!bucket.length) continue;
        const name = displayName(id);
        lines.push(`### ${name} exec result`);
        for (const raw of bucket) {
          lines.push('```\n' + raw.replace(/```/g, '`\u200b``').trimEnd() + '\n```');
        }
        ST.pendingResults[id] = [];
      }
    }

    if (lines.length) {
      // Round counting: merge mode (after resume) or queued single-result
      // emit counts +1; otherwise per-non-empty-slot.
      if (ST.mergeNextFlush) {
        ST.roundCount += 1;
        ST.mergeNextFlush = false;
      } else if (total > 1) {
        ST.roundCount += 1;
      } else {
        ST.roundCount += nonEmpty.length;
      }
      ST.lastFlushTs = now;

      if (ST.roundCount >= CFG.MAX_ROUNDS) {
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
     Section 4.5  Breaker Control + Floating Panel
     =========================================== */
  const PANEL_ID = 'metadsl-control-panel';

  function manualBreak() {
    ST.breakerOn = true;
    ST.roundCount = 0;
    log('[manual] break');
    updatePanel();
  }

  function manualClear() {
    const n = totalPending();
    for (const id of activeSlotIds()) ST.pendingResults[id] = [];
    log(`[manual] clear queue (${n} items)`);
    updatePanel();
  }

  function sendIdentity() {
    if (!isBlindMode()) {
      warn('[identity] only meaningful in blind mode, skip');
      return;
    }
    log('[identity] send');
    chatSend(CFG.IDENTITY_PROMPT);
  }

  function sendPromptToChat() {
    if (typeof callMetaDSL !== 'function') {
      warn('[prompt] callMetaDSL unavailable, skip');
      return;
    }
    // In explicit (non-blind) compare mode, apply the system prompt via
    // the settings dialog for each card, same as page load. In blind mode
    // the settings dialog is unavailable, so fall back to chatSend.
    if (!isBlindMode()) {
      log('[prompt] explicit mode, apply via settings dialog');
      loadAndApplySystemPrompt();
      return;
    }
    let text = '';
    try {
      text = callMetaDSL('get_system_prompt', '');
    } catch (e) {
      err('[prompt] call get_system_prompt failed', e);
      return;
    }
    if (!text || typeof text !== 'string') {
      warn('[prompt] get_system_prompt returned empty, skip');
      return;
    }
    log(`[prompt] send (${text.length} chars)`);
    chatSend(text);
  }

  function manualResume() {
    ST.breakerOn = false;
    ST.roundCount = 0;
    ST.mergeNextFlush = true;
    log('[manual] resume');
    updatePanel();
    clearTimeout(ST.flushTimer);
    flushResults();
  }

  function armNow() {
    // Re-baseline current DOM's code blocks only (do NOT mark aiMsg as
    // processed), so that if a message is still streaming at the moment of
    // arming, any new code blocks appended afterwards are still recognized.
    const n = baselineCodeOnly();
    ST.armed = true;
    log(`[arm] armed (baselined ${n} existing code blocks)`);
    updatePanel();
  }

  /** Baseline only existing `pre code` elements into processedCodes. */
  function baselineCodeOnly() {
    const chatList = document.querySelector(SEL.chatList);
    if (!chatList) return 0;
    let n = 0;
    chatList.querySelectorAll(SEL.codeBlock).forEach(el => {
      if (!ST.processedCodes.has(el)) { ST.processedCodes.add(el); n++; }
    });
    return n;
  }

  function disarmNow() {
    ST.armed = false;
    log('[arm] disarmed (recognition paused, pending queue kept)');
    updatePanel();
  }

  function createPanel() {
    const old = document.getElementById(PANEL_ID);
    if (old) old.remove();

    const p = document.createElement('div');
    p.id = PANEL_ID;
    p.style.cssText = `
      position:fixed; top:100px; right:100px; z-index:2147483647;
      width:auto; min-width:200px; max-width:360px;
      font-family:-apple-system,Segoe UI,sans-serif; font-size:12px;
      background:rgba(30,30,40,0.92); color:#fff; border-radius:6px;
      box-shadow:0 2px 12px rgba(0,0,0,0.35); user-select:none;
      border:1px solid rgba(255,255,255,0.1);
    `;

    const header = document.createElement('div');
    header.className = 'metadsl-panel-header';
    header.style.cssText = `
      padding:6px 10px; cursor:pointer; display:flex;
      justify-content:space-between; align-items:center; gap:8px;
      border-bottom:1px solid rgba(255,255,255,0.08);
    `;
    header.addEventListener('click', () => {
      ST.panelCollapsed = !ST.panelCollapsed;
      updatePanel();
    });

    const body = document.createElement('div');
    body.className = 'metadsl-panel-body';
    body.style.cssText = 'padding:8px 10px;';

    p.appendChild(header);
    p.appendChild(body);
    document.body.appendChild(p);
    ST.panelEl = p;
    updatePanel();
  }

  function updatePanel() {
    const p = ST.panelEl;
    if (!p) return;
    const header = p.querySelector('.metadsl-panel-header');
    const body = p.querySelector('.metadsl-panel-body');
    if (!header || !body) return;

    const armIcon = ST.armed ? 'ARM' : 'IDLE';
    const statusIcon = ST.breakerOn ? 'BRK' : 'RUN';
    const arrow = ST.panelCollapsed ? 'v' : '^';
    const roundText = ST.breakerOn && ST.roundCount >= CFG.MAX_ROUNDS
      ? `${CFG.MAX_ROUNDS}/${CFG.MAX_ROUNDS}`
      : `${ST.roundCount}/${CFG.MAX_ROUNDS}`;

    const failBadge = ST.sendFailAt
      ? `<span class="metadsl-fail-badge" title="${(ST.sendFailInfo || '').replace(/"/g, '&quot;')} - click to clear" style="background:#f44336;color:#fff;padding:1px 5px;border-radius:3px;font-size:10px;margin-left:4px;cursor:pointer;">! send</span>`
      : '';
    header.innerHTML = `<span>MetaDSL ${armIcon} ${statusIcon} ${roundText}${failBadge}</span><span>${arrow}</span>`;
    const badgeEl = header.querySelector('.metadsl-fail-badge');
    if (badgeEl) {
      badgeEl.onclick = (ev) => { ev.stopPropagation(); clearSendFail(); };
    }

    if (ST.panelCollapsed) {
      body.style.display = 'none';
      return;
    }
    body.style.display = 'block';

    const slots = activeSlotIds();
    const queueParts = slots.map(id => {
      const n = (ST.pendingResults[id] || []).length;
        return `${shortName(id, 8)}(${n})`;
    });
    const queueText = queueParts.length ? queueParts.join(' &middot; ') : '(empty)';
    const stateText = (ST.armed ? 'armed' : 'idle') + ' / ' + (ST.breakerOn ? 'broken' : 'running');

    const blind = isBlindMode();
    const execOptions = slots.map(id => {
      const sel = id === ST.executorSlot ? ' selected' : '';
      const nm = displayName(id).replace(/</g, '&lt;').replace(/>/g, '&gt;');
      return `<option value="${id}"${sel}>${nm}</option>`;
    }).join('');
    body.innerHTML = `
      <div style="margin-bottom:6px;">state: ${stateText}</div>
      <div style="margin-bottom:6px;">rounds: ${roundText}</div>
      <div style="margin-bottom:8px; opacity:0.8; white-space:nowrap;">queue: ${queueText}</div>
      <div style="margin-bottom:8px; display:flex; align-items:center; gap:6px;">
        <span style="opacity:0.8;">executor:</span>
        <select class="metadsl-exec-select" style="flex:1; padding:2px 4px; font-size:11px; background:#263238; color:#fff; border:1px solid rgba(255,255,255,0.15); border-radius:3px;">${execOptions}</select>
      </div>
      <div style="display:flex; gap:4px; margin-bottom:4px;">
        <button data-act="arm"    style="flex:1; padding:4px 8px; font-size:11px; cursor:pointer; background:${ST.armed ? '#ef6c00' : '#455a64'}; color:#fff; border:none; border-radius:3px;">${ST.armed ? 'disarm' : 'arm'}</button>
        <button data-act="break"  style="flex:1; padding:4px 8px; font-size:11px; cursor:pointer; background:#d32f2f; color:#fff; border:none; border-radius:3px;" ${ST.breakerOn ? 'disabled' : ''}>break</button>
        <button data-act="clear"  style="flex:1; padding:4px 8px; font-size:11px; cursor:pointer; background:#546e7a; color:#fff; border:none; border-radius:3px;">clear</button>
        <button data-act="resume" style="flex:1; padding:4px 8px; font-size:11px; cursor:pointer; background:#388e3c; color:#fff; border:none; border-radius:3px;" ${ST.breakerOn ? '' : 'disabled'}>resume</button>
      </div>
      <div style="display:flex; gap:4px;">
        <button data-act="identity" style="flex:1; padding:4px 8px; font-size:11px; cursor:pointer; background:#1976d2; color:#fff; border:none; border-radius:3px;" ${blind ? '' : 'disabled'}>identity</button>
        <button data-act="prompt"   style="flex:1; padding:4px 8px; font-size:11px; cursor:pointer; background:#7b1fa2; color:#fff; border:none; border-radius:3px;">prompt</button>
      </div>
    `;
    body.querySelectorAll('button[disabled]').forEach(b => {
      b.style.opacity = '0.4';
      b.style.cursor = 'not-allowed';
    });
    const execSel = body.querySelector('.metadsl-exec-select');
    if (execSel) {
      execSel.onchange = (ev) => {
        ev.stopPropagation();
        const v = execSel.value;
        if (/^slot\d+$/.test(v)) {
          ST.executorSlot = v;
          log(`[executor] switched to ${v} (${displayName(v)})`);
          updatePanel();
        }
      };
    }
    body.querySelectorAll('button').forEach(b => {
      b.onclick = (ev) => {
        ev.stopPropagation();
        if (b.disabled) return;
        const act = b.dataset.act;
        if (act === 'arm') { ST.armed ? disarmNow() : armNow(); }
        else if (act === 'break') manualBreak();
        else if (act === 'clear') manualClear();
        else if (act === 'resume') manualResume();
        else if (act === 'identity') sendIdentity();
        else if (act === 'prompt') sendPromptToChat();
      };
    });
  }

  /* ===========================================
     Section 5  Chat Input (React controlled)
     =========================================== */
  function setReactValue(el, val) {
    const setter = Object.getOwnPropertyDescriptor(
      HTMLTextAreaElement.prototype, 'value'
    ).set;
    setter.call(el, val);
    el.dispatchEvent(new Event('input', { bubbles: true }));
    el.dispatchEvent(new Event('change', { bubbles: true }));
  }

  function chatSend(text) {
    const ta = document.querySelector(SEL.inputTA);
    if (!ta) { err('input textarea not found'); markSendFail('no textarea'); return; }

    setReactValue(ta, text);
    ta.style.height = 'auto';
    ta.style.height = Math.min(ta.scrollHeight, 400) + 'px';

    const deadline = Date.now() + 30000;
    ST.chatSendLastLogTs = 0;
    let firstFailLogged = false;
    const tick = () => {
      const btn = document.querySelector(SEL.sendBtn);
      const btnDisabled = btn ? btn.disabled : null;
      const btnAria = btn ? btn.getAttribute('aria-disabled') : null;
      const btnPE = btn ? getComputedStyle(btn).pointerEvents : null;
      // When the model is streaming, the send button is swapped to a stop
      // button sharing the same class. Visual cue: send icon is a <path>,
      // stop icon is a <rect>. Treat presence of <rect> as "not sendable".
      const hasStopIcon = btn ? !!btn.querySelector('rect') : false;
      const ok = btn
        && !btnDisabled
        && btnAria !== 'true'
        && btnPE !== 'none'
        && !hasStopIcon;
      if (ok) {
        if (!ta.value) setReactValue(ta, text);
        const btnHtmlBefore = (btn.outerHTML || '').slice(0, 200);
        const taLenBefore = ta.value.length;
        log('[chat] clicking send, btn=', btnHtmlBefore, ' ta.len=', taLenBefore);
        btn.click();
        // Verify: React normally clears the textarea right after send.
        setTimeout(() => {
          const cur = document.querySelector(SEL.inputTA);
          const taLenAfter = cur ? cur.value.length : -1;
          if (taLenAfter >= taLenBefore) {
            warn('[chat] click seemed ineffective, ta.len before=',
              taLenBefore, ' after=', taLenAfter);
            markSendFail('click ineffective len=' + taLenAfter);
          } else {
            log('[chat] send confirmed, ta.len', taLenBefore, '->', taLenAfter);
          }
        }, 300);
        log('[chat] sent');
        return;
      }
      // On first wait-cycle, dump all buttons near the input area once
      // for diagnostics (selector may be stale / button replaced by stop-btn
      // during streaming).
      if (!firstFailLogged) {
        firstFailLogged = true;
        try {
          const area = ta.closest('.hy-chat-input-area')
            || ta.closest('[class*="input"]')
            || ta.parentElement
            || document.body;
          const btns = area.querySelectorAll('button');
          const dump = Array.from(btns).slice(0, 8).map(b => ({
            cls: b.className,
            dis: b.disabled,
            aria: b.getAttribute('aria-disabled'),
            pe: getComputedStyle(b).pointerEvents,
            html: (b.outerHTML || '').slice(0, 120),
          }));
          log('[chat] send-btn not ready, input-area buttons:', dump);
        } catch (e) {
          warn('[chat] button dump failed:', e && e.message);
        }
      }
      // Throttled diagnostic: once per second while waiting.
      const now = Date.now();
      if (now - ST.chatSendLastLogTs > 1000) {
        ST.chatSendLastLogTs = now;
        log('[chat] waiting send-btn:',
          'btn=' + !!btn,
          'disabled=' + btnDisabled,
          'aria-disabled=' + btnAria,
          'pointer-events=' + btnPE,
          'stopIcon=' + hasStopIcon);
      }
      if (now > deadline) {
        const taVal = (ta.value || '').slice(0, 40);
        const btnHtml = btn ? (btn.outerHTML || '').slice(0, 160) : '(null)';
        err('send button not usable, give up. ta.value[0..40]=', taVal,
          ' btn=', btnHtml);
        markSendFail('btn not usable');
        return;
      }
      setTimeout(tick, 200);
    };
    setTimeout(tick, 250);
  }

  function markSendFail(reason) {
    ST.sendFailAt = Date.now();
    ST.sendFailInfo = reason || 'unknown';
    updatePanel();
  }

  function clearSendFail() {
    ST.sendFailAt = 0;
    ST.sendFailInfo = '';
    updatePanel();
  }

  /* ===========================================
     Section 6  Code Extraction
     =========================================== */
  const EXECUTE_MARKERS = ['// @execute', '# @execute'];

  function getLang(codeEl) {
    for (const cls of codeEl.classList) {
      if (cls.startsWith('language-')) return cls.slice(9);
    }
    const pre = codeEl.closest('pre');
    if (pre) {
      for (const cls of pre.classList) {
        if (cls.startsWith('language-')) return cls.slice(9);
      }
    }
    return 'metadsl';
  }

  /**
   * Read code text from a <code> element, handling the cherry-markdown
   * variant where each line is wrapped in <span class="code-line"> without
   * inter-line '\n' characters in textContent. In that case textContent
   * would collapse all lines into one, which breaks line-based parsing
   * (execute marker detection AND later MetaDSL execution).
   */
  function readCodeText(el) {
    if (!el) return '';
    const lineSpans = el.querySelectorAll(':scope > .code-line');
    if (lineSpans && lineSpans.length > 0) {
      const parts = [];
      for (const s of lineSpans) parts.push(s.textContent || '');
      return parts.join('\n');
    }
    return el.textContent || '';
  }

  function hasExecuteMarker(code) {
    const lines = code.split('\n');
    for (const line of lines) {
      const trimmed = line.trim();
      if (trimmed === '') continue;
      return EXECUTE_MARKERS.includes(trimmed);
    }
    return false;
  }

  function isSideComplete(contentItem) {
    const dots = contentItem.querySelectorAll(SEL.loadingDot);
    let anyVisible = false;
    for (const d of dots) {
      if (getComputedStyle(d).display !== 'none') { anyVisible = true; break; }
    }
    const body = contentItem.querySelector('.sbs-cherry-markdown, .hy-cherry-markdown');
    if (!body) return false;
    if (!body.querySelector('[data-sign]')) return false;
    // Dot-visible debounce: treat slot as still-streaming for a grace window
    // after the last time any loading-dot was visible. This prevents premature
    // completion during momentary gaps between streamed chunks (cherry sometimes
    // flips all dots to display:none briefly between tokens).
    const now = Date.now();
    if (anyVisible) {
      contentItem.dataset.lastDotAt = String(now);
      return false;
    }
    const last = parseInt(contentItem.dataset.lastDotAt || '0', 10);
    // If we've never seen a visible dot on this slot (e.g. historical msgs on
    // first load), accept completion immediately. Otherwise require the dot to
    // have been gone for at least DOT_DEBOUNCE_MS.
    if (last > 0 && (now - last) < 3000) return false;
    return true;
  }

  /** Extract un-processed exec-marked code blocks from one slot's content item. */
  function extractBlocks(contentItem, slotId) {
    const out = [];
    let scanned = 0, skippedProcessed = 0, skippedEmpty = 0, skippedNoMarker = 0;
    const firstLines = [];
    contentItem.querySelectorAll(SEL.codeBlock).forEach(el => {
      // Skip cherry-markdown indent-code blocks. These are produced by the
      // LLM's indented prose (thought dumps, bullet drafts) which are never
      // real MetaDSL code and would otherwise trigger a spurious 'noMarker'
      // scan result that starts the empty-timeout prematurely.
      if (el.classList && el.classList.contains('indent-code')) return;
      // Skip code blocks embedded inside the "deep thinking" collapsible
      // section. These are the model's thought-dump, never real tool calls.
      // Mark as processed so subsequent scans fast-skip them and they don't
      // inflate 'scanned' / 'firstLines' in diagnostic logs.
      if (el.closest && el.closest('.hy-detail-block.hy-think')) {
        ST.processedCodes.add(el);
        return;
      }
      scanned++;
      if (ST.processedCodes.has(el)) { skippedProcessed++; return; }
      const code = readCodeText(el).trim();
      if (!code) { skippedEmpty++; return; }
      if (!hasExecuteMarker(code)) {
        skippedNoMarker++;
        // Capture first non-empty line (up to 60 chars) for diagnosis.
        const firstLine = (code.split('\n').find(l => l.trim() !== '') || '').trim().slice(0, 60);
        firstLines.push(firstLine);
        return;
      }
      const lang = getLang(el);
      out.push({ el, code, lang, slotId, id: `${slotId}-${++ST.blockSeq}` });
    });
    if (out.length === 0 && (scanned > 0 || skippedProcessed > 0)) {
      log(`[extract] [${slotId}] scanned=${scanned} processed=${skippedProcessed} empty=${skippedEmpty} noMarker=${skippedNoMarker} firstLines=`, firstLines);
    }
    return out;
  }

  /** Try to read the model name shown inside the message item header. */
  function readModelNameFromItem(contentItem) {
    const el = contentItem.querySelector(SEL.itemHeaderName);
    if (!el) return '';
    const t = el.textContent || '';
    return t.trim();
  }

  /* ===========================================
     Section 7  Visual Marking
     =========================================== */
  function markVisual(block, dropped) {
    ST.processedCodes.add(block.el);

    const pre = block.el.closest('pre') || block.el;
    const idx = slotIndexOf(block.slotId);
    // Dropped (non-executor) slot blocks use red to indicate 'recognized
    // but discarded before WS send'. Executor slot blocks use slot color.
    const color = dropped ? '#f44336' : slotColor(idx >= 0 ? idx : 0);

    pre.style.borderLeft = `4px solid ${color}`;
    pre.style.position = 'relative';
    pre.dataset.metadslId = block.id;

    const label = dropped
      ? `${block.id} [${block.lang}] (drop)`
      : `${block.id} [${block.lang}]`;
    const badge = document.createElement('span');
    badge.textContent = label;
    badge.style.cssText = `
      position:absolute; top:2px; right:8px; z-index:10;
      font-size:10px; line-height:1.4; padding:1px 6px; border-radius:3px;
      background:${color}; color:#fff; pointer-events:none;
    `;
    pre.appendChild(badge);
  }

  /* ===========================================
     Section 8  Main Processing Loop
     =========================================== */
  function processMessages() {
    if (ST.paused) return;

    const chatList = document.querySelector(SEL.chatList);
    if (!chatList) return;

    // Rolling baseline while disarmed: absorb any newly rendered code blocks
    // into processedCodes, but DO NOT mark the aiMsg itself as processed,
    // so that a message still streaming when the user arms can still have
    // its later-appended code blocks recognized after arming.
    if (!ST.armed) {
      chatList.querySelectorAll(SEL.codeBlock).forEach(el => {
        if (!ST.processedCodes.has(el)) ST.processedCodes.add(el);
      });
      return;
    }

    chatList.querySelectorAll(SEL.aiMsg).forEach(aiMsg => {
      if (ST.processedMsgs.has(aiMsg)) return;

      const mc = aiMsg.querySelector(SEL.multiContent);
      if (!mc) return;

      const items = Array.from(mc.children);
      if (items.length === 0) return;

      // Dynamically grow slot capacity to match UI.
      if (items.length > ST.slotCount) ensureSlots(items.length);

      // All visible slots must be complete before processing.
      for (const it of items) {
        if (!isSideComplete(it)) return;
      }

      // Update model names from each item header (per-message, most accurate).
      items.forEach((it, i) => {
        const id = slotIdOf(i);
        const nm = readModelNameFromItem(it);
        if (nm) ST.modelNames[id] = nm;
      });

      // Extract code blocks from each slot.
      const all = [];
      items.forEach((it, i) => {
        const id = slotIdOf(i);
        const blocks = extractBlocks(it, id);
        for (const b of blocks) all.push(b);
      });

      if (all.length === 0) {
        // Text-stability based empty wait: only lock the message as
        // processed after textContent length has remained unchanged for
        // >=10s. This prevents premature lockout during long "deep
        // thinking" phases where isSideComplete may briefly misreport
        // completion while the model is still streaming content.
        const curLen = (aiMsg.textContent || '').length;
        const rec = ST.emptyAt.get(aiMsg);
        if (rec === undefined) {
          ST.emptyAt.set(aiMsg, { startTime: Date.now(), textLen: curLen });
          return;
        }
        if (curLen !== rec.textLen) {
          // Content still changing; reset the stability window.
          rec.startTime = Date.now();
          rec.textLen = curLen;
          return;
        }
        if (Date.now() - rec.startTime >= 5000) {
          ST.processedMsgs.add(aiMsg);
          ST.emptyAt.delete(aiMsg);
          log('[msg] confirmed no code, marked. slots=' + items.length + ' textLen=' + curLen);
          // If there are queued results waiting (e.g. after a "continue"
          // reply from the model), proactively schedule a flush so the
          // next queued result can be delivered.
          const hasPending = activeSlotIds().some(id => (ST.pendingResults[id] || []).length > 0);
          if (hasPending) scheduleFlush();
        }
        return;
      }

      ST.emptyAt.delete(aiMsg);

      const summary = {};
      for (const b of all) summary[b.slotId] = (summary[b.slotId] || 0) + 1;
      log('[msg] new blocks:', summary, 'total=' + all.length);

      all.forEach(b => {
        const dropped = b.slotId !== ST.executorSlot;
        markVisual(b, dropped);
        if (dropped) {
          log(`[drop] [${b.slotId}] ${b.id} (${b.lang}, ${b.code.length}B) - not executor (${ST.executorSlot})`);
          return;
        }
        const ok = wsSend(b.slotId, b.code);
        log(ok
          ? `[send] [${b.slotId}] ${b.id} (${b.lang}, ${b.code.length}B)`
          : `[send] [${b.slotId}] failed ${b.id}`);
      });

      // Do NOT mark processedMsgs here. A streaming message may append more
      // code blocks later; we rely on the 10s empty-confirm path above to
      // finalize the message once no new code shows up.
    });
  }

  function markHistory() {
    const chatList = document.querySelector(SEL.chatList);
    if (!chatList) return 0;
    let n = 0;
    chatList.querySelectorAll(SEL.aiMsg).forEach(aiMsg => {
      ST.processedMsgs.add(aiMsg);
      aiMsg.querySelectorAll(SEL.codeBlock).forEach(el => {
        ST.processedCodes.add(el); n++;
      });
    });
    return n;
  }

  /* ===========================================
     Section 9  MutationObserver + Poll Fallback
     =========================================== */
  function startObserver() {
    if (ST.observer) ST.observer.disconnect();

    const target =
      document.querySelector(SEL.chatList) ||
      document.querySelector('.sbs-chat-content-container') ||
      document.body;

    ST.observer = new MutationObserver(() => {
      if (ST.paused) return;
      clearTimeout(ST.debounceTimer);
      ST.debounceTimer = setTimeout(processMessages, CFG.DEBOUNCE_MS);
    });

    ST.observer.observe(target, { childList: true, subtree: true });
    log('[observer] started ->', target.tagName + '.' +
      (target.getAttribute('class') || '').slice(0, 60));

    clearInterval(ST.pollTimer);
    ST.pollTimer = setInterval(() => {
      if (!ST.paused) processMessages();
    }, CFG.POLL_MS);
  }

  /* ===========================================
     Section 10  System Prompt
     =========================================== */
  function setSystemPrompt(key, prompt) {
    const id = normalizeSlot(key);
    if (!id) { err('slot key invalid: ' + key); return; }
    const idx = slotIndexOf(id);
    if (idx < 0) { err('bad slot id: ' + id); return; }
    if (idx >= ST.slotCount) ensureSlots(idx + 1);
    ST.sysPrompt[id] = prompt;
    log(`[${id}] system prompt set (${prompt.length} chars)`);
    if (isBlindMode()) {
      warn(`[${id}] blind mode detected, system prompt UI unavailable, skip injection`);
      return;
    }
    _setPromptUI(id, prompt);
  }

  function _setPromptUI(slotId, prompt) {
    const idx = slotIndexOf(slotId);
    if (idx < 0) return;
    const btn = document.querySelector(SEL.paramsBtn);
    if (!btn) { warn('params button not found'); return; }

    btn.click();

    setTimeout(() => {
      const cards = document.querySelectorAll(SEL.settingsCard);
      if (cards.length <= idx) {
        warn(`settings card count ${cards.length} <= slot index ${idx}`);
        btn.click(); return;
      }
      const ta = cards[idx].querySelector('textarea');
      if (ta) {
        setReactValue(ta, prompt);
        log(`[${slotId}] page prompt written`);
      } else {
        warn(`[${slotId}] textarea not found in card`);
      }

      setTimeout(() => {
        const close = document.querySelector(SEL.settingsClose);
        if (close) close.click();
        else {
          const mask = document.querySelector('.model-setting-v2-dialog .t-dialog__mask');
          if (mask) mask.click();
          else btn.click();
        }
      }, 300);
    }, 600);
  }

  /** Open params dialog once, fill every card with the same prompt, then close. */
  function _setAllPromptUI(prompt) {
    if (isBlindMode()) {
      warn('blind mode detected, system prompt UI unavailable, skip injection');
      return;
    }
    const btn = document.querySelector(SEL.paramsBtn);
    if (!btn) { warn('params button not found'); return; }

    btn.click();

    setTimeout(() => {
      const cards = document.querySelectorAll(SEL.settingsCard);
      if (cards.length === 0) {
        warn('no settings card found'); btn.click(); return;
      }
      // Grow internal slots to match UI card count.
      if (cards.length > ST.slotCount) ensureSlots(cards.length);

      cards.forEach((card, idx) => {
        const id = slotIdOf(idx);
        const ta = card.querySelector('textarea');
        if (ta) {
          setReactValue(ta, prompt);
          ST.sysPrompt[id] = prompt;
          log(`[${id}] page prompt written`);
        } else {
          warn(`[${id}] textarea not found in card`);
        }
      });

      setTimeout(() => {
        const close = document.querySelector(SEL.settingsClose);
        if (close) close.click();
        else {
          const mask = document.querySelector('.model-setting-v2-dialog .t-dialog__mask');
          if (mask) mask.click();
          else btn.click();
        }
      }, 300);
    }, 600);
  }

  function loadAndApplySystemPrompt() {
    if (typeof callMetaDSL !== 'function') {
      warn('callMetaDSL unavailable, skip system prompt load');
      return;
    }
    let text = '';
    try {
      text = callMetaDSL('get_system_prompt', '');
    } catch (e) {
      err('call get_system_prompt failed', e);
      return;
    }
    if (!text || typeof text !== 'string') {
      warn('get_system_prompt returned empty');
      return;
    }
    log(`[prompt] loaded (${text.length} chars)`);
    _setAllPromptUI(text);
  }

  /* ===========================================
     Section 11  Initialization
     =========================================== */
  function init() {
    console.group(`${T} init`);

    const n = markHistory();
    log(`[history] marked ${n} code blocks`);

    const initCount = probeInitialSlotCount();
    ensureSlots(initCount);
    log(`[slots] initial count=${initCount}`);

    // Default executor slot: prefer slot1 when at least 2 slots exist,
    // otherwise fall back to slot0. User may switch via the panel later.
    ST.executorSlot = ST.slotCount >= 2 ? 'slot1' : 'slot0';
    log(`[executor] default=${ST.executorSlot}`);

    startObserver();
    createPanel();

    setTimeout(loadAndApplySystemPrompt, 1500);

    console.groupEnd();
    log(`[ready]
  MetaDSLBridge.arm() / .disarm()                 - start/stop recognizing new code blocks
  MetaDSLBridge.breakNow() / .clearQueue() / .resumeNow()  - circuit breaker controls
  MetaDSLBridge.pause() / .resume()               - pause/resume the scan loop itself
  MetaDSLBridge.setSystemPrompt(slotKey, '...')   - slotKey = 'slot0' | 'slot1' | ...
  MetaDSLBridge.process()                         - manual scan once
  MetaDSLBridge.setLangFilter(...langs)           - filter code blocks by language
  MetaDSLBridge.sendChat('...')                   - type text into chat input and send
  MetaDSLBridge.status()                          - print current status`);
  }

  /* ===========================================
     Section 12  Public API
     =========================================== */
  window.MetaDSLBridge = {
    init,
    state: ST,
    config: CFG,

    pause() { ST.paused = true; log('[pause]'); },
    resume() { ST.paused = false; log('[resume]'); },

    process: processMessages,
    markHistory,

    setSystemPrompt,

    sendChat: chatSend,

    reconnect(key) {
      const id = normalizeSlot(key);
      if (!id) { err('slot key invalid: ' + key); return; }
      const idx = slotIndexOf(id);
      if (idx < 0) return;
      if (idx >= ST.slotCount) ensureSlots(idx + 1);
      if (ST.ws[id]) {
        try { ST.ws[id].close(); } catch (_) {}
      }
      ST.ws[id] = wsCreate(id);
    },

    setLangFilter(...langs) {
      CFG.LANG_FILTER = langs.map(l => l.toLowerCase());
      log('[lang-filter]', CFG.LANG_FILTER.length ? CFG.LANG_FILTER : '(all)');
    },

    breakNow: manualBreak,
    clearQueue: manualClear,
    resumeNow: manualResume,

    arm: armNow,
    disarm: disarmNow,

    status() {
      const chatList = document.querySelector(SEL.chatList);
      if (!chatList) { warn('chatList missing'); return; }
      let total = 0, pending = 0;
      chatList.querySelectorAll(SEL.aiMsg).forEach(m => {
        total++;
        if (!ST.processedMsgs.has(m)) {
          pending++;
          const mc = m.querySelector(SEL.multiContent);
          if (mc) {
            const flags = Array.from(mc.children).map((it, i) => {
              return `${slotIdOf(i)}=${isSideComplete(it)}`;
            }).join(' ');
            log(`  pending: ${flags}`);
          }
        }
      });
      const wsStates = activeSlotIds().map(id => {
        return `${id}(${ST.modelNames[id] || '-'})=${ST.ws[id] ? ST.ws[id].readyState : 'null'}`;
      }).join(' | ');
      log(`status: armed=${ST.armed} breaker=${ST.breakerOn} rounds=${ST.roundCount}/${CFG.MAX_ROUNDS}`);
      log(`  ${total} msgs, ${pending} pending; ${wsStates}`);
    },
  };

  /* ===========================================
     Section 13  Boot
     =========================================== */
  if (document.readyState === 'complete') {
    setTimeout(init, 500);
  } else {
    window.addEventListener('load', () => setTimeout(init, 500));
  }

})();