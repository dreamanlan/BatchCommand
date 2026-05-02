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
    this.logger.debug('typeof callMetaDSL:', { type: typeof callMetaDSL });

    if (typeof window.cefQuery === 'undefined' && typeof callMetaDSL === 'undefined') {
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
    this.logger.debug('typeof callMetaDSL:', { type: typeof callMetaDSL });

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

    if (this.nativeMode && typeof callMetaDSL !== 'undefined') {
      // Use CEF native API - async call to flatten call stack.
      // Response is delivered asynchronously via send_response_to_inject -> handleResponse.
      this.logger.debug('Calling callMetaDSL handle_agent_command', { message });
      setTimeout(() => {
        callMetaDSL('handle_agent_command', JSON.stringify(message));
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

    if (this.nativeMode && typeof callMetaDSL !== 'undefined') {
      // Async call to flatten call stack
      setTimeout(() => {
        callMetaDSL('handle_agent_notification', JSON.stringify(message));
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
  // JS-side plan decider filters easy cases; only 'trigger_plan' falls through to DSL.
  sendAgentNeedToPlan(state, pageAdapter, queuedCount) {
    if (!this.autoPlanEnabled) {
      this.logger.debug('Auto plan disabled, skipping agent_need_to_plan notification', { state });
      return;
    }

    const data = {
      state: state,
      timestamp: Date.now(),
      lastFromLLM: pageAdapter ? pageAdapter.isLastMessageFromLLM() : false,
      lastScannedMessage: pageAdapter ? (pageAdapter.getLastScannedResponse() ?? '') : '',
      isLastResponse: pageAdapter ? pageAdapter.isLastResponseCurrent() : false,
      queuedCount: queuedCount,
      pageType: pageAdapter ? pageAdapter.pageType : 'unknown',
      count: CONFIG.llmContextCountModuloForAlign,
      lockAgent: this.lockAgentEnabled
    };

    // Lazy-init PlanDecider
    if (!this._planDecider && typeof PlanDecider !== 'undefined') {
      this._planDecider = new PlanDecider(this.logger);
    }

    let decision = { action: 'trigger_plan' };
    if (this._planDecider) {
      try {
        decision = this._planDecider.decide(data) || { action: 'none' };
      } catch (e) {
        this.logger.error('PlanDecider.decide failed, fallback to trigger_plan', { error: e.toString() });
        decision = { action: 'trigger_plan' };
      }
    }

    this.logger.info('Plan decision', { state, action: decision.action, reason: decision.reason });

    switch (decision.action) {
      case 'skip':
      case 'none':
        return;
      case 'reply':
        if (decision.text && typeof metadslWorker !== 'undefined'
          && metadslWorker && typeof metadslWorker.queueReply === 'function') {
          metadslWorker.queueReply(decision.text);
        }
        return;
      case 'command':
        if (decision.command === 'start_agent' && typeof window !== 'undefined'
          && window.AgentAPI && typeof window.AgentAPI.startAgent === 'function') {
          window.AgentAPI.startAgent();
        } else if (decision.command === 'stop_agent' && typeof window !== 'undefined'
          && window.AgentAPI && typeof window.AgentAPI.stopAgent === 'function') {
          window.AgentAPI.stopAgent();
        }
        return;
      case 'trigger_plan':
      default:
        this.logger.info('Sending agent_need_to_plan notification to DSL', { state });
        this.sendNotification('agent_need_to_plan', data);
        return;
    }
  }
}
