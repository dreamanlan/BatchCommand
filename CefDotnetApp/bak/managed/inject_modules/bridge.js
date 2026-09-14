// ============================================================================
// AgentBridge - Communication Bridge with C#
// ============================================================================
class AgentBridge {
  constructor() {
    this.logger = logger.createLogger('AgentBridge');
    this.commandId = 0;
    this.callbacks = new Map();
    this.autoPlanEnabled = true; // Auto plan enabled by default
    this._lockUntil = 0; // Lock deadline timestamp (ms); 0 means not locked

    // Initialize CEF native API
    this.initNativeApi();
  }

  // Lock agent state is driven by a deadline. It stays locked only until
  // _lockUntil; reading it after the deadline auto-clears the lock.
  get lockAgentEnabled() {
    if (this._lockUntil > 0 && Date.now() >= this._lockUntil) {
      this._lockUntil = 0;
    }
    return this._lockUntil > 0;
  }

  // Setting true locks for lockTimeMin minutes from now; false unlocks.
  set lockAgentEnabled(val) {
    if (val) {
      const min = CONFIG.lockTimeMin || 0;
      this._lockUntil = Date.now() + min * 60000;
    } else {
      this._lockUntil = 0;
    }
  }

  get lockUntil() {
    if (this._lockUntil > 0 && Date.now() >= this._lockUntil) {
      this._lockUntil = 0;
    }
    return this._lockUntil;
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

  getUserName() {
    if (this.nativeMode && typeof callMetaDSL !== 'undefined') {
      return callMetaDSL('get_user_name');
    } else {
      return 'unknown';
    }
  }

  // Send command to agent backend. Channel priority: the relay transport
  // (browser process -> standalone AgentCore, see relay_transport.js), then
  // the legacy websocket worker, then the legacy callMetaDSL path (old
  // in-process deployments). The relay registers lazily on first use; until
  // it is up the previous behavior is unchanged.
  _isRelayReady() {
    return typeof relayTransport !== 'undefined' && relayTransport
      && relayTransport.isRunning && relayTransport.isConnected;
  }

  // Relay registration in flight (auto-registers on bundle load): the message
  // should wait for it instead of falling through to callMetaDSL, which is
  // broken in the new deployment (the renderer process has no agent apis).
  _isRelayPending() {
    return typeof relayTransport !== 'undefined' && relayTransport
      && relayTransport.relayAvailable && !relayTransport.isRunning;
  }

  _maybeStartRelay() {
    if (this._relayTried) {
      return;
    }
    this._relayTried = true;
    if (typeof relayTransport !== 'undefined' && relayTransport && relayTransport.relayAvailable) {
      relayTransport.register();
    }
  }

  sendCommand(cmd, params, callback) {
    this.logger.debug('sendCommand called', { cmd, params });

    if (!callback) {
      callback = () => { };
    }

    const commandId = ++this.commandId;
    this.callbacks.set(commandId, callback);

    // Timeout backstop (design doc §3.5): if the agent_result never arrives
    // (message dropped, relay connection lost mid-flight), fail the callback
    // instead of leaving it pending forever. handleResponse deletes the
    // entry on success, so an answered command never hits this.
    const self = this;
    setTimeout(function () {
      const cb = self.callbacks.get(commandId);
      if (cb) {
        self.callbacks.delete(commandId);
        self.logger.warn('sendCommand timed out', { cmd: cmd, commandId: commandId });
        try {
          cb(false, null, 'timeout: no agent_result within 30s');
        } catch (e) {
          self.logger.error('timeout callback error', { error: e.toString() });
        }
      }
    }, 30000);

    this._dispatchCommand(commandId, {
      id: commandId,
      command: cmd,
      params: params || {}
    }, 0);

    return commandId;
  }

  _dispatchCommand(commandId, message, attempt) {
    if (this._isRelayReady()) {
      const envelope = {
        type: 'agent_call',
        id: commandId,
        func: 'handle_agent_command',
        args: [JSON.stringify(message)]
      };
      if (relayTransport.queueMessage(JSON.stringify(envelope))) {
        return;
      }
      // Relay dropped the message (disconnect race): fall through.
    } else {
      this._maybeStartRelay();
    }

    // Relay still connecting: wait and retry instead of dropping (a fresh
    // browser needs ~15s to boot the standalone AgentCore).
    if (attempt < 60 && this._isRelayPending()) {
      const self = this;
      setTimeout(function () {
        self._dispatchCommand(commandId, message, attempt + 1);
      }, 300);
      return;
    }

    // No channel available: fail the callback (the renderer has no agent
    // apis in this architecture, there is no callMetaDSL fallback).
    this.logger.warn('No channel available - command not sent', { message });
    const cb = this.callbacks.get(commandId);
    if (cb) {
      this.callbacks.delete(commandId);
      setTimeout(function () {
        cb(false, null, 'No channel available');
      }, 100);
    }
  }

  // Send notification (no response expected)
  sendNotification(type, data) {
    // P2: attach the js state block so the agent side (standalone AgentCore)
    // can read queue counts / llm category from the notification instead of
    // the old CallJavascriptFuncInRenderer queries (agent-side renderer api).
    data = data || {};
    if (data.jsState === undefined) {
      try {
        const api = (typeof window !== 'undefined') ? window.AgentAPI : null;
        data.jsState = {
          operationQueueCount: (api && api.getOperationQueueCount) ? api.getOperationQueueCount() : 0,
          sendQueueCount: (api && api.getSendQueueCount) ? api.getSendQueueCount() : 0,
          receiveQueueCount: (api && api.getReceiveQueueCount) ? api.getReceiveQueueCount() : 0,
          llmCategory: (api && api.getLLMCategory) ? api.getLLMCategory() : ''
        };
      } catch (e) {
        data.jsState = { operationQueueCount: 0, sendQueueCount: 0, receiveQueueCount: 0, llmCategory: '' };
      }
    }

    const message = {
      type: type,
      data: data
    };

    this._dispatchNotification(message, 0);
  }

  _dispatchNotification(message, attempt) {
    if (this._isRelayReady()) {
      const envelope = {
        type: 'agent_notify',
        func: 'handle_agent_notification',
        args: [JSON.stringify(message)]
      };
      if (relayTransport.queueMessage(JSON.stringify(envelope))) {
        return;
      }
      // Relay dropped the message (disconnect race): fall through.
    } else {
      this._maybeStartRelay();
    }

    if (attempt < 60 && this._isRelayPending()) {
      const self = this;
      setTimeout(function () {
        self._dispatchNotification(message, attempt + 1);
      }, 300);
      return;
    }

    this.logger.warn('No channel available - notification dropped', { message });
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

  // Send agent decision notification (encapsulated for reuse)
  // JS-side decider filters easy cases; 'trigger_decision' falls through to DSL.
  dispatchAgentDecision(state, panel, force) {
    let pageAdapter = panel.pageAdapter;
    let metadslWorker = panel.metadslWorker;
    const data = {
      state: state,
      timestamp: Date.now(),
      lastFromLLM: pageAdapter ? pageAdapter.isLastMessageFromLLM() : false,
      lastScannedMessage: pageAdapter ? (pageAdapter.getLastScannedResponse() ?? '') : '',
      isLastResponse: pageAdapter ? pageAdapter.isLastResponseCurrent() : false,
      pageType: pageAdapter ? pageAdapter.pageType : 'unknown',
      count: CONFIG.llmContextCountModuloForAlign,
      autoPlan: this.autoPlanEnabled,
      lockAgent: this.lockAgentEnabled
    };

    // Lazy-init ResponseDecider
    if (!this._responseDecider && typeof ResponseDecider !== 'undefined') {
      this._responseDecider = new ResponseDecider(this.logger);
    }

    let decision = { action: 'trigger_decision' };
    if (this._responseDecider) {
      try {
        decision = this._responseDecider.decide(data, panel) || { action: 'none' };
      } catch (e) {
        this.logger.error('ResponseDecider.decide failed, fallback to trigger_decision', { error: e.toString() });
        decision = { action: 'trigger_decision' };
      }
    }

    this.logger.info('Agent decision', { state, action: decision.action, reason: decision.reason });

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
        if (decision.command === 'start_auto_plan' && typeof window !== 'undefined'
          && window.AgentAPI && typeof window.AgentAPI.startAutoPlan === 'function') {
          window.AgentAPI.startAutoPlan();
        } else if (decision.command === 'stop_auto_plan' && typeof window !== 'undefined'
          && window.AgentAPI && typeof window.AgentAPI.stopAutoPlan === 'function') {
          window.AgentAPI.stopAutoPlan();
        }
        return;
      case 'trigger_decision':
      default: {
        const curMsg = data.lastScannedMessage || '';
        if (!force && curMsg && this._lastTriggerDecisionMsg === curMsg) {
          this.logger.info('Skip duplicate trigger_decision (same message)', {
            state,
            msgPrefix: curMsg.substring(0, 30)
          });
          return;
        }
        this._lastTriggerDecisionMsg = curMsg;
        this.logger.info('Sending agent_need_to_decide notification to DSL', { state, force: !!force });
        this.sendNotification('agent_need_to_decide', data);
        return;
      }
    }
  }
}
