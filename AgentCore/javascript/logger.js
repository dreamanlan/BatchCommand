/**
 * Logger Module
 * Unified logging system for AgentCore JavaScript
 * Supports console logging (disabled by default) and native C# logging
 */

class Logger {
  constructor(prefix = '') {
    this.prefix = prefix;

    // Log buffering for C# communication
    this.logBuffer = [];
    this.logBufferTimer = null;
    this.logBufferDelay = 1000; // Batch send every 1 second
    this.maxBufferSize = 50; // Max logs before forcing flush

    // Log sampling to prevent flooding
    this.lastLogTime = {};
    this.logSampleInterval = 100; // Min ms between same log messages

    // Configuration
    this.enableConsole = false; // Console logging disabled by default
    this.enableNative = true; // Native logging enabled by default
  }

  /**
   * Create a child logger with a prefix
   * @param {string} prefix - Prefix for log messages
   * @returns {Logger} Child logger instance
   */
  createLogger(prefix) {
    const childLogger = new Logger(prefix);
    // Share buffer with parent logger
    childLogger.logBuffer = this.logBuffer;
    childLogger.logBufferTimer = this.logBufferTimer;
    childLogger.lastLogTime = this.lastLogTime;
    childLogger.enableConsole = this.enableConsole;
    childLogger.enableNative = this.enableNative;
    return childLogger;
  }

  /**
   * Truncate large data objects to prevent performance issues
   * @param {*} data - Data to truncate
   * @param {number} maxLength - Maximum length
   * @returns {*} Truncated data
   */
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

  /**
   * Check if a log message should be sampled (rate limiting)
   * @param {string} message - Log message
   * @param {string} level - Log level
   * @returns {boolean} True if should log
   */
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

  /**
   * Flush buffered logs to native C#
   */
  flushLogBuffer() {
    if (this.logBuffer.length === 0) return;

    if (this.enableNative && typeof sendMessage !== 'undefined') {
      try {
        // Send all buffered logs in one batch using the same mechanism as inject.js
        sendMessage('debug_log_batch', JSON.stringify({
          logs: this.logBuffer,
          timestamp: Date.now()
        }));
        this.logBuffer = [];
      } catch (e) {
        // Fallback to console if native logging fails
        if (this.enableConsole) {
          console.error('[Logger] Failed to flush log buffer:', e);
        }
        this.logBuffer = []; // Clear buffer to prevent memory leak
      }
    } else {
      // If native logging is not available, just clear the buffer
      this.logBuffer = [];
    }

    if (this.logBufferTimer) {
      clearTimeout(this.logBufferTimer);
      this.logBufferTimer = null;
    }
  }

  /**
   * Schedule a flush of the log buffer
   */
  scheduleFlush() {
    if (this.logBufferTimer) return;

    this.logBufferTimer = setTimeout(() => {
      this.flushLogBuffer();
    }, this.logBufferDelay);
  }

  /**
   * Main logging method
   * @param {string} message - Log message
   * @param {string} level - Log level (debug, info, warn, error)
   * @param {*} data - Additional data to log
   */
  log(message, level = 'info', data = null) {
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
    const fullMessage = truncatedData
      ? `${prefixStr}${message} ${JSON.stringify(truncatedData)}`
      : `${prefixStr}${message}`;

    // Log to console (commented out by default to avoid console.log file growth)
    if (this.enableConsole) {
      const consoleMethod = level === 'error' ? 'error' : level === 'warn' ? 'warn' : 'log';
      console[consoleMethod](`${levelPrefix} ${fullMessage}`);
    }

    // Buffer log for C# (batch send)
    if (this.enableNative) {
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

  /**
   * Log debug message
   * @param {string} message - Log message
   * @param {*} data - Additional data
   */
  debug(message, data = null) {
    this.log(message, 'debug', data);
  }

  /**
   * Log info message
   * @param {string} message - Log message
   * @param {*} data - Additional data
   */
  info(message, data = null) {
    this.log(message, 'info', data);
  }

  /**
   * Log warning message
   * @param {string} message - Log message
   * @param {*} data - Additional data
   */
  warn(message, data = null) {
    this.log(message, 'warn', data);
  }

  /**
   * Log error message
   * @param {string} message - Log message
   * @param {*} data - Additional data
   */
  error(message, data = null) {
    this.log(message, 'error', data);
  }

  /**
   * Enable or disable console logging
   * @param {boolean} enabled - True to enable console logging
   */
  setConsoleEnabled(enabled) {
    this.enableConsole = enabled;
  }

  /**
   * Enable or disable native logging
   * @param {boolean} enabled - True to enable native logging
   */
  setNativeEnabled(enabled) {
    this.enableNative = enabled;
  }
}

// Create global logger instance
const logger = new Logger();

// Export for use in other modules
window.Logger = Logger;
window.logger = logger;
