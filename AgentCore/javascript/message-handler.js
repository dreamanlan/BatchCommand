/**
 * Message Handler Module
 * Manages conversation history and message formatting
 */

// Create logger instance for this module
const msgLogger = window.logger ? window.logger.createLogger('MessageHandler') : null;

class MessageHandler {
    constructor() {
        this.messages = [];
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

    getConversationContext(maxMessages) {
        // Default to contextRounds * 2 (each round = user + assistant pair)
        const limit = maxMessages || this.config.contextRounds * 2;
        const maxChars = this.config.maxContextChars;

        // Get last N messages and clean content for context
        // Skip cleaning for the last message only if it is a user message (current request)
        const sliced = this.messages.slice(-limit);
        const lastIdx = sliced.length - 1;
        const skipLastClean = lastIdx >= 0 && sliced[lastIdx].role === 'user';
        const messages = sliced.map((m, idx) => ({
            role: m.role,
            content: (skipLastClean && idx === lastIdx) ? m.content : this.cleanContentForContext(m.role, m.content)
        }));

        // Truncate if total chars exceed maxContextChars
        let totalChars = 0;
        let startIdx = 0;
        for (let i = messages.length - 1; i >= 0; i--) {
            totalChars += messages[i].content.length;
            if (totalChars > maxChars) {
                startIdx = i + 1;
                break;
            }
        }
        const trimmedMessages = messages.slice(startIdx);

        // Build context array starting with system prompt if available
        const contextMessages = [];

        // Add dialog prompt as first message if set
        if (this.dialogPrompt) {
            contextMessages.push({
                role: 'system',
                content: this.dialogPrompt
            });
        }

        // Add conversation messages
        contextMessages.push(...trimmedMessages);

        return contextMessages;
    }

    cleanContentForContext(role, content) {
        if (!content) return '';

        if (role === 'user') {
            // Replace agent reply content with placeholder
            const agentReplyMarker = '\u3010Agent\u56de\u590d\u3011'; // Agent reply marker
            if (content.startsWith(agentReplyMarker)) {
                return '...';
            }
        }

        if (role === 'assistant') {
            // Remove MetaDSL code blocks (markdown fenced blocks starting with // @execute or # @execute)
            content = content.replace(/```[^\n]*\n\s*(\/\/ @execute|# @execute)[\s\S]*?```/g, '');
            // Clean up excessive blank lines left after removal
            content = content.replace(/\n{3,}/g, '\n\n').trim();
        }

        return content;
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
