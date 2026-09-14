// relay_agent_lite.js - WeChat bridge relay for single-page agent sites.
//
// No page-side WebSocket anymore: the browser process owns the upstream
// connection (wsclient id "wxb_<agentId>") to the WeChat bridge. This module
// only:
//   - asks the browser to open it (cefQuery action "lite_open"),
//   - receives bridge pushes through the generic onAgentEvent dispatcher by
//     registering a pseudo-slot in relaySlotRegistry (same table the
//     execution slots use; pagehide closes us for free via slot.close()),
//   - sends replies through the existing agent_send action, normalized to
//     the bridge reply shape {type:'message', content, channelId}
//     (the old wx_reply shape was never handled by the bridge).
//
// Requires relay_transport.js on the page (urlKey + relaySlotRegistry).
(function () {
  'use strict';

  var agentId = (typeof window.agentId === 'string' && window.agentId) ? window.agentId : 'webagent';
  var clientId = null;          // the "wxb_<agentId>" wsclient id
  var pendingChannelId = null;  // WeChat userId of the last inbound message
  var pendingContextToken = null;
  var badgeEl = null;
  var manualClose = false;
  var reconnectTimer = null;

  function log() {
    try {
      console.log.apply(console, ['[relay-lite]'].concat(Array.prototype.slice.call(arguments)));
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

  // ---- inbound: bridge pushes arrive as pseudo-slot _onPush payloads ----

  function dispatchToPage(text) {
    if (!text) return;
    var bridge = window.MetaDSLBridge;
    if (!bridge || typeof bridge.sendChat !== 'function') {
      log('MetaDSLBridge not available, message dropped');
      return;
    }
    try {
      bridge.sendChat(text, true);
      log('message dispatched to page via sendChat');
    } catch (e) {
      log('sendChat failed:', e);
    }
  }

  function handlePush(text) {
    var msg = null;
    try {
      msg = JSON.parse(text);
    } catch (e) {
      return;
    }
    if (!msg || !msg.type) return;
    switch (msg.type) {
      case 'relay_state':
        // Browser-side connection state for our wxb_ connection.
        setBadge(msg.state === 'connected' ? 'connected' : 'disconnected');
        if ((msg.state === 'disconnected' || msg.state === 'failed') && !manualClose) {
          scheduleReconnect();
        }
        break;
      case 'message':
        var ch = (msg.channelId !== undefined && msg.channelId !== null) ? msg.channelId : msg.from_user;
        pendingChannelId = (ch !== undefined && ch !== null) ? ch : null;
        if (msg.context_token !== undefined && msg.context_token !== null) {
          pendingContextToken = msg.context_token;
        }
        dispatchToPage((msg.content !== undefined && msg.content !== null) ? msg.content : msg.text);
        break;
      case 'wx_qrcode':
      case 'wx_status':
        log(msg.type, msg.data);
        break;
      default:
        log('unhandled:', msg.type);
    }
  }

  // ---- cefQuery ----

  function query(request, onSuccess, onFailure) {
    if (typeof window !== 'undefined' && typeof window.cefQuery !== 'function') {
      log('cefQuery unavailable, request dropped:', request.action);
      if (onFailure) onFailure(-1, 'cefQuery unavailable');
      return false;
    }
    window.cefQuery({
      request: JSON.stringify(request),
      onSuccess: function (response) { if (onSuccess) onSuccess(response); },
      onFailure: function (code, msgText) { if (onFailure) onFailure(code, msgText); }
    });
    return true;
  }

  // ---- lifecycle ----

  function scheduleReconnect() {
    if (reconnectTimer || manualClose) return;
    reconnectTimer = setTimeout(function () {
      reconnectTimer = null;
      connect();
    }, 3000);
  }

  function connect() {
    // relay_transport.js must be loaded first (urlKey + slot registry).
    if (typeof relayTransport === 'undefined' || !relayTransport.urlKey) {
      setTimeout(connect, 500);
      return;
    }
    if (typeof relaySlotRegistry === 'undefined') {
      setTimeout(connect, 500);
      return;
    }
    manualClose = false;
    setBadge('connecting');
    log('opening bridge upstream for', agentId);
    var cid = 'wxb_' + agentId;
    query({ action: 'lite_open', connId: cid, urlKey: relayTransport.urlKey }, function (id) {
      clientId = id;
      // Register as a pseudo-slot: the generic onAgentEvent dispatcher routes
      // our wxb_ pushes here, and the shared pagehide cleanup calls close().
      relaySlotRegistry.set(id, {
        _onPush: function (message) { handlePush(message); },
        close: function () { disconnect(); }
      });
      // Bridge handshake (the wire protocol lives here, not in the dsl):
      // lenient auth + name registration so the bridge routes by name
      // (broadcast / /solo <name>).
      query({ action: 'agent_send', clientId: id, message: JSON.stringify({ type: 'auth', token: 'relay-lite', clientId: id }) });
      query({ action: 'agent_send', clientId: id, message: JSON.stringify({ type: 'register', name: agentId }) });
      log('bridge upstream ready:', id);
      setBadge('connected');
    }, function (code, msgText) {
      log('lite_open failed (' + code + '): ' + msgText);
      setBadge('disconnected');
      scheduleReconnect();
    });
  }

  function disconnect() {
    manualClose = true;
    if (reconnectTimer) {
      clearTimeout(reconnectTimer);
      reconnectTimer = null;
    }
    if (clientId) {
      if (typeof relaySlotRegistry !== 'undefined') {
        relaySlotRegistry.delete(clientId);
      }
      // The generic agent_unregister action closes the wxb_ connection and
      // the browser cleans its relay tables; the bridge drops the name
      // registration when the connection closes.
      query({ action: 'agent_unregister', clientId: clientId });
      clientId = null;
    }
    setBadge('disconnected');
  }

  // ---- replies: normalized to the bridge's supported shape ----

  function sendReply(text) {
    if (text === undefined || text === null) text = '';
    if (!clientId || pendingChannelId === null) return false;
    var payload = {
      type: 'message',
      content: String(text),
      channelId: pendingChannelId
    };
    return query({ action: 'agent_send', clientId: clientId, message: JSON.stringify(payload) });
  }

  // Public surface: the adapters only use sendReply; connect/disconnect
  // drive the badge toggle and manual control.
  window.relayLite = {
    connect: connect,
    disconnect: disconnect,
    sendReply: sendReply
  };

  // Auto-start once relay_transport is on the page.
  connect();
})();
