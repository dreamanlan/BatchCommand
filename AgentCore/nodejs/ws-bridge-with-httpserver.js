// ============================================================================
// OpenClaw WebSocket Bridge
// Connects to a relay server, forwards message/tool_request to OpenClaw
// via the /v1/chat/completions API, and sends results back.
// Local HTTP API on :3081 allows OpenClaw to push messages to relay server.
// ============================================================================

const WebSocket = require('ws');
const http = require('http');
const { createServer } = require('http');

// ---- Config ----
const WS_URL = process.env.WS_URL || 'ws://www.gamexyz.net:3000/ws';
const API_KEY = process.env.API_KEY || 'xxx';
const GW_HOST = '127.0.0.1';
const GW_PORT = 23001;
const GW_TOKEN = 'xxx';
const GW_MODEL = 'openclaw:main';
const PING_MS = 30 * 1000;
const RECONNECT_MS = 5 * 1000;
const LOCAL_PORT = 3081;  // ← 新增：本地 HTTP API 端口

// ---- Logger ----
function log(level, msg) {
  console.log(`[${new Date().toISOString()}] [${level}] ${msg}`);
}

// ---- Call OpenClaw /v1/chat/completions ----
function askOpenClaw(userText) {
  return new Promise((resolve, reject) => {
    const body = JSON.stringify({
      model: GW_MODEL,
      messages: [{ role: 'user', content: userText }],
      stream: false,
    });
    const options = {
      hostname: GW_HOST,
      port: GW_PORT,
      path: '/v1/chat/completions',
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Content-Length': Buffer.byteLength(body),
        'Authorization': `Bearer ${GW_TOKEN}`,
      },
    };
    const req = http.request(options, (res) => {
      let data = '';
      res.on('data', chunk => data += chunk);
      res.on('end', () => {
        try {
          const parsed = JSON.parse(data);
          const reply = parsed?.choices?.[0]?.message?.content || data;
          resolve(reply);
        } catch (e) {
          resolve(data);
        }
      });
    });
    req.on('error', reject);
    req.setTimeout(120000, () => req.destroy(new Error('OpenClaw timeout')));
    req.write(body);
    req.end();
  });
}

// ---- Bridge state ----
let ws = null;
let pingTimer = null;
let reconnectTimer = null;

function connect() {
  if (reconnectTimer) { clearTimeout(reconnectTimer); reconnectTimer = null; }
  log('INFO', `Connecting to ${WS_URL} ...`);
  ws = new WebSocket(WS_URL);

  ws.on('open', () => {
    log('INFO', 'Connected. Sending auth...');
    ws.send(JSON.stringify({ type: 'auth', token: API_KEY }));
    pingTimer = setInterval(() => {
      if (ws?.readyState === WebSocket.OPEN) {
        ws.send(JSON.stringify({ type: 'ping' }));
        log('DEBUG', 'Ping sent');
      }
    }, PING_MS);
  });

  ws.on('message', async (rawData) => {
    let msg;
    try { msg = JSON.parse(rawData.toString()); }
    catch { log('WARN', 'Non-JSON, ignored'); return; }

    if (msg.type === 'auth_ok') {
      log('INFO', '✅ Auth OK — bridge is active.');
      return;
    }
    if (msg.error) {
      log('ERROR', `Server: ${msg.error.code} - ${msg.error.message}`);
      return;
    }
    if (msg.type === 'message' || msg.type === 'tool_request') {
      log('INFO', `← [${msg.type}] ${JSON.stringify(msg).substring(0, 120)}`);
      let userText;
      if (msg.type === 'message') {
        userText = msg.text ?? msg.content ?? msg.body ?? JSON.stringify(msg);
      } else {
        const tool = msg.tool ?? msg.name ?? '';
        const args = msg.params ?? msg.arguments ?? msg;
        userText = `[Tool Request] ${tool}: ${JSON.stringify(args)}`;
      }
      try {
        const reply = await askOpenClaw(userText);
        log('INFO', `← OpenClaw: ${String(reply).substring(0, 120)}`);
        if (ws?.readyState === WebSocket.OPEN) {
          ws.send(JSON.stringify({
            type: 'message',
            text: reply,
            ...(msg.requestId ? { requestId: msg.requestId } : {}),
            ...(msg.id ? { replyTo: msg.id } : {}),
            from: 'openclaw-bridge',
          }));
        }
      } catch (err) {
        log('ERROR', `OpenClaw call failed: ${err.message}`);
        if (ws?.readyState === WebSocket.OPEN) {
          ws.send(JSON.stringify({
            type: 'message',
            text: `[Bridge Error] ${err.message}`,
            ...(msg.requestId ? { requestId: msg.requestId } : {}),
            from: 'openclaw-bridge',
          }));
        }
      }
      return;
    }
    log('DEBUG', `Unknown type '${msg.type}', ignored`);
  });

  ws.on('close', (code) => {
    log('WARN', `Disconnected (code=${code}). Reconnecting in ${RECONNECT_MS / 1000}s...`);
    cleanup();
    reconnectTimer = setTimeout(connect, RECONNECT_MS);
  });
  ws.on('error', (err) => {
    log('ERROR', `WS error: ${err.message}`);
  });
}

function cleanup() {
  if (pingTimer) { clearInterval(pingTimer); pingTimer = null; }
  if (ws) { ws.removeAllListeners(); ws = null; }
}

process.on('SIGINT', () => { cleanup(); process.exit(0); });
process.on('SIGTERM', () => { cleanup(); process.exit(0); });

// ---- 本地 HTTP API ----
// POST /send  { "text": "...", "type": "message" }  → 发到 relay server
// GET  /status                                       → 连接状态
const localServer = createServer((req, res) => {
  if (req.method === 'POST' && req.url === '/send') {
    let body = '';
    req.on('data', chunk => body += chunk);
    req.on('end', () => {
      try {
        const payload = JSON.parse(body);
        const text = payload.text || payload.message || '';
        const type = payload.type || 'message';
        if (!text) {
          res.writeHead(400, { 'Content-Type': 'application/json' });
          res.end(JSON.stringify({ ok: false, error: 'text is required' })); return;
        }
        if (!ws || ws.readyState !== WebSocket.OPEN) {
          res.writeHead(503, { 'Content-Type': 'application/json' });
          res.end(JSON.stringify({ ok: false, error: 'WebSocket not connected' })); return;
        }
        ws.send(JSON.stringify({ type, text, from: 'openclaw-bridge' }));
        log('INFO', `→ [local→relay] [${type}] ${text.substring(0, 100)}`);
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({ ok: true }));
      } catch (e) {
        res.writeHead(400, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({ ok: false, error: e.message }));
      }
    });
    return;
  }
  if (req.method === 'GET' && req.url === '/status') {
    const connected = !!(ws && ws.readyState === WebSocket.OPEN);
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ ok: true, connected, wsUrl: WS_URL }));
    return;
  }
  res.writeHead(404); res.end('Not Found');
});

localServer.listen(LOCAL_PORT, '127.0.0.1', () => {
  log('INFO', `Local HTTP API on http://127.0.0.1:${LOCAL_PORT} — POST /send, GET /status`);
});

log('INFO', `OpenClaw WS Bridge starting → ${WS_URL}`);
connect();
