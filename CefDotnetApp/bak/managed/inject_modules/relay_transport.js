// ============================================================================
// RelayTransport - page-js transport through the browser process relay.
//
//   js --cefQuery--> browser dsl (on_browser_cef_query "agent_register"/"agent_send")
//        --> wsclient --> standalone AgentCore (script_agent.dsl)
//   push back: AgentCore --> browser dsl on_wsclient_message
//        --> send_javascript_code --> window.onAgentEvent(clientId, message)
//
// Replaces the metadslWorker WebSocket Worker transport (ws_manager.js /
// ws_worker.js) with the same public surface, so bridge.js / main.js /
// adapters switch by binding `metadslWorker` to this module:
//   isConnected / isRunning / queueMessage / queueReply / dequeueMessage /
//   getReceiveQueueCount / getSendQueueCount / start(port) / stop()
//
// Envelope dispatch (agent_result / agent_command / agent_callback) is moved
// here from ws_manager.js; raw MetaDSL result texts keep the "请简要复述"
// suffix semantics and land in the same reply queue.
//
// NOTE: not yet wired into the on_renderer_load_end injection bundle; it goes
// live together with the first migration bundle (bridge.js switching to the
// relay), so current behavior is unchanged.
// ============================================================================

// Creates the relay transport. `urlKey` identifies the page for push routing
// (browser dsl find_browser_id_by_url_key), e.g. "evaluation.woa.com/chat".
class RelayTransport {
  constructor() {
    this.logger = (typeof logger !== 'undefined' && logger)
      ? logger.createLogger('RelayTransport')
      : { info: () => { }, warn: () => { }, error: () => { }, debug: () => { } };
    this.clientId = null;
    this.urlKey = (typeof CONFIG !== 'undefined' && CONFIG && CONFIG.relayUrlKey)
      ? CONFIG.relayUrlKey
      : (typeof location !== 'undefined' ? location.host : 'unknown');
    this.isRunning = false;      // registered with the browser relay
    this.isConnected = false;    // underlying wsclient connected to AgentCore
    this.relayAvailable = true;  // false after repeated failures (stop trying)
    this.registerAttempts = 0;
    this.maxRegisterAttempts = 10;
    this.registerTimeoutMs = 12000;  // C# connect timeout is 10s + margin
    // Reply queue (raw MetaDSL results), same shape as metadslWorker's
    this.fromWorkerQueue = [];
    this.reconnectDelay = 2000;
    this._reconnectTimer = null;
    this._registerTimer = null;
    // Promise-based agent_call support (relayTransport.callAgent): id -> cb
    this.callId = 0;
    this.pendingCalls = new Map();
  }

  // ---- registration ----------------------------------------------------

  // Register this page with the browser relay. Returns true when the
  // request was sent (the clientId arrives asynchronously in onSuccess).
  register(urlKey) {
    if (!this.relayAvailable) {
      return false;
    }
    // Already registered or a register is in flight: nothing to do.
    if (this.isRunning || this._registerTimer) {
      return true;
    }
    if (urlKey) {
      this.urlKey = urlKey;
    }
    this.registerAttempts = this.registerAttempts + 1;
    const sent = this._cefQuery({
      action: 'agent_register',
      urlKey: this.urlKey
    }, (clientId) => {
      this._clearRegisterTimer();
      this.clientId = clientId;
      this.isRunning = true;
      this.isConnected = true;
      this.registerAttempts = 0;
      this.logger.info('relay registered: ' + clientId + ' (' + this.urlKey + ')');
    }, (code, msg) => {
      this._clearRegisterTimer();
      this.isConnected = false;
      this.logger.warn('relay register failed (' + code + '): ' + msg);
      this._giveUpOrRetry();
    });
    if (sent) {
      // Safety net: the browser side completes the query on connected/failed
      // (C# connect timeout 10s); if that event is ever lost, fail here.
      this._registerTimer = setTimeout(() => {
        this._registerTimer = null;
        this.isConnected = false;
        this.logger.warn('relay register timed out after ' + this.registerTimeoutMs + 'ms');
        this._giveUpOrRetry();
      }, this.registerTimeoutMs);
    }
    return sent;
  }

  _clearRegisterTimer() {
    if (this._registerTimer) {
      clearTimeout(this._registerTimer);
      this._registerTimer = null;
    }
  }

  _giveUpOrRetry() {
    if (this.registerAttempts >= this.maxRegisterAttempts) {
      this.relayAvailable = false;
      this.isRunning = false;
      this.logger.warn('relay unavailable after ' + this.registerAttempts + ' attempts, staying on legacy channel');
      return;
    }
    this._scheduleReconnect();
  }

  // Compat shim for metadslWorker.start(port)
  start() {
    return this.register();
  }

  stop() {
    if (this._reconnectTimer) {
      clearTimeout(this._reconnectTimer);
      this._reconnectTimer = null;
    }
    this._clearRegisterTimer();
    if (this.clientId) {
      this._cefQuery({
        action: 'agent_unregister',
        clientId: this.clientId
      }, () => { }, () => { });
      this.clientId = null;
    }
    this.isRunning = false;
    this.isConnected = false;
  }

  _scheduleReconnect() {
    if (this._reconnectTimer) {
      return;
    }
    this._reconnectTimer = setTimeout(() => {
      this._reconnectTimer = null;
      this.register();
    }, this.reconnectDelay);
  }

  // ---- sending ---------------------------------------------------------

  // Send a text message (envelope JSON or raw MetaDSL code) through the
  // relay. Fire-and-forget like the worker path: responses/pushes arrive
  // through window.onAgentEvent.
  queueMessage(message) {
    if (!this.isConnected || !this.clientId) {
      this.logger.warn('relay not connected, message dropped (length: ' + (message ? message.length : 0) + ')');
      return false;
    }
    return this._cefQuery({
      action: 'agent_send',
      clientId: this.clientId,
      message: message
    }, () => {
      this.logger.info('relay message sent (length: ' + message.length + ')');
    }, (code, msg) => {
      this.logger.warn('relay send failed (' + code + '): ' + msg);
      this.isConnected = false;
      this._giveUpOrRetry();
    });
  }

  // ---- agent envelopes (single-page adapters + promise calls) -----------
  //
  // Shared sender with the same wait/retry policy as bridge.js: the relay
  // auto-registers on load; a fresh browser needs ~15s to boot the standalone
  // AgentCore, so messages wait (up to ~18s) instead of being dropped.

  _sendAgentEnvelope(type, id, func, args, attempt) {
    if (this.isConnected && this.clientId) {
      const envelope = JSON.stringify({ type: type, id: id, func: func, args: args });
      if (this.queueMessage(envelope)) {
        return true;
      }
      // Relay dropped the message (disconnect race): retry below.
    }
    if (attempt < 60 && this.relayAvailable) {
      const self = this;
      setTimeout(function () {
        self._sendAgentEnvelope(type, id, func, args, attempt + 1);
      }, 300);
      return false;
    }
    this.logger.warn('relay unavailable, envelope dropped: ' + func);
    return false;
  }

  // Fire-and-forget handle_agent_command for the single-page adapters
  // (msgJson: {id, command, params}); the reply arrives via
  // window.onAgentResponse (agent_result envelope routed in
  // _dispatchAgentEnvelope below).
  sendAgentCommandJson(msgJson) {
    let id = 0;
    try {
      id = JSON.parse(msgJson).id || 0;
    } catch (e) { /* keep id 0 */ }
    return this._sendAgentEnvelope('agent_call', id, 'handle_agent_command', [msgJson], 0);
  }

  // Fire-and-forget handle_agent_notification for the single-page adapters.
  sendAgentNotificationJson(msgJson) {
    return this._sendAgentEnvelope('agent_notify', 0, 'handle_agent_notification', [msgJson], 0);
  }

  // Promise-based agent_call (replaces the old synchronous
  // callMetaDSL('get_plan', ...) style getters in the adapters). Resolves
  // with the data string, rejects on failure/timeout.
  callAgent(funcName, args, timeoutMs) {
    const self = this;
    timeoutMs = timeoutMs || 8000;
    return new Promise(function (resolve, reject) {
      const id = ++self.callId;
      let settled = false;
      const timer = setTimeout(function () {
        if (settled) return;
        settled = true;
        self.pendingCalls.delete(id);
        reject(new Error('relay callAgent timeout: ' + funcName));
      }, timeoutMs);
      self.pendingCalls.set(id, function (success, data, error) {
        if (settled) return;
        settled = true;
        clearTimeout(timer);
        self.pendingCalls.delete(id);
        if (success) {
          resolve(data == null ? '' : String(data));
        } else {
          reject(new Error(error || ('relay callAgent failed: ' + funcName)));
        }
      });
      self._sendAgentEnvelope('agent_call', id, funcName, args || [], 0);
    });
  }

  // ---- reply queue (metadslWorker compatible surface) ------------------

  queueReply(message, noAgentMarker = false, channelId = null) {
    this.fromWorkerQueue.push({ message: message, noAgentMarker: noAgentMarker, channelId: channelId });
    return true;
  }

  dequeueMessage() {
    if (this.fromWorkerQueue.length > 0) {
      return this.fromWorkerQueue.shift();
    }
    return null;
  }

  getReceiveQueueCount() {
    return this.fromWorkerQueue.length;
  }

  getSendQueueCount() {
    return 0;  // cefQuery is fire-and-forget, no send queue
  }

  // ---- push dispatch (called by the browser via send_javascript_code) --

  // Entry point installed on window; routes everything the relay pushes:
  // relay state changes, agent envelopes and raw MetaDSL result texts.
  onAgentEvent(clientId, message) {
    if (clientId !== this.clientId) {
      this.logger.warn('onAgentEvent dropped: clientId mismatch (got ' + clientId + ', own ' + this.clientId + ')');
      return;  // push for another page sharing this renderer
    }
    if (typeof message !== 'string' || message.length === 0) {
      return;
    }
    if (this._dispatchRelayState(message)) {
      return;
    }
    if (this._dispatchAgentEnvelope(message)) {
      return;
    }
    // Raw MetaDSL result text: same queue semantics as the worker transport.
    this.logger.info('onAgentEvent raw result queued (length: ' + message.length + ')');
    this.fromWorkerQueue.push({
      message: message + "\n\n请简要复述本次执行要点以留存；如有新的MetaDSL代码同一轮发出（有才发，不要重复发），避免下轮结果遗忘傻眼。",
      noAgentMarker: false
    });
  }

  _dispatchRelayState(message) {
    if (message.charAt(0) !== '{') {
      return false;
    }
    let msg = null;
    try {
      msg = JSON.parse(message);
    } catch (e) {
      return false;
    }
    if (!msg || msg.type !== 'relay_state') {
      return false;
    }
    if (msg.state === 'connected') {
      this.isConnected = true;
      this.logger.info('relay connected');
    } else {
      this.isConnected = false;
      this.clientId = null;  // the wsclient id is dead, a register gets a new one
      this.logger.warn('relay ' + msg.state + ', will re-register');
      this._scheduleReconnect();
    }
    return true;
  }

  // Envelope dispatch, moved here from ws_manager.js dispatchAgentEnvelope.
  _dispatchAgentEnvelope(text) {
    if (!text || text.charAt(0) !== '{') {
      return false;
    }
    let msg = null;
    try {
      msg = JSON.parse(text);
    } catch (e) {
      return false;
    }
    if (!msg || typeof msg !== 'object' || !msg.type) {
      return false;
    }
    if (msg.type === 'agent_result') {
      // Promise calls (relayTransport.callAgent) first, then the main bundle
      // bridge, then the single-page adapters' window.onAgentResponse.
      const cb = this.pendingCalls.get(msg.id);
      if (cb) {
        cb(msg.success !== false, msg.data, msg.error || '');
        return true;
      }
      if (typeof bridge !== 'undefined' && bridge && typeof bridge.handleResponse === 'function') {
        bridge.handleResponse(text);
        return true;
      }
      if (typeof window !== 'undefined' && typeof window.onAgentResponse === 'function') {
        window.onAgentResponse(text);
        return true;
      }
      return true;
    }
    if (msg.type === 'agent_command') {
      if (typeof window !== 'undefined' && typeof window.onAgentCommand === 'function') {
        const payload = { command: msg.command, params: msg.params || {} };
        window.onAgentCommand(JSON.stringify(payload));
      }
      return true;
    }
    if (msg.type === 'agent_callback') {
      if (typeof window !== 'undefined' && typeof window.onAgentCallback === 'function') {
        window.onAgentCallback(msg.msg, msg.args || []);
      }
      return true;
    }
    return false;
  }

  // ---- cefQuery plumbing -----------------------------------------------

  _cefQuery(request, onSuccess, onFailure) {
    if (typeof window === 'undefined' || typeof window.cefQuery !== 'function') {
      this.logger.warn('cefQuery unavailable, request dropped: ' + request.action);
      if (onFailure) {
        onFailure(-1, 'cefQuery unavailable');
      }
      return false;
    }
    window.cefQuery({
      request: JSON.stringify(request),
      onSuccess: function (response) {
        if (onSuccess) {
          onSuccess(response);
        }
      },
      onFailure: function (code, msg) {
        if (onFailure) {
          onFailure(code, msg);
        }
      }
    });
    return true;
  }
}

// Global instance; bind metadslWorker to this (or keep both and switch in
// bridge.js) when the first migration bundle activates the relay.
const relayTransport = new RelayTransport();

// ============================================================================
// RelaySlot - WebSocket-compatible execution slot over the relay.
//
// The single-page adapters (hyarena/venus/...) run per-slot execution
// connections: raw MetaDSL code in, result text out, slot identity implied
// by the connection. Over the relay each slot gets its own registration
// (its own wsclient connection on the browser side), so results come back
// on the slot's own clientId. This class mimics the WebSocket API surface
// the adapters use (readyState/onopen/onmessage/onclose/send/close) so the
// adapters only swap the constructor call.
//
// After registering, the slot sends an agent_bind envelope so the raw
// MetaDSL worker path resolves its AgentInstance by the slot's agent id
// (context injection etc.) instead of the server default.
// ============================================================================

const relaySlotRegistry = new Map();  // clientId -> RelaySlot

class RelaySlot {
  constructor(agentId, urlKey) {
    this.agentId = agentId || 'webagent';
    this.urlKey = urlKey || relayTransport.urlKey;
    this.clientId = null;
    this.readyState = RelaySlot.CONNECTING;
    this.onopen = null;
    this.onmessage = null;
    this.onclose = null;
    this.onerror = null;
    this._registerAttempts = 0;
    this._registerTimer = null;
    this._reconnectTimer = null;
    this._closedByUser = false;
    this._register();
  }

  // WebSocket readyState constants.
  get CONNECTING() { return 0; }
  get OPEN() { return 1; }
  get CLOSING() { return 2; }
  get CLOSED() { return 3; }

  _register() {
    if (this._closedByUser) {
      return;
    }
    this.readyState = RelaySlot._CONNECTING;
    this._registerAttempts += 1;
    const self = this;
    const sent = relayTransport._cefQuery({
      action: 'agent_register',
      urlKey: this.urlKey
    }, function (clientId) {
      self._clearRegisterTimer();
      self.clientId = clientId;
      relaySlotRegistry.set(clientId, self);
      self._registerAttempts = 0;
      // Bind the connection to this slot's agent id for the raw code path.
      relayTransport._cefQuery({
        action: 'agent_send',
        clientId: clientId,
        message: JSON.stringify({ type: 'agent_bind', agentId: self.agentId })
      }, function () { }, function () { });
      if (self.readyState !== RelaySlot._OPEN) {
        self.readyState = RelaySlot._OPEN;
        if (typeof self.onopen === 'function') self.onopen();
      }
    }, function (code, msg) {
      self._clearRegisterTimer();
      self.loggerWarn('register failed (' + code + '): ' + msg);
      self._retryOrGiveUp();
    });
    if (sent) {
      this._registerTimer = setTimeout(function () {
        self._registerTimer = null;
        self.loggerWarn('register timed out');
        self._retryOrGiveUp();
      }, 12000);
    }
  }

  _clearRegisterTimer() {
    if (this._registerTimer) {
      clearTimeout(this._registerTimer);
      this._registerTimer = null;
    }
  }

  _retryOrGiveUp() {
    if (this._closedByUser) {
      return;
    }
    if (this._registerAttempts >= 60) {
      this.loggerWarn('giving up after ' + this._registerAttempts + ' attempts');
      this.readyState = RelaySlot._CLOSED;
      if (typeof this.onerror === 'function') this.onerror();
      if (typeof this.onclose === 'function') this.onclose();
      return;
    }
    const self = this;
    if (!this._reconnectTimer) {
      this._reconnectTimer = setTimeout(function () {
        self._reconnectTimer = null;
        self._register();
      }, 2000);
    }
  }

  loggerWarn(msg) {
    if (typeof logger !== 'undefined' && logger) {
      logger.createLogger('RelaySlot[' + this.agentId + ']').warn(msg);
    }
  }
  loggerInfo(msg) {
    if (typeof logger !== 'undefined' && logger) {
      logger.createLogger('RelaySlot[' + this.agentId + ']').info(msg);
    }
  }

  // WebSocket-style send: raw MetaDSL code text.
  send(text) {
    if (this.readyState !== RelaySlot._OPEN || !this.clientId) {
      this.loggerWarn('send dropped, slot not open');
      return false;
    }
    return relayTransport._cefQuery({
      action: 'agent_send',
      clientId: this.clientId,
      message: text
    }, function () { }, (function (self) {
      return function (code, msg) {
        self.loggerWarn('send failed (' + code + '): ' + msg);
        self._handleDisconnect();
      };
    })(this));
  }

  // Manual close (adapter ws_stop / reconnect API): does NOT fire onclose,
  // matching the adapters' expectation that close()+recreate is immediate.
  close() {
    this._closedByUser = true;
    this._clearRegisterTimer();
    if (this._reconnectTimer) {
      clearTimeout(this._reconnectTimer);
      this._reconnectTimer = null;
    }
    if (this.clientId) {
      const cid = this.clientId;
      relaySlotRegistry.delete(cid);
      relayTransport._cefQuery({
        action: 'agent_unregister',
        clientId: cid
      }, function () { }, function () { });
      this.clientId = null;
    }
    this.readyState = RelaySlot._CLOSED;
  }

  _handleDisconnect() {
    if (this._closedByUser) {
      return;
    }
    if (this.clientId) {
      relaySlotRegistry.delete(this.clientId);
      this.clientId = null;
    }
    if (this.readyState !== RelaySlot._CLOSED) {
      this.readyState = RelaySlot._CLOSED;
      if (typeof this.onclose === 'function') this.onclose();
    }
    this._retryOrGiveUp();
  }

  // Push entry: called by the window.onAgentEvent dispatcher when the
  // clientId matches this slot.
  _onPush(message) {
    if (typeof message !== 'string' || message.length === 0) {
      return;
    }
    if (message.charAt(0) === '{') {
      let msg = null;
      try {
        msg = JSON.parse(message);
      } catch (e) {
        msg = null;
      }
      if (msg && msg.type === 'relay_state') {
        if (msg.state === 'connected') {
          if (this.readyState !== RelaySlot._OPEN) {
            this.readyState = RelaySlot._OPEN;
            if (typeof this.onopen === 'function') this.onopen();
          }
        } else {
          this._handleDisconnect();
        }
        return;
      }
      // Envelopes are not expected on slot connections (raw code path only);
      // log and drop.
      if (msg && msg.type) {
        this.loggerWarn('unexpected envelope on slot connection: ' + msg.type);
        return;
      }
      // Fall through: JSON-looking raw code result.
    }
    // Raw MetaDSL result text, delivered as-is (the "请简要复述" suffix is a
    // main-page state machine convention, the adapters have their own).
    this.loggerInfo('result ' + message.length + 'B');
    if (typeof this.onmessage === 'function') {
      this.onmessage({ data: message });
    }
  }
}

// Numeric constants accessible without an instance (adapter code compares
// against the global WebSocket.OPEN == 1 etc.).
RelaySlot._CONNECTING = 0;
RelaySlot._OPEN = 1;
RelaySlot._CLOSING = 2;
RelaySlot._CLOSED = 3;

// Factory used by the adapters: relayTransport.createSlot(AGENT_ID) returns
// a WebSocket-compatible slot bound to that agent.
RelayTransport.prototype.createSlot = function (agentId, urlKey) {
  return new RelaySlot(agentId, urlKey);
};

// The browser pushes with: window.onAgentEvent('<clientId>', '<message>')
if (typeof window !== 'undefined' && typeof window.onAgentEvent === 'undefined') {
  window.onAgentEvent = function (clientId, message) {
    if (clientId === relayTransport.clientId) {
      relayTransport.onAgentEvent(clientId, message);
      return;
    }
    const slot = relaySlotRegistry.get(clientId);
    if (slot) {
      slot._onPush(message);
      return;
    }
    // Stale client id (registration died / re-registered): ignore.
  };
}

// Register as soon as the bundle loads: bridge.js then has the relay before
// its first sendCommand/sendNotification, instead of falling through to the
// legacy callMetaDSL path during the registration window (in the new
// deployment the renderer has no agent apis, that path is broken by design).
if (typeof window !== 'undefined') {
  setTimeout(function () {
    relayTransport.register();
  }, 0);

  // Zombie-connection cleanup (graceful path): on refresh/navigation/close
  // unregister the control connection and close every relay slot, so the
  // browser side drops the wsclient connections immediately. Renderer crash
  // (no pagehide) is covered by the browser dsl reaper.
  window.addEventListener('pagehide', function () {
    try {
      const slots = [];
      relaySlotRegistry.forEach(function (slot) { slots.push(slot); });
      slots.forEach(function (slot) {
        try { slot.close(); } catch (e) { /* best effort */ }
      });
      relayTransport.stop();
    } catch (e) { /* best effort */ }
  });
}
