// ============================================================================
// OpenClaw WebSocket + HTTP Relay Server
// - Forwards 'message' and 'tool_request' type messages between authenticated clients
// - Provides HTTP REST API that bridges to WS clients
// - Handles 'auth' protocol with apiKey validation from config.json
// - Ignores 'ping' and other message types
// ============================================================================
const http = require('http');
const { WebSocketServer } = require('ws');
const fs = require('fs');
const path = require('path');

// Configuration via environment variables or defaults
const PORT = parseInt(process.env.OPENCLAW_WS_PORT || '3000', 10);
const HOST = process.env.OPENCLAW_WS_HOST || '0.0.0.0';
const WS_PATH = process.env.OPENCLAW_WS_PATH || '/ws';
const CONFIG_PATH = path.join(__dirname, 'config.json');
const HTTP_TIMEOUT = parseInt(process.env.OPENCLAW_HTTP_TIMEOUT || '60000', 10);

// Simple logger
function log(level, msg) {
  const ts = new Date().toISOString();
  console.log(`[${ts}] [${level}] ${msg}`);
}

// ---- Config loading with hot-reload ----
let config = { apiKeys: [], maxConnectionsPerKey: 3 };

function loadConfig() {
  try {
    const raw = fs.readFileSync(CONFIG_PATH, 'utf-8');
    const parsed = JSON.parse(raw);
    config.apiKeys = Array.isArray(parsed.apiKeys) ? parsed.apiKeys : [];
    config.maxConnectionsPerKey = parseInt(parsed.maxConnectionsPerKey, 10) || 3;
    log('INFO', `Config loaded: ${config.apiKeys.length} apiKey(s), maxConn/key=${config.maxConnectionsPerKey}`);
  } catch (e) {
    log('ERROR', `Failed to load config: ${e.message}`);
  }
}

loadConfig();

// Watch config file for changes
try {
  fs.watch(CONFIG_PATH, (eventType) => {
    if (eventType === 'change') {
      log('INFO', 'Config file changed, reloading...');
      loadConfig();
    }
  });
} catch (e) {
  log('WARN', `Cannot watch config file: ${e.message}`);
}

// ---- Error response helper (WS) ----
function sendError(ws, code, message, details) {
  const resp = {
    error: {
      code: code,
      message: message
    }
  };
  if (details) {
    resp.error.details = details;
  }
  try {
    ws.send(JSON.stringify(resp));
  } catch (_) { }
}

// ---- HTTP response helpers ----
function httpJson(res, statusCode, obj) {
  const body = JSON.stringify(obj);
  res.writeHead(statusCode, {
    'Content-Type': 'application/json',
    'Content-Length': Buffer.byteLength(body),
    'Access-Control-Allow-Origin': '*'
  });
  res.end(body);
}

function httpError(res, statusCode, code, message) {
  httpJson(res, statusCode, { error: { code, message } });
}

function readBody(req) {
  return new Promise((resolve, reject) => {
    const chunks = [];
    req.on('data', (chunk) => chunks.push(chunk));
    req.on('end', () => {
      try {
        resolve(JSON.parse(Buffer.concat(chunks).toString()));
      } catch (e) {
        reject(new Error('Invalid JSON body'));
      }
    });
    req.on('error', reject);
  });
}

// ---- HTTP auth check ----
function httpCheckAuth(req) {
  const auth = req.headers['authorization'] || '';
  const match = auth.match(/^Bearer\s+(.+)$/i);
  if (!match) return false;
  return config.apiKeys.includes(match[1]);
}

// ---- Pending HTTP requests (waiting for WS response) ----
const pendingHttpRequests = new Map();
let httpReqId = 0;

function nextHttpReqId() {
  return 'http_' + (++httpReqId);
}

// Broadcast message to WS clients and wait for response with matching requestId
function broadcastAndWait(msg, timeoutMs) {
  return new Promise((resolve, reject) => {
    const id = nextHttpReqId();
    msg.requestId = id;
    const dataStr = JSON.stringify(msg);

    let sent = 0;
    for (const client of wss.clients) {
      if (client.readyState === 1 && client._authenticated) {
        client.send(dataStr);
        sent++;
      }
    }

    if (sent === 0) {
      reject(new Error('No authenticated WS clients available'));
      return;
    }

    const timer = setTimeout(() => {
      pendingHttpRequests.delete(id);
      reject(new Error('Timeout waiting for WS response'));
    }, timeoutMs || HTTP_TIMEOUT);

    pendingHttpRequests.set(id, { resolve, reject, timer });
    log('DEBUG', `HTTP req ${id} broadcast to ${sent} WS client(s), waiting...`);
  });
}

// ---- HTTP request handler ----
function handleHttpRequest(req, res) {
  // CORS preflight
  if (req.method === 'OPTIONS') {
    res.writeHead(204, {
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'GET, POST, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type, Authorization',
      'Access-Control-Max-Age': '86400'
    });
    res.end();
    return;
  }

  const url = new URL(req.url, `http://${req.headers.host}`);
  const pathname = url.pathname;

  // ---- GET /api/status ----
  if (req.method === 'GET' && pathname === '/api/status') {
    let authenticated = 0;
    for (const c of wss.clients) {
      if (c._authenticated) authenticated++;
    }
    httpJson(res, 200, {
      ok: true,
      clients: wss.clients.size,
      authenticated: authenticated,
      uptime: process.uptime()
    });
    return;
  }

  // All other API routes require auth
  if (!httpCheckAuth(req)) {
    httpError(res, 401, 'UNAUTHORIZED', 'Invalid or missing API key');
    return;
  }

  // ---- POST /api/sessions/heartbeat ----
  if (req.method === 'POST' && pathname === '/api/sessions/heartbeat') {
    readBody(req).then((body) => {
      httpJson(res, 200, { ok: true, sessionKey: body.sessionKey || '' });
    }).catch((e) => {
      httpError(res, 400, 'BAD_REQUEST', e.message);
    });
    return;
  }

  // ---- POST /api/sessions ----
  if (req.method === 'POST' && pathname === '/api/sessions') {
    readBody(req).then((body) => {
      const sessionKey = 'sess_' + Date.now() + '_' + Math.random().toString(36).substring(2, 8);
      httpJson(res, 200, { ok: true, sessionKey, label: body.label || '' });
    }).catch((e) => {
      httpError(res, 400, 'BAD_REQUEST', e.message);
    });
    return;
  }

  // ---- POST /api/chat ----
  if (req.method === 'POST' && pathname === '/api/chat') {
    readBody(req).then((body) => {
      return broadcastAndWait({
        type: 'message',
        content: body.message || '',
        session: body.sessionKey || '',
        channel: body.channel || 'web',
        tools: body.tools || []
      });
    }).then((result) => {
      httpJson(res, 200, result);
    }).catch((e) => {
      httpError(res, 502, 'WS_ERROR', e.message);
    });
    return;
  }

  // ---- POST /api/tools/:tool ----
  const toolMatch = pathname.match(/^\/api\/tools\/([^\/]+)$/);
  if (req.method === 'POST' && toolMatch) {
    const tool = decodeURIComponent(toolMatch[1]);
    readBody(req).then((body) => {
      return broadcastAndWait({
        type: 'tool_request',
        tool: tool,
        params: body.params || {}
      });
    }).then((result) => {
      httpJson(res, 200, result);
    }).catch((e) => {
      httpError(res, 502, 'WS_ERROR', e.message);
    });
    return;
  }

  // ---- 404 ----
  httpError(res, 404, 'NOT_FOUND', 'Unknown endpoint: ' + pathname);
}

// ---- Create HTTP server ----
const server = http.createServer(handleHttpRequest);

// ---- WebSocket server (mounted on HTTP server) ----
const wss = new WebSocketServer({ server: server, path: WS_PATH });

let nextId = 1;

wss.on('connection', (ws, req) => {
  const clientId = nextId++;
  const remoteAddr = req.socket.remoteAddress + ':' + req.socket.remotePort;
  ws._relayId = clientId;
  ws._authenticated = false;
  ws._lastPingTime = Date.now();

  log('INFO', `Client #${clientId} connected from ${remoteAddr} (total: ${wss.clients.size})`);

  ws.on('message', (rawData, isBinary) => {
    // Only handle text (JSON) messages
    if (isBinary) {
      log('DEBUG', `Client #${clientId} sent binary data, ignored`);
      return;
    }

    const dataStr = rawData.toString();
    let msg;
    try {
      msg = JSON.parse(dataStr);
    } catch (e) {
      log('WARN', `Client #${clientId} sent non-JSON message, ignored`);
      sendError(ws, 'INVALID_FORMAT', 'Message must be valid JSON');
      return;
    }

    const msgType = msg.type;
    const preview = dataStr.length > 200 ? dataStr.substring(0, 200) + '...' : dataStr;

    // ---- Handle auth ----
    if (msgType === 'auth') {
      const token = msg.token || '';
      if (config.apiKeys.includes(token)) {
        ws._authenticated = true;
        ws._apiKey = token;
        ws._lastPingTime = Date.now();
        // Enforce max connections per apiKey
        const maxConn = config.maxConnectionsPerKey;
        const sameKeyClients = [];
        for (const c of wss.clients) {
          if (c._authenticated && c._apiKey === token) {
            sameKeyClients.push(c);
          }
        }
        while (sameKeyClients.length > maxConn) {
          // Find least active client (oldest _lastPingTime)
          let leastActive = sameKeyClients[0];
          for (let i = 1; i < sameKeyClients.length; i++) {
            if ((sameKeyClients[i]._lastPingTime || 0) < (leastActive._lastPingTime || 0)) {
              leastActive = sameKeyClients[i];
            }
          }
          log('INFO', `Kicking least active client #${leastActive._relayId} (key=...${token.slice(-6)}, connections=${sameKeyClients.length}/${maxConn})`);
          try {
            leastActive.send(JSON.stringify({ type: 'kicked', reason: 'max_connections_exceeded' }));
          } catch (_) { }
          leastActive.close(4003, 'Max connections per key exceeded');
          sameKeyClients.splice(sameKeyClients.indexOf(leastActive), 1);
        }
        ws.send(JSON.stringify({ type: 'auth_ok' }));
        log('INFO', `Client #${clientId} authenticated (key=...${token.slice(-6)}, connections=${sameKeyClients.length}/${maxConn})`);
      } else {
        log('WARN', `Client #${clientId} auth failed (invalid token)`);
        sendError(ws, 'AUTH_FAILED', 'Invalid API key');
        ws.close(4001, 'Authentication failed');
      }
      return;
    }

    // ---- Handle ping (reset timeout) ----
    if (msgType === 'ping') {
      ws._lastPingTime = Date.now();
      log('DEBUG', `Client #${clientId} ping, timeout reset`);
      return;
    }

    // ---- Check if this is a response to a pending HTTP request ----
    if (msg.requestId && pendingHttpRequests.has(msg.requestId)) {
      const pending = pendingHttpRequests.get(msg.requestId);
      pendingHttpRequests.delete(msg.requestId);
      clearTimeout(pending.timer);
      pending.resolve(msg);
      log('DEBUG', `Client #${clientId} responded to HTTP req ${msg.requestId}`);
      return;
    }

    // ---- Handle message and tool_request (broadcast) ----
    if (msgType === 'message' || msgType === 'tool_request') {
      if (!ws._authenticated) {
        log('WARN', `Client #${clientId} not authenticated, rejecting message`);
        sendError(ws, 'UNAUTHORIZED', 'Authentication required');
        return;
      }

      log('DEBUG', `Client #${clientId} => ${preview}`);

      // Broadcast to all other authenticated clients
      let sent = 0;
      for (const client of wss.clients) {
        if (client !== ws && client.readyState === 1 && client._authenticated) {
          client.send(dataStr);
          sent++;
        }
      }
      log('DEBUG', `Broadcast from #${clientId} to ${sent} client(s)`);
      return;
    }

    // ---- Unknown message type ----
    log('DEBUG', `Client #${clientId} sent unknown type '${msgType}', ignored`);
  });

  ws.on('close', (code, reason) => {
    log('INFO', `Client #${clientId} disconnected (code=${code}, total: ${wss.clients.size})`);
  });

  ws.on('error', (err) => {
    log('ERROR', `Client #${clientId} error: ${err.message}`);
  });
});

wss.on('error', (err) => {
  log('ERROR', `WSS error: ${err.message}`);
});

// ---- Start server ----
server.listen(PORT, HOST, () => {
  log('INFO', `OpenClaw Relay listening on http://${HOST}:${PORT}`);
  log('INFO', `  WS endpoint: ws://${HOST}:${PORT}${WS_PATH}`);
  log('INFO', `  HTTP API:    http://${HOST}:${PORT}/api/...`);
});

// ---- Idle connection cleanup (every 5 minutes, timeout 10 minutes) ----
const CLEANUP_INTERVAL = 5 * 60 * 1000;
const IDLE_TIMEOUT = 10 * 60 * 1000;

const cleanupTimer = setInterval(() => {
  const now = Date.now();
  let closed = 0;
  for (const client of wss.clients) {
    const idle = now - (client._lastPingTime || 0);
    if (idle > IDLE_TIMEOUT) {
      log('INFO', `Client #${client._relayId} idle for ${Math.round(idle / 1000)}s, closing`);
      client.close(4002, 'Idle timeout');
      closed++;
    }
  }
  if (closed > 0) {
    log('INFO', `Cleanup: closed ${closed} idle connection(s), remaining: ${wss.clients.size}`);
  }
}, CLEANUP_INTERVAL);

// Graceful shutdown
function shutdown() {
  log('INFO', 'Shutting down...');
  clearInterval(cleanupTimer);
  // Reject all pending HTTP requests
  for (const [id, pending] of pendingHttpRequests) {
    clearTimeout(pending.timer);
    pending.reject(new Error('Server shutting down'));
  }
  pendingHttpRequests.clear();
  server.close(() => {
    log('INFO', 'Server closed');
    process.exit(0);
  });
  // Force exit after 5s
  setTimeout(() => process.exit(1), 5000);
}

process.on('SIGINT', shutdown);
process.on('SIGTERM', shutdown);
