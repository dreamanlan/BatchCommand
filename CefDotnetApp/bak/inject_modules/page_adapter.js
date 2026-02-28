  // ============================================================================
  // Base Page Adapter
  // ============================================================================
  class PageAdapter {
    constructor(bridge) {
      this.logger = logger.createLogger('PageAdapter');
      this.bridge = bridge;
      this.pageType = this.detectPageType();
      this.lastResponse = null;
      this.messageHistory = [];
      this.onPageTypeChanged = null; // Callback for page type changes

      // Last scanned response: set after code block scan completes
      this.lastScannedResponse = null;
      this.lastScannedElement = null;

      // If detection failed, retry after DOM is fully loaded
      if (this.pageType === 'unknown') {
        if (document.readyState === 'loading') {
          document.addEventListener('DOMContentLoaded', () => {
            this.pageType = this.detectPageType();
            this.logger.debug('Re-detected page type after DOMContentLoaded', { pageType: this.pageType });
            if (this.onPageTypeChanged) {
              this.onPageTypeChanged(this.pageType);
            }
          });
        } else {
          // DOM already loaded, retry after a short delay
          const f = () => {
            const newType = this.detectPageType();
            if (newType !== 'unknown') {
              this.pageType = newType;
              this.logger.debug('Re-detected page type after delay', { pageType: this.pageType });
              if (this.onPageTypeChanged) {
                this.onPageTypeChanged(this.pageType);
              }
            }
            else {
              setTimeout(f, 500);
            }
          };
          setTimeout(f, 500);
        }
      }
    }

    detectPageType() {
      // Detect current page type based on URL or DOM
      // Priority: DOM-based detection first (more specific), then URL-based
      const hostname = window.location.hostname;
      const pathname = window.location.pathname;
      const href = window.location.href;

      // 1. Check for local agent (agent.html)
      // Detect by specific DOM structure: has both #editable-content and #messages-area
      if (document.querySelector('#editable-content[contenteditable="true"]') &&
          document.querySelector('#messages-area')) {
        return 'local-agent';
      }

      // 2. Check for custom LLM interface by DOM (simplified selector)
      // Try multiple selectors to be more flexible
      if (document.querySelector('#editable-content[contenteditable="true"]') ||
          document.querySelector('#editable-content.editable-content') ||
          document.querySelector('.editable-content[contenteditable="true"]')) {
        return 'custom-llm';
      }

      // 3. Fallback to unknown
      return 'unknown';
    }

    sendMessage(text) {
      throw new Error('sendMessage must be implemented by subclass');
    }

    getLastScannedResponse() {
      return this.lastScannedResponse;
    }

    setLastScannedResponse(text, element) {
      this.lastScannedResponse = text;
      this.lastScannedElement = element || null;
    }

    isLastResponseCurrent() {
      // Check if the last scanned response element is still the latest LLM message on page
      if (!this.lastScannedElement) return false;
      const msgBoxes = document.querySelectorAll('.vac-message-box:not(.vac-offset-current)');
      if (msgBoxes.length === 0) return false;
      const lastMsgBox = msgBoxes[msgBoxes.length - 1];
      // Walk up from lastScannedElement to find its message box
      let scannedMsgBox = this.lastScannedElement;
      while (scannedMsgBox && !scannedMsgBox.classList.contains('vac-message-box')) {
        scannedMsgBox = scannedMsgBox.parentElement;
      }
      return scannedMsgBox === lastMsgBox;
    }

    observeResponseChanges(callback) {
      throw new Error('observeResponseChanges must be implemented by subclass');
    }

    getHistory(count) {
      // Get the last N conversation pairs (question-answer)
      // Returns a formatted string with plain text (no HTML/Markdown)
      try {
        const conversations = this.extractConversations(count);
        if (conversations.length === 0) {
          return '';
        }

        // Format conversations as plain text
        const formatted = conversations.map((conv, index) => {
          var userText = this.cleanText(conv.user);
          const assistantText = this.cleanText(conv.assistant);
          return `[Conversation ${index + 1}]\nUser:\n${userText}\n\nAssistant:\n${assistantText}`;
        }).join('\n\n' + '='.repeat(80) + '\n\n');

        return formatted;
      } catch (e) {
        this.logger.error('Error getting history', { error: e.toString() });
        return '';
      }
    }

    getVisibleText(el) {
      // Collect text from visible nodes only (skip display:none elements)
      let text = '';
      for (const node of el.childNodes) {
        if (node.nodeType === Node.TEXT_NODE) {
          text += node.textContent;
        } else if (node.nodeType === Node.ELEMENT_NODE) {
          if (node.style && node.style.display === 'none') continue;
          if (node.style && node.style.visibility === 'hidden') continue;
          text += this.getVisibleText(node);
        }
      }
      return text;
    }

    extractConversations(count) {
      // Extract conversation pairs based on page type
      // Should be overridden by subclasses for specific implementations
      switch (this.pageType) {
        case 'local-agent':
          return this.extractLocalAgentConversations(count);
        case 'custom-llm':
          return this.extractCustomLLMConversations(count);
        default:
          this.logger.warn('Unsupported page type for history extraction', { pageType: this.pageType });
          return [];
      }
    }

    extractNewConversations() {
      // Extract only unsaved conversation pairs by checking data-agent-saved marker on wrappers
      const messageWrappers = Array.from(document.querySelectorAll('.vac-message-wrapper'));
      const newConversations = [];
      let currentPair = null;
      let currentUserWrapper = null;
      let currentAssistantWrapper = null;

      for (const wrapper of messageWrappers) {
        const messageBox = wrapper.querySelector('.vac-message-box');
        if (!messageBox) continue;

        const isUser = messageBox.classList.contains('vac-offset-current');

        if (isUser) {
          // Save previous complete pair if both wrappers are unsaved
          if (currentPair && currentPair.assistant &&
              currentUserWrapper && !currentUserWrapper.dataset.agentSaved &&
              currentAssistantWrapper && !currentAssistantWrapper.dataset.agentSaved) {
            newConversations.push(currentPair);
            currentUserWrapper.dataset.agentSaved = '1';
            currentAssistantWrapper.dataset.agentSaved = '1';
          }
          // Use empty string for agent-injected messages, real text for human user messages
          const userText = messageBox.dataset.agentCollapsed ? '' : this.cleanText(this.getVisibleText(wrapper));
          currentPair = { user: userText, assistant: '' };
          currentUserWrapper = wrapper;
          currentAssistantWrapper = null;
        } else if (currentPair) {
          currentPair.assistant = this.cleanText(this.getVisibleText(wrapper));
          currentAssistantWrapper = wrapper;
        }
      }

      // Handle last pair
      if (currentPair && currentPair.assistant &&
          currentUserWrapper && !currentUserWrapper.dataset.agentSaved &&
          currentAssistantWrapper && !currentAssistantWrapper.dataset.agentSaved) {
        newConversations.push(currentPair);
        currentUserWrapper.dataset.agentSaved = '1';
        currentAssistantWrapper.dataset.agentSaved = '1';
      }

      return newConversations;
    }

    extractLocalAgentConversations(count) {
      // Extract local agent conversations
      // Look for .vac-message-wrapper which contains each message
      const messageWrappers = Array.from(document.querySelectorAll('.vac-message-wrapper'));

      const conversations = [];
      let currentPair = null;

      // Process messages in order
      for (const wrapper of messageWrappers) {
        // Check if this is a user message by looking for .vac-offset-current class in .vac-message-box
        const messageBox = wrapper.querySelector('.vac-message-box');
        if (!messageBox) continue;

        const isUser = messageBox.classList.contains('vac-offset-current');

        if (isUser) {
          // Start new conversation pair
          if (currentPair && currentPair.user && currentPair.assistant) {
            conversations.push(currentPair);
          }
          // Use placeholder for agent-injected messages, real text for human user messages
          const userText = messageBox.dataset.agentCollapsed ? '...' : this.getVisibleText(wrapper);
          currentPair = { user: userText, assistant: '' };
        } else if (currentPair) {
          // Assistant message - LLM reply, read visible text
          currentPair.assistant = this.getVisibleText(wrapper);
        }
      }

      // Add last pair if complete
      if (currentPair && currentPair.user && currentPair.assistant) {
        conversations.push(currentPair);
      }

      // Return last N conversations
      return conversations.slice(-count);
    }

    extractCustomLLMConversations(count) {
      // Extract custom LLM conversations
      // Look for .vac-message-wrapper which contains each message
      const messageWrappers = Array.from(document.querySelectorAll('.vac-message-wrapper'));

      const conversations = [];
      let currentPair = null;

      // Process messages in order
      for (const wrapper of messageWrappers) {
        // Check if this is a user message by looking for .vac-offset-current class in .vac-message-box
        const messageBox = wrapper.querySelector('.vac-message-box');
        if (!messageBox) continue;

        const isUser = messageBox.classList.contains('vac-offset-current');

        if (isUser) {
          // Start new conversation pair
          if (currentPair && currentPair.user && currentPair.assistant) {
            conversations.push(currentPair);
          }
          // Use placeholder for agent-injected messages, real text for human user messages
          const userText = messageBox.dataset.agentCollapsed ? '...' : this.getVisibleText(wrapper);
          currentPair = { user: userText, assistant: '' };
        } else if (currentPair) {
          // Assistant message - LLM reply, read visible text
          currentPair.assistant = this.getVisibleText(wrapper);
        }
      }

      // Add last pair if complete
      if (currentPair && currentPair.user && currentPair.assistant) {
        conversations.push(currentPair);
      }

      // Return last N conversations
      return conversations.slice(-count);
    }

    /**
     * Check if the last message in the conversation is from LLM (assistant)
     * @returns {boolean} true if last message is from LLM, false if from user or unknown
     */
    isLastMessageFromLLM() {
      switch (this.pageType) {
        case 'local-agent':
          return this.isLastMessageFromLLM_LocalAgent();
        case 'custom-llm':
          return this.isLastMessageFromLLM_CustomLLM();
        default:
          return false;
      }
    }

    isLastMessageFromLLM_LocalAgent() {
      const messageWrappers = document.querySelectorAll('.vac-message-wrapper');
      if (messageWrappers.length === 0) return false;

      const lastWrapper = messageWrappers[messageWrappers.length - 1];
      const messageBox = lastWrapper.querySelector('.vac-message-box');
      if (!messageBox) return false;

      // If it has vac-offset-current, it's a user message
      const isUser = messageBox.classList.contains('vac-offset-current');
      return !isUser;
    }

    isLastMessageFromLLM_CustomLLM() {
      const messageWrappers = document.querySelectorAll('.vac-message-wrapper');
      if (messageWrappers.length === 0) return false;

      const lastWrapper = messageWrappers[messageWrappers.length - 1];
      const messageBox = lastWrapper.querySelector('.vac-message-box');
      if (!messageBox) return false;

      // If it has vac-offset-current, it's a user message
      const isUser = messageBox.classList.contains('vac-offset-current');
      return !isUser;
    }

    cleanText(text) {
      if (!text) return '';

      // Remove HTML tags
      let cleaned = text.replace(/<[^>]*>/g, '');

      // Remove Markdown formatting
      // Remove bold/italic: **text** or __text__ or *text* or _text_
      cleaned = cleaned.replace(/(\*\*|__)(.*?)\1/g, '$2');
      cleaned = cleaned.replace(/(\*|_)(.*?)\1/g, '$2');

      // Remove headers: # Header
      cleaned = cleaned.replace(/^#{1,6}\s+/gm, '');

      // Remove inline code: `code`
      cleaned = cleaned.replace(/`([^`]+)`/g, '$1');

      // Remove code blocks: ```code```
      cleaned = cleaned.replace(/```[\s\S]*?```/g, (match) => {
        // Keep code block content but remove markers
        return match.replace(/```[^\n]*\n?/g, '').replace(/```$/g, '');
      });

      // Remove links: [text](url)
      cleaned = cleaned.replace(/\[([^\]]+)\]\([^\)]+\)/g, '$1');

      // Remove images: ![alt](url)
      cleaned = cleaned.replace(/!\[([^\]]*)\]\([^\)]+\)/g, '$1');

      // Normalize whitespace but preserve newlines
      // Replace multiple spaces with single space
      cleaned = cleaned.replace(/ +/g, ' ');

      // Replace tabs with 2 spaces
      cleaned = cleaned.replace(/\t/g, '  ');

      // Remove leading/trailing whitespace from each line
      cleaned = cleaned.split('\n').map(line => line.trim()).join('\n');

      // Remove excessive blank lines (more than 2 consecutive)
      cleaned = cleaned.replace(/\n{3,}/g, '\n\n');

      // Trim overall
      cleaned = cleaned.trim();

      return cleaned;
    }
  }

  // ============================================================================
  // LLM Page Adapter (for local agent and custom LLM interfaces)
  // ============================================================================
  class LLMPageAdapter extends PageAdapter {
    constructor(bridge) {
      super(bridge);
      this.logger = logger.createLogger('LLMPageAdapter');
      this.observer = null;
      this.responseCallbacks = [];
    }

    sendMessage(text) {
      switch (this.pageType) {
        case 'local-agent':
          this.sendLocalAgentMessage(text);
          break;
        case 'custom-llm':
          this.sendCustomLLMMessage(text);
          break;
        case 'test':
          this.sendTestMessage(text);
          break;
        default:
          this.logger.warn('Unsupported page type', { pageType: this.pageType });
      }
    }

    sendTestMessage(text) {
      // Test page specific implementation
      const inputArea = document.getElementById('userInput');
      const outputArea = document.getElementById('simulatedResponse');

      if (inputArea && outputArea) {
        inputArea.value = text;

        // Simulate response
        setTimeout(() => {
          const response = document.createElement('div');
          response.innerHTML = `<p><strong>Simulated AI:</strong> ${text}</p>`;
          outputArea.appendChild(response);
          this.lastResponse = text;
          this.notifyResponseChange(text);
        }, 500);
      }
    }

    sendLocalAgentMessage(text) {
      const input = document.getElementById('editable-content');
      if (!input) {
        this.logger.warn('Local Agent input element #editable-content not found');
        return;
      }

      // Set message content
      input.textContent = text;
      input.dispatchEvent(new Event('input', { bubbles: true }));

      // Click send button after a short delay
      setTimeout(() => {
        const btn = document.querySelector('.vac-icon-textarea .vac-svg-button:last-child');
        if (btn) {
          btn.classList.remove('vac-send-disabled');
          btn.click();
          this.logger.debug('Local Agent message sent', { text });
        } else {
          this.logger.warn('Send button not found');
        }
      }, CONFIG.sendMessageDelay);
    }

    sendCustomLLMMessage(text) {
      const input = document.getElementById('editable-content');
      if (!input) {
        this.logger.warn('Custom LLM input element #editable-content not found');
        return;
      }

      // Set message content
      input.textContent = text;
      input.dispatchEvent(new Event('input', { bubbles: true }));

      // Click send button after a short delay
      setTimeout(() => {
        const btn = document.querySelector('.vac-icon-textarea .vac-svg-button:last-child');
        if (btn) {
          btn.classList.remove('vac-send-disabled');
          btn.click();
          this.logger.debug('Custom LLM message sent', { text });
        } else {
          this.logger.warn('Send button not found');
        }
      }, CONFIG.sendMessageDelay);
    }

    observeResponseChanges(callback) {
      this.responseCallbacks.push(callback);

      if (this.observer) return; // Already observing

      // Use MutationObserver to detect new messages
      const targetNode = document.body;
      const config = { childList: true, subtree: true };

      this.observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
          if (mutation.addedNodes.length > 0) {
            // Check if new response elements were added
            const newResponse = this.extractLatestResponse();
            if (newResponse && newResponse !== this.lastResponse) {
              this.lastResponse = newResponse;
              this.messageHistory.push({
                timestamp: Date.now(),
                content: newResponse
              });
              this.notifyResponseChange(newResponse);
            }
          }
        });
      });

      this.observer.observe(targetNode, config);
    }

    extractLatestResponse() {
      switch (this.pageType) {
        case 'local-agent':
          return this.extractLocalAgentResponse();
        case 'custom-llm':
          return this.extractCustomLLMResponse();
        case 'test':
          return this.extractTestResponse();
        default:
          return null;
      }
    }

    extractTestResponse() {
      const outputArea = document.getElementById('simulatedResponse');
      if (outputArea) {
        return outputArea.textContent;
      }
      return null;
    }

    extractLocalAgentResponse() {
      // Extract latest response from local agent interface
      const messagesArea = document.getElementById('messages-area');
      if (!messagesArea) return null;
      const messages = messagesArea.querySelectorAll('.vac-message-wrapper');
      if (messages.length === 0) return null;
      // Iterate from last to first, find the latest assistant message
      for (let i = messages.length - 1; i >= 0; i--) {
        const msg = messages[i];
        // User messages have the 'user' class on the wrapper
        if (msg.classList.contains('user')) continue;
        // Get text from .message-content to avoid including timestamps
        const content = msg.querySelector('.message-content');
        if (content) {
          const text = content.textContent.trim();
          if (text) return text;
        }
      }
      return null;
    }

    extractCustomLLMResponse() {
      // Extract latest response from custom LLM interface
      // Look for message containers in the chat area
      const messages = document.querySelectorAll('.vac-message-wrapper, .message-wrapper, [class*="message"]');

      if (messages.length > 0) {
        // Get the last message that's not from the user
        for (let i = messages.length - 1; i >= 0; i--) {
          const msg = messages[i];
          // Skip user messages (usually have specific classes)
          if (!msg.classList.contains('user-message') &&
              !msg.classList.contains('vac-message-current')) {
            return msg.textContent;
          }
        }
      }

      return null;
    }

    notifyResponseChange(response) {
      this.responseCallbacks.forEach(callback => {
        try {
          callback(response);
        } catch (e) {
          this.logger.error('Error in response callback', { error: e.toString() });
        }
      });
    }

    setSystemPrompt(promptText) {
      try {
        // Check if this is local-agent page
        if (this.pageType === 'local-agent') {
          // Call window.setDialogPrompt if available
          if (typeof window.setDialogPrompt === 'function') {
            window.setDialogPrompt(promptText);
            this.logger.debug('System prompt set via window.setDialogPrompt', { length: promptText.length });
            return true;
          } else {
            this.logger.warn('window.setDialogPrompt not available on local-agent page');
            return false;
          }
        }

        // Original logic for other page types
        // Find the label with text 'system prompt:'
        const label = [...document.querySelectorAll('label.el-form-item__label')]
          .find(l => l.textContent.trim() === 'system prompt:');

        if (!label) {
          this.logger.warn('System prompt label not found');
          return false;
        }

        // Get the textarea ID from the label's 'for' attribute
        const textareaId = label.getAttribute('for');
        if (!textareaId) {
          this.logger.warn('Textarea ID not found in label');
          return false;
        }

        // Get the textarea element
        const textarea = document.getElementById(textareaId);
        if (!textarea) {
          this.logger.warn('System prompt textarea not found', { textareaId });
          return false;
        }

        // Set the value and trigger input event
        textarea.value = promptText;
        textarea.dispatchEvent(new Event('input', { bubbles: true }));

        this.logger.debug('System prompt set successfully', { length: promptText.length });
        return true;
      } catch (e) {
        this.logger.error('Failed to set system prompt', { error: e.toString() });
        return false;
      }
    }
  }

