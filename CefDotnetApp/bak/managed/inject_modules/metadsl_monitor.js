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
    this._runGeneration = 0; // Invalidates a previous runStateMachine loop
    this.processedMessages = new Set();
    this.processedBlocks = new Set();
    this.panel = null;
    this.observer = null;
    this.sendButtonObserver = null;
    this.canExecuteNewCommands = false; // Only execute when send button is enabled
    this.maxProcessedBlocks = CONFIG.maxProcessedBlocks;
    this.isMarkingHistory = false; // Flag to prevent observer from triggering during history marking
    this.isInitializing = true; // Flag to indicate initialization phase, prevents queueing history blocks
    // Format warning for the current reply, reported to the LLM after the
    // merged command has been queued (null when there is nothing to report).
    this.pendingValidationWarning = null;
    this.hasEverInitialized = false; // Track if first initialization has completed, survives stop/start

    // Operation queue mechanism
    this.operationQueue = [];
    this.pageStableTimer = null;
    this.isProcessingQueue = false;
    this.pageStableDelay = CONFIG.config.panel.streamingPage ? CONFIG.pageStableDelay : 1500;
    // Timeout fallback for onPageStable's unrendered-fence wait: if a residual
    // (unpairable / malformed) code fence lingers, count retries and force-continue
    // after the limit to avoid a permanent deadlock (no self-heal).
    this.unrenderedFenceRetryCount = 0;
    this.unrenderedFenceRetryLimit = 3;
    // Same fallback for onPageStable's "still generating" wait: a stuck loading
    // indicator would reschedule the scan forever. Timestamp of the first
    // "still generating" observation in the current round; 0 = not armed.
    this.generatingSince = 0;

    // Block ID cache. Keyed by DOM node: the id is derived from the message
    // fingerprint, which changes while the message is still streaming, so the
    // cache only holds the value computed for this very node.
    this.blockIdCache = new WeakMap();
    this.nextBlockId = 0;

    // Fingerprints of the messages already handled, in document order. Every
    // scan aligns the current message sequence against this one to tell newly
    // arrived replies from re-loaded history (see alignMessages). Nothing else
    // is used to recognize a message: the page rebuilds its DOM, and wrapper
    // ids are not stable when history messages are loaded.
    this.seenFingerprints = [];
    // Per-round cache of newMessageWrappers() (see onPageStable).
    this._newWrappers = null;
    this._roundAlign = null;

    // Track last content of code blocks to detect if still changing.
    // Keyed by DOM node (a WeakMap) because a block id is not enough: the same
    // id can survive a re-render while the node is a different one.
    this.lastBlockContent = new WeakMap();

    // Create user input monitor
    this.userInputMonitor = new UserInputMonitor(pageAdapter);

    // Scan complete flag - set by onPageStable after scanForNewCodeBlocks
    this.scanComplete = false;

    // State machine - using State Pattern
    this.states = {
      USER_INPUT: new UserInputState(this),
      LLM_RESPONDING: new LLMRespondingState(this),
      SCANNING_CODE_BLOCKS: new ScanningCodeBlocksState(this),
      AGENT_EXECUTING: new AgentExecutingState(this)
    };
    this.currentState = null;
    this.currentStateName = null; // Will be set by transitionTo in start()
    this.isTransitioning = false; // Prevent concurrent state transitions
    this.stateHistory = [];
    this.maxSendRetries = CONFIG.maxSendRetries;
    this.sendRetryDelay = CONFIG.sendRetryDelay;
    // Upper bound for the operation queue (see enqueueOperation).
    this.maxOperationQueue = 100;
    // Upper bound for processedMessages: unlike processedBlocks (trimmed by
    // maxProcessedBlocks) it grew for the whole session.
    this.maxProcessedMessages = 2000;

    // Callback for LLM response forwarding (set by main.js)
    this.onLLMResponse = null;
  }

  // Safely extract className as a string, compatible with SVG elements
  // (SVGElement.className is an SVGAnimatedString, not a string) and non-Element nodes.
  _getClassStr(node) {
    const c = node && node.className;
    if (typeof c === 'string') return c;
    if (c && typeof c.baseVal === 'string') return c.baseVal;
    return '';
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

    // Reset initialization flags for proper restart behavior
    this.isInitializing = true;
    this.canExecuteNewCommands = false;

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

    // Poll the renderer process footprint (the dsl owns the threshold, this
    // side owns the schedule and the restart)
    this.startMemoryGuard();
  }

  stop() {
    if (!this.started) {
      this.warn('Not started');
      return;
    }

    this.info('Stopping...');
    this.started = false;
    this.enabled = false;
    // Drop the current state so a later start() re-enters AGENT_EXECUTING
    // (transitionTo() would otherwise short-circuit on the same name and skip
    // enter(), leaving the input monitor stopped) and so the old run loop,
    // which may be parked in await, stops driving it.
    this.currentState = null;
    this.currentStateName = null;
    this.canExecuteNewCommands = false;
    this.stopMemoryGuard();
    this.processedMessages.clear();
    // Keep processedBlocks to prevent re-processing on restart
    // processedBlocks is also backed by DOM data-metadsl-status attribute
    // WeakMap has no clear(): drop the whole map.
    this.lastBlockContent = new WeakMap();

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
    // Bound the set: it is only cleared on stop(), so a long session grew it
    // without limit (processedBlocks is trimmed by maxProcessedBlocks).
    if (this.processedMessages.size > this.maxProcessedMessages) {
      const oldest = this.processedMessages.values().next().value;
      this.processedMessages.delete(oldest);
    }

    // Response processed - MetaDSL code blocks will be handled by scanForNewCodeBlocks()
    // No command extraction here - MetaDSL only executes from code blocks
  }

  processLastMessage() {
    const response = this.pageAdapter.extractLatestResponse();
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

  alignTarget(count) {
    const operation = {
      type: 'send_notification',
      notificationType: 'llm_align_target',
      data: {
        pageType: this.pageAdapter.pageType,
        count: count
      }
    };
    this.enqueueOperation(operation);
  }

  startAutoPlan() {
    if (this.panel.bridge.autoPlanEnabled) {
      this.sendResultToLLM('Auto Plan already started');
      return;
    }
    this.panel.toggleAutoPlan();
    this.sendResultToLLM('Auto Plan started');
  }
  stopAutoPlan() {
    if (this.panel.bridge.lockAgentEnabled) {
      this.sendResultToLLM('Lock Agent is ON: user has set long-term development mode, agent planning must not be stopped.');
      return;
    }
    if (!this.panel.bridge.autoPlanEnabled) {
      this.sendResultToLLM('Auto Plan already stopped');
      return;
    }
    this.panel.toggleAutoPlan();
    this.sendResultToLLM('Auto Plan stopped');
  }
  keepContext(count) {
    const operation = {
      type: 'send_notification',
      notificationType: 'llm_context_count_down',
      data: {
        pageType: this.pageAdapter.pageType,
        count: count
      }
    };
    this.enqueueOperation(operation);
  }
  triggerReflection() {
    const operation = {
      type: 'send_notification',
      notificationType: 'episodic_reflection',
      data: {
        pageType: this.pageAdapter.pageType
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

    // The observer only re-checks the send button, which just logs at debug
    // level. On pages without #room-footer it would fall back to document.body
    // and turn every class change on the page into a full-document query for
    // nothing, so it is skipped there. The click listener below is kept: it is
    // what actually detects that the user sent a message.
    if (footer !== document.body) {
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
    } else {
      this.info('Send button class observer skipped: #room-footer not found');
    }

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
      // First dialog, update system prompt
      this.updateSystemPrompt();
      // Only send AGENT_INITIALIZED on first start, skip on restart
      if (!this.hasEverInitialized) {
        this.hasEverInitialized = true;
        this.bridge.dispatchAgentDecision('AGENT_INITIALIZED', this.panel, false);
      } else {
        this.info('Restart detected - skipping AGENT_INITIALIZED notification');
      }
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
    // Main state machine execution loop. The generation counter keeps a
    // stop()/start() pair that happens while this loop is parked in await from
    // leaving two concurrent loops driving the same states.
    const generation = ++this._runGeneration;
    while (this.started && generation === this._runGeneration) {
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
    // Keep it on the instance: the control panel disconnects the observer
    // while the user types in the script input and re-attaches it from
    // this.chatContainer. Without this the observer stayed disconnected
    // forever after the first keystroke.
    this.chatContainer = chatContainer;
    const specificContainer = document.querySelector('.chat-container') ||
      document.querySelector('.message-container') ||
      document.querySelector('[role="main"]');
    this.info(`startCodeBlockObserver: using body, specificContainer=${specificContainer ? specificContainer.tagName + '.' + this._getClassStr(specificContainer).slice(0, 40) : 'none'}`);

    this.observer = new MutationObserver((mutations) => {
      if (!this.started) {
        return;
      }

      // Ignore mutations during history marking to prevent initial scan
      if (this.isMarkingHistory) {
        this.debug('Ignoring mutations during history marking');
        return;
      }

      // Filter out mutations from the control panel to avoid crashes.
      // The panel node is resolved once per batch: walking up the ancestors of
      // every mutation record is one of the hottest paths during streaming.
      const panelEl = document.getElementById('agent-control-panel');
      const relevantMutations = mutations.filter(mutation => {
        if (panelEl && panelEl.contains(mutation.target)) {
          return false; // Ignore mutations inside panel
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
        this.info(`relevantMutations[0]: type=${m0.type}, target=${(t0.tagName || t0.nodeName || '')}.${this._getClassStr(t0).slice(0, 40)}, id=${t0.id || ''}, addedNodes=${m0.addedNodes.length}, removedNodes=${m0.removedNodes.length}`);

        // Reset page stable timer
        this.resetPageStableTimer();
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
        if (node.id === 'agent-control-panel') {
          return false; // Exclude blocks inside panel
        }
        node = node.parentElement;
      }
      return true;
    });

    codeBlocks.forEach((block) => {
      const rawCode = block.textContent || '';
      const metadslCode = this.extractMetaDSLCode(rawCode);

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

    // Mark all existing wrappers as already saved (no need to save history conversations)
    document.querySelectorAll('.vac-message-wrapper').forEach(wrapper => {
      wrapper.dataset.agentSaved = '1';
    });

    // Seed the known message sequence: every message currently on the page is
    // history, so a later scan only executes blocks inside messages that
    // appear after these ones.
    const seedWrappers = this.pageAdapter.getMessageWrappers
      ? this.pageAdapter.getMessageWrappers()
      : Array.from(document.querySelectorAll('.vac-message-wrapper'));
    this.seenFingerprints = seedWrappers.map(w => {
      try {
        return this.pageAdapter.getMessageFingerprint(w);
      } catch (e) {
        return null;
      }
    }).filter(Boolean);
    this.info(`Seeded ${this.seenFingerprints.length} known messages from the current page`);

    // Collapse all existing agent reply messages on page
    this.collapseHistoryAgentMessages();

    // Clear isMarkingHistory flag immediately after DOM modifications
    // isInitializing will be cleared when user first interacts (enters USER_INPUT state)
    this.isMarkingHistory = false;
    this.debug('History marking complete, waiting for user interaction to finish initialization');
  }

  // Align the message sequence currently on the page against the fingerprints
  // already known (seenFingerprints).
  //
  // Messages are time ordered, so a re-loaded history block is inserted in
  // FRONT of the messages we already know, and a new reply is appended at the
  // END. The known sequence is therefore matched against the tail of the
  // current one:
  //   - fingerprints at the end of the page that are not known -> new messages
  //   - fingerprints in front of the aligned block             -> re-loaded history
  //
  // Uniqueness of a fingerprint is not required: two identical short replies
  // share one and are told apart by their position in the sequence.
  //
  // Returns { wrappers, fingerprints, newIdx, matched, reseeded } where newIdx
  // holds indices into wrappers/fingerprints, oldest first.
  // matched === 0 while the known sequence is not empty means the two have
  // nothing in common (the conversation was replaced, the window moved away
  // from the newest messages, ...). The known sequence is then re-seeded from
  // the page and every message counts as history - the safe direction: never
  // execute what cannot be recognized.
  alignMessages() {
    const wrappers = this.pageAdapter.getMessageWrappers
      ? this.pageAdapter.getMessageWrappers()
      : Array.from(document.querySelectorAll('.vac-message-wrapper'));
    const fingerprints = wrappers.map(w => {
      try {
        return this.pageAdapter.getMessageFingerprint(w);
      } catch (e) {
        return null;
      }
    });
    const seen = this.seenFingerprints;
    const newIdx = [];
    let i = fingerprints.length - 1;
    let j = seen.length - 1;
    let matched = 0;
    while (i >= 0 && j >= 0) {
      if (fingerprints[i] && fingerprints[i] === seen[j]) {
        matched++; i--; j--;
      } else if (matched === 0) {
        // Not part of the known sequence: a message that arrived after it.
        newIdx.push(i); i--;
      } else {
        break;
      }
    }
    newIdx.reverse();

    if (matched === 0 && seen.length > 0) {
      this.warn('Known message sequence not found on the page; re-seeding from the current messages (all treated as history)');
      this.seenFingerprints = fingerprints.filter(Boolean);
      return { wrappers, fingerprints, newIdx: [], matched: 0, reseeded: true };
    }
    return { wrappers, fingerprints, newIdx, matched, reseeded: false };
  }

  // Remember the messages handled by this scan as known, so the next scan
  // aligns them instead of treating them as new.
  markMessagesAsSeen(align) {
    if (!align || !align.newIdx.length) return;
    for (const k of align.newIdx) {
      const fp = align.fingerprints[k];
      // Duplicates are kept on purpose: two identical short replies share a
      // fingerprint and are told apart by their position in the sequence, so
      // the known sequence has to hold one entry per message.
      if (fp) {
        this.seenFingerprints.push(fp);
      }
    }
    const limit = this.maxProcessedMessages || 2000;
    if (this.seenFingerprints.length > limit) {
      this.seenFingerprints.splice(0, this.seenFingerprints.length - limit);
    }
  }

  // Annotate a code block as history: it is never queued for execution. The
  // attribute also lets page_adapter collapse the block when the conversation
  // is saved, so the visual mark and the archive stay in sync.
  markBlockAsHistory(block) {
    if (!block || block.dataset.metadslStatus) return;
    block.dataset.metadslStatus = 'history';
    block.style.borderLeft = '3px solid #9E9E9E';
    block.style.backgroundColor = 'rgba(158, 158, 158, 0.05)';
    this.scheduleHideContainer(block);
  }

  // Wrappers of the messages that arrived after the already known sequence,
  // as a Set so callers can test membership. Cached for one scan round: the
  // validation step and the code block scan must see the same answer.
  newMessageWrappers() {
    if (!this._newWrappers) {
      const align = this.alignMessages();
      this._roundAlign = align;
      this._newWrappers = new Set(align.newIdx.map(k => align.wrappers[k]));
    }
    return this._newWrappers;
  }

  // ========================================================================
  // DOM Pruning - Remove old conversation messages to prevent page slowdown
  // ========================================================================

  pruneOldMessages() {
    const maxRounds = CONFIG.get('panel.maxConversationRounds');
    if (!maxRounds || maxRounds <= 0) return;

    const allWrappers = Array.from(document.querySelectorAll('.vac-message-wrapper'));
    if (allWrappers.length === 0) return;

    // Count conversation rounds from the end (a round = user message + LLM reply)
    // Walk backwards to find the cut-off point
    let roundCount = 0;
    let cutoffIndex = 0; // wrappers before this index will be removed

    for (let i = allWrappers.length - 1; i >= 0; i--) {
      const box = allWrappers[i].querySelector('.vac-message-box');
      if (!box) continue;
      const isUser = box.classList.contains('vac-offset-current');

      if (isUser) {
        roundCount++;
        if (roundCount > maxRounds) {
          cutoffIndex = i + 1; // keep from i+1 onwards (this user msg is the (maxRounds+1)th round)
          break;
        }
      }
    }

    // If we didn't exceed maxRounds, nothing to prune
    if (roundCount <= maxRounds) return;

    // Also include any leading LLM messages before the cutoff user message
    // (orphan LLM replies without a preceding user message in the kept range)
    // cutoffIndex already points to the first wrapper to keep, so remove 0..cutoffIndex-1
    if (cutoffIndex <= 0) return;

    const toRemove = allWrappers.slice(0, cutoffIndex);
    this.info(`Pruning ${toRemove.length} old message wrappers (keeping last ${maxRounds} rounds)`);

    for (const wrapper of toRemove) {
      wrapper.remove();
    }

    this.info(`DOM pruning complete, removed ${toRemove.length} wrappers`);
  }

  // ========================================================================
  // Operation Queue Management
  // ========================================================================

  hasUnrenderedCodeFences() {
    // Check if the last LLM message still has raw ``` fences that haven't been
    // rendered into <code> elements yet (markdown rendering not complete).
    const msgBoxes = document.querySelectorAll('.vac-message-box:not(.vac-offset-current)');
    if (msgBoxes.length === 0) return false;
    const lastMsg = msgBoxes[msgBoxes.length - 1];
    if (!lastMsg) return false;
    // Rendered code blocks are the source of truth: any ``` inside them is code
    // content, not a fence. Strip the rendered blocks out, then only the leftover
    // text can carry raw (unrendered) fence markers. This avoids miscounting a
    // ``` that sits in the middle of rendered code content as a real fence.
    const sel = 'code.code-block-body, code[class*="language-"], pre code';
    const codeElements = lastMsg.querySelectorAll(sel);
    const clone = lastMsg.cloneNode(true);
    clone.querySelectorAll(sel).forEach((el) => {
      const host = el.closest('pre') || el;
      if (host && host.parentNode) host.parentNode.removeChild(host);
    });
    const outsideText = clone.textContent || '';
    // Match a run of 3+ backticks (covers variable-length fences too).
    //
    // Intentionally BACKTICK-ONLY: we do NOT extend this to tilde (`~~~`)
    // fences, even though CommonMark allows them and other MetaDSL cleanup
    // paths (message-handler.js A-path, freebie FENCE_*_RE) do accept tilde.
    // Rationale:
    //   * This function is only a heuristic that DELAYS `onPageStable` until
    //     markdown rendering settles. It is NOT part of the correctness
    //     path -- the actual MetaDSL scan looks at rendered <code> elements
    //     via MutationObserver and is fence-character agnostic.
    //   * If we matched `~~~` here and the host chat UI's markdown renderer
    //     does not treat tilde as a fence (renderer support varies), any
    //     stray `~~~` in prose (decorative separators, ASCII art, quoted
    //     code, etc.) would linger in `outsideText` forever -> this method
    //     would return true forever -> `onPageStable` would never fire ->
    //     hard deadlock with no self-heal.
    //   * The worst case of leaving tilde out is a few extra empty scan
    //     passes when the LLM streams a `~~~` fence mid-flight; the next
    //     mutation self-heals. No correctness impact.
    // If we ever confirm the renderer supports tilde fences AND add a
    // timeout fallback for `onPageStable`, revisit this decision.
    const fenceMatches = outsideText.match(/`{3,}/g);
    if (fenceMatches && fenceMatches.length > 0) {
      this.info(`Detected ${fenceMatches.length} unrendered fence marker(s) outside rendered code blocks (rendered=${codeElements.length})`);
      return true;
    }
    return false;
  }

  // ---- Memory guard ------------------------------------------------------
  // Every round leaks a Vue component tree the page never releases, so a long
  // auto-plan session pushes the renderer process into the GBs. Only a real
  // process restart reclaims it: location.reload() keeps the same renderer
  // process (and its allocator arenas) alive.
  // The verdict comes from the dsl (callMetaDSL -> check_memory_guard in
  // script_renderer.dsl), the only side that can read a process level figure -
  // performance.memory covers the js heap alone, a small fraction of the real
  // footprint. The schedule, the cooldown, the "is the agent idle" check and
  // the restart itself stay here, in the side that knows them.
  startMemoryGuard() {
    if (this.memoryGuardTimer) return;
    const intervalSec = CONFIG.get('panel.memoryGuardIntervalSec') || 60;
    this.memoryGuardTimer = setInterval(() => {
      try {
        this.checkProcessMemory();
      } catch (e) {
        this.warn('Memory guard check failed: ' + e);
      }
    }, intervalSec * 1000);
  }

  stopMemoryGuard() {
    if (this.memoryGuardTimer) {
      clearInterval(this.memoryGuardTimer);
      this.memoryGuardTimer = null;
    }
  }

  // Returns true when a restart has been requested.
  checkProcessMemory() {
    if (typeof callMetaDSL !== 'function') return false;

    let over = false;
    try {
      over = callMetaDSL('check_memory_guard') === true;
    } catch (e) {
      this.warn('callMetaDSL(check_memory_guard) failed: ' + e);
      return false;
    }
    if (!over) return false;

    // Cooldown: the restart rebuilds the process, so never ask twice in a row.
    let lastRestart = 0;
    try {
      lastRestart = Number(sessionStorage.getItem('inject_last_restart') || 0);
    } catch (e) { /* storage unavailable */ }
    if (lastRestart && Date.now() - lastRestart < 10 * 60 * 1000) return false;

    // A restart drops whatever round is in flight, so wait for a quiet moment.
    if (!(this.currentStateName === 'USER_INPUT' && this.operationQueue.length === 0)) {
      this.warn(`Renderer memory is over the limit, restart postponed (state=${this.currentStateName}, queue=${this.operationQueue.length})`);
      return false;
    }

    let usedMB = 0;
    try {
      usedMB = Number(callMetaDSL('get_renderer_memory')) || 0;
    } catch (e) { /* the figure is only used for the log */ }
    try {
      sessionStorage.setItem('inject_last_restart', String(Date.now()));
    } catch (e) { /* storage unavailable */ }

    this.warn(`Renderer process at ${usedMB.toFixed(0)}MB is over the limit: restarting the browser window`);
    // restartBrowserWindow is the single implementation behind the hot_reload
    // command (component agentcore / restart), so this is the same action the
    // dsl api restart_page() and the C# side end up performing. It is called
    // directly instead of pushing a command because window.onAgentCommand is
    // re-assigned by the page adapters, which drop commands they do not know.
    if (typeof restartBrowserWindow === 'function') {
      restartBrowserWindow('Restart');
      return true;
    }
    this.warn('restartBrowserWindow is unavailable, cannot restart');
    return false;
  }

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
    // One alignment per round: the validation below and the code block scan
    // have to agree on which messages are new.
    this._newWrappers = null;
    this._roundAlign = null;

    // Check if LLM is still generating using state machine and checkLLMResponding
    const isInLLMRespondingState = this.currentStateName === 'LLM_RESPONDING';
    const isLLMGenerating = this.states && this.states['LLM_RESPONDING']
      ? this.states['LLM_RESPONDING'].checkLLMResponding()
      : false;

    if (isInLLMRespondingState || isLLMGenerating) {
      this.info(`LLM is still generating (state=${this.currentStateName}, isGenerating=${isLLMGenerating}), skipping code block scan to avoid incomplete code`);
      // Deadline: a stuck loading indicator (or a permanent element matching
      // the selector) would reschedule this check forever and the scan would
      // never run. The deadline mirrors LLMRespondingState's response timeout -
      // it must never be shorter, or a legitimately long reply would be
      // scanned while it is still streaming.
      if (!this.generatingSince) {
        this.generatingSince = Date.now();
        this.resetPageStableTimer();
        return;
      }
      if (Date.now() - this.generatingSince < CONFIG.llmResponseTimeoutMin * 60000) {
        this.resetPageStableTimer();
        return;
      }
      this.warn(`Page reported generating for ${CONFIG.llmResponseTimeoutMin}min; forcing scan to avoid deadlock`);
      this.generatingSince = 0;
    } else {
      this.generatingSince = 0;
    }

    // Check if markdown rendering is complete by looking for unrendered code fences
    if (this.hasUnrenderedCodeFences()) {
      this.unrenderedFenceRetryCount++;
      if (this.unrenderedFenceRetryCount < this.unrenderedFenceRetryLimit) {
        this.info(`Markdown code blocks not fully rendered yet, waiting... (retry ${this.unrenderedFenceRetryCount}/${this.unrenderedFenceRetryLimit})`);
        this.resetPageStableTimer();
        return;
      }
      // Timeout fallback: LLM has already stopped generating (checked above), so a
      // fence that still lingers is a malformed/unpairable one that will never render
      // (e.g. a ``` glued to the end of a prose line). Force-continue to self-heal
      // instead of deadlocking forever.
      this.warn(`Unrendered fence marker(s) persisted after ${this.unrenderedFenceRetryLimit} retries; forcing continue to avoid deadlock`);
    }
    // Reached a stable, scannable state: reset the fence retry counter for the next round.
    this.unrenderedFenceRetryCount = 0;

    // Validate MetaDSL formatting in the latest LLM response before scanning code blocks.
    // On failure, feedback is sent to LLM and code block scan is skipped for this round
    // to avoid executing partially rendered or split code.
    // A warning (e.g. several MetaDSL blocks in one reply) does NOT skip the
    // scan: the blocks are merged and executed, and the model is told after
    // the command has been queued. Held on the instance rather than in a local
    // because an unstable scan reschedules this method, and by then
    // `metadslValidated` short-circuits validation so the warning would be lost.
    if (CONFIG.get('metadsl.strictValidation') && !this.isInitializing) {
      const allMsgBoxesForValidate = document.querySelectorAll('.vac-message-box:not(.vac-offset-current)');
      const lastMsgBoxForValidate = allMsgBoxesForValidate.length > 0
        ? allMsgBoxesForValidate[allMsgBoxesForValidate.length - 1]
        : null;
      // Skip validation if this message has already been validated (success or failure)
      const alreadyValidated = lastMsgBoxForValidate
        && (lastMsgBoxForValidate.dataset.metadslInvalid === '1'
          || lastMsgBoxForValidate.dataset.metadslValidated === '1');
      // Only validate a message that actually just arrived. A DOM attribute
      // cannot tell that (the page rebuilds the message list), the sequence
      // alignment can - otherwise a re-loaded history reply would be validated
      // and the model would be told about a format error it never produced.
      const validateWrapper = lastMsgBoxForValidate && lastMsgBoxForValidate.closest
        ? lastMsgBoxForValidate.closest('.vac-message-wrapper')
        : null;
      if (lastMsgBoxForValidate && !alreadyValidated && this.newMessageWrappers().has(validateWrapper)) {
        const rawMarkdown = this.pageAdapter.extractLatestResponse();
        const validation = this.validateLatestResponseMetaDSL(lastMsgBoxForValidate);

        if (!validation.ok) {
          this.info('MetaDSL validation failed: ' + validation.reason);
          this.sendResultToLLM('MetaDSL validation failed: ' + validation.reason
            + '\nPlease resend with correct format.');
          // Mark all code blocks inside this invalid message as processed/skipped,
          // so the subsequent scanForNewCodeBlocks() will not enqueue them for execution.
          // Without this, blocks would still be picked up on the next page-stable round
          // because alreadyValidated short-circuits the validation but scan still runs.
          const blocksInMsg = lastMsgBoxForValidate.querySelectorAll(
            'code.code-block-body, code[class*="language-"], pre code');
          blocksInMsg.forEach((blk) => {
            if (blk.dataset.metadslStatus) return;
            blk.dataset.metadslStatus = 'invalid_skipped';
            blk.style.borderLeft = '3px solid #f44336';
            blk.style.backgroundColor = 'rgba(244, 67, 54, 0.05)';
            try {
              const bid = this.getBlockId(blk);
              if (bid) this.processedBlocks.add(bid);
            } catch (e) { /* ignore id errors */ }
          });
          // Mark the message as invalid and scan-complete to avoid repeated feedback on the same message
          lastMsgBoxForValidate.dataset.metadslInvalid = '1';
          lastMsgBoxForValidate.dataset.scanComplete = '1';
          this.pageAdapter.setLastScannedResponse(rawMarkdown, lastMsgBoxForValidate);
          this.scanComplete = true;
          this.pruneOldMessages();
          return;
        }
        // Mark as validated to skip future re-validation on the same message
        lastMsgBoxForValidate.dataset.metadslValidated = '1';
        if (validation.warning) {
          this.warn('MetaDSL validation warning: ' + validation.warning);
          this.pendingValidationWarning = validation.warning;
        }
      }
    }

    this.info('LLM has stopped generating, processing code blocks...');

    // Collapse any lazy-loaded history agent reply messages
    this.collapseHistoryAgentMessages();

    // Scan for new code blocks and add to queue
    const hasUnstableBlocks = this.scanForNewCodeBlocks();
    if (hasUnstableBlocks) {
      this.info('Some code blocks still changing, scheduling rescan');
      this.resetPageStableTimer();
    } else {
      // Update lastScannedResponse before marking scan complete
      const lastResponse = this.pageAdapter.extractLatestResponse();
      const allMsgBoxes = document.querySelectorAll('.vac-message-box:not(.vac-offset-current)');
      const lastMsgBox = allMsgBoxes.length > 0 ? allMsgBoxes[allMsgBoxes.length - 1] : null;
      this.pageAdapter.setLastScannedResponse(lastResponse, lastMsgBox);
      // Mark the last LLM message container as scan-complete
      if (lastMsgBox) {
        lastMsgBox.dataset.scanComplete = '1';
      }
      this.scanComplete = true;
    }

    // Report a format warning only once the command is queued: sendResultToLLM
    // then appends the "operations are queued / just reply 继续" note, which
    // stops the model from sending new code while the merged command runs.
    // Held back while blocks are unstable, because nothing is queued yet and
    // this method will run again.
    if (this.pendingValidationWarning && !hasUnstableBlocks) {
      const warning = this.pendingValidationWarning;
      // Cleared either way, so a warning never leaks into a later reply.
      this.pendingValidationWarning = null;
      if (this.canExecuteNewCommands) {
        this.sendResultToLLM('MetaDSL format warning: ' + warning);
      } else {
        // Blocks were marked as history instead of executed, so telling the
        // model they ran would be wrong.
        this.info('Format warning dropped, blocks were not executed: ' + warning);
      }
    }

    // Trigger state machine to process queue
    // If in AGENT_EXECUTING state, the loop will pick up new operations
    // If in other states, wait for state transition
    if (this.currentStateName === 'AGENT_EXECUTING') {
      this.info('Already in AGENT_EXECUTING state, operations will be processed');
    } else {
      this.info('Not in AGENT_EXECUTING state, operations queued for later');
    }

    // Prune old conversation DOM nodes to prevent page slowdown
    this.pruneOldMessages();
  }

  scanForNewCodeBlocks() {
    // Find all code blocks with comprehensive selectors
    // Returns true if any block's content is still changing (needs rescan)
    const allCodeBlocks = document.querySelectorAll('code.code-block-body, code[class*="language-"], pre code');

    // Filter out code blocks inside the control panel
    const codeBlocks = Array.from(allCodeBlocks).filter(block => {
      let node = block;
      while (node) {
        if (node.id === 'agent-control-panel') {
          return false;
        }
        node = node.parentElement;
      }
      return true;
    });

    this.info(`Scanning for new code blocks, found ${codeBlocks.length} total blocks`);

    // Only blocks inside messages that arrived AFTER the already known message
    // sequence may be executed. Everything else belongs to history that was
    // (re)loaded, or to a message already handled - those are annotated and
    // never queued, which is what keeps a history reload from re-running code.
    const newWrappers = this.newMessageWrappers();
    const align = this._roundAlign || this.alignMessages();
    if (align.reseeded) {
      this.info('Message sequence was reseeded, no block will be executed this round');
    } else if (align.newIdx.length > 0) {
      this.debug(`Message alignment: ${align.matched} known, ${align.newIdx.length} new`);
    }

    let newBlocksCount = 0;
    let metadslBlocksCount = 0;
    let unstableBlocksCount = 0;

    // Phase 1: collect every unprocessed block that has content.
    const items = [];

    codeBlocks.forEach((block) => {
      // A block whose message is not part of the newly arrived ones is history
      // (or already handled): annotate it and leave it alone.
      const msgWrapper = block.closest ? block.closest('.vac-message-wrapper') : null;
      if (!newWrappers.has(msgWrapper)) {
        this.markBlockAsHistory(block);
        return;
      }

      const blockId = this.getBlockId(block);

      // Skip already processed blocks (check both in-memory set and DOM attribute)
      if (this.processedBlocks.has(blockId) || block.dataset.metadslStatus) {
        return;
      }

      newBlocksCount++;

      const rawCode = block.textContent || '';
      if (!rawCode.trim()) {
        return;
      }

      // A MetaDSL marker opens a command unit; a marker-less block continues
      // the preceding unit when only whitespace separates them (stage 1).
      items.push({
        block,
        blockId,
        rawCode,
        root: this.findMessageContainer(block),
        starts: this.stripMetaDSLMarker(rawCode) !== null,
      });
    });

    // Phase 2: build the command per message (stage 1 + stage 2), then gate on
    // the whole command. Gating per block instead would enqueue the leading
    // fragment on its own whenever a later fence was still streaming.
    this.groupCommandUnitsByMessage(items).forEach(entry => {
      // Plain blocks that continue no command are left alone: they are not
      // recorded as processed, matching the pre-existing behaviour that only
      // executed blocks are consumed.
      const cmdBlocks = this.mergeCommandUnits(entry.units);
      if (cmdBlocks.length === 0) {
        return;
      }
      const head = cmdBlocks[0];

      const changing = cmdBlocks.filter(it => {
        const lastContent = this.lastBlockContent.get(it.block);
        return lastContent !== undefined && lastContent !== it.rawCode;
      });
      if (changing.length > 0) {
        // Content is still changing, update cache but skip processing
        cmdBlocks.forEach(it => this.lastBlockContent.set(it.block, it.rawCode));
        unstableBlocksCount++;
        this.debug(`Command of ${cmdBlocks.length} block(s) still changing, skipping (${changing.length} unstable)`);
        return;
      }

      // Content is stable (first time or unchanged), record it
      cmdBlocks.forEach(it => this.lastBlockContent.set(it.block, it.rawCode));

      // The marker line is stripped from every fence that carries one; the
      // remaining fences hold plain code and are appended verbatim.
      const metadslCode = cmdBlocks.map(it => {
        const stripped = this.stripMetaDSLMarker(it.rawCode);
        return stripped !== null ? stripped : it.rawCode;
      }).join('\n').trim();

      if (metadslCode) {
        if (cmdBlocks.length > 1) {
          this.info(`Merged ${cmdBlocks.length} code blocks into one MetaDSL command`);
        }
        metadslBlocksCount++;

        // Mark as processed immediately.
        cmdBlocks.forEach(it => this.processedBlocks.add(it.blockId));

        // While initializing, every block is history by definition. Past that
        // point anything reaching here belongs to a newly arrived message:
        // blocks of already known messages never get this far, the sequence
        // alignment in scanForNewCodeBlocks filters them out.
        if (this.isInitializing) {
          cmdBlocks.forEach(it => this.markBlockAsHistory(it.block));
          this.debug(`✓ Marked block seen during initialization: ${head.blockId} (command of ${cmdBlocks.length})`);
          return;
        }

        // After initialization, determine operation type based on canExecuteNewCommands
        this.debug(`📌 canExecuteNewCommands = ${this.canExecuteNewCommands}`);
        const operationType = this.canExecuteNewCommands ? 'execute' : 'mark_history';

        // Pre-tag: attach data-metadsl-status='pending' synchronously BEFORE
        // enqueue, so a concurrent archive scan (extractNewConversations ->
        // getVisibleTextForHistory) can already recognize this block as
        // MetaDSL and collapse it to the [...metadsl...] placeholder,
        // instead of racing with the state machine and capturing raw code
        // into SQLite. The state machine overwrites this to 'executed' or
        // 'history' once the operation runs; only the existence of the
        // attribute matters to page_adapter.getVisibleTextForHistory.
        cmdBlocks.forEach(it => {
          if (!it.block.dataset.metadslStatus) {
            it.block.dataset.metadslStatus = 'pending';
          }
        });

        // Add to operation queue. `blocks` carries every merged block so the
        // state machine can mark, annotate and hide the whole snippet rather
        // than only its first fence.
        this.enqueueOperation({
          type: operationType,
          block: head.block,
          blocks: cmdBlocks.map(it => it.block),
          code: metadslCode,
          blockId: head.blockId,
          timestamp: Date.now()
        });

        this.debug(`✓ Queued ${operationType} operation for block: ${head.blockId}`);
      }
    });

    if (newBlocksCount > 0 || unstableBlocksCount > 0) {
      this.info(`Scan complete: ${newBlocksCount} new blocks, ${metadslBlocksCount} MetaDSL, ${unstableBlocksCount} unstable`);
    }

    // Remember the messages handled now, so the next scan aligns them instead
    // of running their blocks again. Skipped while a block is still changing:
    // that message is not finished and has to be scanned once more.
    if (unstableBlocksCount === 0) {
      this.markMessagesAsSeen(align);
    }

    // Drop the per-round alignment. This method is also called directly from
    // the state machine, and the page may have moved on since it was computed,
    // so the next scan has to look at the page again.
    this._newWrappers = null;
    this._roundAlign = null;

    // Limit processed blocks size to prevent memory leak
    if (this.processedBlocks.size > this.maxProcessedBlocks) {
      const toDelete = this.processedBlocks.size - this.maxProcessedBlocks;
      const iterator = this.processedBlocks.values();
      for (let i = 0; i < toDelete; i++) {
        this.processedBlocks.delete(iterator.next().value);
      }
      this.debug(`Cleaned up ${toDelete} old processed blocks`);
    }

    return unstableBlocksCount > 0;
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

    // Bound the queue: a page that keeps producing commands while the agent
    // is stuck would otherwise grow it without limit.
    if (this.operationQueue.length >= this.maxOperationQueue) {
      this.warn(`Operation queue full (${this.maxOperationQueue}), dropping oldest operation`);
      this.operationQueue.shift();
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

  // Identity of a code block: which message it belongs to (message
  // fingerprint) plus which code block it is inside that message.
  //
  // Both parts survive a DOM rebuild and a history reload. The previous id
  // also carried the token tag text, which is absent or different depending on
  // how the page renders a message, so the same block got a different id after
  // a reload and was executed again.
  //
  // The block content is deliberately NOT part of the id: it changes on every
  // streamed token, and lastBlockContent needs a stable key to notice that.
  getBlockId(block) {
    if (!block) return null;
    const msgWrapper = block.closest ? block.closest('.vac-message-wrapper') : null;
    let msgFp = null;
    try {
      msgFp = msgWrapper ? this.pageAdapter.getMessageFingerprint(msgWrapper) : null;
    } catch (e) {
      msgFp = null;
    }

    // Cache is only valid while the message fingerprint it was built from is
    // unchanged - a streaming message keeps re-rendering with new content.
    const cached = this.blockIdCache.get(block);
    if (cached && cached.fp === msgFp) {
      return cached.id;
    }

    let blockId;
    if (msgFp) {
      let indexInMessage = 0;
      if (msgWrapper) {
        const all = msgWrapper.querySelectorAll('code.code-block-body, code[class*="language-"], pre code');
        const idx = Array.prototype.indexOf.call(all, block);
        indexInMessage = idx >= 0 ? idx : 0;
      } else {
        indexInMessage = this.nextBlockId++;
      }
      blockId = `blk_${msgFp}_#${indexInMessage}`;
    } else {
      // No fingerprint (page type not recognized, unexpected markup): fall
      // back to the block content. Every block then keeps its own identity
      // across renders, instead of all of them collapsing onto one shared id
      // and swallowing each other.
      const content = (block.textContent || '').trim();
      blockId = `blk_content_${this.hashString(content)}`;
    }
    this.blockIdCache.set(block, { fp: msgFp, id: blockId });
    return blockId;
  }

  scheduleHideContainer(block) {
    if (!CONFIG.get('panel.hideMetaDslBlock')) return;
    const container = block.closest('pre') || block.closest('div.code-block') || block.parentElement || block;
    this.info(`scheduleHideContainer: block=${block.tagName}.${this._getClassStr(block).slice(0, 30)}, container=${container.tagName}.${this._getClassStr(container).slice(0, 30)}`);
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
    if (!CONFIG.get('panel.collapseAgentReply')) return;
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

  isMetaDSLMarkerLine(line) {
    const trimmedLine = (line || '').trim();
    return CONFIG.metadslMarkers.some(marker => trimmedLine.startsWith(marker));
  }

  /**
   * Nearest ancestor message container (.vac-message-box) of a code block.
   * Used both to decide whether a block belongs to a history message and to
   * scope the whitespace-only gap check to a single reply.
   */
  findMessageContainer(block) {
    let node = block ? block.parentElement : null;
    while (node && !(node.classList && node.classList.contains('vac-message-box'))) {
      node = node.parentElement;
    }
    return node || null;
  }

  /**
   * True when nothing but whitespace separates the end of block a from the
   * start of block b inside root, i.e. the two fences are rendered back to
   * back. Climbs out of each block's own wrapper first, so page chrome (copy
   * button, language label) rendered next to a code block is not mistaken for
   * LLM content sitting in the gap.
   */
  isBlankGap(root, a, b) {
    if (a === b || a.contains(b) || b.contains(a)) return false;
    let n = a, m = b;
    while (n.parentElement && n.parentElement !== root && !n.parentElement.contains(b)) {
      n = n.parentElement;
    }
    while (m.parentElement && m.parentElement !== root && !m.parentElement.contains(a)) {
      m = m.parentElement;
    }
    if (!n.parentElement || n.parentElement !== m.parentElement) return false;
    let cur = n.nextSibling;
    while (cur && cur !== m) {
      if (cur.nodeType === Node.TEXT_NODE) {
        if ((cur.nodeValue || '').trim()) return false;
      } else if (cur.nodeType === Node.ELEMENT_NODE) {
        // img/video/hr/table carry no text but are visible content.
        if (cur.matches('img, video, hr, table')) return false;
        if ((cur.textContent || '').trim()) return false;
      }
      cur = cur.nextSibling;
    }
    return true;
  }

  /**
   * Stage 1 of code block recognition: absorb continuations.
   * A block carrying a MetaDSL marker opens a command unit. A block without
   * one continues the preceding unit ONLY when nothing but whitespace
   * separates it from the previous block; otherwise it belongs to no command
   * (a leading plain fence, or one separated from the unit by visible text).
   * Returns { units, dropped }. The units are NOT required to be adjacent to
   * each other - joining them is stage 2.
   */
  groupCommandUnits(root, items) {
    const units = [];
    const dropped = [];
    let cur = null, prev = null;
    items.forEach(it => {
      if (it.starts) {
        cur = [it];
        units.push(cur);
      } else if (cur && prev && this.isBlankGap(root, prev.block, it.block)) {
        cur.push(it);
      } else {
        dropped.push(it);
      }
      prev = it;
    });
    return { units: units, dropped: dropped };
  }

  /**
   * Stage 2 of code block recognition: join the commands.
   * The protocol asks for one MetaDSL block per reply, but when the model
   * sends several they are concatenated and run as a single command; fixing
   * whatever that breaks is the model's job. Stage 1 units are joined here,
   * with no adjacency requirement between them.
   */
  mergeCommandUnits(units) {
    const merged = [];
    units.forEach(u => u.forEach(it => merged.push(it)));
    return merged;
  }

  /**
   * Same as groupCommandUnits, but splits the items by their message container
   * first. This scan is document-wide, so without this a marker block ending
   * one reply would merge with a plain block starting the next reply when no
   * text separates the two messages.
   */
  /**
   * Same as groupCommandUnits, but partitions by message container first.
   * This scan is document-wide, so stage 1 must not let a command at the end
   * of one reply absorb a plain block starting the next reply. Stage 2 then
   * joins only the units of the SAME message: blocks belonging to different
   * replies are separate commands.
   * Returns [{ units, dropped }, ...] - one entry per message container.
   */
  groupCommandUnitsByMessage(items) {
    const byMessage = new Map();
    const roots = new Map();
    let orphanSeq = 0;
    items.forEach(it => {
      // A block outside any message container gets a unique key, so it can
      // never merge with an unrelated block elsewhere in the document.
      const key = it.root || ('orphan:' + (orphanSeq++));
      if (!byMessage.has(key)) {
        byMessage.set(key, []);
        roots.set(key, it.root);
      }
      byMessage.get(key).push(it);
    });
    const entries = [];
    byMessage.forEach((list, key) => {
      entries.push(this.groupCommandUnits(roots.get(key), list));
    });
    return entries;
  }

  /**
   * Strip the marker line from a code block.
   * Returns null when the block is NOT MetaDSL at all, and '' when it carries a
   * marker but no code. extractMetaDSLCode collapses both cases to null, which
   * makes it unusable as an "is this MetaDSL" predicate: a marker-only block
   * would look like plain text and its formatting error would go unreported.
   */
  stripMetaDSLMarker(code) {
    // Strip BOM and any leading blank lines so the marker is matched against the
    // first non-blank line (mirrors the page adapters' hasExecuteMarker), avoiding
    // a false miss when the code block starts with an empty line.
    const sourceCode = String(code || '').replace(/^\uFEFF/, '').replace(/^(?:[ \t]*\r?\n)+/, '');
    const firstNewline = sourceCode.indexOf('\n');
    const firstLine = firstNewline === -1
      ? sourceCode
      : sourceCode.substring(0, firstNewline);
    if (!this.isMetaDSLMarkerLine(firstLine)) {
      return null;
    }
    return firstNewline === -1 ? '' : sourceCode.substring(firstNewline + 1).trim();
  }

  extractMetaDSLCode(code) {
    if (!code) return null;

    // Strip BOM and any leading blank lines so the marker is matched against the
    // first non-blank line, avoiding a false miss when the block starts blank.
    const sourceCode = String(code).replace(/^\uFEFF/, '').replace(/^(?:[ \t]*\r?\n)+/, '');
    const firstNewline = sourceCode.indexOf('\n');
    const firstLine = firstNewline === -1
      ? sourceCode
      : sourceCode.substring(0, firstNewline);

    if (!this.isMetaDSLMarkerLine(firstLine)) {
      return null;
    }

    this.debug('✓ MetaDSL marker detected in code block');

    // Remove the marker line and return only the actual MetaDSL code
    if (firstNewline === -1) {
      // Only marker line, no actual code
      return null;
    }
    return sourceCode.substring(firstNewline + 1).trim();
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
  // MetaDSL Format Validation
  // ========================================================================

  // Validate actual rendered code blocks in the latest LLM message.
  // A MetaDSL block must be a code block whose first line starts with a configured marker.
  // Returns { ok: boolean, reason?: string, warning?: string }.
  // `reason` rejects the reply; `warning` is reported to the model but the
  // command still runs.
  validateLatestResponseMetaDSL(messageElement) {
    if (!messageElement || typeof messageElement.querySelectorAll !== 'function') {
      return { ok: true };
    }

    const codeBlocks = messageElement.querySelectorAll(
      'code.code-block-body, code[class*="language-"], pre code');

    // One MetaDSL command may be split over several fences, so validate the
    // assembled command, not each block on its own.
    const items = [];
    codeBlocks.forEach((block) => {
      const code = (block.textContent || '').replace(/^\uFEFF/, '');
      if (!code.trim()) return;
      items.push({ block, code, starts: this.stripMetaDSLMarker(code) !== null });
    });

    // Stage 1 opens one unit per MetaDSL block, so the unit count is the
    // number of MetaDSL blocks the model sent.
    const units = this.groupCommandUnits(messageElement, items).units;
    const cmdBlocks = this.mergeCommandUnits(units);

    // Only reject when the merged command holds nothing but marker lines.
    if (cmdBlocks.length > 0) {
      const merged = cmdBlocks.map(it => {
        const stripped = this.stripMetaDSLMarker(it.code);
        return stripped !== null ? stripped : it.code;
      }).join('\n');
      if (merged.trim().length === 0) {
        return {
          ok: false,
          reason: 'MetaDSL block contains only the marker line, no actual code.'
        };
      }
    }

    // Several MetaDSL blocks in one reply are NOT rejected: they are merged
    // and run as a single command. The protocol rule stays "send one block",
    // so the model is told about the violation and is expected to fix it
    // itself; execution proceeds regardless.
    if (units.length > 1) {
      return {
        ok: true,
        warning: 'Detected ' + units.length
          + ' MetaDSL code blocks in one reply, but only 1 is allowed.'
          + ' They were merged in order and executed as a single command.'
          + ' Beware that a `return` in an earlier block ends the merged'
          + ' script early. Send exactly one MetaDSL block next time.'
      };
    }

    return { ok: true };
  }

  // ========================================================================
  // Command Execution
  // ========================================================================

  executeCommand(command) {
    try {
      // Check if this is a JavaScript request (not MetaDSL)
      const js_request_prefix = "js_request:";
      const trimedCommand = command.trim();
      if (trimedCommand.startsWith(js_request_prefix)) {
        let jsRequest = trimedCommand.substring(js_request_prefix.length).trim();
        if (jsRequest.length > 0 && jsRequest[jsRequest.length - 1] === ';') {
          jsRequest = jsRequest.substring(0, jsRequest.length - 1);
        }
        this.info('JavaScript request detected:', jsRequest);

        if (jsRequest === "start_auto_plan") {
          this.startAutoPlan();
          return;
        }
        else if (jsRequest === "stop_auto_plan") {
          this.stopAutoPlan();
          return;
        }
        else if (jsRequest === "keep_llm_context") {
          this.keepContext(CONFIG.llmContextCountModuloForKeep);
          return;
        }
        else if (jsRequest === "reflect") {
          this.triggerReflection();
          this.sendResultToLLM("Reflection triggered, episodic_reflection notification enqueued.");
          return;
        }

        this.sendResultToLLM("unknown request: " + jsRequest);
        return; // Don't send to C#
      }

      // Check for local_js: prefix - execute in current window and report result
      const local_js_prefix = "local_js:";
      if (trimedCommand.startsWith(local_js_prefix)) {
        const jsCode = trimedCommand.substring(local_js_prefix.length).trim();
        this.info('Local JS eval detected:', jsCode.length <= 100 ? jsCode : jsCode.substring(0, 100) + '...');
        try {
          const result = eval(jsCode);
          if (result !== undefined && result !== null) {
            if (result instanceof Promise) {
              result.then(r => {
                this.sendResultToLLM(r === undefined || r === null ? 'Promise resolved (no value)' : r);
              }).catch(e => {
                this.sendResultToLLM('Promise rejected: ' + e.message);
              });
            } else {
              this.sendResultToLLM(result);
            }
          } else {
            this.sendResultToLLM('executed (no return value)');
          }
        } catch (e) {
          this.sendResultToLLM('local_js error: ' + e.message);
        }
        return; // Don't send to C#
      }

      // Normal MetaDSL command execution
      this.info('Sending MetaDSL command to C# via WebSocket:', getStringInLength(command, 100));

      if (!this.metadslWorker || !this.metadslWorker.isRunning) {
        this.error('MetaDSL Worker is not running');
        return false;
      }

      // Send MetaDSL code directly via MetaDSL Worker
      const success = this.metadslWorker.queueMessage(command);
      if (success) {
        this.info('✓ MetaDSL command queued for execution');
      } else {
        this.error('Failed to queue MetaDSL command');
      }
      // Returns false only when the command did not reach the transport, so
      // the state machine can retry instead of waiting for a reply that will
      // never arrive. js_request / local_js paths return undefined (= handled
      // locally, no transport involved).
      return success;
    } catch (error) {
      this.error('Error in executeCommand:', error);
      return false;
    }
  }
  sendResultToLLM(message, noAgentMarker = false) {
    if (!this.pageAdapter) {
      this.warn('PageAdapter not available');
      return;
    }
    let messageStr = '';
    if (!noAgentMarker) {
      messageStr = CONFIG.get('metadsl.agentReplyMarker') + "\n";
    }
    if (message === null || message === undefined) {
      messageStr += 'null';
    } else if (typeof message === 'object') {
      messageStr += JSON.stringify(message, null, 2);
    } else {
      messageStr += String(message).trim();
    }

    let operationCount = this.operationQueue.length;
    let sendCount = this.metadslWorker.getSendQueueCount();
    let receiveCount = this.metadslWorker.getReceiveQueueCount();
    if (operationCount > 0 || sendCount > 0 || receiveCount > 0) {
      messageStr += '\n\n**当前有' + operationCount + '个操作在排队执行，' + sendCount + '个请求在排队发送，' + receiveCount + '个结果在排队接收**';
    }
    if (operationCount > 0) {
      messageStr += '\n\n**特别注意：你看到消息后只回复继续即可，不要再发新的metadsl代码块**';
    }

    this.info('Sending result to LLM (noAgentMarker=' + noAgentMarker + ')', getStringInLength(messageStr, 100));
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
