#!/usr/bin/env node
// wechat-bridge-for-agent.js
// Local relay server + wechat bridge, direct agent connection.
// Chain: agent (relay_ws.js) -> this server (port 3000) -> wechat.
// Deployed in parallel with the remote relay chain, NOT a replacement.
// Step 2: relay protocol subset + wechat bridge layer (ported from
// wechat-bridge.js, auto login on start).

'use strict';

const http = require('http');
const https = require('https');
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const { WebSocketServer } = require('ws');

const PORT = process.env.PORT || 3000;

// ---------------------------------------------------------------------------
// Wechat configuration (ported from wechat-bridge.js)
// ---------------------------------------------------------------------------
const CREDENTIALS_FILE = path.join(__dirname, 'wechat-credentials.json');
const CHANNEL_VERSION = '2.0.1-js';
const DEFAULT_BASE_URL = 'https://ilinkai.weixin.qq.com';
const POLL_TIMEOUT_MS = 40000; // slightly longer than server's ~35s
const QR_POLL_TIMEOUT_MS = 35000;
const QR_TOTAL_TIMEOUT_MS = 8 * 60 * 1000; // 8 minutes
const QR_MAX_REFRESH = 3;

// ---------------------------------------------------------------------------
// Relay protocol subset (client side: relay_ws.js in inject_modules)
// - auth: client sends {type:'auth', token}; server replies auth_response.
//   The client does NOT validate the response body, lenient check is enough.
// - ping: client sends {type:'ping'} every 30s; reply pong (harmless).
// - message with requestId: chat test round trip, reply with the same
//   requestId and body field content (or text).
// - message without requestId: fire-and-forget push from LLM side
//   (forwardToRelay -> pushMessage), carries channelId for routing.
// - inbound push to agent: {type:'message', content, channelId}.
// ---------------------------------------------------------------------------

// Connected agent clients. Single-agent design, but keep a set for safety.
const agentClients = new Set();

    // Side table for R3: clientId -> ws (identity only, no routing yet).
    const agentClientIds = new Map();

    // Side tables for R5: ws -> registered name, name -> ws for routing.
    const agentClientNames = new Map();
    const nameToClient = new Map();

    // Route state for R4: 'broadcast' or 'solo', target is registered agent name.
    let wxRouteMode = 'broadcast';
    let wxRouteTarget = null;

// channelId -> wechat session info (filled by wechat layer in step 2).
const channelSessions = new Map();

function send(ws, obj) {
  if (ws.readyState === ws.OPEN) {
    ws.send(JSON.stringify(obj));
  }
}

// Push a wechat inbound message to agents based on the R4 route mode.
// Called by the wechat layer (step 2) on every pollMessages hit.
function pushToAgent(content, channelId) {
  if (wxRouteMode === 'solo' && wxRouteTarget) {
    const ws = nameToClient.get(wxRouteTarget);
    if (ws && ws.readyState === ws.OPEN) {
      send(ws, { type: 'message', content: content, channelId: channelId });
    } else {
      console.log('[WX] solo target not connected:', wxRouteTarget);
    }
    return;
  }
  for (const ws of agentClients) {
    send(ws, { type: 'message', content: content, channelId: channelId });
  }
}

// ---------------------------------------------------------------------------
// Wechat bridge layer (ported from wechat-bridge.js)
// ---------------------------------------------------------------------------

// Wechat state
let credentials = null; // { bot_token, ilink_bot_id, base_url, ilink_user_id }
let getUpdatesBuf = '';
let polling = false;
let contextTokenMap = {}; // userId -> contextToken

function randomWechatUIN() {
  const n = Math.floor(Math.random() * 900000000) + 100000000;
  return String(n);
}

function nowMs() {
  return Date.now();
}

function elapsedMs(startMs) {
  return Date.now() - startMs + 'ms';
}

function shortText(value, maxLen) {
  const max = maxLen || 120;
  const text = String(value == null ? '' : value);
  return text.length > max ? text.substring(0, max) + '...' : text;
}

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

// ---------------------------------------------------------------------------
// HTTPS request helper
// ---------------------------------------------------------------------------

function httpsRequest(method, urlStr, headers, body, timeoutMs, tag) {
  return new Promise((resolve, reject) => {
    const logTag = tag ? '[' + tag + '] ' : '';
    const url = new URL(urlStr);
    const options = {
      hostname: url.hostname,
      port: url.port || 443,
      path: url.pathname + url.search,
      method: method,
      headers: headers || {},
      timeout: timeoutMs || 30000
    };

    const req = https.request(options, (res) => {
      let data = '';
      res.on('data', chunk => data += chunk);
      res.on('end', () => {
        try {
          resolve({ status: res.statusCode, data: JSON.parse(data) });
        } catch (e) {
          resolve({ status: res.statusCode, data: data });
        }
      });
    });

    req.on('error', (err) => {
      console.error('[WX] ' + logTag + method + ' ' + url.pathname + ' error: ' + err.message);
      reject(err);
    });
    req.on('timeout', () => {
      req.destroy();
      reject(new Error('Request timeout'));
    });

    if (body) {
      const bodyStr = typeof body === 'string' ? body : JSON.stringify(body);
      req.setHeader('Content-Length', Buffer.byteLength(bodyStr));
      req.write(bodyStr);
    }
    req.end();
  });
}

function getAuthHeaders() {
  if (!credentials) return {};
  return {
    'Content-Type': 'application/json',
    'AuthorizationType': 'ilink_bot_token',
    'Authorization': 'Bearer ' + credentials.bot_token,
    'X-WECHAT-UIN': randomWechatUIN(),
    'iLink-App-ClientVersion': '1'
  };
}

function getBaseUrl() {
  return (credentials && credentials.base_url) || DEFAULT_BASE_URL;
}

// ---------------------------------------------------------------------------
// Credentials management
// ---------------------------------------------------------------------------

function loadCredentials() {
  try {
    if (fs.existsSync(CREDENTIALS_FILE)) {
      const data = JSON.parse(fs.readFileSync(CREDENTIALS_FILE, 'utf8'));
      if (data.bot_token && data.ilink_bot_id) {
        credentials = data;
        getUpdatesBuf = data.get_updates_buf || '';
        console.log('[WX] Credentials loaded from file');
        return true;
      }
    }
  } catch (e) {
    console.error('[WX] Failed to load credentials:', e.message);
  }
  return false;
}

function saveCredentials() {
  try {
    const data = Object.assign({}, credentials, { get_updates_buf: getUpdatesBuf });
    fs.writeFileSync(CREDENTIALS_FILE, JSON.stringify(data, null, 2), 'utf8');
  } catch (e) {
    console.error('[WX] Failed to save credentials:', e.message);
  }
}

// ---------------------------------------------------------------------------
// Push wechat status / qrcode to connected agents
// ---------------------------------------------------------------------------

function pushWxStatus(status, extra) {
  const data = Object.assign({ status: status }, extra || {});
  for (const ws of agentClients) {
    send(ws, { type: 'wx_status', data: data });
  }
}

function pushWxQrcode(qrcodeKey, qrcodeImg) {
  for (const ws of agentClients) {
    send(ws, { type: 'wx_qrcode', data: { qrcode_key: qrcodeKey, qrcode_img: qrcodeImg } });
  }
}

// ---------------------------------------------------------------------------
// QR Code Login
// ---------------------------------------------------------------------------

async function getQRCode() {
  const url = getBaseUrl() + '/ilink/bot/getbotqrcode?bot_type=3';
  const headers = { 'iLink-App-ClientVersion': '1' };
  const res = await httpsRequest('GET', url, headers, null, 15000, 'QR');
  return res.data;
}

async function pollQRCodeStatus(qrcodeKey) {
  const url = getBaseUrl() + '/ilink/bot/getqrcodestatus?qrcode=' + encodeURIComponent(qrcodeKey);
  const headers = { 'iLink-App-ClientVersion': '1' };
  const res = await httpsRequest('GET', url, headers, null, QR_POLL_TIMEOUT_MS, 'QR-poll');
  return res.data;
}

async function loginFlow() {
  console.log('[WX] Starting login flow...');
  pushWxStatus('login_started');

  const startTime = Date.now();
  let refreshCount = 0;

  while (Date.now() - startTime < QR_TOTAL_TIMEOUT_MS) {
    let qrData;
    try {
      qrData = await getQRCode();
    } catch (e) {
      console.error('[WX] Failed to get QR code:', e.message);
      pushWxStatus('error', { message: 'Failed to get QR code: ' + e.message });
      await sleep(5000);
      continue;
    }

    if (!qrData || !qrData.qrcode) {
      console.error('[WX] Invalid QR code response');
      pushWxStatus('error', { message: 'Invalid QR code response' });
      await sleep(5000);
      continue;
    }

    const qrcodeKey = qrData.qrcode;
    const qrcodeImg = qrData.qrcode_img_content || '';

    console.log('[WX] QR code obtained, key:', qrcodeKey);
    pushWxQrcode(qrcodeKey, qrcodeImg);

    let expired = false;
    while (!expired && Date.now() - startTime < QR_TOTAL_TIMEOUT_MS) {
      try {
        const statusData = await pollQRCodeStatus(qrcodeKey);
        const status = statusData.status || statusData.Status || '';

        if (status === 'confirmed') {
          credentials = {
            bot_token: statusData.bottoken || statusData.bot_token || '',
            ilink_bot_id: statusData.ilinkbotid || statusData.ilink_bot_id || '',
            base_url: statusData.baseurl || statusData.base_url || DEFAULT_BASE_URL,
            ilink_user_id: statusData.ilinkuser_id || statusData.ilink_user_id || ''
          };
          getUpdatesBuf = '';
          saveCredentials();
          console.log('[WX] Login successful!');
          pushWxStatus('logged_in');
          return true;
        } else if (status === 'scaned') {
          pushWxStatus('scanned');
        } else if (status === 'expired') {
          expired = true;
          refreshCount++;
          if (refreshCount >= QR_MAX_REFRESH) {
            pushWxStatus('error', { message: 'QR code expired, max refresh reached' });
            return false;
          }
          console.log('[WX] QR code expired, refreshing...');
          pushWxStatus('qr_expired_refreshing');
        }
        // status 'wait' -> keep polling
      } catch (e) {
        if (e.message === 'Request timeout') {
          continue;
        }
        console.error('[WX] QR poll error:', e.message);
        await sleep(3000);
      }
    }
  }

  console.log('[WX] Login timeout');
  pushWxStatus('error', { message: 'Login timeout (8 minutes)' });
  return false;
}

// ---------------------------------------------------------------------------
// Message Polling
// ---------------------------------------------------------------------------

async function pollMessages() {
  if (polling) return;
  if (!credentials) {
    console.log('[WX] No credentials, cannot poll');
    return;
  }

  polling = true;
  console.log('[WX] Starting message polling...');
  pushWxStatus('polling_started');

  while (polling && credentials) {
    try {
      const url = getBaseUrl() + '/ilink/bot/getupdates';
      const body = {
        get_updates_buf: getUpdatesBuf,
        base_info: { channel_version: CHANNEL_VERSION }
      };
      const res = await httpsRequest('POST', url, getAuthHeaders(), body, POLL_TIMEOUT_MS, 'poll');
      const data = res.data;

      if (typeof data !== 'object') {
        console.error('[WX] Invalid poll response (status=' + res.status + '):', shortText(data, 200));
        await sleep(5000);
        continue;
      }

      // Session expired: clear credentials and auto re-login
      if (data.errcode === -14) {
        console.log('[WX] Session expired, need re-login');
        credentials = null;
        polling = false;
        pushWxStatus('session_expired');
        try { fs.unlinkSync(CREDENTIALS_FILE); } catch (e) { }
        setTimeout(() => {
          loginFlow().then(success => {
            if (success) startPolling();
          }).catch(err => console.error('[WX] Auto re-login failed:', err));
        }, 3000);
        return;
      }

      // Update cursor
      if (data.get_updates_buf) {
        getUpdatesBuf = data.get_updates_buf;
        saveCredentials();
      }

      const msgs = data.msgs || [];
      for (const msg of msgs) {
        processIncomingMessage(msg);
      }
    } catch (e) {
      if (e.message === 'Request timeout') {
        continue;
      }
      console.error('[WX] Poll error:', e.message);
      await sleep(5000);
    }
  }

  console.log('[WX] Message polling stopped');
}

function processIncomingMessage(msg) {
  // Only process user text messages (message_type=1)
  if (msg.message_type !== 1) return;

  const fromUser = msg.from_user_id || msg.fromuserid || '';
  const contextToken = msg.context_token || '';

  if (fromUser && contextToken) {
    contextTokenMap[fromUser] = contextToken;
  }

  let text = '';
  const items = msg.item_list || msg.itemlist || [];
  for (const item of items) {
    if (item.type === 1 && (item.text_item || item.textitem)) {
      text += (item.text_item || item.textitem).text || '';
    }
  }

  if (!text) return;

  console.log('[WX] Message from', fromUser, ':', text.substring(0, 50));

  // R4: parse leading / commands for route control (wechat -> ws only).
  if (text.startsWith('/')) {
    const trimmed = text.trim();
    const sp = trimmed.indexOf(' ');
    const cmd = sp >= 0 ? trimmed.substring(0, sp) : trimmed;
    const arg = sp >= 0 ? trimmed.substring(sp + 1).trim() : '';
    if (cmd === '/solo' || cmd === '/chat') {
      let feedback;
      if (arg) {
        wxRouteMode = 'solo';
        wxRouteTarget = arg;
        console.log('[WX] route set to solo:', arg);
        feedback = 'route set: solo ' + arg + (nameToClient.has(arg) ? '' : ' (target agent not connected)');
      } else {
        wxRouteMode = 'broadcast';
        wxRouteTarget = null;
        console.log('[WX] route reset to broadcast');
        feedback = 'route reset: broadcast';
      }
      forwardToWechat(feedback, fromUser);
      return;
    }
    console.log('[WX] unknown command forwarded:', cmd);
  }

  // channelId for agent routing is the wechat userId itself.
  channelSessions.set(fromUser, { userId: fromUser, lastSeen: Date.now() });
  pushToAgent(text, fromUser);
}

// ---------------------------------------------------------------------------
// Send Message
// ---------------------------------------------------------------------------

async function sendTextMessage(toUserId, text, contextToken) {
  if (!credentials) {
    console.error('[WX] Cannot send: no credentials');
    return { success: false, error: 'No credentials' };
  }

  const url = getBaseUrl() + '/ilink/bot/sendmessage';
  const clientId = crypto.randomUUID ? crypto.randomUUID() :
    crypto.randomBytes(16).toString('hex');

  const body = {
    msg: {
      to_user_id: toUserId,
      client_id: clientId,
      message_type: 2,
      message_state: 2,
      item_list: [
        { type: 1, text_item: { text: text } }
      ],
      context_token: contextToken || contextTokenMap[toUserId] || ''
    },
    base_info: { channel_version: CHANNEL_VERSION }
  };

  try {
    const res = await httpsRequest('POST', url, getAuthHeaders(), body, 15000, 'send');
    console.log('[WX] Send result: status=' + res.status + ' data=' + shortText(JSON.stringify(res.data), 200));
    return { success: true, data: res.data };
  } catch (e) {
    console.error('[WX] Send error:', e.message);
    return { success: false, error: e.message };
  }
}

function startPolling() {
  if (!polling) {
    pollMessages().catch(e => {
      console.error('[WX] Poll loop error:', e.message);
      polling = false;
    });
  }
}

// ---------------------------------------------------------------------------
// Route agent reply to wechat by channelId (= wechat userId)
// ---------------------------------------------------------------------------

function forwardToWechat(content, channelId) {
  if (!credentials) {
    console.error('[wechat] Cannot forward: wechat not logged in');
    return;
  }
  if (!channelId) {
    console.error('[wechat] Cannot forward: no channelId');
    return;
  }
  const session = channelSessions.get(channelId);
  const toUser = (session && session.userId) || channelId;
  sendTextMessage(toUser, String(content)).then(result => {
    if (!result.success) {
      console.error('[wechat] Forward failed:', result.error);
    }
  });
}

function handleAgentMessage(ws, msg) {
  const content = msg.content || msg.text;
  const channelId = msg.channelId;
  if (channelId) {
    channelSessions.set(channelId, { lastSeen: Date.now() });
  }
  if (msg.requestId) {
    // Chat test round trip: echo back so relay_panel Send works end to end.
    send(ws, {
      type: 'message',
      content: 'echo: ' + content,
      requestId: msg.requestId,
      channelId: channelId
    });
  } else {
    // Push from LLM (pushMessage, no requestId): route to wechat.
    forwardToWechat(String(content), channelId);
  }
}

// Handle wx_login command from an agent client.
function handleWxLogin(ws, msg) {
  if (msg.requestId) {
    send(ws, { type: 'message', content: 'wx_login accepted', requestId: msg.requestId });
  }
  if (credentials && polling) {
    pushWxStatus('already_logged_in');
  } else if (credentials) {
    console.log('[ws] wx_login: saved credentials found, starting polling');
    startPolling();
    pushWxStatus('logged_in');
  } else {
    console.log('[ws] wx_login: no credentials, starting login flow');
    loginFlow().then(success => {
      if (success) startPolling();
    }).catch(err => console.error('[WX] Login flow failed:', err));
  }
}

// Handle wx_logout command from an agent client.
function handleWxLogout(ws, msg) {
  console.log('[ws] wx_logout: stopping polling and clearing credentials');
  polling = false;
  credentials = null;
  getUpdatesBuf = '';
  contextTokenMap = {};
  try {
    if (fs.existsSync(CREDENTIALS_FILE)) {
      fs.unlinkSync(CREDENTIALS_FILE);
    }
  } catch (err) {
    console.error('[WX] Failed to remove credentials file:', err.message);
  }
  pushWxStatus('logged_out');
  if (msg.requestId) {
    send(ws, { type: 'message', content: 'wx_logout done', requestId: msg.requestId });
  }
}

// Handle wx_status_request command: reply with current wechat state.
function handleWxStatusRequest(ws, msg) {
  send(ws, {
    type: 'wx_status',
    data: {
      status: credentials ? 'logged_in' : 'not_logged_in',
      polling: polling
    }
  });
}

// ---------------------------------------------------------------------------
// HTTP + WebSocket server
// ---------------------------------------------------------------------------

const server = http.createServer((req, res) => {
  res.writeHead(200, { 'Content-Type': 'text/plain' });
  res.end('wechat-bridge-for-agent\n');
});

const wss = new WebSocketServer({ server: server });

wss.on('connection', (ws, req) => {
  console.log('[ws] client connected from ' + req.socket.remoteAddress);
  agentClients.add(ws);

  ws.on('message', (raw) => {
    let msg = null;
    try {
      msg = JSON.parse(raw.toString());
    } catch (e) {
      console.log('[ws] invalid json ignored');
      return;
    }
    switch (msg.type) {
      case 'auth':
        // Lenient auth by design: accept any token, the client does not
        // validate the auth_response body either.
        // R3: register per-connection clientId, new connection wins on conflict.
        if (msg.clientId) {
          agentClientIds.set(String(msg.clientId), ws);
        }
        send(ws, { type: 'auth_response', data: { name: 'wechat-bridge-for-agent', clientId: msg.clientId || null } });
        console.log('[ws] auth ok' + (msg.clientId ? ' clientId=' + msg.clientId : ''));
        break;
      case 'ping':
        send(ws, { type: 'pong' });
        break;
      case 'register':
        // R5: agent page registers a human readable name for routing.
        if (msg.name) {
          const regName = String(msg.name);
          const prevWs = nameToClient.get(regName);
          if (prevWs && prevWs !== ws) {
            agentClientNames.delete(prevWs);
          }
          agentClientNames.set(ws, regName);
          nameToClient.set(regName, ws);
          console.log('[ws] registered name=' + regName);
        }
        send(ws, { type: 'register_response', data: { name: msg.name || null }, requestId: msg.requestId });
        break;
      case 'wx_login':
        handleWxLogin(ws, msg);
        break;
      case 'wx_logout':
        handleWxLogout(ws, msg);
        break;
      case 'wx_status_request':
        handleWxStatusRequest(ws, msg);
        break;
      case 'message':
        handleAgentMessage(ws, msg);
        break;
      default:
        if (msg.requestId) {
          send(ws, {
            type: 'message',
            content: 'unsupported type: ' + String(msg.type),
            requestId: msg.requestId
          });
        }
        console.log('[ws] unhandled type: ' + msg.type);
    }
  });

  ws.on('close', () => {
    agentClients.delete(ws);
    for (const [cid, w] of agentClientIds) {
      if (w === ws) agentClientIds.delete(cid);
    }
    const closedName = agentClientNames.get(ws);
    if (closedName && nameToClient.get(closedName) === ws) {
      nameToClient.delete(closedName);
    }
    agentClientNames.delete(ws);
    console.log('[ws] client closed');
  });

  ws.on('error', (err) => {
    console.log('[ws] client error: ' + err.message);
  });
});

server.listen(PORT, () => {
  console.log('wechat-bridge-for-agent listening on ws://localhost:' + PORT);
  // Auto-start wechat layer: saved credentials -> polling, else login flow.
  if (loadCredentials()) {
    console.log('[WX] Auto-starting message polling with saved credentials');
    startPolling();
  } else {
    console.log('[WX] No saved credentials, starting login flow...');
    loginFlow().then(success => {
      if (success) startPolling();
    }).catch(err => console.error('[WX] Login flow failed:', err));
  }
});
