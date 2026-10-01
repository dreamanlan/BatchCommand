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
            agentIdInput: document.getElementById('agent-id'),
            agentIdGroup: document.getElementById('agent-id-group'),
            modelSelect: document.getElementById('model'),
            configModelManage: document.getElementById('config-model-manage'),
            configModelStatus: document.getElementById('config-model-status'),
            configModelAddInput: document.getElementById('config-model-add-input'),
            configModelAddBtn: document.getElementById('config-model-add-btn'),
            configModelDetectBtn: document.getElementById('config-model-detect-btn'),
            streamCheckbox: document.getElementById('stream-enabled'),
            streamGroup: document.getElementById('stream-group'),
            enableWebSearchCheckbox: document.getElementById('enable-web-search'),
            webSearchGroup: document.getElementById('web-search-group'),
            enableThinkingCheckbox: document.getElementById('enable-thinking'),
            thinkingGroup: document.getElementById('thinking-group'),
            reasoningEffortSelect: document.getElementById('reasoning-effort'),
            reasoningEffortGroup: document.getElementById('reasoning-effort-group'),
            maxContextTokensSelect: document.getElementById('max-context-tokens'),
            maxContextTokensGroup: document.getElementById('max-context-tokens-group'),
            contextRoundsInput: document.getElementById('context-rounds'),
            maxContextCharsInput: document.getElementById('max-context-chars'),
            maxHistoryMessagesInput: document.getElementById('max-history-messages'),
            saveBtn: document.getElementById('save-btn'),
            cancelBtn: document.getElementById('cancel-btn'),
            errorMessage: document.getElementById('error-message'),
            imageUrlsList: document.getElementById('image-urls-list'),
            addImageUrlBtn: document.getElementById('add-image-url-btn'),
            clearImageUrlsBtn: document.getElementById('clear-image-urls-btn'),
            modelQuickBar: document.getElementById('model-quick-bar'),
            modelQuickBtn: document.getElementById('model-quick-btn'),
            modelQuickPanel: document.getElementById('model-quick-panel'),
            modelQuickListSection: document.getElementById('model-quick-list-section'),
            modelQuickList: document.getElementById('model-quick-list'),
            ollamaManageSection: document.getElementById('ollama-manage-section'),
            ollamaStatus: document.getElementById('ollama-status'),
            ollamaAddInput: document.getElementById('ollama-add-input'),
            ollamaAddBtn: document.getElementById('ollama-add-btn'),
            ollamaRefreshBtn: document.getElementById('ollama-refresh-btn'),
            modelFlyout: document.getElementById('model-flyout')
        };

        // Cached auto-detection state per local API type (ollama / local_openai)
        this._localState = {};
        this._flyoutHideTimer = null;
        this._flyoutModelValue = null;

        this.initializeEventListeners();
        this.loadExistingMessages();
        this.updateSendButtonState();
        this.updateModelQuickBar();
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

        // API type selector: only switch the type, the target type keeps
        // its own saved settings (loaded by the effective view)
        this.elements.apiTypeSelect.addEventListener('change', (e) => {
            this.apiClient.saveConfig({ apiType: e.target.value });
            this.closeModelQuickPanel();
            this.updateModelQuickBar();
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

        // Config API type change: reload the whole form from that type's
        // saved settings so each API type has independent configuration
        this.elements.configApiType.addEventListener('change', (e) => {
            this.updateConfigFormForType(e.target.value);
        });

        // Config modal: add a model to the user-configured list
        this.elements.configModelAddBtn.addEventListener('click', () => {
            this.handleConfigAddModel();
        });
        this.elements.configModelAddInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                this.handleConfigAddModel();
            }
        });
        // Config modal: auto-detect the model list of the selected type
        this.elements.configModelDetectBtn.addEventListener('click', () => {
            this.handleConfigAutoDetect();
        });

        // Model selection change (auto_metadsl model-dependent options)
        this.elements.modelSelect.addEventListener('change', (e) => {
            this.updateModelDependentOptions(e.target.value);
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

        // Quick model settings panel (chat area)
        this.elements.modelQuickBtn.addEventListener('click', () => {
            this.toggleModelQuickPanel();
        });

        // Close the quick panel when clicking outside of it
        document.addEventListener('click', (e) => {
            if (!this.elements.modelQuickPanel.classList.contains('active')) return;
            if (this.elements.modelQuickPanel.contains(e.target)) return;
            if (this.elements.modelQuickBtn.contains(e.target)) return;
            this.closeModelQuickPanel();
        });

        this.elements.ollamaAddBtn.addEventListener('click', () => {
            this.handleAddLocalModel(this.apiClient.getConfig().apiType);
        });

        this.elements.ollamaAddInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                this.handleAddLocalModel(this.apiClient.getConfig().apiType);
            }
        });

        this.elements.ollamaRefreshBtn.addEventListener('click', () => {
            this.renderLocalModelList(this.apiClient.getConfig().apiType, true);
        });

        // Keep the hover flyout open while the pointer is inside it
        this.elements.modelFlyout.addEventListener('mouseenter', () => {
            clearTimeout(this._flyoutHideTimer);
        });
        this.elements.modelFlyout.addEventListener('mouseleave', () => {
            this.scheduleHideModelFlyout();
        });
        // A click inside the flyout must never close it
        this.elements.modelFlyout.addEventListener('click', () => {
            clearTimeout(this._flyoutHideTimer);
        });

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

        } catch (error) {
            if (error.message === 'Request cancelled') {
                // User clicked Stop: save partial response if any
                if (uiLogger) uiLogger.info('Generation stopped by user, partial response length:', { length: fullResponse.length });
                this.finalizeAssistantReasoning(assistantMessageId);
                if (fullResponse) {
                    this.messageHandler.addMessage('assistant', fullResponse);
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
            this.displayMessage(msg.role, msg.content);
        });
    }

    showConfigModal() {
        const config = this.apiClient.getConfig();
        this.elements.configApiType.value = config.apiType;

        // Load the saved settings of the current API type into the form
        this.updateConfigFormForType(config.apiType);

        // Load context configuration
        const contextConfig = this.messageHandler.getContextConfig();
        this.elements.contextRoundsInput.value = contextConfig.contextRounds;
        this.elements.maxContextCharsInput.value = contextConfig.maxContextChars;
        this.elements.maxHistoryMessagesInput.value = contextConfig.maxHistoryMessages;

        this.elements.configModal.classList.add('active');
        this.hideError();
    }

    /**
     * Fill the config modal form with the saved settings of the given
     * API type (each type stores its settings independently).
     */
    updateConfigFormForType(apiType) {
        const t = this.apiClient.getTypeConfig(apiType);
        this.elements.apiKeyInput.value = t.apiKey || '';
        this.elements.authModeSelect.value = t.authMode || 'personal';
        this.elements.usernameInput.value = t.username || '';
        this.elements.streamCheckbox.checked = !!t.stream;
        this.elements.enableWebSearchCheckbox.checked = !!t.enableWebSearch;
        this.elements.apiEndpointInput.value = t.apiEndpoint || '';
        this.elements.agentIdInput.value = t.agentId || '';
        this.updateUsernameFieldVisibility(t.authMode || 'personal');
        this.updateModelOptions(apiType);
        // Model list manage area: only for types with runtime model lists
        const isRuntimeList = (apiType !== 'auto_metadsl');
        this.elements.configModelManage.style.display = isRuntimeList ? 'block' : 'none';
        if (isRuntimeList) {
            this.updateConfigModelStatus(apiType);
        }
        this.updateAutoMetaDSLFields(apiType);
        // Restore the model-dependent options of the type's saved model
        this.updateModelDependentOptions(this.elements.modelSelect.value);
    }

    /**
     * Update the model manage status hint in the config modal.
     */
    updateConfigModelStatus(apiType) {
        const state = this.ensureLocalState(apiType);
        let text;
        if (state.querying) {
            text = 'Querying model list...';
        } else if (state.error) {
            text = 'Auto-detect failed: ' + state.error;
        } else if (state.fetched) {
            text = 'Auto-detected ' + (state.models || []).length + ' model(s).';
        } else {
            text = 'Model list not detected yet. Click Auto Detect or add model names.';
        }
        this.elements.configModelStatus.textContent = text;
    }

    /**
     * Auto-detect the model list of the type selected in the config
     * modal, using the (possibly unsaved) endpoint / key form values.
     */
    async handleConfigAutoDetect() {
        const apiType = this.elements.configApiType.value;
        if (apiType === 'auto_metadsl') return;
        const endpoint = this.elements.apiEndpointInput.value.trim();
        const apiKey = this.elements.apiKeyInput.value.trim();
        this.elements.configModelStatus.textContent = 'Querying model list...';
        await this.autoDetectModels(apiType, endpoint, apiKey);
        // Keep the current dropdown selection when still available
        this.updateModelOptions(apiType, true);
        this.updateConfigModelStatus(apiType);
    }

    /**
     * Add a model name to the user-configured list of the type selected
     * in the config modal and select it in the dropdown.
     */
    handleConfigAddModel() {
        const apiType = this.elements.configApiType.value;
        if (apiType === 'auto_metadsl') return;
        const name = (this.elements.configModelAddInput.value || '').trim();
        if (!name) return;
        const list = this.apiClient.getUserModelList(apiType);
        if (!list.includes(name)) {
            list.push(name);
            this.apiClient.saveUserModelList(apiType, list);
        }
        this.elements.configModelAddInput.value = '';
        this.updateModelOptions(apiType, true);
        this.elements.modelSelect.value = name;
        this.updateConfigModelStatus(apiType);
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

    /**
     * Update the visibility and label of the quick model chip in the chat
     * footer. The chip is only shown for API types backed by a selectable
     * model list (openai / claude / auto_metadsl) or ollama (auto-detected
     * or user-configured list).
     */
    updateModelQuickBar() {
        const config = this.apiClient.getConfig();
        const supported = ['openai', 'claude', 'auto_metadsl', 'ollama', 'local_openai'].includes(config.apiType);
        this.elements.modelQuickBar.classList.toggle('visible', supported);
        if (!supported) {
            this.closeModelQuickPanel();
            return;
        }
        const models = this.apiClient.getAvailableModels(config.apiType);
        const current = models.find(m => m.value === config.model);
        let label = current ? current.label : (config.model || '(model not set)');
        if (config.apiType === 'auto_metadsl') {
            const parts = [];
            if (current && current.thinking && config.enableThinking) parts.push('Thinking');
            if (config.reasoningEffort) parts.push(config.reasoningEffort);
            if (config.maxContextTokens && config.maxContextTokens > 0) {
                parts.push(this.formatContextTokens(config.maxContextTokens));
            }
            if (parts.length > 0) label += ' \u00B7 ' + parts.join(' \u00B7 ');
        }
        this.elements.modelQuickBtn.textContent = label;
    }

    toggleModelQuickPanel() {
        const panel = this.elements.modelQuickPanel;
        if (panel.classList.contains('active')) {
            this.closeModelQuickPanel();
        } else {
            this.renderModelQuickPanel();
            panel.classList.add('active');
        }
    }

    closeModelQuickPanel() {
        this.elements.modelQuickPanel.classList.remove('active');
        this.hideModelFlyout();
    }

    renderModelQuickPanel() {
        // Always start from the model list without a hover flyout
        this.hideModelFlyout();
        const config = this.apiClient.getConfig();
        if (config.apiType === 'auto_metadsl') {
            // auto_metadsl uses the builtin list from api-client.js
            this.elements.ollamaManageSection.style.display = 'none';
            this.renderPredefinedModelList(config.apiType);
        } else {
            // All other types: auto-detected / user-configured model list
            this.renderLocalModelList(config.apiType);
        }
    }

    /**
     * Render the model list for auto_metadsl from the predefined list in
     * api-client.js. Clicking a row selects the model and closes the panel;
     * hovering a row (models with options) shows a flyout with that
     * model's options, like a menu.
     */
    renderPredefinedModelList(apiType) {
        const config = this.apiClient.getConfig();
        const models = this.apiClient.getAvailableModels(apiType);
        const title = this.elements.modelQuickListSection.querySelector('.model-quick-section-title');
        title.textContent = 'Model';
        const list = this.elements.modelQuickList;
        list.innerHTML = '';
        models.forEach(model => {
            const row = document.createElement('div');
            row.className = 'model-quick-item-row';
            const item = document.createElement('button');
            item.type = 'button';
            item.className = 'model-quick-item' + (model.value === config.model ? ' active' : '');
            item.textContent = model.label;
            item.addEventListener('click', () => {
                // Clicking a model in the list selects it and closes the panel
                this.apiClient.saveConfig({ model: model.value });
                this.updateModelQuickBar();
                this.closeModelQuickPanel();
            });
            row.appendChild(item);
            if (this.modelHasQuickOptions(apiType, model.value)) {
                row.addEventListener('mouseenter', () => {
                    this.showModelFlyout(model.value, row);
                });
                row.addEventListener('mouseleave', () => {
                    this.scheduleHideModelFlyout();
                });
            }
            list.appendChild(row);
        });
    }

    /**
     * Get (or create) the auto-detection state of a local API type.
     */
    ensureLocalState(apiType) {
        if (!this._localState[apiType]) {
            // models: null = never queried
            this._localState[apiType] = { models: null, fetched: false, error: '', querying: false };
        }
        return this._localState[apiType];
    }

    /**
     * Query the model list of an API type via its standard protocol
     * (/api/tags for ollama, /v1/models for the others). Optional
     * endpoint / apiKey let callers probe unsaved form values.
     */
    async fetchModelsForType(apiType, endpoint, apiKey) {
        if (apiType === 'ollama') return await this.apiClient.fetchOllamaModels(endpoint);
        if (apiType === 'local_openai') return await this.apiClient.fetchLocalOpenAIModels(endpoint);
        if (apiType === 'openai') return await this.apiClient.fetchOpenAIModels(endpoint, apiKey);
        if (apiType === 'claude') return await this.apiClient.fetchClaudeModels(endpoint, apiKey);
        return [];
    }

    /**
     * Shared auto-detection core: queries the model list of an API type
     * and stores the result (or error) in the per-type state cache used
     * by both the quick panel and the config modal.
     *
     * The detected list (plus the user-configured list) is the full model
     * list: when the selected model is no longer in it (e.g. the vendor
     * retired it), the selection automatically switches to the first
     * available model.
     */
    async autoDetectModels(apiType, endpoint, apiKey) {
        const state = this.ensureLocalState(apiType);
        state.models = [];
        state.error = '';
        state.querying = true;
        try {
            state.models = await this.fetchModelsForType(apiType, endpoint, apiKey);
            state.fetched = true;
        } catch (e) {
            if (uiLogger) uiLogger.warn('Model auto-detect failed:', e);
            state.error = (e && e.message) ? e.message : String(e);
        }
        state.querying = false;
        // Auto-switch when the selected model was retired
        if (state.fetched && state.models.length > 0) {
            const t = this.apiClient.getTypeConfig(apiType);
            const custom = this.apiClient.getUserModelList(apiType);
            const full = [];
            state.models.concat(custom).forEach(name => {
                if (name && !full.includes(name)) full.push(name);
            });
            if (t.model && full.length > 0 && !full.includes(t.model)) {
                if (uiLogger) uiLogger.info('Selected model no longer available, switching:', { from: t.model, to: full[0] });
                this.apiClient.setModelForType(apiType, full[0]);
            }
        }
    }

    /**
     * Render the runtime model list of an API type (openai / claude /
     * local_openai / ollama). Auto-detects models on first open (or when
     * forceFetch is true); on failure falls back to the user-configured
     * list (add / remove below the list).
     */
    async renderLocalModelList(apiType, forceFetch) {
        const state = this.ensureLocalState(apiType);
        if (forceFetch === true || !state.fetched) {
            this.elements.ollamaManageSection.style.display = 'block';
            this.elements.ollamaStatus.textContent = 'Querying model list...';
            this.renderLocalModelItems(apiType);
            await this.autoDetectModels(apiType);
            this.renderLocalModelItems(apiType);
            return;
        }
        this.renderLocalModelItems(apiType);
    }

    renderLocalModelItems(apiType) {
        const config = this.apiClient.getConfig();
        const state = this.ensureLocalState(apiType);
        const fetched = state.models || [];
        const custom = this.apiClient.getUserModelList(apiType);
        const title = this.elements.modelQuickListSection.querySelector('.model-quick-section-title');
        const titles = {
            ollama: 'Ollama Models',
            local_openai: 'Local Models',
            openai: 'OpenAI Models',
            claude: 'Claude Models'
        };
        title.textContent = titles[apiType] || 'Models';
        const list = this.elements.modelQuickList;
        list.innerHTML = '';

        const seen = [];
        const addItem = (name, removable) => {
            if (seen.includes(name)) return;
            seen.push(name);
            const row = document.createElement('div');
            row.className = 'model-quick-item-row';
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'model-quick-item' + (name === config.model ? ' active' : '');
            btn.textContent = name;
            btn.title = 'Use this model';
            btn.addEventListener('click', () => {
                // Clicking a model selects it and closes the panel
                this.apiClient.saveConfig({ model: name });
                this.updateModelQuickBar();
                this.closeModelQuickPanel();
            });
            row.appendChild(btn);
            if (removable) {
                const rm = document.createElement('button');
                rm.type = 'button';
                rm.className = 'model-item-remove';
                rm.textContent = '\u00D7';
                rm.title = 'Remove from custom list';
                rm.addEventListener('click', () => {
                    const updated = this.apiClient.getUserModelList(apiType).filter(m => m !== name);
                    this.apiClient.saveUserModelList(apiType, updated);
                    this.renderLocalModelItems(apiType);
                });
                row.appendChild(rm);
            }
            list.appendChild(row);
        };

        fetched.forEach(name => addItem(name, false));
        custom.forEach(name => addItem(name, !fetched.includes(name)));

        if (seen.length === 0) {
            const hint = document.createElement('div');
            hint.className = 'model-quick-hint';
            hint.textContent = 'No models yet. Click Auto Detect or add model names below.';
            list.appendChild(hint);
        }

        if (state.querying) {
            this.elements.ollamaStatus.textContent = 'Querying model list...';
        } else if (state.error) {
            this.elements.ollamaStatus.textContent =
                'Auto-detect failed: ' + state.error + ' Add model names manually below.';
        } else {
            this.elements.ollamaStatus.textContent =
                'Auto-detected ' + fetched.length + ' model(s).';
        }
    }

    /**
     * Add a model name to the custom list of an API type and select it
     * for use.
     */
    handleAddLocalModel(apiType) {
        const name = (this.elements.ollamaAddInput.value || '').trim();
        if (!name) return;
        const list = this.apiClient.getUserModelList(apiType);
        if (!list.includes(name)) {
            list.push(name);
            this.apiClient.saveUserModelList(apiType, list);
        }
        this.elements.ollamaAddInput.value = '';
        this.apiClient.saveConfig({ model: name });
        this.updateModelQuickBar();
        this.closeModelQuickPanel();
    }

    /**
     * Whether a model has quick options to configure (auto_metadsl models
     * with thinking support, reasoning efforts or context windows).
     */
    modelHasQuickOptions(apiType, modelValue) {
        if (apiType !== 'auto_metadsl') return false;
        const models = this.apiClient.getAvailableModels('auto_metadsl');
        const model = models.find(m => m.value === modelValue);
        if (!model) return false;
        return !!(model.thinking
            || (Array.isArray(model.reasoningEfforts) && model.reasoningEfforts.length > 0)
            || (Array.isArray(model.contextWindows) && model.contextWindows.length > 0));
    }

    /**
     * Show the hover flyout with the given model's options, vertically
     * aligned with the hovered row (fixed positioning relative to the
     * viewport so the panel's scrolling does not clip it).
     */
    showModelFlyout(modelValue, row) {
        clearTimeout(this._flyoutHideTimer);
        this.renderFlyoutContent(modelValue);
        const flyout = this.elements.modelFlyout;
        flyout.classList.add('active');
        const rowRect = row.getBoundingClientRect();
        const panelRect = this.elements.modelQuickPanel.getBoundingClientRect();
        const left = panelRect.right + 8;
        let top = rowRect.top;
        const maxTop = window.innerHeight - flyout.offsetHeight - 8;
        if (maxTop > 0) top = Math.min(Math.max(top, 8), maxTop);
        flyout.style.left = left + 'px';
        flyout.style.top = top + 'px';
    }

    scheduleHideModelFlyout() {
        clearTimeout(this._flyoutHideTimer);
        // Small delay so moving the pointer from row to flyout keeps it open
        this._flyoutHideTimer = setTimeout(() => this.hideModelFlyout(), 150);
    }

    hideModelFlyout() {
        clearTimeout(this._flyoutHideTimer);
        this.elements.modelFlyout.classList.remove('active');
    }

    /**
     * Build the flyout content for a model: title, Thinking toggle (when
     * supported), Context list and Effort list with check marks on the
     * active entries. Reads/writes that model's own saved settings.
     */
    renderFlyoutContent(modelValue) {
        // Remember which model the flyout currently shows (for in-place
        // check mark updates that must not change the selected model)
        this._flyoutModelValue = modelValue;
        const config = this.apiClient.getConfig();
        const ms = this.apiClient.getModelSettings(config.apiType, modelValue);
        const models = this.apiClient.getAvailableModels('auto_metadsl');
        const model = models.find(m => m.value === modelValue) || null;
        const flyout = this.elements.modelFlyout;
        flyout.innerHTML = '';

        const title = document.createElement('div');
        title.className = 'model-flyout-title';
        title.textContent = (model && model.label) || modelValue;
        flyout.appendChild(title);

        const supportsThinking = !!(model && model.thinking);
        const efforts = (model && Array.isArray(model.reasoningEfforts)) ? model.reasoningEfforts : [];
        const windows = (model && Array.isArray(model.contextWindows)) ? model.contextWindows : [];

        // Thinking toggle (only for models that support it)
        if (supportsThinking) {
            const label = document.createElement('label');
            label.className = 'model-quick-toggle';
            const cb = document.createElement('input');
            cb.type = 'checkbox';
            cb.checked = !!ms.enableThinking;
            cb.addEventListener('change', () => {
                this.apiClient.setModelSettings(config.apiType, modelValue, { enableThinking: cb.checked });
                this.updateModelQuickBar();
            });
            label.appendChild(cb);
            label.appendChild(document.createTextNode('Thinking'));
            flyout.appendChild(label);
        }

        // Context window: Default (do not send) + model-specific options
        if (windows.length > 0) {
            flyout.appendChild(this.createFlyoutSectionTitle('Context'));
            const list = document.createElement('div');
            list.className = 'model-flyout-list';
            const items = [{ value: '0', label: 'Default' }].concat(windows.map(w => ({
                value: String(w),
                label: this.formatContextTokens(w)
            })));
            const saved = String(ms.maxContextTokens || 0);
            items.forEach(item => {
                list.appendChild(this.createFlyoutItem(item.label, 'context', item.value, item.value === saved, () => {
                    this.apiClient.setModelSettings(config.apiType, modelValue, { maxContextTokens: parseInt(item.value, 10) || 0 });
                    this.updateModelQuickBar();
                    this.refreshFlyoutActiveStates();
                }));
            });
            flyout.appendChild(list);
        }

        // Reasoning effort: Default (not set) + model-specific options
        if (efforts.length > 0) {
            flyout.appendChild(this.createFlyoutSectionTitle('Effort'));
            const list = document.createElement('div');
            list.className = 'model-flyout-list';
            const items = [{ value: '', label: 'Default' }].concat(efforts.map(level => ({
                value: level,
                label: level
            })));
            const saved = ms.reasoningEffort || '';
            items.forEach(item => {
                list.appendChild(this.createFlyoutItem(item.label, 'effort', item.value, item.value === saved, () => {
                    this.apiClient.setModelSettings(config.apiType, modelValue, { reasoningEffort: item.value });
                    this.updateModelQuickBar();
                    this.refreshFlyoutActiveStates();
                }));
            });
            flyout.appendChild(list);
        }
    }

    /**
     * Update the check marks in the flyout after an option click without
     * rebuilding the DOM — rebuilding removes the element under the
     * pointer, which can disturb the hover state and close the flyout.
     */
    refreshFlyoutActiveStates() {
        if (!this._flyoutModelValue) return;
        const config = this.apiClient.getConfig();
        const ms = this.apiClient.getModelSettings(config.apiType, this._flyoutModelValue);
        this.elements.modelFlyout.querySelectorAll('.model-flyout-item').forEach(btn => {
            let active = false;
            if (btn.dataset.group === 'context') {
                active = btn.dataset.value === String(ms.maxContextTokens || 0);
            } else if (btn.dataset.group === 'effort') {
                active = btn.dataset.value === (ms.reasoningEffort || '');
            }
            btn.classList.toggle('active', active);
        });
    }

    createFlyoutSectionTitle(text) {
        const el = document.createElement('div');
        el.className = 'model-quick-section-title model-flyout-section-title';
        el.textContent = text;
        return el;
    }

    createFlyoutItem(text, group, value, active, onClick) {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'model-flyout-item' + (active ? ' active' : '');
        btn.dataset.group = group;
        btn.dataset.value = value;
        const label = document.createElement('span');
        label.textContent = text;
        const check = document.createElement('span');
        check.className = 'check';
        check.textContent = '\u2713';
        btn.appendChild(label);
        btn.appendChild(check);
        btn.addEventListener('click', onClick);
        return btn;
    }

    formatContextTokens(tokens) {
        if (tokens >= 1000000 && tokens % 1000000 === 0) return (tokens / 1000000) + 'M';
        if (tokens >= 1000 && tokens % 1000 === 0) return (tokens / 1000) + 'K';
        return String(tokens);
    }

    /**
     * Populate the config modal model dropdown. auto_metadsl uses the
     * builtin list; all other types use the merged list of auto-detected
     * and user-configured models. When preferCurrent is true the current
     * dropdown selection is kept if still present (e.g. after detecting
     * or adding a model); otherwise the type's saved model is selected.
     */
    updateModelOptions(apiType, preferCurrent) {
        const config = this.apiClient.getTypeConfig(apiType);
        const select = this.elements.modelSelect;
        const prevSelected = preferCurrent ? select.value : '';
        select.innerHTML = '';
        const values = [];

        if (apiType === 'auto_metadsl') {
            const models = this.apiClient.getAvailableModels('auto_metadsl');
            models.forEach(model => {
                const option = document.createElement('option');
                option.value = model.value;
                option.textContent = model.label;
                select.appendChild(option);
                values.push(model.value);
            });
        } else {
            // Full model list: auto-detected models + user-configured
            // models (nothing else - retired models disappear from here)
            const state = this.ensureLocalState(apiType);
            const detected = state.models || [];
            const custom = this.apiClient.getUserModelList(apiType);
            const merged = [];
            detected.concat(custom).forEach(name => {
                if (name && !merged.includes(name)) merged.push(name);
            });
            if (merged.length === 0) {
                const option = document.createElement('option');
                option.value = '';
                option.textContent = '(no models yet - detect or add below)';
                select.appendChild(option);
                return;
            }
            merged.forEach(name => {
                const option = document.createElement('option');
                option.value = name;
                option.textContent = name;
                select.appendChild(option);
                values.push(name);
            });
        }

        // Selection priority: current selection > saved model > first
        if (values.includes(prevSelected)) {
            select.value = prevSelected;
        } else if (values.includes(config.model)) {
            select.value = config.model;
        } else if (values.length > 0) {
            select.value = values[0];
        }
    }

    updateAutoMetaDSLFields(apiType) {
        // Show auth mode and username fields only for auto_metadsl
        if (apiType === 'auto_metadsl') {
            this.elements.authModeGroup.style.display = 'block';
            this.elements.usernameGroup.style.display = 'block';
            this.elements.agentIdGroup.style.display = 'block';
            this.elements.streamGroup.style.display = 'block';
            this.elements.webSearchGroup.style.display = 'block';
            this.elements.thinkingGroup.style.display = 'block';
            this.elements.reasoningEffortGroup.style.display = 'block';
            this.elements.maxContextTokensGroup.style.display = 'block';
        } else {
            this.elements.authModeGroup.style.display = 'none';
            this.elements.usernameGroup.style.display = 'none';
            this.elements.agentIdGroup.style.display = 'none';
            this.elements.streamGroup.style.display = 'none';
            this.elements.webSearchGroup.style.display = 'none';
            this.elements.thinkingGroup.style.display = 'none';
            this.elements.reasoningEffortGroup.style.display = 'none';
            this.elements.maxContextTokensGroup.style.display = 'none';
        }
    }

    // Update model-dependent fields (thinking toggle, reasoning effort options,
    // context window options) based on the selected auto_metadsl model.
    // Each model keeps its own saved settings.
    updateModelDependentOptions(modelValue) {
        const apiType = this.elements.configApiType.value;
        if (apiType !== 'auto_metadsl') return;
        const ms = this.apiClient.getModelSettings(apiType, modelValue);
        const models = this.apiClient.getAvailableModels('auto_metadsl');
        const model = models.find(m => m.value === modelValue) || null;

        // Thinking toggle: only models with thinking=true
        const supportsThinking = !!(model && model.thinking);
        this.elements.thinkingGroup.style.display = supportsThinking ? 'block' : 'none';
        if (supportsThinking) {
            this.elements.enableThinkingCheckbox.checked = !!ms.enableThinking;
        }

        // Reasoning effort dropdown: rebuild options from model.reasoningEfforts
        const efforts = (model && Array.isArray(model.reasoningEfforts)) ? model.reasoningEfforts : [];
        if (efforts.length > 0) {
            this.elements.reasoningEffortSelect.innerHTML = '';
            efforts.forEach(level => {
                const option = document.createElement('option');
                option.value = level;
                option.textContent = level;
                this.elements.reasoningEffortSelect.appendChild(option);
            });
            // Restore this model's saved effort when valid, otherwise pick first
            this.elements.reasoningEffortSelect.value =
                efforts.includes(ms.reasoningEffort) ? ms.reasoningEffort : efforts[0];
            this.elements.reasoningEffortGroup.style.display = 'block';
        } else {
            this.elements.reasoningEffortGroup.style.display = 'none';
        }

        // Context window dropdown: rebuild options from model.contextWindows
        const windows = (model && Array.isArray(model.contextWindows)) ? model.contextWindows : [];
        if (windows.length > 0) {
            this.elements.maxContextTokensSelect.innerHTML = '';
            const zeroOption = document.createElement('option');
            zeroOption.value = '0';
            zeroOption.textContent = '0 (do not send)';
            this.elements.maxContextTokensSelect.appendChild(zeroOption);
            windows.forEach(w => {
                const option = document.createElement('option');
                option.value = String(w);
                option.textContent = String(w);
                this.elements.maxContextTokensSelect.appendChild(option);
            });
            // Restore this model's saved value when present, otherwise default 0
            const saved = ms.maxContextTokens > 0 ? String(ms.maxContextTokens) : '0';
            this.elements.maxContextTokensSelect.value =
                windows.some(w => String(w) === saved) ? saved : '0';
            this.elements.maxContextTokensGroup.style.display = 'block';
        } else {
            this.elements.maxContextTokensGroup.style.display = 'none';
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
        const agentId = this.elements.agentIdInput ? this.elements.agentIdInput.value.trim() : '';
        // Read model from the dropdown (merged list or builtin list)
        const model = this.elements.modelSelect.value;
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

        // auto_metadsl requires either an endpoint or an agent id
        if (apiType === 'auto_metadsl' && !apiEndpoint && !agentId) {
            this.showError('For auto_metadsl, either API endpoint or agent id must be specified');
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
        const maxContextTokensRaw = parseInt(this.elements.maxContextTokensSelect.value, 10);
        const maxContextTokens = (isNaN(maxContextTokensRaw) || maxContextTokensRaw < 0) ? 0 : maxContextTokensRaw;
        const config = {
            apiType: apiType,
            apiKey: apiKey,
            authMode: authMode,
            username: username,
            stream: stream,
            enableWebSearch: enableWebSearch,
            enableThinking: enableThinking,
            reasoningEffort: reasoningEffort,
            maxContextTokens: maxContextTokens,
            apiEndpoint: apiEndpoint,
            agentId: agentId,
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
        this.updateModelQuickBar();
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
