  // ============================================================================
  // OpenClawPanel - Floating configuration & chat test panel for OpenClaw
  // ============================================================================
  class OpenClawPanel {
    static STORAGE_KEY = 'openclaw_panel_config';

    constructor() {
      this.visible = false;
      this.panel = null;
      this.chatLog = null;
      this.chatInput = null;
      this.statusDot = null;
      this.statusText = null;
      this.connectBtn = null;
      // config fields
      this.cfgHttpBase = null;
      this.cfgWsUrl = null;
      this.cfgApiKey = null;
      this.cfgSession = null;
      this.onVisibilityChange = null;
      // Remote mode: forward openclaw messages to LLM and LLM responses to openclaw
      this.remoteEnabled = false;
      this.echoEnabled = false;
      // Reference to metadslWorker (set by main.js)
      this.metadslWorker = null;
      this.createPanel();
      // _restoreConfig is async, called explicitly from main.js after SecretStore is ready
      this._bindWsEvents();
    }

    _inputStyle() {
      return `
        flex: 1;
        background: #1e1e1e;
        color: #d4d4d4;
        border: 1px solid #444;
        border-radius: 4px;
        padding: 5px 8px;
        font-size: 12px;
        outline: none;
        min-width: 0;
      `;
    }

    _makeInput(type, placeholder, value) {
      const el = document.createElement('input');
      el.type = type;
      el.placeholder = placeholder;
      el.value = value || '';
      el.style.cssText = this._inputStyle();
      return el;
    }

    _makeLabel(text) {
      const el = document.createElement('span');
      el.textContent = text;
      el.style.cssText = 'color:#aaa;font-size:11px;white-space:nowrap;min-width:60px;';
      return el;
    }

    _makeRow() {
      const row = document.createElement('div');
      row.style.cssText = 'display:flex;gap:6px;align-items:center;';
      return row;
    }

    _makeBtn(text, bg, onClick) {
      const btn = document.createElement('button');
      btn.textContent = text;
      btn.style.cssText = `
        padding: 4px 12px;
        background: ${bg};
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
        white-space: nowrap;
      `;
      btn.onclick = onClick;
      return btn;
    }

    createPanel() {
      this.panel = document.createElement('div');
      this.panel.id = 'openclaw-panel';
      this.panel.style.cssText = `
        position: fixed;
        right: 20px;
        bottom: 120px;
        width: 420px;
        background: #2d2d2d;
        border: 1px solid #555;
        border-radius: 8px;
        display: none;
        flex-direction: column;
        z-index: 10002;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Arial, sans-serif;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
        padding: 0;
        gap: 0;
        max-height: 520px;
      `;

      // Prevent events from bubbling out
      ['input', 'change', 'paste', 'cut', 'keydown', 'keyup', 'keypress', 'beforeinput'].forEach(t => {
        this.panel.addEventListener(t, (e) => { e.stopPropagation(); }, false);
      });

      // ---- Header (draggable) ----
      const header = document.createElement('div');
      header.style.cssText = `
        padding: 8px 12px;
        background: #1a1a1a;
        border-bottom: 1px solid #444;
        border-radius: 8px 8px 0 0;
        display: flex;
        justify-content: space-between;
        align-items: center;
        cursor: move;
      `;
      const titleBox = document.createElement('div');
      titleBox.style.cssText = 'display:flex;align-items:center;gap:8px;';
      const title = document.createElement('span');
      title.textContent = 'OpenClaw';
      title.style.cssText = 'color:#fff;font-weight:600;font-size:13px;';
      // Status indicator
      this.statusDot = document.createElement('span');
      this.statusDot.style.cssText = 'width:8px;height:8px;border-radius:50%;background:#666;display:inline-block;';
      this.statusText = document.createElement('span');
      this.statusText.textContent = 'disconnected';
      this.statusText.style.cssText = 'color:#999;font-size:11px;';
      titleBox.appendChild(title);
      titleBox.appendChild(this.statusDot);
      titleBox.appendChild(this.statusText);
      const closeBtn = document.createElement('button');
      closeBtn.innerHTML = '&times;';
      closeBtn.style.cssText = 'background:none;border:none;color:#999;font-size:18px;cursor:pointer;padding:0 4px;';
      closeBtn.onclick = () => this.hide();
      header.appendChild(titleBox);
      header.appendChild(closeBtn);
      this.panel.appendChild(header);

      // ---- Config area ----
      const configArea = document.createElement('div');
      configArea.style.cssText = 'display:flex;flex-direction:column;gap:5px;padding:8px 10px;border-bottom:1px solid #444;';

      // HTTP Base
      const r1 = this._makeRow();
      this.cfgHttpBase = this._makeInput('text', 'http://localhost:3000', '');
      r1.appendChild(this._makeLabel('HTTP:'));
      r1.appendChild(this.cfgHttpBase);
      configArea.appendChild(r1);

      // WS URL
      const r2 = this._makeRow();
      this.cfgWsUrl = this._makeInput('text', 'ws://localhost:3000/ws', '');
      r2.appendChild(this._makeLabel('WS:'));
      r2.appendChild(this.cfgWsUrl);
      configArea.appendChild(r2);

      // API Key
      const r3 = this._makeRow();
      this.cfgApiKey = this._makeInput('password', 'API Key (optional)', '');
      r3.appendChild(this._makeLabel('API Key:'));
      r3.appendChild(this.cfgApiKey);
      configArea.appendChild(r3);

      // Session + buttons
      const r4 = this._makeRow();
      this.cfgSession = this._makeInput('text', 'session-id', '');
      this.cfgSession.style.cssText += 'max-width:120px;';
      r4.appendChild(this._makeLabel('Session:'));
      r4.appendChild(this.cfgSession);
      this.connectBtn = this._makeBtn('Connect', '#2196f3', () => this._toggleConnect());
      r4.appendChild(this.connectBtn);
      r4.appendChild(this._makeBtn('Status', '#555', () => this._testStatus()));

      // Remote checkbox - forward openclaw<->LLM
      const remoteLabel = document.createElement('label');
      remoteLabel.style.cssText = 'display:flex;align-items:center;gap:3px;color:#aaa;font-size:11px;cursor:pointer;white-space:nowrap;';
      this.remoteChk = document.createElement('input');
      this.remoteChk.type = 'checkbox';
      this.remoteChk.style.cssText = 'margin:0;cursor:pointer;';
      this.remoteChk.onchange = () => { this.remoteEnabled = this.remoteChk.checked; };
      remoteLabel.appendChild(this.remoteChk);
      remoteLabel.appendChild(document.createTextNode('Remote'));
      r4.appendChild(remoteLabel);

      // Echo checkbox - show forwarded messages in chat log
      const echoLabel = document.createElement('label');
      echoLabel.style.cssText = 'display:flex;align-items:center;gap:3px;color:#aaa;font-size:11px;cursor:pointer;white-space:nowrap;';
      this.echoChk = document.createElement('input');
      this.echoChk.type = 'checkbox';
      this.echoChk.style.cssText = 'margin:0;cursor:pointer;';
      this.echoChk.onchange = () => { this.echoEnabled = this.echoChk.checked; };
      echoLabel.appendChild(this.echoChk);
      echoLabel.appendChild(document.createTextNode('Echo'));
      r4.appendChild(echoLabel);

      configArea.appendChild(r4);

      this.panel.appendChild(configArea);

      // ---- Chat test area ----
      this.chatLog = document.createElement('textarea');
      this.chatLog.readOnly = true;
      this.chatLog.style.cssText = `
        flex: 1;
        min-height: 140px;
        background: #1e1e1e;
        color: #d4d4d4;
        border: none;
        border-bottom: 1px solid #444;
        padding: 8px 10px;
        font-family: 'Consolas', 'Monaco', monospace;
        font-size: 12px;
        resize: none;
        outline: none;
      `;
      this.panel.appendChild(this.chatLog);

      // Chat input row
      const chatRow = document.createElement('div');
      chatRow.style.cssText = 'display:flex;gap:6px;padding:8px 10px;align-items:flex-start;';
      this.chatInput = document.createElement('textarea');
      this.chatInput.placeholder = 'Type a message to test... (Shift+Enter for newline)';
      this.chatInput.rows = 3;
      this.chatInput.style.cssText = `
        flex: 1;
        background: #1e1e1e;
        color: #d4d4d4;
        border: 1px solid #444;
        border-radius: 4px;
        padding: 5px 8px;
        font-size: 12px;
        outline: none;
        min-width: 0;
        resize: vertical;
        line-height: 1.4;
        font-family: 'Consolas', 'Monaco', monospace;
      `;
      this.chatInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); this._sendChat(); }
      });
      chatRow.appendChild(this.chatInput);
      const btnCol = document.createElement('div');
      btnCol.style.cssText = 'display:flex;flex-direction:column;gap:4px;';
      btnCol.appendChild(this._makeBtn('Send', '#2196f3', () => this._sendChat()));
      btnCol.appendChild(this._makeBtn('Clear', '#ff9800', () => { this.chatLog.value = ''; }));
      btnCol.appendChild(this._makeBtn('Save', '#4caf50', () => this._saveConfig()));
      chatRow.appendChild(btnCol);
      this.panel.appendChild(chatRow);

      document.body.appendChild(this.panel);
      this._initDrag(header);
    }

    // ---- Config persistence ----

    async _saveConfig() {
      const cfg = {
        httpBase: this.cfgHttpBase.value.trim(),
        wsUrl: this.cfgWsUrl.value.trim(),
        apiKey: this.cfgApiKey.value,
        session: this.cfgSession.value.trim(),
        remoteEnabled: this.remoteEnabled,
        echoEnabled: this.echoEnabled
      };
      // Save sensitive fields (apiKey, session) to SecretStore
      try {
        await secretStore.setItem(OpenClawPanel.STORAGE_KEY, JSON.stringify(cfg));
      } catch (_) {}
      // Push to CONFIG so OpenClawHttp / OpenClawWs pick it up
      CONFIG.set('openclaw.httpBase', cfg.httpBase);
      CONFIG.set('openclaw.wsUrl', cfg.wsUrl);
      CONFIG.set('openclaw.apiKey', cfg.apiKey);
      CONFIG.set('openclaw.session', cfg.session);
      this._chatLog('[config] Saved');
    }

    async _restoreConfig() {
      // Restore from SecretStore
      try {
        const raw = await secretStore.getItem(OpenClawPanel.STORAGE_KEY);
        if (raw) {
          const cfg = JSON.parse(raw);
          this.cfgHttpBase.value = cfg.httpBase || '';
          this.cfgWsUrl.value = cfg.wsUrl || '';
          this.cfgApiKey.value = cfg.apiKey || '';
          this.cfgSession.value = cfg.session || '';
          // Restore checkbox states
          if (cfg.remoteEnabled) { this.remoteEnabled = true; this.remoteChk.checked = true; }
          if (cfg.echoEnabled) { this.echoEnabled = true; this.echoChk.checked = true; }
          // Push to CONFIG
          if (cfg.httpBase) CONFIG.set('openclaw.httpBase', cfg.httpBase);
          if (cfg.wsUrl) CONFIG.set('openclaw.wsUrl', cfg.wsUrl);
          if (cfg.apiKey) CONFIG.set('openclaw.apiKey', cfg.apiKey);
          if (cfg.session) CONFIG.set('openclaw.session', cfg.session);
        }
      } catch (_) {}
    }

    // ---- WebSocket events ----

    _bindWsEvents() {
      const ws = window.OpenClaw && window.OpenClaw.ws;
      if (!ws) return;
      ws.onStatus((status) => {
        this._updateStatus(status);
      });
      ws.onMessage((data) => {
        // Display server-push messages in chat log
        const msgText = data.content || data.text;
        if (data.type === 'message' && msgText) {
          this._chatLog('[server] ' + msgText);
          // Remote mode: forward openclaw message to LLM (no agent marker)
          if (this.remoteEnabled && this.metadslWorker) {
            this.metadslWorker.queueReply(msgText, true);
            if (this.echoEnabled) {
              this._chatLog('[remote->LLM] ' + msgText);
            }
          }
        } else {
          this._chatLog('[server] ' + JSON.stringify(data));
        }
      });
    }

    _updateStatus(status) {
      const colors = {
        'connecting': '#ff9800',
        'connected': '#4caf50',
        'disconnected': '#666',
        'error': '#f44336',
        'auth_ok': '#2196f3'
      };
      this.statusDot.style.background = colors[status] || '#666';
      this.statusText.textContent = status;
      // Update connect button text
      if (status === 'connected' || status === 'auth_ok') {
        this.connectBtn.textContent = 'Disconnect';
        this.connectBtn.style.background = '#f44336';
      } else if (status === 'disconnected' || status === 'error') {
        this.connectBtn.textContent = 'Connect';
        this.connectBtn.style.background = '#2196f3';
      }
    }

    async _toggleConnect() {
      const ws = window.OpenClaw && window.OpenClaw.ws;
      if (!ws) return;
      if (ws.connected) {
        ws.disconnect();
        this._chatLog('[ws] Disconnected');
      } else {
        // Push latest config to CONFIG before connecting
        await this._saveConfig();
        ws.connect();
        this._chatLog('[ws] Connecting...');
      }
    }

    // ---- Chat test ----

    async _sendChat() {
      const text = this.chatInput.value.trim();
      if (!text) return;
      this.chatInput.value = '';
      this._chatLog('[you] ' + text);

      const ws = window.OpenClaw && window.OpenClaw.ws;
      if (ws && ws.connected) {
        // Use WebSocket
        try {
          const resp = await ws.sendMessage(text);
          const wsText = resp && (resp.content || resp.text);
          if (wsText) {
            this._chatLog('[claw] ' + wsText);
          } else {
            this._chatLog('[claw] ' + JSON.stringify(resp));
          }
        } catch (e) {
          this._chatLog('[error] ' + e.message);
        }
      } else {
        // Fallback to HTTP
        const http = window.OpenClaw && window.OpenClaw.http;
        if (!http) {
          this._chatLog('[error] OpenClaw HTTP not available');
          return;
        }
        try {
          const resp = await http.chat(text);
          const httpText = resp && (resp.content || resp.text);
          if (httpText) {
            this._chatLog('[claw] ' + httpText);
          } else {
            this._chatLog('[claw] ' + JSON.stringify(resp));
          }
        } catch (e) {
          this._chatLog('[error] ' + e.message);
        }
      }
    }

    async _testStatus() {
      const http = window.OpenClaw && window.OpenClaw.http;
      if (!http) {
        this._chatLog('[error] OpenClaw HTTP not available');
        return;
      }
      await this._saveConfig();
      this._chatLog('[http] Checking status...');
      try {
        const resp = await http.status();
        this._chatLog('[status] ' + JSON.stringify(resp));
      } catch (e) {
        this._chatLog('[error] ' + e.message);
      }
    }

    _chatLog(msg) {
      const ts = new Date().toLocaleTimeString();
      this.chatLog.value += '[' + ts + '] ' + msg + '\n';
      this.chatLog.scrollTop = this.chatLog.scrollHeight;
    }

    // ---- Drag ----

    _initDrag(header) {
      const panel = this.panel;
      let dragging = false;
      let startX, startY, startLeft, startTop;
      header.addEventListener('mousedown', (e) => {
        const tag = e.target.tagName.toLowerCase();
        if (tag === 'input' || tag === 'button') return;
        dragging = true;
        startX = e.clientX;
        startY = e.clientY;
        const rect = panel.getBoundingClientRect();
        startLeft = rect.left;
        startTop = rect.top;
        panel.style.right = '';
        panel.style.bottom = '';
        panel.style.left = startLeft + 'px';
        panel.style.top = startTop + 'px';
        e.preventDefault();
      });
      document.addEventListener('mousemove', (e) => {
        if (!dragging) return;
        panel.style.left = (startLeft + e.clientX - startX) + 'px';
        panel.style.top = (startTop + e.clientY - startY) + 'px';
      });
      document.addEventListener('mouseup', () => { dragging = false; });
    }

    // ---- Show / hide ----

    show() {
      this.visible = true;
      this.panel.style.display = 'flex';
      if (this.onVisibilityChange) this.onVisibilityChange(true);
    }

    hide() {
      this.visible = false;
      this.panel.style.display = 'none';
      if (this.onVisibilityChange) this.onVisibilityChange(false);
    }

    toggle() {
      if (this.visible) this.hide(); else this.show();
    }

    // Forward LLM response to OpenClaw via WebSocket
    forwardToOpenClaw(text) {
      if (!this.remoteEnabled) return;
      const ws = window.OpenClaw && window.OpenClaw.ws;
      if (ws && ws.connected) {
        ws.sendMessage(text).catch(e => {
          this._chatLog('[error] Forward to OpenClaw failed: ' + e.message);
        });
        if (this.echoEnabled) {
          this._chatLog('[LLM->remote] ' + text.substring(0, 200) + (text.length > 200 ? '...' : ''));
        }
      }
    }
  }
