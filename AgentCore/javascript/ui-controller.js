/**
 * UI Controller Module
 * Manages UI interactions and updates
 */

// Create logger instance for this module
const uiLogger = window.logger ? window.logger.createLogger('UIController') : null;

class UIController {
    constructor(messageHandler, apiClient) {
        this.messageHandler = messageHandler;
        this.apiClient = apiClient;
        this.isProcessing = false;

        // Initialize marked.js for Markdown rendering
        this.initializeMarkdown();

        this.elements = {
            messagesArea: document.getElementById('messages-area'),
            editableContent: document.getElementById('editable-content'),
            sendBtn: document.getElementById('send-btn'),
            stopBtn: document.getElementById('stop-btn'),
            loadingIndicator: document.getElementById('loading-indicator'),
            apiTypeSelect: document.getElementById('api-type'),
            configBtn: document.getElementById('config-btn'),
            configModal: document.getElementById('config-modal'),
            configApiType: document.getElementById('config-api-type'),
            apiKeyInput: document.getElementById('api-key'),
            authModeSelect: document.getElementById('auth-mode'),
            authModeGroup: document.getElementById('auth-mode-group'),
            usernameInput: document.getElementById('username'),
            usernameGroup: document.getElementById('username-group'),
            apiEndpointInput: document.getElementById('api-endpoint'),
            modelSelect: document.getElementById('model'),
            modelTextInput: document.getElementById('model-text'),
            streamCheckbox: document.getElementById('stream-enabled'),
            streamGroup: document.getElementById('stream-group'),
            enableWebSearchCheckbox: document.getElementById('enable-web-search'),
            webSearchGroup: document.getElementById('web-search-group'),
            enableThinkingCheckbox: document.getElementById('enable-thinking'),
            thinkingGroup: document.getElementById('thinking-group'),
            reasoningEffortSelect: document.getElementById('reasoning-effort'),
            reasoningEffortGroup: document.getElementById('reasoning-effort-group'),
            contextRoundsInput: document.getElementById('context-rounds'),
            maxContextCharsInput: document.getElementById('max-context-chars'),
            maxHistoryMessagesInput: document.getElementById('max-history-messages'),
            saveBtn: document.getElementById('save-btn'),
            cancelBtn: document.getElementById('cancel-btn'),
            errorMessage: document.getElementById('error-message'),
            imageUrlsList: document.getElementById('image-urls-list'),
            addImageUrlBtn: document.getElementById('add-image-url-btn'),
            clearImageUrlsBtn: document.getElementById('clear-image-urls-btn')
        };

        this.initializeEventListeners();
        this.loadExistingMessages();
        this.updateSendButtonState();
    }

    initializeMarkdown() {
        // Configure marked.js
        if (typeof marked !== 'undefined') {
            marked.setOptions({
                highlight: function (code, lang) {
                    if (typeof hljs !== 'undefined' && lang && hljs.getLanguage(lang)) {
                        try {
                            return hljs.highlight(code, { language: lang }).value;
                        } catch (e) {
                            if (uiLogger) uiLogger.error('Highlight error:', e);
                        }
                    }
                    return code;
                },
                breaks: true,
                gfm: true
            });
        }
        // Initialize mermaid (figures rendered after streaming completes)
        if (typeof mermaid !== 'undefined') {
            try {
                mermaid.initialize({ startOnLoad: false, theme: 'default', securityLevel: 'loose' });
            } catch (e) {
                if (uiLogger) uiLogger.error('Mermaid init error:', e);
            }
        }

    }


    processCodeBlocks(container) {
        if (!container) return;
        if (typeof mermaid !== 'undefined') {
            const mermaidBlocks = container.querySelectorAll('pre code.language-mermaid');
            mermaidBlocks.forEach((codeEl, idx) => {
                const code = codeEl.textContent;
                const host = document.createElement('div');
                host.className = 'mermaid-rendered';
                const id = 'mmd-' + Date.now() + '-' + idx + '-' + Math.floor(Math.random() * 10000);
                mermaid.render(id, code).then(function (result) {
                    host.innerHTML = result.svg;
                }).catch(function (e) {
                    if (uiLogger) uiLogger.error('Mermaid render error:', e);
                    host.textContent = 'Mermaid render error: ' + (e && e.message ? e.message : String(e));
                });
                const pre = codeEl.closest('pre');
                if (pre && pre.parentNode) {
                    pre.parentNode.replaceChild(host, pre);
                }
            });
        }
        if (typeof DOMPurify !== 'undefined') {
            const svgBlocks = container.querySelectorAll('pre code.language-svg');
            svgBlocks.forEach(function (codeEl) {
                const raw = codeEl.textContent;
                const clean = DOMPurify.sanitize(raw, { USE_PROFILES: { svg: true, svgFilters: true } });
                const host = document.createElement('div');
                host.className = 'svg-rendered';
                host.innerHTML = clean;
                const pre = codeEl.closest('pre');
                if (pre && pre.parentNode) {
                    pre.parentNode.replaceChild(host, pre);
                }
            });
        }
    }

    renderMarkdown(content) {
        // Render Markdown to HTML
        if (typeof marked !== 'undefined') {
            try {
                return marked.parse(content);
            } catch (e) {
                if (uiLogger) uiLogger.error('Markdown parsing error:', e);
                return this.escapeHtml(content);
            }
        }
        // Fallback: escape HTML and preserve line breaks
        return this.escapeHtml(content).replace(/\n/g, '<br>');
    }

    /**
     * Lightweight text renderer for streaming display.
     * Only does HTML escaping and line-break preservation — no Markdown parsing.
     * This avoids blocking the main thread with expensive marked.parse() calls
     * on every single token, which causes the "freeze then all-at-once" effect.
     */
    renderStreamingText(content) {
        return this.escapeHtml(content).replace(/\n/g, '<br>');
    }

    /**
     * Update the assistant message's reasoning ("thinking") block.
     * Reasoning is rendered as plain pre-wrapped text (never as Markdown)
     * so it can never interfere with code-block detection in the answer.
     * The block is shown auto-expanded while streaming and auto-collapsed
     * once the answer streaming starts/finishes.
     */
    updateAssistantReasoning(messageId, reasoningText) {
        const wrapper = this.elements.messagesArea.querySelector(`[data-message-id="${messageId}"]`);
        if (!wrapper) return;
        let block = wrapper.querySelector('.reasoning-block');
        if (!block) {
            block = document.createElement('details');
            block.className = 'reasoning-block';
            block.open = true; // expanded while reasoning is streaming
            const summary = document.createElement('summary');
            summary.className = 'reasoning-summary';
            summary.textContent = '💭 思考中…';
            const body = document.createElement('div');
            body.className = 'reasoning-body';
            block.appendChild(summary);
            block.appendChild(body);
            // Insert reasoning block before message-content so it appears above
            const messageContent = wrapper.querySelector('.message-content');
            if (messageContent && messageContent.parentNode) {
                messageContent.parentNode.insertBefore(block, messageContent);
            } else {
                wrapper.querySelector('.vac-message-box')?.appendChild(block);
            }
        }
        const body = block.querySelector('.reasoning-body');
        if (body) {
            body.textContent = reasoningText; // plain text only, no HTML / no Markdown
            this.scrollToBottom();
        }
    }

    /**
     * Called once the assistant starts producing its real answer (or when
     * streaming finishes). Collapses the reasoning block and switches the
     * label from "thinking…" to a final "thought" caption.
     */
    finalizeAssistantReasoning(messageId) {
        const wrapper = this.elements.messagesArea.querySelector(`[data-message-id="${messageId}"]`);
        if (!wrapper) return;
        const block = wrapper.querySelector('.reasoning-block');
        if (!block) return;
        if (block.dataset.finalized === '1') return;
        block.dataset.finalized = '1';
        block.open = false;
        const summary = block.querySelector('.reasoning-summary');
        if (summary) summary.textContent = '💭 已思考（点击展开）';
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    getEditableText(element) {
        // Extract plain text from contenteditable div, preserving line breaks
        let text = '';
        const childNodes = element.childNodes;
        for (let i = 0; i < childNodes.length; i++) {
            const node = childNodes[i];
            if (node.nodeType === Node.TEXT_NODE) {
                text += node.textContent;
            } else if (node.nodeName === 'BR') {
                text += '\n';
            } else if (node.nodeType === Node.ELEMENT_NODE) {
                // Block-level elements (div, p) add a newline before their content
                // unless it's the first child
                if (i > 0 && (node.nodeName === 'DIV' || node.nodeName === 'P')) {
                    text += '\n';
                }
                text += this.getEditableText(node);
            }
        }
        return text;
    }

    initializeEventListeners() {
        // Input content changes
        this.elements.editableContent.addEventListener('input', () => {
            this.updateSendButtonState();
        });

        // Send button click
        this.elements.sendBtn.addEventListener('click', () => {
            this.handleSendMessage();
        });

        // Enter key to send (Shift+Enter for new line)
        this.elements.editableContent.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                if (!this.elements.sendBtn.classList.contains('vac-send-disabled')) {
                    this.handleSendMessage();
                }
            }
        });

        // API type selector
        this.elements.apiTypeSelect.addEventListener('change', (e) => {
            const config = this.apiClient.getConfig();
            config.apiType = e.target.value;
            this.apiClient.saveConfig(config);
        });

        // Stop button
        this.elements.stopBtn.addEventListener('click', () => {
            this.handleStopGeneration();
        });

        // Config button
        this.elements.configBtn.addEventListener('click', () => {
            this.showConfigModal();
        });

        // Clear history button
        const clearHistoryBtn = document.getElementById('clear-history-btn');
        if (clearHistoryBtn) {
            clearHistoryBtn.addEventListener('click', () => {
                this.handleClearHistory();
            });
        }

        // Config modal buttons
        this.elements.saveBtn.addEventListener('click', () => {
            this.saveConfiguration();
        });

        this.elements.cancelBtn.addEventListener('click', () => {
            this.hideConfigModal();
        });

        // Config API type change
        this.elements.configApiType.addEventListener('change', (e) => {
            this.updateModelOptions(e.target.value);
            this.updateAutoMetaDSLFields(e.target.value);
        });

        // Auth mode change
        this.elements.authModeSelect.addEventListener('change', (e) => {
            this.updateUsernameFieldVisibility(e.target.value);
        });

        // Close modal on background click
        this.elements.configModal.addEventListener('click', (e) => {
            if (e.target === this.elements.configModal) {
                this.hideConfigModal();
            }
        });

        // Image URL rows: add first empty row and bind buttons
        this.addImageUrlRow();
        if (this.elements.addImageUrlBtn) {
            this.elements.addImageUrlBtn.addEventListener('click', () => {
                this.addImageUrlRow();
            });
        }
        if (this.elements.clearImageUrlsBtn) {
            this.elements.clearImageUrlsBtn.addEventListener('click', () => {
                this.clearImageUrls();
            });
        }

        // Load current API type
        const config = this.apiClient.getConfig();
        this.elements.apiTypeSelect.value = config.apiType;
    }

    addImageUrlRow(initialValue = '') {
        if (!this.elements.imageUrlsList) return;
        const row = document.createElement('div');
        row.className = 'image-url-row';
        const input = document.createElement('input');
        input.type = 'text';
        input.placeholder = 'https://... or data:image/png;base64,...';
        input.value = initialValue;
        const removeBtn = document.createElement('button');
        removeBtn.type = 'button';
        removeBtn.className = 'remove-btn';
        removeBtn.textContent = '\u00D7';
        removeBtn.title = 'Remove this URL';
        removeBtn.addEventListener('click', () => {
            const list = this.elements.imageUrlsList;
            if (list.children.length > 1) {
                row.remove();
            } else {
                input.value = '';
            }
        });
        row.appendChild(input);
        row.appendChild(removeBtn);
        this.elements.imageUrlsList.appendChild(row);
    }

    getImageUrls() {
        if (!this.elements.imageUrlsList) return [];
        const inputs = this.elements.imageUrlsList.querySelectorAll('input');
        const urls = [];
        inputs.forEach((el) => {
            const v = (el.value || '').trim();
            if (v) urls.push(v);
        });
        return urls;
    }

    clearImageUrls() {
        if (!this.elements.imageUrlsList) return;
        this.elements.imageUrlsList.innerHTML = '';
        this.addImageUrlRow();
    }

    updateSendButtonState() {
        const content = this.elements.editableContent.textContent.trim();
        if (content && !this.isProcessing) {
            this.elements.sendBtn.classList.remove('vac-send-disabled');
        } else {
            this.elements.sendBtn.classList.add('vac-send-disabled');
        }
    }

    handleStopGeneration() {
        if (!this.isProcessing || this.elements.stopBtn.classList.contains('stop-disabled')) {
            return;
        }
        if (uiLogger) uiLogger.info('Stop button clicked, aborting request');
        this.apiClient.abort();
    }

    async handleSendMessage() {
        if (this.isProcessing) {
            return;
        }

        const content = this.getEditableText(this.elements.editableContent).trim();
        if (!content) {
            return;
        }

        // Check if API is configured
        const config = this.apiClient.getConfig();
        const requiresKey = config.apiType !== 'auto_metadsl'
            && config.apiType !== 'local_openai'
            && config.apiType !== 'ollama';
        if (requiresKey && !config.apiKey) {
            this.showError('Please configure your API key first');
            this.showConfigModal();
            return;
        }

        this.isProcessing = true;
        this.updateSendButtonState();
        this.elements.stopBtn.classList.remove('stop-disabled');

        // Add user message
        this.messageHandler.addMessage('user', content);
        this.displayMessage('user', content);

        // Clear input
        this.elements.editableContent.textContent = '';
        this.updateSendButtonState();

        // Show loading indicator
        this.showLoading();

        let fullResponse = '';
        let assistantMessageId = null;

        try {
            // Get API type for context optimization
            const config = this.apiClient.getConfig();

            // Get conversation context (optimized for auto_metadsl)
            const messages = this.messageHandler.getConversationContext(undefined, config.apiType);

            // Create assistant message placeholder
            assistantMessageId = this.createAssistantMessagePlaceholder();

            // Send to API with streaming
            fullResponse = '';
            let fullReasoning = '';
            const contextRounds = this.messageHandler.getContextConfig().contextRounds;
            const imageUrls = this.getImageUrls();
            await this.apiClient.sendMessage(messages, (chunk, accumulated, kind) => {
                if (kind === 'reasoning') {
                    fullReasoning = accumulated;
                    this.updateAssistantReasoning(assistantMessageId, accumulated);
                } else {
                    fullResponse = accumulated;
                    this.updateAssistantMessage(assistantMessageId, accumulated);
                }
            }, contextRounds, imageUrls);

            // Make sure the reasoning block ends up collapsed even if no
            // answer text arrived (rare edge case).
            this.finalizeAssistantReasoning(assistantMessageId);

            // Finalize: full Markdown render + code blocks after streaming is done
            this.finalizeAssistantMessage(assistantMessageId, fullResponse);

            // Save assistant response
            this.messageHandler.addMessage('assistant', fullResponse);

            // Add el-tag with timestamp for block ID stability (same structure as custom-llm)
            this.addMessageTag(assistantMessageId, new Date().toLocaleTimeString());

        } catch (error) {
            if (error.message === 'Request cancelled') {
                // User clicked Stop: save partial response if any
                if (uiLogger) uiLogger.info('Generation stopped by user, partial response length:', { length: fullResponse.length });
                this.finalizeAssistantReasoning(assistantMessageId);
                if (fullResponse) {
                    this.messageHandler.addMessage('assistant', fullResponse);
                    this.addMessageTag(assistantMessageId, new Date().toLocaleTimeString());
                    this.finalizeAssistantMessage(assistantMessageId, fullResponse);
                }

            } else {
                if (uiLogger) uiLogger.error('Error sending message:', error);
                this.showError(error.message);
                this.displayMessage('assistant', `Error: ${error.message}`);
            }
        } finally {
            this.hideLoading();
            this.isProcessing = false;
            this.elements.stopBtn.classList.add('stop-disabled');
            this.updateSendButtonState();
        }
    }

    displayMessage(role, content) {
        // Check if we need to remove old messages to maintain display limit
        const displayLimit = this.messageHandler.getContextConfig().contextRounds * 2;
        const currentVisibleMessages = this.elements.messagesArea.querySelectorAll('.vac-message-wrapper').length;

        // Remove oldest message if we exceed the display limit
        if (currentVisibleMessages >= displayLimit) {
            const oldestMessage = this.elements.messagesArea.querySelector('.vac-message-wrapper');
            if (oldestMessage) {
                oldestMessage.remove();
                if (uiLogger) uiLogger.debug('Removed oldest message from display (limit: ' + displayLimit + ')');
            }
        }

        const wrapper = document.createElement('div');
        wrapper.className = 'vac-message-wrapper';
        if (role === 'user') {
            wrapper.classList.add('user');
        }

        const box = document.createElement('div');
        box.className = 'vac-message-box';
        if (role === 'user') {
            box.classList.add('vac-offset-current');
        }

        const messageContent = document.createElement('div');
        messageContent.className = 'message-content';
        // Render Markdown for assistant messages, plain text for user messages
        if (role === 'assistant') {
            messageContent.innerHTML = this.renderMarkdown(content);
        } else {
            messageContent.classList.add('user-message');
            messageContent.textContent = content;
        }

        const messageTime = document.createElement('div');
        messageTime.className = 'message-time';
        messageTime.textContent = new Date().toLocaleTimeString();

        box.appendChild(messageContent);
        box.appendChild(messageTime);
        wrapper.appendChild(box);

        this.elements.messagesArea.appendChild(wrapper);
        this.scrollToBottom();

        if (role === 'assistant') {
            try {
                this.processCodeBlocks(messageContent);
            } catch (e) {
                if (uiLogger) uiLogger.error('Finalize codeblocks error (displayMessage):', e);
            }
        }

        return wrapper;
    }

    createAssistantMessagePlaceholder() {
    const wrapper = document.createElement('div');
    wrapper.className = 'vac-message-wrapper';
    wrapper.dataset.messageId = 'assistant-' + Date.now();

    const box = document.createElement('div');
    box.className = 'vac-message-box';

    const messageContent = document.createElement('div');
    messageContent.className = 'message-content';
    messageContent.textContent = '';

    const messageTime = document.createElement('div');
    messageTime.className = 'message-time';
    messageTime.textContent = new Date().toLocaleTimeString();

    box.appendChild(messageContent);
    box.appendChild(messageTime);
    wrapper.appendChild(box);

    this.elements.messagesArea.appendChild(wrapper);
    this.scrollToBottom();

    return wrapper.dataset.messageId;
}

updateAssistantMessage(messageId, content) {
    const wrapper = this.elements.messagesArea.querySelector(`[data-message-id="${messageId}"]`);
    if (wrapper) {
        const messageContent = wrapper.querySelector('.message-content');
        if (messageContent) {
            // Use lightweight text rendering during streaming to avoid
            // expensive Markdown parsing on every single token chunk
            messageContent.innerHTML = this.renderStreamingText(content);
            this.scrollToBottom();
        }
        // First real answer chunk arrived — collapse the thinking block
        this.finalizeAssistantReasoning(messageId);
    }
}

/**
 * Finalize an assistant message after streaming completes.
 * Re-renders with full Markdown parsing and processes code blocks.
 *
 * IMPORTANT: rawText must be the original streamed text. We cannot recover
 * it from DOM.textContent because the streaming renderer converted "\n"
 * into "<br>" tags, and textContent does not turn <br> back into "\n".
 */
finalizeAssistantMessage(messageId, rawText) {
    if (!messageId) return;
    const wrapper = this.elements.messagesArea.querySelector(`[data-message-id="${messageId}"]`);
    if (!wrapper) return;
    const messageContent = wrapper.querySelector('.message-content');
    if (!messageContent) return;

    // Re-render with full Markdown now that streaming is done.
    // Fall back to empty string when rawText is missing (e.g. cancelled
    // before any token arrived) so we don't feed undefined to marked.
    const text = (typeof rawText === 'string') ? rawText : '';
    messageContent.innerHTML = this.renderMarkdown(text);
    this.scrollToBottom();

    // Process code blocks (mermaid, svg, etc.)
    try {
        this.processCodeBlocks(messageContent);
    } catch (e) {
        if (uiLogger) uiLogger.error('Finalize codeblocks error:', e);
    }
}

addMessageTag(messageId, timeText) {
    // Add el-tag element to assistant message for block ID stability
    const wrapper = this.elements.messagesArea.querySelector(`[data-message-id="${messageId}"]`);
    if (wrapper) {
        this.addMessageTagToWrapper(wrapper, timeText);
    }
}

addMessageTagToWrapper(wrapper, timeText) {
    // Add el-tag element (same structure as custom-llm) for block ID computation
    const box = wrapper.querySelector('.vac-message-box');
    if (box && !box.querySelector('.el-tag')) {
        const tag = document.createElement('span');
        tag.className = 'el-tag';
        const tagContent = document.createElement('span');
        tagContent.className = 'el-tag__content';
        tagContent.textContent = timeText;
        tag.appendChild(tagContent);
        tag.style.display = 'none'; // Hidden, only used for block ID
        box.appendChild(tag);
    }
}

showLoading() {
    this.elements.loadingIndicator.classList.add('active');
}

hideLoading() {
    this.elements.loadingIndicator.classList.remove('active');
}

scrollToBottom() {
    this.elements.messagesArea.scrollTop = this.elements.messagesArea.scrollHeight;
}

loadExistingMessages() {
    const messages = this.messageHandler.messages;
    // Only display the last N rounds on the page
    // Older messages are still stored in localStorage but not displayed
    const displayLimit = this.messageHandler.getContextConfig().contextRounds * 2;
    const messagesToDisplay = messages.slice(-displayLimit);

    if (messagesToDisplay.length < messages.length) {
        if (uiLogger) uiLogger.info('Displaying last ' + messagesToDisplay.length + ' of ' + messages.length + ' messages');
    }

    messagesToDisplay.forEach(msg => {
        const wrapper = this.displayMessage(msg.role, msg.content);
        // Add el-tag for assistant messages (history messages are already complete)
        if (msg.role === 'assistant' && wrapper) {
            const time = msg.timestamp ? new Date(msg.timestamp).toLocaleTimeString() : '';
            this.addMessageTagToWrapper(wrapper, time);
        }
    });
}

showConfigModal() {
    const config = this.apiClient.getConfig();
    this.elements.configApiType.value = config.apiType;
    this.elements.apiKeyInput.value = config.apiKey || '';
    this.elements.authModeSelect.value = config.authMode || 'personal';
    this.elements.usernameInput.value = config.username || '';
        this.elements.streamCheckbox.checked = !!config.stream;
        this.elements.enableWebSearchCheckbox.checked = !!config.enableWebSearch;
        this.elements.enableThinkingCheckbox.checked = !!config.enableThinking;
        this.elements.reasoningEffortSelect.value = config.reasoningEffort || '';
        this.elements.apiEndpointInput.value = config.apiEndpoint || '';

    // Load context configuration
    const contextConfig = this.messageHandler.getContextConfig();
    this.elements.contextRoundsInput.value = contextConfig.contextRounds;
    this.elements.maxContextCharsInput.value = contextConfig.maxContextChars;
    this.elements.maxHistoryMessagesInput.value = contextConfig.maxHistoryMessages;

    this.updateModelOptions(config.apiType);
    this.updateAutoMetaDSLFields(config.apiType);
    this.updateUsernameFieldVisibility(config.authMode || 'personal');
    // Apply saved model only when a select is active; for free-text the
    // value was already set inside updateModelOptions.
    if (this._modelInputMode !== 'text') {
        this.elements.modelSelect.value = config.model || '';
    }

    this.elements.configModal.classList.add('active');
    this.hideError();
}

hideConfigModal() {
    this.elements.configModal.classList.remove('active');
}

handleClearHistory() {
    // Confirm before clearing
    if (confirm('Are you sure you want to clear all conversation history? This action cannot be undone.')) {
        // Clear history from MessageHandler
        this.messageHandler.clearHistory();

        // Reset API client conversation state
        this.apiClient.resetConversation();

        // Clear displayed messages
        this.elements.messagesArea.innerHTML = '';

        if (uiLogger) uiLogger.info('Conversation history cleared');
    }
}

    updateModelOptions(apiType) {
        const models = this.apiClient.getAvailableModels(apiType);
        const config = this.apiClient.getConfig();
        // Use a free-text input when no predefined model list is available
        // (e.g. local_openai where users name models themselves).
        const useFreeText = (models.length === 0);
        // Track current mode on the controller so other methods (save/read)
        // don't have to inspect DOM inline styles.
        this._modelInputMode = useFreeText ? 'text' : 'select';

        if (useFreeText) {
            this.elements.modelSelect.style.display = 'none';
            this.elements.modelTextInput.style.display = 'block';
            this.elements.modelTextInput.value = config.model || '';
            return;
        }

        // Predefined list: use the select control
        this.elements.modelSelect.style.display = 'block';
        this.elements.modelTextInput.style.display = 'none';
        this.elements.modelSelect.innerHTML = '';

        models.forEach(model => {
            const option = document.createElement('option');
            option.value = model.value;
            option.textContent = model.label;
            this.elements.modelSelect.appendChild(option);
        });

        // Set default model
        if (config.model && models.find(m => m.value === config.model)) {
            this.elements.modelSelect.value = config.model;
        } else if (models.length > 0) {
            this.elements.modelSelect.value = models[0].value;
        }
    }

updateAutoMetaDSLFields(apiType) {
    // Show auth mode and username fields only for auto_metadsl
    if (apiType === 'auto_metadsl') {
        this.elements.authModeGroup.style.display = 'block';
        this.elements.usernameGroup.style.display = 'block';
        this.elements.streamGroup.style.display = 'block';
        this.elements.webSearchGroup.style.display = 'block';
        this.elements.thinkingGroup.style.display = 'block';
        this.elements.reasoningEffortGroup.style.display = 'block';
    } else {
        this.elements.authModeGroup.style.display = 'none';
        this.elements.usernameGroup.style.display = 'none';
        this.elements.streamGroup.style.display = 'none';
        this.elements.webSearchGroup.style.display = 'none';
        this.elements.thinkingGroup.style.display = 'none';
        this.elements.reasoningEffortGroup.style.display = 'none';
    }
}

updateUsernameFieldVisibility(authMode) {
    // Update username field label and requirement based on auth mode
    const usernameLabel = this.elements.usernameGroup.querySelector('label');
    const usernameHint = this.elements.usernameGroup.querySelector('small');

    if (authMode === 'agent') {
        usernameLabel.textContent = 'Username (Required):';
        usernameHint.textContent = 'Required for agent token mode.';
    } else {
        usernameLabel.textContent = 'Username (Optional):';
        usernameHint.textContent = 'Optional for personal token mode.';
    }
}

saveConfiguration() {
    const apiType = this.elements.configApiType.value;
    const apiKey = this.elements.apiKeyInput.value.trim();
    const authMode = this.elements.authModeSelect.value;
    const username = this.elements.usernameInput.value.trim();
    const apiEndpoint = this.elements.apiEndpointInput.value.trim();
    // Read model from whichever control is currently active
    const model = (this._modelInputMode === 'text')
        ? this.elements.modelTextInput.value.trim()
        : this.elements.modelSelect.value;
    const contextRounds = parseInt(this.elements.contextRoundsInput.value, 10);
    const maxContextChars = parseInt(this.elements.maxContextCharsInput.value, 10);
    const maxHistoryMessages = parseInt(this.elements.maxHistoryMessagesInput.value, 10);

    // API key is optional for auto_metadsl, local_openai and ollama
    if (apiType !== 'auto_metadsl' && apiType !== 'local_openai' && apiType !== 'ollama' && !apiKey) {
        this.showError('API key is required');
        return;
    }

    // local_openai/ollama endpoint is optional: defaults to http://localhost:11434
    // and the proper suffix is appended automatically when missing.

    // Username is required for agent token mode
    if (apiType === 'auto_metadsl' && authMode === 'agent' && !username) {
        this.showError('Username is required for agent token mode');
        return;
    }

    // Validate context configuration
    if (isNaN(contextRounds) || contextRounds < 1 || contextRounds > 50) {
        this.showError('Context rounds must be between 1 and 50');
        return;
    }

    if (isNaN(maxContextChars) || maxContextChars < 1024 || maxContextChars > 1048576) {
        this.showError('Max context characters must be between 1024 and 1048576');
        return;
    }

    if (isNaN(maxHistoryMessages) || maxHistoryMessages < 10 || maxHistoryMessages > 200) {
        this.showError('Max history messages must be between 10 and 200');
        return;
    }

    const stream = this.elements.streamCheckbox.checked;
    const enableWebSearch = this.elements.enableWebSearchCheckbox.checked;
    const enableThinking = this.elements.enableThinkingCheckbox.checked;
    const reasoningEffort = this.elements.reasoningEffortSelect.value;
    const config = {
        apiType: apiType,
        apiKey: apiKey,
        authMode: authMode,
        username: username,
        stream: stream,
        enableWebSearch: enableWebSearch,
        enableThinking: enableThinking,
        reasoningEffort: reasoningEffort,
        apiEndpoint: apiEndpoint,
        model: model
    };

    this.apiClient.saveConfig(config);
    this.elements.apiTypeSelect.value = apiType;

    // Save context configuration
    this.messageHandler.setContextConfig({
        contextRounds: contextRounds,
        maxContextChars: maxContextChars,
        maxHistoryMessages: maxHistoryMessages
    });

    if (uiLogger) uiLogger.info('Configuration saved:', {
        api: config,
        context: { contextRounds, maxContextChars, maxHistoryMessages }
    });

    this.hideConfigModal();
    this.showSuccess('Configuration saved successfully');
}

showError(message) {
    this.elements.errorMessage.textContent = message;
    this.elements.errorMessage.classList.add('active');
}

hideError() {
    this.elements.errorMessage.classList.remove('active');
}

showSuccess(message) {
    // Could implement a success toast notification here
    if (uiLogger) uiLogger.info('Success:', { message });
}
}

// Export for use in other modules
window.UIController = UIController;
