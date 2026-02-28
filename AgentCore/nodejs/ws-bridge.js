// ============================================================================
// OpenClaw WebSocket Bridge
// Connects to a relay server, forwards message/tool_request to OpenClaw
// via the /v1/chat/completions API, and sends results back.
// ============================================================================

const WebSocket = require('ws');
const http = require('http');

// ---- Config ----
const WS_URL = process.env.WS_URL || 'ws://www.gamexyz.net:3000/ws';
const API_KEY = process.env.API_KEY || 'xxx';
const GW_HOST = '127.0.0.1';
const GW_PORT = 23001;
const GW_TOKEN = 'xxx';
const GW_MODEL = 'openclaw:main';
const PING_MS = 30 * 1000;   // 30s ping to relay server
const RECONNECT_MS = 5 * 1000;   // 5s reconnect delay

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

log('INFO', `OpenClaw WS Bridge starting → ${WS_URL}`);
connect();
