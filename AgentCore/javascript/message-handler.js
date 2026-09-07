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
            // Historical assistant messages: collapse MetaDSL code bodies to
            // the first 30 lines (wrapped in a fenced code block) so the LLM
            // keeps enough context to stay effective across rounds, while
            // avoiding unbounded accumulation of stale code.
            // The most recent assistant message is preserved uncleaned by
            // getConversationContext, so the LLM always sees the currently
            // executing round's full code.
            const collapseBody = (body) => {
                const lines = body.replace(/\r\n?/g, '\n').split('\n');
                if (lines.length && lines[lines.length - 1] === '') lines.pop();
                const MAX = 30;
                const kept = lines.slice(0, MAX);
                if (lines.length > MAX) kept.push('...');
                return '```\n' + kept.join('\n') + '\n```';
            };
            return content
                .replace(/<metadsl\b[^>]*>([\s\S]*?)<\/metadsl\s*>/gi,
                    (_, body) => collapseBody(body))
                .replace(/```[ \t]*metadsl\b[^\n]*\n([\s\S]*?)```/gi,
                    (_, body) => collapseBody(body))
                // Bare fenced code block whose first non-blank content line is
                // the MetaDSL execute marker (// @execute or # @execute).
                // Backreference \1 handles 4+ backtick/tilde fences that wrap
                // a body containing ```.
                .replace(/([`~]{3,})[^\n]*\n((?:[ \t]*\n)*[ \t]*(?:\/\/|#)[ \t]*@execute\b[\s\S]*?)\1/gi,
                    (_, _fence, body) => collapseBody(body));
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
