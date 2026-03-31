  // ============================================================================
  // ChatInputPanel - Floating input panel for sending messages to Ollama
  // ============================================================================
  class ChatInputPanel {
    static STATE_KEY = 'chat_panel_state';
    static ENV_KEY = 'agent_environments';
    static DEFAULT_CATEGORIES = ['llm', 'mcp'];
    static MAX_CHAT_LINES = 200;

    constructor(bridge) {
      this.bridge = bridge;
      this.visible = true;
      this.panel = null;
      this.chatLog = null;
      this.textInput = null;
      this.topicInput = null;
      this.tagInput = null;
      this.providerIdSelect = null;
      this.configArea = null;
      // config fields - browse row (3 dropdowns for browsing existing configs)
      this.cfgBrowseCategory = null;
      this.cfgBrowseGroup = null;
      this.cfgBrowseKey = null;
      // config fields - input row (3 inputs for entering new values)
      this.cfgEnvGroup = null;
      this.cfgEnvKey = null;
      this.cfgEnvValue = null;
      this.onVisibilityChange = null;
      this.createPanel();
      // _restoreConfig is async, called explicitly from main.js after SecretStore is ready
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
      el.style.cssText = 'color:#aaa;font-size:11px;white-space:nowrap;';
      return el;
    }

    createPanel() {
      this.panel = document.createElement('div');
      this.panel.id = 'chat-input-panel';
      this.panel.style.cssText = `
        position: fixed;
        left: 20px;
        bottom: 120px;
        width: 480px;
        background: #2d2d2d;
        border: 1px solid #555;
        border-radius: 8px;
        display: flex;
        flex-direction: column;
        z-index: 10001;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Arial, sans-serif;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
        padding: 8px;
        gap: 6px;
      `;

      // Prevent events from bubbling out (use bubble phase so events reach target first)
      ['input', 'change', 'paste', 'cut', 'keydown', 'keyup', 'keypress', 'beforeinput'].forEach(eventType => {
        this.panel.addEventListener(eventType, (e) => { e.stopPropagation(); }, false);
      });

      // Row 0: quick action buttons
      const row0 = document.createElement('div');
      row0.style.cssText = 'display:flex;gap:6px;align-items:center;';

      const actionButtons = [
        { label: 'Prompt', action: () => window.AgentAPI.updateSystemPrompt() },
        { label: 'Context', action: () => window.AgentAPI.keepContext() },
        { label: 'Align', action: () => window.AgentAPI.alignTarget() },
        { label: 'Plan', action: () => window.AgentAPI.needToPlan() }
      ];
      actionButtons.forEach(({ label, action }) => {
        const btn = document.createElement('button');
        btn.textContent = label;
        btn.style.cssText = `
          padding: 4px 10px;
          background: #3a3a3a;
          color: #ccc;
          border: 1px solid #555;
          border-radius: 4px;
          cursor: pointer;
          font-size: 11px;
          white-space: nowrap;
        `;
        btn.onmouseenter = () => { btn.style.background = '#505050'; };
        btn.onmouseleave = () => { btn.style.background = '#3a3a3a'; };
        btn.onclick = action;
        row0.appendChild(btn);
      });

      // Close button in row0
      const spacer = document.createElement('div');
      spacer.style.cssText = 'flex:1;';
      row0.appendChild(spacer);
      const closeBtn = document.createElement('button');
      closeBtn.innerHTML = '&times;';
      closeBtn.style.cssText = 'background:none;border:none;color:#999;font-size:18px;cursor:pointer;padding:0 4px;';
      closeBtn.onclick = () => this.hide();
      row0.appendChild(closeBtn);

      // Row 1: multi-line text input (3 lines height)
      this.textInput = document.createElement('textarea');
      this.textInput.placeholder = 'Enter message... (Shift+Enter for newline)';
      this.textInput.rows = 3;
      this.textInput.style.cssText = `
        width: 100%;
        background: #1e1e1e;
        color: #d4d4d4;
        border: 1px solid #444;
        border-radius: 4px;
        padding: 6px 8px;
        font-family: 'Consolas', 'Monaco', monospace;
        font-size: 12px;
        outline: none;
        box-sizing: border-box;
        resize: vertical;
        line-height: 1.4;
      `;
      this.textInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); this.sendChat(); }
      });

      // Row 2: topic + tag + provider_id + send + config
      const row2 = document.createElement('div');
      row2.style.cssText = 'display:flex;gap:6px;align-items:center;';

      this.topicInput = this._makeInput('text', 'Topic', 'align_target');
      this.tagInput = this._makeInput('text', 'Session tag', 'llm_pm_inject');
      this.providerIdSelect = document.createElement('select');
      this.providerIdSelect.style.cssText = `
        background:#1e1e1e;color:#d4d4d4;border:1px solid #444;
        border-radius:4px;padding:5px 6px;font-size:12px;outline:none;
        max-width:120px;min-width:80px;
      `;
      this._refreshProviderOptions();

      const sendBtn = document.createElement('button');
      sendBtn.textContent = 'Send';
      sendBtn.style.cssText = `
        padding: 5px 14px;
        background: #2196f3;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
        white-space: nowrap;
      `;
      sendBtn.onclick = () => this.sendChat();

      const clearBtn = document.createElement('button');
      clearBtn.textContent = 'Clear';
      clearBtn.style.cssText = `
        padding: 5px 10px;
        background: #ff9800;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
        white-space: nowrap;
      `;
      clearBtn.onmouseenter = () => { clearBtn.style.background = '#ffa726'; };
      clearBtn.onmouseleave = () => { clearBtn.style.background = '#ff9800'; };
      clearBtn.onclick = () => { this.chatLog.value = ''; };

      const configBtn = document.createElement('button');
      configBtn.textContent = 'Config';
      configBtn.style.cssText = `
        padding: 5px 10px;
        background: #555;
        color: #ddd;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
        white-space: nowrap;
      `;
      configBtn.onclick = () => this._toggleConfig();

      row2.appendChild(this.topicInput);
      row2.appendChild(this.tagInput);
      row2.appendChild(this.providerIdSelect);
      row2.appendChild(sendBtn);
      row2.appendChild(clearBtn);
      row2.appendChild(configBtn);

      // Config area (hidden by default)
      this.configArea = document.createElement('div');
      this.configArea.style.cssText = `
        display: none;
        flex-direction: column;
        gap: 5px;
        background: #252525;
        border: 1px solid #444;
        border-radius: 4px;
        padding: 8px;
      `;

      // button row: Save Ctx + Load Ctx
      const btnRow = document.createElement('div');
      btnRow.style.cssText = 'display:flex;gap:8px;justify-content:flex-end;';

      const saveCtxBtn = document.createElement('button');
      saveCtxBtn.textContent = 'Save Ctx';
      saveCtxBtn.style.cssText = `
        padding: 5px 12px;
        background: #7b1fa2;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      saveCtxBtn.onclick = () => { this._saveAgentEnv(); };

      const loadCtxBtn = document.createElement('button');
      loadCtxBtn.textContent = 'Load Ctx';
      loadCtxBtn.style.cssText = `
        padding: 5px 12px;
        background: #5c6bc0;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      loadCtxBtn.onclick = () => { this._loadAgentEnv(); };

      btnRow.appendChild(saveCtxBtn);
      btnRow.appendChild(loadCtxBtn);
      this.configArea.appendChild(btnRow);

      // Browse row: 3 cascading dropdowns for browsing existing configs
      const envBrowseRow = document.createElement('div');
      envBrowseRow.style.cssText = 'display:flex;gap:6px;align-items:center;';
      envBrowseRow.appendChild(this._makeLabel('Env:'));
      const selectStyle = `
        background:#1e1e1e;color:#d4d4d4;border:1px solid #444;
        border-radius:4px;padding:4px 6px;font-size:12px;outline:none;
        max-width:110px;min-width:60px;
      `;
      this.cfgBrowseCategory = document.createElement('select');
      this.cfgBrowseCategory.style.cssText = selectStyle;
      this.cfgBrowseCategory.onchange = () => { this._onBrowseCategoryChange(); };
      this.cfgBrowseGroup = document.createElement('select');
      this.cfgBrowseGroup.style.cssText = selectStyle;
      this.cfgBrowseGroup.onchange = () => { this._onBrowseGroupChange(); };
      this.cfgBrowseKey = document.createElement('select');
      this.cfgBrowseKey.style.cssText = selectStyle;
      this.cfgBrowseKey.onchange = () => this._onBrowseKeyChange();
      envBrowseRow.appendChild(this.cfgBrowseCategory);
      envBrowseRow.appendChild(this.cfgBrowseGroup);
      envBrowseRow.appendChild(this.cfgBrowseKey);
      const delEnvBtn = document.createElement('button');
      delEnvBtn.textContent = 'Del';
      delEnvBtn.style.cssText = `
        padding: 4px 8px;
        background: #c62828;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
        white-space: nowrap;
      `;
      delEnvBtn.onmouseenter = () => { delEnvBtn.style.background = '#e53935'; };
      delEnvBtn.onmouseleave = () => { delEnvBtn.style.background = '#c62828'; };
      delEnvBtn.onclick = () => this._deleteAgentEnv();
      envBrowseRow.appendChild(delEnvBtn);
      this.configArea.appendChild(envBrowseRow);

      // Input row: 3 inputs (group + key + value) for entering/editing values
      const envRow = document.createElement('div');
      envRow.style.cssText = 'display:flex;gap:6px;align-items:center;';
      envRow.appendChild(this._makeLabel('Val:'));
      this.cfgEnvGroup = this._makeInput('text', 'Group', '');
      this.cfgEnvGroup.style.cssText += 'max-width:110px;';
      this.cfgEnvKey = this._makeInput('text', 'Key', '');
      this.cfgEnvKey.style.cssText += 'max-width:120px;';
      this.cfgEnvValue = this._makeInput('password', 'Value (secret)', '');
      envRow.appendChild(this.cfgEnvGroup);
      envRow.appendChild(this.cfgEnvKey);
      envRow.appendChild(this.cfgEnvValue);
      this.configArea.appendChild(envRow);

      // Chat log area (readonly, displays conversation history)
      this.chatLog = document.createElement('textarea');
      this.chatLog.readOnly = true;
      this.chatLog.style.cssText = `
        width: 100%;
        height: 160px;
        background: #1e1e1e;
        color: #d4d4d4;
        border: 1px solid #444;
        border-radius: 4px;
        padding: 8px 10px;
        font-family: 'Consolas', 'Monaco', monospace;
        font-size: 12px;
        resize: vertical;
        outline: none;
        box-sizing: border-box;
      `;

      this.panel.appendChild(row0);
      this.panel.appendChild(this.chatLog);
      this.panel.appendChild(this.textInput);
      this.panel.appendChild(row2);
      this.panel.appendChild(this.configArea);

      document.body.appendChild(this.panel);
      this._initDrag();
    }

    _toggleConfig() {
      const visible = this.configArea.style.display !== 'none';
      if (visible) {
        this.configArea.style.display = 'none';
      } else {
        this.configArea.style.display = 'flex';
      }
    }

    async _refreshProviderOptions() {
      const currentVal = this.providerIdSelect ? this.providerIdSelect.value : '';
      while (this.providerIdSelect.firstChild) this.providerIdSelect.removeChild(this.providerIdSelect.firstChild);
      // Built-in providers that don't need apiKey
      const builtIn = ['ollama', 'lmstudio'];
      builtIn.forEach(pid => {
        const opt = document.createElement('option');
        opt.value = pid;
        opt.textContent = pid;
        this.providerIdSelect.appendChild(opt);
      });
      // Add providers from agent_environments.llm (those with stored tokens)
      try {
        const envs = await this._getEnvData();
        if (envs.llm) {
          Object.keys(envs.llm).forEach(group => {
            if (!builtIn.includes(group)) {
              const opt = document.createElement('option');
              opt.value = group;
              opt.textContent = group;
              this.providerIdSelect.appendChild(opt);
            }
          });
        }
      } catch (e) {
        console.warn('[ChatInputPanel] Failed to load provider list from env:', e);
      }
      // Restore previous selection if still exists
      const allValues = Array.from(this.providerIdSelect.options).map(o => o.value);
      if (currentVal && allValues.includes(currentVal)) {
        this.providerIdSelect.value = currentVal;
      }
    }

    async _getEnvData() {
      try {
        const raw = await secretStore.getItem(ChatInputPanel.ENV_KEY);
        return raw ? JSON.parse(raw) : {};
      } catch (e) {
        return {};
      }
    }

    _populateSelect(sel, items, preserveValue) {
      const prev = preserveValue ? sel.value : '';
      while (sel.firstChild) sel.removeChild(sel.firstChild);
      items.forEach(item => {
        const opt = document.createElement('option');
        opt.value = item;
        opt.textContent = item;
        sel.appendChild(opt);
      });
      if (prev && items.includes(prev)) sel.value = prev;
    }

    async _refreshBrowseUI() {
      const envs = await this._getEnvData();
      const dataCategories = Object.keys(envs);
      // Merge default categories with data categories (deduplicated, defaults first)
      const categories = [...ChatInputPanel.DEFAULT_CATEGORIES];
      dataCategories.forEach(c => { if (!categories.includes(c)) categories.push(c); });
      this._populateSelect(this.cfgBrowseCategory, categories, true);
      await this._onBrowseCategoryChange();
    }

    async _onBrowseCategoryChange() {
      const envs = await this._getEnvData();
      const category = this.cfgBrowseCategory.value;
      const groups = (envs[category]) ? Object.keys(envs[category]) : [];
      this._populateSelect(this.cfgBrowseGroup, groups, true);
      await this._onBrowseGroupChange();
    }

    async _onBrowseGroupChange() {
      const envs = await this._getEnvData();
      const category = this.cfgBrowseCategory.value;
      const group = this.cfgBrowseGroup.value;
      const keys = (envs[category] && envs[category][group]) ? Object.keys(envs[category][group]) : [];
      this._populateSelect(this.cfgBrowseKey, keys, true);
      // Sync selected group to input row
      if (group) this.cfgEnvGroup.value = group;
      this._onBrowseKeyChange();
    }

    _onBrowseKeyChange() {
      // Sync selected key to input row
      const key = this.cfgBrowseKey.value;
      if (key) this.cfgEnvKey.value = key;
    }

    async _deleteAgentEnv() {
      const category = this.cfgBrowseCategory.value;
      const group = this.cfgBrowseGroup.value;
      const key = this.cfgBrowseKey.value;
      if (!category || !group || !key) {
        console.warn('[ChatInputPanel] Delete: need category, group and key selected');
        return;
      }
      try {
        const raw = await secretStore.getItem('agent_environments');
        const envs = raw ? JSON.parse(raw) : {};
        if (envs[category] && envs[category][group]) {
          delete envs[category][group][key];
          // Clean up empty group
          if (Object.keys(envs[category][group]).length === 0) {
            delete envs[category][group];
          }
          // Clean up empty category (data only, defaults still show via DEFAULT_CATEGORIES)
          if (Object.keys(envs[category]).length === 0) {
            delete envs[category];
          }
          await secretStore.setItem('agent_environments', JSON.stringify(envs));
          console.log('[ChatInputPanel] Deleted env: ' + category + '/' + group + '/' + key);
        }
      } catch (e) {
        console.warn('[ChatInputPanel] Failed to delete agent env:', e);
      }
      await this._refreshBrowseUI();
      await this._refreshProviderOptions();
      // Notify DSL that agent configs have been updated
      this.bridge.sendCommand('update_agent_configs', {}, () => {});
    }

    async _saveAgentEnv() {
      // Use browse dropdown category + input row group/key/value
      const category = this.cfgBrowseCategory.value;
      const group = this.cfgEnvGroup.value.trim();
      const key = this.cfgEnvKey.value.trim();
      const value = this.cfgEnvValue.value;
      // If value is empty, only refresh UI without writing
      if (!value) {
        await this._refreshBrowseUI();
        await this._refreshProviderOptions();
        console.log('[ChatInputPanel] No value provided, refreshed UI only');
        return;
      }
      if (!category || !group || !key) {
        console.warn('[ChatInputPanel] Agent Env: category, group, key and value are all required');
        return;
      }
      // Save to SecretStore (three-level structure)
      try {
        const raw = await secretStore.getItem('agent_environments');
        const envs = raw ? JSON.parse(raw) : {};
        if (!envs[category]) envs[category] = {};
        if (!envs[category][group]) envs[category][group] = {};
        envs[category][group][key] = value;
        await secretStore.setItem('agent_environments', JSON.stringify(envs));
      } catch (e) {
        console.warn('[ChatInputPanel] Failed to save agent env:', e);
      }
      // Send to backend via bridge
      this.bridge.sendCommand('set_agent_environment', { category, group, key, value }, async (success, data, error) => {
        if (!success) {
          console.warn('[ChatInputPanel] set_agent_environment failed:', error);
        } else {
          console.log('[ChatInputPanel] Agent env saved: ' + category + '/' + group + '/' + key);
          this.cfgEnvValue.value = '';
          // Refresh UI to reflect latest data
          await this._refreshBrowseUI();
          if (category === 'llm') {
            await this._refreshProviderOptions();
          }
        }
      });
      // Notify DSL that agent configs have been updated
      this.bridge.sendCommand('update_agent_configs', {}, () => {});
    }

    async _loadAgentEnv() {
      try {
        const raw = await secretStore.getItem('agent_environments');
        if (!raw) {
          console.log('[ChatInputPanel] No agent environments in SecretStore');
          // Still refresh UI to show default categories
          await this._refreshBrowseUI();
          await this._refreshProviderOptions();
          return;
        }
        const envs = JSON.parse(raw);
        let total = 0;
        Object.keys(envs).forEach(category => {
          const groups = envs[category];
          Object.keys(groups).forEach(group => {
            const kvs = groups[group];
            Object.keys(kvs).forEach(key => {
              this.bridge.sendCommand('set_agent_environment', { category, group, key, value: kvs[key] }, (success, data, error) => {
                if (!success) {
                  console.warn('[ChatInputPanel] set_agent_environment failed for ' + category + '/' + group + '/' + key + ':', error);
                }
              });
              total++;
            });
          });
        });
        console.log('[ChatInputPanel] Loaded ' + total + ' agent env(s) from SecretStore');
        // Refresh UI to reflect latest data
        await this._refreshBrowseUI();
        await this._refreshProviderOptions();
        // Notify DSL that agent configs have been updated
        if (total > 0) {
          this.bridge.sendCommand('update_agent_configs', {}, () => {});
        }
      } catch (e) {
        console.warn('[ChatInputPanel] Failed to load agent environments:', e);
      }
    }

    async _restoreConfig() {
      try {
        // Restore panel state (topic, tag, currentProviderId) - non-secret, stays in localStorage
        const stateRaw = localStorage.getItem(ChatInputPanel.STATE_KEY);
        if (stateRaw) {
          const state = JSON.parse(stateRaw);
          if (state.topic) this.topicInput.value = state.topic;
          if (state.tag) this.tagInput.value = state.tag;
          if (state.currentProviderId) {
            await this._refreshProviderOptions();
            this.providerIdSelect.value = state.currentProviderId;
          }
        }
        // Auto-load agent environments from SecretStore on startup
        await this._loadAgentEnv();
        // _loadAgentEnv already sends update_agent_configs after loading
      } catch (e) {
        console.warn('[ChatInputPanel] Failed to restore config:', e);
      }
    }

    _initDrag() {
      const panel = this.panel;
      let dragging = false;
      let startX, startY, startLeft, startTop;

      panel.addEventListener('mousedown', (e) => {
        const tag = e.target.tagName.toLowerCase();
        if (tag === 'textarea' || tag === 'input' || tag === 'button' || tag === 'select') return;
        dragging = true;
        startX = e.clientX;
        startY = e.clientY;
        const rect = panel.getBoundingClientRect();
        startLeft = rect.left;
        startTop = rect.top;
        panel.style.bottom = '';
        panel.style.top = startTop + 'px';
        panel.style.left = startLeft + 'px';
        e.preventDefault();
      });

      document.addEventListener('mousemove', (e) => {
        if (!dragging) return;
        panel.style.left = (startLeft + e.clientX - startX) + 'px';
        panel.style.top = (startTop + e.clientY - startY) + 'px';
      });

      document.addEventListener('mouseup', () => { dragging = false; });
    }

    _chatLog(msg) {
      const ts = new Date().toLocaleTimeString();
      this.chatLog.value += '[' + ts + '] ' + msg + '\n';
      // Enforce max lines
      const lines = this.chatLog.value.split('\n');
      if (lines.length > ChatInputPanel.MAX_CHAT_LINES) {
        this.chatLog.value = lines.slice(lines.length - ChatInputPanel.MAX_CHAT_LINES).join('\n');
      }
      this.chatLog.scrollTop = this.chatLog.scrollHeight;
    }

    addMessage(role, text) {
      this._chatLog('[' + role + '] ' + text);
    }

    sendChat() {
      const text = this.textInput.value.trim();
      const topic = this.topicInput.value.trim();
      const tag = this.tagInput.value.trim();
      const providerId = this.providerIdSelect ? this.providerIdSelect.value : 'ollama';
      if (!text) return;
      // Save current provider selection to panel state
      try {
        const state = {
          topic: this.topicInput.value.trim(),
          tag: this.tagInput.value.trim(),
          currentProviderId: providerId
        };
        localStorage.setItem(ChatInputPanel.STATE_KEY, JSON.stringify(state));
      } catch (e) {}
      // Show user message in chat log
      this._chatLog('[you] ' + text);
      this.textInput.value = '';
      // Unified: all types go through llm_chat
      this.bridge.sendCommand('llm_chat', { providerId, tag, topic, text }, (success, data, error) => {
        if (!success) {
          console.warn('[ChatInputPanel] llm_chat failed:', error);
          this._chatLog('[error] ' + (error || 'llm_chat failed'));
        }
      });
    }

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
      if (this.visible) {
        this.hide();
      } else {
        this.show();
      }
    }
  }

