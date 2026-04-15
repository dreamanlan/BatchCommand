// ============================================================================
// Configuration Manager - Centralized configuration with validation and persistence
// ============================================================================
class ConfigManager {
  constructor() {
    // Default configuration grouped by category
    this.defaults = {
      // Panel settings
      panel: {
        enabled: true,
        position: 'bottom-right',
        width: 760,
        height: 540,
        minWidth: 400,
        minHeight: 300,
        resizeHandleSize: 8,
        maxLogLines: 1024,
        collapseAgentReply: true,
        hideMetaDslBlock: true,
        streamingPage: true,
        jsHotReload: false,
        maxConversationRounds: 12
      },

      // Timing settings
      timing: {
        stateCheckInterval: 100,
        llmRespondingCheckInterval: 500,
        stateMachineLoopInterval: 100,
        sendMessageDelay: 100,
        mutationDebounce: 2000,
        mutationObserverRestartDelay: 300,
        userInputTimeout: 10000,
        userTypingDelay: 2000,
        pageStableDelay: 5000,
        operationDelay: 500,
        sendRetryDelay: 1000,
        operationExecuteTimeout: 5000,
      },

      // MetaDSL settings
      metadsl: {
        agentReplyMarker: '\u3010Agent\u56DE\u590D\u3011',
        markers: ['// @execute', '# @execute'],
        maxProcessedBlocks: 1000,
        maxSendRetries: 5,
        llmContextCountModuloForKeep: 32,
        llmContextCountModuloForAlign: 8
      },

      // WebSocket settings
      websocket: {
        port: 9527,
        reconnectDelay: 3000
      },

      // Log level settings
      logging: {
        consoleOutput: false,
        levels: {
          debug: false,
          info: true,
          warn: true,
          error: true
        }
      },

      // Relay settings
      relay: {
        httpBase: 'https://www.gamexyz.net:8443',
        wsUrl: 'wss://www.gamexyz.net:8443/ws',
        apiKey: '',
        session: '',
        reconnectDelay: 5000,
        heartbeatInterval: 30000
      }
    };

    // Keys that need persistence (have UI editing entry)
    // Note: relay.apiKey and relay.session are stored in SecretStore, not here
    this.persistKeys = [
      'panel.collapseAgentReply',
      'panel.hideMetaDslBlock',
      'panel.streamingPage',
      'panel.jsHotReload',
      'panel.maxConversationRounds',
      
      'relay.wsUrl'
    ];

    // Load configuration from localStorage or use defaults
    this.config = this.loadConfig();
  }

  // Load relay secrets from SecretStore (async, called after secretStore.ready())
  async loadSecrets() {
    try {
      const raw = await secretStore.getItem('relay_secrets');
      if (raw) {
        const secrets = JSON.parse(raw);
        if (secrets.apiKey) this.config.relay.apiKey = secrets.apiKey;
        if (secrets.session) this.config.relay.session = secrets.session;
      }
    } catch (e) {
      console.warn('[ConfigManager] Failed to load secrets from SecretStore:', e);
    }
  }

  // Save relay secrets to SecretStore (async)
  async saveSecrets() {
    try {
      const secrets = {};
      if (this.config.relay.apiKey) secrets.apiKey = this.config.relay.apiKey;
      if (this.config.relay.session) secrets.session = this.config.relay.session;
      await secretStore.setItem('relay_secrets', JSON.stringify(secrets));
    } catch (e) {
      console.warn('[ConfigManager] Failed to save secrets to SecretStore:', e);
    }
  }

  loadConfig() {
    // Start with deep copy of defaults
    const config = JSON.parse(JSON.stringify(this.defaults));
    try {
      const stored = localStorage.getItem('inject_config');
      if (stored) {
        const parsed = JSON.parse(stored);
        // Only restore persistKeys from stored data
        for (const key of this.persistKeys) {
          const val = this._getNested(parsed, key);
          if (val !== undefined) {
            this._setNested(config, key, val);
          }
        }
      }
    } catch (e) {
      console.warn('[ConfigManager] Failed to load config from localStorage:', e);
    }
    return config;
  }

  saveConfig() {
    try {
      // Only save persistKeys to localStorage
      const toSave = {};
      for (const key of this.persistKeys) {
        const val = this._getNested(this.config, key);
        if (val !== undefined) {
          this._setNested(toSave, key, val);
        }
      }
      localStorage.setItem('inject_config', JSON.stringify(toSave));
      return true;
    } catch (e) {
      console.error('[ConfigManager] Failed to save config to localStorage:', e);
      return false;
    }
  }

  _getNested(obj, path) {
    const parts = path.split('.');
    let current = obj;
    for (const part of parts) {
      if (current && typeof current === 'object') {
        current = current[part];
      } else {
        return undefined;
      }
    }
    return current;
  }

  _setNested(obj, path, value) {
    const parts = path.split('.');
    let current = obj;
    for (let i = 0; i < parts.length - 1; i++) {
      if (!current[parts[i]] || typeof current[parts[i]] !== 'object') {
        current[parts[i]] = {};
      }
      current = current[parts[i]];
    }
    current[parts[parts.length - 1]] = value;
  }

  deepMerge(target, source) {
    const result = JSON.parse(JSON.stringify(target));

    for (const key in source) {
      if (source[key] && typeof source[key] === 'object' && !Array.isArray(source[key])) {
        result[key] = this.deepMerge(result[key] || {}, source[key]);
      } else {
        result[key] = source[key];
      }
    }

    return result;
  }

  get(path) {
    const parts = path.split('.');
    let value = this.config;

    for (const part of parts) {
      if (value && typeof value === 'object') {
        value = value[part];
      } else {
        return undefined;
      }
    }

    return value;
  }

  set(path, value) {
    const parts = path.split('.');
    let obj = this.config;

    for (let i = 0; i < parts.length - 1; i++) {
      const part = parts[i];
      if (!obj[part] || typeof obj[part] !== 'object') {
        obj[part] = {};
      }
      obj = obj[part];
    }

    obj[parts[parts.length - 1]] = value;
    this.saveConfig();
    // Persist relay secrets to SecretStore when changed
    if (path === 'relay.apiKey' || path === 'relay.session') {
      this.saveSecrets();
    }
  }

  reset() {
    this.config = JSON.parse(JSON.stringify(this.defaults));
    this.saveConfig();
  }

  // Backward compatibility: flat access like CONFIG.agentPanelEnabled
  get agentPanelEnabled() { return this.config.panel.enabled; }
  get metadslMarkers() { return this.config.metadsl.markers; }
  get mutationDebounce() { return this.config.timing.mutationDebounce; }
  get mutationObserverRestartDelay() { return this.config.timing.mutationObserverRestartDelay; }
  get stateCheckInterval() { return this.config.timing.stateCheckInterval; }
  get llmRespondingCheckInterval() { return this.config.timing.llmRespondingCheckInterval; }
  get stateMachineLoopInterval() { return this.config.timing.stateMachineLoopInterval; }
  get sendMessageDelay() { return this.config.timing.sendMessageDelay; }
  get panelPosition() { return this.config.panel.position; }
  get panelWidth() { return this.config.panel.width; }
  get panelHeight() { return this.config.panel.height; }
  get panelMinWidth() { return this.config.panel.minWidth; }
  get panelMinHeight() { return this.config.panel.minHeight; }
  get panelResizeHandleSize() { return this.config.panel.resizeHandleSize; }
  get logLevels() { return this.config.logging.levels; }
  get consoleOutput() { return this.config.logging.consoleOutput; }
  set consoleOutput(val) { this.config.logging.consoleOutput = val; }
  get userInputTimeout() { return this.config.timing.userInputTimeout; }
  get llmContextCountModuloForKeep() { return this.config.metadsl.llmContextCountModuloForKeep; }
  get llmContextCountModuloForAlign() { return this.config.metadsl.llmContextCountModuloForAlign; }
  get websocketPort() { return this.config.websocket.port; }
  get websocketReconnectDelay() { return this.config.websocket.reconnectDelay; }
  get pageStableDelay() { return this.config.timing.pageStableDelay; }
  get operationDelay() { return this.config.timing.operationDelay; }
  get maxProcessedBlocks() { return this.config.metadsl.maxProcessedBlocks; }
  get maxSendRetries() { return this.config.metadsl.maxSendRetries; }
  get sendRetryDelay() { return this.config.timing.sendRetryDelay; }
  get operationExecuteTimeout() { return this.config.timing.operationExecuteTimeout; }
  get userTypingDelay() { return this.config.timing.userTypingDelay; }
  get maxLogLines() { return this.config.panel.maxLogLines; }
}

// Create global config instance
const CONFIG = new ConfigManager();
