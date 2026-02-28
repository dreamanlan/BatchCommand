/**
 * Main Entry Point
 * Initializes the application
 */

// Create logger instance for this module
const mainLogger = window.logger ? window.logger.createLogger('Main') : null;

(function() {
    'use strict';

    // Wait for DOM to be ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    function init() {
        if (mainLogger) mainLogger.info('Initializing...');

        // Create instances
        const apiClient = new APIClient();
        const messageHandler = new MessageHandler();
        const uiController = new UIController(messageHandler, apiClient);

        // Expose API for inject.js PageAdapter compatibility
        window.AgentLLM = {
            apiClient: apiClient,
            messageHandler: messageHandler,
            uiController: uiController,

            // PageAdapter compatible methods
            sendMessage: function(text) {
                const editableContent = document.getElementById('editable-content');
                if (editableContent) {
                    editableContent.textContent = text;
                    // Trigger send
                    setTimeout(() => {
                        uiController.handleSendMessage();
                    }, 100);
                }
            },

            getLastResponse: function() {
                const lastMsg = messageHandler.getLastMessage();
                return lastMsg && lastMsg.role === 'assistant' ? lastMsg.content : null;
            },

            getHistory: function(count) {
                const messages = messageHandler.getConversationContext(count || 10);
                return messages.map(m => `${m.role}: ${m.content}`).join('\n\n');
            },

            isLastMessageFromLLM: function() {
                return messageHandler.isLastMessageFromAssistant();
            },

            clearHistory: function() {
                messageHandler.clearHistory();
                apiClient.resetConversation();
                const messagesArea = document.getElementById('messages-area');
                if (messagesArea) {
                    messagesArea.innerHTML = '';
                }
            },

            // Context configuration methods
            setContextConfig: function(config) {
                messageHandler.setContextConfig(config);
            },

            getContextConfig: function() {
                return messageHandler.getContextConfig();
            },

            // System prompt management methods
            setDialogPrompt: function(prompt) {
                messageHandler.setDialogPrompt(prompt);
                apiClient.autoMetaDSLRoundCount = 0; // Reset so next message injects system context
                if (mainLogger) mainLogger.info('Set dialog prompt via window API');
            },

            getDialogPrompt: function() {
                return messageHandler.getDialogPrompt();
            },

            clearDialogPrompt: function() {
                messageHandler.clearDialogPrompt();
                if (mainLogger) mainLogger.info('Cleared dialog prompt via window API');
            }
        };

        // Also expose setDialogPrompt directly on window for inject.js compatibility
        window.setDialogPrompt = function(prompt) {
            messageHandler.setDialogPrompt(prompt);
            apiClient.autoMetaDSLRoundCount = 0; // Reset so next message injects system context
            if (mainLogger) mainLogger.info('Set dialog prompt via window.setDialogPrompt');
        };

        if (mainLogger) mainLogger.info('Initialization complete');
        if (mainLogger) mainLogger.info('Page type should be detected as: custom-llm');
        if (mainLogger) mainLogger.info('API available at: window.AgentLLM');
    }
})();
