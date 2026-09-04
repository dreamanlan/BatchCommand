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
            contextRounds: 6, // Number of conversation rounds to include in auto context
            maxContextChars: 128 * 1024, // Maximum characters for auto context (128KB)
            maxHistoryMessages: 64 // Maximum messages to keep in memory and localStorage
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
            const limit = this.config.maxHistoryMessages;
            const toSave = this.messages.slice(-limit);
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
        // Trim memory array to maxHistoryMessages
        const limit = this.config.maxHistoryMessages;
        if (this.messages.length > limit) {
            this.messages = this.messages.slice(-limit);
        }
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

    getConversationContext(maxMessages, apiType) {
        // Build context array starting with system prompt if available
        const contextMessages = [];

        // Add dialog prompt as first message if set
        if (this.dialogPrompt) {
            contextMessages.push({
                role: 'system',
                content: this.dialogPrompt
            });
        }

        // For auto_metadsl, manage full conversation context locally (no conversation_id on server)
        // Frontend controls all context to avoid MetaDSL execution result accumulation
        const limit = maxMessages || this.config.contextRounds * 2;

        // Get last N messages and clean content for context.
        // Preserve two entries uncleaned:
        //   1) the last message (typically the current user request)
        //   2) the most recent assistant message
        // The LLM needs to see the executing round's raw MetaDSL code and,
        // when applicable, its execution result echoed by the agent.
        const sliced = this.messages.slice(-limit);
        const lastIdx = sliced.length - 1;
        const skipLastClean = lastIdx >= 0 && sliced[lastIdx].role === 'user';
        let lastAssistantIdx = -1;
        for (let i = lastIdx; i >= 0; i--) {
            if (sliced[i].role === 'assistant') { lastAssistantIdx = i; break; }
        }
        const messages = sliced.map((m, idx) => {
            const preserve = (idx === lastIdx) || (idx === lastAssistantIdx);
            return {
                role: m.role,
                content: preserve ? m.content : this.cleanContentForContext(m.role, m.content)
            };
        });

        // Apply maxContextChars limit on history messages (exclude the last/current user message)
        // When total history chars exceed the limit, keep only the most recent messages that fit
        const maxChars = this.config.maxContextChars;
        if (maxChars > 0 && messages.length > 1) {
            // Separate current message (last) from history
            const hasCurrentMsg = skipLastClean;
            const historyEnd = hasCurrentMsg ? messages.length - 1 : messages.length;
            let totalChars = 0;
            let startFrom = 0;
            // Walk backwards through history to find how many messages fit within maxChars
            for (let i = historyEnd - 1; i >= 0; i--) {
                totalChars += messages[i].content.length;
                if (totalChars > maxChars) {
                    startFrom = i + 1;
                    break;
                }
            }
            if (startFrom > 0) {
                // Trim older history messages that exceed the char limit
                const trimmed = messages.slice(startFrom);
                if (msgLogger) msgLogger.info('Trimmed history by maxContextChars:', {
                    removed: startFrom,
                    remaining: trimmed.length,
                    maxChars: maxChars
                });
                messages.length = 0;
                messages.push(...trimmed);
            }
        }

        // Add conversation messages
        contextMessages.push(...messages);

        if (msgLogger) msgLogger.debug('Context built:', {
            messageCount: contextMessages.length,
            limit: limit
        });

        return contextMessages;
    }

    cleanContentForContext(role, content) {
        if (!content) return '';

        if (role === 'user') {
            // Replace MetaDSL execution results (agent reply) with placeholder to reduce context size
            const agentReplyMarker = '\u3010Agent\u56de\u590d\u3011'; // Agent reply marker
            if (content.startsWith(agentReplyMarker)) {
                return '[Agent reply omitted]';
            }
            return content;
        }

        if (role === 'assistant') {
            // Historical assistant messages: strip MetaDSL code bodies to
            // avoid stale code accumulating in context. The most recent
            // assistant message is preserved uncleaned by getConversationContext,
            // so the LLM still sees the currently executing round's code.
            const placeholder = '[...metadsl...]';
            return content
                .replace(/<metadsl\b[^>]*>[\s\S]*?<\/metadsl\s*>/gi, placeholder)
                .replace(/```[ \t]*metadsl\b[^\n]*\n[\s\S]*?```/gi, placeholder)
                // Bare fenced code block whose FIRST content line is the MetaDSL
                // execute marker (// @execute or # @execute). This is the form
                // foundation_prompt.txt teaches the LLM to use, so it dominates
                // in the wild. Backreference \1 lets the outer fence be 4+
                // backticks OR tildes when the body itself contains ``` (see
                // foundation_prompt.txt point 4: "different or more characters").
                // The character class [`~] combined with the backreference
                // guarantees the opening and closing fences use the same char.
                .replace(/([`~]{3,})[^\n]*\n[ \t]*(?:\/\/|#)[ \t]*@execute\b[\s\S]*?\1/gi, placeholder);
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
        if (config.maxHistoryMessages !== undefined) {
            this.config.maxHistoryMessages = config.maxHistoryMessages;
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
