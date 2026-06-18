/**
 * API Client Module
 * Handles communication with OpenAI and Claude APIs
 */

// Create logger instance for this module
const apiLogger = window.logger ? window.logger.createLogger('APIClient') : null;

class APIClient {
    constructor() {
        this.config = this.loadConfig();
        this.abortController = null;
        this.conversationId = ''; // For auto_metadsl conversation management
    }

    resetConversation() {
        this.conversationId = '';
        if (apiLogger) apiLogger.info('Conversation reset');
    }

    loadConfig() {
        const stored = localStorage.getItem('llm_api_config');
        if (stored) {
            try {
                return JSON.parse(stored);
            } catch (e) {
                if (apiLogger) apiLogger.error('Failed to parse stored config:', e);
            }
        }

        return {
            apiType: 'openai',
            apiKey: '',
            apiEndpoint: '',
            model: 'gpt-4.1',
            username: '', // For auto_metadsl auth
            authMode: 'personal', // 'personal' or 'agent' for auto_metadsl
            stream: true // Use streaming for auto_metadsl
        };
    }

    saveConfig(config) {
        this.config = { ...this.config, ...config };
        localStorage.setItem('llm_api_config', JSON.stringify(this.config));
    }

    getConfig() {
        return { ...this.config };
    }

    validateConfig() {
        // auto_metadsl, local_openai and ollama may not require API key
        if (this.config.apiType !== 'auto_metadsl' &&
            this.config.apiType !== 'local_openai' &&
            this.config.apiType !== 'ollama' &&
            !this.config.apiKey) {
            throw new Error('API key is required');
        }
        // local_openai/ollama endpoints are optional: when empty we fall back to
        // http://localhost:11434 and append the proper suffix automatically.
        return true;
    }

    getDefaultEndpoint() {
        if (this.config.apiType === 'openai') {
            return 'https://api.openai.com/v1/chat/completions';
        } else if (this.config.apiType === 'claude') {
            return 'https://api.anthropic.com/v1/messages';
        } else if (this.config.apiType === 'auto_metadsl') {
            return 'https://knot.woa.com/apigw/api/v1/agents/agui/114631ca85184f639f69572bbcfcbe7a';
        } else if (this.config.apiType === 'local_openai') {
            // Default to Ollama's standard local address. The OpenAI-compatible
            // suffix '/v1/chat/completions' will be appended automatically by
            // resolveLocalOpenAIEndpoint() if the user only provides the base URL.
            return 'http://localhost:11434';
        } else if (this.config.apiType === 'ollama') {
            // Default to Ollama's standard local address. The native Ollama
            // suffix '/api/chat' will be appended automatically by
            // resolveOllamaEndpoint() if the user only provides the base URL.
            return 'http://localhost:11434';
        }
        return '';
    }

    getEndpoint() {
        return this.config.apiEndpoint || this.getDefaultEndpoint();
    }

    /**
     * Normalize a local OpenAI-compatible endpoint URL.
     * Accepts either a base URL (e.g. http://localhost:11434) or a full URL
     * (e.g. http://localhost:11434/v1/chat/completions). When only the base
     * URL is supplied, '/v1/chat/completions' is appended automatically.
     */
    resolveLocalOpenAIEndpoint(rawUrl) {
        let url = (rawUrl || '').trim();
        if (!url) return url;
        // Strip trailing slashes for consistent suffix detection
        url = url.replace(/\/+$/, '');
        // If user already provided the chat completions path, keep as-is
        if (/\/v1\/chat\/completions$/i.test(url)) {
            return url;
        }
        // If user provided '/v1' only, append the rest
        if (/\/v1$/i.test(url)) {
            return url + '/chat/completions';
        }
        // Otherwise treat as base URL and append the standard suffix
        return url + '/v1/chat/completions';
    }

    /**
     * Normalize an Ollama native endpoint URL.
     * Accepts either a base URL (e.g. http://localhost:11434) or a full URL
     * (e.g. http://localhost:11434/api/chat). When only the base URL is
     * supplied, '/api/chat' is appended automatically.
     */
    resolveOllamaEndpoint(rawUrl) {
        let url = (rawUrl || '').trim();
        if (!url) return url;
        // Strip trailing slashes for consistent suffix detection
        url = url.replace(/\/+$/, '');
        // If user already provided the chat path, keep as-is
        if (/\/api\/chat$/i.test(url)) {
            return url;
        }
        // If user provided '/api' only, append the rest
        if (/\/api$/i.test(url)) {
            return url + '/chat';
        }
        // Otherwise treat as base URL and append the standard suffix
        return url + '/api/chat';
    }

    /**
     * Stateful splitter that extracts reasoning segments from a streaming
     * text fragment. Recognises three tag styles emitted by reasoning
     * models (qwen3, gpt-oss, deepseek-r1, glm, ...):
     *     <think>...</think>
     *     <thinking>...</thinking>
     *     <reasoning>...</reasoning>
     * Tags are case-insensitive and may span chunk boundaries.
     *
     * Returns { reasoning, text }:
     *   - reasoning: portion of the new fragment that belongs inside a
     *                reasoning tag
     *   - text:      portion that belongs to the visible answer
     *
     * State is carried via the `state` object the caller owns:
     *   { inThink: bool, pending: string, closeTag: string }
     *  - inThink:  whether we are currently inside a reasoning block
     *  - pending:  small carry-over buffer holding a possibly-incomplete
     *              tag at the tail of the previous fragment (e.g. '<thi')
     *  - closeTag: when inThink, the exact close tag we are waiting for
     *              (matches the open tag that started the block, lower-cased)
     */
    splitThinkFragment(fragment, state) {
        // All recognised open/close tag pairs (lower-case for matching).
        const OPENS = ['<think>', '<thinking>', '<reasoning>'];
        const CLOSES = ['</think>', '</thinking>', '</reasoning>'];

        let buf = (state.pending || '') + (fragment || '');
        state.pending = '';
        let reasoning = '';
        let text = '';

        // Find the earliest occurrence of any candidate in buf, doing a
        // case-insensitive search. Returns { idx, length, matchedLower }
        // or null when none of the candidates appear in buf.
        const findEarliest = (haystack, candidates) => {
            const lower = haystack.toLowerCase();
            let best = null;
            for (const cand of candidates) {
                const i = lower.indexOf(cand);
                if (i !== -1 && (best === null || i < best.idx)) {
                    best = { idx: i, length: cand.length, matchedLower: cand };
                }
            }
            return best;
        };

        // Maximum chars we may need to retain at tail to detect a partial
        // tag that straddles the chunk boundary. Equals (longestTag - 1).
        const maxTagLen = Math.max(
            ...OPENS.map(s => s.length),
            ...CLOSES.map(s => s.length)
        );

        while (buf.length > 0) {
            if (state.inThink) {
                // Wait for the specific close tag matching the open tag.
                const closeTag = state.closeTag || '</think>';
                const lower = buf.toLowerCase();
                const idx = lower.indexOf(closeTag);
                if (idx === -1) {
                    // No close tag yet; keep last few chars as pending in
                    // case a partial close tag is straddling the boundary.
                    const keep = Math.min(buf.length, closeTag.length - 1);
                    const tail = buf.slice(buf.length - keep);
                    if (keep > 0 && closeTag.startsWith(tail.toLowerCase())) {
                        reasoning += buf.slice(0, buf.length - keep);
                        state.pending = tail;
                    } else {
                        reasoning += buf;
                    }
                    buf = '';
                } else {
                    reasoning += buf.slice(0, idx);
                    buf = buf.slice(idx + closeTag.length);
                    state.inThink = false;
                    state.closeTag = '';
                }
            } else {
                const hit = findEarliest(buf, OPENS);
                if (hit === null) {
                    // No open tag found. Watch for a partial open tag at
                    // the tail that could be the prefix of any candidate.
                    const keep = Math.min(buf.length, maxTagLen - 1);
                    const tail = buf.slice(buf.length - keep);
                    const tailLower = tail.toLowerCase();
                    let isPrefix = false;
                    for (let k = tailLower.length; k > 0 && !isPrefix; k--) {
                        const sub = tailLower.slice(tailLower.length - k);
                        for (const cand of OPENS) {
                            if (cand.startsWith(sub)) {
                                // Carry the matching suffix only; flush
                                // everything before it as visible text.
                                text += buf.slice(0, buf.length - k);
                                state.pending = buf.slice(buf.length - k);
                                isPrefix = true;
                                break;
                            }
                        }
                    }
                    if (!isPrefix) text += buf;
                    buf = '';
                } else {
                    text += buf.slice(0, hit.idx);
                    buf = buf.slice(hit.idx + hit.length);
                    state.inThink = true;
                    // Derive the matching close tag from the open tag,
                    // e.g. '<thinking>' -> '</thinking>'.
                    state.closeTag = '</' + hit.matchedLower.slice(1);
                }
            }
        }
        return { reasoning, text };
    }

    async sendMessage(messages, onChunk, contextRounds) {
        this.validateConfig();

        // Create new abort controller for this request
        this.abortController = new AbortController();

        if (this.config.apiType === 'openai' || this.config.apiType === 'local_openai') {
            return await this.sendOpenAIMessage(messages, onChunk);
        } else if (this.config.apiType === 'claude') {
            return await this.sendClaudeMessage(messages, onChunk);
        } else if (this.config.apiType === 'auto_metadsl') {
            return await this.sendAutoMetaDSLMessage(messages, onChunk);
        } else if (this.config.apiType === 'ollama') {
            return await this.sendOllamaMessage(messages, onChunk);
        } else {
            throw new Error('Unsupported API type: ' + this.config.apiType);
        }
    }

    async sendOpenAIMessage(messages, onChunk) {
        // For 'openai' the endpoint is used as-is. For 'local_openai' we accept
        // either a base URL (http://localhost:11434) or a full URL and normalize
        // it to '<base>/v1/chat/completions' automatically.
        let endpoint = this.getEndpoint();
        if (this.config.apiType === 'local_openai') {
            endpoint = this.resolveLocalOpenAIEndpoint(endpoint);
        }

        const requestBody = {
            model: this.config.model || 'gpt-4.1',
            messages: messages,
            stream: true
        };

        const headers = {
            'Content-Type': 'application/json'
        };
        // Authorization is optional for local_openai (Ollama / LM Studio usually
        // don't require auth). Only attach the header when an API key is set.
        if (this.config.apiKey) {
            headers['Authorization'] = `Bearer ${this.config.apiKey}`;
        }

        const response = await fetch(endpoint, {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(requestBody),
            signal: this.abortController.signal
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(`OpenAI API error: ${response.status} - ${error}`);
        }

        return await this.handleStreamResponse(response, onChunk, 'openai');
    }

    /**
     * Send a chat request to a local Ollama server using its native
     * '/api/chat' endpoint. The endpoint accepts either a base URL or a
     * full URL; '/api/chat' is appended automatically when missing.
     *
     * The response is NDJSON: one JSON object per line, each containing
     * { message: { role, content }, done } until 'done' is true.
     */
    async sendOllamaMessage(messages, onChunk) {
        let endpoint = this.getEndpoint();
        endpoint = this.resolveOllamaEndpoint(endpoint);

        // Ollama's /api/chat accepts the same role/content shape used by
        // OpenAI, so we can pass the messages array directly.
        const requestBody = {
            model: this.config.model || 'llama3',
            messages: messages,
            stream: true
        };

        const headers = {
            'Content-Type': 'application/json'
        };
        // Ollama normally has no auth, but allow an optional bearer token
        // for users who put it behind a reverse proxy.
        if (this.config.apiKey) {
            headers['Authorization'] = `Bearer ${this.config.apiKey}`;
        }

        const response = await fetch(endpoint, {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(requestBody),
            signal: this.abortController.signal
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Ollama API error: ${response.status} - ${error}`);
        }

        return await this.handleOllamaStreamResponse(response, onChunk);
    }

    /**
     * Parse an Ollama NDJSON streaming response. Each line is a standalone
     * JSON object: { message: { content }, done }. We accumulate content
     * fragments and invoke onChunk for each new piece.
     */
    async handleOllamaStreamResponse(response, onChunk) {
        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let fullResponse = '';
        let fullReasoning = '';
        let buffer = '';
        // State for inline <think>...</think> extraction across chunks
        const thinkState = { inThink: false, pending: '', closeTag: '' };

        const emitContent = (content) => {
            if (!content) return;
            const { reasoning, text } = this.splitThinkFragment(content, thinkState);
            if (reasoning) {
                fullReasoning += reasoning;
                if (onChunk) onChunk(reasoning, fullReasoning, 'reasoning');
            }
            if (text) {
                fullResponse += text;
                if (onChunk) onChunk(text, fullResponse, 'text');
            }
        };

        const emitReasoning = (reasoning) => {
            if (!reasoning) return;
            fullReasoning += reasoning;
            if (onChunk) onChunk(reasoning, fullReasoning, 'reasoning');
        };

        try {
            while (true) {
                const { done, value } = await reader.read();
                if (done) break;

                buffer += decoder.decode(value, { stream: true });
                const lines = buffer.split('\n');
                // Keep the last (possibly incomplete) line in the buffer
                buffer = lines.pop() || '';

                for (const line of lines) {
                    const trimmed = line.trim();
                    if (!trimmed) continue;
                    try {
                        const data = JSON.parse(trimmed);
                        if (data.error) {
                            throw new Error(`Ollama API error: ${data.error}`);
                        }
                        // Ollama-native thinking field (gpt-oss / qwen3 / r1, etc.)
                        const thinking = data.message?.thinking || '';
                        if (thinking) emitReasoning(thinking);
                        const content = data.message?.content || '';
                        if (content) emitContent(content);
                        if (data.done) {
                            return fullResponse;
                        }
                    } catch (e) {
                        if (apiLogger) apiLogger.error('Error parsing Ollama stream line:', { error: e, trimmed });
                    }
                }
            }
            // Flush any remaining buffered line
            const tail = buffer.trim();
            if (tail) {
                try {
                    const data = JSON.parse(tail);
                    const thinking = data.message?.thinking || '';
                    if (thinking) emitReasoning(thinking);
                    const content = data.message?.content || '';
                    if (content) emitContent(content);
                } catch (e) {
                    if (apiLogger) apiLogger.error('Error parsing trailing Ollama chunk:', { error: e, tail });
                }
            }
        } catch (error) {
            if (error.name === 'AbortError') {
                if (apiLogger) apiLogger.info('Request aborted');
                throw new Error('Request cancelled');
            }
            throw error;
        }

        return fullResponse;
    }

    async sendClaudeMessage(messages, onChunk) {
        const endpoint = this.getEndpoint();

        // Convert OpenAI format to Claude format
        const claudeMessages = this.convertToClaudeFormat(messages);

        const requestBody = {
            model: this.config.model || 'claude-sonnet-4-20250514',
            messages: claudeMessages,
            max_tokens: 8192,
            stream: true
        };

        const response = await fetch(endpoint, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'x-api-key': this.config.apiKey,
                'anthropic-version': '2023-06-01'
            },
            body: JSON.stringify(requestBody),
            signal: this.abortController.signal
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Claude API error: ${response.status} - ${error}`);
        }

        return await this.handleStreamResponse(response, onChunk, 'claude');
    }

    async sendAutoMetaDSLMessage(messages, onChunk) {
        const endpoint = this.getEndpoint();

        // Build full context message in the same format as C# AutoMetaDslProvider
        // Format: [role]: content\n for each message
        let messageContent = '';
        
        // Always prepend system prompt (frontend manages context, server is stateless)
        const systemMessages = messages.filter(m => m.role === 'system');
        if (systemMessages.length > 0) {
            const systemContext = systemMessages.map(m => m.content).join('\n\n');
            messageContent += '[system]: ' + systemContext + '\n';
        }
        
        // Append all user/assistant history messages (already cleaned by getConversationContext)
        const conversationMessages = messages.filter(m => m.role !== 'system');
        for (const m of conversationMessages) {
            messageContent += '[' + m.role + ']: ' + m.content + '\n';
        }
        
        if (!messageContent.trim()) {
            throw new Error('No message content to send');
        }

        // Build request body in auto_metadsl format
        // Note: conversation_id is intentionally not used to avoid server-side context accumulation
        // Frontend manages all conversation context to prevent MetaDSL execution result bloat
        const requestBody = {
            input: {
                message: messageContent,
                conversation_id: '', // Always empty - frontend manages context
                model: this.config.model || 'deepseek-v3.1',
                stream: !!this.config.stream,
                enable_web_search: false,
                chat_extra: {
                    attached_images: [],
                    extra_headers: {}
                },
                temperature: 0.5
            }
        };

        const headers = {
            'Content-Type': 'application/json'
        };

        // Add authentication headers
        if (this.config.apiKey) {
            const authMode = this.config.authMode || 'personal';

            if (authMode === 'agent') {
                // Agent token mode: requires x-knot-token + X-Username
                headers['x-knot-token'] = this.config.apiKey;
                headers['X-Username'] = this.config.username || '';
                if (apiLogger) apiLogger.info('Using agent token mode with username:', { username: this.config.username });
            } else {
                // Personal token mode: uses x-knot-api-token, username is optional
                headers['x-knot-api-token'] = this.config.apiKey;

                if (this.config.username) {
                    headers['X-Username'] = this.config.username;
                    if (apiLogger) apiLogger.info('Using personal token mode with username:', { username: this.config.username });
                } else {
                    if (apiLogger) apiLogger.info('Using personal token mode without username');
                }
            }
        }

        if (apiLogger) apiLogger.debug('Request details:', {
            endpoint,
            headers: { ...headers, 'x-knot-api-token': headers['x-knot-api-token'] ? '***' : undefined },
            body: requestBody
        });

        // Debug: Log actual headers being sent (remove in production)
        if (apiLogger) apiLogger.debug('DEBUG - Actual headers:', {
            'x-knot-api-token': headers['x-knot-api-token'] ? `${headers['x-knot-api-token'].substring(0, 10)}...` : 'NOT SET',
            'x-knot-token': headers['x-knot-token'] ? `${headers['x-knot-token'].substring(0, 10)}...` : 'NOT SET',
            'X-Username': headers['X-Username'] || 'NOT SET',
            'Content-Type': headers['Content-Type']
        });

        const response = await fetch(endpoint, {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(requestBody),
            signal: this.abortController.signal
        });

        // Check for HTTP errors
        if (!response.ok) {
            let errorMessage = `HTTP ${response.status}`;
            try {
                const errorText = await response.text();
                const errorJson = JSON.parse(errorText);
                if (errorJson.msg) {
                    errorMessage = `${errorJson.msg} (code: ${errorJson.code || 'unknown'})`;
                } else {
                    errorMessage = errorText;
                }
            } catch (e) {
                errorMessage = await response.text();
            }
            throw new Error(`Auto MetaDSL API error: ${errorMessage}`);
        }

        // Check if response is a stream or an error JSON
        // Some APIs return 200 with error JSON instead of proper HTTP error codes
        const contentType = response.headers.get('content-type');
        if (apiLogger) apiLogger.debug('Response content-type:', { contentType });
        if (apiLogger) apiLogger.debug('Response status:', { status: response.status });

        if (contentType && contentType.includes('application/json')) {
            // Peek at the response to check if it's an error
            const clonedResponse = response.clone();
            try {
                const firstChunk = await clonedResponse.text();
                if (apiLogger) apiLogger.debug('First chunk received:', { chunk: firstChunk.substring(0, 200) });

                // Check if it's a complete JSON error (not a stream)
                if (firstChunk.length < 500) { // Error messages are usually short
                    try {
                        const errorJson = JSON.parse(firstChunk);
                        if (apiLogger) apiLogger.debug('Parsed JSON:', errorJson);

                        if (errorJson.code && errorJson.msg) {
                            throw new Error(`Auto MetaDSL API error: ${errorJson.msg} (code: ${errorJson.code})`);
                        }
                    } catch (parseError) {
                        if (apiLogger) apiLogger.debug('Not a JSON error, continue with stream processing');
                        // Not a JSON error, continue with stream processing
                    }
                }
            } catch (e) {
                if (apiLogger) apiLogger.debug('Error checking response:', e);
                // Continue with stream processing
            }
        }
        return await this.handleStreamResponse(response, onChunk, 'auto_metadsl');
    }

    convertToClaudeFormat(messages) {
        // Claude doesn't support system messages in the same way
        // Extract system message if present
        const systemMessage = messages.find(m => m.role === 'system');
        const userMessages = messages.filter(m => m.role !== 'system');

        // If there's a system message, prepend it to the first user message
        if (systemMessage && userMessages.length > 0) {
            const firstUserMsg = userMessages.find(m => m.role === 'user');
            if (firstUserMsg) {
                firstUserMsg.content = `${systemMessage.content}\n\n${firstUserMsg.content}`;
            }
        }

        return userMessages;
    }

    async handleStreamResponse(response, onChunk, apiType) {
        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let fullResponse = '';
        let fullReasoning = '';
        let buffer = '';
        // State for inline <think>...</think> extraction across chunks
        const thinkState = { inThink: false, pending: '', closeTag: '' };

        const emitContent = (content) => {
            if (!content) return;
            const { reasoning, text } = this.splitThinkFragment(content, thinkState);
            if (reasoning) {
                fullReasoning += reasoning;
                if (onChunk) onChunk(reasoning, fullReasoning, 'reasoning');
            }
            if (text) {
                fullResponse += text;
                if (onChunk) onChunk(text, fullResponse, 'text');
            }
        };

        const emitReasoning = (reasoning) => {
            if (!reasoning) return;
            fullReasoning += reasoning;
            if (onChunk) onChunk(reasoning, fullReasoning, 'reasoning');
        };

        try {
            while (true) {
                const { done, value } = await reader.read();

                if (done) {
                    break;
                }

                buffer += decoder.decode(value, { stream: true });
                const lines = buffer.split('\n');

                // Keep the last incomplete line in buffer
                buffer = lines.pop() || '';

                for (const line of lines) {
                    const trimmed = line.trim();
                    if (!trimmed || trimmed === 'data: [DONE]') {
                        continue;
                    }

                    if (trimmed.startsWith('data:')) {
                        try {
                            const jsonStr = trimmed.substring(5).trim();
                            const data = JSON.parse(jsonStr);

                            let content = '';
                            let reasoning = '';
                            if (apiType === 'openai') {
                                const delta = data.choices?.[0]?.delta || {};
                                content = delta.content || '';
                                // DeepSeek-R1 / o1 / many vendors: reasoning_content
                                // Some Ollama OpenAI-compat builds: reasoning
                                reasoning = delta.reasoning_content || delta.reasoning || '';
                            } else if (apiType === 'claude') {
                                if (data.type === 'content_block_delta') {
                                    const dt = data.delta || {};
                                    if (dt.type === 'thinking_delta') {
                                        reasoning = dt.thinking || '';
                                    } else {
                                        content = dt.text || '';
                                    }
                                }
                            } else if (apiType === 'auto_metadsl') {
                                // Handle auto_metadsl response format
                                if (apiLogger) apiLogger.debug('Received data:', data);

                                if (data.type === 'TEXT_MESSAGE_CONTENT') {
                                    content = data.rawEvent?.content || '';
                                    if (apiLogger) apiLogger.debug('Text content:', { content });
                                    // conversation_id not saved - frontend manages context independently
                                } else {
                                    if (apiLogger) apiLogger.debug('Non-text message type:', { type: data.type });
                                }
                            }

                            if (reasoning) emitReasoning(reasoning);
                            if (content) emitContent(content);
                        } catch (e) {
                            if (apiLogger) apiLogger.error('Error parsing stream data:', { error: e, trimmed });
                        }
                    }
                }
            }
        } catch (error) {
            if (error.name === 'AbortError') {
                if (apiLogger) apiLogger.info('Request aborted');
                throw new Error('Request cancelled');
            }
            throw error;
        }

        return fullResponse;
    }

    abort() {
        if (this.abortController) {
            this.abortController.abort();
            this.abortController = null;
        }
    }

    getAvailableModels(apiType) {
        if (apiType === 'openai') {
            return [
                { value: 'gpt-5.4', label: 'GPT-5.4' },
                { value: 'gpt-5', label: 'GPT-5' },
                { value: 'gpt-4o', label: 'GPT-4o' }
            ];
        } else if (apiType === 'claude') {
            return [
                { value: 'claude-4.7-opus', label: 'Claude-4.7-Opus' },
                { value: 'claude-4.6-opus', label: 'Claude-4.6-Opus' },
                { value: 'claude-4.6-sonnet', label: 'Claude-4.6-Sonnet' },
                { value: 'claude-3-7-sonnet-20250219', label: 'Claude 3.7 Sonnet' }
            ];
        } else if (apiType === 'auto_metadsl') {
            return [
                { value: 'deepseek-v3.1', label: 'DeepSeek-V3.1' },
                { value: 'deepseek-v3.2', label: 'DeepSeek-V3.2' },
                { value: 'glm-5.1', label: 'GLM-5.1' },
                { value: 'claude-4.7-opus', label: 'Claude-4.7-Opus' },
                { value: 'claude-4.6-sonnet', label: 'Claude-4.6-Sonnet' },
                { value: 'claude-4.6-sonnet-1m-context', label: 'Claude-4.6-Sonnet-1M' },
                { value: 'claude-4.6-opus', label: 'Claude-4.6-Opus' },
                { value: 'claude-4.6-opus-1m-context', label: 'Claude-4.6-Opus-1M' },
                { value: 'gpt-5.4', label: 'GPT-5.4' },
                { value: 'hy3-preview', label: 'HY3-Preview' },
                { value: 'kimi-k2.6', label: 'Kimi-K2.6' }
            ];
        } else if (apiType === 'local_openai') {
            // Empty list signals UI to switch to a free-text model input,
            // because local model names are user-defined (e.g. Ollama tag).
            return [];
        } else if (apiType === 'ollama') {
            // Empty list signals UI to switch to a free-text model input,
            // because local model names are user-defined (e.g. 'llama3:8b').
            return [];
        }
        return [];
    }
}

// Export for use in other modules
window.APIClient = APIClient;
