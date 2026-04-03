// ============================================================================
// OpenClaw HTTP API - HTTP REST communication with OpenClaw server
// Used for tool calls and session management
// ============================================================================
class OpenClawHttp {
  constructor() {
    this.logger = logger.createLogger('OpenClawHttp');
  }

  _getBase() {
    return (CONFIG.get('openclaw.httpBase') || 'https://www.gamexyz.net:8443').replace(/\/+$/, '');
  }

  _getApiKey() {
    return CONFIG.get('openclaw.apiKey') || '';
  }

  _getSession() {
    return CONFIG.get('openclaw.session') || '';
  }

  _headers() {
    const h = { 'Content-Type': 'application/json' };
    const key = this._getApiKey();
    if (key) h['Authorization'] = 'Bearer ' + key;
    return h;
  }

  // POST /api/chat  (single-shot, non-streaming)
  async chat(message, opts) {
    opts = opts || {};
    const body = {
      message: message,
      sessionKey: opts.session || this._getSession(),
      channel: opts.channel || 'web',
      tools: opts.tools || []
    };
    return this._post('/api/chat', body);
  }

  // POST /api/tools/:tool
  async callTool(tool, params) {
    return this._post('/api/tools/' + encodeURIComponent(tool), { tool, params: params || {} });
  }

  // POST /api/sessions
  async createSession(label, metadata) {
    return this._post('/api/sessions', { action: 'create', label, metadata: metadata || {} });
  }

  // GET /api/status
  async status() {
    return this._get('/api/status');
  }

  // POST /api/sessions/heartbeat
  async heartbeat(sessionKey) {
    return this._post('/api/sessions/heartbeat', { sessionKey: sessionKey || this._getSession() });
  }

  // ---- low-level helpers ----

  async _get(path) {
    const url = this._getBase() + path;
    this.logger.debug('GET ' + url);
    try {
      const resp = await fetch(url, { method: 'GET', headers: this._headers() });
      return this._handleResp(resp);
    } catch (e) {
      this.logger.error('GET failed: ' + e.message);
      throw e;
    }
  }

  async _post(path, body) {
    const url = this._getBase() + path;
    this.logger.debug('POST ' + url);
    try {
      const resp = await fetch(url, {
        method: 'POST',
        headers: this._headers(),
        body: JSON.stringify(body)
      });
      return this._handleResp(resp);
    } catch (e) {
      this.logger.error('POST failed: ' + e.message);
      throw e;
    }
  }

  async _handleResp(resp) {
    if (!resp.ok) {
      const text = await resp.text().catch(() => '');
      const err = new Error('HTTP ' + resp.status + ': ' + text);
      err.status = resp.status;
      throw err;
    }
    const ct = resp.headers.get('content-type') || '';
    if (ct.includes('application/json')) {
      return resp.json();
    }
    return resp.text();
  }
}

// Expose on window
if (!window.OpenClaw) window.OpenClaw = {};
window.OpenClaw.http = new OpenClawHttp();
