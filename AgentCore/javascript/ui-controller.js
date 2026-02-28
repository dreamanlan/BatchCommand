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
            streamCheckbox: document.getElementById('stream-enabled'),
            streamGroup: document.getElementById('stream-group'),
            contextRoundsInput: document.getElementById('context-rounds'),
            maxContextCharsInput: document.getElementById('max-context-chars'),
            saveBtn: document.getElementById('save-btn'),
            cancelBtn: document.getElementById('cancel-btn'),
            errorMessage: document.getElementById('error-message')
        };

        this.initializeEventListeners();
        this.loadExistingMessages();
        this.updateSendButtonState();
    }

    initializeMarkdown() {
        // Configure marked.js
        if (typeof marked !== 'undefined') {
            marked.setOptions({
                highlight: function(code, lang) {
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

        // Load current API type
        const config = this.apiClient.getConfig();
        this.elements.apiTypeSelect.value = config.apiType;
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
        if (config.apiType !== 'auto_metadsl' && !config.apiKey) {
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

        try {
            // Get conversation context (auto context is built-in)
            const messages = this.messageHandler.getConversationContext();

            // Create assistant message placeholder
            const assistantMessageId = this.createAssistantMessagePlaceholder();

            // Send to API with streaming
            fullResponse = '';
            const contextRounds = this.messageHandler.getContextConfig().contextRounds;
            await this.apiClient.sendMessage(messages, (chunk, accumulated) => {
                fullResponse = accumulated;
                this.updateAssistantMessage(assistantMessageId, accumulated);
            }, contextRounds);

            // Save assistant response
            this.messageHandler.addMessage('assistant', fullResponse);

        } catch (error) {
            if (error.message === 'Request cancelled') {
                // User clicked Stop: save partial response if any
                if (uiLogger) uiLogger.info('Generation stopped by user, partial response length:', { length: fullResponse.length });
                if (fullResponse) {
                    this.messageHandler.addMessage('assistant', fullResponse);
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
                // Render Markdown for assistant messages
                messageContent.innerHTML = this.renderMarkdown(content);
                this.scrollToBottom();
            }
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
        messages.forEach(msg => {
            this.displayMessage(msg.role, msg.content);
        });
    }

    showConfigModal() {
        const config = this.apiClient.getConfig();
        this.elements.configApiType.value = config.apiType;
        this.elements.apiKeyInput.value = config.apiKey || '';
        this.elements.authModeSelect.value = config.authMode || 'personal';
        this.elements.usernameInput.value = config.username || '';
        this.elements.streamCheckbox.checked = !!config.stream;
        this.elements.apiEndpointInput.value = config.apiEndpoint || '';

        // Load context configuration
        const contextConfig = this.messageHandler.getContextConfig();
        this.elements.contextRoundsInput.value = contextConfig.contextRounds;
        this.elements.maxContextCharsInput.value = contextConfig.maxContextChars;

        this.updateModelOptions(config.apiType);
        this.updateAutoMetaDSLFields(config.apiType);
        this.updateUsernameFieldVisibility(config.authMode || 'personal');
        this.elements.modelSelect.value = config.model || '';

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
        this.elements.modelSelect.innerHTML = '';

        models.forEach(model => {
            const option = document.createElement('option');
            option.value = model.value;
            option.textContent = model.label;
            this.elements.modelSelect.appendChild(option);
        });

        // Set default model
        const config = this.apiClient.getConfig();
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
        } else {
            this.elements.authModeGroup.style.display = 'none';
            this.elements.usernameGroup.style.display = 'none';
            this.elements.streamGroup.style.display = 'none';
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
        const model = this.elements.modelSelect.value;
        const contextRounds = parseInt(this.elements.contextRoundsInput.value, 10);
        const maxContextChars = parseInt(this.elements.maxContextCharsInput.value, 10);

        // API key is optional for auto_metadsl
        if (apiType !== 'auto_metadsl' && !apiKey) {
            this.showError('API key is required');
            return;
        }

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

        const stream = this.elements.streamCheckbox.checked;
        const config = {
            apiType: apiType,
            apiKey: apiKey,
            authMode: authMode,
            username: username,
            stream: stream,
            apiEndpoint: apiEndpoint,
            model: model
        };

        this.apiClient.saveConfig(config);
        this.elements.apiTypeSelect.value = apiType;

        // Save context configuration
        this.messageHandler.setContextConfig({
            contextRounds: contextRounds,
            maxContextChars: maxContextChars
        });

        if (uiLogger) uiLogger.info('Configuration saved:', {
            api: config,
            context: { contextRounds, maxContextChars }
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
