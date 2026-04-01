  // ============================================================================
  // OpenClaw WebSocket - Real-time bidirectional communication with OpenClaw
  // Used for chat / streaming
  // ============================================================================
  class OpenClawWs {
    constructor() {
      this.logger = logger.createLogger('OpenClawWs');
      this.ws = null;
      this.callbacks = {};
      this.reqId = 0;
      this.connected = false;
      this.autoReconnect = false;
      this._reconnectTimer = null;
      this._heartbeatTimer = null;
      this._onMessage = null;    // user callback: (data) => void
      this._onStatus = null;     // user callback: (status) => void  status: 'connected'|'disconnected'|'error'|'auth_ok'
    }

    _getUrl() {
      return CONFIG.get('openclaw.wsUrl') || 'wss://www.gamexyz.net:8443/ws';
    }

    _getApiKey() {
      return CONFIG.get('openclaw.apiKey') || '';
    }

    _getSession() {
      return CONFIG.get('openclaw.session') || '';
    }

    // ---- public API ----

    // Connect to WebSocket server
    connect(opts) {
      opts = opts || {};
      if (this.ws && (this.ws.readyState === WebSocket.CONNECTING || this.ws.readyState === WebSocket.OPEN)) {
        this.logger.warn('Already connected or connecting');
        return;
      }

      this.autoReconnect = opts.autoReconnect !== undefined ? opts.autoReconnect : true;
      const url = opts.url || this._getUrl();
      this.logger.info('Connecting to ' + url);
      this._emitStatus('connecting');

      try {
        this.ws = new WebSocket(url);
      } catch (e) {
        this.logger.error('Failed to create WebSocket: ' + e.message);
        this._emitStatus('error');
        return;
      }

      this.ws.onopen = () => {
        this.connected = true;
        this.logger.info('Connected');
        this._emitStatus('connected');
        // Auto-authenticate if apiKey is set
        const key = this._getApiKey();
        if (key) {
          this._send({ type: 'auth', token: key });
        }
        this._startHeartbeat();
      };

      this.ws.onmessage = (event) => {
        this._handleMessage(event);
      };

      this.ws.onerror = (err) => {
        this.logger.error('WebSocket error');
        this._emitStatus('error');
      };

      this.ws.onclose = () => {
        this.connected = false;
        this._stopHeartbeat();
        this.logger.info('Disconnected');
        this._emitStatus('disconnected');
        if (this.autoReconnect) {
          this._scheduleReconnect();
        }
      };
    }

    // Disconnect
    disconnect() {
      this.autoReconnect = false;
      if (this._reconnectTimer) {
        clearTimeout(this._reconnectTimer);
        this._reconnectTimer = null;
      }
      this._stopHeartbeat();
      if (this.ws) {
        this.ws.close();
        this.ws = null;
      }
      this.connected = false;
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
          requestId: id
        });
      });
    }

    // Push a message without waiting for response (fire-and-forget)
    pushMessage(text, opts) {
      opts = opts || {};
      return this._send({
        type: 'message',
        content: text,
        session: opts.session || this._getSession()
      });
    }

    // Call a tool via WebSocket
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

    // ---- internal ----

    _nextId() {
      return 'req_' + (++this.reqId);
    }

    _send(obj) {
      if (!this.ws || this.ws.readyState !== WebSocket.OPEN) {
        this.logger.warn('Cannot send, not connected');
        return false;
      }
      this.ws.send(JSON.stringify(obj));
      return true;
    }

    _handleMessage(event) {
      let data;
      try {
        data = JSON.parse(event.data);
      } catch (e) {
        this.logger.warn('Non-JSON message received');
        return;
      }

      // Check auth response
      if (data.type === 'auth_response' || data.type === 'auth_ok') {
        this._emitStatus('auth_ok');
        return;
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

    _emitStatus(status) {
      if (this._onStatus) {
        try { this._onStatus(status); } catch (_) {}
      }
    }

    _scheduleReconnect() {
      if (this._reconnectTimer) return;
      const delay = CONFIG.get('openclaw.reconnectDelay') || 5000;
      this.logger.info('Reconnecting in ' + delay + 'ms');
      this._reconnectTimer = setTimeout(() => {
        this._reconnectTimer = null;
        this.connect({ autoReconnect: this.autoReconnect });
      }, delay);
    }

    _startHeartbeat() {
      this._stopHeartbeat();
      const interval = CONFIG.get('openclaw.heartbeatInterval') || 30000;
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
  }

  // Expose on window
  if (!window.OpenClaw) window.OpenClaw = {};
  window.OpenClaw.ws = new OpenClawWs();
