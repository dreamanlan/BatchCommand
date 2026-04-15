// ============================================================================
// AgentBridge - Communication Bridge with C#
// ============================================================================
class AgentBridge {
  constructor() {
    this.logger = logger.createLogger('AgentBridge');
    this.commandId = 0;
    this.callbacks = new Map();
    this.autoPlanEnabled = true; // Auto plan enabled by default
    this.lockAgentEnabled = false; // Lock agent disabled by default

    // Initialize CEF native API
    this.initNativeApi();
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
      callback = () => { };
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
  sendAgentNeedToPlan(state, pageAdapter, queuedCount) {
    if (!this.autoPlanEnabled) {
      this.logger.debug('Auto plan disabled, skipping agent_need_to_plan notification', { state });
      return;
    }
    this.logger.info('Sending agent_need_to_plan notification', { state });
    this.sendNotification('agent_need_to_plan', {
      state: state,
      timestamp: Date.now(),
      lastFromLLM: pageAdapter ? pageAdapter.isLastMessageFromLLM() : false,
      lastScannedMessage: pageAdapter ? (pageAdapter.getLastScannedResponse() ?? '') : '',
      isLastResponse: pageAdapter ? pageAdapter.isLastResponseCurrent() : false,
      queuedCount: queuedCount,
      pageType: pageAdapter.pageType,
      count: CONFIG.llmContextCountModuloForAlign,
      lockAgent: this.lockAgentEnabled
    });
  }
}
