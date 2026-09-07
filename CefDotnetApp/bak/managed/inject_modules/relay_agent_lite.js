// relay_agent_lite.js - lightweight relay client for single-page agent sites.
// Standalone script: no global logger/CONFIG/secretStore/metadslWorker dependencies.
// Config is persisted in localStorage as JSON under 'relay_lite_config'.
(function () {
  'use strict';

  var CONFIG_KEY = 'relay_lite_config';
  var CLIENT_ID_KEY = 'relay_lite_client_id';
  var DEFAULTS = {
    wsUrl: 'ws://localhost:3000/ws',
    token: 'lite-token',
    name: 'webagent'
  };

  function loadConfig() {
    try {
      var raw = localStorage.getItem(CONFIG_KEY);
      if (raw) {
        var obj = JSON.parse(raw);
        var cfg = {};
        for (var k in DEFAULTS) {
          cfg[k] = obj[k] || DEFAULTS[k];
        }
        return cfg;
      }
    } catch (e) { /* fall back to defaults */ }
    return JSON.parse(JSON.stringify(DEFAULTS));
  }

  function saveConfig(cfg) {
    try {
      localStorage.setItem(CONFIG_KEY, JSON.stringify(cfg));
    } catch (e) { /* storage may be unavailable */ }
  }

  function getClientId() {
    var id = null;
    try {
      id = localStorage.getItem(CLIENT_ID_KEY);
      if (!id) {
        id = 'lite-' + Date.now().toString(36) + '-' + Math.random().toString(36).slice(2, 10);
        localStorage.setItem(CLIENT_ID_KEY, id);
      }
    } catch (e) {
      id = 'lite-' + Date.now().toString(36);
    }
    return id;
  }

  var config = loadConfig();
  var clientId = getClientId();
  var ws = null;
  var pingTimer = null;
  var reconnectTimer = null;
  var reconnectDelay = 3000;
  var reqN = 0;
  var manualClose = false;
  var badgeEl = null;
  var pendingReply = false;
  var pendingChannelId = null;
  var pendingContextToken = null;
  var pendingReplyRounds = 0; // 10-round budget decremented per successful sendReply

  function log() {
    try {
      var args = ['[relay-lite]'].concat(Array.prototype.slice.call(arguments));
      console.log.apply(console, args);
    } catch (e) { /* ignore */ }
  }

  function setBadge(state) {
    try {
      if (!badgeEl) {
        badgeEl = document.createElement('div');
        badgeEl.id = 'relay-lite-badge';
        badgeEl.style.cssText = 'position:fixed;top:8px;right:8px;z-index:2147483647;padding:4px 10px;border-radius:12px;color:#fff;font:12px/1.4 sans-serif;cursor:pointer;box-shadow:0 1px 4px rgba(0,0,0,0.4);user-select:none;background-color:#999999;';
        badgeEl.textContent = 'relay';
        badgeEl.addEventListener('click', function () {
          if (!window.relayLite) return;
          if (manualClose) {
            window.relayLite.connect();
          } else {
            window.relayLite.disconnect();
          }
        });
        (document.body || document.documentElement).appendChild(badgeEl);
      }
      var colors = { connecting: '#e6a700', connected: '#2e9e44', disconnected: '#999999' };
      badgeEl.style.backgroundColor = colors[state] || colors.disconnected;
      badgeEl.setAttribute('data-state', state);
      badgeEl.title = 'relay-lite: ' + state + ' (click to toggle connect)';
    } catch (e) { /* badge is best-effort */ }
  }

  // Phase 2 gate chain: bridge exists -> sendChat.
  function handleAgentMessage(text) {
    if (!text) return;
    var bridge = window.MetaDSLBridge;
    if (!bridge || typeof bridge.sendChat !== 'function') {
      log('MetaDSLBridge not available, message dropped');
      return;
    }
    try {
      bridge.sendChat(text, true);
      pendingReply = true; // reply expected: adapters call sendReply after this
      pendingReplyRounds = 10; // reset round budget on each incoming message
      log('message dispatched to page via sendChat');
    } catch (e) {
      log('sendChat failed:', e);
    }
  }

  function nextId() {
    reqN += 1;
    return 'req' + reqN;
  }

  function send(obj) {
    if (ws && ws.readyState === WebSocket.OPEN) {
      ws.send(JSON.stringify(obj));
      return true;
    }
    return false;
  }

  // Reply pass-through: adapters call sendReply(assistantText) once an
  // agent message was dispatched, so the answer flows back to the bridge.
  function sendReply(text) {
    if (!pendingReply) return false;
    var ch = pendingChannelId;
    var tok = pendingContextToken;
    if (text === undefined || text === null) text = '';
    var payload = { type: 'wx_reply', data: { to_user: ch, text: String(text), context_token: tok } };
    var ok = send(payload);
    if (ok) {
      pendingReplyRounds = Math.max(0, pendingReplyRounds - 1);
      if (pendingReplyRounds === 0) {
        pendingReply = false;
        pendingChannelId = null;
        pendingContextToken = null;
      }
    }
    log('sendReply', ok ? 'sent' : 'ws not open, reply kept pending');
    return ok;
  }

  function startPing() {
    stopPing();
    pingTimer = setInterval(function () {
      send({ type: 'ping' });
    }, 25000);
  }

  function stopPing() {
    if (pingTimer) {
      clearInterval(pingTimer);
      pingTimer = null;
    }
  }

  function scheduleReconnect() {
    if (reconnectTimer) return;
    reconnectTimer = setTimeout(function () {
      reconnectTimer = null;
      connect();
    }, reconnectDelay);
  }

  function connect() {
    if (ws) {
      var old = ws;
      old.onclose = null; // avoid stale handler scheduling a duplicate reconnect
      try { old.close(); } catch (e) { /* ignore */ }
      ws = null;
    }
    manualClose = false;
    setBadge('connecting');
    log('connecting to', config.wsUrl);
    ws = new WebSocket(config.wsUrl);

    ws.onopen = function () {
      log('connected');
      setBadge('connected');
      send({ type: 'auth', token: config.token, clientId: clientId });
      var regName = (typeof window.agentId === 'string' && window.agentId) ? window.agentId : config.name;
      send({ type: 'register', name: regName, requestId: nextId() });
      startPing();
    };

    ws.onmessage = function (event) {
      var msg = null;
      try {
        msg = JSON.parse(event.data);
      } catch (e) {
        return;
      }
      if (!msg || !msg.type) return;
      switch (msg.type) {
        case 'auth_response':
          log('auth ok, clientId =', (msg.data && msg.data.clientId) || clientId);
          break;
        case 'register_response':
          log('register ok, name =', (msg.data && msg.data.name) || config.name);
          break;
        case 'pong':
          break;
        case 'message':
          var mCh = (msg.channelId !== undefined && msg.channelId !== null) ? msg.channelId : msg.from_user;
          pendingChannelId = (mCh !== undefined && mCh !== null) ? mCh : null;
          var mText = (msg.content !== undefined && msg.content !== null) ? msg.content : msg.text;
          if (msg.context_token !== undefined && msg.context_token !== null) pendingContextToken = msg.context_token;
          handleAgentMessage(mText);
          break;
        case 'wx_qrcode':
        case 'wx_status':
          log(msg.type, msg.data);
          break;
        default:
          log('unhandled:', msg.type);
      }
    };

    ws.onclose = function () {
      stopPing();
      setBadge('disconnected');
      if (manualClose) {
        log('disconnected (manual), reconnect suppressed');
        return;
      }
      log('disconnected, retry in', reconnectDelay, 'ms');
      scheduleReconnect();
    };

    ws.onerror = function () {
      log('socket error');
    };
  }

  // Public helper for console debugging and phase 2 UI.
  window.relayLite = {
    connect: connect,
    disconnect: function () {
      manualClose = true;
      if (reconnectTimer) {
        clearTimeout(reconnectTimer);
        reconnectTimer = null;
      }
      stopPing();
      if (ws) {
        var cur = ws;
        cur.onclose = null;
        try { cur.close(); } catch (e) { /* ignore */ }
        ws = null;
      }
      setBadge('disconnected');
    },
    getConfig: function () {
      return JSON.parse(JSON.stringify(config));
    },
    setConfig: function (patch) {
      var p = patch || {};
      for (var k in DEFAULTS) {
        if (p[k] !== undefined) config[k] = p[k];
      }
      saveConfig(config);
    },
    send: send,
    sendReply: sendReply
  };

  // Auto-start on injection.
  connect();
})();
