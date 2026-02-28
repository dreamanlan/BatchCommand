  // ============================================================================
  // MetaDSLWorkerManager - Dual queue communication with WebSocket for MetaDSL
  // ============================================================================
  class MetaDSLWorkerManager {
    constructor() {
      this.logger = logger.createLogger('MetaDSLWorker');
      this.worker = null;
      this.isRunning = false;
      this.port = CONFIG.websocketPort;
      // Main thread queues
      this.toWorkerQueue = [];      // Messages to send to worker
      this.fromWorkerQueue = [];    // Messages received from worker
      this.tickInterval = null;
      // Auto-reconnect
      this.autoReconnect = true;
      this.isConnected = false;
      this.firstStart = true;       // Track first start for auto-connect
    }

    // Start WebSocket Worker
    start(port) {
      if (port) {
        this.port = port;
      }

      if (this.isRunning) {
        this.logger.warn('MetaDSL Worker is already running');
        return false;
      }

      try {
        // Create worker using externalized code constant
        // const blob = new Blob([WEBSOCKET_WORKER_CODE], { type: 'application/javascript' });
        // const workerUrl = URL.createObjectURL(blob);

        // Blob URL doesn't work with file:// protocol due to security restrictions
        const workerUrl = 'data:application/javascript;charset=utf-8,' + encodeURIComponent(WEBSOCKET_WORKER_CODE);
        // Create worker using Data URL (compatible with file:// protocol)
        this.worker = new Worker(workerUrl);

        // Set up message handler
        this.worker.onmessage = (event) => {
          this.handleWorkerMessage(event.data);
        };

        // Start worker with port
        this.worker.postMessage({ type: 'start', port: this.port });

        // Start main thread tick processing
        this.startTick();

        this.isRunning = true;
        this.firstStart = false;
        this.logger.info('MetaDSL Worker started on port ' + this.port);
        return true;
      } catch (e) {
        this.logger.error('Failed to start MetaDSL Worker: ' + e.message);
        // Schedule reconnect if autoReconnect is enabled
        if (this.autoReconnect) {
          this.scheduleReconnect();
        }
        return false;
      }
    }

    // Stop WebSocket Worker
    stop() {
      if (!this.isRunning) {
        return;
      }

      this.stopTick();

      if (this.worker) {
        this.worker.postMessage({ type: 'stop' });
        setTimeout(() => {
          if (this.worker) {
            this.worker.terminate();
            this.worker = null;
          }
        }, 500);
      }

      // Clear queues
      this.toWorkerQueue = [];
      this.fromWorkerQueue = [];

      this.isRunning = false;
      this.logger.info('WebSocket Worker stopped');
    }

    // Start main thread tick processing
    startTick() {
      this.tickInterval = setInterval(() => {
        this.processQueues();
      }, CONFIG.sendMessageDelay);
    }

    // Stop main thread tick processing
    stopTick() {
      if (this.tickInterval) {
        clearInterval(this.tickInterval);
        this.tickInterval = null;
      }
    }

    // Schedule automatic reconnect
    scheduleReconnect() {
      if (this.reconnectTimeout) {
        clearTimeout(this.reconnectTimeout);
      }
      this.logger.info('Scheduling reconnect in 3 seconds...');
      this.reconnectTimeout = setTimeout(() => {
        this.logger.info('Attempting automatic reconnect...');
        this.stopInternal();
        this.start();
      }, 3000);
    }

    // Stop internal state without logging
    stopInternal() {
      this.stopTick();

      if (this.worker) {
        this.worker.postMessage({ type: 'stop' });
        this.worker.terminate();
        this.worker = null;
      }

      this.isRunning = false;
      this.isConnected = false;
    }

    // Process main thread queues
    processQueues() {
      // Send one message per tick to worker
      if (this.toWorkerQueue.length > 0 && this.worker) {
        const msg = this.toWorkerQueue.shift();
        this.logger.info('Sending message to worker (length: ' + msg.length + '): ' + msg.substring(0, 100) + '...');
        this.worker.postMessage({ type: 'send', message: msg });
      }
    }

    // Handle messages from worker
    handleWorkerMessage(data) {
      if (data.type === 'message') {
        // Queue message from worker
        this.fromWorkerQueue.push(data.data);
        this.logger.info('Message from worker queued (length: ' + data.data.length + '): ' + data.data.substring(0, 100) + '...');
      } else if (data.type === 'connected') {
        this.isConnected = true;
        this.logger.info('MetaDSL Worker connected to server');
      } else if (data.type === 'disconnected') {
        this.isConnected = false;
        this.logger.warn('MetaDSL Worker disconnected from server');
        // Auto-reconnect if enabled
        if (this.autoReconnect) {
          this.scheduleReconnect();
        }
      } else if (data.type === 'error') {
        this.isConnected = false;
        this.logger.error('MetaDSL Worker error: ' + data.error);
        // Auto-reconnect if enabled
        if (this.autoReconnect) {
          this.scheduleReconnect();
        }
      } else if (data.type === 'log') {
        this.logger[data.level]('[Worker] ' + data.message, data.data);
      }
    }

    // Queue message to send via WebSocket (called by C#)
    queueMessage(message) {
      if (!this.isRunning) {
        this.logger.warn('Cannot queue message: Worker not running');
        return false;
      }
      this.toWorkerQueue.push(message);
      this.logger.info('Message queued to send (length: ' + message.length + ', queue size: ' + this.toWorkerQueue.length + ')');
      return true;
    }

    queueReply(message) {
      if (!this.isRunning) {
        this.logger.warn('Cannot queue reply: Worker not running');
        return false;
      }
      this.fromWorkerQueue.push(message);
      this.logger.info('Reply queued to send (length: ' + message.length + ', queue size: ' + this.fromWorkerQueue.length + ')');
      return true;
    }

    // Dequeue message received from WebSocket (called by C#)
    dequeueMessage() {
      if (this.fromWorkerQueue.length > 0) {
        const msg = this.fromWorkerQueue.shift();
        this.logger.info('Message dequeued from receive queue (length: ' + msg.length + ', remaining: ' + this.fromWorkerQueue.length + ')');
        return msg;
      }
      return '';
    }
    // Get queue counts
    getReceiveQueueCount() {
      return this.fromWorkerQueue.length;
    }

    getSendQueueCount() {
      return this.toWorkerQueue.length;
    }
  }

  // Create global MetaDSL Worker manager instance
  const metadslWorker = new MetaDSLWorkerManager();

  // Listen for postMessage and queue string messages as replies to LLM
  window.addEventListener('message', (event) => {
    if (typeof event.data === 'string' && event.data.length > 0) {
      metadslWorker.queueReply(event.data);
    }
  });

