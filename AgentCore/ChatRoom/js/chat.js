// ============================================================================
// ChatRoom - Chat UI logic for AiClaw
// ============================================================================

function getStringInLength(str, length, getTheEnd) {
  const mode = (getTheEnd === true) ? 1 : (getTheEnd || 0);
  if (str.length <= length) return str;
  if (mode === 1) {
    return '...' + str.substring(str.length - length);
  } else if (mode === 2) {
    const half = Math.floor(length / 2);
    return str.substring(0, half) + ' ... ' + str.substring(str.length - half);
  }
  return str.substring(0, length) + '...';
}

class ChatRoom {
  constructor(relay) {
    this.relay = relay;
    this.wxBridge = new WxBridge();
    this._wxPendingReplies = {}; // {userId: {contextToken, userName, timestamp}}
    this._lastWxUserId = null; // last known wx user for fallback forwarding
    this.wxEnabled = localStorage.getItem('aiclaw_wx_enabled') === 'true';

    this.messages = [];
    this._sessionData = {}; // per-session message storage {sessionId: {messages, historyMessages, currentMessages, contextSummary}}
    this._lastSessionId = null; // track session switches
    this.userName = 'User';
    // Context management for AiClaw hub mode
    this.historyMessages = []; // completed rounds, {role, content, name} array
    this.currentMessages = []; // current round messages, {role, content, name} array
    this.systemPrompt = localStorage.getItem('aiclaw_system_prompt') || ''; // system prompt from Config

    this.soloTarget = null;    // solo mode target name
    this.MAX_CONTEXT_COUNT = 16; // max context message count
    this.MAX_CONTEXT_CHARS = 100000; // max history chars sent to LLM/PM (100KB)
    this.contextSummary = localStorage.getItem('aiclaw_context_summary') || ''; // accumulated summary from PM
    this._summarizing = false; // flag to prevent concurrent summarization
    this.pmClient = null; // LlmClient for PM (context summarizer)
    this._loadPmConfig();
    // Host mode state
    this.pmPanel = new PmPanel();
    const self = this;
    this.pmPanel.onSend = function (text) {
      if (!self.pmClient) {
        self.pmPanel.addMessage('system', '[PM not configured. Set PM in config.]');
        return;
      }
      // Build messages array from panel history for multi-turn context
      const msgs = [];
      for (let i = 0; i < self.pmPanel.messages.length; i++) {
        const m = self.pmPanel.messages[i];
        if (m.role === 'user' || m.role === 'assistant') {
          msgs.push({ role: m.role, content: m.content });
        }
      }
      self.pmClient.chat(msgs)
        .then(function (reply) {
          self.pmPanel.addMessage('assistant', reply);
        })
        .catch(function (err) {
          self.pmPanel.addMessage('system', '[PM error] ' + err.message);
        });
    };
    this._hostRoundCount = 0;     // replies received in current round
    this._hostExpectedCount = 0;  // expected replies for current round
    this._hostTimer = null;       // timeout timer for current round
    this._hostActive = false;     // whether a host round is in progress
    this._lastClawClients = [];   // cached clients from last status query
    this.HOST_TIMEOUT = 180000;   // 3 minutes timeout
    this._hostChannelId = null;   // channelId of current host round
    // DOM elements
    this.chatMessages = document.getElementById('chat-messages');
    this.chatInput = document.getElementById('chat-input');
    this.sendBtn = document.getElementById('send-btn');
    this.connectBtn = document.getElementById('connect-btn');
    this.disconnectBtn = document.getElementById('disconnect-btn');
    this._wsUrl = '';
    this._apiKey = '';
    this.statusDot = document.getElementById('status-dot');
    this.statusText = document.getElementById('status-text');
    this.modeBtn = document.getElementById('mode-btn');

    this.statusBtn = document.getElementById('status-btn');
    this.configBtn = document.getElementById('config-btn');
    this.configModal = document.getElementById('llm-config-modal');
    this.modalCloseBtn = document.getElementById('modal-close-btn');
    this.llmListEl = document.getElementById('llm-list');
    this.llmAddBtn = document.getElementById('llm-add-btn');
    // LLM manager
    this.llmManager = new LlmManager();
    this.muted = true;    // Message queue
    this._messageQueue = [];
    this._queueTimer = setInterval(this._processQueue.bind(this), 5000);

    // ContactManager for multi-session support
    console.log('[chat] creating ContactManager...');
    this.contactManager = new ContactManager();
    this._sidebarGroupsEl = document.getElementById('sidebar-groups');
    this._sidebarContactsEl = document.getElementById('sidebar-contacts');
    this._membersListEl = document.getElementById('members-list');
    this._announcementEl = document.getElementById('group-announcement');
    this._newGroupBtn = document.getElementById('new-group-btn');
    this._addMemberBtn = document.getElementById('add-member-btn');
    this._dissolveGroupBtn = document.getElementById('dissolve-group-btn');
    this._saveGroupBtn = document.getElementById('save-group-btn');
    this._bindEvents();
    this._loadConfig();

    this.contactManager._onChange = function () {
      // Step 3b: detect session switch and swap messages
      const newSessionId = self.contactManager.currentSessionId;
      if (self._lastSessionId !== null && newSessionId !== self._lastSessionId) {
        self._saveCurrentSession();
        self._loadSession(newSessionId);
        // Sync soloTarget with session type
        if (self.contactManager.isContactSession(newSessionId)) {
          const cid = self.contactManager.getContactIdFromSession(newSessionId);
          const ct = self.contactManager.contacts.get(cid);
          self.soloTarget = ct ? ct.name : null;
        } else {
          self.soloTarget = null;
        }
        self._updateSoloDisplay();

      } else if (self._lastSessionId === null && newSessionId) {
        self._lastSessionId = newSessionId;
      }
      self._renderSidebar();
      self._renderMembers();
    };
    this._initContacts();

    // Auto-connect to relay if config exists
    if (this._wsUrl && this._wsUrl.trim()) {
      this._connect();
    }

  }

  _bindEvents() {
    // Send button
    this.sendBtn.addEventListener('click', () => this._sendMessage());
    // Enter key
    this.chatInput.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        this._sendMessage();
      }
    });
    // Connect / Disconnect
    this.connectBtn.addEventListener('click', () => this._connect());
    this.disconnectBtn.addEventListener('click', () => this._disconnect());
    // Relay callbacks
    // Control buttons
    this.modeBtn.addEventListener('click', () => {
      this.muted = !this.muted;
      if (this.muted) {
        this.modeBtn.textContent = 'Mute';
        this.modeBtn.style.background = '#cc6600';
        this._addSystemMessage('Entered Mute mode');
        this._cancelHostRound();
      } else {
        this.modeBtn.textContent = 'Host';
        this.modeBtn.style.background = '#4caf50';
        this._addSystemMessage('Entered Host mode (PM will moderate)');
      }
    });

    this.statusBtn.addEventListener('click', () => this._sendControl('status'));
    // PM panel toggle
    this.pmBtn = document.getElementById('pm-btn');
    if (this.pmBtn) {
      this.pmBtn.addEventListener('click', () => {
        this.pmPanel.toggle();
      });
    }
    // Config modal
    this.configBtn.addEventListener('click', () => this._openConfigModal());
    this.modalCloseBtn.addEventListener('click', () => this._closeConfigModal());
    this.llmAddBtn.addEventListener('click', () => this._addLlmItem());
    // Queue clear button
    // Config export/import: inline dialog (file + password in one UI)
    const exportBtn = document.getElementById('config-export-btn');
    const importBtn = document.getElementById('config-import-btn');
    this._setupConfigDialog(exportBtn, importBtn);

    this._queueDisplayEl = document.getElementById('queue-display');
    const queueClearBtn = document.getElementById('queue-clear-btn');
    if (queueClearBtn) {
      queueClearBtn.addEventListener('click', () => this._clearQueue());
    }
    const clearChatBtn = document.getElementById('clear-chat-btn');
    if (clearChatBtn) {
      clearChatBtn.addEventListener('click', () => this._clearChat());
    }

    this.relay.onMessage((data) => this._onRelayMessage(data));
    this.relay.onStatus((status) => this._onRelayStatus(status));
    // WxBridge callbacks
    this.wxBridge.onMessage((data) => this._onWxMessage(data));
    this.wxBridge.onStatus((info) => this._onWxStatus(info));

    // Auto-connect wxBridge if enabled
    if (this.wxEnabled) {
      this.wxBridge.connect();
    }

    // Sidebar: new group button
    if (this._newGroupBtn) {
      this._newGroupBtn.addEventListener('click', () => {
        const name = prompt('Enter group name:');
        if (!name || !name.trim()) return;
        const initMembers = [];
        const curSid = this.contactManager.currentSessionId;
        if (this.contactManager.isContactSession(curSid)) {
          const cid = this.contactManager.getContactIdFromSession(curSid);
          if (cid) initMembers.push(cid);
        }
        this.contactManager.createGroup(name.trim(), initMembers);

      });
    }


    // Members panel: save group announcement
    if (this._saveGroupBtn) {
      this._saveGroupBtn.addEventListener('click', () => {
        const sid = this.contactManager.currentSessionId;
        if (!sid || !this.contactManager.isGroupSession(sid)) return;
        const text = this._announcementEl ? this._announcementEl.value : '';
        this.contactManager.setAnnouncement(sid, text);
        this._addSystemMessage('Group announcement saved.');
      });
    }

    // Members panel: dissolve group
    if (this._dissolveGroupBtn) {
      this._dissolveGroupBtn.addEventListener('click', () => {
        const sid = this.contactManager.currentSessionId;
        if (!sid || sid === 'all' || !this.contactManager.isGroupSession(sid)) return;
        if (!confirm('Dissolve this group? This cannot be undone.')) return;
        this.contactManager.dissolveGroup(sid);
      });
    }
  }

  _loadConfig() {
    try {
      const saved = localStorage.getItem('aiclaw_config');

      if (saved) {
        const cfg = JSON.parse(saved);
        if (cfg.wsUrl) this._wsUrl = cfg.wsUrl;
        if (cfg.apiKey) this._apiKey = cfg.apiKey;
      }
    } catch (_) { }
  }

  _saveConfig() {
    try {
      localStorage.setItem('aiclaw_config', JSON.stringify({
        wsUrl: this._wsUrl,
        apiKey: this._apiKey
      }));
    } catch (_) { }
  }

  _initContacts() {
    console.log('[chat] _initContacts called');
    // Sync LLM contacts from llmManager
    this.contactManager.syncFromLlmManager(this.llmManager);

    // Switch to default "All" group session
    this.contactManager.switchSession('all');
    // Initial render
    this._renderSidebar();
    this._renderMembers();
  }

  _renderSidebar() {
    const cm = this.contactManager;
    const self = this;
    // Render groups
    if (this._sidebarGroupsEl) {
      this._sidebarGroupsEl.innerHTML = '';
      const groups = cm.groups;
      groups.forEach(function (g, gid) {
        const div = document.createElement('div');
        div.className = 'sidebar-item sidebar-group';
        if (cm.currentSessionId === gid) {
          div.className += ' active';
        }
        const count = (g.id === 'all') ? cm.contacts.size : g.memberIds.size;
        div.textContent = g.name + ' (' + count + ')';
        div.dataset.sessionId = gid;
        div.addEventListener('click', function () {
          cm.switchSession(this.dataset.sessionId);
        });
        self._sidebarGroupsEl.appendChild(div);
      });
    }
    // Render contacts
    if (this._sidebarContactsEl) {
      this._sidebarContactsEl.innerHTML = '';
      const contacts = cm.contacts;
      contacts.forEach(function (c, cid) {
        const div = document.createElement('div');
        div.className = 'sidebar-item sidebar-contact';
        const sessionId = 'contact_' + cid;
        if (cm.currentSessionId === sessionId) {
          div.className += ' active';
        }
        let badge = '';
        if (c.type === 'LLM') badge = '[LLM] ';
        else if (c.type === 'CLAW') badge = '[Claw] ';
        else if (c.type === 'AICLAW') badge = '[AC] ';
        div.textContent = badge + c.name;
        div.dataset.sessionId = sessionId;
        div.addEventListener('click', function () {
          cm.switchSession(this.dataset.sessionId);
        });
        self._sidebarContactsEl.appendChild(div);
      });
    }
  }

  _renderMembers() {
    const cm = this.contactManager;
    const sessionId = cm.currentSessionId || 'all';
    const session = cm.getSession(sessionId);
    // Update announcement area
    if (this._announcementEl) {
      if (cm.isGroupSession(sessionId)) {
        this._announcementEl.parentElement.style.display = '';
        const grp = cm.getGroup(sessionId);
        this._announcementEl.value = (grp && grp.announcement) || '';
      } else {
        this._announcementEl.parentElement.style.display = 'none';
      }
    }
    if (this._membersListEl) {
      this._membersListEl.innerHTML = '';
      if (cm.isGroupSession(sessionId)) {
        const members = cm.getGroupMembers(sessionId);
        if (members) {
          // Group members list (with remove button)
          members.forEach(function (m) {
            const div = document.createElement('div');
            div.className = 'member-item';
            let badge = '';
            if (m.type === 'LLM') badge = '[LLM] ';
            else if (m.type === 'CLAW') badge = '[Claw] ';
            else if (m.type === 'AICLAW') badge = '[AC] ';
            const nameSpan = document.createElement('span');
            nameSpan.textContent = badge + m.name;
            div.appendChild(nameSpan);
            if (sessionId !== 'all') {
              const removeBtn = document.createElement('span');
              removeBtn.className = 'member-remove-btn';
              removeBtn.textContent = '\u00d7';
              removeBtn.title = 'Remove from group';
              removeBtn.dataset.contactId = m.id;
              removeBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                const cid = this.dataset.contactId;
                if (confirm('Remove ' + m.name + ' from group?')) {
                  cm.removeMemberFromGroup(sessionId, cid);
                }
              });
              div.appendChild(removeBtn);
            }
            this._membersListEl.appendChild(div);
          }.bind(this));
        }
        // Available contacts list (not in group, with add button)
        if (sessionId !== 'all') {
          const memberIds = new Set((members || []).map(function (m) { return m.id; }));
          const available = [];
          cm.contacts.forEach(function (c, cid) {
            if (!memberIds.has(cid)) available.push(c);
          });
          if (available.length > 0) {
            const divider = document.createElement('div');
            divider.className = 'member-divider';
            divider.textContent = '-- Available --';
            this._membersListEl.appendChild(divider);
            available.forEach(function (c) {
              const div = document.createElement('div');
              div.className = 'member-item member-available';
              const nameSpan = document.createElement('span');
              nameSpan.textContent = c.name;
              div.appendChild(nameSpan);
              const addBtn = document.createElement('span');
              addBtn.className = 'member-add-btn';
              addBtn.textContent = '+';
              addBtn.title = 'Add to group';
              addBtn.dataset.contactId = c.id;
              addBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                cm.addMemberToGroup(sessionId, this.dataset.contactId);
              });
              div.appendChild(addBtn);
              this._membersListEl.appendChild(div);
            }.bind(this));
          }
        }
      } else {
        const contactId = sessionId.replace('contact_', '');
        const contact = cm.contacts.get(contactId);
        if (contact) {
          const div = document.createElement('div');
          div.className = 'member-item';
          div.textContent = contact.name;
          this._membersListEl.appendChild(div);
        }
      }
    }
  }

  _connect() {
    const url = this._wsUrl.trim();
    const key = this._apiKey.trim();
    if (!url) {
      this._addSystemMessage('Please enter WebSocket URL');
      return;
    }
    this._saveConfig();
    this.relay.autoReconnect = true;
    this.relay.connect(url, key);
  }

  _disconnect() {
    this.relay.disconnect();
  }

  // Shared command handler, returns true if text was a command
  _handleCommand(text) {
    if (text === '/connect') {
      this._connect();
      return true;
    }
    if (text === '/disconnect') {
      this._disconnect();
      return true;
    }
    if (text === '/mute') {
      this.muted = true;
      this.modeBtn.textContent = 'Mute';
      this.modeBtn.style.background = '#cc6600';
      this._addSystemMessage('Entered Mute mode');
      this._cancelHostRound();
      return true;
    }
    if (text === '/host') {
      this.muted = false;
      this.modeBtn.textContent = 'Host';
      this.modeBtn.style.background = '#4caf50';
      this._addSystemMessage('Entered Host mode (PM will moderate)');
      return true;
    }
    if (text === '/clearq') {
      this._clearQueue();
      return true;
    }
    if (text === '/clear') {
      this._clearChat();
      return true;
    }
    if (text === '/auto on' || text === '/auto off') {
      const action = text === '/auto on' ? 'on' : 'off';
      const url = 'http://127.0.0.1:23003/v1/continue/' + action;
      const self = this;
      fetch(url, { method: 'POST' })
        .then(function (resp) {
          return resp.ok ? resp.text() : Promise.reject(new Error('HTTP ' + resp.status));
        })
        .then(function (body) {
          const msg = '[auto ' + action + '] ' + body;
          self._addSystemMessage(msg);
          self._forwardToWx('System', msg);
        })
        .catch(function (err) {
          const msg = '[auto ' + action + ' error] ' + err.message;
          self._addSystemMessage(msg);
          self._forwardToWx('System', msg);
        });
      return true;
    }
    if (text === '/status') {
      const mode = this.muted ? 'Mute' : 'Host';
      const solo = this.soloTarget ? this.soloTarget : 'OFF';
      const relay = (this.relay && this.relay.connected) ? 'Connected' : 'Disconnected';
      const wx = (this.wxEnabled && this.wxBridge && this.wxBridge.connected) ? 'Connected' : 'Disconnected';
      const llmTotal = this.llmManager ? this.llmManager.clients.length : 0;
      const llmEnabled = this.llmManager ? this.llmManager.clients.filter(function (c) { return c.enabled; }).length : 0;
      const queue = this._messageQueue.length;
      const host = this._hostActive ? 'Active' : 'Inactive';
      let statusText = 'Mode: ' + mode + ' | Solo: ' + solo + ' | Relay: ' + relay
        + ' | WeChat: ' + wx + ' | LLM: ' + llmEnabled + '/' + llmTotal
        + ' | Queue: ' + queue + ' | Host: ' + host;
      if (this._lastClawClients && this._lastClawClients.length > 0) {
        statusText += '\nRelay clients:';
        this._lastClawClients.forEach(function (c) {
          statusText += '\n  - ' + (c.name || c.id);
        });
      }
      this._addSystemMessage(statusText);
      this._forwardToWx('System', statusText);
      if (this.relay && this.relay.connected) {
        this._sendControl('status');
      }
      return true;
    }
    if (text.startsWith('/group ')) {
      const args = text.substring(7).trim().split(/\s+/);
      if (args.length === 0 || (args.length === 1 && !args[0])) {
        this._addSystemMessage('Usage: /group <name1> <name2> ...');
        return true;
      }
      const groupName = 'TmpGrp_' + Math.random().toString(36).substr(2, 6);
      const grp = this.contactManager.createGroup(groupName, []);
      const added = [];
      const notFound = [];
      for (let i = 0; i < args.length; i++) {
        const cid = this.contactManager.findContactIdByName(args[i]);
        if (cid) {
          this.contactManager.addMemberToGroup(grp.id, cid);
          added.push(args[i]);
        } else {
          notFound.push(args[i]);
        }
      }
      let msg = 'Created group "' + groupName + '" with: ' + added.join(', ');
      if (notFound.length > 0) msg += ' (not found: ' + notFound.join(', ') + ')';
      this._addSystemMessage(msg);
      this.contactManager.switchSession(grp.id);
      this._renderSidebar();
      this._renderMembers();
      return true;
    }

    if (text === '/dismiss') {
      const groups = this.contactManager.getAllGroups();
      const dismissed = [];
      for (let i = 0; i < groups.length; i++) {
        if (groups[i].name.startsWith('TmpGrp_')) {
          dismissed.push(groups[i].id);
        }
      }
      for (let i = 0; i < dismissed.length; i++) {
        this.contactManager.dissolveGroup(dismissed[i]);
      }
      if (dismissed.length > 0) {
        this.contactManager.switchSession('all');
        this._renderSidebar();
        this._renderMembers();
      }
      this._addSystemMessage('Dismissed ' + dismissed.length + ' temp group(s).');
      return true;
    }

    if (text.startsWith('/chat ')) {
      const target = text.substring(6).trim();
      if (target === 'all' || target === '') {
        this.contactManager.switchSession('all');
      } else {
        const cid = this.contactManager.findContactIdByName(target);
        if (cid) {
          this.contactManager.switchSession('contact_' + cid);
        } else {
          let foundGid = null;
          this.contactManager.groups.forEach(function (g, gid) {
            if (g.name === target) { foundGid = gid; }
          });
          if (foundGid) {
            this.contactManager.switchSession(foundGid);
          } else {
            this._addSystemMessage('Not found: ' + target);
            this._forwardToWx('System', 'Not found: ' + target);
            return true;
          }
        }
      }
      this._renderSidebar();
      this._renderMembers();
      this._addSystemMessage('Switched to: ' + target);
      this._forwardToWx('System', 'Switched to: ' + target);
      return true;
    }

    if (text.startsWith('/solo ')) {
      const target = text.substring(6).trim();
      if (target === 'off' || target === '') {
        this.soloTarget = null;
        this._addSystemMessage('Solo mode OFF');
        this._forwardToWx('System', 'Solo mode OFF');
        this._updateSoloDisplay();
      } else if (target) {
        this.soloTarget = target;
        this._addSystemMessage('Solo mode ON: ' + target);
        this._forwardToWx('System', 'Solo mode ON: ' + target);
        const cid = this.contactManager.findContactIdByName(target);
        if (cid) {
          this.contactManager.switchSession('contact_' + cid);
          this._renderSidebar();
          this._renderMembers();
        }
        this._updateSoloDisplay();
      } else {
        this._addSystemMessage('Usage: /solo <name> or /solo off');
        this._forwardToWx('System', 'Usage: /solo <name> or /solo off');
      }
      return true;
    }

    if (text === '/stop') {
      if (this.relay && this.relay.connected) {
        let receiver = this.soloTarget ? this.soloTarget : '*';
        if (receiver === '*' && this.contactManager) {
          const sid = this.contactManager.currentSessionId;
          if (sid !== 'all' && this.contactManager.isGroupSession(sid)) {
            const members = this.contactManager.getGroupMembers(sid);
            const names = members.map(function (m) { return m.name; });
            if (names.length === 0) {
              this._addSystemMessage('No members in current group');
              return true;
            }
            receiver = names;
          }
        }
        // Forward raw command without context
        this.relay.sendMessage('/stop', null, receiver, '', this.contactManager.currentSessionId);
        const targetDesc = Array.isArray(receiver) ? receiver.join(',') : receiver;
        this._addSystemMessage('Stop command forwarded to ' + targetDesc);
        this._forwardToWx('System', 'Stop command forwarded to ' + targetDesc);
      } else {
        this._addSystemMessage('Not connected to relay');
      }
      return true;
    }

    if (text === '/clrhist' || text === '/clearhistory') {
      if (this.relay && this.relay.connected) {
        let receiver = this.soloTarget ? this.soloTarget : '*';
        if (receiver === '*' && this.contactManager) {
          const sid = this.contactManager.currentSessionId;
          if (sid !== 'all' && this.contactManager.isGroupSession(sid)) {
            const members = this.contactManager.getGroupMembers(sid);
            const names = members.map(function (m) { return m.name; });
            if (names.length === 0) {
              this._addSystemMessage('No members in current group');
              return true;
            }
            receiver = names;
          }
        }
        // Forward raw command without context
        this.relay.sendMessage(text, null, receiver, '', this.contactManager.currentSessionId);
        const targetDesc = Array.isArray(receiver) ? receiver.join(',') : receiver;
        this._addSystemMessage('Clear history command forwarded to ' + targetDesc);
        this._forwardToWx('System', 'Clear history command forwarded to ' + targetDesc);
      } else {
        this._addSystemMessage('Not connected to relay');
      }
      return true;
    }
    if (text === '/wx_login') {
      if (this.wxEnabled && this.wxBridge && this.wxBridge.connected) {
        this.wxBridge.requestLogin();
        this._addSystemMessage('WeChat login requested, QR code will be shown when received');
      } else {
        this._addSystemMessage('WeChat bridge not connected or disabled');
      }
      return true;
    }
    if (text === '/wx_logout') {
      if (this.wxEnabled && this.wxBridge && this.wxBridge.connected) {
        this.wxBridge.requestLogout();
        this._addSystemMessage('WeChat logout requested');
      } else {
        this._addSystemMessage('WeChat bridge not connected or disabled');
      }
      return true;
    }
    return false;

  }

  _sendMessage() {
    const text = this.chatInput.value.trim();
    if (!text) return;
    this.chatInput.value = '';

    // Handle commands (shared with WeChat messages)
    if (this._handleCommand(text)) return;

    // Show locally
    this._addMessage(this.userName, text, 'user');
    // Add to context
    this._addToContext('user', text, this.userName);

    // Enqueue user message forwarding (display already done above)
    this._enqueueMessage(this.userName, text, 'user', { userForward: true, isAiClawMessage: true, channelId: this.contactManager.currentSessionId });
  }

  _sendControl(action) {
    if (!this.relay.ws || this.relay.ws.readyState !== 1) {
      this._addSystemMessage('Not connected to relay');
      return;
    }
    this.relay.ws.send(JSON.stringify({ type: 'control', action: action }));
  }

  _onRelayMessage(data) {
    if (data.type === 'message') {
      console.log('[chat] received message data:', JSON.stringify(data));
      // In AiClaw relay mode, sender comes as a separate field from server.js
      const sender = data.sender || 'Unknown';
      let content = data.content || data.text || JSON.stringify(data);

      // Enqueue relay message for delayed processing (mark fromRelay to avoid echo back)
      let relayChannelId = data.channelId;
      if (relayChannelId && this.contactManager.isGroupSession(relayChannelId)) {
        // group channel: keep as-is
      } else {
        // private or missing: always route to sender's private channel
        relayChannelId = 'contact_claw_' + sender;
      }
      console.log('[chat] relay message channelId: ' + relayChannelId + ' for sender=' + sender);
      // MetaDSL detection: extract and execute code, use clean text for display
      content = this._handleMetaDSL(content, sender, relayChannelId);

      this._enqueueMessage(sender, content, 'claw', { forward: true, hostReply: true, fromRelay: true, channelId: relayChannelId });
    } else if (data.type === 'control_status') {
      let info = 'Relay status:';
      info += ' connections: ' + data.totalConnections;
      // Cache non-AiClaw clients for Host mode round counting
      if (data.clients) {
        this._lastClawClients = data.clients.filter(function (c) {
          return c.name !== 'AiClaw';
        });
        this.contactManager.syncFromRelayClaw(this._lastClawClients);
      }
      if (data.clients && data.clients.length > 0) {
        data.clients.forEach(function (c) {
          info += '\n  - ' + (c.name || c.id) + ' (key: ' + c.apiKey + ')';
        });
      }
      this._addSystemMessage(info);
      this._forwardToWx('System', info);
    } else {
      this._addSystemMessage('[' + (data.type || 'unknown') + '] ' + JSON.stringify(data));
    }
  }

  _onRelayStatus(status) {
    const colors = {
      'connecting': '#ff9800',
      'connected': '#4caf50',
      'disconnected': '#666',
      'error': '#f44336',
      'auth_ok': '#2196f3'
    };
    this.statusDot.style.background = colors[status] || '#666';
    this.statusText.textContent = status;
    if (status === 'auth_ok') {
      if (this.relay.clientName) {
        this.userName = this.relay.clientName;
      }
      this._addSystemMessage('Authenticated with relay server');
      this._sendControl('status');
    } else if (status === 'connected') {
      this._addSystemMessage('Connected to relay server');
    } else if (status === 'disconnected') {
      this._addSystemMessage('Disconnected from relay server');
    } else if (status === 'error') {
      this._addSystemMessage('Connection error');
    }
  }

  // WeChat bridge message handler
  _onWxMessage(data) {
    if (!this.wxEnabled) return;
    const userId = data.from_user || 'unknown';
    const userName = data.from_name || ('WX_' + userId.substring(0, 8));
    const text = data.text || '';
    if (!text) return;

    // Record pending reply info
    this._wxPendingReplies[userId] = {
      contextToken: data.context_token || '',
      userName: userName,
      timestamp: Date.now(),
      sentCount: 0
    };

    // Handle commands from WeChat
    if (this._handleCommand(text)) return;

    this._lastWxUserId = userId;
    this._lastWxContextToken = data.context_token || '';
    if (this.muted) {
      // Mute mode: display message first (enqueue skips display for userForward)
      this._addMessage(userName, text, 'user');
      this._addToContext('user', text, userName);
      // Then enqueue for forwarding to LLM/relay
      this._enqueueMessage(userName, text, 'user', {
        userForward: true,
        isAiClawMessage: true,
        fromWx: true,
        wxUserId: userId
      });
    } else {
      // Host mode: treat like PM panel input
      this.pmPanel.addMessage('user', text);
      this._forwardToWx('PM', '[WX user sent] ' + text);
      this.pmPanel.onSend(text);
    }
  }

  // WeChat bridge status handler
  _onWxStatus(info) {
    if (!info) return;
    const type = info.type;
    const data = info.data || {};

    if (type === 'connection') {
      const status = data.status || 'unknown';
      console.log('[ChatRoom] WxBridge connection:', status);
      this._addSystemMessage('WeChat bridge: ' + status);
    } else if (type === 'qrcode') {
      console.log('[ChatRoom] WxBridge QR code received');
      this._addSystemMessage('WeChat QR code ready - scan to login');
      if (data.qrcode_img) {
        let imgSrc = data.qrcode_img;
        if (imgSrc.indexOf('data:') !== 0) {
          imgSrc = 'data:image/png;base64,' + imgSrc;
        }
        const qrImg = document.createElement('img');
        qrImg.src = imgSrc;
        qrImg.alt = 'WeChat QR code';
        qrImg.style.maxWidth = '240px';
        qrImg.style.display = 'block';
        qrImg.style.margin = '8px 0';
        this.chatMessages.appendChild(qrImg);
        this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
      }
    } else if (type === 'status') {

      const wxStatus = data.status || data;
      console.log('[ChatRoom] WxBridge status:', wxStatus);
      this._addSystemMessage('WeChat: ' + (typeof wxStatus === 'string' ? wxStatus : JSON.stringify(wxStatus)));
    } else if (type === 'sendresult') {
      console.log('[ChatRoom] WxBridge send result:', data);
    }
  }

  // Forward a reply to WeChat users who have pending replies
  _forwardToWx(senderName, text) {
    if (!this.wxEnabled || !this.wxBridge.connected) return;
    // Split long text into chunks of up to 4000 characters each and send them
    // sequentially with a 1-second interval between each message.
    const maxLen = 4000;
    const chunks = [];
    for (let i = 0; i < text.length; i += maxLen) {
      chunks.push(text.substring(i, i + maxLen));
    }
    const now = Date.now();
    const maxAge = 5 * 60 * 1000; // 5 minutes timeout
    let hasPending = false;
    const maxReplies = 10; // WeChat allows max 10 replies per incoming message
    const pendingUsers = Object.keys(this._wxPendingReplies);
    const self = this;
    // Shared send queue: each chunk is scheduled after the previous one so
    // that across calls/users messages are sent strictly in order with a
    // 1-second gap, never concurrently.
    if (!this._wxNextSendTime) this._wxNextSendTime = 0;
    for (let i = 0; i < pendingUsers.length; i++) {
      const userId = pendingUsers[i];
      const info = this._wxPendingReplies[userId];
      if (now - info.timestamp > maxAge) {
        delete this._wxPendingReplies[userId];
        continue;
      }
      if (info.sentCount >= maxReplies) {
        continue;
      }
      hasPending = true;
      chunks.forEach(function (chunk) {
        const delay = Math.max(0, self._wxNextSendTime - Date.now());
        setTimeout(function () {
          if (!self.wxEnabled || !self.wxBridge.connected) return;
          self.wxBridge.sendReply(userId, '[' + senderName + '] ' + chunk, info.contextToken);
        }, delay);
        self._wxNextSendTime = Date.now() + delay + 1000;
      });
      info.sentCount += chunks.length;
    }
    // If no pending users but wx is connected, send to last known user
    if (!hasPending && this._lastWxUserId && this._lastWxContextToken) {
      const lastUserId = this._lastWxUserId;
      const lastToken = this._lastWxContextToken;
      chunks.forEach(function (chunk) {
        const delay = Math.max(0, self._wxNextSendTime - Date.now());
        setTimeout(function () {
          if (!self.wxEnabled || !self.wxBridge.connected) return;
          self.wxBridge.sendReply(lastUserId, '[' + senderName + '] ' + chunk, lastToken);
        }, delay);
        self._wxNextSendTime = Date.now() + delay + 1000;
      });
    }
  }

  _renderMessageBody(bodyEl, text) {
    // Safe markdown rendering with fallback to textContent
    if (typeof marked === 'undefined' || typeof DOMPurify === 'undefined') {
      bodyEl.textContent = text;
      return;
    }
    try {
      const rawHtml = marked.parse(text, { breaks: true, gfm: true });
      const cleanHtml = DOMPurify.sanitize(rawHtml, {
        USE_PROFILES: { html: true, svg: true, svgFilters: true },
        ADD_TAGS: ['foreignObject'],
        ADD_ATTR: ['class', 'target', 'rel']
      });
      bodyEl.innerHTML = cleanHtml;
      // Open external links in new tab for safety
      const links = bodyEl.querySelectorAll('a[href]');
      for (let i = 0; i < links.length; i++) {
        links[i].setAttribute('target', '_blank');
        links[i].setAttribute('rel', 'noopener noreferrer');
      }
      // Inline SVG code blocks: render raw SVG
      const svgBlocks = bodyEl.querySelectorAll('pre > code.language-svg');
      for (let s = 0; s < svgBlocks.length; s++) {
        const svgWrap = document.createElement('div');
        svgWrap.className = 'chat-svg-block';
        svgWrap.innerHTML = DOMPurify.sanitize(svgBlocks[s].textContent, {
          USE_PROFILES: { html: true, svg: true, svgFilters: true }
        });
        svgBlocks[s].parentNode.replaceWith(svgWrap);
      }
      // Mermaid code blocks: render diagram
      const mmBlocks = bodyEl.querySelectorAll('pre > code.language-mermaid');
      if (mmBlocks.length > 0 && typeof mermaid !== 'undefined') {
        if (!window._chatMermaidInited) {
          try { mermaid.initialize({ startOnLoad: false, theme: 'default', securityLevel: 'loose' }); } catch (e0) { }
          window._chatMermaidInited = true;
        }
        for (let m = 0; m < mmBlocks.length; m++) {
          const mmDiv = document.createElement('div');
          mmDiv.className = 'mermaid';
          mmDiv.textContent = mmBlocks[m].textContent;
          mmBlocks[m].parentNode.replaceWith(mmDiv);
        }
        try {
          mermaid.run({ nodes: bodyEl.querySelectorAll('.mermaid') });
        } catch (e1) {
          console.warn('[Chat] Mermaid render failed:', e1);
        }
      }
    } catch (e) {
      console.warn('[Chat] Markdown render failed, fallback to textContent:', e);
      bodyEl.textContent = text;
    }
  }

  _addMessage(sender, text, type) {

    const msg = { sender: sender, text: text, type: type, time: new Date() };

    this.messages.push(msg);
    const el = document.createElement('div');
    el.className = 'chat-msg chat-msg-' + type;
    const header = document.createElement('div');
    header.className = 'chat-msg-header';
    const nameSpan = document.createElement('span');
    nameSpan.className = 'chat-msg-sender';
    nameSpan.textContent = sender;
    const timeSpan = document.createElement('span');
    timeSpan.className = 'chat-msg-time';
    timeSpan.textContent = msg.time.toLocaleTimeString();
    header.appendChild(nameSpan);
    header.appendChild(timeSpan);
    const body = document.createElement('div');
    body.className = 'chat-msg-body';
    this._renderMessageBody(body, text);
    el.appendChild(header);
    el.appendChild(body);
    this.chatMessages.appendChild(el);
    this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
  }

  _addSystemMessage(text) {
    const el = document.createElement('div');
    el.className = 'chat-msg chat-msg-system';
    el.textContent = '[' + new Date().toLocaleTimeString() + '] ' + text;
    this.chatMessages.appendChild(el);
    this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
  }
  // ---- LLM Config Modal ----

  _openConfigModal() {
    // Fill prompt textarea with current value
    const promptEl = document.getElementById('system-prompt');
    if (promptEl) promptEl.value = this.systemPrompt || '';
    // Fill PM config fields
    const pmBaseUrlEl = document.getElementById('pm-base-url');
    const pmApiKeyEl = document.getElementById('pm-api-key');
    const pmModelEl = document.getElementById('pm-model');
    if (pmBaseUrlEl) pmBaseUrlEl.value = localStorage.getItem('aiclaw_pm_base_url') || '';
    if (pmApiKeyEl) pmApiKeyEl.value = localStorage.getItem('aiclaw_pm_api_key') || '';
    if (pmModelEl) pmModelEl.value = localStorage.getItem('aiclaw_pm_model') || '';
    // Fill WeChat Bridge checkbox
    const wxEnabledEl = document.getElementById('wx-enabled');
    if (wxEnabledEl) wxEnabledEl.checked = this.wxEnabled || false;
    // Fill Relay config fields
    const relayUrlEl = document.getElementById('relay-ws-url');
    const relayKeyEl = document.getElementById('relay-api-key');
    if (relayUrlEl) relayUrlEl.value = this._wsUrl || '';
    if (relayKeyEl) relayKeyEl.value = this._apiKey || '';

    this._renderLlmList();
    this.configModal.style.display = 'flex';
  }


  _closeConfigModal() {
    this.configModal.style.display = 'none';
    // Save system prompt
    const promptEl = document.getElementById('system-prompt');
    if (promptEl) {
      this.systemPrompt = promptEl.value.trim();
      localStorage.setItem('aiclaw_system_prompt', this.systemPrompt);
    }
    // Save PM config
    const pmBaseUrlEl = document.getElementById('pm-base-url');
    const pmApiKeyEl = document.getElementById('pm-api-key');
    const pmModelEl = document.getElementById('pm-model');
    if (pmBaseUrlEl) {
      localStorage.setItem('aiclaw_pm_base_url', pmBaseUrlEl.value.trim());
    }
    if (pmApiKeyEl) {
      localStorage.setItem('aiclaw_pm_api_key', pmApiKeyEl.value.trim());
    }
    if (pmModelEl) {
      localStorage.setItem('aiclaw_pm_model', pmModelEl.value.trim());
    }
    this._loadPmConfig(); // Reload PM client with new config
    // Save any field changes
    // Save WeChat Bridge config
    // Save Relay config
    const relayUrlEl = document.getElementById('relay-ws-url');
    const relayKeyEl = document.getElementById('relay-api-key');
    if (relayUrlEl) {
      this._wsUrl = relayUrlEl.value.trim();
    }
    if (relayKeyEl) {
      this._apiKey = relayKeyEl.value.trim();
    }
    this._saveConfig();
    const wxEnabledEl = document.getElementById('wx-enabled');
    if (wxEnabledEl) {
      const wasEnabled = this.wxEnabled;
      this.wxEnabled = wxEnabledEl.checked;
      localStorage.setItem('aiclaw_wx_enabled', this.wxEnabled ? 'true' : 'false');
      if (this.wxEnabled && !wasEnabled) {
        this.wxBridge.connect();
      } else if (!this.wxEnabled && wasEnabled) {
        this.wxBridge.disconnect();
      }
    }


    this.llmManager.clients.forEach((client, i) => {
      const card = this.llmListEl.children[i];
      if (!card) return;
      const getVal = function (label) {
        const row = card.querySelector('[data-field="' + label + '"]');
        if (!row) return null;
        const el = row.querySelector('input, select');
        if (!el) return null;
        return el.type === 'checkbox' ? el.checked : el.value;
      };
      client.updateConfig({
        name: getVal('Name'),
        type: getVal('Type') || 'openai',
        baseUrl: getVal('Base URL'),
        apiKey: getVal('API Key'),
        anydevHost: getVal('Anydev Host'),
        model: getVal('Model') || null,
        stateless: getVal('Stateless') === true
      });
    });
    this.llmManager._saveConfig();
  }


  // Setup inline config dialog for export/import
  _setupConfigDialog(exportBtn, importBtn) {
    const self = this;
    let _dlgCallback = null;
    let _dlgMode = null;
    // Overlay
    const dlgOverlay = document.createElement('div');
    dlgOverlay.style.cssText = 'display:none; position:fixed; top:0; left:0; width:100%; height:100%;' +
      'background:rgba(0,0,0,0.5); z-index:999999; align-items:center; justify-content:center;';
    // Dialog box
    const dlgBox = document.createElement('div');
    dlgBox.style.cssText = 'background:#333; padding:16px; border-radius:8px; min-width:280px; text-align:center;';
    const dlgTitle = document.createElement('div');
    dlgTitle.style.cssText = 'color:#eee; margin-bottom:10px; font-size:14px; font-weight:bold;';
    // File chooser row (import only)
    const fileRow = document.createElement('div');
    fileRow.style.cssText = 'margin-bottom:10px; display:none;';
    const fileChooseBtn = document.createElement('button');
    fileChooseBtn.textContent = 'Choose File...';
    fileChooseBtn.style.cssText = 'padding:4px 12px; background:#555; color:white; border:none;' +
      'border-radius:4px; cursor:pointer; font-size:12px;';
    const fileNameLabel = document.createElement('span');
    fileNameLabel.style.cssText = 'color:#aaa; margin-left:8px; font-size:12px;';
    fileNameLabel.textContent = 'No file selected';
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.accept = '.enc';
    fileInput.style.display = 'none';
    fileChooseBtn.onclick = function () { fileInput.value = ''; fileInput.click(); };
    fileInput.addEventListener('change', function () {
      if (fileInput.files[0]) {
        fileNameLabel.textContent = fileInput.files[0].name;
        fileNameLabel.style.color = '#8f8';
      } else {
        fileNameLabel.textContent = 'No file selected';
        fileNameLabel.style.color = '#aaa';
      }
    });
    fileRow.appendChild(fileChooseBtn);
    fileRow.appendChild(fileNameLabel);
    fileRow.appendChild(fileInput);
    // Password row
    const pwdLabel = document.createElement('div');
    pwdLabel.style.cssText = 'color:#ccc; margin-bottom:4px; font-size:12px; text-align:left;';
    const pwdInput = document.createElement('input');
    pwdInput.type = 'password';
    pwdInput.style.cssText = 'width:90%; padding:5px; margin-bottom:10px; border:1px solid #666;' +
      'border-radius:4px; background:#222; color:#eee; font-size:13px;';
    // Button bar
    const dlgBtnBar = document.createElement('div');
    dlgBtnBar.style.cssText = 'display:flex; gap:8px; justify-content:center;';
    const dlgOk = document.createElement('button');
    dlgOk.textContent = 'OK';
    dlgOk.style.cssText = 'padding:4px 16px; background:#00796b; color:white; border:none;' +
      'border-radius:4px; cursor:pointer; font-size:12px;';
    const dlgCancel = document.createElement('button');
    dlgCancel.textContent = 'Cancel';
    dlgCancel.style.cssText = 'padding:4px 16px; background:#666; color:white; border:none;' +
      'border-radius:4px; cursor:pointer; font-size:12px;';
    dlgBtnBar.appendChild(dlgOk);
    dlgBtnBar.appendChild(dlgCancel);
    // Assemble dialog
    dlgBox.appendChild(dlgTitle);
    dlgBox.appendChild(fileRow);
    dlgBox.appendChild(pwdLabel);
    dlgBox.appendChild(pwdInput);
    dlgBox.appendChild(dlgBtnBar);
    dlgOverlay.appendChild(dlgBox);
    document.body.appendChild(dlgOverlay);
    const showDialog = function (mode, callback) {
      _dlgMode = mode;
      _dlgCallback = callback;
      pwdInput.value = '';
      fileInput.value = '';
      fileNameLabel.textContent = 'No file selected';
      fileNameLabel.style.color = '#aaa';
      if (mode === 'import') {
        dlgTitle.textContent = 'Import Config';
        pwdLabel.textContent = 'Decryption password:';
        fileRow.style.display = 'block';
      } else {
        dlgTitle.textContent = 'Export Config';
        pwdLabel.textContent = 'Encryption password:';
        fileRow.style.display = 'none';
      }
      dlgOverlay.style.display = 'flex';
      pwdInput.focus();
    };
    const hideDialog = function () {
      dlgOverlay.style.display = 'none';
      _dlgCallback = null;
      _dlgMode = null;
    };
    dlgOk.onclick = function () {
      const pwd = pwdInput.value;
      if (!pwd) { pwdInput.focus(); return; }
      if (_dlgMode === 'import' && (!fileInput.files || !fileInput.files[0])) {
        fileChooseBtn.focus();
        return;
      }
      const cb = _dlgCallback;
      const file = _dlgMode === 'import' ? fileInput.files[0] : null;
      hideDialog();
      if (cb) cb(pwd, file);
    };
    dlgCancel.onclick = function () { hideDialog(); };
    pwdInput.addEventListener('keydown', function (e) {
      if (e.key === 'Enter') dlgOk.click();
      if (e.key === 'Escape') hideDialog();
    });
    if (exportBtn) {
      exportBtn.addEventListener('click', function () {
        console.log('[Export] Export button clicked');
        showDialog('export', function (pwd) {
          console.log('[Export] Password confirmed');
          self._exportConfig(pwd);
        });
      });
    }
    if (importBtn) {
      importBtn.addEventListener('click', function () {
        console.log('[Import] Import button clicked');
        showDialog('import', function (pwd, file) {
          console.log('[Import] Confirmed, file:', file.name, 'size:', file.size);
          self._importConfig(file, pwd);
        });
      });
    }
  }

  // Derive AES-GCM key from password using PBKDF2
  async _deriveKey(password, salt) {
    const enc = new TextEncoder();
    const keyMaterial = await crypto.subtle.importKey(
      'raw', enc.encode(password), 'PBKDF2', false, ['deriveKey']
    );
    return crypto.subtle.deriveKey(
      { name: 'PBKDF2', salt: salt, iterations: 100000, hash: 'SHA-256' },
      keyMaterial,
      { name: 'AES-GCM', length: 256 },
      false,
      ['encrypt', 'decrypt']
    );
  }

  // Export config: collect localStorage keys + SecretStore apiKeys, encrypt, download
  async _exportConfig(password) {
    if (!password) return;
    const keys = [
      'aiclaw_config', 'aiclaw_system_prompt', 'aiclaw_context_summary',
      'aiclaw_pm_base_url', 'aiclaw_pm_api_key', 'aiclaw_pm_model',
      'aiclaw_llm_configs',
      'aiclaw_groups', 'aiclaw_sessions',
      'aiclaw_wx_enabled'
    ];
    const data = {};
    keys.forEach(function (k) {
      const v = localStorage.getItem(k);
      if (v !== null) data[k] = v;
    });
    // Also export apiKeys from SecretStore (stored in IndexedDB, not localStorage)
    try {
      await this.llmManager.secretStore.ready();
      for (let i = 0; i < this.llmManager.clients.length; i++) {
        const apiKey = await this.llmManager.secretStore.getItem('llm_apikey_' + i);
        if (apiKey) {
          data['__secret__llm_apikey_' + i] = apiKey;
        }
      }
    } catch (e) {
      console.log('[Export] Warning: failed to read SecretStore apiKeys:', e.message);
    }
    const enc = new TextEncoder();
    const plaintext = enc.encode(JSON.stringify(data));
    const salt = crypto.getRandomValues(new Uint8Array(16));
    const iv = crypto.getRandomValues(new Uint8Array(12));
    try {
      const key = await this._deriveKey(password, salt);
      const ciphertext = await crypto.subtle.encrypt(
        { name: 'AES-GCM', iv: iv }, key, plaintext
      );
      // Pack: salt(16) + iv(12) + ciphertext
      const buf = new Uint8Array(salt.length + iv.length + ciphertext.byteLength);
      buf.set(salt, 0);
      buf.set(iv, salt.length);
      buf.set(new Uint8Array(ciphertext), salt.length + iv.length);
      const blob = new Blob([buf], { type: 'application/octet-stream' });
      const a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = 'aiclaw_config_backup.enc';
      a.click();
      URL.revokeObjectURL(a.href);
    } catch (e) {
      alert('Export failed: ' + e.message);
    }
  }

  // Import config: read .enc file, decrypt, write to localStorage + SecretStore, reload
  async _importConfig(file, password) {
    if (!file || !password) return;
    console.log('[Import] _importConfig called, file:', file.name, 'size:', file.size);
    try {
      const arrayBuf = await file.arrayBuffer();
      const buf = new Uint8Array(arrayBuf);
      const salt = buf.slice(0, 16);
      const iv = buf.slice(16, 28);
      const ciphertext = buf.slice(28);
      const key = await this._deriveKey(password, salt);
      const plainBuf = await crypto.subtle.decrypt(
        { name: 'AES-GCM', iv: iv }, key, ciphertext
      );
      const dec = new TextDecoder();
      const data = JSON.parse(dec.decode(plainBuf));
      // Separate localStorage keys and SecretStore keys
      const secretKeys = {};
      Object.keys(data).forEach(function (k) {
        if (k.indexOf('__secret__') === 0) {
          secretKeys[k] = data[k];
        } else {
          localStorage.setItem(k, data[k]);
        }
      });
      // Write apiKeys to SecretStore
      try {
        await this.llmManager.secretStore.ready();
        const secretKeyNames = Object.keys(secretKeys);
        for (let i = 0; i < secretKeyNames.length; i++) {
          const sk = secretKeyNames[i];
          const realKey = sk.replace('__secret__', '');
          await this.llmManager.secretStore.setItem(realKey, secretKeys[sk]);
          console.log('[Import] Restored secret:', realKey);
        }
      } catch (e) {
        console.log('[Import] Warning: failed to write SecretStore apiKeys:', e.message);
      }
      alert('Config imported successfully. Page will reload.');
      location.reload();
    } catch (e) {
      alert('Import failed: wrong password or corrupted file.');
    }
  }

  _renderLlmList() {
    this.llmListEl.innerHTML = '';
    const self = this;
    this.llmManager.clients.forEach(function (client, index) {
      const card = document.createElement('div');
      card.className = 'llm-item';
      const header = document.createElement('div');
      header.className = 'llm-item-header';
      const expandIcon = document.createElement('span');
      expandIcon.className = 'llm-expand-icon';
      expandIcon.textContent = '\u25B6';
      const title = document.createElement('span');
      title.className = 'llm-item-title';
      title.textContent = client.name || 'LLM #' + (index + 1);
      const actions = document.createElement('div');
      actions.className = 'llm-item-actions';
      const toggleBtn = document.createElement('button');
      toggleBtn.className = 'llm-toggle-btn' + (client.enabled ? '' : ' disabled');
      toggleBtn.textContent = client.enabled ? 'ON' : 'OFF';
      toggleBtn.addEventListener('click', function (event) {
        event.stopPropagation();

        client.enabled = !client.enabled;
        self.llmManager._saveConfig();
        self._renderLlmList();
      });
      const deleteBtn = document.createElement('button');
      deleteBtn.className = 'llm-delete-btn';
      deleteBtn.textContent = 'Del';
      deleteBtn.addEventListener('click', function (event) {
        event.stopPropagation();

        self.llmManager.remove(index);
        self._renderLlmList();
      });
      actions.appendChild(toggleBtn);
      actions.appendChild(deleteBtn);
      header.appendChild(expandIcon);
      header.appendChild(title);
      header.appendChild(actions);
      header.addEventListener('click', function () {
        const isExpanded = fields.classList.toggle('expanded');
        expandIcon.textContent = isExpanded ? '\u25BC' : '\u25B6';
      });
      const fields = document.createElement('div');
      fields.className = 'llm-item-fields';
      const fieldDefs = [
        { label: 'Name', value: client.name, type: 'text' },
        { label: 'Type', value: client.type || 'openai', type: 'select', options: ['openai', 'knot'] },
        { label: 'Base URL', value: client.baseUrl, type: 'text' },
        { label: 'API Key', value: client.apiKey, type: 'password' },
        { label: 'Anydev Host', value: client.anydevHost, type: 'text' },
        { label: 'Model', value: client.model || '', type: 'text' },
        { label: 'Stateless', value: client.stateless === true, type: 'checkbox' }
      ];
      fieldDefs.forEach(function (fd) {
        const row = document.createElement('div');
        row.className = 'llm-field';
        row.setAttribute('data-field', fd.label);
        const lbl = document.createElement('label');
        lbl.textContent = fd.label;
        let inp;
        if (fd.type === 'select') {
          inp = document.createElement('select');
          fd.options.forEach(function (opt) {
            const o = document.createElement('option');
            o.value = opt;
            o.textContent = opt;
            if (opt === fd.value) o.selected = true;
            inp.appendChild(o);
          });
        } else if (fd.type === 'checkbox') {
          inp = document.createElement('input');
          inp.type = 'checkbox';
          inp.checked = fd.value === true;
          // Persist immediately: checkbox has no explicit save gesture,
          // and toggling On/Off button would otherwise overwrite unsaved state.
          if (fd.label === 'Stateless') {
            inp.addEventListener('change', function () {
              client.updateConfig({ stateless: inp.checked });
              self.llmManager._saveConfig();
            });
          }
        } else {
          inp = document.createElement('input');
          inp.type = fd.type;
          inp.value = fd.value;
        }
        inp.addEventListener('change', function () {
          title.textContent = fields.querySelector('input').value || 'LLM #' + (index + 1);
        });
        // For Type select, add handler to show/hide fields based on type
        if (fd.label === 'Type') {
          inp.addEventListener('change', function () {
            const typeVal = this.value;
            const anydevRow = fields.querySelector('[data-field="Anydev Host"]');
            if (anydevRow) {
              anydevRow.style.display = typeVal === 'knot' ? 'none' : '';
            }
          });

        }
        row.appendChild(lbl);
        row.appendChild(inp);
        fields.appendChild(row);
      });
      // Initialize field visibility based on current type
      const typeSelect = fields.querySelector('[data-field="Type"] select');
      if (typeSelect) {
        const anydevRow = fields.querySelector('[data-field="Anydev Host"]');
        if (anydevRow) {
          anydevRow.style.display = typeSelect.value === 'knot' ? 'none' : '';
        }
      }

      card.appendChild(header);
      card.appendChild(fields);
      self.llmListEl.appendChild(card);
    });
  }

  _addLlmItem() {
    this.llmManager.add({ name: 'LLM-' + (this.llmManager.clients.length + 1) });
    this._renderLlmList();
  }

  // ---- Message queue ----

  _enqueueMessage(sender, text, type, options) {
    this._messageQueue.push({ sender: sender, text: text, type: type, options: options || {} });
    this._updateQueueDisplay();
  }

  _processQueue() {
    if (this._messageQueue.length === 0) return;
    const item = this._messageQueue.shift();
    this._updateQueueDisplay();
    const sender = item.sender;
    const text = item.text;
    const type = item.type;
    const opts = item.options;
    // Display message and add to context (skip for userForward, already done in _sendMessage)
    if (!opts.userForward) {
      const role = (type === 'user') ? 'user' : 'assistant';
      const curSid = this.contactManager ? this.contactManager.currentSessionId : 'all';
      const isOtherSession = opts.channelId && opts.channelId !== curSid;
      console.log('[chat] processQueue route: sender=' + sender + ' opts.channelId=' + opts.channelId + ' curSid=' + curSid + ' isOtherSession=' + isOtherSession);
      if (isOtherSession) {
        // Message belongs to a different session: store without DOM render
        this._addMessageToSession(opts.channelId, sender, text, type);
        this._addToContextForSession(opts.channelId, role, text, sender);
      } else {
        this._addMessage(sender, text, type);
        if (this.soloTarget && !opts.isAiClawMessage) {
          // Solo mode: non-AiClaw messages go to history context
          this.historyMessages.push({ role: role, content: text, name: sender });
        } else {
          this._addToContext(role, text, sender);
        }
      }

      // Forward logic based on message source
      if (opts.forward) {
        // Forward all non-wx messages to WeChat (any channel)
        if (!opts.fromWx) {
          this._forwardToWx(sender, text);
        }

        // Forward routing: only PM messages trigger LLM+Relay cascade.
        // AI messages (hostReply or plain) never cascade to prevent loops;
        // they accumulate in history and will be carried when user/PM next forwards.
        if (opts.pmTrigger && !this.muted) {
          this._forwardToLlm(text, sender, opts.channelId);
          if (!opts.fromRelay) {
            const soloIsLlm = this.soloTarget && this.llmManager.clients.some(function (c) { return c.name === this.soloTarget; }.bind(this));
            if (!soloIsLlm && this.relay && this.relay.connected) {
              let receiver = this.soloTarget ? this.soloTarget : '*';
              // Group session filter: only relay to group members
              if (receiver === '*' && this.contactManager) {
                const sid = opts.channelId || this.contactManager.currentSessionId;
                if (sid !== 'all' && this.contactManager.isGroupSession(sid)) {
                  const members = this.contactManager.getGroupMembers(sid);
                  const names = members.map(function (m) { return m.name; });
                  if (names.length === 0) return;
                  receiver = names;
                }
              }
              const ctx = this._buildContextString();
              const relayContent = ctx.content || ('[' + sender + '] ' + text);
              console.log('[chat] relay forward (pm): sid=' + (this.contactManager ? this.contactManager.currentSessionId : 'N/A') + ', receiver=' + JSON.stringify(receiver));
              this.relay.sendMessage(relayContent, null, receiver, ctx.context, opts.channelId);
            }
          }
        }
        // Host mode reply counting
        if (opts.hostReply && !this.muted) {
          this._onHostReply();
        }
      }
    }
    // User message forward (display was already done, just forward)
    if (opts.userForward) {
      // Forward to LLM unconditionally: user messages always trigger cascade.
      // muted only controls PM auto-scheduling in host mode, not user forward.
      this._forwardToLlm(text, 'user', opts.channelId);
      // Always forward to relay regardless of mute
      const soloIsLlm = this.soloTarget && this.llmManager.clients.some(function (c) { return c.name === this.soloTarget; }.bind(this));
      if (!soloIsLlm && this.relay && this.relay.connected) {
        let receiver = this.soloTarget ? this.soloTarget : '*';
        // Group session filter: only relay to group members
        if (receiver === '*' && this.contactManager) {
          const sid = opts.channelId || this.contactManager.currentSessionId;
          if (sid !== 'all' && this.contactManager.isGroupSession(sid)) {
            const members = this.contactManager.getGroupMembers(sid);
            const names = members.map(function (m) { return m.name; });
            if (names.length === 0) return;
            receiver = names;
          }
        }
        const ctx = this._buildContextString();
        // Fallback: if currentMessages is empty, use current text as content
        // so peer LLM has a user message to work with.
        const relayContent = ctx.content || ('[' + sender + '] ' + text);
        console.log('[chat] relay forward (user): sid=' + (this.contactManager ? this.contactManager.currentSessionId : 'N/A') + ', receiver=' + JSON.stringify(receiver));

        this.relay.sendMessage(relayContent, null, receiver, ctx.context, opts.channelId);
      }
      // Forward user message to WeChat
      if (!opts.fromWx) {
        this._forwardToWx(sender, text);
      }
      // Reset host round on user message
      if (!this.muted) {
        this._cancelHostRound();
        this._startHostRound(opts.channelId);
      }
    }
    // PM host trigger: PM acts as user proxy in host mode,
    // auto-start next round to drive the meeting forward.
    if (opts.pmTrigger && !this.muted) {
      this._startHostRound(opts.channelId);
    }
    // Round boundary: when AiClaw sends a message, move current to history
    // Must be after all forwarding so _buildContextString() sees current messages
    if (opts.isAiClawMessage && this.currentMessages.length > 0) {
      // Clean MetaDSL execution results before archiving to history (reduce context size)
      const cleaned = this.currentMessages.map(function (m) {
        if (m.role === 'system' && m.content && m.content.startsWith('[MetaDSL result')) {
          return { role: m.role, content: '[Agent reply omitted]', name: m.name };
        }
        return m;
      });
      this.historyMessages = this.historyMessages.concat(cleaned);
      this.currentMessages = [];
    }
  }

  _clearQueue() {
    this._messageQueue = [];
    this._updateQueueDisplay();
    this._addSystemMessage('Message queue cleared');
  }

  _clearChat() {
    this.chatMessages.innerHTML = '';
    this.messages = [];
    this.historyMessages = [];
    this.currentMessages = [];
    this.contextSummary = '';
    // Clear session data for current session
    if (this._lastSessionId) {
      delete this._sessionData[this._lastSessionId];
    }
    localStorage.removeItem('aiclawcontext_summary');
    this._addSystemMessage('Chat cleared');
  }

  // ---- Session switching (Step 3b) ----

  _saveCurrentSession() {
    if (!this._lastSessionId) return;
    this._sessionData[this._lastSessionId] = {
      messages: this.messages.slice(),
      historyMessages: this.historyMessages.slice(),
      currentMessages: this.currentMessages.slice(),
      contextSummary: this.contextSummary || ''
    };
  }

  _loadSession(sessionId) {
    // Clear DOM
    this.chatMessages.innerHTML = '';
    const data = this._sessionData[sessionId];
    if (data) {
      this.messages = data.messages.slice();
      this.historyMessages = data.historyMessages.slice();
      this.currentMessages = data.currentMessages.slice();
      this.contextSummary = data.contextSummary || '';
      // Rebuild DOM from messages
      for (let i = 0; i < this.messages.length; i++) {
        const msg = this.messages[i];
        const el = document.createElement('div');
        el.className = 'chat-msg chat-msg-' + msg.type;
        const header = document.createElement('div');
        header.className = 'chat-msg-header';
        const nameSpan = document.createElement('span');
        nameSpan.className = 'chat-msg-sender';
        nameSpan.textContent = msg.sender;
        const timeSpan = document.createElement('span');
        timeSpan.className = 'chat-msg-time';
        timeSpan.textContent = msg.time instanceof Date ? msg.time.toLocaleTimeString() : new Date(msg.time).toLocaleTimeString();
        header.appendChild(nameSpan);
        header.appendChild(timeSpan);
        const body = document.createElement('div');
        body.className = 'chat-msg-body';
        this._renderMessageBody(body, msg.text);
        el.appendChild(header);
        el.appendChild(body);
        this.chatMessages.appendChild(el);
      }
      this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
    } else {
      this.messages = [];
      this.historyMessages = [];
      this.currentMessages = [];
      this.contextSummary = '';
    }
    this._lastSessionId = sessionId;
  }
  _updateQueueDisplay() {
    if (this._queueDisplayEl) {
      this._queueDisplayEl.textContent = 'Queue: ' + this._messageQueue.length;
    }
  }

  // ---- Context management ----

  _addToContext(role, content, name) {
    const entry = { role: role, content: content };
    if (name) entry.name = name;
    this.currentMessages.push(entry);
    this._trimContext();
  }
  _addMessageToSession(sessionId, sender, text, type) {
    let data = this._sessionData[sessionId];
    if (!data) {
      data = { messages: [], historyMessages: [], currentMessages: [], contextSummary: '' };
      this._sessionData[sessionId] = data;
    }
    data.messages.push({ sender: sender, text: text, type: type, time: new Date() });
  }

  _addToContextForSession(sessionId, role, content, name) {
    let data = this._sessionData[sessionId];
    if (!data) {
      data = { messages: [], historyMessages: [], currentMessages: [], contextSummary: '' };
      this._sessionData[sessionId] = data;
    }
    const entry = { role: role, content: content };
    if (name) entry.name = name;
    data.currentMessages.push(entry);
  }


  _trimHistoryByLength(historyList, maxChars) {
    // Trim history messages from oldest, keep newest within maxChars limit.
    // Only affects history messages; system prompt and current messages are not touched.
    if (!historyList || historyList.length === 0) return [];
    let total = 0;
    const result = [];
    for (let i = historyList.length - 1; i >= 0; i--) {
      const msg = historyList[i];
      const len = (msg.content ? msg.content.length : 0) + (msg.name ? msg.name.length : 0);
      if (result.length > 0 && total + len > maxChars) break;
      total += len;
      result.unshift(msg);
    }
    return result;
  }

  _trimContext() {
    // Trigger PM summarization when reaching max count
    if (this.historyMessages.length + this.currentMessages.length >= this.MAX_CONTEXT_COUNT) {
      this._triggerSummarize();
    }
  }

  _buildContextString(overrideSystemPrompt) {
    // Build content from currentMessages and context from historyMessages
    const contentParts = [];
    for (let i = 0; i < this.currentMessages.length; i++) {
      const msg = this.currentMessages[i];
      const prefix = msg.name ? '[' + msg.name + ']' : '[' + msg.role + ']';
      contentParts.push(prefix + ' ' + msg.content);
    }
    const contextParts = [];
    if (this.historyMessages.length > 0) {
      contextParts.push('【历史对话】');
      for (let i = 0; i < this.historyMessages.length; i++) {
        const msg = this.historyMessages[i];
        const p = msg.name ? '[' + msg.name + ']' : '[' + msg.role + ']';
        contextParts.push(p + ' ' + msg.content);
      }
    }
    if (this.contextSummary) {
      if (contextParts.length > 0) contextParts.push('------');
      contextParts.push('【上下文】');
      contextParts.push(this.contextSummary);
    }
    const sysPrompt = overrideSystemPrompt !== undefined ? overrideSystemPrompt : this.systemPrompt;
    if (sysPrompt) {
      if (contextParts.length > 0) contextParts.push('------');
      contextParts.push('【系统提示词】');
      contextParts.push(sysPrompt);
    }
    return {
      content: contentParts.join('\n'),
      context: contextParts.join('\n')
    };
  }

  _buildLlmMessages(overrideSystemPrompt, includePmPrivate) {
    // Build messages array for LLM API call
    // includePmPrivate: only true when PM hosts (triggerPmHost), so PM private chat
    // is not exposed to other LLMs / relay / chat room.
    const msgs = [];
    const sysPrompt = overrideSystemPrompt || this.systemPrompt;
    if (sysPrompt) {
      msgs.push({ role: 'system', content: sysPrompt });
    }
    if (this.contextSummary) {
      msgs.push({ role: 'system', content: '[Context Summary]\n' + this.contextSummary });
    }
    // Inject group announcement as system context
    if (this.contactManager) {
      const sid = this.contactManager.currentSessionId;
      if (sid !== 'all' && this.contactManager.isGroupSession(sid)) {
        const grp = this.contactManager.getGroup(sid);
        if (grp && grp.announcement) {
          msgs.push({ role: 'system', content: '[Group Announcement]\n' + grp.announcement });
        }
      }
    }
    // Inject PM private chat as system context ONLY when PM hosts (guard by includePmPrivate).
    // Other LLMs / relay / chat room must NOT see PM private chat content.
    if (includePmPrivate && this.pmPanel && this.pmPanel.messages && this.pmPanel.messages.length > 0) {
      const pmLines = [];
      for (let pi = 0; pi < this.pmPanel.messages.length; pi++) {
        const pm = this.pmPanel.messages[pi];
        if (pm.role === 'user' || pm.role === 'assistant') {
          const speaker = (pm.role === 'user') ? 'User' : 'PM';
          pmLines.push('[' + speaker + '] ' + pm.content);
        }
      }
      if (pmLines.length > 0) {
        msgs.push({ role: 'system', content: '[PM Private Chat]\n' + pmLines.join('\n') });
      }
    }
    const trimmedHistory = this._trimHistoryByLength(this.historyMessages, this.MAX_CONTEXT_CHARS);
    const allMsgs = trimmedHistory.concat(this.currentMessages);
    for (let i = 0; i < allMsgs.length; i++) {
      const c = allMsgs[i].name ? '[' + allMsgs[i].name + '] ' + allMsgs[i].content : allMsgs[i].content;
      msgs.push({
        role: allMsgs[i].role,
        content: c
      });
    }
    return msgs;
  }

  _triggerSummarize() {
    if (this._summarizing || !this.pmClient) {
      // No PM configured or already summarizing, just trim oldest
      while (this.historyMessages.length + this.currentMessages.length > this.MAX_CONTEXT_COUNT) {
        if (this.historyMessages.length > 0) {
          this.historyMessages.shift();
        } else {
          this.currentMessages.shift();
        }
      }
      return;
    }
    this._summarizing = true;
    // Snapshot history messages for summarization, keep current round
    const snapshot = this._trimHistoryByLength(this.historyMessages, this.MAX_CONTEXT_CHARS);
    this.historyMessages = [];
    // Build summarization request
    const SUMMARIZE_PROMPT = 'Please summarize the following conversation into a concise context summary. Keep key information, decisions, and conclusions. Remove redundant dialogue:\n\nIMPORTANT: Never omit any underscores in variable names, paths, or formulas. Strictly preserve all snake_case formatting.\n\n';
    let conversationText = '';
    for (let i = 0; i < snapshot.length; i++) {
      const msg = snapshot[i];
      const prefix = msg.name ? '[' + msg.name + ']' : '[' + msg.role + ']';
      conversationText += prefix + ' ' + msg.content + '\n';
    }
    const messages = [{ role: 'user', content: SUMMARIZE_PROMPT + conversationText }];
    const self = this;
    console.log('[PM] summarize request:', getStringInLength(conversationText, 400, 2));
    this.pmClient.chat(messages).then(function (reply) {
      if (reply) {
        self.contextSummary = (self.contextSummary ? self.contextSummary + '\n---\n' : '') + reply;
        localStorage.setItem('aiclaw_context_summary', self.contextSummary);
        console.log('[PM] summarize reply:', reply.substring(0, 300));
      }
      self._summarizing = false;
    }).catch(function (err) {
      console.error('PM summarization failed:', err);
      self._appendSystemMessage('[PM] Summarization failed: ' + err.message);
      self._summarizing = false;
    });
  }

  _loadPmConfig() {
    const pmBaseUrl = localStorage.getItem('aiclaw_pm_base_url') || '';
    const pmApiKey = localStorage.getItem('aiclaw_pm_api_key') || '';
    const pmModel = localStorage.getItem('aiclaw_pm_model') || '';
    if (pmBaseUrl && pmApiKey) {
      this.pmClient = new LlmClient({
        name: 'PM',
        type: 'knot',
        baseUrl: pmBaseUrl,
        apiKey: pmApiKey,
        model: pmModel,
        enabled: true,
      });
    } else {
      this.pmClient = null;
    }
  }
  _updateSoloDisplay() {
    let el = document.getElementById('solo-display');
    if (!el) {
      // Create solo display element after resume button
      el = document.createElement('span');
      el.id = 'solo-display';
      el.style.color = '#ff9800';
      el.style.marginLeft = '8px';
      el.style.fontSize = '13px';
      this.resumeBtn.parentNode.insertBefore(el, this.resumeBtn.nextSibling);
    }
    el.textContent = this.soloTarget ? 'solo:' + this.soloTarget : '';
    el.style.display = this.soloTarget ? 'inline' : 'none';
  }

  // ---- MetaDSL detection and execution ----

  _handleMetaDSL(text, senderName, channelId) {
    if (typeof MetaDSLUtils === 'undefined') {
      return text;
    }
    const result = MetaDSLUtils.validateAndExtract(text);
    // No MetaDSL tag at all -> return original text
    if (result.ok && result.code === null) {
      return text;
    }
    // Validation failed -> show system message, return cleanText with error placeholder
    if (!result.ok) {
      console.warn('[MetaDSL] Validation failed for ' + senderName + ': ' + result.reason);
      this._addSystemMessage('[MetaDSL invalid] from ' + senderName + ': ' + result.reason);
      return result.cleanText;
    }
    // Format warning (e.g. several <metadsl> blocks in one message) -> report
    // it but keep going. The note for the model is already appended to
    // cleanText by validateAndExtract; this only surfaces it locally.
    if (result.warning) {
      console.warn('[MetaDSL] Format warning for ' + senderName + ': ' + result.warning);
      this._addSystemMessage('[MetaDSL warning] from ' + senderName + ': ' + result.warning);
    }
    // Valid MetaDSL extracted -> execute
    console.log('[MetaDSL] Detected code in message from ' + senderName + ' (length: ' + result.code.length + ')');
    // Connect executor if not connected
    if (!metadslExecutor.isReady()) {
      metadslExecutor.connect();
    }
    // Execute asynchronously, send result back as system message
    const self = this;
    metadslExecutor.execute(result.code, channelId, senderName)
      .then(function (execResult) {
        console.log('[MetaDSL] Execution result (length: ' + execResult.length + ')');
        const resultMsg = '[MetaDSL result for ' + senderName + ']\n' + execResult;
        self._enqueueMessage('system', resultMsg, 'system', { forward: true, channelId: channelId });
        // Solo mode: inject a user "continue" to drive the peer LLM.
        // Mimic _sendMessage: display, add to context, then enqueue with userForward
        // so it bypasses Mute/Host (always forwards).
        if (self.soloTarget) {
          const followup = 'continue';
          self._enqueueMessage(self.userName, followup, 'user', {
            userForward: true,
            isAiClawMessage: true,
            channelId: channelId
          });
        }
      })
      .catch(function (err) {
        console.error('[MetaDSL] Execution error:', err.message);
        self._addSystemMessage('[MetaDSL error] ' + err.message);
        // Solo mode: inject a user "MetaDSL execution error" to drive the peer LLM.
        // Mimic _sendMessage: display, add to context, then enqueue with userForward
        // so it bypasses Mute/Host (always forwards).
        if (self.soloTarget) {
          const followup = 'MetaDSL execution error';
          self._enqueueMessage(self.userName, followup, 'user', {
            userForward: true,
            isAiClawMessage: true,
            channelId: channelId
          });
        }
      });
    return result.cleanText;
  }


  // ---- LLM message forwarding ----

  _forwardToLlm(text, senderName, channelId) {
    const self = this;
    const capturedChannelId = channelId || this.contactManager.currentSessionId;
    console.log('[chat] forwardToLlm: channelId param=' + channelId + ' capturedChannelId=' + capturedChannelId);
    // Build messages array with full context for LLM
    const prompt = this._buildLlmMessages();
    // Fallback: ensure at least one user message so peer LLM API accepts it
    const hasUser = prompt.some(function (m) { return m.role === 'user'; });
    if (!hasUser) {
      prompt.push({ role: 'user', content: '[' + (senderName || 'user') + '] ' + text });
    }
    console.log('[chat] forwarding to LLM, messages count:', prompt.length);

    this.llmManager.clients.forEach(function (client) {
      if (!client.enabled) return;
      // Skip the LLM that sent this message (avoid echo)
      if (senderName && client.name === senderName) return;

      // Solo mode: only send to solo target LLM
      if (self.soloTarget && client.name !== self.soloTarget) return;

      // Group session filter: only send to group members
      if (self.contactManager) {
        const sid = capturedChannelId;
        if (sid && sid !== 'all' && self.contactManager.isGroupSession(sid)) {
          const cid = self.contactManager.findContactIdByName(client.name);
          const members = self.contactManager.getGroupMembers(sid); const inSession = cid ? members.some(function (m) { return m.id === cid; }) : false;
          console.log('[chat] group filter: client=' + client.name + ' sid=' + sid + ' cid=' + cid + ' inSession=' + inSession);
          if (!cid || !inSession) return;
        }
      }

      client.chat(prompt)
        .then(function (reply) {
          // Stateless client with no new messages returned empty: skip display but compensate host counter
          if (client.stateless && (!reply || reply === '')) {
            console.log('[chat] stateless ' + client.name + ' returned empty, skip display');
            if (self._hostActive) self._onHostReply();
            return;
          }
          // Enqueue LLM reply for delayed processing
          console.log('[chat] LLM reply from [' + client.name + ']:', getStringInLength(reply, 300, 2));

          // MetaDSL detection: extract and execute code, use clean text for display
          reply = self._handleMetaDSL(reply, client.name, capturedChannelId);

          // Route LLM reply per rule: group channelId keep, otherwise sender's private channel
          let finalChannelId;
          if (capturedChannelId && self.contactManager.isGroupSession(capturedChannelId)) {
            finalChannelId = capturedChannelId;
          } else {
            finalChannelId = 'contact_llm_' + client.name;
          }
          console.log('[chat] LLM reply route: captured=' + capturedChannelId + ' final=' + finalChannelId);
          self._enqueueMessage(client.name, reply, 'claw', { forward: true, hostReply: true, channelId: finalChannelId });
        })
        .catch(function (err) {
          self._addSystemMessage('[' + client.name + ' error] ' + err.message);
        });
    });
  }

  // --- Host mode methods ---

  _startHostRound(channelId) {
    if (this.muted) return; // only in Host mode
    this._hostChannelId = channelId || (this.contactManager && this.contactManager.currentSessionId) || null;
    // Query status to refresh client list
    this._sendControl('status');
    // Calculate expected replies: mirror _forwardToLlm and relay forward(user) filters
    const self = this;
    const sid = this._hostChannelId;
    const isGroup = sid && sid !== 'all' && this.contactManager && this.contactManager.isGroupSession(sid);
    const members = isGroup ? this.contactManager.getGroupMembers(sid) : null;
    const soloIsLlm = this.soloTarget && this.llmManager && this.llmManager.clients.some(function (c) { return c.name === self.soloTarget; });
    const llmCount = this.llmManager ? this.llmManager.clients.filter(function (c) {
      if (!c.enabled) return false;
      if (self.soloTarget && c.name !== self.soloTarget) return false;
      if (isGroup) {
        const cid = self.contactManager.findContactIdByName(c.name);
        if (!cid) return false;
        return members.some(function (m) { return m.id === cid; });
      }
      return true;
    }).length : 0;
    let clawCount;
    if (soloIsLlm) {
      clawCount = 0;
    } else if (this.soloTarget) {
      clawCount = this._lastClawClients.some(function (c) { return c.name === self.soloTarget; }) ? 1 : 0;
    } else if (isGroup) {
      clawCount = this._lastClawClients.filter(function (c) {
        const cid = self.contactManager.findContactIdByName(c.name);
        if (!cid) return false;
        return members.some(function (m) { return m.id === cid; });
      }).length;
    } else {
      clawCount = this._lastClawClients.length;
    }
    this._hostExpectedCount = llmCount + clawCount;
    this._hostRoundCount = 0;
    this._hostActive = true;
    if (this._hostExpectedCount === 0) {
      // No participants, trigger PM immediately
      this._onRoundComplete();
      return;
    }
    // Start timeout timer
    this._hostTimer = setTimeout(function () {
      if (self._hostActive) {
        console.log('[Host] Round timeout after ' + self.HOST_TIMEOUT + 'ms, received ' + self._hostRoundCount + '/' + self._hostExpectedCount);
        self._onRoundComplete();
      }
    }, this.HOST_TIMEOUT);
  }

  _cancelHostRound() {
    this._hostActive = false;
    this._hostRoundCount = 0;
    this._hostExpectedCount = 0;
    this._hostChannelId = null;
    if (this._hostTimer) {
      clearTimeout(this._hostTimer);
      this._hostTimer = null;
    }
  }

  _onHostReply() {
    if (!this._hostActive) return;
    this._hostRoundCount++;
    console.log('[Host] Reply ' + this._hostRoundCount + '/' + this._hostExpectedCount);
    if (this._hostRoundCount >= this._hostExpectedCount) {
      this._onRoundComplete();
    }
  }

  _onRoundComplete() {
    this._hostActive = false;
    if (this._hostTimer) {
      clearTimeout(this._hostTimer);
      this._hostTimer = null;
    }
    console.log('[Host] Round complete, triggering PM host');
    this._triggerPmHost();
  }

  _triggerPmHost() {
    if (!this.pmClient) {
      this._addSystemMessage('[Host] PM not configured, skipping host turn');
      return;
    }
    const self = this;
    // Build PM prompt from private chat history
    const pmChatHistory = this.pmPanel.messages;
    let pmSysPrompt = 'You are the meeting host/moderator. ';
    if (pmChatHistory.length > 0) {
      pmSysPrompt += 'The user gave you these instructions:\n';
      pmChatHistory.forEach(function (msg) {
        pmSysPrompt += '[' + msg.role + '] ' + msg.content + '\n';
      });
    }
    pmSysPrompt += 'As the host, please provide your moderation comment, summary, or guide the discussion. Keep it concise.';
    // Build context using chat room messages with PM-specific system prompt
    const hostPrompt = this._buildLlmMessages(pmSysPrompt, true);
    console.log('[Host] PM request, messages count:', hostPrompt.length);
    this.pmClient.chat(hostPrompt)
      .then(function (reply) {
        // If switched to Mute while PM was thinking, discard reply
        if (self.muted) {
          console.log('[Host] PM reply discarded (switched to Mute)');
          return;
        }
        console.log('[Host] PM reply: ' + reply.substring(0, 300));
        // Enqueue PM message for delayed processing
        self._enqueueMessage('PM', reply, 'claw', { forward: true, pmTrigger: true, isAiClawMessage: true, channelId: self._hostChannelId });
      })
      .catch(function (err) {
        self._addSystemMessage('[PM Host error] ' + err.message);
      });
  }
}
