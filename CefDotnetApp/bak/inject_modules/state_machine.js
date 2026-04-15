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
      this.contextCounterForKeep = 0;
      this.contextCounterForAlign = 0;
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
      this.contextCounterForKeep = (this.contextCounterForKeep + 1) % CONFIG.llmContextCountModuloForKeep;
      this.debug(`Context counter: ${this.contextCounterForKeep}`);

      this.contextCounterForAlign = (this.contextCounterForAlign + 1) % CONFIG.llmContextCountModuloForAlign;
      this.debug(`Context counter: ${this.contextCounterForAlign}`);

      // When counter reaches 0, trigger context countdown notification
      if (this.contextCounterForKeep === 0) {
        this.info('Context counter reached 0, keep context');
        this.monitor.keepContext(CONFIG.llmContextCountModuloForKeep);
      }
      if (this.contextCounterForAlign === 0) {
        this.info('Context counter reached 0, align target');
        this.monitor.alignTarget(CONFIG.llmContextCountModuloForAlign);
      }
    }

    exit() {
      this.info('Exiting LLM responding state');
      // Replace pending agent reply message with collapsed text
      const msgBox = this.monitor.pendingAgentMessageWrapper;
      if (msgBox && this.monitor.isAgentMessage(msgBox) && CONFIG.get('panel.collapseAgentReply')) {
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
          this.info('[LLMRespondingState] LLM finished responding, transitioning to SCANNING_CODE_BLOCKS');
          this.monitor.transitionTo('SCANNING_CODE_BLOCKS', 'LLM finished responding');
          break;
        }
      }

      this.info('[LLMRespondingState] Run loop exited');
    }
  }

  // Scanning Code Blocks State - waits for page stable scan to complete after LLM reply
  class ScanningCodeBlocksState extends State {
    constructor(monitor) {
      super(monitor);
      this.scanTimeout = 10000; // max wait time in ms
    }

    enter() {
      this.info('Entering scanning code blocks state');
      this.monitor.scanComplete = false;
      this.monitor.resetPageStableTimer();
      this.enterTime = Date.now();
    }

    exit() {
      this.info('Exiting scanning code blocks state');
      const newConvs = this.monitor.pageAdapter.extractNewConversations();
      if (newConvs.length > 0) {
        this.monitor.bridge.sendNotification('save_conversation_history', { conversations: newConvs });
        this.info(`Sent save_conversation_history notification with ${newConvs.length} new conversation(s)`);
      }
      // Trigger onLLMResponse callback for remote forwarding
      if (this.monitor.onLLMResponse) {
        const resp = this.monitor.pageAdapter.getLastScannedResponse();
        if (resp) {
          this.info('Triggering onLLMResponse callback');
          this.monitor.onLLMResponse(resp);
        }
      }
    }

    async run() {
      this.info('[ScanningCodeBlocksState] Waiting for code block scan to complete');
      const maxWait = this.enterTime + this.scanTimeout;

      while (this.monitor.currentStateName === 'SCANNING_CODE_BLOCKS') {
        await this.sleep(CONFIG.stateCheckInterval);

        if (this.monitor.scanComplete) {
          this.info('[ScanningCodeBlocksState] Scan complete, transitioning to AGENT_EXECUTING');
          this.monitor.transitionTo('AGENT_EXECUTING', 'Code block scan complete');
          break;
        }

        if (Date.now() > maxWait) {
          this.info('[ScanningCodeBlocksState] Scan timeout, forcing scan before transitioning');
          this.monitor.scanForNewCodeBlocks();
          this.monitor.transitionTo('AGENT_EXECUTING', 'Code block scan timeout');
          break;
        }
      }

      this.info('[ScanningCodeBlocksState] Run loop exited');
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
      this.operationExecuteTime = Date.now();

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
          const item = this.monitor.metadslWorker.dequeueMessage();
          if (item) {
            this.info('Received MetaDSL execution result from C#');
            if (item.channelId && window.Relay && window.Relay.ws) { window.Relay.ws._lastChannelId = item.channelId; }
            // Send result directly to LLM, respecting noAgentMarker flag
            this.monitor.sendResultToLLM(item.message, item.noAgentMarker);
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
            this.operationExecuteTime = Date.now();
          } finally {
            // Restart monitoring after operation completes
            this.monitor.userInputMonitor.start();
            // Reset the hasUserInput flag since we just executed an operation
            this.monitor.userInputMonitor.reset();
          }
        }

        // 5. Check if agent needs to plan (queue empty and no operation executed)
        if (this.monitor.operationQueue.length === 0) {
          if (this.operationExecuted) {
            if (Date.now() - this.operationExecuteTime > CONFIG.operationExecuteTimeout) {
              this.info(`Operation executed for too long, transitioning to USER_INPUT`);
              this.monitor.transitionTo('USER_INPUT', 'User started typing');
              break;
            }
          }
          else {
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
              if (this.monitor.isInitializing) {
                // Still initializing, wait for user first message before planning
                this.debug('Still initializing, skipping need_to_plan');
              } else {
                // Send notification to Script.dsl to trigger planning
                this.monitor.bridge.sendAgentNeedToPlan('AGENT_EXECUTING', this.monitor.pageAdapter, this.monitor.operationQueue.length);
                // Mark as executed to avoid sending notification repeatedly
                this.operationExecuted = true;
                this.operationExecuteTime = Date.now();
              }
            }
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
