// ============================================================================
// WebSocket Worker Code - Externalized for better maintainability
// This code runs in a Web Worker context
// ============================================================================
const WEBSOCKET_WORKER_CODE = `
    // Worker internal state
    let ws = null;
    let isConnected = false;
    let reconnectAttempts = 0;
    const MAX_RECONNECT = 5;

    // Worker queues (dual queue mechanism)
    const toMainQueue = [];     // Messages to send to main thread
    const fromMainQueue = [];   // Messages received from main thread

    // Tick processing for queues
    let tickInterval = null;

    function log(level, message, data) {
      self.postMessage({
        type: 'log',
        level: level,
        message: message,
        data: data
      });
    }

    function connect(port) {
      const wsUrl = 'ws://localhost:' + port + '/';
      log('info', 'Connecting to WebSocket server: ' + wsUrl);

      try {
        ws = new WebSocket(wsUrl);

        ws.onopen = function() {
          isConnected = true;
          reconnectAttempts = 0;
          log('info', 'WebSocket connected to ' + wsUrl);
          self.postMessage({ type: 'connected' });
        };

        ws.onclose = function() {
          isConnected = false;
          log('info', 'WebSocket disconnected');
          self.postMessage({ type: 'disconnected' });

          // Attempt reconnection
          if (reconnectAttempts < MAX_RECONNECT) {
            reconnectAttempts++;
            log('info', 'Reconnecting... attempt ' + reconnectAttempts);
            setTimeout(function() { connect(port); }, 3000);
          }
        };

        ws.onerror = function(error) {
          log('error', 'WebSocket error', error.message || 'connection error');
          self.postMessage({ type: 'error', error: 'WebSocket error' });
        };

        ws.onmessage = function(event) {
          // Queue message to main thread
          toMainQueue.push(event.data);
          log('info', 'Message received from server, queued to main (length: ' + event.data.length + ')');
        };
      } catch (e) {
        log('error', 'Failed to create WebSocket', { error: e.message });
      }
    }

    function disconnect() {
      if (ws) {
        try {
          ws.close();
        } catch (e) {}
        ws = null;
      }
      isConnected = false;
      if (tickInterval) {
        clearInterval(tickInterval);
        tickInterval = null;
      }
      log('info', 'WebSocket disconnected by request');
    }

    function processQueues() {
      if (!isConnected) return;

      // Send one message per tick to server (from fromMainQueue)
      if (fromMainQueue.length > 0) {
        const msg = fromMainQueue.shift();
        if (ws && ws.readyState === WebSocket.OPEN) {
          ws.send(msg);
          log('info', 'Message sent to server (length: ' + msg.length + '): ' + msg.substring(0, 100) + '...');
        } else {
          log('warn', 'Cannot send message, WebSocket not open (state: ' + (ws ? ws.readyState : 'null') + ')');
          // Put message back to queue
          fromMainQueue.unshift(msg);
        }
      }
    }

    // Handle messages from main thread
    self.onmessage = function(event) {
      const data = event.data;

      if (data.type === 'start') {
        connect(data.port);
        // Start tick processing
        tickInterval = setInterval(processQueues, 100);
      } else if (data.type === 'stop') {
        disconnect();
      } else if (data.type === 'send') {
        // Queue message to send to server
        fromMainQueue.push(data.message);
        log('info', 'Message queued from main thread (length: ' + data.message.length + ')');
      }
    };

    // Send queued messages to main thread on each tick
    setInterval(function() {
      while (toMainQueue.length > 0) {
        const msg = toMainQueue.shift();
        self.postMessage({
          type: 'message',
          data: msg
        });
      }
    }, 100);
  `;
