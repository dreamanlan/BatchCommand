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
    metadslMonitor = new MetaDSLMonitor(bridge, pageAdapter, metadslWorker);

    panel = null;
    if (CONFIG.agentPanelEnabled) {
      panel = new AgentPanel(bridge, metadslMonitor, pageAdapter);
      // Create and link chat input panel
      const chatInputPanel = new ChatInputPanel(bridge);
      // Async restore config (secrets loaded from SecretStore)
      await chatInputPanel._restoreConfig();
      panel.chatInputPanel = chatInputPanel;
      // Default hidden, user can toggle via panel button
      chatInputPanel.hide();
      // Create and link OpenClaw panel (default hidden)
      const openClawPanel = new OpenClawPanel();
      // Async restore config (secrets loaded from SecretStore)
      await openClawPanel._restoreConfig();
      panel.openClawPanel = openClawPanel;
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
    getResponse: () => pageAdapter && pageAdapter.getLastScannedResponse(),
    getHistory: (count) => pageAdapter && pageAdapter.getHistory(count || 5),
    isLastMessageFromLLM: () => pageAdapter && pageAdapter.isLastMessageFromLLM(),
    updateSystemPrompt: () => metadslMonitor && metadslMonitor.updateSystemPrompt(),
    keepContext: () => metadslMonitor && metadslMonitor.keepContext(CONFIG.llmContextCountModuloForKeep),
    alignTarget: () => metadslMonitor && metadslMonitor.alignTarget(CONFIG.llmContextCountModuloForAlign),
    needToPlan: () => bridge && pageAdapter && metadslMonitor && bridge.sendAgentNeedToPlan('AGENT_EXECUTING', pageAdapter, metadslMonitor.operationQueue.length),
    getQueuedCount: () => metadslMonitor ? metadslMonitor.operationQueue.length : 0,
    showPanel: () => panel && panel.show(),
    hidePanel: () => panel && panel.hide(),
    togglePanel: () => panel && panel.toggle(),
    startMetaDSL: () => metadslMonitor && metadslMonitor.start(),
    stopMetaDSL: () => metadslMonitor && metadslMonitor.stop(),
    startAgent: () => metadslMonitor && metadslMonitor.startAgent(),
    stopAgent: () => metadslMonitor && metadslMonitor.stopAgent(),

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

      if (cmd.command === 'start_agent') {
        this.AgentAPI.startAgent();
        return;
      }

      if (cmd.command === 'stop_agent') {
        this.AgentAPI.stopAgent();
        return;
      }

      if (cmd.command === 'send_message') {
        if (cmd.params && cmd.params.text) {
          metadslWorker.queueReply(cmd.params.text);
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
