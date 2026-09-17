// ============================================================================
// PmPanel - Floating panel for private chat with PM (Host mode)
// ============================================================================
class PmPanel {
  constructor() {
    this.visible = false;
    this.panel = null;
    this.onSend = null; // callback(text) set by chat.js
    this.chatLog = null;
    this.chatInput = null;
    this.messages = []; // private chat history (not persisted)
    this._createPanel();
  }

  _createPanel() {
    this.panel = document.createElement('div');
    this.panel.id = 'pm-panel';
    this.panel.style.cssText =
      'position:fixed;right:20px;bottom:80px;width:380px;' +
      'background:#2d2d2d;border:1px solid #555;border-radius:8px;' +
      'display:none;flex-direction:column;z-index:10002;' +
      'font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",Arial,sans-serif;' +
      'box-shadow:0 4px 12px rgba(0,0,0,0.4);max-height:420px;';

    // Prevent events from bubbling out
    const stopEvents = ['input', 'change', 'paste', 'cut', 'keydown', 'keyup', 'keypress', 'beforeinput'];
    for (let i = 0; i < stopEvents.length; i++) {
      this.panel.addEventListener(stopEvents[i], function (e) { e.stopPropagation(); }, false);
    }

    // Header (draggable)
    const header = document.createElement('div');
    header.style.cssText =
      'padding:8px 12px;background:#1a1a1a;border-bottom:1px solid #444;' +
      'border-radius:8px 8px 0 0;display:flex;justify-content:space-between;' +
      'align-items:center;cursor:move;';

    const titleBox = document.createElement('div');
    titleBox.style.cssText = 'display:flex;align-items:center;gap:8px;';
    const title = document.createElement('span');
    title.textContent = 'PM Private Chat';
    title.style.cssText = 'color:#fff;font-weight:600;font-size:13px;';
    const hint = document.createElement('span');
    hint.textContent = '(guide the host)';
    hint.style.cssText = 'color:#888;font-size:11px;';
    titleBox.appendChild(title);
    titleBox.appendChild(hint);

    const self = this;
    const closeBtn = document.createElement('button');
    closeBtn.innerHTML = '&times;';
    closeBtn.style.cssText =
      'background:none;border:none;color:#999;font-size:18px;cursor:pointer;padding:0 4px;';
    closeBtn.onclick = function () { self.hide(); };

    header.appendChild(titleBox);
    header.appendChild(closeBtn);
    this.panel.appendChild(header);

    // Chat log area
    this.chatLog = document.createElement('div');
    this.chatLog.style.cssText =
      'flex:1;min-height:200px;max-height:300px;overflow-y:auto;' +
      'padding:8px 10px;background:#1e1e1e;border-bottom:1px solid #444;' +
      'font-size:12px;line-height:1.5;';
    this.panel.appendChild(this.chatLog);

    // Input row
    const inputRow = document.createElement('div');
    inputRow.style.cssText = 'display:flex;flex-direction:row;gap:6px;padding:8px 10px;align-items:stretch;';


    this.chatInput = document.createElement('textarea');
    this.chatInput.placeholder = 'Tell PM what to discuss...';
    this.chatInput.style.cssText =
      'flex:1;min-width:0;background:#1e1e1e;color:#d4d4d4;border:1px solid #444;' +

      'border-radius:4px;padding:5px 8px;font-size:12px;outline:none;' +
      'resize:none;height:50px;font-family:inherit;line-height:1.4;box-sizing:border-box;';
    this.chatInput.addEventListener('keydown', function (e) {
      if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); self._sendMessage(); }
    });

    const sendBtn = document.createElement('button');
    sendBtn.textContent = 'Send';
    sendBtn.style.cssText =
      'padding:4px 12px;background:#2196f3;color:white;border:none;' +
      'border-radius:4px;cursor:pointer;font-size:11px;white-space:nowrap;';
    sendBtn.onclick = function () { self._sendMessage(); };

    const clearBtn = document.createElement('button');
    clearBtn.textContent = 'Clear';
    clearBtn.style.cssText =
      'padding:4px 12px;background:#ff9800;color:white;border:none;' +
      'border-radius:4px;cursor:pointer;font-size:11px;white-space:nowrap;';
    clearBtn.onclick = function () { self.clearMessages(); };

    const btnRow = document.createElement('div');
    btnRow.style.cssText = 'display:flex;flex-direction:column;gap:4px;';

    btnRow.appendChild(sendBtn);
    btnRow.appendChild(clearBtn);

    inputRow.appendChild(this.chatInput);
    inputRow.appendChild(btnRow);
    this.panel.appendChild(inputRow);

    document.body.appendChild(this.panel);
    this._initDrag(header);
  }

  _sendMessage() {
    const text = this.chatInput.value.trim();
    if (!text) return;
    this.chatInput.value = '';
    this.addMessage('user', text);
    if (this.onSend) {
      this.onSend(text);
    }
  }

  // Add a message to the private chat
  addMessage(role, text) {
    this.messages.push({ role: role, content: text });
    this._appendMsgDom(role, text);
  }

  _appendMsgDom(role, text) {
    const div = document.createElement('div');
    div.style.cssText = 'margin-bottom:6px;';

    const label = document.createElement('span');
    label.style.cssText = 'font-weight:bold;font-size:11px;';
    if (role === 'user') {
      label.textContent = '[You] ';
      label.style.color = '#6cb6ff';
    } else {
      label.textContent = '[System] ';
      label.style.color = '#81c784';
    }

    const content = document.createElement('span');
    content.textContent = text;
    content.style.color = '#d4d4d4';

    div.appendChild(label);
    div.appendChild(content);
    this.chatLog.appendChild(div);
    this.chatLog.scrollTop = this.chatLog.scrollHeight;
  }

  // Get all private chat messages as a formatted string
  getMessagesText() {
    if (this.messages.length === 0) return '';
    const lines = [];
    for (let i = 0; i < this.messages.length; i++) {
      const m = this.messages[i];
      const prefix = m.role === 'user' ? '[User Instruction]' : '[System]';
      lines.push(prefix + ' ' + m.content);
    }
    return lines.join('\n');
  }

  clearMessages() {
    this.messages = [];
    this.chatLog.innerHTML = '';
  }

  // Drag support
  _initDrag(header) {
    const panel = this.panel;
    let dragging = false;
    let startX, startY, startLeft, startTop;

    header.addEventListener('mousedown', function (e) {
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

    document.addEventListener('mousemove', function (e) {
      if (!dragging) return;
      panel.style.left = (startLeft + e.clientX - startX) + 'px';
      panel.style.top = (startTop + e.clientY - startY) + 'px';
    });

    document.addEventListener('mouseup', function () { dragging = false; });
  }

  show() {
    this.visible = true;
    this.panel.style.display = 'flex';
  }

  hide() {
    this.visible = false;
    this.panel.style.display = 'none';
  }

  toggle() {
    if (this.visible) this.hide(); else this.show();
  }
}
