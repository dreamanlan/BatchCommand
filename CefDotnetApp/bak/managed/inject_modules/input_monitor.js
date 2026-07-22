// ============================================================================
// User Input Monitor - Independent monitoring class
// ============================================================================
class UserInputMonitor {
  constructor(pageAdapter) {
    this.logger = logger.createLogger('UserInputMonitor');
    this.pageAdapter = pageAdapter;
    this.isMonitoring = false;
    this.hasUserInput = false;
    this.isUserTyping = false;
    this.userTypingTimer = null;
    this.userTypingDelay = CONFIG.userTypingDelay;
    this.inputElement = null;
    this.sendButton = null;

    // Event handlers (need to be bound for proper removal)
    this.handleFocus = this.onUserStartTyping.bind(this);
    this.handleInput = this.onUserStartTyping.bind(this);
    this.handleKeydown = this.onUserStartTyping.bind(this);
  }

  start() {
    // Always reset flags when starting
    this.hasUserInput = false;
    this.isUserTyping = false;

    if (this.isMonitoring) {
      this.logger.debug('Already monitoring, flags reset');
      return;
    }

    this.isMonitoring = true;

    // Find input element based on page type
    this.inputElement = this.findInputElement();

    if (this.inputElement) {
      this.inputElement.addEventListener('focus', this.handleFocus);
      this.inputElement.addEventListener('input', this.handleInput);
      this.inputElement.addEventListener('keydown', this.handleKeydown);
      this.logger.debug('Monitoring started on element', { element: this.inputElement.tagName });
      this.logger.debug('Page type', { pageType: this.pageAdapter.pageType });
    } else {
      this.logger.warn('Input element not found', { pageType: this.pageAdapter.pageType });
    }
  }

  stop() {
    if (!this.isMonitoring) {
      return;
    }

    this.isMonitoring = false;
    this.clearUserTypingTimer();

    if (this.inputElement) {
      this.inputElement.removeEventListener('focus', this.handleFocus);
      this.inputElement.removeEventListener('input', this.handleInput);
      this.inputElement.removeEventListener('keydown', this.handleKeydown);
    }

    this.logger.debug('Monitoring stopped');
  }

  findInputElement() {
    const pageType = this.pageAdapter.pageType;
    switch (pageType) {
      case 'local-agent':
        return document.getElementById('editable-content');
      case 'custom-llm':
        return document.getElementById('editable-content');
      default:
        return null;
    }
  }

  checkInputHasContent() {
    if (!this.inputElement) {
      this.inputElement = this.findInputElement();
    }
    if (!this.inputElement) return false;

    const content = this.inputElement.textContent || this.inputElement.value || '';
    return content.trim().length > 0;
  }

  onUserStartTyping() {
    if (!this.isMonitoring) {
      this.logger.debug('Event received but not monitoring');
      return;
    }

    this.isUserTyping = true;
    this.hasUserInput = true;
    this.logger.debug('User input detected! hasUserInput=true, isUserTyping=true');
    this.clearUserTypingTimer();

    this.userTypingTimer = setTimeout(() => {
      this.isUserTyping = false;
      this.logger.debug('User stopped typing (isUserTyping=false)');
    }, this.userTypingDelay);
  }

  clearUserTypingTimer() {
    if (this.userTypingTimer) {
      clearTimeout(this.userTypingTimer);
      this.userTypingTimer = null;
    }
  }

  reset() {
    this.hasUserInput = false;
    this.isUserTyping = false;
    this.clearUserTypingTimer();
  }
}
