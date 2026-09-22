async function initializeAgent() {
  logger.info('Initializing agent...');

  // Wait for SecretStore to be ready before accessing any secrets
  try {
    await secretStore.ready();
    await CONFIG.loadSecrets();
    logger.info('SecretStore initialized');
  } catch (e) {
    logger.error('SecretStore initialization failed, secrets unavailable', { error: e.toString() });
  }

  // CRITICAL: Disable spellcheck on all contenteditable elements to prevent Blink rendering crashes
  // This must be done BEFORE any other initialization
  disableSpellcheckGlobally();

  // Create instances (assign to shared variables for window API access)
  bridge = new AgentBridge();
  pageAdapter = new LLMPageAdapter(bridge);
  // MetaDSL code execution goes through the relay transport (cefQuery ->
  // browser process -> standalone AgentCore worker pool). relayTransport has
  // the same queue interface as the old metadslWorker (queueMessage /
  // dequeueMessage / queue counts), so the monitor and the state machine work
  // unchanged - and the path no longer depends on the page being allowed to
  // open websockets to localhost (https pages on external sites cannot).
  metadslMonitor = new MetaDSLMonitor(bridge, pageAdapter, relayTransport);

  panel = null;
  if (CONFIG.agentPanelEnabled) {
    panel = new AgentPanel(bridge, metadslMonitor, pageAdapter, relayTransport);
    // Create and link chat input panel
    const chatInputPanel = new ChatInputPanel(bridge);
    // Async restore config (secrets loaded from SecretStore)
    await chatInputPanel._restoreConfig();
    panel.chatInputPanel = chatInputPanel;
    // Sync button state when chat panel visibility changes
    chatInputPanel.onVisibilityChange = () => {
      const on = chatInputPanel.visible;
      panel.toggleChatButton.textContent = on ? '\u2713 Chat Panel' : '\u2717 Chat Panel';
      panel.toggleChatButton.style.background = on ? '#4caf50' : '#666';
    };
    // Default hidden, user can toggle via panel button
    chatInputPanel.hide();
    // Create and link Relay panel (default hidden)
    const relayPanel = new RelayPanel();
    // Async restore config (secrets loaded from SecretStore)
    await relayPanel._restoreConfig();
    // Set the transport reference for remote mode (queueReply surface)
    relayPanel.metadslWorker = relayTransport;
    panel.relayPanel = relayPanel;
    // Register LLM response callback for remote forwarding
    metadslMonitor.onLLMResponse = (resp) => {
      relayPanel.forwardToRelay(resp);
    };
    // Sync button state when relay panel visibility changes
    relayPanel.onVisibilityChange = () => {
      const on = relayPanel.visible;
      panel.toggleClawButton.textContent = on ? '\u2713 Relay' : '\u2717 Relay';
      panel.toggleClawButton.style.background = on ? '#4caf50' : '#666';
    };
    // Create and link Project panel (default visible)
    const projectPanel = new ProjectPanel(bridge);
    // Async restore config (query C# for current project, then load localStorage)
    await projectPanel._restoreConfig();
    panel.projectPanel = projectPanel;
    // Sync button state when project panel visibility changes
    projectPanel.onVisibilityChange = () => {
      panel.updateProjectButtonState();
    };
    // Only show project panel when LLM type is detected (not unknown)
    if (pageAdapter.pageType !== 'unknown') {
      projectPanel.show();
    }
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
      // Show/hide project panel based on LLM type
      if (panel.projectPanel) {
        if (newType !== 'unknown') {
          panel.projectPanel.show();
          panel.updateProjectButtonState();
        } else {
          panel.projectPanel.hide();
          panel.updateProjectButtonState();
        }
      }
      // Auto-start monitor when page type is detected
      if (newType !== 'unknown' && !metadslMonitor.enabled) {
        metadslMonitor.start();
        logger.info('MetaDSL monitor auto-started after page type detection', { pageType: newType });
        panel.log(`✅ Monitor started for ${newType}`);
        // Update button state to reflect the monitor is now running
        panel.updateMetaDSLButtonState();
      }
    };
    // Resync the label: the panel is constructed before the page type retry
    // succeeds, so its initial snapshot may still say unknown when the retry
    // completed while onPageTypeChanged was not wired yet.
    panel.updateLLMType();
  } else {
    // No panel, but still need to start monitor when page type is detected
    pageAdapter.onPageTypeChanged = (newType) => {
      if (newType !== 'unknown' && !metadslMonitor.enabled) {
        metadslMonitor.start();
        logger.info('MetaDSL monitor auto-started after page type detection', { pageType: newType });
      }
    };
  }

  // Keyboard shortcut: Ctrl+Shift+K to toggle panel
  document.addEventListener('keydown', (e) => {
    if (e.ctrlKey && e.shiftKey && (e.key === 'K' || e.key === 'k')) {
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

  // Auto-connect Relay if config is complete
  if (panel && panel.relayPanel) {
    panel.relayPanel.tryAutoConnect();
  }

  logger.info('Agent initialization complete', {
    pageType: pageAdapter.pageType,
    url: window.location.href
  });

  // Restore what the previous page load was doing (auto plan, agent lock)
  // before the monitor starts: both shape what the state machine does, and
  // both would otherwise fall back to their constructor defaults.
  if (panel) {
    panel.applyRuntimeState();
  }

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
  logger.info('Use Ctrl+Shift+K to toggle control panel');
  logger.info('Access API via window.AgentAPI');
}

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
  getLLMCategory: () => pageAdapter && pageAdapter.getLLMCategory(),
  getResponse: () => pageAdapter && pageAdapter.getLastScannedResponse(),
  getHistory: (count) => pageAdapter && pageAdapter.getHistory(count || 5),
  isLastMessageFromLLM: () => pageAdapter && pageAdapter.isLastMessageFromLLM(),
  updateSystemPrompt: () => metadslMonitor && metadslMonitor.updateSystemPrompt(),
  keepContext: () => metadslMonitor && metadslMonitor.keepContext(CONFIG.llmContextCountModuloForKeep),
  alignTarget: () => metadslMonitor && metadslMonitor.alignTarget(CONFIG.llmContextCountModuloForAlign),
  needToPlan: () => bridge && pageAdapter && metadslMonitor && bridge.dispatchAgentDecision('AGENT_EXECUTING', panel, true),
  triggerReflection: () => metadslMonitor && metadslMonitor.triggerReflection(),
  getOperationQueueCount: () => metadslMonitor ? metadslMonitor.operationQueue.length : 0,
  getSendQueueCount: () => relayTransport ? relayTransport.getSendQueueCount() : 0,
  getReceiveQueueCount: () => relayTransport ? relayTransport.getReceiveQueueCount() : 0,
  showPanel: () => panel && panel.show(),
  hidePanel: () => panel && panel.hide(),
  togglePanel: () => panel && panel.toggle(),
  startMetaDSL: () => metadslMonitor && metadslMonitor.start(),
  stopMetaDSL: () => metadslMonitor && metadslMonitor.stop(),
  startAutoPlan: () => metadslMonitor && metadslMonitor.startAutoPlan(),
  stopAutoPlan: () => metadslMonitor && metadslMonitor.stopAutoPlan(),

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
window.onAgentResponse = function (responseJson) {
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

// Server-pushed async callbacks (e.g. llm_callback) that were not handled by
// a dsl hook in script_agent.dsl. Called from ws_manager envelope dispatch.
window.onAgentCallback = function (msg, args) {
  try {
    if (msg === 'llm_callback' && args && args.length >= 4) {
      const reply = args[3];
      if (panel && panel.chatInputPanel && typeof reply === 'string') {
        panel.chatInputPanel.addMessage('llm', reply);
      }
    } else {
      logger.debug('Unhandled agent callback', { msg: msg, argCount: args ? args.length : 0 });
    }
  } catch (e) {
    logger.error('Error processing agent callback', { error: e.toString() });
  }
};

// Restart the browser window: ask C++ to close the window, terminate the
// renderers and reopen the page with the current url (the hot_reload flow).
// files is intentionally empty: C# dlls (AgentCore.dll) stay locked while the
// pages and the agent process are up, so the dll update itself is done outside
// this flow, in the window of time when the page is closed. Skipping the copy
// also avoids the 10s file-lock wait in C++.
// The single implementation behind the hot_reload command (component agentcore
// or restart): the C# side and the dsl api restart_page() reach it through
// onAgentCommand, the memory guard in metadsl_monitor.js calls it directly - it
// cannot push a command of its own, because window.onAgentCommand is
// re-assigned by the page adapters, which drop the commands they do not know.
function restartBrowserWindow(label) {
  const hotReloadRequest = {
    action: 'hot_reload',
    files: []
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
      onSuccess: function (response) {
        logger.info(label + ' cefQuery success', { response });
        logger.debug('cefQuery success', { response: response });

        if (panel) {
          panel.log('✓ ' + label + ' completed: ' + response);
        }
      },
      onFailure: function (error_code, error_message) {
        logger.error(label + ' cefQuery failed', { error_code, error_message });
        logger.error('cefQuery failed', {
          error_code: error_code,
          error_message: error_message
        });

        if (panel) {
          panel.log('✗ ' + label + ' failed: ' + error_message);
        }
      }
    });
  } else {
    logger.error('cefQuery not available');
  }
};

// Receive commands from C# (called by C#)
window.onAgentCommand = function (commandJson) {
  try {
    logger.debug('Received command from C#', { commandJson });

    const cmd = JSON.parse(commandJson);

    // Extract component from params or directly from cmd
    const component = cmd.component || (cmd.params && cmd.params.component);

    if (cmd.command === 'update_system_prompt') {
      if (cmd.params && cmd.params.prompt) {
        this.AgentAPI.setSystemPrompt(cmd.params.prompt);
      }
      return;
    }

    if (cmd.command === 'start_auto_plan') {
      this.AgentAPI.startAutoPlan();
      return;
    }

    if (cmd.command === 'stop_auto_plan') {
      this.AgentAPI.stopAutoPlan();
      return;
    }

    if (cmd.command === 'send_message') {
      if (cmd.params && cmd.params.text) {
        // DSL pushes land in the relay reply queue (the state machine drains
        // the relay transport now, not the old worker).
        relayTransport.queueReply(cmd.params.text);
      }
      return;
    }

    if (cmd.command === 'send_llm_callback') {
      if (cmd.params && cmd.params.text) {
        if (panel && panel.chatInputPanel) {
          panel.chatInputPanel.addMessage('llm', cmd.params.text);
        }
      }
      return;
    }

    // Handle hot reload command
    if (cmd.command === 'hot_reload' && component) {
      logger.info('Processing hot_reload command', { component });

      if (component === 'agentcore') {
        // AgentCore.dll update flow: close the page first so the dll is unlocked,
        // then (outside this flow) stop the AgentCore process, rebuild + copy the
        // dll, restart the process; the reopened page reconnects to the new one.
        restartBrowserWindow('Hot reload');
      } else if (component === 'restart') {
        // Plain restart: drop the renderer process and reopen the page, e.g. to
        // reclaim the memory a long running session accumulates. No dll update.
        restartBrowserWindow('Restart');
      } else if (component === 'inject') {
        // Check JS hot reload toggle
        if (!CONFIG.config.panel.jsHotReload) {
          logger.info('JS file changed, reload skipped (JS Reload is off)');
          if (panel) {
            panel.log('\u26A1 JS file changed, reload skipped (JS Reload is off)');
          }
          return;
        }
        logger.info('Reloading page for JS changes');
        if (panel) {
          panel.log('\u26A1 JS file changed, reloading page...');
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
