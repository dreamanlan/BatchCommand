/**
 * inject.js - Agent Collaboration Framework
 *
 * This script is injected into the browser page to enable agent automation.
 * It provides:
 * - Communication bridge with C# backend
 * - Page operations (send messages, read responses)
 * - MetaDSL command detection and execution
 * - Visual control panel for debugging
 */
(function() {
  'use strict';

  // ============================================================================
  // Shared instances for window API (initialized in initializeAgent)
  // ============================================================================
  let bridge = null;
  let pageAdapter = null;
  let metadslMonitor = null;
  let panel = null;

  let gameWindow = null;

  // ============================================================================
  // Configuration
  // ============================================================================
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
          maxLogLines: 1024
        },

        // Timing settings
        timing: {
          stateCheckInterval: 50,
          llmRespondingCheckInterval: 500,
          stateMachineLoopInterval: 100,
          sendMessageDelay: 100,
          mutationDebounce: 2000,
          mutationObserverRestartDelay: 300,
          userInputTimeout: 10000,
          userTypingDelay: 2000,
          pageStableDelay: 5000,
          operationDelay: 500,
          sendRetryDelay: 1000
        },

        // MetaDSL settings
        metadsl: {
          agentReplyMarker: '【Agent回复】',
          markers: ['// @execute', '# @execute'],
          maxProcessedBlocks: 1000,
          maxSendRetries: 5,
          llmContextCountModulo: 10,
          llmContextCount: 64
        },

        // WebSocket settings
        websocket: {
          port: 9527,
          reconnectDelay: 3000
        },

        // Log level settings
        logging: {
          levels: {
            debug: false,
            info: true,
            warn: true,
            error: true
          }
        }
      };

      // Load configuration from localStorage or use defaults
      this.config = this.loadConfig();
    }

    loadConfig() {
      try {
        const stored = localStorage.getItem('inject_config');
        if (stored) {
          const parsed = JSON.parse(stored);
          // Merge with defaults to ensure all keys exist
          return this.deepMerge(this.defaults, parsed);
        }
      } catch (e) {
        console.warn('[ConfigManager] Failed to load config from localStorage:', e);
      }

      // Return deep copy of defaults
      return JSON.parse(JSON.stringify(this.defaults));
    }

    saveConfig() {
      try {
        localStorage.setItem('inject_config', JSON.stringify(this.config));
        return true;
      } catch (e) {
        console.error('[ConfigManager] Failed to save config to localStorage:', e);
        return false;
      }
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
    get userInputTimeout() { return this.config.timing.userInputTimeout; }
    get llmContextCountModulo() { return this.config.metadsl.llmContextCountModulo; }
    get llmContextCount() { return this.config.metadsl.llmContextCount; }
    get websocketPort() { return this.config.websocket.port; }
    get websocketReconnectDelay() { return this.config.websocket.reconnectDelay; }
    get pageStableDelay() { return this.config.timing.pageStableDelay; }
    get operationDelay() { return this.config.timing.operationDelay; }
    get maxProcessedBlocks() { return this.config.metadsl.maxProcessedBlocks; }
    get maxSendRetries() { return this.config.metadsl.maxSendRetries; }
    get sendRetryDelay() { return this.config.timing.sendRetryDelay; }
    get userTypingDelay() { return this.config.timing.userTypingDelay; }
    get maxLogLines() { return this.config.panel.maxLogLines; }
  }

  // Create global config instance
  const CONFIG = new ConfigManager();

  // ============================================================================
  // WebSocket Worker Code - Externalized for better maintainability
  // This code runs in a Web Worker context
  // ============================================================================
  const WEBSOCKET_WORKER_CODE = `
    // Worker internal state
    let ws = null;
    let isConnected = false;
    let reconnectAttempts = 0;
    const MAX_RECONNECT = 5;

    // Worker queues (dual queue mechanism)
    const toMainQueue = [];     // Messages to send to main thread
    const fromMainQueue = [];   // Messages received from main thread

    // Tick processing for queues
    let tickInterval = null;

    function log(level, message, data) {
      self.postMessage({
        type: 'log',
        level: level,
        message: message,
        data: data
      });
    }

    function connect(port) {
      const wsUrl = 'ws://localhost:' + port + '/';
      log('info', 'Connecting to WebSocket server: ' + wsUrl);

      try {
        ws = new WebSocket(wsUrl);

        ws.onopen = function() {
          isConnected = true;
          reconnectAttempts = 0;
          log('info', 'WebSocket connected to ' + wsUrl);
          self.postMessage({ type: 'connected' });
        };

        ws.onclose = function() {
          isConnected = false;
          log('info', 'WebSocket disconnected');
          self.postMessage({ type: 'disconnected' });

          // Attempt reconnection
          if (reconnectAttempts < MAX_RECONNECT) {
            reconnectAttempts++;
            log('info', 'Reconnecting... attempt ' + reconnectAttempts);
            setTimeout(function() { connect(port); }, 3000);
          }
        };

        ws.onerror = function(error) {
          log('error', 'WebSocket error', error);
          self.postMessage({ type: 'error', error: 'WebSocket error' });
        };

        ws.onmessage = function(event) {
          // Queue message to main thread
          toMainQueue.push(event.data);
          log('info', 'Message received from server, queued to main (length: ' + event.data.length + ')');
        };
      } catch (e) {
        log('error', 'Failed to create WebSocket', { error: e.message });
      }
    }

    function disconnect() {
      if (ws) {
        try {
          ws.close();
        } catch (e) {}
        ws = null;
      }
      isConnected = false;
      if (tickInterval) {
        clearInterval(tickInterval);
        tickInterval = null;
      }
      log('info', 'WebSocket disconnected by request');
    }

    function processQueues() {
      if (!isConnected) return;

      // Send one message per tick to server (from fromMainQueue)
      if (fromMainQueue.length > 0) {
        const msg = fromMainQueue.shift();
        if (ws && ws.readyState === WebSocket.OPEN) {
          ws.send(msg);
          log('info', 'Message sent to server (length: ' + msg.length + '): ' + msg.substring(0, 100) + '...');
        } else {
          log('warn', 'Cannot send message, WebSocket not open (state: ' + (ws ? ws.readyState : 'null') + ')');
          // Put message back to queue
          fromMainQueue.unshift(msg);
        }
      }
    }

    // Handle messages from main thread
    self.onmessage = function(event) {
      const data = event.data;

      if (data.type === 'start') {
        connect(data.port);
        // Start tick processing
        tickInterval = setInterval(processQueues, 100);
      } else if (data.type === 'stop') {
        disconnect();
      } else if (data.type === 'send') {
        // Queue message to send to server
        fromMainQueue.push(data.message);
        log('info', 'Message queued from main thread (length: ' + data.message.length + ')');
      }
    };

    // Send queued messages to main thread on each tick
    setInterval(function() {
      while (toMainQueue.length > 0) {
        const msg = toMainQueue.shift();
        self.postMessage({
          type: 'message',
          data: msg
        });
      }
    }, 100);
  `;

  // ============================================================================
  // Logger - Unified logging to console, panel, and C#
  // ============================================================================
  class Logger {
    constructor(prefix = '') {
      this.panel = null;
      this.prefix = prefix;

      // Log buffering for C# communication
      this.logBuffer = [];
      this.logBufferTimer = null;
      this.logBufferDelay = 1000; // Batch send every 1 second
      this.maxBufferSize = 50; // Max logs before forcing flush

      // Log sampling to prevent flooding
      this.lastLogTime = {};
      this.logSampleInterval = 100; // Min ms between same log messages
    }

    setPanel(panel) {
      this.panel = panel;
    }

    createLogger(prefix) {
      const childLogger = new Logger(prefix);
      childLogger.panel = this.panel;
      // Share buffer with parent logger
      childLogger.logBuffer = this.logBuffer;
      childLogger.logBufferTimer = this.logBufferTimer;
      childLogger.lastLogTime = this.lastLogTime;
      return childLogger;
    }

    truncateData(data, maxLength = 500) {
      if (!data) return null;

      try {
        const str = JSON.stringify(data);
        if (str.length <= maxLength) {
          return data;
        }

        // Return truncated string representation
        return str.substring(0, maxLength) + '... (truncated)';
      } catch (e) {
        return '[Circular or non-serializable data]';
      }
    }

    shouldSampleLog(message, level) {
      const key = `${level}:${message}`;
      const now = Date.now();
      const lastTime = this.lastLogTime[key] || 0;

      if (now - lastTime < this.logSampleInterval) {
        return false; // Skip this log (too frequent)
      }

      this.lastLogTime[key] = now;
      return true;
    }

    flushLogBuffer() {
      if (this.logBuffer.length === 0) return;

      if (typeof sendMessage !== 'undefined') {
        try {
          // Send all buffered logs in one batch
          sendMessage('debug_log_batch', JSON.stringify({
            logs: this.logBuffer,
            timestamp: Date.now()
          }));
          this.logBuffer = [];
        } catch (e) {
          console.error('[Logger] Failed to flush log buffer:', e);
          this.logBuffer = []; // Clear buffer to prevent memory leak
        }
      }

      if (this.logBufferTimer) {
        clearTimeout(this.logBufferTimer);
        this.logBufferTimer = null;
      }
    }

    scheduleFlush() {
      if (this.logBufferTimer) return;

      this.logBufferTimer = setTimeout(() => {
        this.flushLogBuffer();
      }, this.logBufferDelay);
    }

    log(message, level = 'info', data = null) {
      if (!CONFIG.logLevels[level]) return;

      // Sample frequent logs
      if (!this.shouldSampleLog(message, level)) {
        return;
      }

      const levelPrefixes = {
        debug: '🔍',
        info: '📝',
        warn: '⚠️',
        error: '❌'
      };

      const levelPrefix = levelPrefixes[level] || '📝';
      const prefixStr = this.prefix ? `[${this.prefix}] ` : '';

      // Truncate large data objects
      const truncatedData = this.truncateData(data);
      const fullMessage = truncatedData ? `${prefixStr}${message} ${JSON.stringify(truncatedData)}` : `${prefixStr}${message}`;

      // Log to console (commented out to avoid console.log file growth)
      // const consoleMethod = level === 'error' ? 'error' : level === 'warn' ? 'warn' : 'log';
      // console[consoleMethod](fullMessage);

      // Log to panel if available (with size limit)
      if (this.panel) {
        this.panel.log(`${levelPrefix} ${fullMessage}`);
      }

      // Buffer log for C# (batch send)
      if (typeof sendMessage !== 'undefined') {
        this.logBuffer.push({
          level: level,
          message: fullMessage,
          data: truncatedData,
          timestamp: Date.now()
        });

        // Force flush if buffer is full
        if (this.logBuffer.length >= this.maxBufferSize) {
          this.flushLogBuffer();
        } else {
          this.scheduleFlush();
        }
      }
    }

    debug(message, data = null) { this.log(message, 'debug', data); }
    info(message, data = null) { this.log(message, 'info', data); }
    warn(message, data = null) { this.log(message, 'warn', data); }
    error(message, data = null) { this.log(message, 'error', data); }
  }

  // Create global logger instance
  const logger = new Logger();

  // ============================================================================
  // AgentBridge - Communication Bridge with C#
  // ============================================================================
  class AgentBridge {
    constructor() {
      this.logger = logger.createLogger('AgentBridge');
      this.commandId = 0;
      this.callbacks = new Map();
      this.autoPlanEnabled = true; // Auto plan enabled by default

      // Initialize CEF native API
      this.initNativeApi();

      // Start queue processing timer (independent of debug panel)
      this.startQueueProcessingTimer();
    }

    startQueueProcessingTimer() {
      // Process queues every second
      this.queueProcessingTimer = setInterval(() => {
        try {
          this.sendCommand('handle_thread_queue', { });
        } catch (e) {
          this.logger.error('Error in queue processing', { error: e.toString() });
        }
      }, 1000);
      this.logger.info('Queue processing timer started');
    }

    initNativeApi() {
      // Check if CEF native API is available
      this.logger.debug('Checking native API...');
      this.logger.debug('typeof window.cefQuery:', { type: typeof window.cefQuery });
      this.logger.debug('typeof sendMessage:', { type: typeof sendMessage });

      if (typeof window.cefQuery === 'undefined' && typeof sendMessage === 'undefined') {
        this.logger.warn('CEF native API not found, using mock mode');
        this.nativeMode = false;
      } else {
        this.nativeMode = true;
        this.logger.info('Native API available, nativeMode = true');
      }
    }

    // Send command to C# backend
    sendCommand(cmd, params, callback) {
      this.logger.debug('sendCommand called', { cmd, params });
      this.logger.debug('nativeMode:', { nativeMode: this.nativeMode });
      this.logger.debug('typeof sendMessage:', { type: typeof sendMessage });

      if (!callback) {
        callback = () => {};
      }

      const commandId = ++this.commandId;
      this.callbacks.set(commandId, callback);

      const message = {
        id: commandId,
        command: cmd,
        params: params || {}
      };

      if (this.nativeMode && typeof sendMessage !== 'undefined') {
        // Use CEF native API - async call to flatten call stack
        this.logger.debug('Calling sendMessage with agent_command', { message });
        setTimeout(() => {
          sendMessage('agent_command', JSON.stringify(message));
        }, 0);
      } else {
        // Mock mode for testing
        this.logger.warn('Mock mode - command not sent', { message });
        setTimeout(() => callback(false, null, 'Native API not available'), 100);
      }

      return commandId;
    }

    // Send notification (no response expected)
    sendNotification(type, data) {
      const message = {
        type: type,
        data: data || {}
      };

      if (this.nativeMode && typeof sendMessage !== 'undefined') {
        // Async call to flatten call stack
        setTimeout(() => {
          sendMessage('agent_notification', JSON.stringify(message));
        }, 0);
      } else {
        this.logger.debug('Mock notification', { message });
      }
    }

    // Handle response from C#
    handleResponse(message) {
      try {
        const response = JSON.parse(message);
        const callback = this.callbacks.get(response.id);
        if (callback) {
          callback(response.success, response.data, response.error);
          this.callbacks.delete(response.id);
        }
      } catch (e) {
        this.logger.error('Error parsing response', { error: e.toString() });
      }
    }

    // Send agent_need_to_plan notification (encapsulated for reuse)
    sendAgentNeedToPlan(state, pageAdapter) {
      if (!this.autoPlanEnabled) {
        this.logger.debug('Auto plan disabled, skipping agent_need_to_plan notification', { state });
        return;
      }
      this.logger.info('Sending agent_need_to_plan notification', { state });
      this.sendNotification('agent_need_to_plan', {
        state: state,
        timestamp: Date.now(),
        lastFromLLM: pageAdapter ? pageAdapter.isLastMessageFromLLM() : false,
        lastMessage: pageAdapter ? pageAdapter.getLastResponse() : ''
      });
    }
  }

  // ============================================================================
  // User Input Monitor - Independent monitoring class
  // ============================================================================
  class UserInputMonitor {
    constructor(pageAdapter) {
      this.logger = logger.createLogger('UserInputMonitor');
      this.pageAdapter = pageAdapter;
      this.isMonitoring = false;
      this.hasUserInput = false;
      this.isUserTyping = false;
      this.userTypingTimer = null;
      this.userTypingDelay = CONFIG.userTypingDelay;
      this.inputElement = null;
      this.sendButton = null;

      // Event handlers (need to be bound for proper removal)
      this.handleFocus = this.onUserStartTyping.bind(this);
      this.handleInput = this.onUserStartTyping.bind(this);
      this.handleKeydown = this.onUserStartTyping.bind(this);
    }

    start() {
      // Always reset flags when starting
      this.hasUserInput = false;
      this.isUserTyping = false;

      if (this.isMonitoring) {
        this.logger.debug('Already monitoring, flags reset');
        return;
      }

      this.isMonitoring = true;

      // Find input element based on page type
      this.inputElement = this.findInputElement();

      if (this.inputElement) {
        this.inputElement.addEventListener('focus', this.handleFocus);
        this.inputElement.addEventListener('input', this.handleInput);
        this.inputElement.addEventListener('keydown', this.handleKeydown);
        this.logger.debug('Monitoring started on element', { element: this.inputElement.tagName });
        this.logger.debug('Page type', { pageType: this.pageAdapter.pageType });
      } else {
        this.logger.warn('Input element not found', { pageType: this.pageAdapter.pageType });
      }
    }

    stop() {
      if (!this.isMonitoring) {
        return;
      }

      this.isMonitoring = false;
      this.clearUserTypingTimer();

      if (this.inputElement) {
        this.inputElement.removeEventListener('focus', this.handleFocus);
        this.inputElement.removeEventListener('input', this.handleInput);
        this.inputElement.removeEventListener('keydown', this.handleKeydown);
      }

      this.logger.debug('Monitoring stopped');
    }

    findInputElement() {
      const pageType = this.pageAdapter.pageType;
      switch (pageType) {
        case 'local-agent':
          return document.getElementById('editable-content');
        case 'custom-llm':
          return document.getElementById('editable-content');
        default:
          return null;
      }
    }

    checkInputHasContent() {
      if (!this.inputElement) {
        this.inputElement = this.findInputElement();
      }
      if (!this.inputElement) return false;

      const content = this.inputElement.textContent || this.inputElement.value || '';
      return content.trim().length > 0;
    }

    onUserStartTyping() {
      if (!this.isMonitoring) {
        this.logger.debug('Event received but not monitoring');
        return;
      }

      this.isUserTyping = true;
      this.hasUserInput = true;
      this.logger.debug('User input detected! hasUserInput=true, isUserTyping=true');
      this.clearUserTypingTimer();

      this.userTypingTimer = setTimeout(() => {
        this.isUserTyping = false;
        this.logger.debug('User stopped typing (isUserTyping=false)');
      }, this.userTypingDelay);
    }

    clearUserTypingTimer() {
      if (this.userTypingTimer) {
        clearTimeout(this.userTypingTimer);
        this.userTypingTimer = null;
      }
    }

    reset() {
      this.hasUserInput = false;
      this.isUserTyping = false;
      this.clearUserTypingTimer();
    }
  }

  // ============================================================================
  // State Machine Pattern - State Classes
  // ============================================================================

  // Base State class
  class State {
    constructor(monitor) {
      this.monitor = monitor;
    }

    // Lifecycle methods
    enter() {}
    exit() {}

    // Main loop for this state
    async run() {}

    // Shared LLM detection methods
    checkLLMResponding() {
      const pageType = this.monitor.pageAdapter.pageType;

      switch (pageType) {
        case 'local-agent':
          return this.checkLocalAgentResponding();
        case 'custom-llm':
          return this.checkCustomLLMResponding();
        default:
          return false;
      }
    }

    checkLocalAgentResponding() {
      // Check for loading indicator in local-agent
      const loadingIcon = document.querySelector('.el-icon.is-loading');
      const loadingIndicator = document.querySelector('#loading-indicator.active');
      return loadingIcon !== null || loadingIndicator !== null;
    }

    checkCustomLLMResponding() {
      // Check for loading indicator in both custom-llm and local-agent
      const loadingIcon = document.querySelector('.el-icon.is-loading');
      const loadingIndicator = document.querySelector('#loading-indicator.active');
      return loadingIcon !== null || loadingIndicator !== null;
    }

    // Utility methods
    debug(message) {
      this.monitor.debug(`[${this.constructor.name}] ${message}`);
    }

    info(message) {
      this.monitor.info(`[${this.constructor.name}] ${message}`);
    }

    warn(message) {
      this.monitor.warn(`[${this.constructor.name}] ${message}`);
    }

    error(message) {
      this.monitor.error(`[${this.constructor.name}] ${message}`);
    }

    sleep(ms) {
      return new Promise(resolve => setTimeout(resolve, ms));
    }
  }

  // User Input State - manages user typing detection
  class UserInputState extends State {
    constructor(monitor) {
      super(monitor);
      this.enterTime = 0;
      this.lastInputTime = 0;
      this.handleInput = this.onInput.bind(this);
    }

    onInput() {
      // Reset timeout by updating last input time
      this.lastInputTime = Date.now();
    }

    enter() {
      this.info('Entering user input state');
      this.enterTime = Date.now();
      this.lastInputTime = Date.now();

      // Start user input monitoring
      this.monitor.userInputMonitor.start();

      // Setup input listener to reset timeout
      const inputArea = document.querySelector('.vac-textarea');
      if (inputArea) {
        inputArea.addEventListener('input', this.handleInput);
        inputArea.addEventListener('keydown', this.handleInput);
      }
    }

    exit() {
      this.info('Exiting user input state');

      // Stop user input monitoring
      this.monitor.userInputMonitor.stop();

      // Remove input listener
      const inputArea = document.querySelector('.vac-textarea');
      if (inputArea) {
        inputArea.removeEventListener('input', this.handleInput);
        inputArea.removeEventListener('keydown', this.handleInput);
      }
    }

    async run() {
      while (this.monitor.currentStateName === 'USER_INPUT') {
        await this.sleep(CONFIG.stateCheckInterval);

        if (!this.monitor.userInputMonitor.checkInputHasContent()) {
          // Check if LLM started responding
          const isLLMResponding = this.checkLLMResponding();
          if (isLLMResponding) {
            this.info('Detected LLM started responding, transitioning to LLM_RESPONDING');
            this.monitor.transitionTo('LLM_RESPONDING', 'LLM started responding');
            break;
          }
        }

        // Check timeout: timeout means no input activity for specified duration
        const currentTime = Date.now();
        const timeSinceLastInput = currentTime - this.lastInputTime;

        if (timeSinceLastInput >= CONFIG.userInputTimeout) {
          // Check if input has content
          const hasContent = this.monitor.userInputMonitor.checkInputHasContent();
          if (!hasContent) {
            this.info(`User input timeout (${CONFIG.userInputTimeout / 1000}s with empty input), transitioning back to AGENT_EXECUTING`);
            this.monitor.transitionTo('AGENT_EXECUTING', 'User input timeout');
            break;
          } else {
            // Input has content but no activity, still timeout
            this.info(`User input timeout (${CONFIG.userInputTimeout / 1000}s with content but no activity), transitioning back to AGENT_EXECUTING`);
            this.monitor.transitionTo('AGENT_EXECUTING', 'User input timeout with content');
            break;
          }
        }
      }
    }
  }

  // LLM Responding State - monitors LLM response
  class LLMRespondingState extends State {
    constructor(monitor) {
      super(monitor);
      // Counter for context countdown, persists across state transitions
      this.contextCounter = 0;
    }

    enter() {
      this.info('Entering LLM responding state');
      // User must have sent a message to trigger LLM response
      this.monitor.onUserSendMessage();
      // Record the last user message node after a short delay (wait for DOM render)
      setTimeout(() => {
        const userMsgBoxes = document.querySelectorAll('.vac-message-box.vac-offset-current');
        this.monitor.pendingAgentMessageWrapper = userMsgBoxes.length > 0 ? userMsgBoxes[userMsgBoxes.length - 1] : null;
        this.info(`Recorded pending agent message wrapper, found ${userMsgBoxes.length} user messages`);
      }, 500);

      // Increment context counter and check for countdown
      this.contextCounter = (this.contextCounter + 1) % CONFIG.llmContextCountModulo;
      this.debug(`Context counter: ${this.contextCounter}`);

      // When counter reaches 0, trigger context countdown notification
      if (this.contextCounter === 0) {
        this.info('Context counter reached 0, align target');
        this.monitor.alignTarget();
      }
    }

    exit() {
      this.info('Exiting LLM responding state');
      // Replace pending agent reply message with collapsed text
      const msgBox = this.monitor.pendingAgentMessageWrapper;
      if (msgBox && this.monitor.isAgentMessage(msgBox)) {
        this.info(`Collapsing agent reply, msgBox tagName=${msgBox.tagName}, className=${msgBox.className}`);
        msgBox.textContent = '[Agent reply omitted]';
        this.monitor.pendingAgentMessageWrapper = null;
        this.info('Collapsed agent reply message');
      } else {
        if (msgBox) this.info('Pending message is not an agent message, skip collapse');
        else this.info('No pending agent message wrapper to collapse');
        this.monitor.pendingAgentMessageWrapper = null;
      }
    }

    async run() {
      this.info(`[LLMRespondingState] Starting run loop, pageType=${this.monitor.pageAdapter.pageType}`);

      while (this.monitor.currentStateName === 'LLM_RESPONDING') {
        await this.sleep(CONFIG.llmRespondingCheckInterval);

        const isResponding = this.checkLLMResponding();
        const lastFromLLM = this.monitor.pageAdapter.isLastMessageFromLLM();
        this.debug(`[LLMRespondingState] Checking LLM status: isResponding=${isResponding}, lastFromLLM=${lastFromLLM}`);

        if (!isResponding && lastFromLLM) {
          this.info('[LLMRespondingState] LLM finished responding, transitioning to AGENT_EXECUTING');
          this.monitor.transitionTo('AGENT_EXECUTING', 'LLM finished responding');
          break;
        }
      }

      this.info('[LLMRespondingState] Run loop exited');
    }
  }

  // Agent Executing State - executes operations
  class AgentExecutingState extends State {
    constructor(monitor) {
      super(monitor);
    }

    enter() {
      this.info('Entering agent executing state');

      // Clear state-specific data
      this.operationExecuted = false;

      // Reset user input monitor flags before starting
      this.monitor.userInputMonitor.reset();

      // Start user input monitoring
      this.monitor.userInputMonitor.start();
    }

    exit() {
      this.info('Exiting agent executing state');

      // Stop user input monitoring
      this.monitor.userInputMonitor.stop();

      // Clear state-specific data
      this.operationExecuted = false;
    }

    async run() {
      while (this.monitor.currentStateName === 'AGENT_EXECUTING') {
        await this.sleep(CONFIG.stateCheckInterval);

        // 1. Check if user has input (highest priority)
        const hasInput = this.monitor.userInputMonitor.hasUserInput;
        const isTyping = this.monitor.userInputMonitor.isUserTyping;
        const hasContent = this.monitor.userInputMonitor.checkInputHasContent();
        if (hasInput || isTyping || hasContent) {
          this.info(`User input detected (hasInput=${hasInput}, isTyping=${isTyping}, hasContent=${hasContent}), transitioning to USER_INPUT`);
          this.monitor.transitionTo('USER_INPUT', 'User started typing');
          break;
        }

        // 2. Check if LLM is responding
        const isLLMResponding = this.checkLLMResponding();
        if (isLLMResponding) {
          this.info('LLM is responding, transitioning to LLM_RESPONDING');
          this.monitor.transitionTo('LLM_RESPONDING', 'LLM started responding');
          break;
        }

        // 3. Check MetaDSL Worker receive queue for execution results
        if (this.monitor.metadslWorker && this.monitor.metadslWorker.isRunning) {
          const message = this.monitor.metadslWorker.dequeueMessage();
          if (message) {
            this.info('Received MetaDSL execution result from C#');
            // Send result directly to LLM (message is the execution result)
            this.monitor.sendResultToLLM(message);
            continue; // Continue processing more results
          }
        }

        if (this.operationExecuted && this.monitor.operationQueue.length > 0) {
          this.info(`Operation executed, waiting for next cycle`);
          this.monitor.transitionTo('LLM_RESPONDING', 'Agent operation completed');
          break;
        }

        // 4. Execute one operation if not executed yet
        if (!this.operationExecuted && this.monitor.operationQueue.length > 0) {
          // Stop monitoring before executing operation
          this.monitor.userInputMonitor.stop();

          try {
            const shouldMarkAsExecuted = this.executeOperation();
            this.operationExecuted = shouldMarkAsExecuted;
          } finally {
            // Restart monitoring after operation completes
            this.monitor.userInputMonitor.start();
            // Reset the hasUserInput flag since we just executed an operation
            this.monitor.userInputMonitor.reset();
          }
        }

        // 5. Check if agent needs to plan (queue empty and no operation executed)
        if (!this.operationExecuted && this.monitor.operationQueue.length === 0) {
          var handled = false;
          for (var ix = 0; ix < 10; ++ix) {
            await this.sleep(CONFIG.stateCheckInterval);
            const hasInput2 = this.monitor.userInputMonitor.hasUserInput;
            const isTyping2 = this.monitor.userInputMonitor.isUserTyping;
            const hasContent2 = this.monitor.userInputMonitor.checkInputHasContent();
            if (hasInput2 || isTyping2 || hasContent2) {
              this.info(`User input detected (hasInput=${hasInput2}, isTyping=${isTyping2}, hasContent=${hasContent2}), transitioning to USER_INPUT`);
              this.monitor.transitionTo('USER_INPUT', 'User started typing');
              handled = true;
              break;
            }
            const isLLMResponding = this.checkLLMResponding();
            if (isLLMResponding) {
              this.info('LLM is responding, transitioning to LLM_RESPONDING');
              this.monitor.transitionTo('LLM_RESPONDING', 'LLM started responding');
              handled = true;
              break;
            }
          }
          if (!handled) {
            // Send notification to Script.dsl to trigger planning
            this.monitor.bridge.sendAgentNeedToPlan('AGENT_EXECUTING', this.monitor.pageAdapter);
            // Mark as executed to avoid sending notification repeatedly
            this.operationExecuted = true;
          }
        }
      }
    }

    executeOperation() {
      if (this.monitor.operationQueue.length === 0) {
        this.info('No operations in queue');
        return true; // No operation to execute, mark as executed
      }

      const operation = this.monitor.operationQueue.shift();
      this.info(`Executing operation: ${operation.type}`);

      try {
        if (operation.type === 'execute') {
          // Mark as executed
          operation.block.dataset.metadslStatus = 'executed';

          // Execute the code
          this.info('Executing MetaDSL code block...');
          this.monitor.executeCommand(operation.code);

          // Add visual indicator
          this.monitor.addVisualIndicator(operation.block, 'executed');

          // Schedule hiding the container after 3 seconds
          this.monitor.scheduleHideContainer(operation.block);

          this.info('✓ Executed code block:', operation.blockId);

          // Update state display after operation completes
          if (window.agentPanel) {
            window.agentPanel.updateStateDisplay();
          }

          // Return true to mark as executed (wait for next cycle)
          return true;
        } else if (operation.type === 'mark_history') {
          // Mark as history
          operation.block.dataset.metadslStatus = 'history';

          // Add visual indicator
          this.monitor.addVisualIndicator(operation.block, 'history');

          // Schedule hiding the container after 3 seconds
          this.monitor.scheduleHideContainer(operation.block);

          this.info('✓ Marked as history:', operation.blockId);

          // Update state display after operation completes
          if (window.agentPanel) {
            window.agentPanel.updateStateDisplay();
          }

          // Return false to continue processing next operation immediately
          return false;
        } else if (operation.type === 'send_notification') {
          // Send notification to C#
          this.info(`Sending notification: ${operation.notificationType}`);

          this.monitor.bridge.sendNotification(operation.notificationType, operation.data);

          this.info(`✓ Sent notification: ${operation.notificationType}`);

          // Return true to mark as executed (wait for next cycle)
          return true;
        }
      } catch (error) {
        this.info(`Error executing operation: ${error.message}`);
        this.error('Error executing operation:', error);
        // On error, mark as executed to avoid infinite loop
        return true;
      }

      // Default: mark as executed
      return true;
    }
  }

  // ============================================================================
  // Base Page Adapter
  // ============================================================================
  class PageAdapter {
    constructor(bridge) {
      this.logger = logger.createLogger('PageAdapter');
      this.bridge = bridge;
      this.pageType = this.detectPageType();
      this.lastResponse = null;
      this.messageHistory = [];
      this.onPageTypeChanged = null; // Callback for page type changes

      // If detection failed, retry after DOM is fully loaded
      if (this.pageType === 'unknown') {
        if (document.readyState === 'loading') {
          document.addEventListener('DOMContentLoaded', () => {
            this.pageType = this.detectPageType();
            this.logger.debug('Re-detected page type after DOMContentLoaded', { pageType: this.pageType });
            if (this.onPageTypeChanged) {
              this.onPageTypeChanged(this.pageType);
            }
          });
        } else {
          // DOM already loaded, retry after a short delay
          const f = () => {
            const newType = this.detectPageType();
            if (newType !== 'unknown') {
              this.pageType = newType;
              this.logger.debug('Re-detected page type after delay', { pageType: this.pageType });
              if (this.onPageTypeChanged) {
                this.onPageTypeChanged(this.pageType);
              }
            }
            else {
              setTimeout(f, 500);
            }
          };
          setTimeout(f, 500);
        }
      }
    }

    detectPageType() {
      // Detect current page type based on URL or DOM
      // Priority: DOM-based detection first (more specific), then URL-based
      const hostname = window.location.hostname;
      const pathname = window.location.pathname;
      const href = window.location.href;

      // 1. Check for local agent (agent.html)
      // Detect by specific DOM structure: has both #editable-content and #messages-area
      if (document.querySelector('#editable-content[contenteditable="true"]') &&
          document.querySelector('#messages-area')) {
        return 'local-agent';
      }

      // 2. Check for custom LLM interface by DOM (simplified selector)
      // Try multiple selectors to be more flexible
      if (document.querySelector('#editable-content[contenteditable="true"]') ||
          document.querySelector('#editable-content.editable-content') ||
          document.querySelector('.editable-content[contenteditable="true"]')) {
        return 'custom-llm';
      }

      // 3. Fallback to unknown
      return 'unknown';
    }

    sendMessage(text) {
      throw new Error('sendMessage must be implemented by subclass');
    }

    getLastResponse() {
      return this.lastResponse;
    }

    observeResponseChanges(callback) {
      throw new Error('observeResponseChanges must be implemented by subclass');
    }

    getHistory(count) {
      // Get the last N conversation pairs (question-answer)
      // Returns a formatted string with plain text (no HTML/Markdown)
      try {
        const conversations = this.extractConversations(count);
        if (conversations.length === 0) {
          return '';
        }

        // Format conversations as plain text
        const formatted = conversations.map((conv, index) => {
          var userText = this.cleanText(conv.user);
          const assistantText = this.cleanText(conv.assistant);
          return `[Conversation ${index + 1}]\nUser:\n${userText}\n\nAssistant:\n${assistantText}`;
        }).join('\n\n' + '='.repeat(80) + '\n\n');

        return formatted;
      } catch (e) {
        this.logger.error('Error getting history', { error: e.toString() });
        return '';
      }
    }

    getVisibleText(el) {
      // Collect text from visible nodes only (skip display:none elements)
      let text = '';
      for (const node of el.childNodes) {
        if (node.nodeType === Node.TEXT_NODE) {
          text += node.textContent;
        } else if (node.nodeType === Node.ELEMENT_NODE) {
          if (node.style && node.style.display === 'none') continue;
          if (node.style && node.style.visibility === 'hidden') continue;
          text += this.getVisibleText(node);
        }
      }
      return text;
    }

    extractConversations(count) {
      // Extract conversation pairs based on page type
      // Should be overridden by subclasses for specific implementations
      switch (this.pageType) {
        case 'local-agent':
          return this.extractLocalAgentConversations(count);
        case 'custom-llm':
          return this.extractCustomLLMConversations(count);
        default:
          this.logger.warn('Unsupported page type for history extraction', { pageType: this.pageType });
          return [];
      }
    }

    extractLocalAgentConversations(count) {
      // Extract local agent conversations
      // Look for .vac-message-wrapper which contains each message
      const messageWrappers = Array.from(document.querySelectorAll('.vac-message-wrapper'));

      const conversations = [];
      let currentPair = null;

      // Process messages in order
      for (const wrapper of messageWrappers) {
        // Check if this is a user message by looking for .vac-offset-current class in .vac-message-box
        const messageBox = wrapper.querySelector('.vac-message-box');
        if (!messageBox) continue;

        const isUser = messageBox.classList.contains('vac-offset-current');

        if (isUser) {
          // Start new conversation pair
          if (currentPair && currentPair.user && currentPair.assistant) {
            conversations.push(currentPair);
          }
          // Use placeholder for agent-injected messages, real text for human user messages
          const userText = messageBox.dataset.agentCollapsed ? '...' : this.getVisibleText(wrapper);
          currentPair = { user: userText, assistant: '' };
        } else if (currentPair) {
          // Assistant message - LLM reply, read visible text
          currentPair.assistant = this.getVisibleText(wrapper);
        }
      }

      // Add last pair if complete
      if (currentPair && currentPair.user && currentPair.assistant) {
        conversations.push(currentPair);
      }

      // Return last N conversations
      return conversations.slice(-count);
    }

    extractCustomLLMConversations(count) {
      // Extract custom LLM conversations
      // Look for .vac-message-wrapper which contains each message
      const messageWrappers = Array.from(document.querySelectorAll('.vac-message-wrapper'));

      const conversations = [];
      let currentPair = null;

      // Process messages in order
      for (const wrapper of messageWrappers) {
        // Check if this is a user message by looking for .vac-offset-current class in .vac-message-box
        const messageBox = wrapper.querySelector('.vac-message-box');
        if (!messageBox) continue;

        const isUser = messageBox.classList.contains('vac-offset-current');

        if (isUser) {
          // Start new conversation pair
          if (currentPair && currentPair.user && currentPair.assistant) {
            conversations.push(currentPair);
          }
          // Use placeholder for agent-injected messages, real text for human user messages
          const userText = messageBox.dataset.agentCollapsed ? '...' : this.getVisibleText(wrapper);
          currentPair = { user: userText, assistant: '' };
        } else if (currentPair) {
          // Assistant message - LLM reply, read visible text
          currentPair.assistant = this.getVisibleText(wrapper);
        }
      }

      // Add last pair if complete
      if (currentPair && currentPair.user && currentPair.assistant) {
        conversations.push(currentPair);
      }

      // Return last N conversations
      return conversations.slice(-count);
    }

    /**
     * Check if the last message in the conversation is from LLM (assistant)
     * @returns {boolean} true if last message is from LLM, false if from user or unknown
     */
    isLastMessageFromLLM() {
      switch (this.pageType) {
        case 'local-agent':
          return this.isLastMessageFromLLM_LocalAgent();
        case 'custom-llm':
          return this.isLastMessageFromLLM_CustomLLM();
        default:
          return false;
      }
    }

    isLastMessageFromLLM_LocalAgent() {
      const messageWrappers = document.querySelectorAll('.vac-message-wrapper');
      if (messageWrappers.length === 0) return false;

      const lastWrapper = messageWrappers[messageWrappers.length - 1];
      const messageBox = lastWrapper.querySelector('.vac-message-box');
      if (!messageBox) return false;

      // If it has vac-offset-current, it's a user message
      const isUser = messageBox.classList.contains('vac-offset-current');
      return !isUser;
    }

    isLastMessageFromLLM_CustomLLM() {
      const messageWrappers = document.querySelectorAll('.vac-message-wrapper');
      if (messageWrappers.length === 0) return false;

      const lastWrapper = messageWrappers[messageWrappers.length - 1];
      const messageBox = lastWrapper.querySelector('.vac-message-box');
      if (!messageBox) return false;

      // If it has vac-offset-current, it's a user message
      const isUser = messageBox.classList.contains('vac-offset-current');
      return !isUser;
    }

    cleanText(text) {
      if (!text) return '';

      // Remove HTML tags
      let cleaned = text.replace(/<[^>]*>/g, '');

      // Remove Markdown formatting
      // Remove bold/italic: **text** or __text__ or *text* or _text_
      cleaned = cleaned.replace(/(\*\*|__)(.*?)\1/g, '$2');
      cleaned = cleaned.replace(/(\*|_)(.*?)\1/g, '$2');

      // Remove headers: # Header
      cleaned = cleaned.replace(/^#{1,6}\s+/gm, '');

      // Remove inline code: `code`
      cleaned = cleaned.replace(/`([^`]+)`/g, '$1');

      // Remove code blocks: ```code```
      cleaned = cleaned.replace(/```[\s\S]*?```/g, (match) => {
        // Keep code block content but remove markers
        return match.replace(/```[^\n]*\n?/g, '').replace(/```$/g, '');
      });

      // Remove links: [text](url)
      cleaned = cleaned.replace(/\[([^\]]+)\]\([^\)]+\)/g, '$1');

      // Remove images: ![alt](url)
      cleaned = cleaned.replace(/!\[([^\]]*)\]\([^\)]+\)/g, '$1');

      // Normalize whitespace but preserve newlines
      // Replace multiple spaces with single space
      cleaned = cleaned.replace(/ +/g, ' ');

      // Replace tabs with 2 spaces
      cleaned = cleaned.replace(/\t/g, '  ');

      // Remove leading/trailing whitespace from each line
      cleaned = cleaned.split('\n').map(line => line.trim()).join('\n');

      // Remove excessive blank lines (more than 2 consecutive)
      cleaned = cleaned.replace(/\n{3,}/g, '\n\n');

      // Trim overall
      cleaned = cleaned.trim();

      return cleaned;
    }
  }

  // ============================================================================
  // LLM Page Adapter (for local agent and custom LLM interfaces)
  // ============================================================================
  class LLMPageAdapter extends PageAdapter {
    constructor(bridge) {
      super(bridge);
      this.logger = logger.createLogger('LLMPageAdapter');
      this.observer = null;
      this.responseCallbacks = [];
    }

    sendMessage(text) {
      switch (this.pageType) {
        case 'local-agent':
          this.sendLocalAgentMessage(text);
          break;
        case 'custom-llm':
          this.sendCustomLLMMessage(text);
          break;
        case 'test':
          this.sendTestMessage(text);
          break;
        default:
          this.logger.warn('Unsupported page type', { pageType: this.pageType });
      }
    }

    sendTestMessage(text) {
      // Test page specific implementation
      const inputArea = document.getElementById('userInput');
      const outputArea = document.getElementById('simulatedResponse');

      if (inputArea && outputArea) {
        inputArea.value = text;

        // Simulate response
        setTimeout(() => {
          const response = document.createElement('div');
          response.innerHTML = `<p><strong>Simulated AI:</strong> ${text}</p>`;
          outputArea.appendChild(response);
          this.lastResponse = text;
          this.notifyResponseChange(text);
        }, 500);
      }
    }

    sendLocalAgentMessage(text) {
      const input = document.getElementById('editable-content');
      if (!input) {
        this.logger.warn('Local Agent input element #editable-content not found');
        return;
      }

      // Set auto context before sending message (local-agent specific)
      if (window.AgentLLM && window.AgentLLM.getAutoContext) {
        try {
          const autoContext = window.AgentLLM.getAutoContext();
          if (autoContext) {
            window.AgentLLM.setHiddenContext([
              'Recent conversation history for context:',
              autoContext
            ]);
            this.logger.debug('Auto context set for inject.js', { length: autoContext.length });
          }
        } catch (e) {
          this.logger.warn('Failed to set auto context', { error: e.toString() });
        }
      }

      // Set message content
      input.textContent = text;
      input.dispatchEvent(new Event('input', { bubbles: true }));

      // Click send button after a short delay
      setTimeout(() => {
        const btn = document.querySelector('.vac-icon-textarea .vac-svg-button:last-child');
        if (btn) {
          btn.classList.remove('vac-send-disabled');
          btn.click();
          this.logger.debug('Local Agent message sent', { text });
        } else {
          this.logger.warn('Send button not found');
        }
      }, CONFIG.sendMessageDelay);
    }

    sendCustomLLMMessage(text) {
      const input = document.getElementById('editable-content');
      if (!input) {
        this.logger.warn('Custom LLM input element #editable-content not found');
        return;
      }

      // Set message content
      input.textContent = text;
      input.dispatchEvent(new Event('input', { bubbles: true }));

      // Click send button after a short delay
      setTimeout(() => {
        const btn = document.querySelector('.vac-icon-textarea .vac-svg-button:last-child');
        if (btn) {
          btn.classList.remove('vac-send-disabled');
          btn.click();
          this.logger.debug('Custom LLM message sent', { text });
        } else {
          this.logger.warn('Send button not found');
        }
      }, CONFIG.sendMessageDelay);
    }

    observeResponseChanges(callback) {
      this.responseCallbacks.push(callback);

      if (this.observer) return; // Already observing

      // Use MutationObserver to detect new messages
      const targetNode = document.body;
      const config = { childList: true, subtree: true };

      this.observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
          if (mutation.addedNodes.length > 0) {
            // Check if new response elements were added
            const newResponse = this.extractLatestResponse();
            if (newResponse && newResponse !== this.lastResponse) {
              this.lastResponse = newResponse;
              this.messageHistory.push({
                timestamp: Date.now(),
                content: newResponse
              });
              this.notifyResponseChange(newResponse);
            }
          }
        });
      });

      this.observer.observe(targetNode, config);
    }

    extractLatestResponse() {
      switch (this.pageType) {
        case 'local-agent':
          return this.extractLocalAgentResponse();
        case 'custom-llm':
          return this.extractCustomLLMResponse();
        case 'test':
          return this.extractTestResponse();
        default:
          return null;
      }
    }

    extractTestResponse() {
      const outputArea = document.getElementById('simulatedResponse');
      if (outputArea) {
        return outputArea.textContent;
      }
      return null;
    }

    extractLocalAgentResponse() {
      // Extract latest response from local agent interface
      // Look for message containers in the chat area
      const messages = document.querySelectorAll('.vac-message-wrapper, .message-wrapper, [class*="message"]');

      if (messages.length > 0) {
        // Get the last message that's not from the user
        for (let i = messages.length - 1; i >= 0; i--) {
          const msg = messages[i];
          // Skip user messages (usually have specific classes)
          if (!msg.classList.contains('user-message') &&
              !msg.classList.contains('vac-message-current')) {
            return msg.textContent;
          }
        }
      }

      return null;
    }

    extractCustomLLMResponse() {
      // Extract latest response from custom LLM interface
      // Look for message containers in the chat area
      const messages = document.querySelectorAll('.vac-message-wrapper, .message-wrapper, [class*="message"]');

      if (messages.length > 0) {
        // Get the last message that's not from the user
        for (let i = messages.length - 1; i >= 0; i--) {
          const msg = messages[i];
          // Skip user messages (usually have specific classes)
          if (!msg.classList.contains('user-message') &&
              !msg.classList.contains('vac-message-current')) {
            return msg.textContent;
          }
        }
      }

      return null;
    }

    notifyResponseChange(response) {
      this.responseCallbacks.forEach(callback => {
        try {
          callback(response);
        } catch (e) {
          this.logger.error('Error in response callback', { error: e.toString() });
        }
      });
    }

    setSystemPrompt(promptText) {
      try {
        // Check if this is local-agent page
        if (this.pageType === 'local-agent') {
          // Call window.setDialogPrompt if available
          if (typeof window.setDialogPrompt === 'function') {
            window.setDialogPrompt(promptText);
            this.logger.debug('System prompt set via window.setDialogPrompt', { length: promptText.length });
            return true;
          } else {
            this.logger.warn('window.setDialogPrompt not available on local-agent page');
            return false;
          }
        }

        // Original logic for other page types
        // Find the label with text 'system prompt:'
        const label = [...document.querySelectorAll('label.el-form-item__label')]
          .find(l => l.textContent.trim() === 'system prompt:');

        if (!label) {
          this.logger.warn('System prompt label not found');
          return false;
        }

        // Get the textarea ID from the label's 'for' attribute
        const textareaId = label.getAttribute('for');
        if (!textareaId) {
          this.logger.warn('Textarea ID not found in label');
          return false;
        }

        // Get the textarea element
        const textarea = document.getElementById(textareaId);
        if (!textarea) {
          this.logger.warn('System prompt textarea not found', { textareaId });
          return false;
        }

        // Set the value and trigger input event
        textarea.value = promptText;
        textarea.dispatchEvent(new Event('input', { bubbles: true }));

        this.logger.debug('System prompt set successfully', { length: promptText.length });
        return true;
      } catch (e) {
        this.logger.error('Failed to set system prompt', { error: e.toString() });
        return false;
      }
    }
  }

  // ============================================================================
  // MetaDSLMonitor - Monitor for MetaDSL commands in messages
  // ============================================================================
  class MetaDSLMonitor {
    constructor(bridge, pageAdapter, metadslWorker) {
      this.logger = logger.createLogger('MetaDSLMonitor');
      this.bridge = bridge;
      this.pageAdapter = pageAdapter;
      this.metadslWorker = metadslWorker;
      this.started = false; // Only controlled by panel start/stop
      this.processedMessages = new Set();
      this.processedBlocks = new Set();
      this.panel = null;
      this.observer = null;
      this.sendButtonObserver = null;
      this.canExecuteNewCommands = false; // Only execute when send button is enabled
      this.maxProcessedBlocks = CONFIG.maxProcessedBlocks;
      this.isMarkingHistory = false; // Flag to prevent observer from triggering during history marking
      this.isInitializing = true; // Flag to indicate initialization phase, prevents queueing history blocks

      // Operation queue mechanism
      this.operationQueue = [];
      this.pageStableTimer = null;
      this.isProcessingQueue = false;
      this.pageStableDelay = CONFIG.pageStableDelay;
      this.operationDelay = CONFIG.operationDelay;

      // Block ID cache for stable ID generation
      this.blockIdCache = new WeakMap();
      this.nextBlockId = 0;

      // Track last content of code blocks to detect if still changing
      this.lastBlockContent = new Map();

      // Create user input monitor
      this.userInputMonitor = new UserInputMonitor(pageAdapter);

      // State machine - using State Pattern
      this.states = {
        USER_INPUT: new UserInputState(this),
        LLM_RESPONDING: new LLMRespondingState(this),
        AGENT_EXECUTING: new AgentExecutingState(this)
      };
      this.currentState = null;
      this.currentStateName = null; // Will be set by transitionTo in start()
      this.isTransitioning = false; // Prevent concurrent state transitions
      this.stateHistory = [];
      this.maxSendRetries = CONFIG.maxSendRetries;
      this.sendRetryDelay = CONFIG.sendRetryDelay;
    }

    injectStyles() {
      // Check if styles already injected
      if (document.getElementById('metadsl-monitor-styles')) {
        return;
      }

      const style = document.createElement('style');
      style.id = 'metadsl-monitor-styles';
      style.textContent = `
        /* MetaDSL Monitor Visual Indicators */
        .metadsl-executed,
        pre:has(> code[data-metadsl-status="executed"]),
        div:has(> code[data-metadsl-status="executed"]) {
          border-left: 3px solid #4CAF50 !important;
          background-color: rgba(76, 175, 80, 0.05) !important;
        }

        .metadsl-history,
        pre:has(> code[data-metadsl-status="history"]),
        div:has(> code[data-metadsl-status="history"]) {
          border-left: 3px solid #9E9E9E !important;
          background-color: rgba(158, 158, 158, 0.05) !important;
        }
      `;

      document.head.appendChild(style);
      this.info('CSS styles injected');
    }

    start() {
      if (this.started) {
        this.warn('Already started');
        return;
      }

      this.info('Starting...');
      this.started = true;
      this.enabled = true;

      // Initialize state machine with State Pattern
      this.transitionTo('AGENT_EXECUTING', 'Initial state');

      // Inject CSS styles for visual indicators
      this.injectStyles();

      // Check initial send button state
      this.checkSendButtonState();

      // Start monitoring send button state
      this.startSendButtonMonitor();

      // Start observing response changes
      this.pageAdapter.observeResponseChanges((response) => {
        this.handleNewResponse(response);
      });

      // Watch for new code blocks (only execute new ones)
      this.startCodeBlockObserver();

      // Start state machine execution loop
      this.runStateMachine();
    }

    stop() {
      if (!this.started) {
        this.warn('Not started');
        return;
      }

      this.info('Stopping...');
      this.started = false;
      this.enabled = false;
      this.canExecuteNewCommands = false;
      this.processedMessages.clear();
      this.processedBlocks.clear();

      // Clear operation queue
      this.operationQueue = [];

      if (this.pageStableTimer) {
        clearTimeout(this.pageStableTimer);
        this.pageStableTimer = null;
      }

      if (this.userTypingTimer) {
        clearTimeout(this.userTypingTimer);
        this.userTypingTimer = null;
      }
      this.isUserTyping = false;

      if (this.observer) {
        this.observer.disconnect();
        this.observer = null;
      }

      if (this.sendButtonObserver) {
        this.sendButtonObserver.disconnect();
        this.sendButtonObserver = null;
      }
    }

    handleNewResponse(response) {
      if (!this.started) return;

      // Process response immediately (no debounce needed)
      this.processResponse(response);
    }

    processResponse(response) {
      if (!response) return;

      const messageHash = this.hashMessage(response);
      if (this.processedMessages.has(messageHash)) {
        return; // Already processed
      }

      this.processedMessages.add(messageHash);

      // Response processed - MetaDSL code blocks will be handled by scanForNewCodeBlocks()
      // No command extraction here - MetaDSL only executes from code blocks
    }

    processLastMessage() {
      const response = this.pageAdapter.getLastResponse();
      if (response) {
        this.processResponse(response);
      }
    }

    extractMetaDSLCommand(text) {
      if (!text) return null;

      // Check for command markers
      for (const marker of CONFIG.metadslMarkers) {
        const markerIndex = text.indexOf(marker);
        if (markerIndex !== -1) {
          // Extract command after marker
          const startIndex = markerIndex + marker.length;
          const endIndex = text.indexOf('\n', startIndex);

          if (endIndex !== -1) {
            return text.substring(startIndex, endIndex).trim();
          } else {
            return text.substring(startIndex).trim();
          }
        }
      }

      return null;
    }

    alignTarget() {
      const operation = {
        type: 'send_notification',
        notificationType: 'llm_align_target',
        data: { }
      };
      this.enqueueOperation(operation);
    }

    keepContext(count) {
      var history = this.pageAdapter.getHistory(count);
      const operation = {
        type: 'send_notification',
        notificationType: 'llm_context_count_down',
        data: {
          pageType: this.pageAdapter.pageType,
          history: history
        }
      };
      this.enqueueOperation(operation);
    }

    updateSystemPrompt() {
      const operation = {
        type: 'send_notification',
        notificationType: 'llm_update_system_prompt',
        data: {
          pageType: this.pageAdapter.pageType
        }
      };
      this.enqueueOperation(operation);
    }

    openGameWindow() {
      gameWindow = window.open('http://localhost:8081', '_blank');
      this.sendResultToLLM("game window opened");
    }
    refreshGameWindow() {
      if (gameWindow && !gameWindow.closed) {
        gameWindow.location.reload();
        this.sendResultToLLM("game window refreshed");
      }
      else {
        this.sendResultToLLM("gameWindow not available, use js_request:open_game_window first");
      }
    }
    closeGameWindow() {
      if (gameWindow && !gameWindow.closed) {
        gameWindow.close();
        gameWindow = null;
      }
      this.sendResultToLLM("game window closed");
    }

    // ========================================================================
    // Send Button Monitoring
    // ========================================================================

    checkSendButtonState() {
      // State machine logic:
      // 1. Initial state: canExecuteNewCommands = false (mark all as history)
      // 2. Once user sends first message: canExecuteNewCommands = true (permanently)

      // This method now only checks button state, doesn't set canExecuteNewCommands
      // canExecuteNewCommands will be set when user actually sends a message
      const sendButton = document.querySelector('.vac-svg-button');
      if (sendButton) {
        const isDisabled = sendButton.classList.contains('vac-send-disabled');
        if (!isDisabled) {
          this.debug('✓ Send button enabled - ready for new conversations');
        }
      }
    }

    startSendButtonMonitor() {
      // Monitor send button state changes and user send actions
      const footer = document.querySelector('#room-footer') || document.body;

      this.sendButtonObserver = new MutationObserver(() => {
        if (this.started) {
          this.checkSendButtonState();
        }
      });

      this.sendButtonObserver.observe(footer, {
        attributes: true,
        attributeFilter: ['class'],
        subtree: true
      });

      // Monitor send button clicks to detect when user sends a message
      footer.addEventListener('click', (e) => {
        const sendButton = e.target.closest('.vac-svg-button');
        if (sendButton && !sendButton.classList.contains('vac-send-disabled')) {
          this.onUserSendMessage();
        }
      });

      this.info('Send button monitor started on:', footer.tagName);
    }

    onUserSendMessage() {
      // Called when user sends a message

      // End initialization phase when user actually sends first message
      if (this.isInitializing) {
        this.isInitializing = false;
        this.info('✓ Initialization finished - user sent first message');
        // Mark all existing code blocks as history and collapse agent replies
        this.markHistoryCodeBlocks();
        //First dialog, update system prompt
        this.updateSystemPrompt();
      }

      if (!this.canExecuteNewCommands) {
        this.canExecuteNewCommands = true;
        this.info('Flag updated: canExecuteNewCommands = true');
        this.info('From now on, all new code blocks will be executed');
      }
    }

    // ========================================================================
    // User Input Detection
    // ========================================================================

    getLLMInputContent() {
      // Get LLM input control content based on page type
      if (!this.pageAdapter) {
        return null;
      }

      const pageType = this.pageAdapter.pageType;

      try {
        switch (pageType) {
          case 'local-agent':
            const localAgentInput = document.querySelector('#editable-content[contenteditable="true"]') ||
                                   document.querySelector('#editable-content.editable-content') ||
                                   document.querySelector('.editable-content[contenteditable="true"]');
            return localAgentInput ? localAgentInput.textContent : null;

          case 'custom-llm':
            const customInput = document.querySelector('#editable-content[contenteditable="true"]') ||
                               document.querySelector('#editable-content.editable-content') ||
                               document.querySelector('.editable-content[contenteditable="true"]');
            return customInput ? customInput.textContent : null;

          case 'test':
            const testTextarea = document.querySelector('textarea');
            return testTextarea ? testTextarea.value : null;

          default:
            return null;
        }
      } catch (error) {
        this.warn('Error getting LLM input content:', error);
        return null;
      }
    }

    // ========================================================================
    // State Machine Management
    // ========================================================================

    transitionTo(stateName, reason = '') {
      // Check if already in target state
      if (this.currentStateName === stateName) {
        return;
      }

      // Validate target state exists
      if (!this.states[stateName]) {
        this.error(`Invalid state transition: state '${stateName}' does not exist`);
        return;
      }

      // Prevent concurrent transitions
      if (this.isTransitioning) {
        this.warn(`State transition already in progress, ignoring transition to '${stateName}'`);
        return;
      }

      this.isTransitioning = true;

      try {
        const oldStateName = this.currentStateName || 'NONE';
        this.info(`State transition: ${oldStateName} -> ${stateName} (${reason})`);

        // Exit current state with error handling
        if (this.currentState) {
          try {
            this.currentState.exit();
          } catch (error) {
            this.error(`Error exiting state '${oldStateName}':`, error);
          }
        }

        // Record state history
        this.stateHistory.push({
          from: oldStateName,
          to: stateName,
          reason: reason,
          timestamp: Date.now()
        });

        // Keep only last 100 state transitions
        if (this.stateHistory.length > 100) {
          this.stateHistory.shift();
        }

        // Enter new state with error handling
        this.currentStateName = stateName;
        this.currentState = this.states[stateName];

        try {
          this.currentState.enter();
        } catch (error) {
          this.error(`Error entering state '${stateName}':`, error);
        }

        // Update state display in panel
        if (window.agentPanel) {
          window.agentPanel.updateStateDisplay();
        }
      } finally {
        this.isTransitioning = false;
      }
    }

    async runStateMachine() {
      // Main state machine execution loop
      while (this.started) {
        try {
          if (this.currentState) {
            await this.currentState.run();
          }
          await this.sleep(CONFIG.stateMachineLoopInterval);
        } catch (error) {
          this.info(`Error in ${this.currentStateName}: ${error.message}`);
          this.error(`Error in ${this.currentStateName}:`, error);
        }
      }
    }

    sleep(ms) {
      return new Promise(resolve => setTimeout(resolve, ms));
    }

    // ========================================================================
    // Code Block Processing
    // ========================================================================

    startCodeBlockObserver() {
      // Use document.body to ensure lazy-loaded messages are captured
      // (virtual scroll may insert nodes outside specific chat containers)
      const chatContainer = document.body;
      const specificContainer = document.querySelector('.chat-container') ||
                               document.querySelector('.message-container') ||
                               document.querySelector('[role="main"]');
      this.info(`startCodeBlockObserver: using body, specificContainer=${specificContainer ? specificContainer.tagName + '.' + specificContainer.className.slice(0, 40) : 'none'}`);

      this.observer = new MutationObserver((mutations) => {
        if (!this.started) {
          return;
        }

        // Ignore mutations during history marking to prevent initial scan
        if (this.isMarkingHistory) {
          this.debug('Ignoring mutations during history marking');
          return;
        }

        // Filter out mutations from the control panel to avoid crashes
        const relevantMutations = mutations.filter(mutation => {
          // Check if mutation is inside the control panel
          let node = mutation.target;
          while (node) {
            if (node.id === 'metadsl-control-panel') {
              return false; // Ignore mutations inside panel
            }
            node = node.parentElement;
          }
          // Ignore pure text node replacements (e.g. clock/timer UI animations)
          // These are childList mutations where all added/removed nodes are text nodes
          if (mutation.type === 'childList') {
            const allTextNodes = (nodes) => Array.from(nodes).every(n => n.nodeType === Node.TEXT_NODE);
            if (allTextNodes(mutation.addedNodes) && allTextNodes(mutation.removedNodes)) {
              return false;
            }
          }
          return true;
        });

        // Only process if there are relevant mutations
        if (relevantMutations.length > 0) {
          this.info(`MutationObserver fired: ${mutations.length} mutations, started=${this.started}, isMarkingHistory=${this.isMarkingHistory}`);
          // Log first mutation details for diagnostics
          const m0 = relevantMutations[0];
          const t0 = m0.target;
          this.info(`relevantMutations[0]: type=${m0.type}, target=${t0.tagName}.${(t0.className||'').slice(0,40)}, id=${t0.id||''}, addedNodes=${m0.addedNodes.length}, removedNodes=${m0.removedNodes.length}`);
          // Reset page stable timer
          this.resetPageStableTimer();

          // During initialization, mark newly added message boxes as history immediately
          if (this.isInitializing) {
            for (const mutation of relevantMutations) {
              for (const node of mutation.addedNodes) {
                if (node.nodeType !== Node.ELEMENT_NODE) continue;
                const msgBoxes = node.classList && node.classList.contains('vac-message-box')
                  ? [node]
                  : Array.from(node.querySelectorAll('.vac-message-box'));
                for (const msgBox of msgBoxes) {
                  msgBox.dataset.historyMsg = '1';
                }
              }
            }
          }
        }
      });

      this.observer.observe(chatContainer, {
        childList: true,
        subtree: true,
        characterData: true
      });

      this.info('Code block observer started on:', chatContainer.tagName);
    }

    markHistoryCodeBlocks() {
      // Set flag to prevent observer from triggering during DOM modifications
      this.isMarkingHistory = true;

      // Mark all existing code blocks as history (don't execute)
      const allCodeBlocks = document.querySelectorAll('code.code-block-body, code[class*="language-"], pre code');

      // Filter out code blocks inside the control panel
      const codeBlocks = Array.from(allCodeBlocks).filter(block => {
        let node = block;
        while (node) {
          if (node.id === 'metadsl-control-panel') {
            return false; // Exclude blocks inside panel
          }
          node = node.parentElement;
        }
        return true;
      });

      codeBlocks.forEach((block) => {
        const rawCode = block.textContent.trim();
        const metadslCode = this.extractMetaDSLCode(rawCode);

        // Mark the message container as history
        let msgContainer = block.parentElement;
        while (msgContainer && !msgContainer.classList.contains('vac-message-box')) {
          msgContainer = msgContainer.parentElement;
        }
        if (msgContainer) {
          msgContainer.dataset.historyMsg = '1';
        }

        if (metadslCode) {
          const blockId = this.getBlockId(block);
          this.processedBlocks.add(blockId);

          // Mark as history block
          block.dataset.metadslStatus = 'history';
          block.style.borderLeft = '3px solid #9E9E9E';
          block.style.backgroundColor = 'rgba(158, 158, 158, 0.05)';

          // Schedule hiding the container after 3 seconds
          this.scheduleHideContainer(block);

          this.info('✓ Marked history block:', blockId);
        }
      });

      this.info(`Marked ${this.processedBlocks.size} history code blocks (not executed)`);

      // Mark ALL existing vac-message-box nodes as history (not just those with code blocks)
      document.querySelectorAll('.vac-message-box').forEach(box => {
        box.dataset.historyMsg = '1';
      });

      // Collapse all existing agent reply messages on page
      this.collapseHistoryAgentMessages();

      // Clear isMarkingHistory flag immediately after DOM modifications
      // isInitializing will be cleared when user first interacts (enters USER_INPUT state)
      this.isMarkingHistory = false;
      this.debug('History marking complete, waiting for user interaction to finish initialization');
    }

    // Returns true if the given DOM node is a history node (marked during initial scan or lazy-load re-marking)
    isHistoryNode(node) {
      return !!(node.dataset && node.dataset.historyMsg === '1');
    }

    // ========================================================================
    // Operation Queue Management
    // ========================================================================

    resetPageStableTimer() {
      // Clear existing timer
      if (this.pageStableTimer) {
        clearTimeout(this.pageStableTimer);
      }

      // Set new timer - when it fires, page is considered stable
      this.pageStableTimer = setTimeout(() => {
        this.onPageStable();
      }, this.pageStableDelay);
    }

    onPageStable() {
      this.info(`Page stable, isInitializing=${this.isInitializing}, state=${this.currentStateName}`);
      this.pageStableTimer = null;

      // Check if LLM is still generating using state machine and checkLLMResponding
      const isInLLMRespondingState = this.currentStateName === 'LLM_RESPONDING';
      const isLLMGenerating = this.states && this.states['LLM_RESPONDING']
        ? this.states['LLM_RESPONDING'].checkLLMResponding()
        : false;

      if (isInLLMRespondingState || isLLMGenerating) {
        this.info(`LLM is still generating (state=${this.currentStateName}, isGenerating=${isLLMGenerating}), skipping code block scan to avoid incomplete code`);
        // Reset timer to check again later
        this.resetPageStableTimer();
        return;
      }

      this.info('LLM has stopped generating, processing code blocks...');

      // Re-mark any lazy-loaded history message boxes by DOM order:
      // find the last wrapper that already has historyMsg='1', then mark all preceding unmarked wrappers
      const allWrappers = Array.from(document.querySelectorAll('.vac-message-wrapper'));
      let lastHistoryIdx = -1;
      for (let i = allWrappers.length - 1; i >= 0; i--) {
        const box = allWrappers[i].querySelector('.vac-message-box');
        if (box && box.dataset.historyMsg === '1') {
          lastHistoryIdx = i;
          break;
        }
      }
      if (lastHistoryIdx > 0) {
        for (let i = 0; i < lastHistoryIdx; i++) {
          const box = allWrappers[i].querySelector('.vac-message-box');
          if (box && !box.dataset.historyMsg) {
            box.dataset.historyMsg = '1';
          }
        }
      }

      // Collapse any lazy-loaded history agent reply messages
      this.collapseHistoryAgentMessages();

      // Scan for new code blocks and add to queue
      this.scanForNewCodeBlocks();

      // Trigger state machine to process queue
      // If in AGENT_EXECUTING state, the loop will pick up new operations
      // If in other states, wait for state transition
      if (this.currentStateName === 'AGENT_EXECUTING') {
        this.info('Already in AGENT_EXECUTING state, operations will be processed');
      } else {
        this.info('Not in AGENT_EXECUTING state, operations queued for later');
      }
    }

    scanForNewCodeBlocks() {
      // Find all code blocks with comprehensive selectors
      const allCodeBlocks = document.querySelectorAll('code.code-block-body, code[class*="language-"], pre code');

      // Filter out code blocks inside the control panel
      const codeBlocks = Array.from(allCodeBlocks).filter(block => {
        let node = block;
        while (node) {
          if (node.id === 'metadsl-control-panel') {
            return false;
          }
          node = node.parentElement;
        }
        return true;
      });

      this.info(`Scanning for new code blocks, found ${codeBlocks.length} total blocks`);

      let newBlocksCount = 0;
      let metadslBlocksCount = 0;

      codeBlocks.forEach((block) => {
        const blockId = this.getBlockId(block);

        // Skip already processed blocks
        if (this.processedBlocks.has(blockId)) {
          return;
        }

        newBlocksCount++;

        const rawCode = block.textContent.trim();

        // Check if this block's content is still changing
        const lastContent = this.lastBlockContent.get(blockId);
        if (lastContent !== undefined && lastContent !== rawCode) {
          // Content is still changing, update cache but skip processing
          this.lastBlockContent.set(blockId, rawCode);
          this.debug(`Block ${blockId} content still changing, skipping (length: ${lastContent.length} -> ${rawCode.length})`);
          return;
        }

        // Content is stable (first time or unchanged), record it
        this.lastBlockContent.set(blockId, rawCode);

        const metadslCode = this.extractMetaDSLCode(rawCode);

        if (metadslCode) {
          metadslBlocksCount++;

          // Mark as processed immediately
          this.processedBlocks.add(blockId);

          // Check if this block is inside a history message container
          let isHistoryBlock = this.isInitializing;
          if (!isHistoryBlock) {
            let msgContainer = block.parentElement;
            while (msgContainer && !msgContainer.classList.contains('vac-message-box')) {
              msgContainer = msgContainer.parentElement;
            }
            if (msgContainer && this.isHistoryNode(msgContainer)) {
              isHistoryBlock = true;
            }
          }

          if (isHistoryBlock) {
            block.dataset.metadslStatus = 'history';
            block.style.borderLeft = '3px solid #9E9E9E';
            block.style.backgroundColor = 'rgba(158, 158, 158, 0.05)';
            this.scheduleHideContainer(block);
            this.debug(`✓ Marked late-arriving history block: ${blockId}`);
            return;
          }

          // After initialization, determine operation type based on canExecuteNewCommands
          this.debug(`📌 canExecuteNewCommands = ${this.canExecuteNewCommands}`);
          const operationType = this.canExecuteNewCommands ? 'execute' : 'mark_history';

          // Add to operation queue
          this.enqueueOperation({
            type: operationType,
            block: block,
            code: metadslCode,
            blockId: blockId,
            timestamp: Date.now()
          });

          this.debug(`✓ Queued ${operationType} operation for block: ${blockId}`);
        }
      });

      if (newBlocksCount > 0) {
        this.info(`Scan complete: ${newBlocksCount} new blocks, ${metadslBlocksCount} with MetaDSL markers`);
      }

      // Limit processed blocks size to prevent memory leak
      if (this.processedBlocks.size > this.maxProcessedBlocks) {
        const toDelete = this.processedBlocks.size - this.maxProcessedBlocks;
        const iterator = this.processedBlocks.values();
        for (let i = 0; i < toDelete; i++) {
          this.processedBlocks.delete(iterator.next().value);
        }
        this.debug(`Cleaned up ${toDelete} old processed blocks`);
      }
    }

    enqueueOperation(operation) {
      // Check if the last operation in queue is identical to current operation
      if (this.operationQueue.length > 0) {
        const lastOperation = this.operationQueue[this.operationQueue.length - 1];
        if (this.areOperationsEqual(lastOperation, operation)) {
          this.debug(`Skipping duplicate operation: ${operation.type}`);
          return;
        }
      }

      this.operationQueue.push(operation);

      // Update state display when queue changes
      if (window.agentPanel) {
        window.agentPanel.updateStateDisplay();
      }
    }

    areOperationsEqual(op1, op2) {
      // Compare operations by their essential properties, skip DOM elements
      if (op1 === op2) return true;
      if (!op1 || !op2) return false;

      // Compare blockId instead of block element
      if (op1.blockId !== op2.blockId) return false;

      // Compare code content
      if (op1.code !== op2.code) return false;

      // Compare language if present
      if (op1.language !== op2.language) return false;

      return true;
    }

    async processOperationQueue() {
      // This method is now deprecated in favor of state machine
      // Kept for backward compatibility, redirects to state machine
      this.info('processOperationQueue called, using state machine instead');

      // If not in AGENT_EXECUTING state, transition to it
      if (this.currentStateName !== 'AGENT_EXECUTING') {
        this.transitionTo('AGENT_EXECUTING', 'processOperationQueue called');
      }
    }

    getBlockId(block) {
      // Use cached ID if available
      if (this.blockIdCache.has(block)) {
        return this.blockIdCache.get(block);
      }

      // Generate stable ID based on content hash and unique sequence number
      const content = block.textContent.trim();
      const contentHash = this.hashString(content);
      const blockId = `${this.nextBlockId++}_${contentHash}`;

      // Cache the ID for this block
      this.blockIdCache.set(block, blockId);

      return blockId;
    }

    scheduleHideContainer(block) {
      const container = block.closest('pre') || block.closest('div.code-block') || block.parentElement || block;
      this.info(`scheduleHideContainer: block=${block.tagName}.${block.className.slice(0,30)}, container=${container.tagName}.${container.className.slice(0,30)}`);
      setTimeout(() => {
        container.style.display = 'none';
        this.info(`scheduleHideContainer: hidden container=${container.tagName}, display=${container.style.display}`);
      }, 3000);
    }

    isAgentMessage(msgBox) {
      const marker = CONFIG.get('metadsl.agentReplyMarker');
      return msgBox && marker && msgBox.textContent.includes(marker);
    }

    collapseHistoryAgentMessages() {
      // Find all user messages on page that contain agent reply marker and replace with collapsed text
      const userMessages = document.querySelectorAll('.vac-message-box.vac-offset-current');
      this.info(`collapseHistoryAgentMessages: found ${userMessages.length} user message boxes`);
      let collapsed = 0;
      userMessages.forEach((msgBox) => {
        if (this.isAgentMessage(msgBox) && !msgBox.dataset.agentCollapsed) {
          this.debug(`Collapsing history agent msg, tagName=${msgBox.tagName}, className=${msgBox.className}`);
          msgBox.dataset.agentCollapsed = '1';
          msgBox.textContent = '[Agent reply omitted]';
          collapsed++;
        }
      });
      this.info(`collapseHistoryAgentMessages: collapsed ${collapsed} messages`);
    }

    addVisualIndicator(block, status) {
      try {
        // Find the parent container (pre or div) to modify instead of code element
        let container = block.closest('pre') || block.closest('div.code-block') || block.parentElement;

        // If we can't find a suitable container, use the block itself but be more careful
        if (!container || container === block) {
          container = block;
        }

        // Use CSS classes instead of direct style manipulation when possible
        if (status === 'executed') {
          container.classList.add('metadsl-executed');
          // Fallback to inline styles if CSS class doesn't work
          if (!container.classList.contains('metadsl-executed')) {
            container.style.borderLeft = '3px solid #4CAF50';
            container.style.backgroundColor = 'rgba(76, 175, 80, 0.05)';
          }
        } else if (status === 'history') {
          container.classList.add('metadsl-history');
          // Fallback to inline styles if CSS class doesn't work
          if (!container.classList.contains('metadsl-history')) {
            container.style.borderLeft = '3px solid #9E9E9E';
            container.style.backgroundColor = 'rgba(158, 158, 158, 0.05)';
          }
        }
      } catch (error) {
        this.error('Error in addVisualIndicator:', error);
      }
    }

    extractMetaDSLCode(code) {
      if (!code) return null;

      // Trim the entire code block and check if it starts with marker comment
      const trimmedCode = code.trim();

      // Check all configured markers
      let hasMarker = false;
      for (const marker of CONFIG.metadslMarkers) {
        if (trimmedCode.startsWith(marker)) {
          hasMarker = true;
          break;
        }
      }

      if (!hasMarker) {
        // Debug info for blocks without marker (optional, can be commented out if too verbose)
        // this.info('Code block has no MetaDSL marker, skipping');
        return null;
      }

      this.debug('✓ MetaDSL marker detected in code block');

      // Remove the marker line and return only the actual MetaDSL code
      const firstNewline = trimmedCode.indexOf('\n');
      if (firstNewline === -1) {
        // Only marker line, no actual code
        return null;
      }
      return trimmedCode.substring(firstNewline + 1).trim();
    }

    hashString(str) {
      // Simple hash for string
      let hash = 0;
      for (let i = 0; i < str.length; i++) {
        const char = str.charCodeAt(i);
        hash = ((hash << 5) - hash) + char;
        hash = hash | 0; // Convert to 32-bit integer
      }
      return hash.toString();
    }

    // ========================================================================
    // Command Execution
    // ========================================================================

    executeCommand(command) {
      try {
        // Check if this is a JavaScript request (not MetaDSL)
        const js_request_prefix = "js_request:";
        const js_eval_prefix = "js_eval:";
        const trimedCommand = command.trim();
        if (trimedCommand.startsWith(js_request_prefix)) {
          const jsRequest = trimedCommand.substring(js_request_prefix.length).trim();
          this.info('JavaScript request detected:', jsRequest);

          if (jsRequest === "keep_llm_context") {
            this.keepContext(CONFIG.llmContextCount);
            return;
          }
          else if (jsRequest === "open_game_window") {
            this.openGameWindow();
            return;
          }
          else if (jsRequest === "refresh_game_window") {
            this.refreshGameWindow();
            return;
          }
          else if (jsRequest === "close_game_window") {
            this.closeGameWindow();
            return;
          }

          this.sendResultToLLM("unknown request: " + jsRequest);
          return; // Don't send to C#
        }
        else if (trimedCommand.startsWith(js_eval_prefix)) {
          const jsEval = trimedCommand.substring(js_eval_prefix.length).trim();
          this.info('JavaScript eval detected:', jsEval);
          if (gameWindow) {
            gameWindow.postMessage({ type: 'js_eval', code: jsEval }, "http://localhost:8081");
          } else {
            this.sendResultToLLM("gameWindow not available, use js_request:open_game_window first");
          }
          return; // Don't send to C#
        }

        // Normal MetaDSL command execution
        this.info('Sending MetaDSL command to C# via WebSocket:', command.length <= 100 ? command : command.substring(0, 100) + "...");

        if (!this.metadslWorker || !this.metadslWorker.isRunning) {
          this.error('MetaDSL Worker is not running');
          return;
        }

        // Send MetaDSL code directly via MetaDSL Worker
        const success = this.metadslWorker.queueMessage(command);
        if (success) {
          this.info('✓ MetaDSL command queued for execution');
        } else {
          this.error('Failed to queue MetaDSL command');
        }
      } catch (error) {
        this.error('Error in executeCommand:', error);
      }
    }
    sendResultToLLM(message) {
      if (!this.pageAdapter) {
        this.warn('PageAdapter not available');
        return;
      }
      let messageStr = CONFIG.get('metadsl.agentReplyMarker') + "\n";
      if (message === null || message === undefined) {
        messageStr += 'null';
      } else if (typeof message === 'object') {
        messageStr += JSON.stringify(message, null, 2);
      } else {
        messageStr += String(message).trim();
      }

      if (messageStr.indexOf("MetaDSL") >= 0 && this.operationQueue.length > 0) {
        messageStr += '\n\n特别注意：当前有' + this.operationQueue.length + '个操作在排队执行，你看到消息后只回复继续即可，不要再发新的metadsl代码块';
      }

      this.info('Sending result to LLM', messageStr.length <= 100 ? messageStr : messageStr.substring(0, 100) + "...");
      // Send message through page adapter
      try {
        this.pageAdapter.sendMessage(messageStr);
      } catch (err) {
        this.error('Failed to send result to LLM:', err);
      }
    }

    hashMessage(text) {
      // Simple hash for message deduplication
      let hash = 0;
      for (let i = 0; i < text.length; i++) {
        const char = text.charCodeAt(i);
        hash = ((hash << 5) - hash) + char;
        hash = hash | 0; // Convert to 32-bit integer
      }
      return hash.toString();
    }

    info(message, data) {
      this.logger.info(message, data);
    }

    debug(message, data) {
      this.logger.debug(message, data);
    }

    warn(message, data) {
      this.logger.warn(message, data);
    }

    error(message, data) {
      this.logger.error(message, data);
    }
  }



  // ============================================================================
  // AgentPanel - Visual Control Panel
  // ============================================================================
  class AgentPanel {
    constructor(bridge, metadslMonitor, pageAdapter) {
      this.bridge = bridge;
      this.metadslMonitor = metadslMonitor;
      this.pageAdapter = pageAdapter;
      this.visible = false;
      this.panel = null;
      this.logArea = null;
      this.metadslButton = null;
      this.llmTypeLabel = null;
      this.stateUpdateTimer = null;

      this.createPanel();
      this.updateLLMType();
    }

    createPanel() {
      // Create panel element
      this.panel = document.createElement('div');
      this.panel.id = 'agent-control-panel';
      this.panel.style.cssText = `
        position: fixed;
        ${CONFIG.panelPosition === 'bottom-right' ? 'right: 20px; bottom: 20px;' : 'left: 20px; top: 20px;'}
        width: ${CONFIG.panelWidth}px;
        height: ${CONFIG.panelHeight}px;
        background: #2d2d2d;
        border: 1px solid #444;
        border-radius: 8px;
        display: none;
        flex-direction: column;
        z-index: 10000;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Arial, sans-serif;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
        resize: both;
        overflow: hidden;
        min-width: ${CONFIG.panelMinWidth}px;
        min-height: ${CONFIG.panelMinHeight}px;
      `;

      // Prevent all events from bubbling out of the panel
      // This ensures panel operations don't trigger external MutationObserver
      ['input', 'change', 'paste', 'cut', 'keydown', 'keyup', 'keypress', 'beforeinput'].forEach(eventType => {
        this.panel.addEventListener(eventType, (e) => {
          e.stopPropagation();
        }, true);
      });

      // Create header
      const header = document.createElement('div');
      header.style.cssText = `
        padding: 10px 15px;
        background: #1a1a1a;
        border-bottom: 1px solid #444;
        border-radius: 8px 8px 0 0;
        display: flex;
        justify-content: space-between;
        align-items: center;
        cursor: move;
      `;

      const titleContainer = document.createElement('div');
      titleContainer.style.cssText = `
        display: flex;
        flex-direction: column;
        gap: 4px;
      `;

      const title = document.createElement('span');
      title.textContent = 'Agent Control Panel';
      title.style.cssText = `
        color: #fff;
        font-weight: 600;
        font-size: 14px;
      `;

      this.llmTypeLabel = document.createElement('span');
      this.llmTypeLabel.textContent = 'LLM: unknown';
      this.llmTypeLabel.style.cssText = `
        color: #999;
        font-size: 11px;
        font-weight: normal;
      `;

      titleContainer.appendChild(title);
      titleContainer.appendChild(this.llmTypeLabel);

      const closeBtn = document.createElement('button');
      closeBtn.innerHTML = '&times;';
      closeBtn.style.cssText = `
        background: none;
        border: none;
        color: #999;
        font-size: 20px;
        cursor: pointer;
        padding: 0 5px;
      `;
      closeBtn.onclick = () => this.hide();

      header.appendChild(titleContainer);
      header.appendChild(closeBtn);
      this.panel.appendChild(header);

      // Create state display bar
      this.stateBar = document.createElement('div');
      this.stateBar.style.cssText = `
        padding: 8px 15px;
        background: #1e1e1e;
        border-bottom: 1px solid #444;
        display: flex;
        justify-content: space-between;
        align-items: center;
        font-size: 12px;
        gap: 10px;
      `;

      this.stateLabel = document.createElement('span');
      this.stateLabel.style.cssText = `
        color: #fff;
        font-weight: 500;
        padding: 4px 8px;
        border-radius: 4px;
        background: #555;
      `;

      this.stateInfo = document.createElement('span');
      this.stateInfo.style.cssText = `
        color: #999;
        font-size: 11px;
        flex: 1;
      `;

      // Create log level filter buttons (compact icon style)
      const logFilterContainer = document.createElement('div');
      logFilterContainer.style.cssText = `
        display: flex;
        gap: 6px;
        align-items: center;
      `;

      const logLevels = [
        { key: 'debug', icon: '🔍', color: '#2196f3', title: 'Debug' },
        { key: 'info', icon: '📝', color: '#4caf50', title: 'Info' },
        { key: 'warn', icon: '⚠️', color: '#ff9800', title: 'Warn' },
        { key: 'error', icon: '❌', color: '#f44336', title: 'Error' }
      ];

      logLevels.forEach(level => {
        const btn = document.createElement('button');
        btn.textContent = level.icon;
        btn.title = `Toggle ${level.title} logs`;
        btn.style.cssText = `
          background: ${CONFIG.logLevels[level.key] ? level.color : '#555'};
          border: 1px solid ${level.color};
          color: #fff;
          cursor: pointer;
          padding: 2px 6px;
          border-radius: 4px;
          font-size: 12px;
          transition: all 0.2s;
          opacity: ${CONFIG.logLevels[level.key] ? '1' : '0.4'};
        `;
        btn.onclick = () => {
          CONFIG.logLevels[level.key] = !CONFIG.logLevels[level.key];
          btn.style.background = CONFIG.logLevels[level.key] ? level.color : '#555';
          btn.style.opacity = CONFIG.logLevels[level.key] ? '1' : '0.4';
          this.log(`${level.icon} ${level.title} logs ${CONFIG.logLevels[level.key] ? 'enabled' : 'disabled'}`);
        };
        logFilterContainer.appendChild(btn);
      });

      this.stateBar.appendChild(this.stateLabel);
      this.stateBar.appendChild(this.stateInfo);
      this.stateBar.appendChild(logFilterContainer);
      this.panel.appendChild(this.stateBar);

      // Create button bar
      const buttonBar = document.createElement('div');
      buttonBar.style.cssText = `
        padding: 10px;
        background: #252525;
        border-bottom: 1px solid #444;
        display: flex;
        gap: 8px;
        flex-wrap: wrap;
      `;

      // Test connection button
      const testBtn = document.createElement('button');
      testBtn.textContent = 'Test Connection';
      testBtn.style.cssText = `
        padding: 6px 12px;
        background: #2196f3;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      testBtn.onclick = () => this.testConnection();

      // MetaDSL toggle button
      this.metadslButton = document.createElement('button');
      this.metadslButton.textContent = 'Start MetaDSL';
      this.metadslButton.style.cssText = `
        padding: 6px 12px;
        background: #4caf50;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      this.metadslButton.onclick = () => this.toggleMetaDSL();

      // Clear log button
      const clearBtn = document.createElement('button');
      clearBtn.textContent = 'Clear Log';
      clearBtn.style.cssText = `
        padding: 6px 12px;
        background: #ff9800;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      clearBtn.onclick = () => this.clearLog();

      // Copy page HTML button
      const copyHtmlBtn = document.createElement('button');
      copyHtmlBtn.textContent = 'Copy Selection';
      copyHtmlBtn.style.cssText = `
        padding: 6px 12px;
        background: #9c27b0;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      copyHtmlBtn.onclick = () => this.copyPageHTML();

      // Execute MetaDSL button
      const execMetaDslBtn = document.createElement('button');
      execMetaDslBtn.textContent = 'Execute MetaDSL';
      execMetaDslBtn.style.cssText = `
        padding: 6px 12px;
        background: #673ab7;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      execMetaDslBtn.onclick = () => this.runMetaDSL();

      // Execute JavaScript button
      const execJsBtn = document.createElement('button');
      execJsBtn.textContent = 'Execute JS';
      execJsBtn.style.cssText = `
        padding: 6px 12px;
        background: #f44336;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      execJsBtn.onclick = () => this.executeJavascript();

      // Clear operation queue button
      const clearQueueBtn = document.createElement('button');
      clearQueueBtn.textContent = 'Clear Queue';
      clearQueueBtn.style.cssText = `
        padding: 6px 12px;
        background: #ff5722;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      clearQueueBtn.onclick = () => this.clearOperationQueue();

      // Auto Plan toggle button
      this.autoPlanButton = document.createElement('button');
      this.autoPlanButton.textContent = '✓ Auto Plan';
      this.autoPlanButton.style.cssText = `
        padding: 6px 12px;
        background: #4caf50;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      this.autoPlanButton.onclick = () => this.toggleAutoPlan();

      buttonBar.appendChild(testBtn);
      buttonBar.appendChild(this.metadslButton);
      buttonBar.appendChild(clearQueueBtn);
      buttonBar.appendChild(this.autoPlanButton);
      buttonBar.appendChild(clearBtn);
      buttonBar.appendChild(copyHtmlBtn);
      buttonBar.appendChild(execMetaDslBtn);
      buttonBar.appendChild(execJsBtn);
      this.panel.appendChild(buttonBar);

      // Create MetaDSL input area
      this.scriptInput = document.createElement('textarea');
      this.scriptInput.placeholder = 'Enter MetaDSL/Javascript here to execute...';
      this.scriptInput.value = 'read_file("d:/GitHub/BatchCommand/AgentCore/docs/QUICK_START.md");';
      this.scriptInput.style.cssText = `
        height: 100px;
        background: #2d2d2d;
        color: #d4d4d4;
        border: 1px solid #444;
        border-left: 3px solid #673ab7;
        padding: 10px;
        font-family: 'Consolas', 'Monaco', monospace;
        font-size: 12px;
        resize: vertical;
        outline: none;
        margin-bottom: 5px;
      `;

      // Prevent MutationObserver from triggering during input operations
      // Use throttling to reduce disconnect/reconnect frequency
      let metadslInputThrottleTimer = null;
      this.scriptInput.addEventListener('beforeinput', (e) => {
        e.stopPropagation();

        if (this.metadslMonitor && this.metadslMonitor.enabled) {
          // Only disconnect once at the start of input session
          if (!metadslInputThrottleTimer) {
            this.metadslMonitor.enabled = false;
            if (this.metadslMonitor.observer) {
              this.metadslMonitor.observer.disconnect();
            }
          }

          // Reset timer on each input (throttle)
          if (metadslInputThrottleTimer) {
            clearTimeout(metadslInputThrottleTimer);
          }

          metadslInputThrottleTimer = setTimeout(() => {
            if (this.metadslMonitor) {
              this.metadslMonitor.enabled = true;
              if (this.metadslMonitor.observer && this.metadslMonitor.chatContainer) {
                this.metadslMonitor.observer.observe(this.metadslMonitor.chatContainer, {
                  childList: true,
                  subtree: true
                });
              }
            }
            metadslInputThrottleTimer = null;
          }, CONFIG.mutationObserverRestartDelay);
        }
      }, true);

      this.scriptInput.addEventListener('paste', (e) => {
        e.stopPropagation();
      }, true);

      this.scriptInput.addEventListener('input', (e) => {
        e.stopPropagation();
      }, true);

      this.panel.appendChild(this.scriptInput);

      // Create log area
      this.logArea = document.createElement('textarea');
      this.logArea.readOnly = true;
      this.logArea.style.cssText = `
        flex: 1;
        background: #1e1e1e;
        color: #d4d4d4;
        border: none;
        padding: 10px;
        font-family: 'Consolas', 'Monaco', monospace;
        font-size: 12px;
        resize: none;
        outline: none;
      `;
      this.panel.appendChild(this.logArea);

      // Add to document
      document.body.appendChild(this.panel);

      // Make panel draggable
      this.makeDraggable(header, this.panel);

      // Make panel resizable
      this.makeResizable(this.panel);

      // Update button state based on actual monitor status
      this.updateMetaDSLButtonState();
    }

    updateMetaDSLButtonState() {
      if (this.metadslMonitor.started) {
        this.metadslButton.textContent = 'Stop MetaDSL';
        this.metadslButton.style.background = '#ff9800';
      } else {
        this.metadslButton.textContent = 'Start MetaDSL';
        this.metadslButton.style.background = '#4caf50';
      }
    }

    updateStateDisplay() {
      if (!this.stateLabel || !this.stateInfo) {
        return;
      }

      if (!this.metadslMonitor) {
        return;
      }

      const state = this.metadslMonitor.currentStateName;
      const stateColors = {
        'USER_INPUT': '#2196f3',      // Blue
        'LLM_RESPONDING': '#ff9800',  // Orange
        'AGENT_EXECUTING': '#4caf50'  // Green
      };

      const stateNames = {
        'USER_INPUT': 'User Input',
        'LLM_RESPONDING': 'LLM Responding',
        'AGENT_EXECUTING': 'Agent Executing'
      };

      // Update state label
      this.stateLabel.textContent = stateNames[state] || state;
      this.stateLabel.style.background = stateColors[state] || '#555';

      // Calculate state duration
      const history = this.metadslMonitor.stateHistory;
      let duration = 0;
      let lastTransitionTime = 0;
      if (history && history.length > 0) {
        const lastTransition = history[history.length - 1];
        if (lastTransition && lastTransition.timestamp) {
          lastTransitionTime = lastTransition.timestamp;
          duration = Math.floor((Date.now() - lastTransition.timestamp) / 1000);
        }
      }

      // Get operation queue length
      const queueLength = this.metadslMonitor.operationQueue ? this.metadslMonitor.operationQueue.length : 0;

      // Get context counter from LLM_RESPONDING state
      let contextCounter = 0;
      if (this.metadslMonitor.states && this.metadslMonitor.states['LLM_RESPONDING']) {
        contextCounter = this.metadslMonitor.states['LLM_RESPONDING'].contextCounter || 0;
      }

      // Get operation executed flag from AGENT_EXECUTING state
      let operationExecuted = false;
      if (this.metadslMonitor.states && this.metadslMonitor.states['AGENT_EXECUTING']) {
        operationExecuted = this.metadslMonitor.states['AGENT_EXECUTING'].operationExecuted || false;
      }

      // Update info text
      const execStatus = operationExecuted ? 'Yes' : 'No';
      this.stateInfo.textContent = `Duration: ${duration}s | Queue: ${queueLength} | Counter: ${contextCounter} | Executed: ${execStatus}`;
    }

    updateLLMType() {
      if (this.pageAdapter && this.llmTypeLabel) {
        const llmType = this.pageAdapter.pageType || 'unknown';
        const llmTypeMap = {
          'local-agent': 'Local Agent',
          'custom-llm': 'Custom LLM',
          'test': 'Test Page',
          'unknown': 'Unknown'
        };
        const displayName = llmTypeMap[llmType] || llmType;
        this.llmTypeLabel.textContent = `LLM: ${displayName}`;

        // Update color based on type
        const colorMap = {
          'local-agent': '#27ae60',
          'custom-llm': '#2196f3',
          'test': '#9c27b0',
          'unknown': '#999'
        };
        this.llmTypeLabel.style.color = colorMap[llmType] || '#999';
      }
    }

    makeDraggable(header, panel) {
      let isDragging = false;
      let offsetX, offsetY;

      header.addEventListener('mousedown', (e) => {
        isDragging = true;
        offsetX = e.clientX - panel.offsetLeft;
        offsetY = e.clientY - panel.offsetTop;
      });

      document.addEventListener('mousemove', (e) => {
        if (!isDragging) return;
        panel.style.left = (e.clientX - offsetX) + 'px';
        panel.style.top = (e.clientY - offsetY) + 'px';
        panel.style.right = 'auto';
        panel.style.bottom = 'auto';
      });

      document.addEventListener('mouseup', () => {
        isDragging = false;
      });
    }

    makeResizable(panel) {
      const resizeHandles = ['n', 'e', 's', 'w', 'ne', 'se', 'sw', 'nw'];
      const handleSize = CONFIG.panelResizeHandleSize;

      resizeHandles.forEach(direction => {
        const handle = document.createElement('div');
        handle.className = `resize-handle resize-${direction}`;

        let cursorStyle = '';
        let positionStyle = '';

        switch (direction) {
          case 'n':
            cursorStyle = 'ns-resize';
            positionStyle = 'top: 0; left: 0; right: 0; height: ' + handleSize + 'px;';
            break;
          case 'e':
            cursorStyle = 'ew-resize';
            positionStyle = 'top: 0; right: 0; bottom: 0; width: ' + handleSize + 'px;';
            break;
          case 's':
            cursorStyle = 'ns-resize';
            positionStyle = 'bottom: 0; left: 0; right: 0; height: ' + handleSize + 'px;';
            break;
          case 'w':
            cursorStyle = 'ew-resize';
            positionStyle = 'top: 0; left: 0; bottom: 0; width: ' + handleSize + 'px;';
            break;
          case 'ne':
            cursorStyle = 'nesw-resize';
            positionStyle = 'top: 0; right: 0; width: ' + handleSize + 'px; height: ' + handleSize + 'px;';
            break;
          case 'se':
            cursorStyle = 'nwse-resize';
            positionStyle = 'bottom: 0; right: 0; width: ' + handleSize + 'px; height: ' + handleSize + 'px;';
            break;
          case 'sw':
            cursorStyle = 'nesw-resize';
            positionStyle = 'bottom: 0; left: 0; width: ' + handleSize + 'px; height: ' + handleSize + 'px;';
            break;
          case 'nw':
            cursorStyle = 'nwse-resize';
            positionStyle = 'top: 0; left: 0; width: ' + handleSize + 'px; height: ' + handleSize + 'px;';
            break;
        }

        handle.style.cssText = `
          position: absolute;
          ${positionStyle}
          cursor: ${cursorStyle};
          z-index: 10;
        `;

        panel.appendChild(handle);

        let isResizing = false;
        let startX, startY, startWidth, startHeight, startLeft, startTop;

        handle.addEventListener('mousedown', (e) => {
          e.stopPropagation();
          isResizing = true;
          startX = e.clientX;
          startY = e.clientY;
          startWidth = panel.offsetWidth;
          startHeight = panel.offsetHeight;
          startLeft = panel.offsetLeft;
          startTop = panel.offsetTop;

          document.addEventListener('mousemove', onMouseMove);
          document.addEventListener('mouseup', onMouseUp);
        });

        const onMouseMove = (e) => {
          if (!isResizing) return;

          const deltaX = e.clientX - startX;
          const deltaY = e.clientY - startY;

          const minWidth = parseInt(panel.style.minWidth) || 400;
          const minHeight = parseInt(panel.style.minHeight) || 300;

          if (direction.includes('e')) {
            const newWidth = Math.max(minWidth, startWidth + deltaX);
            panel.style.width = newWidth + 'px';
          }
          if (direction.includes('w')) {
            const newWidth = Math.max(minWidth, startWidth - deltaX);
            if (newWidth > minWidth) {
              panel.style.width = newWidth + 'px';
              panel.style.left = (startLeft + deltaX) + 'px';
            }
          }
          if (direction.includes('s')) {
            const newHeight = Math.max(minHeight, startHeight + deltaY);
            panel.style.height = newHeight + 'px';
          }
          if (direction.includes('n')) {
            const newHeight = Math.max(minHeight, startHeight - deltaY);
            if (newHeight > minHeight) {
              panel.style.height = newHeight + 'px';
              panel.style.top = (startTop + deltaY) + 'px';
            }
          }
        };

        const onMouseUp = () => {
          isResizing = false;
          document.removeEventListener('mousemove', onMouseMove);
          document.removeEventListener('mouseup', onMouseUp);
        };
      });
    }

    show() {
      this.panel.style.display = 'flex';
      this.visible = true;

      // Start state display update timer
      this.startStateUpdateTimer();
    }

    hide() {
      this.panel.style.display = 'none';
      this.visible = false;

      // Stop state display update timer
      this.stopStateUpdateTimer();
    }

    startStateUpdateTimer() {
      // Clear existing timer if any
      this.stopStateUpdateTimer();

      // Update immediately
      try {
        this.updateStateDisplay();
      } catch (e) {
        console.error('[AgentPanel] Error in initial updateStateDisplay:', e);
      }

      // Update every second
      this.stateUpdateTimer = setInterval(() => {
        try {
          this.updateStateDisplay();
        } catch (e) {
          console.error('[AgentPanel] Error in updateStateDisplay:', e);
        }
      }, 1000);
    }

    stopStateUpdateTimer() {
      if (this.stateUpdateTimer) {
        clearInterval(this.stateUpdateTimer);
        this.stateUpdateTimer = null;
      }
    }

    toggle() {
      if (this.visible) {
        this.hide();
      } else {
        this.show();
      }
    }

    log(message) {
      const timestamp = new Date().toLocaleTimeString();
      this.logArea.value += `[${timestamp}] ${message}\n`;

      // Limit log area to max configured lines to prevent memory issues
      const lines = this.logArea.value.split('\n');
      if (lines.length > CONFIG.maxLogLines) {
        this.logArea.value = lines.slice(-CONFIG.maxLogLines).join('\n');
      }

      this.logArea.scrollTop = this.logArea.scrollHeight;
    }

    clearLog() {
      this.logArea.value = '';
    }

    testConnection() {
      this.log('Testing connection to C# Agent...');
      this.bridge.sendCommand('ping', {}, (success, data, error) => {
        if (success) {
          this.log('✓ Connection successful!');
        } else {
          this.log('✗ Connection failed: ' + error);
        }
      });
    }

    toggleMetaDSL() {
      if (this.metadslMonitor.enabled) {
        this.log('Stopping MetaDSL monitor...');
        this.metadslMonitor.stop();
        this.metadslButton.textContent = 'Start MetaDSL';
        this.metadslButton.style.background = '#4caf50';
        this.log('✓ MetaDSL monitor stopped');
      } else {
        this.log('Starting MetaDSL monitor...');
        this.metadslMonitor.start();
        this.metadslButton.textContent = 'Stop MetaDSL';
        this.metadslButton.style.background = '#ff9800';
        this.log('✓ MetaDSL monitor started');
        this.log('  Watching for // @execute or # @execute comment markers');
      }
    }

    toggleAutoPlan() {
      this.bridge.autoPlanEnabled = !this.bridge.autoPlanEnabled;
      if (this.bridge.autoPlanEnabled) {
        this.autoPlanButton.textContent = '✓ Auto Plan';
        this.autoPlanButton.style.background = '#4caf50';
        this.log('✓ Auto plan enabled');
      } else {
        this.autoPlanButton.textContent = '✗ Auto Plan';
        this.autoPlanButton.style.background = '#666';
        this.log('✗ Auto plan disabled');
      }
    }

    clearOperationQueue() {
      if (this.metadslMonitor && this.metadslMonitor.operationQueue) {
        const queueLength = this.metadslMonitor.operationQueue.length;
        this.metadslMonitor.operationQueue = [];
        this.log(`✓ Operation queue cleared (${queueLength} operations removed)`);
        this.updateStateDisplay();
      } else {
        this.log('⚠ No operation queue to clear');
      }
    }

    copyPageHTML() {
      this.log('Copying selected HTML to clipboard...');

      try {
        // Get the current selection
        const selection = window.getSelection();

        if (!selection || selection.rangeCount === 0 || selection.isCollapsed) {
          this.log('✗ No content selected. Please select HTML content first.');
          return;
        }

        // Get the selected range
        const range = selection.getRangeAt(0);

        // Create a temporary container to hold the selected content
        const container = document.createElement('div');
        container.appendChild(range.cloneContents());

        // Remove the agent control panel if it exists in the selection
        const agentPanel = container.querySelector('#agent-control-panel');
        if (agentPanel) {
          agentPanel.remove();
          this.log('  Removed agent control panel from selection');
        }

        // Get the HTML
        const html = container.innerHTML;

        if (!html || html.trim().length === 0) {
          this.log('✗ Selected content is empty');
          return;
        }

        // Copy to clipboard
        if (navigator.clipboard && navigator.clipboard.writeText) {
          navigator.clipboard.writeText(html).then(() => {
            this.log('✓ Selected HTML copied to clipboard successfully');
            this.log(`  HTML length: ${html.length} characters`);
            this.log(`  Selected elements: ${container.children.length} top-level elements`);
          }).catch(err => {
            this.log('✗ Failed to copy to clipboard: ' + err);
            this.fallbackCopy(html);
          });
        } else {
          // Fallback for older browsers
          this.fallbackCopy(html);
        }
      } catch (e) {
        this.log('✗ Error: ' + e.message);
      }
    }

    fallbackCopy(text) {
      try {
        // Create a temporary textarea
        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'fixed';
        textarea.style.opacity = '0';
        document.body.appendChild(textarea);
        textarea.select();

        const success = document.execCommand('copy');
        document.body.removeChild(textarea);

        if (success) {
          this.log('✓ Page HTML copied using fallback method');
          this.log(`  HTML length: ${text.length} characters`);
        } else {
          this.log('✗ Fallback copy also failed');
        }
      } catch (err) {
        this.log('✗ Fallback copy error: ' + err);
      }
    }

    runMetaDSL() {
      this.log('Executing MetaDSL script...');
      try {
        const script = this.scriptInput.value.trim();
        if (!script) {
          this.log('✗ MetaDSL input is empty. Please enter MetaDSL script first.');
          return;
        }
        this.log('─'.repeat(50));
        this.log('Executing MetaDSL:');
        this.log(script.substring(0, 200) + (script.length > 200 ? '...' : ''));
        this.log('─'.repeat(50));

        // Call window.executeMetaDSL directly (provided by C++ layer)
        if (typeof window.executeMetaDSL === 'function') {
          const result = window.executeMetaDSL(script);
          this.log('✓ MetaDSL executed successfully');
          this.log('Result:');
          this.log(result);
        } else {
          this.log('✗ window.executeMetaDSL is not available');
        }
      } catch (e) {
        this.log('✗ MetaDSL execution error: ' + e.message);
        this.log('  Stack: ' + e.stack);
      }
    }

    executeJavascript() {
      this.log('Executing JavaScript...');
      try {
        const script = this.scriptInput.value.trim();
        if (!script) {
          this.log('✗ Input is empty. Please enter JavaScript code first.');
          return;
        }
        this.log('─'.repeat(50));
        this.log('Executing script:');
        this.log(script.substring(0, 200) + (script.length > 200 ? '...' : ''));
        this.log('─'.repeat(50));
        const result = eval(script);
        this.log('✓ Script executed successfully');
        if (result !== undefined) {
          this.log('Result: ' + JSON.stringify(result, null, 2));
        }
      } catch (e) {
        this.log('✗ Script execution error: ' + e.message);
        this.log('  Stack: ' + e.stack);
      }
    }
  }

  // ============================================================================
  // Spellcheck Management
  // ============================================================================
  /**
   * Disable spellcheck globally to prevent Blink rendering crashes
   *
   * Background:
   * - Blink's spellcheck rendering (HighlightPainter::FastPaintSpellingGrammarDecorations)
   *   can crash with DCHECK errors when accessing DOM during layout/paint phase
   * - This happens especially during paste operations or rapid DOM mutations
   * - Disabling spellcheck prevents Blink from entering the problematic code path
   *
   * This is a more reliable solution than trying to disable our monitor during
   * Blink's internal rendering phases, which we cannot fully control.
  /**
   * Globally disable spellcheck on all contenteditable elements to prevent Blink rendering crashes
   * This is a critical workaround for crashes in HighlightPainter::FastPaintSpellingGrammarDecorations
   */
  function disableSpellcheckGlobally() {
    logger.info('Disabling spellcheck globally to prevent Blink crashes...');

    // Disable spellcheck on all existing contenteditable elements
    const editableElements = document.querySelectorAll('[contenteditable="true"]');
    editableElements.forEach(element => {
      element.setAttribute('spellcheck', 'false');
      logger.debug('Disabled spellcheck on element', { id: element.id, className: element.className, tagName: element.tagName });
    });

    // Monitor for new contenteditable elements and disable spellcheck on them
    const spellcheckObserver = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        mutation.addedNodes.forEach((node) => {
          if (node.nodeType === Node.ELEMENT_NODE) {
            // Check if the node itself is contenteditable
            if (node.getAttribute('contenteditable') === 'true') {
              node.setAttribute('spellcheck', 'false');
              logger.debug('Disabled spellcheck on new element', { id: node.id, className: node.className, tagName: node.tagName });
            }

            // Check for contenteditable descendants
            const editableDescendants = node.querySelectorAll('[contenteditable="true"]');
            editableDescendants.forEach(element => {
              element.setAttribute('spellcheck', 'false');
              logger.debug('Disabled spellcheck on new descendant', { id: element.id, className: element.className, tagName: element.tagName });
            });
          }
        });

        // Also check for attribute changes (element becoming contenteditable)
        if (mutation.type === 'attributes' && mutation.attributeName === 'contenteditable') {
          const element = mutation.target;
          if (element.getAttribute('contenteditable') === 'true') {
            element.setAttribute('spellcheck', 'false');
            logger.debug('Disabled spellcheck on modified element', { id: element.id, className: element.className, tagName: element.tagName });
          }
        }
      });
    });

    // Start observing
    spellcheckObserver.observe(document.body, {
      childList: true,
      subtree: true,
      attributes: true,
      attributeFilter: ['contenteditable']
    });

    logger.info('Spellcheck globally disabled, monitoring for new contenteditable elements');
  }

  function initializeAgent() {
    logger.info('Initializing agent...');

    // CRITICAL: Disable spellcheck on all contenteditable elements to prevent Blink rendering crashes
    // This must be done BEFORE any other initialization
    disableSpellcheckGlobally();

    // Create instances (assign to shared variables for window API access)
    bridge = new AgentBridge();
    pageAdapter = new LLMPageAdapter(bridge);
    metadslMonitor = new MetaDSLMonitor(bridge, pageAdapter, metadslWorker);

    panel = null;
    if (CONFIG.agentPanelEnabled) {
      panel = new AgentPanel(bridge, metadslMonitor, pageAdapter);
      // Link panel to monitor for logging
      metadslMonitor.panel = panel;
      // Link panel to logger
      logger.setPanel(panel);
      // Update all child loggers' panel
      metadslMonitor.logger.setPanel(panel);
      bridge.logger.setPanel(panel);
      pageAdapter.logger.setPanel(panel);
      // Set callback for page type changes
      pageAdapter.onPageTypeChanged = (newType) => {
        panel.updateLLMType(newType);
        // Auto-start monitor when page type is detected
        if (newType !== 'unknown' && !metadslMonitor.enabled) {
          metadslMonitor.start();
          logger.info('MetaDSL monitor auto-started after page type detection', { pageType: newType });
          panel.log(`✅ Monitor started for ${newType}`);
          // Update button state to reflect the monitor is now running
          panel.updateMetaDSLButtonState();
        }
      };
    } else {
      // No panel, but still need to start monitor when page type is detected
      pageAdapter.onPageTypeChanged = (newType) => {
        if (newType !== 'unknown' && !metadslMonitor.enabled) {
          metadslMonitor.start();
          logger.info('MetaDSL monitor auto-started after page type detection', { pageType: newType });
        }
      };
    }

    // Keyboard shortcut: Ctrl+Shift+A to toggle panel
    document.addEventListener('keydown', (e) => {
      if (e.ctrlKey && e.shiftKey && (e.key === 'A' || e.key === 'a')) {
        if (panel) {
          panel.toggle();
        }
      }
    });

    // Notify C# that agent is ready
    bridge.sendNotification('agent_ready', {
      pageType: pageAdapter.pageType,
      url: window.location.href
    });

    logger.info('Agent initialization complete', {
      pageType: pageAdapter.pageType,
      url: window.location.href
    });

    // Send agent_need_to_plan notification after initialization
    bridge.sendAgentNeedToPlan('AGENT_INITIALIZED', pageAdapter);

    // Auto-start MetaDSL monitor only if page type is detected
    if (pageAdapter.pageType !== 'unknown') {
      metadslMonitor.start();
      logger.info('MetaDSL monitor auto-started (with code block processing)');
      // Update button state to reflect the monitor is now running
      if (panel) {
        panel.updateMetaDSLButtonState();
      }
    } else {
      logger.info('MetaDSL monitor NOT started - page type is unknown');
      logger.info('Monitor will auto-start after page type is detected');
    }

    logger.info('Initialization complete', { pageType: pageAdapter.pageType });
    logger.info('Use Ctrl+Shift+A to toggle control panel');
    logger.info('Access API via window.AgentAPI');
  }

  // ============================================================================
  // MetaDSLWorkerManager - Dual queue communication with WebSocket for MetaDSL
  // ============================================================================
  class MetaDSLWorkerManager {
    constructor() {
      this.logger = logger.createLogger('MetaDSLWorker');
      this.worker = null;
      this.isRunning = false;
      this.port = CONFIG.websocketPort;
      // Main thread queues
      this.toWorkerQueue = [];      // Messages to send to worker
      this.fromWorkerQueue = [];    // Messages received from worker
      this.tickInterval = null;
      // Auto-reconnect
      this.autoReconnect = true;
      this.isConnected = false;
      this.firstStart = true;       // Track first start for auto-connect
    }

    // Start WebSocket Worker
    start(port) {
      if (port) {
        this.port = port;
      }

      if (this.isRunning) {
        this.logger.warn('MetaDSL Worker is already running');
        return false;
      }

      try {
        // Create worker using externalized code constant
        // const blob = new Blob([WEBSOCKET_WORKER_CODE], { type: 'application/javascript' });
        // const workerUrl = URL.createObjectURL(blob);

        // Blob URL doesn't work with file:// protocol due to security restrictions
        const workerUrl = 'data:application/javascript;charset=utf-8,' + encodeURIComponent(WEBSOCKET_WORKER_CODE);
        // Create worker using Data URL (compatible with file:// protocol)
        this.worker = new Worker(workerUrl);

        // Set up message handler
        this.worker.onmessage = (event) => {
          this.handleWorkerMessage(event.data);
        };

        // Start worker with port
        this.worker.postMessage({ type: 'start', port: this.port });

        // Start main thread tick processing
        this.startTick();

        this.isRunning = true;
        this.firstStart = false;
        this.logger.info('MetaDSL Worker started on port ' + this.port);
        return true;
      } catch (e) {
        this.logger.error('Failed to start MetaDSL Worker: ' + e.message);
        // Schedule reconnect if autoReconnect is enabled
        if (this.autoReconnect) {
          this.scheduleReconnect();
        }
        return false;
      }
    }

    // Stop WebSocket Worker
    stop() {
      if (!this.isRunning) {
        return;
      }

      this.stopTick();

      if (this.worker) {
        this.worker.postMessage({ type: 'stop' });
        setTimeout(() => {
          if (this.worker) {
            this.worker.terminate();
            this.worker = null;
          }
        }, 500);
      }

      // Clear queues
      this.toWorkerQueue = [];
      this.fromWorkerQueue = [];

      this.isRunning = false;
      this.logger.info('WebSocket Worker stopped');
    }

    // Start main thread tick processing
    startTick() {
      this.tickInterval = setInterval(() => {
        this.processQueues();
      }, CONFIG.sendMessageDelay);
    }

    // Stop main thread tick processing
    stopTick() {
      if (this.tickInterval) {
        clearInterval(this.tickInterval);
        this.tickInterval = null;
      }
    }

    // Schedule automatic reconnect
    scheduleReconnect() {
      if (this.reconnectTimeout) {
        clearTimeout(this.reconnectTimeout);
      }
      this.logger.info('Scheduling reconnect in 3 seconds...');
      this.reconnectTimeout = setTimeout(() => {
        this.logger.info('Attempting automatic reconnect...');
        this.stopInternal();
        this.start();
      }, 3000);
    }

    // Stop internal state without logging
    stopInternal() {
      this.stopTick();

      if (this.worker) {
        this.worker.postMessage({ type: 'stop' });
        this.worker.terminate();
        this.worker = null;
      }

      this.isRunning = false;
      this.isConnected = false;
    }

    // Process main thread queues
    processQueues() {
      // Send one message per tick to worker
      if (this.toWorkerQueue.length > 0 && this.worker) {
        const msg = this.toWorkerQueue.shift();
        this.logger.info('Sending message to worker (length: ' + msg.length + '): ' + msg.substring(0, 100) + '...');
        this.worker.postMessage({ type: 'send', message: msg });
      }
    }

    // Handle messages from worker
    handleWorkerMessage(data) {
      if (data.type === 'message') {
        // Queue message from worker
        this.fromWorkerQueue.push(data.data);
        this.logger.info('Message from worker queued (length: ' + data.data.length + '): ' + data.data.substring(0, 100) + '...');
      } else if (data.type === 'connected') {
        this.isConnected = true;
        this.logger.info('MetaDSL Worker connected to server');
      } else if (data.type === 'disconnected') {
        this.isConnected = false;
        this.logger.warn('MetaDSL Worker disconnected from server');
        // Auto-reconnect if enabled
        if (this.autoReconnect) {
          this.scheduleReconnect();
        }
      } else if (data.type === 'error') {
        this.isConnected = false;
        this.logger.error('MetaDSL Worker error: ' + data.error);
        // Auto-reconnect if enabled
        if (this.autoReconnect) {
          this.scheduleReconnect();
        }
      } else if (data.type === 'log') {
        this.logger[data.level]('[Worker] ' + data.message, data.data);
      }
    }

    // Queue message to send via WebSocket (called by C#)
    queueMessage(message) {
      if (!this.isRunning) {
        this.logger.warn('Cannot queue message: Worker not running');
        return false;
      }
      this.toWorkerQueue.push(message);
      this.logger.info('Message queued to send (length: ' + message.length + ', queue size: ' + this.toWorkerQueue.length + ')');
      return true;
    }

    queueReply(message) {
      if (!this.isRunning) {
        this.logger.warn('Cannot queue reply: Worker not running');
        return false;
      }
      this.fromWorkerQueue.push(message);
      this.logger.info('Reply queued to send (length: ' + message.length + ', queue size: ' + this.fromWorkerQueue.length + ')');
      return true;
    }

    // Dequeue message received from WebSocket (called by C#)
    dequeueMessage() {
      if (this.fromWorkerQueue.length > 0) {
        const msg = this.fromWorkerQueue.shift();
        this.logger.info('Message dequeued from receive queue (length: ' + msg.length + ', remaining: ' + this.fromWorkerQueue.length + ')');
        return msg;
      }
      return '';
    }
    // Get queue counts
    getReceiveQueueCount() {
      return this.fromWorkerQueue.length;
    }

    getSendQueueCount() {
      return this.toWorkerQueue.length;
    }
  }

  // Create global MetaDSL Worker manager instance
  const metadslWorker = new MetaDSLWorkerManager();

  // ============================================================================
  // Window API - External interfaces for C# communication
  // ============================================================================

  // Expose global API
  window.AgentAPI = {
    get bridge() { return bridge; },
    get page() { return pageAdapter; },
    get panel() { return panel; },
    get metadsl() { return metadslMonitor; },

    // Convenience methods
    sendToAgent: (cmd, params, callback) => bridge && bridge.sendCommand(cmd, params, callback),
    sendMessage: (text) => pageAdapter && pageAdapter.sendMessage(text),
    setSystemPrompt: (text) => pageAdapter && pageAdapter.setSystemPrompt(text),
    getResponse: () => pageAdapter && pageAdapter.getLastResponse(),
    getHistory: (count) => pageAdapter && pageAdapter.getHistory(count || 5),
    isLastMessageFromLLM: () => pageAdapter && pageAdapter.isLastMessageFromLLM(),
    keepContext: () => metadslMonitor && metadslMonitor.keepContext(CONFIG.llmContextCount),
    showPanel: () => panel && panel.show(),
    hidePanel: () => panel && panel.hide(),
    togglePanel: () => panel && panel.toggle(),
    startMetaDSL: () => metadslMonitor && metadslMonitor.start(),
    stopMetaDSL: () => metadslMonitor && metadslMonitor.stop(),

    // Manual trigger for command detection (useful for testing)
    detectCommands: () => {
      if (metadslMonitor && metadslMonitor.enabled) {
        metadslMonitor.processLastMessage();
      } else {
        logger.warn('MetaDSL monitor is not started. Call startMetaDSL() first.');
      }
    },

    // Clear processed messages cache (useful for testing)
    clearProcessedMessages: () => {
      if (metadslMonitor && metadslMonitor.processedMessages) {
        const count = metadslMonitor.processedMessages.size;
        metadslMonitor.processedMessages.clear();
        if (panel) {
          panel.log(`🗑️ Cleared ${count} processed message(s) from cache`);
        }
        logger.debug(`Cleared ${count} processed messages`);
      }
    },

    // Clear processed code blocks cache (useful for testing)
    clearProcessedBlocks: () => {
      if (metadslMonitor && metadslMonitor.processedBlocks) {
        const count = metadslMonitor.processedBlocks.size;
        metadslMonitor.processedBlocks.clear();
        if (panel) {
          panel.log(`🗑️ Cleared ${count} processed code block(s) from cache`);
        }
        logger.debug(`Cleared ${count} processed code blocks`);
      }
    }
  };

  // Global callback for C# to send agent command responses
  // Called by C#
  window.onAgentResponse = function(responseJson) {
    try {
      logger.debug('Received response from C#', { responseJson });

      const response = JSON.parse(responseJson);

      // Call the callback registered in AgentBridge
      if (bridge) {
        bridge.handleResponse(responseJson);
      }
    } catch (e) {
      logger.error('Error processing agent response', { error: e.toString() });
    }
  };

  // Receive commands from C# (called by C#)
  window.onAgentCommand = function(commandJson) {
    try {
      logger.debug('Received command from C#', { commandJson });

      const cmd = JSON.parse(commandJson);

      // Extract component from params or directly from cmd
      const component = cmd.component || (cmd.params && cmd.params.component);

      // Handle WebSocket commands
      if (cmd.command === 'ws_start' && cmd.params && cmd.params.port) {
        const port = parseInt(cmd.params.port);
        const success = metadslWorker.start(port);
        if (panel) {
          panel.log(success ? 'MetaDSL Worker started on port ' + port : 'Failed to start MetaDSL Worker');
        }
        return;
      }

      if (cmd.command === 'ws_stop') {
        metadslWorker.stop();
        if (panel) {
          panel.log('MetaDSL Worker stopped');
        }
        return;
      }

      if (cmd.command === 'update_system_prompt') {
        if (cmd.params && cmd.params.prompt) {
          this.AgentAPI.setSystemPrompt(cmd.params.prompt);
        }
        return;
      }

      if (cmd.command === 'send_message') {
        if (cmd.params && cmd.params.text) {
          metadslWorker.queueReply(cmd.params.text);
        }
        return;
      }

      // Handle hot reload command
      if (cmd.command === 'hot_reload' && component) {
        logger.info('Processing hot_reload command', { component });

        if (component === 'agentcore') {
          // Send cefQuery to C++ hot_reload_test
          const hotReloadRequest = {
            action: 'hot_reload',
            files: [
              {
                source: 'd:\\GitHub\\BatchCommand\\AgentCore\\bin\\Debug\\net8.0\\AgentCore.dll',
                dest: 'd:\\GitHub\\CEF_Src_Build\\cefclient\\managed\\AgentCore.dll'
              }
            ],
            custom_process_killer: false
          };

          logger.debug('Sending cefQuery for hot_reload', { request: hotReloadRequest });
          logger.debug('About to call cefQuery', {
            cefQueryAvailable: typeof window.cefQuery !== 'undefined',
            request: hotReloadRequest
          });

          if (typeof window.cefQuery !== 'undefined') {
            window.cefQuery({
              request: JSON.stringify(hotReloadRequest),
              persistent: false,
              onSuccess: function(response) {
                logger.info('Hot reload cefQuery success', { response });
                logger.debug('cefQuery success', { response: response });

                if (panel) {
                  panel.log('✓ Hot reload completed: ' + response);
                }
              },
              onFailure: function(error_code, error_message) {
                logger.error('Hot reload cefQuery failed', { error_code, error_message });
                logger.error('cefQuery failed', {
                  error_code: error_code,
                  error_message: error_message
                });

                if (panel) {
                  panel.log('✗ Hot reload failed: ' + error_message);
                }
              }
            });
          } else {
            logger.error('cefQuery not available');
          }
        } else if (component === 'inject') {
          // Reload inject.js
          logger.info('Reloading inject.js');
          if (panel) {
            panel.log('Reloading inject.js');
          }
          window.location.reload();
        }
      }
    } catch (e) {
      logger.error('Error processing agent command', { error: e.toString() });
    }
  };

  // Start when DOM is ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeAgent);
  } else {
    initializeAgent();
  }

})();