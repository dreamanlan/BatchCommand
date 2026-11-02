'use strict';

// WxBridge: WebSocket client connecting to wechat-bridge.js (port 3000)
// Receives WeChat messages and sends replies back

class WxBridge {
  constructor() {
    this.ws = null;
    this.connected = false;
    this.url = 'ws://localhost:3000';
    this._onMessageCb = null;
    this._onStatusCb = null;
    this._reconnectTimer = null;
    this._autoReconnect = true;
  }

  connect(url) {
    if (url) this.url = url;
    this.disconnect();
    this._autoReconnect = true;

    try {
      this.ws = new WebSocket(this.url);
    } catch (e) {
      console.error('[WxBridge] Failed to create WebSocket:', e.message);
      this._emitStatus('error');
      this._scheduleReconnect();
      return;
    }

    this.ws.onopen = () => {
              console.log('[WxBridge] Connected to', this.url);
              this.connected = true;
              this._emitStatus('connected');
              this._send({ type: 'register', name: 'aichat' });
              // Request current status
              this._send({ type: 'wx_status_request' });
            };


    this.ws.onmessage = (event) => {
      this._handleMessage(event);
    };

    this.ws.onclose = () => {
      console.log('[WxBridge] Disconnected');
      this.connected = false;
      this._emitStatus('disconnected');
      this._scheduleReconnect();
    };

    this.ws.onerror = (e) => {
      console.error('[WxBridge] WebSocket error');
      this.connected = false;
      this._emitStatus('error');
    };
  }

  disconnect() {
    this._autoReconnect = false;
    if (this._reconnectTimer) {
      clearTimeout(this._reconnectTimer);
      this._reconnectTimer = null;
    }
    if (this.ws) {
      this.ws.onclose = null;
      this.ws.onerror = null;
      this.ws.close();
      this.ws = null;
    }
    this.connected = false;
  }

  // Send a reply to a WeChat user
  sendReply(toUser, text, contextToken) {
    this._send({
      type: 'wx_reply',
      data: {
        to_user: toUser,
        text: text,
        context_token: contextToken || ''
      }
    });
  }

  // Request login (triggers QR code flow on bridge)
  requestLogin() {
    this._send({ type: 'wx_login' });
  }

  // Request logout
  requestLogout() {
    this._send({ type: 'wx_logout' });
  }

  onMessage(fn) { this._onMessageCb = fn; }
  onStatus(fn) { this._onStatusCb = fn; }

  _send(obj) {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(obj));
    }
  }

  _handleMessage(event) {
    let data;
    try {
      data = JSON.parse(event.data);
    } catch (e) {
      console.error('[WxBridge] Invalid JSON:', event.data);
      return;
    }

    console.log('[WxBridge] Received:', data.type, data.data ? JSON.stringify(data.data).substring(0, 200) : '');
    switch (data.type) {
      case 'message':
                  // Incoming WeChat message
                  if (this._onMessageCb) {
                    this._onMessageCb(data);
                  }
        break;

      case 'wx_qrcode':
        // QR code for login
        if (this._onStatusCb) {
          this._onStatusCb({ type: 'qrcode', data: data.data });
        }
        break;

      case 'wx_status':
        // Status update
        if (this._onStatusCb) {
          this._onStatusCb({ type: 'status', data: data.data });
        }
        break;

      case 'wx_send_result':
        // Send result
        if (this._onStatusCb) {
          this._onStatusCb({ type: 'send_result', data: data.data });
        }
        break;

      default:
        console.log('[WxBridge] Unknown type:', data.type);
    }
  }

  _emitStatus(status) {
    if (this._onStatusCb) {
      this._onStatusCb({ type: 'connection', data: { status: status } });
    }
  }

  _scheduleReconnect() {
    if (!this._autoReconnect) return;
    if (this._reconnectTimer) return;
    this._reconnectTimer = setTimeout(() => {
      this._reconnectTimer = null;
      if (this._autoReconnect && !this.connected) {
        console.log('[WxBridge] Reconnecting...');
        this.connect();
      }
    }, 5000);
  }
}
