// ============================================================================
// RelayWs - relay server client for the main agent page (chat / streaming).
//
// No page-side WebSocket anymore: the browser process owns the upstream
// connection (wsclient id "wsp") opened via the cefQuery action "lite_open".
// Which relay server the connection targets is decided by the URL configured
// in the relay panel (CONFIG 'relay.wsUrl') - the local WeChat bridge and the
// remote relay chain share the same wire protocol. All outbound messages go
// through the existing agent_send action; inbound pushes arrive through the
// generic onAgentEvent dispatcher by registering a pseudo-slot in
// relaySlotRegistry (same table the execution slots use; pagehide closes us
// for free via slot.close()).
//
// The public surface matches the old direct-WebSocket RelayWs exactly
// (connect/disconnect/sendMessage/pushMessage/callTool/sendCommand/
// onMessage/onStatus/connected/_lastChannelId) so relay_panel.js,
// state_machine.js and main.js keep working unchanged.
// ============================================================================
class RelayWs {
  constructor() {
    this.logger = logger.createLogger('RelayWs');
    this.clientId = null;        // the "wsp" wsclient id (browser side)
    this._lastChannelId = null;
    this.callbacks = {};
    this.reqId = 0;
    this.connected = false;
    this.autoReconnect = false;
    this._reconnectTimer = null;
    this._heartbeatTimer = null;
    this._onMessage = null;    // user callback: (data) => void
    this._onStatus = null;     // user callback: (status) => void  status: 'connected'|'disconnected'|'error'|'auth_ok'
    this._wsClientId = null;   // stable per-page client identity (R3)
    this._hadRemoteInbound = false;  // any inbound 'message' push seen (diagnostic only)
    // Outbound pushes queued while the relay connection is down; flushed on
    // reconnect (after auth) so replies finished during a disconnect window
    // are delivered instead of silently dropped.
    this._pendingPushes = [];
    this.maxPendingPushes = 50;
    this.authenticated = false;
    this._loadSessionState();
  }

  _getUrl() {
    return CONFIG.get('relay.wsUrl') || 'wss://www.gamexyz.net:8443/ws';
  }

  _getApiKey() {
    return CONFIG.get('relay.apiKey') || '';
  }

  _getSession() {
    return CONFIG.get('relay.session') || '';
  }

  // Stable client identity for R3: prefer relay.session, else generate once and reuse.
  _getClientId() {
    if (!this._wsClientId) {
      const s = this._getSession();
      this._wsClientId = s || ('agent_' + Date.now().toString(36) + '_' + Math.random().toString(36).slice(2, 10));
    }
    return this._wsClientId;
  }

  // ---- session-state persistence ----
  // _hadRemoteInbound / _lastChannelId survive a page reload (browser
  // restart, renderer memory-guard reopen): without this, replies generated
  // right after the reload are dropped by the "no remote session yet" gate
  // until the next inbound message arrives.
  _loadSessionState() {
    try {
      const raw = localStorage.getItem('relay_ws_session_state');
      if (!raw) return;
      const s = JSON.parse(raw);
      if (s && s.hadRemoteInbound) this._hadRemoteInbound = true;
      if (s && s.lastChannelId) this._lastChannelId = s.lastChannelId;
    } catch (e) {
      this.logger.warn('Failed to load relay session state: ' + e.message);
    }
  }

  _saveSessionState() {
    try {
      localStorage.setItem('relay_ws_session_state', JSON.stringify({
        hadRemoteInbound: this._hadRemoteInbound,
        lastChannelId: this._lastChannelId
      }));
    } catch (e) { /* storage unavailable: keep in-memory state */ }
  }

  // ---- public API ----

  // Connect to the relay server (the browser opens and owns the connection)
  connect(opts) {
    opts = opts || {};
    if (this.connected) {
      this.logger.warn('Already connected');
      return;
    }

    this.autoReconnect = opts.autoReconnect !== undefined ? opts.autoReconnect : true;
    const url = opts.url || this._getUrl();
    this._emitStatus('connecting');

    // A previous connection with this id may still be up (e.g. switching the
    // panel URL): release it first so lite_open opens the NEW url.
    this._release();

    if (typeof relaySlotRegistry === 'undefined' || typeof window.cefQuery !== 'function') {
      this.logger.error('relay transport unavailable (relay_transport.js / cefQuery)');
      this._emitStatus('error');
      return;
    }

    this.logger.info('Connecting to ' + url);
    this._cefQuery({
      action: 'lite_open',
      connId: 'wsp',
      url: url,
      urlKey: (typeof relayTransport !== 'undefined' && relayTransport.urlKey) ? relayTransport.urlKey : 'unknown'
    }, (cid) => {
      this.clientId = cid;
      this.connected = true;
      // Route our pushes through the generic dispatcher (pseudo-slot).
      // _resyncIfNeeded hooks the shared 10s watchdog in relay_transport.js.
      relaySlotRegistry.set(cid, {
        _onPush: (message) => { this._handlePush(message); },
        _resyncIfNeeded: () => { this._resyncIfNeeded(); },
        close: () => { this.disconnect(); }
      });
      this.logger.info('Connected');
      this._emitStatus('connected');
      this.authenticated = false;
      // Auto-authenticate if apiKey is set (same envelope as before)
      const key = this._getApiKey();
      if (key) {
        this._send({ type: 'auth', token: key, clientId: this._getClientId() });
      }
      // Register a routing name so /solo <name> can target this page
      // (mirrors the relay_agent_lite.js handshake).
      this._send({ type: 'register', name: CONFIG.get('relay.agentName') || 'webagent' });
      this._startHeartbeat();
      // With no api key there is no auth_ok to wait for: flush right away.
      // With a key the flush is gated on auth_ok inside _flushPendingPushes.
      this._flushPendingPushes();
    }, (code, msgText) => {
      this.logger.error('lite_open failed (' + code + '): ' + msgText);
      this._emitStatus('error');
      if (this.autoReconnect) {
        this._scheduleReconnect();
      }
    });
  }

  // Disconnect
  disconnect() {
    this.autoReconnect = false;
    if (this._reconnectTimer) {
      clearTimeout(this._reconnectTimer);
      this._reconnectTimer = null;
    }
    this._stopHeartbeat();
    this._release();
    this.connected = false;
    this.authenticated = false;
    // Notify listeners (e.g. the relay panel button) so the UI updates.
    this._emitStatus('disconnected');
  }

  // Send a chat message, returns a Promise that resolves with the response
  sendMessage(text, opts) {
    opts = opts || {};
    const id = this._nextId();
    return new Promise((resolve, reject) => {
      this.callbacks[id] = { resolve, reject, timer: null };
      // Timeout after 60s
      this.callbacks[id].timer = setTimeout(() => {
        if (this.callbacks[id]) {
          delete this.callbacks[id];
          reject(new Error('Timeout'));
        }
      }, opts.timeout || 60000);

      this._send({
        type: 'message',
        content: text,
        session: opts.session || this._getSession(),
        requestId: id,
        channelId: this._lastChannelId
      });
    });
  }

  // Push a message without waiting for response (fire-and-forget).
  // When the relay connection is down the push is queued (bounded) and
  // flushed after reconnect + auth instead of being silently dropped.
  // No inbound gate: LLM replies are always forwarded. With no channelId
  // the relay server path is routed by the chatroom (contact_claw_<sender>),
  // and the wechat bridges fall back to the most recent wechat session.
  pushMessage(text, opts) {
    opts = opts || {};
    if (!this.connected || !this.clientId) {
      if (this._pendingPushes.length >= this.maxPendingPushes) {
        const dropped = this._pendingPushes.shift();
        this.logger.warn('pending push queue full (' + this.maxPendingPushes
          + '), dropped oldest (len ' + String(dropped && dropped.text || '').length + ')');
      }
      // Snapshot the channelId at enqueue time: a newer inbound message may
      // update _lastChannelId before the flush, but this reply belongs to
      // the channel it was produced for.
      this._pendingPushes.push({ text: text, opts: opts, channelId: this._lastChannelId });
      this.logger.info('relay not connected, push queued (pending: ' + this._pendingPushes.length + ')');
      return true;
    }
    return this._send({
      type: 'message',
      content: text,
      session: opts.session || this._getSession(),
      channelId: this._lastChannelId
    });
  }

  // Call a tool via the relay
  callTool(tool, params, opts) {
    opts = opts || {};
    const id = this._nextId();
    return new Promise((resolve, reject) => {
      this.callbacks[id] = { resolve, reject, timer: null };
      this.callbacks[id].timer = setTimeout(() => {
        if (this.callbacks[id]) {
          delete this.callbacks[id];
          reject(new Error('Timeout'));
        }
      }, opts.timeout || 60000);

      this._send({
        type: 'tool_request',
        tool: tool,
        params: params || {},
        requestId: id
      });
    });
  }

  // Set callback for incoming messages (streaming, server-push, etc.)
  onMessage(fn) {
    this._onMessage = fn;
  }

  // Set callback for connection status changes
  onStatus(fn) {
    this._onStatus = fn;
  }

  // Send a custom command type to the relay server (fire-and-forget).
  sendCommand(type, extra) {
    const obj = Object.assign({ type: type }, extra || {});
    return this._send(obj);
  }

  // ---- internal ----

  _nextId() {
    return 'req_' + (++this.reqId);
  }

  _send(obj) {
    if (!this.connected || !this.clientId) {
      this.logger.warn('Cannot send, not connected');
      return false;
    }
    this._cefQuery({
      action: 'agent_send',
      clientId: this.clientId,
      message: JSON.stringify(obj)
    });
    return true;
  }

  _release() {
    if (this.clientId) {
      if (typeof relaySlotRegistry !== 'undefined') {
        relaySlotRegistry.delete(this.clientId);
      }
      this._cefQuery({ action: 'agent_unregister', clientId: this.clientId });
      this.clientId = null;
    }
  }

  // Inbound: pushes from the relay connection (pseudo-slot _onPush payload).
  _handlePush(text) {
    let data;
    try {
      data = JSON.parse(text);
    } catch (e) {
      this.logger.warn('Non-JSON message received');
      return;
    }
    if (!data || !data.type) return;

    // Browser-side connection state for our wsp connection.
    if (data.type === 'relay_state') {
      if (data.state === 'connected') {
        // Link-layer recovery on the same client id: the connection may be
        // usable again (auth gate is enforced inside the flush).
        this._flushPendingPushes();
        return;  // already emitted at open
      }
      this.connected = false;
      this.authenticated = false;
      this._stopHeartbeat();
      this.logger.info('Disconnected');
      this._emitStatus('disconnected');
      if (this.autoReconnect) {
        this._scheduleReconnect();
      }
      return;
    }

    // Check auth response
    if (data.type === 'auth_response' || data.type === 'auth_ok') {
      this.authenticated = true;
      this._emitStatus('auth_ok');
      this._flushPendingPushes();
      return;
    }

    // Track remote conversations: any inbound 'message' push marks that a
    // remote session exists (the local bridge sends channelId, the remote
    // relay sends sender/session instead - both count) and remembers the
    // channel for bridge-side reply routing. Diagnostic only - pushMessage
    // no longer gates on this.
    if (data.type === 'message') {
      this._hadRemoteInbound = true;
      if (data.channelId) {
        this._lastChannelId = data.channelId;
      }
      this._saveSessionState();
    }

    // Check if this is a response to a pending request
    if (data.requestId && this.callbacks[data.requestId]) {
      const cb = this.callbacks[data.requestId];
      clearTimeout(cb.timer);
      delete this.callbacks[data.requestId];
      cb.resolve(data);
      return;
    }

    // Delegate to user callback
    if (this._onMessage) {
      try {
        this._onMessage(data);
      } catch (e) {
        this.logger.error('onMessage callback error: ' + e.message);
      }
    }
  }

  // Send everything queued while the connection was down. Order-preserving;
  // a send failure mid-flush re-queues the remainder.
  _flushPendingPushes() {
    if (this._pendingPushes.length === 0) return;
    if (!this.connected || !this.clientId) return;
    // The relay server rejects messages from unauthenticated clients, so
    // with an api key configured the flush must wait for auth_ok.
    if (this._getApiKey() && !this.authenticated) return;
    const queue = this._pendingPushes;
    this._pendingPushes = [];
    this.logger.info('Flushing ' + queue.length + ' pending push(es)');
    for (const item of queue) {
      const sent = this._send({
        type: 'message',
        content: item.text,
        session: (item.opts && item.opts.session) || this._getSession(),
        channelId: item.channelId != null ? item.channelId : this._lastChannelId
      });
      if (!sent) {
        // Connection dropped mid-flush: re-queue the remainder in order.
        const idx = queue.indexOf(item);
        this._pendingPushes = queue.slice(idx).concat(this._pendingPushes);
        this.logger.warn('Flush interrupted, ' + this._pendingPushes.length + ' push(es) re-queued');
        return;
      }
    }
  }

  _emitStatus(status) {
    if (this._onStatus) {
      try { this._onStatus(status); } catch (_) { }
    }
  }

  _scheduleReconnect() {
    if (this._reconnectTimer) return;
    const delay = CONFIG.get('relay.reconnectDelay') || 5000;
    this.logger.info('Reconnecting in ' + delay + 'ms');
    this._reconnectTimer = setTimeout(() => {
      this._reconnectTimer = null;
      this.connect({ autoReconnect: this.autoReconnect });
    }, delay);
  }

  // Pull-based link-state resync, called by the shared 10s watchdog in
  // relay_transport.js. Heals the case where a relay_state push was lost:
  // connected stays true locally while the underlying wsclient link is dead.
  _resyncIfNeeded() {
    if (!this.connected || !this.clientId) return;
    const cid = this.clientId;
    this._cefQuery({
      action: 'agent_state',
      clientId: cid
    }, (state) => {
      // Ignore stale answers (manual disconnect / reconnect raced the query).
      if (!this.connected || this.clientId !== cid) return;
      if (state === 'disconnected' || state === 'failed' || state === 'unknown') {
        this.connected = false;
        this._stopHeartbeat();
        this.logger.info('Disconnected (resync, link state ' + state + ')');
        this._emitStatus('disconnected');
        if (this.autoReconnect) {
          this._scheduleReconnect();
        }
      }
      // connected: already fine; reconnecting / connecting: link layer
      // is still healing itself, wait for the next tick.
    }, () => { /* query failed: the next watchdog tick retries */ });
  }

  _startHeartbeat() {
    this._stopHeartbeat();
    const interval = CONFIG.get('relay.heartbeatInterval') || 30000;
    this._heartbeatTimer = setInterval(() => {
      this._send({ type: 'ping' });
    }, interval);
  }

  _stopHeartbeat() {
    if (this._heartbeatTimer) {
      clearInterval(this._heartbeatTimer);
      this._heartbeatTimer = null;
    }
  }

  _cefQuery(request, onSuccess, onFailure) {
    if (typeof window.cefQuery !== 'function') {
      this.logger.warn('cefQuery unavailable, request dropped: ' + request.action);
      if (onFailure) onFailure(-1, 'cefQuery unavailable');
      return false;
    }
    window.cefQuery({
      request: JSON.stringify(request),
      onSuccess: function (response) { if (onSuccess) onSuccess(response); },
      onFailure: function (code, msgText) { if (onFailure) onFailure(code, msgText); }
    });
    return true;
  }
}

// Expose on window
if (!window.Relay) window.Relay = {};
window.Relay.ws = new RelayWs();
