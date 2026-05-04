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
    this.hasEverInitialized = false; // Track if first initialization has completed, survives stop/start

    // Operation queue mechanism
    this.operationQueue = [];
    this.pageStableTimer = null;
    this.isProcessingQueue = false;
    this.pageStableDelay = CONFIG.config.panel.streamingPage ? CONFIG.pageStableDelay : 1500;
    this.operationDelay = CONFIG.operationDelay;

    // Block ID cache for stable ID generation
    this.blockIdCache = new WeakMap();
    this.nextBlockId = 0;

    // Track last content of code blocks to detect if still changing
    this.lastBlockContent = new Map();

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
    // Keep processedBlocks to prevent re-processing on restart
    // processedBlocks is also backed by DOM data-metadsl-status attribute
    this.lastBlockContent.clear();

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
        queuedCount: this.operationQueue.length,
        count: count
      }
    };
    this.enqueueOperation(operation);
  }

  startAgent() {
    if (this.panel.bridge.autoPlanEnabled) {
      this.sendResultToLLM('Agent already started');
      return;
    }
    this.panel.toggleAutoPlan();
    this.sendResultToLLM('Agent started');
  }
  stopAgent() {
    if (this.panel.bridge.lockAgentEnabled) {
      this.sendResultToLLM('Lock Agent is ON: user has set long-term development mode, agent planning must not be stopped.');
      return;
    }
    if (!this.panel.bridge.autoPlanEnabled) {
      this.sendResultToLLM('Agent already stopped');
      return;
    }
    this.panel.toggleAutoPlan();
    this.sendResultToLLM('Agent stopped');
  }
  keepContext(count) {
    const operation = {
      type: 'send_notification',
      notificationType: 'llm_context_count_down',
      data: {
        pageType: this.pageAdapter.pageType,
        queuedCount: this.operationQueue.length,
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
        pageType: this.pageAdapter.pageType,
        queuedCount: this.operationQueue.length
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
  openProjectWindow() {
    if (projectWindow && !projectWindow.closed) {
      projectWindow.location.reload();
      this.sendResultToLLM("project window refreshed");
    }
    else {
      const projData = JSON.parse(localStorage.getItem('project_panel_configs') || '{}');
      const proj = (projData.projects || [])[projData.currentIndex || 0];
      const url = (proj && proj.projectUrl) || 'http://localhost:8081';
      projectWindow = window.open(url, '_blank');
      this.sendResultToLLM("project window opened");
    }
  }
  closeProjectWindow() {
    if (projectWindow && !projectWindow.closed) {
      projectWindow.close();
      projectWindow = null;
    }
    this.sendResultToLLM("project window closed");
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
      // First dialog, update system prompt
      this.updateSystemPrompt();
      // Only send AGENT_INITIALIZED on first start, skip on restart
      if (!this.hasEverInitialized) {
        this.hasEverInitialized = true;
        this.bridge.sendAgentNeedToPlan('AGENT_INITIALIZED', this.pageAdapter, this.operationQueue.length);
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
        this.info(`relevantMutations[0]: type=${m0.type}, target=${(t0.tagName || t0.nodeName || '')}.${this._getClassStr(t0).slice(0, 40)}, id=${t0.id || ''}, addedNodes=${m0.addedNodes.length}, removedNodes=${m0.removedNodes.length}`);

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

    // Mark all existing wrappers as already saved (no need to save history conversations)
    document.querySelectorAll('.vac-message-wrapper').forEach(wrapper => {
      wrapper.dataset.agentSaved = '1';
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
    // Check if the last LLM message contains raw ``` fences that haven't been rendered to <code> elements
    // This indicates markdown rendering is not yet complete
    const msgBoxes = document.querySelectorAll('.vac-message-box:not(.vac-offset-current)');
    if (msgBoxes.length === 0) return false;
    const lastMsg = msgBoxes[msgBoxes.length - 1];
    if (!lastMsg) return false;
    // Get raw text content and count code fences (``` markers)
    const textContent = lastMsg.textContent || '';
    const fenceMatches = textContent.match(/```/g);
    if (!fenceMatches || fenceMatches.length < 2) return false;
    // If there are fence markers but no rendered code elements, rendering is incomplete
    const codeElements = lastMsg.querySelectorAll('code.code-block-body, code[class*="language-"], pre code');
    if (codeElements.length === 0) {
      this.info(`Detected ${fenceMatches.length} fence markers but 0 rendered code elements`);
      return true;
    }
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

    // Check if markdown rendering is complete by looking for unrendered code fences
    if (this.hasUnrenderedCodeFences()) {
      this.info('Markdown code blocks not fully rendered yet, waiting...');
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
    let unstableBlocksCount = 0;

    codeBlocks.forEach((block) => {
      const blockId = this.getBlockId(block);

      // Skip already processed blocks (check both in-memory set and DOM attribute)
      if (this.processedBlocks.has(blockId) || block.dataset.metadslStatus) {
        return;
      }

      // Check if this is a stable block upgrading from a streaming prefix.
      // Streaming blocks have ID "blk_<hash>_", stable blocks "blk_<hash>_<tokenHash>".
      // If a prefix match is found, upgrade: replace the old prefix ID with the full ID.
      const idParts = blockId.split('_');
      if (idParts.length === 3 && idParts[2] !== '') {
        // This is a stable block (has tokenInfoHash), check for streaming prefix
        const streamingPrefix = `blk_${idParts[1]}_`;
        if (this.processedBlocks.has(streamingPrefix)) {
          this.processedBlocks.delete(streamingPrefix);
          this.processedBlocks.add(blockId);
          this.lastBlockContent.delete(streamingPrefix);
          this.debug(`Upgraded streaming block ${streamingPrefix} -> ${blockId}`);
          return;
        }
      }

      newBlocksCount++;

      const rawCode = block.textContent.trim();

      // Check if this block's content is still changing
      const lastContent = this.lastBlockContent.get(blockId);
      if (lastContent !== undefined && lastContent !== rawCode) {
        // Content is still changing, update cache but skip processing
        this.lastBlockContent.set(blockId, rawCode);
        unstableBlocksCount++;
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

    if (newBlocksCount > 0 || unstableBlocksCount > 0) {
      this.info(`Scan complete: ${newBlocksCount} new blocks, ${metadslBlocksCount} MetaDSL, ${unstableBlocksCount} unstable`);
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
    // Use cached ID if available, but only if it has tokenInfoHash (stable)
    if (this.blockIdCache.has(block)) {
      const cached = this.blockIdCache.get(block);
      // Only return cached value if it has tokenInfoHash (not ending with _)
      const parts = cached.split('_');
      if (parts.length === 3 && parts[2] !== '') {
        return cached;
      }
      // Cached ID has no tokenInfoHash, re-check if token info is now available
    }

    // Generate stable ID: blk_${contentHash}_${tokenInfoHash}
    // When token info is absent (streaming), tokenInfoHash is empty,
    // producing a prefix ID like "blk_abc123_". Once the message completes
    // and token info appears, the full ID becomes "blk_abc123_tok456".
    // This prefix relationship is used to upgrade streaming blocks to stable ones.
    const content = block.textContent.trim();
    const contentHash = this.hashString(content);

    // Find token info from the parent message container's el-tag
    let tokenInfoHash = '';
    let msgContainer = block.parentElement;
    while (msgContainer && !msgContainer.classList.contains('vac-message-box')) {
      msgContainer = msgContainer.parentElement;
    }
    if (msgContainer) {
      const tokenTag = msgContainer.querySelector('.el-tag .el-tag__content');
      if (tokenTag) {
        const tokenText = tokenTag.textContent.trim();
        if (tokenText) {
          tokenInfoHash = this.hashString(tokenText);
        }
      }
    }

    const blockId = `blk_${contentHash}_${tokenInfoHash}`;

    // Only cache when tokenInfoHash is present (stable ID)
    if (tokenInfoHash) {
      this.blockIdCache.set(block, blockId);
    }

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
        let jsRequest = trimedCommand.substring(js_request_prefix.length).trim();
        if (jsRequest.length > 0 && jsRequest[jsRequest.length - 1] === ';') {
          jsRequest = jsRequest.substring(0, jsRequest.length - 1);
        }
        this.info('JavaScript request detected:', jsRequest);

        if (jsRequest === "start_agent") {
          this.startAgent();
          return;
        }
        else if (jsRequest === "stop_agent") {
          this.stopAgent();
          return;
        }
        else if (jsRequest === "keep_llm_context") {
          this.keepContext(CONFIG.llmContextCountModuloForKeep);
          return;
        }
        else if (jsRequest === "open_project_window") {
          this.openProjectWindow();
          return;
        }
        else if (jsRequest === "close_project_window") {
          this.closeProjectWindow();
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
      else if (trimedCommand.startsWith(js_eval_prefix)) {
        const jsEval = trimedCommand.substring(js_eval_prefix.length).trim();
        this.info('JavaScript eval detected:', jsEval);
        if (projectWindow) {
          projectWindow.postMessage({ type: 'js_eval', code: jsEval }, '*');
        } else {
          this.sendResultToLLM("projectWindow not available, use js_request:open_project_window first");
        }
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

    if (this.operationQueue.length > 0) {
      messageStr += '\n\n特别注意：当前有' + this.operationQueue.length + '个操作在排队执行，你看到消息后只回复继续即可，不要再发新的metadsl代码块';
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
