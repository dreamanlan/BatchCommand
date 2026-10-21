// ============================================================================
// State Machine Pattern - State Classes
// ============================================================================

// Base State class
class State {
  constructor(monitor) {
    this.monitor = monitor;
  }

  // Lifecycle methods
  enter() { }
  exit() { }

  // Main loop for this state
  async run() { }

  createStateGuard() {
    const token = this.monitor._stateToken;
    return () => this.monitor.started &&
      token != null &&
      this.monitor.currentState === this &&
      this.monitor._stateToken === token;
  }

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
    // Check for loading indicator in local-agent. One combined query instead of
    // two: this runs from several 100ms/500ms loops.
    return document.querySelector('.el-icon.is-loading, #loading-indicator.active') !== null;
  }

  checkCustomLLMResponding() {
    // Check for loading indicator in both custom-llm and local-agent
    return document.querySelector('.el-icon.is-loading, #loading-indicator.active') !== null;
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
    const isCurrent = this.createStateGuard();
    while (this.monitor.currentStateName === 'USER_INPUT') {
      await this.sleep(CONFIG.stateCheckInterval);
      if (!isCurrent()) return;

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
    // Timeout stop retry interval and grace deadline (ms). The grace deadline
    // is the hard cap after a timeout stop: even if generation never stops
    // (stuck loading indicator or stop button not found), the state machine
    // is forced forward instead of deadlocking in LLM_RESPONDING forever.
    this.stopRetryIntervalMs = 5000;
    this.stopGraceMs = 30000;
    // Grace window (ms) for "no generation and no reply" (e.g. the request
    // failed server side): leave LLM_RESPONDING instead of waiting for the
    // full response timeout, which stalls the whole flow.
    this.noOutputGraceMs = 15000;
    // Start window (ms) before the no-output check is armed: the state can be
    // entered from AGENT_EXECUTING ("Agent operation completed") while the
    // page has not started generating yet, so an immediate check would exit
    // while the request is still queued.
    this.startGraceMs = 15000;
  }

  enter() {
    this.info('Entering LLM responding state');
    // Record start time for response timeout check
    this.responseStartTime = Date.now();
    // Entry time for the no-output start window. Note: responseStartTime is
    // cleared once a timeout stop fires, so the start window needs its own.
    this.enterTime = Date.now();
    // Timestamp since generation has been stopped with no reply; 0 = not armed
    this.noOutputSince = 0;
    // Flag: stop button clicked due to timeout, force transition once LLM stops
    this.timeoutStopTriggered = false;
    // Timestamp when the timeout stop was first attempted; 0 = none
    this.timeoutStopAt = 0;
    // Flag: "stuck, please continue" nudge already sent after leaving with no
    // usable reply (timeout stop or generation stopped without any output)
    this.timeoutNudgeSent = false;
    // Flag: this state was left because generation produced no output at all
    this.noOutputExit = false;
    // Timestamp of the last stop click attempt (first try or retry)
    this.stopRetryLastTs = 0;
    // A nudge deferred by the previous round is obsolete: the LLM is already
    // responding again.
    this.monitor.pendingContinueNudge = false;
    // User must have sent a message to trigger LLM response
    this.monitor.onUserSendMessage();
    // Record the last user message node after a short delay (wait for DOM render)
    const isCurrent = this.createStateGuard();
    setTimeout(() => {
      // The state may already be left (the first poll is 500ms too): writing
      // the wrapper then would make a later exit() collapse a message that is
      // no longer the pending agent reply.
      if (!isCurrent()) {
        return;
      }
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

    // Nudge the LLM to continue when auto planning is active, so long-term
    // agent planning resumes immediately after a timeout stop or after a
    // response that produced no output at all.
    // The nudge is only scheduled here and sent by ScanningCodeBlocksState
    // once its scan has finished: sending it now made the next reply start
    // streaming while SCANNING_CODE_BLOCKS was still waiting, and that state's
    // timeout scan then marked the streaming reply seen before its code block
    // rendered, so the block was never executed.
    if ((this.timeoutStopTriggered || this.noOutputExit) && !this.timeoutNudgeSent) {
      this.timeoutNudgeSent = true;
      if (this.monitor.bridge && this.monitor.bridge.autoPlanEnabled) {
        this.info('[LLMRespondingState] AutoPlan on, continue nudge deferred until code block scan finishes');
        this.monitor.pendingContinueNudge = true;
      }
    }
  }

  async run() {
    const isCurrent = this.createStateGuard();
    this.info(`[LLMRespondingState] Starting run loop, pageType=${this.monitor.pageAdapter.pageType}`);

    while (this.monitor.currentStateName === 'LLM_RESPONDING') {
      await this.sleep(CONFIG.llmRespondingCheckInterval);
      if (!isCurrent()) return;

      const isResponding = this.checkLLMResponding();
      const lastFromLLM = this.monitor.pageAdapter.isLastMessageFromLLM();
      this.debug(`[LLMRespondingState] Checking LLM status: isResponding=${isResponding}, lastFromLLM=${lastFromLLM}`);

      if (!isResponding && lastFromLLM) {
        this.info('[LLMRespondingState] LLM finished responding, transitioning to SCANNING_CODE_BLOCKS');
        this.monitor.transitionTo('SCANNING_CODE_BLOCKS', 'LLM finished responding');
        break;
      }

      // If stop was clicked due to timeout and LLM has stopped responding,
      // force transition to SCANNING_CODE_BLOCKS even if no LLM reply arrived.
      // Without this, the state machine gets stuck when LLM was stopped
      // before producing any output (lastFromLLM would be false).
      if (this.timeoutStopTriggered && !isResponding) {
        this.info('[LLMRespondingState] Timeout stop confirmed, forcing transition to SCANNING_CODE_BLOCKS');
        this.monitor.transitionTo('SCANNING_CODE_BLOCKS', 'LLM response timeout stopped');
        break;
      }

      // No generation and no reply at all (e.g. the request failed server
      // side, or the page never started generating). Nothing will ever arrive,
      // so leave the state instead of stalling until the response timeout.
      // The condition must hold continuously: during normal streaming the
      // loading indicator can blink off for a moment before the reply node is
      // inserted. The start window covers the case where the state was entered
      // before the page even began generating.
      if (!isResponding && !lastFromLLM) {
        if (Date.now() - this.enterTime < this.startGraceMs) {
          // Still inside the start window: keep waiting for generation.
          this.noOutputSince = 0;
        } else if (!this.noOutputSince) {
          this.noOutputSince = Date.now();
        } else if (Date.now() - this.noOutputSince >= this.noOutputGraceMs) {
          this.warn(`[LLMRespondingState] No generation and no output for ${this.noOutputGraceMs}ms, transitioning to SCANNING_CODE_BLOCKS`);
          // Set before the transition: exit() sends the continue nudge.
          this.noOutputExit = true;
          this.monitor.transitionTo('SCANNING_CODE_BLOCKS', 'LLM response produced no output');
          break;
        }
      } else {
        this.noOutputSince = 0;
      }

      // Check response timeout
      if (this.responseStartTime) {
        const elapsed = Date.now() - this.responseStartTime;
        const timeoutMs = CONFIG.llmResponseTimeoutMin * 60000;
        if (elapsed >= timeoutMs) {
          this.warn(`[LLMRespondingState] LLM response timeout (${CONFIG.llmResponseTimeoutMin}min elapsed), stopping generation`);
          this.triggerTimeoutStop();
        }
      }

      // Retry the stop click while the page still reports generating after a
      // timeout stop (the first click may have missed the stop button, e.g.
      // it appeared late or the selector needs the page in stoppable state).
      if (this.timeoutStopTriggered && isResponding
        && Date.now() - this.stopRetryLastTs >= this.stopRetryIntervalMs) {
        this.warn('[LLMRespondingState] Still generating after timeout stop, retrying stop click');
        this.stopLLMGeneration();
        // Clear start time and set flag so next iteration force-transitions
        // once checkLLMResponding() returns false (stop took effect).
        this.responseStartTime = null;
        this.timeoutStopTriggered = true;
      }

      // Hard grace deadline: a timeout stop was issued but generation never
      // stopped (stuck loading indicator or stop button not found). Force the
      // transition so the state machine cannot deadlock in LLM_RESPONDING.
      if (this.timeoutStopTriggered
        && Date.now() - this.timeoutStopAt >= this.stopGraceMs) {
        this.warn('[LLMRespondingState] Timeout stop grace period exceeded, forcing transition to SCANNING_CODE_BLOCKS');
        this.monitor.transitionTo('SCANNING_CODE_BLOCKS', 'LLM response timeout stop grace exceeded');
        break;
      }
    }

    this.info('[LLMRespondingState] Run loop exited');
  }

  // Handle response timeout: attempt to click the page stop button and arm
  // the retry / grace deadline logic in the run loop. responseStartTime is
  // cleared so the timeout branch does not re-fire every poll cycle; the
  // retry throttle and grace deadline are driven by timeoutStopTriggered.
  triggerTimeoutStop() {
    this.stopLLMGeneration();
    this.responseStartTime = null;
    this.timeoutStopTriggered = true;
    this.timeoutStopAt = Date.now();
  }

  // Click the page stop button to abort LLM generation.
  // local-agent and custom-llm share the same DOM structure.
  // Returns true when a stop button was found and clicked.
  stopLLMGeneration() {
    this.stopRetryLastTs = Date.now();
    // local-agent: text button #stop-btn
    // custom-llm: SVG icon button, stop = red el-icon (power-off)
    const stopBtn = document.querySelector('#stop-btn:not(.stop-disabled)')
      || document.querySelector('.vac-svg-button i.el-icon[style*="red"]');
    if (stopBtn) {
      this.info('[LLMRespondingState] Clicking stop button to abort generation');
      stopBtn.click();
      return true;
    }
    this.warn('[LLMRespondingState] Stop button not found or disabled, cannot abort generation');
    return false;
  }
}

// Scanning Code Blocks State - waits for page stable scan to complete after LLM reply
class ScanningCodeBlocksState extends State {
  constructor(monitor) {
    super(monitor);
    // Max wait time in ms, recomputed on enter(). The page stable scan needs
    // at least two observations pageStableDelay apart (a block seen for the
    // first time always counts as changing), so the timeout must clearly
    // exceed 2 x pageStableDelay or it wins the race and scans first.
    this.scanTimeout = 20000;
    // Delay of the one extra look the timeout scan takes when blocks were
    // seen for the first time (unstable), before giving up on queuing them.
    this.timeoutRecheckDelay = 1500;
  }

  enter() {
    this.info('Entering scanning code blocks state');
    this.monitor.scanComplete = false;
    this.monitor.resetPageStableTimer();
    this.enterTime = Date.now();
    this.scanTimeout = Math.max(20000, (this.monitor.pageStableDelay || 0) * 4);
  }

  // Send the "stuck, please continue" nudge scheduled by LLMRespondingState,
  // now that this round's code blocks have been scanned. Called right before
  // leaving to AGENT_EXECUTING (no user input monitor runs in this state, so
  // the programmatic send is not mistaken for typing). Skipped when work is
  // already pending: its result reaches the LLM anyway.
  sendPendingContinueNudge() {
    if (!this.monitor.pendingContinueNudge) return;
    this.monitor.pendingContinueNudge = false;
    if (!this.monitor.bridge || !this.monitor.bridge.autoPlanEnabled) return;
    if (this.checkLLMResponding()) {
      this.warn('[ScanningCodeBlocksState] LLM is generating, skip continue nudge');
      return;
    }
    // A draft in the input means the user is there and about to act: a nudge
    // would overwrite what they are typing and is not needed.
    const pa = this.monitor.pageAdapter;
    if (pa && typeof pa.hasUserDraft === 'function' && pa.hasUserDraft()) {
      this.info('[ScanningCodeBlocksState] User is composing a message, skip continue nudge');
      return;
    }
    const worker = this.monitor.metadslWorker;
    const receive = worker && typeof worker.getReceiveQueueCount === 'function'
      ? worker.getReceiveQueueCount() : 0;
    if (this.monitor.operationQueue.length > 0 || receive > 0) {
      this.info('[ScanningCodeBlocksState] Work pending, skip continue nudge');
      return;
    }
    this.info('[ScanningCodeBlocksState] AutoPlan on, sending continue nudge to LLM');
    this.monitor.sendResultToLLM('卡了，请继续');
  }

  exit() {
    this.info('Exiting scanning code blocks state');
    const newConvs = this.monitor.pageAdapter.extractNewConversations();
    if (newConvs.length > 0) {
      this.monitor.bridge.sendNotification('save_conversation_history', { conversations: newConvs, pageType: this.monitor.pageAdapter.pageType });
      this.info(`Sent save_conversation_history notification with ${newConvs.length} new conversation(s)`);
    }
    // Trigger onLLMResponse callback for remote forwarding
    if (this.monitor.onLLMResponse) {
      const resp = this.monitor.pageAdapter.getLastScannedResponse();
      if (resp) {
        // Skip a reply that was already forwarded in an earlier round: a
        // timeout stop / no-output round leaves the previous round's reply
        // as the newest scanned response, and pushing it again duplicates it
        // on the remote side while this round's real reply (if any) is lost.
        const respHash = this.monitor.pageAdapter.hashString(resp);
        if (respHash === this.monitor.lastForwardedResponseHash) {
          this.info('Skip onLLMResponse: scanned response already forwarded (hash match)');
        } else {
          this.monitor.lastForwardedResponseHash = respHash;
          this.info('Triggering onLLMResponse callback');
          this.monitor.onLLMResponse(resp);
        }
      }
    }
  }

  async run() {
    const isCurrent = this.createStateGuard();
    this.info('[ScanningCodeBlocksState] Waiting for code block scan to complete');
    const maxWait = this.enterTime + this.scanTimeout;

    while (this.monitor.currentStateName === 'SCANNING_CODE_BLOCKS') {
      await this.sleep(CONFIG.stateCheckInterval);
      if (!isCurrent()) return;

      if (this.monitor.scanComplete) {
        this.info('[ScanningCodeBlocksState] Scan complete, transitioning to AGENT_EXECUTING');
        this.sendPendingContinueNudge();
        this.monitor.transitionTo('AGENT_EXECUTING', 'Code block scan complete');
        break;
      }

      if (Date.now() > maxWait) {
        // A new reply started streaming while this state was waiting: a scan
        // now would mark the streaming reply seen before its code block
        // renders, and that block would never run. Follow the reply instead;
        // the next SCANNING_CODE_BLOCKS round scans every unseen message.
        if (this.checkLLMResponding()) {
          this.info('[ScanningCodeBlocksState] LLM is generating at scan timeout, back to LLM_RESPONDING without scanning');
          this.monitor.transitionTo('LLM_RESPONDING', 'LLM generating at scan timeout');
          break;
        }
        this.info('[ScanningCodeBlocksState] Scan timeout, forcing scan before transitioning');
        let unstable = this.monitor.scanForNewCodeBlocks();
        if (unstable) {
          // Blocks seen for the first time always count as changing. Take one
          // more look shortly after instead of leaving them unqueued until
          // some later round.
          await this.sleep(this.timeoutRecheckDelay);
          if (!isCurrent()) return;
          if (this.checkLLMResponding()) {
            this.info('[ScanningCodeBlocksState] LLM started generating during recheck, back to LLM_RESPONDING');
            this.monitor.transitionTo('LLM_RESPONDING', 'LLM generating at scan timeout');
            break;
          }
          this.info('[ScanningCodeBlocksState] Blocks unstable at timeout, rechecking once');
          unstable = this.monitor.scanForNewCodeBlocks();
        }
        // The forced scan does not update lastScannedResponse (only
        // onPageStable does). Generation has already stopped (that is how
        // this state was entered), so capture the latest response directly:
        // a stale value would make exit() forward the previous round's reply
        // (or nothing), losing this round's reply on the remote side.
        if (!this.monitor.scanComplete) {
          const latest = this.monitor.pageAdapter.extractLatestResponse();
          const allBoxes = document.querySelectorAll('.vac-message-box:not(.vac-offset-current)');
          const lastBox = allBoxes.length > 0 ? allBoxes[allBoxes.length - 1] : null;
          if (latest && lastBox) {
            this.monitor.pageAdapter.setLastScannedResponse(latest, lastBox);
            this.monitor.scanComplete = true;
            if (unstable) {
              this.warn('[ScanningCodeBlocksState] Blocks still unstable at timeout, captured latest response directly');
            }
          }
        }
        this.sendPendingContinueNudge();
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
    const isCurrent = this.createStateGuard();
    while (this.monitor.currentStateName === 'AGENT_EXECUTING') {
      await this.sleep(CONFIG.stateCheckInterval);
      if (!isCurrent()) return;

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
          this.monitor.handleExecutionReply(item.rawMessage);
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
          let handled = false;
          for (let ix = 0; ix < 10; ++ix) {
            await this.sleep(CONFIG.stateCheckInterval);
            if (!isCurrent()) return;
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
            // Defer planning when work arrived during the wait.
            if (this.monitor.operationQueue.length > 0 ||
                    (this.monitor.metadslWorker &&
                      this.monitor.metadslWorker.getReceiveQueueCount() > 0)) {
                handled = true;
                break;
            }
        }
        if (!handled) {
            if (this.monitor.isInitializing) {
              // Still initializing, wait for user first message before planning
              this.debug('Still initializing, skipping need_to_plan');
            } else {
              // Send notification to Script.dsl to trigger planning. Only a
              // decision that actually reached the agent side counts as
              // executed: a suppressed duplicate is not progress, and treating
              // it as executed made the fallback below (operationExecuteTimeout)
              // bounce the state machine back to USER_INPUT forever, never
              // entering LLM_RESPONDING again.
              const dispatched = this.monitor.bridge.dispatchAgentDecision('AGENT_EXECUTING', this.monitor.panel);
              this.operationExecuted = dispatched;
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

    // A merged command may span several fences, so every block it covers must
    // be marked, annotated and hidden, not just the first one.
    const blocks = (operation.blocks && operation.blocks.length > 0)
      ? operation.blocks
      : [operation.block];

    try {
      if (operation.type === 'execute') {
        // Retry gate: a previous attempt failed to reach the transport, wait
        // out the retry delay before trying again.
        if (operation.nextRetryAt && Date.now() < operation.nextRetryAt) {
          this.monitor.operationQueue.unshift(operation);
          return false;
        }

        // Sending is not proof of delivery or execution.
        blocks.forEach((block) => {
          block.dataset.metadslStatus = 'sending';
        });

        // Execute the code
        this.info('Executing MetaDSL code block...');
        const sent = this.monitor.executeCommand(operation.code, blocks);
        if (sent === false) {
          // The command never reached the transport: leaving the state machine
          // to wait for its reply would stall until the response timeout.
          // Requeue and retry, bounded by maxSendRetries.
          operation.retryCount = (operation.retryCount || 0) + 1;
          operation.nextRetryAt = Date.now() + this.monitor.sendRetryDelay;
          if (operation.retryCount > this.monitor.maxSendRetries) {
            blocks.forEach((block) => {
              block.dataset.metadslStatus = 'failed';
            });
            this.error(`Send failed after ${operation.retryCount - 1} retries, dropping operation ${operation.blockId}`);
            return true;
          }
          this.warn(`Send failed, requeue retry ${operation.retryCount}/${this.monitor.maxSendRetries} for ${operation.blockId}`);
          blocks.forEach((block) => {
            block.dataset.metadslStatus = 'retrying';
          });
          this.monitor.operationQueue.unshift(operation);
          return false;
        }

        // undefined means locally handled; true only means submitted.
        const status = sent === undefined ? 'executed'
          : sent === true ? 'submitted' : 'unknown';
        blocks.forEach((block) => {
          block.dataset.metadslStatus = status;
          this.monitor.addVisualIndicator(block, status);
          if (status === 'executed') {
            this.monitor.scheduleHideContainer(block);
          }
        });

        this.info(`Command status: ${status}`, operation.blockId);

        // Update state display after operation completes
        if (window.agentPanel) {
          window.agentPanel.updateStateDisplay();
        }

        // Return true to mark as executed (wait for next cycle)
        return true;
      } else if (operation.type === 'mark_history') {
        // Mark as history
        blocks.forEach((block) => {
          block.dataset.metadslStatus = 'history';

          // Add visual indicator
          this.monitor.addVisualIndicator(block, 'history');

          // Schedule hiding the container after 3 seconds
          this.monitor.scheduleHideContainer(block);
        });

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
      if (operation.type === 'execute') {
        blocks.forEach((block) => {
          if (block) block.dataset.metadslStatus = 'unknown';
        });
      }
      // Delivery is ambiguous. Do not automatically resend.
      return true;
    } finally {
      this.monitor.resumeDeferredScan();
    }

    // Default: mark as executed
    return true;
  }
}
