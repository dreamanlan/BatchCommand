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
        this.autoMetaDSLRoundCount = 0; // Track rounds for periodic system prompt injection
    }

    resetConversation() {
        this.conversationId = '';
        this.autoMetaDSLRoundCount = 0;
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
        // auto_metadsl may not require API key in some cases
        if (this.config.apiType !== 'auto_metadsl' && !this.config.apiKey) {
            throw new Error('API key is required');
        }
        return true;
    }

    getDefaultEndpoint() {
        if (this.config.apiType === 'openai') {
            return 'https://api.openai.com/v1/chat/completions';
        } else if (this.config.apiType === 'claude') {
            return 'https://api.anthropic.com/v1/messages';
        } else if (this.config.apiType === 'auto_metadsl') {
            return 'https://knot.woa.com/apigw/api/v1/agents/agui/114631ca85184f639f69572bbcfcbe7a';
        }
        return '';
    }

    getEndpoint() {
        return this.config.apiEndpoint || this.getDefaultEndpoint();
    }

    async sendMessage(messages, onChunk, contextRounds) {
        this.validateConfig();

        // Create new abort controller for this request
        this.abortController = new AbortController();

        if (this.config.apiType === 'openai') {
            return await this.sendOpenAIMessage(messages, onChunk);
        } else if (this.config.apiType === 'claude') {
            return await this.sendClaudeMessage(messages, onChunk);
        } else if (this.config.apiType === 'auto_metadsl') {
            return await this.sendAutoMetaDSLMessage(messages, onChunk, contextRounds);
        } else {
            throw new Error('Unsupported API type: ' + this.config.apiType);
        }
    }

    async sendOpenAIMessage(messages, onChunk) {
        const endpoint = this.getEndpoint();

        const requestBody = {
            model: this.config.model || 'gpt-4.1',
            messages: messages,
            stream: true
        };

        const response = await fetch(endpoint, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.config.apiKey}`
            },
            body: JSON.stringify(requestBody),
            signal: this.abortController.signal
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(`OpenAI API error: ${response.status} - ${error}`);
        }

        return await this.handleStreamResponse(response, onChunk, 'openai');
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

    async sendAutoMetaDSLMessage(messages, onChunk, contextRounds) {
        const endpoint = this.getEndpoint();

        // Get the last user message
        const lastUserMessage = messages.filter(m => m.role === 'user').pop();
        if (!lastUserMessage) {
            throw new Error('No user message found');
        }

        // Periodically prepend system context (dialogPrompt)
        let messageContent = lastUserMessage.content;
        if (contextRounds && contextRounds > 0 && this.autoMetaDSLRoundCount % contextRounds === 0) {
            const systemMessages = messages.filter(m => m.role === 'system');
            if (systemMessages.length > 0) {
                const systemContext = systemMessages.map(m => m.content).join('\n\n');
                messageContent = systemContext + '\n\n' + messageContent;
                if (apiLogger) apiLogger.info('Injected system context at round:', { round: this.autoMetaDSLRoundCount });
            }
        }
        this.autoMetaDSLRoundCount++;

        // Build request body in auto_metadsl format
        const requestBody = {
            input: {
                message: messageContent,
                conversation_id: this.conversationId || '',
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
        let buffer = '';

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
                            if (apiType === 'openai') {
                                content = data.choices?.[0]?.delta?.content || '';
                            } else if (apiType === 'claude') {
                                if (data.type === 'content_block_delta') {
                                    content = data.delta?.text || '';
                                }
                            } else if (apiType === 'auto_metadsl') {
                                // Handle auto_metadsl response format
                                if (apiLogger) apiLogger.debug('Received data:', data);

                                if (data.type === 'TEXT_MESSAGE_CONTENT') {
                                    content = data.rawEvent?.content || '';
                                    if (apiLogger) apiLogger.debug('Text content:', { content });
                                    // Save conversation_id for next request
                                    if (data.rawEvent?.conversation_id) {
                                        this.conversationId = data.rawEvent.conversation_id;
                                        if (apiLogger) apiLogger.info('Conversation ID:', { conversationId: this.conversationId });
                                    }
                                } else {
                                    if (apiLogger) apiLogger.debug('Non-text message type:', { type: data.type });
                                }
                            }

                            if (content) {
                                fullResponse += content;
                                if (onChunk) {
                                    onChunk(content, fullResponse);
                                }
                            }
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
                { value: 'o3', label: 'o3' },
                { value: 'o3-mini', label: 'o3-mini' },
                { value: 'o4-mini', label: 'o4-mini' },
                { value: 'gpt-4.1', label: 'GPT-4.1' },
                { value: 'gpt-4.1-mini', label: 'GPT-4.1 Mini' },
                { value: 'gpt-4.1-nano', label: 'GPT-4.1 Nano' },
                { value: 'gpt-4o', label: 'GPT-4o' },
                { value: 'gpt-4o-mini', label: 'GPT-4o Mini' }
            ];
        } else if (apiType === 'claude') {
            return [
                { value: 'claude-sonnet-4-20250514', label: 'Claude Sonnet 4' },
                { value: 'claude-opus-4-20250514', label: 'Claude Opus 4' },
                { value: 'claude-3-7-sonnet-20250219', label: 'Claude 3.7 Sonnet' },
                { value: 'claude-3-5-sonnet-20241022', label: 'Claude 3.5 Sonnet' },
                { value: 'claude-3-5-haiku-20241022', label: 'Claude 3.5 Haiku' }
            ];
        } else if (apiType === 'auto_metadsl') {
            return [
                { value: 'deepseek-v3.1', label: 'DeepSeek-V3.1' },
                { value: 'deepseek-v3.2', label: 'DeepSeek-V3.2' },
                { value: 'claude-4.6-sonnet', label: 'Claude-4.6-Sonnet' },
                { value: 'kimi-k2.5', label: 'Kimi-K2.5' },
                { value: 'glm-4.7', label: 'GLM-4.7' },
                { value: 'glm-5', label: 'GLM-5' },
                { value: 'hunyuan-2.0-thinking', label: 'HY-2.0-Think' },
                { value: 'hunyuan-2.0-instruct', label: 'HY-2.0-Instruct' }
            ];
        }
        return [];
    }
}

// Export for use in other modules
window.APIClient = APIClient;
