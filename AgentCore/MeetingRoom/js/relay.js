// ============================================================================
// RelayClient - WebSocket connection to OpenClaw-Relay server
// ============================================================================
class RelayClient {
  constructor() {
    this.ws = null;
    this.connected = false;
    this.authenticated = false;
    this.autoReconnect = true;
    this._reconnectTimer = null;
    this._heartbeatTimer = null;
    this._onMessage = null;
    this._onStatus = null;
    this._reqId = 0;
    this._callbacks = {};
  }

  // Connect to relay server
  connect(url, apiKey) {
    if (this.ws && (this.ws.readyState === WebSocket.CONNECTING || this.ws.readyState === WebSocket.OPEN)) {
      console.warn('[relay] Already connected or connecting');
      return;
    }
    this._url = url;
    this._apiKey = apiKey;
    console.log('[relay] Connecting to ' + url);
    this._emitStatus('connecting');

    try {
      this.ws = new WebSocket(url);
    } catch (e) {
      console.error('[relay] Failed to create WebSocket: ' + e.message);
      this._emitStatus('error');
      return;
    }

    this.ws.onopen = () => {
      this.connected = true;
      console.log('[relay] Connected');
      this._emitStatus('connected');
      if (this._apiKey) {
        this._send({ type: 'auth', token: this._apiKey });
      }
      this._startHeartbeat();
    };

    this.ws.onmessage = (event) => {
      this._handleMessage(event);
    };

    this.ws.onerror = () => {
      console.error('[relay] WebSocket error');
      this._emitStatus('error');
    };

    this.ws.onclose = () => {
      this.connected = false;
      this.authenticated = false;
      this._stopHeartbeat();
      console.log('[relay] Disconnected');
      this._emitStatus('disconnected');
      if (this.autoReconnect) {
        this._scheduleReconnect();
      }
    };
  }

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
    this.authenticated = false;
  }

  // Send a chat message to relay
  // receiver: '*' = all non-AiClaw, '!name' = exclude name, 'name' = only name, array = multiple names
  sendMessage(text, sender, receiver, context, channelId) {
    const msg = {
      type: 'message',
      content: text,
      requestId: 'req_' + (++this._reqId)
    };
    if (sender) {
      msg.sender = sender;
    }
    if (receiver) {
      // Support array of receivers: join into comma-separated string
      msg.receiver = Array.isArray(receiver) ? receiver.join(',') : receiver;
    }
    if (context) {
      msg.context = context;
    }
    if (channelId) {
      msg.channelId = channelId;
    }
    return this._send(msg);
  }


  onMessage(fn) { this._onMessage = fn; }
  onStatus(fn) { this._onStatus = fn; }

  // ---- internal ----

  _send(obj) {
    if (!this.ws || this.ws.readyState !== WebSocket.OPEN) {
      console.warn('[relay] Cannot send, not connected');
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
      console.warn('[relay] Non-JSON message received');
      return;
    }

    if (data.type === 'auth_response' || data.type === 'auth_ok') {
      this.authenticated = true;
      this.clientName = data.name || null;
      this._emitStatus('auth_ok');
      return;
    }

    if (data.type === 'pong') {
      return;
    }

    // Delegate to user callback
    if (this._onMessage) {
      try {
        this._onMessage(data);
      } catch (e) {
        console.error('[relay] onMessage callback error: ' + e.message);
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
    console.log('[relay] Reconnecting in 5s');
    this._reconnectTimer = setTimeout(() => {
      this._reconnectTimer = null;
      this.connect(this._url, this._apiKey);
    }, 5000);
  }

  _startHeartbeat() {
    this._stopHeartbeat();
    this._heartbeatTimer = setInterval(() => {
      this._send({ type: 'ping' });
    }, 30000);
  }

  _stopHeartbeat() {
    if (this._heartbeatTimer) {
      clearInterval(this._heartbeatTimer);
      this._heartbeatTimer = null;
    }
  }
}
