function getStringInLength(str, length, beginOrEndOrBeginEnd) {
  if (str.length <= length) {
    return str;
  }
  switch (beginOrEndOrBeginEnd) {
    case 0:
      return str.substring(0, length) + '...';
    case 1:
      return '...' + str.substring(str.length - length);
    case 2:
      return str.substring(0, length / 2) + '...' + str.substring(str.length - length / 2);
  }
}
// ============================================================================
// Logger - Unified logging to console, panel, and C#
// ============================================================================
class Logger {
  constructor(prefix = '') {
    this.panel = null;
    this.prefix = prefix;

    // Log buffering for C# communication
    this.logBuffer = [];
    this.logBufferTimer = null;
    this.logBufferDelay = 1000; // Batch send every 1 second
    this.maxBufferSize = 50; // Max logs before forcing flush

    // Log sampling to prevent flooding
    this.lastLogTime = {};
    this.logSampleInterval = 100; // Min ms between same log messages
  }

  setPanel(panel) {
    this.panel = panel;
  }

  createLogger(prefix) {
    const childLogger = new Logger(prefix);
    childLogger.panel = this.panel;
    // Share buffer with parent logger
    childLogger.logBuffer = this.logBuffer;
    childLogger.logBufferTimer = this.logBufferTimer;
    childLogger.lastLogTime = this.lastLogTime;
    return childLogger;
  }

  truncateData(data, maxLength = 500) {
    if (!data) return null;

    try {
      const str = JSON.stringify(data);
      if (str.length <= maxLength) {
        return data;
      }

      // Return truncated string representation
      return str.substring(0, maxLength) + '... (truncated)';
    } catch (e) {
      return '[Circular or non-serializable data]';
    }
  }

  shouldSampleLog(message, level) {
    const key = `${level}:${message}`;
    const now = Date.now();
    const lastTime = this.lastLogTime[key] || 0;

    if (now - lastTime < this.logSampleInterval) {
      return false; // Skip this log (too frequent)
    }

    this.lastLogTime[key] = now;
    return true;
  }

  flushLogBuffer() {
    if (this.logBuffer.length === 0) return;

    if (typeof callMetaDSL !== 'undefined') {
      try {
        // Send all buffered logs in one batch
        /*
        callMetaDSL('handle_nativelog_batch', JSON.stringify({
          logs: this.logBuffer,
          timestamp: Date.now()
        }));
        */
        this.logBuffer = [];
      } catch (e) {
        console.error('[Logger] Failed to flush log buffer:', e);
        this.logBuffer = []; // Clear buffer to prevent memory leak
      }
    }

    if (this.logBufferTimer) {
      clearTimeout(this.logBufferTimer);
      this.logBufferTimer = null;
    }
  }

  scheduleFlush() {
    if (this.logBufferTimer) return;

    this.logBufferTimer = setTimeout(() => {
      this.flushLogBuffer();
    }, this.logBufferDelay);
  }

  log(message, level = 'info', data = null) {
    if (!CONFIG.logLevels[level]) return;

    // Sample frequent logs
    if (!this.shouldSampleLog(message, level)) {
      return;
    }

    const levelPrefixes = {
      debug: '🔍',
      info: '📝',
      warn: '⚠️',
      error: '❌'
    };

    const levelPrefix = levelPrefixes[level] || '📝';
    const prefixStr = this.prefix ? `[${this.prefix}] ` : '';

    // Truncate large data objects
    const truncatedData = this.truncateData(data);
    const fullMessage = truncatedData ? `${prefixStr}${message} ${JSON.stringify(truncatedData)}` : `${prefixStr}${message}`;

    // Log to console (controlled by CONFIG.consoleOutput toggle)
    if (CONFIG.consoleOutput) {
      const consoleMethod = level === 'error' ? 'error' : level === 'warn' ? 'warn' : 'log';
      console[consoleMethod](fullMessage);
    }

    // Log to panel if available (with size limit)
    if (this.panel) {
      this.panel.log(`${levelPrefix} ${fullMessage}`);
    }

    // Buffer log for C# (batch send)
    if (typeof callMetaDSL !== 'undefined') {
      this.logBuffer.push({
        level: level,
        message: fullMessage,
        data: truncatedData,
        timestamp: Date.now()
      });

      // Force flush if buffer is full
      if (this.logBuffer.length >= this.maxBufferSize) {
        this.flushLogBuffer();
      } else {
        this.scheduleFlush();
      }
    }
  }

  debug(message, data = null) { this.log(message, 'debug', data); }
  info(message, data = null) { this.log(message, 'info', data); }
  warn(message, data = null) { this.log(message, 'warn', data); }
  error(message, data = null) { this.log(message, 'error', data); }
}

// Create global logger instance
const logger = new Logger();
