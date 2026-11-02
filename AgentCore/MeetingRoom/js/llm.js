// ============================================================================
// LlmClient - HTTP client for LLM chat API (OpenAI-compatible)
// ----------------------------------------------------------------------------
// All requests go through the local AgentCore CORS proxy (llm_proxy on the
// MetaDSL websocket port): pages served from http://localhost:<port> cannot
// fetch the LLM gateways directly (they do not answer the CORS preflight for
// localhost origins), so the fetch targets the proxy and the proxy forwards
// natively (no CORS in the C# HttpClient). The upstream URL is passed in the
// 'target' query parameter; auth headers are forwarded as-is, cookies set by
// an upstream are kept in the proxy's cookie container.
// ============================================================================
// Port = @HttpProxyPort in script_agent.dsl (httpproxy_start_server); the
// path is ignored by the proxy (the upstream travels in 'target').
const LLM_PROXY_URL = 'http://localhost:9528/proxy';

function llmProxyUrl(url) {
  return LLM_PROXY_URL + '?target=' + encodeURIComponent(url);
}

class LlmClient {
  constructor(config) {
    this.name = config.name || 'LLM';
    this.type = config.type || 'openai';
    this.baseUrl = (config.baseUrl || '').replace(/\/+$/, '');
    this.apiKey = config.apiKey || '';
    this.anydevHost = config.anydevHost || '';
    this.model = config.model || null;
    this.chatPath = config.chatPath || '/v1/chat/completions';
    this.enabled = config.enabled !== false;
    this.stateless = config.stateless === true;
    this._sentFingerprints = new Set();
  }

  get busy() { return this._busy; }

  // Send a chat message and get reply (non-streaming)
  // messages: string (single prompt) or array of {role, content} objects
  async chat(messages) {
    if (!this.baseUrl || !this.apiKey) {
      throw new Error('LLM not configured: missing baseUrl or apiKey');
    }
    // Stateless mode: only send unsent user messages, strip "[name] " prefix that chat.js injected into content.
    let _isStateless = false;
    if (this.stateless && Array.isArray(messages)) {
      const newUserMsgs = [];
      for (let i = 0; i < messages.length; i++) {
        const m = messages[i];
        if (m.role !== 'user') continue;
        const fp = (m.name || '') + '|' + m.content;
        if (this._sentFingerprints.has(fp)) continue;
        this._sentFingerprints.add(fp);
        const _cleanContent = m.content.replace(/^\[[^\]]+\]\s+/, '');
        newUserMsgs.push({ role: 'user', content: _cleanContent });
      }
      if (newUserMsgs.length === 0) {
        console.log('[llm] stateless ' + this.name + ': no new user messages, skip');
        return '';
      }
      messages = newUserMsgs;
      _isStateless = true;
    }
    if (this.type === 'knot') {
      return this._chatKnot(messages);
    }

    const url = this.baseUrl + this.chatPath;
    // Support both string prompt and messages array
    let msgArray;
    if (typeof messages === 'string') {
      msgArray = [{ role: 'user', content: messages }];
    } else if (Array.isArray(messages)) {
      msgArray = messages;
    } else {
      msgArray = [{ role: 'user', content: String(messages) }];
    }
    // Flatten all messages into a single user message string.
    // Stateless mode sends raw content only (no [role] prefix) so the target LLM receives exactly what the user typed.
    let flatContent;
    if (_isStateless) {
      flatContent = msgArray.map(function (m) { return m.content; }).join('\n');
    } else {
      flatContent = msgArray.map(function (m) {
        return '[' + (m.name || m.role) + '] ' + m.content;
      }).join('\n');
    }

    console.log('[llm] sending flatContent len=' + flatContent.length + ':\n' + getStringInLength(flatContent, 300, 2));

    const payload = {
      messages: [{ role: 'user', content: flatContent }],
      stream: false
    };
    if (this.model) {
      payload.model = this.model;
    }

    const headers = {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'Authorization': 'Bearer ' + this.apiKey,
      'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36'
    };
    if (this.anydevHost) {
      headers['X-Anydev-Host'] = this.anydevHost;
    }

    this._busy = true;
    try {
      const resp = await fetch(llmProxyUrl(url), {
        method: 'POST',
        headers: headers,
        body: JSON.stringify(payload)
      });


      if (!resp.ok) {

        const text = await resp.text();
        throw new Error('[' + resp.status + '] ' + text);
      }

      const rawText = await resp.text();
      let data;
      try {
        data = JSON.parse(rawText);
      } catch (e) {
        throw new Error('JSON parse failed: ' + rawText);
      }
      const choices = data.choices || [];
      if (!choices.length) {
        throw new Error('No choices in response');
      }
      return choices[0].message.content || '';
    } finally {
      this._busy = false;
    }
  }

  // Knot protocol chat (streaming SSE)
  async _chatKnot(messages) {
    // Build message string from messages input
    let messageStr;
    if (typeof messages === 'string') {
      messageStr = messages;
    } else if (Array.isArray(messages)) {
      messageStr = messages.map(function (m) {
        return '[' + (m.name || m.role) + '] ' + m.content;
      }).join('\n');
    } else {
      messageStr = String(messages);
    }

    console.log('[llm-knot] sending messageStr len=' + messageStr.length + ':\n' + getStringInLength(messageStr, 300, 2));

    const payload = {
      input: {
        message: messageStr,
        conversation_id: '',
        stream: true,
        enable_web_search: false,
        chat_extra: {},
        temperature: 0.5
      }
    };
    if (this.model) {
      payload.input.model = this.model;
    }

    const headers = {
      'Content-Type': 'application/json',
      'x-knot-api-token': this.apiKey
    };

    this._busy = true;
    try {
      const resp = await fetch(llmProxyUrl(this.baseUrl), {
        method: 'POST',
        headers: headers,
        body: JSON.stringify(payload)
      });

      if (!resp.ok) {
        const text = await resp.text();
        throw new Error('[' + resp.status + '] ' + text);
      }

      // Read SSE stream
      const reader = resp.body.getReader();
      const decoder = new TextDecoder();
      let resultText = '';
      let buffer = '';

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;
        buffer += decoder.decode(value, { stream: true });

        // Process complete lines
        const lines = buffer.split('\n');
        buffer = lines.pop(); // keep incomplete line in buffer
        for (let i = 0; i < lines.length; i++) {
          let line = lines[i].trim();
          if (!line) continue;
          // Strip "data:" prefix
          if (line.startsWith('data:')) {
            line = line.substring(5).trim();
          }
          if (line === '[DONE]') continue;
          try {
            const msg = JSON.parse(line);
            if (!msg.type) continue;
            // Save conversation_id for future use
            if (msg.type === 'TEXT_MESSAGE_CONTENT' && msg.rawEvent && msg.rawEvent.content) {
              resultText += msg.rawEvent.content;
            }
          } catch (e) {
            // skip non-JSON lines
          }
        }
      }

      if (!resultText) {
        throw new Error('No content in knot response');
      }
      return resultText;
    } finally {
      this._busy = false;
    }
  }

  // Update config
  updateConfig(config) {
    if (config.name !== undefined) this.name = config.name;
    if (config.baseUrl !== undefined) this.baseUrl = (config.baseUrl || '').replace(/\/+$/, '');
    if (config.apiKey !== undefined) this.apiKey = config.apiKey;
    if (config.anydevHost !== undefined) this.anydevHost = config.anydevHost;
    if (config.type !== undefined) this.type = config.type;
    if (config.model !== undefined) this.model = config.model;
    if (config.chatPath !== undefined) this.chatPath = config.chatPath;
    if (config.enabled !== undefined) this.enabled = config.enabled;
    if (config.stateless !== undefined) {
      const oldStateless = this.stateless;
      this.stateless = config.stateless === true;
      if (oldStateless && !this.stateless) {
        this._sentFingerprints.clear();
      }
    }
  }

  // Serialize to plain object for localStorage
  toJSON() {
    return {
      name: this.name,
      type: this.type,
      baseUrl: this.baseUrl,
      anydevHost: this.anydevHost,
      model: this.model,
      chatPath: this.chatPath,
      enabled: this.enabled,
      stateless: this.stateless
    };
  }
}
// ============================================================================
// LlmManager - Manage multiple LLM clients
// ============================================================================
class LlmManager {
  constructor() {
    this.clients = [];
    this.secretStore = new SecretStore('aiclaw_secret_store');
    this._loadConfig();
  }

  // Add a new LLM client
  add(config) {
    const client = new LlmClient(config);
    this.clients.push(client);
    this._saveConfig();
    return client;
  }

  // Remove LLM client by index
  remove(index) {
    if (index >= 0 && index < this.clients.length) {
      this.clients.splice(index, 1);
      this._saveConfig();
    }
  }

  // Update LLM client config by index
  update(index, config) {
    if (index >= 0 && index < this.clients.length) {
      this.clients[index].updateConfig(config);
      this._saveConfig();
    }
  }

  // Get all enabled clients
  getEnabled() {
    return this.clients.filter(c => c.enabled);
  }

  // Get clients matching @name target (or all enabled if no target)
  getTargets(text) {
    const match = text.match(/^@([\w-]+)\s/);
    if (match) {
      const targetName = match[1];
      const target = this.clients.find(c => c.enabled && c.name === targetName);
      return target ? [target] : [];
    }
    return this.getEnabled();
  }

  // Strip @name prefix from text if present
  stripTarget(text) {
    return text.replace(/^@[\w-]+\s+/, '');
  }

  // Send message to targeted LLM(s) and collect replies
  async sendToAll(text, onReply, onError) {
    const targets = this.getTargets(text);
    const prompt = this.stripTarget(text);
    if (!targets.length) return;

    const promises = targets.map(async (client) => {
      try {
        const reply = await client.chat(prompt);
        if (onReply) onReply(client, reply);
      } catch (e) {
        if (onError) onError(client, e);
      }
    });
    await Promise.all(promises);
  }

  // ---- persistence ----

  _loadConfig() {
    try {
      const raw = localStorage.getItem('aiclaw_llm_configs');
      if (raw) {
        const arr = JSON.parse(raw);
        this.clients = arr.map(c => new LlmClient(c));
      }
    } catch (e) {
      console.warn('[llm] Failed to load config: ' + e.message);
    }
  }

  // Load apiKey from SecretStore (async, call after construction)
  async _loadSecrets() {
    try {
      await this.secretStore.ready();
      for (let i = 0; i < this.clients.length; i++) {
        const key = await this.secretStore.getItem('llm_apikey_' + i);
        if (key) {
          this.clients[i].apiKey = key;
        }
      }
    } catch (e) {
      console.warn('[llm] Failed to load secrets: ' + e.message);
    }
  }

  _saveConfig() {
    try {
      const arr = this.clients.map(c => c.toJSON());
      localStorage.setItem('aiclaw_llm_configs', JSON.stringify(arr));
    } catch (e) {
      console.warn('[llm] Failed to save config: ' + e.message);
    }
    // Save apiKeys to SecretStore (async, fire-and-forget)
    this._saveSecrets();
  }

  async _saveSecrets() {
    try {
      await this.secretStore.ready();
      for (let i = 0; i < this.clients.length; i++) {
        if (this.clients[i].apiKey) {
          await this.secretStore.setItem('llm_apikey_' + i, this.clients[i].apiKey);
        } else {
          await this.secretStore.removeItem('llm_apikey_' + i);
        }
      }
    } catch (e) {
      console.warn('[llm] Failed to save secrets: ' + e.message);
    }
  }
}
