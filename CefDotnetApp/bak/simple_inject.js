/**
 * simple_inject.js - Simplified Control Panel
 *
 * This script provides a simple control panel with:
 * - Copy selected HTML to clipboard
 * - Execute JavaScript code
 * - Log output with level filtering
 */

(function() {
  'use strict';

  // ============================================================================
  // Configuration
  // ============================================================================
  const CONFIG = {
    // Panel settings
    panelPosition: 'bottom-right',
    panelWidth: 760,
    panelHeight: 540,

    // Log level settings
    logLevels: {
      debug: false,
      info: true,
      warn: true,
      error: true
    }
  };

  // ============================================================================
  // Logger - Simple logging with level filtering
  // ============================================================================
  class Logger {
    constructor(logArea) {
      this.logArea = logArea;
    }

    log(message, level = 'info') {
      if (!CONFIG.logLevels[level]) return;

      const levelPrefixes = {
        debug: '🔍',
        info: '📝',
        warn: '⚠️',
        error: '❌'
      };

      const timestamp = new Date().toLocaleTimeString();
      const prefix = levelPrefixes[level] || '📝';
      const text = `[${timestamp}] ${prefix} ${message}\n`;

      if (this.logArea) {
        this.logArea.value += text;
        this.logArea.scrollTop = this.logArea.scrollHeight;
      }

      // Also log to console
      const consoleMethod = level === 'error' ? 'error' : level === 'warn' ? 'warn' : 'log';
      console[consoleMethod](`[SimplePanel] ${message}`);
    }

    debug(message) { this.log(message, 'debug'); }
    info(message) { this.log(message, 'info'); }
    warn(message) { this.log(message, 'warn'); }
    error(message) { this.log(message, 'error'); }

    clear() {
      if (this.logArea) {
        this.logArea.value = '';
      }
    }
  }

  // ============================================================================
  // ControlPanel - Visual Control Panel
  // ============================================================================
  class ControlPanel {
    constructor() {
      this.visible = false;
      this.panel = null;
      this.logArea = null;
      this.scriptInput = null;
      this.logger = null;

      this.createPanel();
    }

    createPanel() {
      // Create panel element
      this.panel = document.createElement('div');
      this.panel.id = 'simple-control-panel';
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
        min-width: 400px;
        min-height: 300px;
      `;

      // Prevent events from bubbling
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

      const title = document.createElement('span');
      title.textContent = 'Simple Control Panel';
      title.style.cssText = `
        color: #fff;
        font-weight: 600;
        font-size: 14px;
      `;

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

      header.appendChild(title);
      header.appendChild(closeBtn);
      this.panel.appendChild(header);

      // Create log level filter bar
      const logFilterBar = document.createElement('div');
      logFilterBar.style.cssText = `
        padding: 8px 15px;
        background: #1e1e1e;
        border-bottom: 1px solid #444;
        display: flex;
        gap: 6px;
        align-items: center;
      `;

      const logFilterLabel = document.createElement('span');
      logFilterLabel.textContent = 'Log Levels:';
      logFilterLabel.style.cssText = `
        color: #999;
        font-size: 12px;
        margin-right: 4px;
      `;
      logFilterBar.appendChild(logFilterLabel);

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
          padding: 2px 6px;
          border-radius: 4px;
          font-size: 12px;
          transition: all 0.2s;
          opacity: ${CONFIG.logLevels[level.key] ? '1' : '0.4'};
        `;
        btn.onclick = () => {
          CONFIG.logLevels[level.key] = !CONFIG.logLevels[level.key];
          btn.style.background = CONFIG.logLevels[level.key] ? level.color : '#555';
          btn.style.opacity = CONFIG.logLevels[level.key] ? '1' : '0.4';
          this.logger.info(`${level.icon} ${level.title} logs ${CONFIG.logLevels[level.key] ? 'enabled' : 'disabled'}`);
        };
        logFilterBar.appendChild(btn);
      });

      this.panel.appendChild(logFilterBar);

      // Create button bar
      const buttonBar = document.createElement('div');
      buttonBar.style.cssText = `
        padding: 10px;
        background: #252525;
        border-bottom: 1px solid #444;
        display: flex;
        gap: 8px;
        flex-wrap: wrap;
      `;

      // Copy HTML button
      const copyHtmlBtn = document.createElement('button');
      copyHtmlBtn.textContent = 'Copy Selection';
      copyHtmlBtn.style.cssText = `
        padding: 6px 12px;
        background: #9c27b0;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      copyHtmlBtn.onclick = () => this.copySelectedHTML();

      // Execute JavaScript button
      const execJsBtn = document.createElement('button');
      execJsBtn.textContent = 'Execute JS';
      execJsBtn.style.cssText = `
        padding: 6px 12px;
        background: #f44336;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      execJsBtn.onclick = () => this.executeJavascript();

      // Clear log button
      const clearBtn = document.createElement('button');
      clearBtn.textContent = 'Clear Log';
      clearBtn.style.cssText = `
        padding: 6px 12px;
        background: #ff9800;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 12px;
      `;
      clearBtn.onclick = () => this.logger.clear();

      buttonBar.appendChild(copyHtmlBtn);
      buttonBar.appendChild(execJsBtn);
      buttonBar.appendChild(clearBtn);
      this.panel.appendChild(buttonBar);

      // Create script input area
      this.scriptInput = document.createElement('textarea');
      this.scriptInput.placeholder = 'Paste JavaScript code here to execute...';
      this.scriptInput.style.cssText = `
        height: 120px;
        background: #2d2d2d;
        color: #d4d4d4;
        border: 1px solid #444;
        border-left: 3px solid #f44336;
        padding: 10px;
        font-family: 'Consolas', 'Monaco', monospace;
        font-size: 12px;
        resize: vertical;
        outline: none;
        margin-bottom: 5px;
      `;

      this.scriptInput.addEventListener('beforeinput', (e) => {
        e.stopPropagation();
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

      // Initialize logger
      this.logger = new Logger(this.logArea);

      // Make panel draggable
      this.makeDraggable(header, this.panel);

      // Make panel resizable
      this.makeResizable(this.panel);
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
      const handleSize = 8;

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
      this.logger.info('Control panel opened');
    }

    hide() {
      this.panel.style.display = 'none';
      this.visible = false;
    }

    toggle() {
      if (this.visible) {
        this.hide();
      } else {
        this.show();
      }
    }

    copySelectedHTML() {
      this.logger.info('Copying selected HTML to clipboard...');

      try {
        const selection = window.getSelection();

        if (!selection || selection.rangeCount === 0 || selection.isCollapsed) {
          this.logger.warn('No content selected. Please select HTML content first.');
          return;
        }

        const range = selection.getRangeAt(0);
        const container = document.createElement('div');
        container.appendChild(range.cloneContents());

        // Remove the control panel if it exists in the selection
        const controlPanel = container.querySelector('#simple-control-panel');
        if (controlPanel) {
          controlPanel.remove();
          this.logger.debug('Removed control panel from selection');
        }

        const html = container.innerHTML;

        if (!html || html.trim().length === 0) {
          this.logger.warn('Selected content is empty');
          return;
        }

        // Copy to clipboard
        if (navigator.clipboard && navigator.clipboard.writeText) {
          navigator.clipboard.writeText(html).then(() => {
            this.logger.info(`✓ Selected HTML copied to clipboard (${html.length} characters)`);
          }).catch(err => {
            this.logger.error('Failed to copy to clipboard: ' + err);
            this.fallbackCopy(html);
          });
        } else {
          this.fallbackCopy(html);
        }
      } catch (e) {
        this.logger.error('Error: ' + e.message);
      }
    }

    fallbackCopy(text) {
      try {
        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'fixed';
        textarea.style.opacity = '0';
        document.body.appendChild(textarea);
        textarea.select();

        const success = document.execCommand('copy');
        document.body.removeChild(textarea);

        if (success) {
          this.logger.info(`✓ HTML copied using fallback method (${text.length} characters)`);
        } else {
          this.logger.error('Fallback copy also failed');
        }
      } catch (err) {
        this.logger.error('Fallback copy error: ' + err);
      }
    }

    executeJavascript() {
      this.logger.info('Executing JavaScript...');
      try {
        const script = this.scriptInput.value.trim();
        if (!script) {
          this.logger.warn('Script input is empty. Please paste JavaScript code first.');
          return;
        }

        this.logger.info('─'.repeat(50));
        this.logger.info('Executing script:');
        this.logger.info(script.substring(0, 200) + (script.length > 200 ? '...' : ''));
        this.logger.info('─'.repeat(50));

        const result = eval(script);

        this.logger.info('✓ Script executed successfully');
        if (result !== undefined) {
          this.logger.info('Result: ' + JSON.stringify(result, null, 2));
        }
      } catch (e) {
        this.logger.error('Script execution error: ' + e.message);
        this.logger.error('Stack: ' + e.stack);
      }
    }
  }

  // ============================================================================
  // Initialization
  // ============================================================================
  function initialize() {
    // Check if running in DevTools context
    if (window.location.protocol === 'devtools:' || window.location.href.includes('devtools://')) {
      console.log('[SimplePanel] Skipping initialization in DevTools context');
      return;
    }

    // Check if already initialized
    if (window.SimplePanel) {
      console.log('[SimplePanel] Already initialized, skipping');
      return;
    }

    console.log('[SimplePanel] Initializing...');

    const panel = new ControlPanel();

    // Expose global API
    window.SimplePanel = {
      panel: panel,
      show: () => panel.show(),
      hide: () => panel.hide(),
      toggle: () => panel.toggle(),
      log: (msg, level) => panel.logger.log(msg, level)
    };

    // Keyboard shortcut: Ctrl+Shift+A to toggle panel (use capture phase to ensure it works)
    document.addEventListener('keydown', (e) => {
      if (e.ctrlKey && e.shiftKey && (e.key === 'A' || e.key === 'a')) {
        e.preventDefault();
        e.stopPropagation();
        panel.toggle();
      }
    }, true);

    console.log('[SimplePanel] Initialization complete');
    console.log('[SimplePanel] Press Ctrl+Shift+A to toggle control panel');
    console.log('[SimplePanel] Access API via window.SimplePanel');
  }

  // Start when DOM is ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initialize);
  } else {
    initialize();
  }

})();