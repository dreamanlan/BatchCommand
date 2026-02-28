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
        padding: 6px 8px;
        background: #1e1e1e;
        border-bottom: 1px solid #444;
        display: flex;
        justify-content: space-between;
        align-items: center;
        font-size: 11px;
        gap: 5px;
      `;

      this.stateLabel = document.createElement('span');
      this.stateLabel.style.cssText = `
        color: #fff;
        font-weight: 500;
        padding: 3px 7px;
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
        gap: 5px;
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
          padding: 3px 7px;
          border-radius: 4px;
          font-size: 11px;
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
        padding: 6px 8px;
        background: #252525;
        border-bottom: 1px solid #444;
        display: flex;
        gap: 5px;
        flex-wrap: wrap;
      `;

      // Test connection button
      const testBtn = document.createElement('button');
      testBtn.textContent = 'Test';
      testBtn.style.cssText = `
        padding: 3px 7px;
        background: #2196f3;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      testBtn.onclick = () => this.testConnection();

      // MetaDSL toggle button
      this.metadslButton = document.createElement('button');
      this.metadslButton.textContent = 'Start MetaDSL';
      this.metadslButton.style.cssText = `
        padding: 3px 7px;
        background: #4caf50;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      this.metadslButton.onclick = () => this.toggleMetaDSL();

      // Clear log button
      const clearBtn = document.createElement('button');
      clearBtn.textContent = 'Clear Log';
      clearBtn.style.cssText = `
        padding: 3px 7px;
        background: #ff9800;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      clearBtn.onclick = () => this.clearLog();

      // Copy page HTML button
      const copyHtmlBtn = document.createElement('button');
      copyHtmlBtn.textContent = 'Copy Sel';
      copyHtmlBtn.style.cssText = `
        padding: 3px 7px;
        background: #9c27b0;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      copyHtmlBtn.onclick = () => this.copyPageHTML();

      // Execute MetaDSL button
      const execMetaDslBtn = document.createElement('button');
      execMetaDslBtn.textContent = 'Exec DSL';
      execMetaDslBtn.style.cssText = `
        padding: 3px 7px;
        background: #673ab7;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      execMetaDslBtn.onclick = () => this.runMetaDSL();

      // Execute JavaScript button
      const execJsBtn = document.createElement('button');
      execJsBtn.textContent = 'Exec JS';
      execJsBtn.style.cssText = `
        padding: 3px 7px;
        background: #f44336;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      execJsBtn.onclick = () => this.executeJavascript();

      // Clear operation queue button
      const clearQueueBtn = document.createElement('button');
      clearQueueBtn.textContent = 'Clr Queue';
      clearQueueBtn.style.cssText = `
        padding: 3px 7px;
        background: #ff5722;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      clearQueueBtn.onclick = () => this.clearOperationQueue();

      // Auto Plan toggle button
      this.autoPlanButton = document.createElement('button');
      this.autoPlanButton.textContent = '✓ Auto Plan';
      this.autoPlanButton.style.cssText = `
        padding: 3px 7px;
        background: #4caf50;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      this.autoPlanButton.onclick = () => this.toggleAutoPlan();

      buttonBar.appendChild(testBtn);
      buttonBar.appendChild(this.metadslButton);
      buttonBar.appendChild(clearQueueBtn);
      buttonBar.appendChild(clearBtn);
      buttonBar.appendChild(copyHtmlBtn);
      buttonBar.appendChild(execMetaDslBtn);
      buttonBar.appendChild(execJsBtn);

      // Option bar: Auto Plan + toggle options
      const optionBar = document.createElement('div');
      optionBar.style.cssText = `
        padding: 6px 8px;
        background: #252525;
        border-bottom: 1px solid #444;
        display: flex;
        gap: 5px;
        flex-wrap: wrap;
      `;

      // Collapse Agent Reply toggle button
      this.collapseReplyButton = document.createElement('button');
      this.collapseReplyButton.textContent = CONFIG.config.panel.collapseAgentReply ? '✓ Collapse Reply' : '✗ Collapse Reply';
      this.collapseReplyButton.style.cssText = `
        padding: 3px 7px;
        background: ${CONFIG.config.panel.collapseAgentReply ? '#4caf50' : '#666'};
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      this.collapseReplyButton.onclick = () => this.toggleCollapseReply();

      // Hide MetaDSL Block toggle button
      this.hideBlockButton = document.createElement('button');
      this.hideBlockButton.textContent = CONFIG.config.panel.hideMetaDslBlock ? '✓ Hide DSL Block' : '✗ Hide DSL Block';
      this.hideBlockButton.style.cssText = `
        padding: 3px 7px;
        background: ${CONFIG.config.panel.hideMetaDslBlock ? '#4caf50' : '#666'};
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      this.hideBlockButton.onclick = () => this.toggleHideBlock();

      optionBar.appendChild(this.autoPlanButton);
      optionBar.appendChild(this.collapseReplyButton);
      optionBar.appendChild(this.hideBlockButton);

      // Streaming Page toggle button
      this.streamingPageButton = document.createElement('button');
      this.streamingPageButton.textContent = CONFIG.config.panel.streamingPage ? '\u2713 Streaming' : '\u2717 Streaming';
      this.streamingPageButton.style.cssText = `
        padding: 3px 7px;
        background: ${CONFIG.config.panel.streamingPage ? '#4caf50' : '#666'};
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      this.streamingPageButton.onclick = () => this.toggleStreamingPage();
      optionBar.appendChild(this.streamingPageButton);

      // Toggle Chat Input Panel button (default hidden)
      this.toggleChatButton = document.createElement('button');
      this.toggleChatButton.textContent = '\u2717 Chat Panel';
      this.toggleChatButton.style.cssText = `
        padding: 3px 7px;
        background: #666;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      this.toggleChatButton.onclick = () => this.toggleChatPanel();
      optionBar.appendChild(this.toggleChatButton);

      // Toggle OpenClaw Panel button (default hidden)
      this.toggleClawButton = document.createElement('button');
      this.toggleClawButton.textContent = '\u2717 OpenClaw';
      this.toggleClawButton.style.cssText = `
        padding: 3px 7px;
        background: #666;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      this.toggleClawButton.onclick = () => this.toggleClawPanel();
      optionBar.appendChild(this.toggleClawButton);

      // JS Hot Reload toggle button
      this.jsHotReloadButton = document.createElement('button');
      this.jsHotReloadButton.textContent = CONFIG.config.panel.jsHotReload ? '\u2713 JS Reload' : '\u2717 JS Reload';
      this.jsHotReloadButton.style.cssText = `
        padding: 3px 7px;
        background: ${CONFIG.config.panel.jsHotReload ? '#4caf50' : '#666'};
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 11px;
      `;
      this.jsHotReloadButton.onclick = () => this.toggleJsHotReload();
      optionBar.appendChild(this.jsHotReloadButton);

      this.panel.appendChild(optionBar);
      this.panel.appendChild(buttonBar);

      // Create MetaDSL input area
      this.scriptInput = document.createElement('textarea');
      this.scriptInput.placeholder = 'Enter MetaDSL/Javascript here to execute...';
      this.scriptInput.value = 'format("{0} {1}",@LlmProviderId,llm_get_providers_config());';
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
        'USER_INPUT': '#2196f3',           // Blue
        'LLM_RESPONDING': '#ff9800',       // Orange
        'SCANNING_CODE_BLOCKS': '#9c27b0', // Purple
        'AGENT_EXECUTING': '#4caf50'       // Green
      };

      const stateNames = {
        'USER_INPUT': 'User Input',
        'LLM_RESPONDING': 'LLM Responding',
        'SCANNING_CODE_BLOCKS': 'Scanning Blocks',
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
      let contextCounterForKeep = 0;
      let contextCounterForAlign = 0;
      if (this.metadslMonitor.states && this.metadslMonitor.states['LLM_RESPONDING']) {
        contextCounterForKeep = this.metadslMonitor.states['LLM_RESPONDING'].contextCounterForKeep || 0;
        contextCounterForAlign = this.metadslMonitor.states['LLM_RESPONDING'].contextCounterForAlign || 0;
      }

      // Get operation executed flag from AGENT_EXECUTING state
      let operationExecuted = false;
      if (this.metadslMonitor.states && this.metadslMonitor.states['AGENT_EXECUTING']) {
        operationExecuted = this.metadslMonitor.states['AGENT_EXECUTING'].operationExecuted || false;
      }

      // Update info text
      const execStatus = operationExecuted ? 'Yes' : 'No';
      this.stateInfo.textContent = `Duration: ${duration}s | Queue: ${queueLength} | Counter: ${contextCounterForKeep} ${contextCounterForAlign} | Executed: ${execStatus}`;
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

    toggleCollapseReply() {
      CONFIG.config.panel.collapseAgentReply = !CONFIG.config.panel.collapseAgentReply;
      CONFIG.saveConfig();
      const on = CONFIG.config.panel.collapseAgentReply;
      this.collapseReplyButton.textContent = on ? '✓ Collapse Reply' : '✗ Collapse Reply';
      this.collapseReplyButton.style.background = on ? '#4caf50' : '#666';
      this.log(on ? '✓ Collapse reply enabled' : '✗ Collapse reply disabled');
    }

    toggleHideBlock() {
      CONFIG.config.panel.hideMetaDslBlock = !CONFIG.config.panel.hideMetaDslBlock;
      CONFIG.saveConfig();
      const on = CONFIG.config.panel.hideMetaDslBlock;
      this.hideBlockButton.textContent = on ? '✓ Hide DSL Block' : '✗ Hide DSL Block';
      this.hideBlockButton.style.background = on ? '#4caf50' : '#666';
      this.log(on ? '✓ Hide DSL block enabled' : '✗ Hide DSL block disabled');
    }

    toggleChatPanel() {
      if (this.chatInputPanel) {
        this.chatInputPanel.toggle();
        const on = this.chatInputPanel.visible;
        this.toggleChatButton.textContent = on ? '\u2713 Chat Panel' : '\u2717 Chat Panel';
        this.toggleChatButton.style.background = on ? '#4caf50' : '#666';
      }
    }

    toggleClawPanel() {
      if (this.openClawPanel) {
        this.openClawPanel.toggle();
        const on = this.openClawPanel.visible;
        this.toggleClawButton.textContent = on ? '\u2713 OpenClaw' : '\u2717 OpenClaw';
        this.toggleClawButton.style.background = on ? '#4caf50' : '#666';
      }
    }

    toggleStreamingPage() {
      CONFIG.config.panel.streamingPage = !CONFIG.config.panel.streamingPage;
      CONFIG.saveConfig();
      const on = CONFIG.config.panel.streamingPage;
      this.streamingPageButton.textContent = on ? '\u2713 Streaming' : '\u2717 Streaming';
      this.streamingPageButton.style.background = on ? '#4caf50' : '#666';
      // Update metadslMonitor pageStableDelay
      if (this.metadslMonitor) {
        this.metadslMonitor.pageStableDelay = on ? 5000 : 300;
      }
      this.log(on ? '\u2713 Streaming page mode (5000ms delay)' : '\u2717 Non-streaming page mode (300ms delay)');
    }

    toggleJsHotReload() {
      CONFIG.config.panel.jsHotReload = !CONFIG.config.panel.jsHotReload;
      CONFIG.saveConfig();
      const on = CONFIG.config.panel.jsHotReload;
      this.jsHotReloadButton.textContent = on ? '\u2713 JS Reload' : '\u2717 JS Reload';
      this.jsHotReloadButton.style.background = on ? '#4caf50' : '#666';
      this.log(on ? '\u2713 JS hot reload enabled' : '\u2717 JS hot reload disabled');
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

