/**
 * Message Handler Module
 * Manages conversation history and message formatting
 */

// Create logger instance for this module
const msgLogger = window.logger ? window.logger.createLogger('MessageHandler') : null;

class MessageHandler {
    constructor() {
        this.messages = [];
        this.hiddenContext = []; // Hidden context that won't be displayed in UI
        this.dialogPrompt = ''; // Dialog prompt that will be sent as first message
        this.config = {
            contextRounds: 12, // Number of conversation rounds to include in auto context
            maxContextChars: 128 * 1024 // Maximum characters for auto context (128KB)
        };
        this.loadHistory();
    }

    loadHistory() {
        const stored = localStorage.getItem('llm_conversation_history');
        if (stored) {
            try {
                this.messages = JSON.parse(stored);
            } catch (e) {
                if (msgLogger) msgLogger.error('Failed to parse conversation history:', e);
                this.messages = [];
            }
        }
    }

    saveHistory() {
        try {
            // Keep only last 50 messages to avoid localStorage limits
            const toSave = this.messages.slice(-50);
            localStorage.setItem('llm_conversation_history', JSON.stringify(toSave));
        } catch (e) {
            if (msgLogger) msgLogger.error('Failed to save conversation history:', e);
        }
    }

    addMessage(role, content) {
        const message = {
            role: role,
            content: content,
            timestamp: Date.now()
        };
        this.messages.push(message);
        this.saveHistory();
        return message;
    }

    getMessages() {
        return this.messages.map(m => ({
            role: m.role,
            content: m.content
        }));
    }

    getLastMessage() {
        return this.messages[this.messages.length - 1] || null;
    }

    isLastMessageFromAssistant() {
        const last = this.getLastMessage();
        return last && last.role === 'assistant';
    }

    clearHistory() {
        this.messages = [];
        localStorage.removeItem('llm_conversation_history');
    }

    formatMessageForDisplay(message) {
        const time = new Date(message.timestamp).toLocaleTimeString();
        return {
            role: message.role,
            content: message.content,
            time: time
        };
    }

    getConversationContext(maxMessages = 20) {
        // Get last N messages for API context
        const messages = this.messages.slice(-maxMessages).map(m => ({
            role: m.role,
            content: m.content
        }));

        // Build context array starting with system prompt if available
        const contextMessages = [];

        // Add dialog prompt as first message if set
        if (this.dialogPrompt) {
            contextMessages.push({
                role: 'system',
                content: this.dialogPrompt
            });
        }

        // Add other hidden context as system messages if any
        if (this.hiddenContext.length > 0) {
            const hiddenContextMessages = this.hiddenContext.map(ctx => ({
                role: 'system',
                content: ctx
            }));
            contextMessages.push(...hiddenContextMessages);
        }

        // Add conversation messages
        contextMessages.push(...messages);

        return contextMessages;
    }

    addHiddenContext(context) {
        if (context && typeof context === 'string') {
            this.hiddenContext.push(context);
            if (msgLogger) msgLogger.info('Added hidden context, total:', { count: this.hiddenContext.length });
        }
    }

    setHiddenContext(context) {
        if (Array.isArray(context)) {
            this.hiddenContext = context.filter(c => typeof c === 'string');
        } else if (typeof context === 'string') {
            this.hiddenContext = [context];
        } else {
            this.hiddenContext = [];
        }
        if (msgLogger) msgLogger.info('Set hidden context, total:', { count: this.hiddenContext.length });
    }

    clearHiddenContext() {
        this.hiddenContext = [];
        if (msgLogger) msgLogger.info('Cleared hidden context');
    }

    getHiddenContext() {
        return [...this.hiddenContext];
    }

    setContextConfig(config) {
        if (config.contextRounds !== undefined) {
            this.config.contextRounds = config.contextRounds;
        }
        if (config.maxContextChars !== undefined) {
            this.config.maxContextChars = config.maxContextChars;
        }
        if (msgLogger) msgLogger.info('Updated context config:', this.config);
    }

    getContextConfig() {
        return { ...this.config };
    }

    getAutoContext() {
        // Get recent conversation history for auto context
        // First, get last N rounds (each round = user + assistant message pair)
        const rounds = this.config.contextRounds;
        const maxChars = this.config.maxContextChars;

        // Get last N*2 messages (N rounds)
        const recentMessages = this.messages.slice(-rounds * 2);

        if (recentMessages.length === 0) {
            return '';
        }

        // Format messages as context string
        let contextStr = recentMessages.map(m => {
            const role = m.role === 'user' ? 'User' : 'Assistant';
            return `${role}: ${m.content}`;
        }).join('\n\n');

        // If exceeds max chars, take the last part
        if (contextStr.length > maxChars) {
            contextStr = contextStr.substring(contextStr.length - maxChars);
            // Try to start from a complete message boundary
            const firstNewline = contextStr.indexOf('\n\n');
            if (firstNewline > 0 && firstNewline < 1000) {
                contextStr = contextStr.substring(firstNewline + 2);
            }
        }

        return contextStr;
    }

    setDialogPrompt(prompt) {
        this.dialogPrompt = prompt || '';
        if (msgLogger) msgLogger.info('Set dialog prompt:', { length: this.dialogPrompt.length });
    }

    getDialogPrompt() {
        return this.dialogPrompt;
    }

    clearDialogPrompt() {
        this.dialogPrompt = '';
        if (msgLogger) msgLogger.info('Cleared dialog prompt');
    }
}

// Export for use in other modules
window.MessageHandler = MessageHandler;
