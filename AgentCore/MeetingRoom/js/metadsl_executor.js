// ============================================================================
// MetaDSL Executor - MetaDSL code execution over the browser relay
// (window.cefQuery -> browser process wsclient -> AgentCore), the same
// channel the single-page agents use. Registers with the agent id 'aichat':
// the browser dsl opens a dedicated browser->AgentCore websocket connection
// for this registration and re-sends the agent_bind envelope on every
// (re)connect, so the raw MetaDSL worker path resolves the 'aichat'
// AgentInstance. Code goes out through the agent_send action, the wrapped
// result text comes back through the window.onAgentEvent push.
// ============================================================================
class MetaDSLExecutor {
    constructor() {
        this.clientId = null;       // relay client id (the wsclient id)
        this.isRegistered = false;  // agent_register answered with a client id
        this.isConnected = false;   // underlying relay link state
        this._registerPending = false;
        this._registerTimer = null;
        this._reconnectTimer = null;

        // Execution queue: each item is { code, resolve, reject, channelId, senderId }
        this.pendingQueue = [];
        this.currentTask = null;
        // Timeout for current execution (30 seconds default)
        this.executionTimeout = 30000;
        this.executionTimer = null;
    }

    // ---- cefQuery plumbing ----------------------------------------------

    _cefQuery(request, onSuccess, onFailure) {
        if (typeof window.cefQuery !== 'function') {
            if (onFailure) {
                onFailure(-1, 'cefQuery unavailable');
            }
            return false;
        }
        window.cefQuery({
            request: JSON.stringify(request),
            onSuccess: function (response) {
                if (onSuccess) onSuccess(response);
            },
            onFailure: function (code, msg) {
                if (onFailure) onFailure(code, msg);
            }
        });
        return true;
    }

    // ---- registration ----------------------------------------------------

    // Register with the browser relay. The $port argument of the old direct
    // websocket transport (ws://localhost:<port>) is ignored: the relay
    // connection is opened by the browser dsl and targets the AgentCore
    // relay port. Never gives up while cefQuery exists (retry every 2s);
    // transient link drops are healed by the C# link layer (same client id).
    connect(port) {
        if (this.isRegistered || this._registerPending) {
            return;
        }
        this._registerPending = true;
        const self = this;
        const sent = this._cefQuery({
            action: 'agent_register',
            urlKey: window.location.host,
            agentId: 'aichat'
        }, function (clientId) {
            self._registerPending = false;
            self._clearRegisterTimer();
            self.clientId = clientId;
            self.isRegistered = true;
            self.isConnected = true;
            console.log('[MetaDSL] relay registered (aichat): ' + clientId);
            self._installPushHandler();
            self._processNext();
        }, function (code, msg) {
            self._registerPending = false;
            self._clearRegisterTimer();
            self.isConnected = false;
            console.warn('[MetaDSL] relay register failed (' + code + '): ' + msg);
            self._scheduleReconnect();
        });
        if (!sent) {
            this._registerPending = false;
            console.error('[MetaDSL] cefQuery unavailable, cannot reach the AgentCore relay');
            return;
        }
        // Safety net for a lost state event (same pattern as relay_transport).
        this._registerTimer = setTimeout(function () {
            self._registerTimer = null;
            self._registerPending = false;
            self.isConnected = false;
            console.warn('[MetaDSL] relay register timed out');
            self._scheduleReconnect();
        }, 12000);
    }

    _clearRegisterTimer() {
        if (this._registerTimer) {
            clearTimeout(this._registerTimer);
            this._registerTimer = null;
        }
    }

    _scheduleReconnect() {
        if (this._reconnectTimer) {
            return;
        }
        const self = this;
        this._reconnectTimer = setTimeout(function () {
            self._reconnectTimer = null;
            self.connect();
        }, 2000);
    }

    // ---- push handling ---------------------------------------------------

    // Chain our dispatcher into window.onAgentEvent (a shared dispatcher
    // from an injected relay_transport.js, if ever present, stays intact).
    _installPushHandler() {
        const self = this;
        const prev = window.onAgentEvent;
        window.onAgentEvent = function (clientId, message) {
            if (clientId === self.clientId) {
                self._onPush(message);
                return;
            }
            if (typeof prev === 'function') {
                prev(clientId, message);
            }
        };
    }

    _onPush(message) {
        if (typeof message !== 'string' || message.length === 0) {
            return;
        }
        if (message.charAt(0) === '{') {
            let msg = null;
            try {
                msg = JSON.parse(message);
            } catch (e) {
                msg = null;
            }
            if (msg && msg.type === 'relay_state') {
                this._onRelayState(msg.state);
                return;
            }
            // Other envelopes are not expected on the raw code connection.
            if (msg && msg.type) {
                console.warn('[MetaDSL] unexpected envelope: ' + msg.type);
                return;
            }
            // Fall through: JSON-looking raw result text.
        }
        // Wrapped MetaDSL result text (MetaDSL <{:> ... <:}>; Result ...).
        console.log('[MetaDSL] Received result (length: ' + message.length + ')');
        if (this.currentTask) {
            this.currentTask.resolve(message);
            this._clearCurrentTask();
            this._processNext();
        }
    }

    _onRelayState(state) {
        if (state === 'connected') {
            this.isConnected = true;
            console.log('[MetaDSL] relay link up');
            this._processNext();
        } else if (state === 'reconnecting' || state === 'connecting') {
            // Link-layer retry on the SAME connection: keep the client id.
            this.isConnected = false;
            console.warn('[MetaDSL] relay link ' + state + ', waiting for recovery');
        } else {
            // failed / disconnected: the connection is dead, re-register.
            this.isConnected = false;
            this.isRegistered = false;
            this.clientId = null;
            console.warn('[MetaDSL] relay link ' + state + ', will re-register');
            this._scheduleReconnect();
        }
    }

    // ---- execution -------------------------------------------------------

    // Disconnect from the relay
    disconnect() {
        this.isRegistered = false;
        this.isConnected = false;
        this._registerPending = false;
        this._clearRegisterTimer();
        if (this._reconnectTimer) {
            clearTimeout(this._reconnectTimer);
            this._reconnectTimer = null;
        }
        if (this.clientId) {
            const cid = this.clientId;
            this.clientId = null;
            this._cefQuery({ action: 'agent_unregister', clientId: cid }, function () { }, function () { });
        }
        // Reject all pending tasks
        if (this.currentTask) {
            this.currentTask.reject(new Error('Executor disconnected'));
            this._clearCurrentTask();
        }
        while (this.pendingQueue.length > 0) {
            const task = this.pendingQueue.shift();
            task.reject(new Error('Executor disconnected'));
        }
        console.log('[MetaDSL] Disconnected');
    }

    // Execute MetaDSL code, returns a Promise with the result
    execute(code, channelId, senderId) {
        return new Promise((resolve, reject) => {
            this.pendingQueue.push({ code, resolve, reject, channelId, senderId });
            console.log('[MetaDSL] Task queued (queue size: ' + this.pendingQueue.length + ', channel: ' + channelId + ', sender: ' + senderId + ')');
            // If no current task running, process immediately
            if (!this.currentTask) {
                this._processNext();
            }
        });
    }

    // Process next task in queue
    _processNext() {
        if (this.currentTask || this.pendingQueue.length === 0) {
            return;
        }

        if (!this.isRegistered || !this.isConnected || !this.clientId) {
            console.log('[MetaDSL] Relay not ready, waiting to process queue');
            return;
        }

        this.currentTask = this.pendingQueue.shift();
        console.log('[MetaDSL] Executing code (length: ' + this.currentTask.code.length + ', channel: ' + this.currentTask.channelId + ')');

        const self = this;
        const sent = this._cefQuery({
            action: 'agent_send',
            clientId: this.clientId,
            message: this.currentTask.code
        }, function () {
            // Sent: the wrapped result arrives through the onAgentEvent push.
        }, function (code, msg) {
            console.error('[MetaDSL] agent_send failed (' + code + '): ' + msg);
            self.isConnected = false;
            if (self.currentTask) {
                self.currentTask.reject(new Error('Relay send failed: ' + msg));
                self._clearCurrentTask();
                self._processNext();
            }
            // Link recovery is driven by the relay_state pushes.
        });
        if (!sent) {
            this.currentTask.reject(new Error('cefQuery unavailable'));
            this._clearCurrentTask();
            this._processNext();
            return;
        }
        // Set execution timeout
        this.executionTimer = setTimeout(function () {
            if (self.currentTask) {
                console.warn('[MetaDSL] Execution timeout');
                self.currentTask.reject(new Error('Execution timeout'));
                self._clearCurrentTask();
                self._processNext();
            }
        }, this.executionTimeout);
    }

    // Clear current task state
    _clearCurrentTask() {
        if (this.executionTimer) {
            clearTimeout(this.executionTimer);
            this.executionTimer = null;
        }
        this.currentTask = null;
    }

    // Check if executor is ready
    isReady() {
        return this.isRegistered && this.isConnected && !!this.clientId;
    }

    // Get pending task count
    getPendingCount() {
        return this.pendingQueue.length + (this.currentTask ? 1 : 0);
    }
}

// ============================================================================
// MetaDSL tag detection and extraction utilities
// ============================================================================

const MetaDSLUtils = {
    // Regex to match <metadsl>...</metadsl> tags (case-insensitive, dotAll)
    TAG_REGEX: /<metadsl>([\s\S]*?)<\/metadsl>/i,
    OPEN_TAG_REGEX: /<metadsl\b[^>]*>/gi,
    CLOSE_TAG_REGEX: /<\/metadsl\s*>/gi,

    // Check if text contains a MetaDSL tag
    hasMetaDSLTag(text) {
        return this.TAG_REGEX.test(text);
    },

    // Extract MetaDSL code from text, returns { code, cleanText } or null
    extractMetaDSL(text) {
        const match = text.match(this.TAG_REGEX);
        if (!match) {
            return null;
        }
        const code = match[1].trim();
        const cleanText = text.replace(this.TAG_REGEX, '[MetaDSL code executing...]').trim();
        return { code, cleanText };
    },

    // Validate and extract MetaDSL code from text.
    // Rules:
    //   1. <metadsl> count must equal </metadsl> count
    //   2. If both counts are 0 -> no MetaDSL, return { ok:true, code:null }
    //   3. Every well-formed pair contributes its body; the bodies are joined
    //      in order and executed as ONE command. Sending more than 1 block is
    //      still a protocol violation, so `warning` is set and the caller is
    //      expected to report it, but execution proceeds.
    //   4. Otherwise -> { ok:false, reason }
    //   5. A body must not contain another <metadsl or </metadsl substring
    //   6. A well-formed but empty block is rejected, so the caller never runs
    //      an empty script (the "no MetaDSL" contract is code === null)
    // Returns { ok, code, cleanText, reason, warning }
    //
    // IMPORTANT: no reason/warning string may contain a literal metadsl tag.
    // cleanText is RETURNED as the message content and forwarded to peers
    // (chat.js relay and LLM-reply paths), where it is validated again on
    // arrival. A literal tag inside a note would be counted as a real tag
    // there and trip the count/order checks, turning one diagnosis into a
    // different, bogus one. Always write the tag name without angle brackets.
    //
    // Note on stages: the DOM-based detectors have a stage that absorbs an
    // unmarked code block into the preceding command when only whitespace
    // separates them. Here every chunk is delimited by its own tag pair, so
    // there is no unmarked continuation to absorb and that stage collapses to
    // "one pair = one unit". Only the merge stage below is meaningful, and it
    // deliberately ignores what sits between the pairs.
    validateAndExtract(text) {
        if (!text || typeof text !== 'string') {
            return { ok: true, code: null, cleanText: text || '' };
        }
        // Reset lastIndex on global regex via fresh match
        const openMatches = text.match(this.OPEN_TAG_REGEX) || [];
        const closeMatches = text.match(this.CLOSE_TAG_REGEX) || [];
        const openCount = openMatches.length;
        const closeCount = closeMatches.length;
        if (openCount === 0 && closeCount === 0) {
            return { ok: true, code: null, cleanText: text };
        }
        if (openCount !== closeCount) {
            const reason = 'metadsl open tag count (' + openCount
                + ') does not match close tag count (' + closeCount + ').';
            const cleanText = text + '\n[MetaDSL invalid: ' + reason + ']';
            return { ok: false, code: null, cleanText: cleanText, reason: reason };
        }
        const openIdx = text.search(/<metadsl\b[^>]*>/i);
        const closeIdx = text.search(/<\/metadsl\s*>/i);
        if (openIdx < 0 || closeIdx < 0 || closeIdx < openIdx) {
            const reason = 'metadsl open and close tags are in wrong order.';
            const cleanText = text + '\n[MetaDSL invalid: ' + reason + ']';
            return { ok: false, code: null, cleanText: cleanText, reason: reason };
        }
        // Collect every pair body in document order. A fresh RegExp is used so
        // no lastIndex state is shared with the module-level patterns.
        const bodyRegex = new RegExp(this.TAG_REGEX.source, 'gi');
        const bodies = [];
        let match;
        while ((match = bodyRegex.exec(text)) !== null) {
            bodies.push(match[1]);
        }
        // Fewer bodies than opening tags means at least one pair is malformed,
        // e.g. an attribute form that OPEN_TAG_REGEX accepts but TAG_REGEX
        // does not, or tags that interleave instead of nesting cleanly.
        if (bodies.length !== openCount) {
            const reason = 'Failed to extract metadsl body ('
                + bodies.length + ' of ' + openCount + ' pairs parsed).';
            const cleanText = text + '\n[MetaDSL invalid: ' + reason + ']';
            return { ok: false, code: null, cleanText: cleanText, reason: reason };
        }
        for (let i = 0; i < bodies.length; i++) {
            // Body must not contain inner open/close tag substrings
            if (/<metadsl\b/i.test(bodies[i]) || /<\/metadsl\b/i.test(bodies[i])) {
                const reason = 'metadsl body must not contain nested metadsl tags.';
                const cleanText = text.replace(new RegExp(this.TAG_REGEX.source, 'gi'),
                    '[MetaDSL invalid: ' + reason + ']').trim();
                return { ok: false, code: null, cleanText: cleanText, reason: reason };
            }
        }
        // Merge stage: join every body into a single command.
        const code = bodies.map(function (b) { return b.trim(); })
            .filter(function (b) { return b.length > 0; })
            .join('\n');
        if (code.length === 0) {
            // Well-formed but empty, e.g. an unfilled template. Without this the
            // caller would run an empty script, because code is '' and the
            // "no MetaDSL" contract is code === null.
            const reason = 'metadsl block is empty, no code to execute.';
            const cleanText = text.replace(new RegExp(this.TAG_REGEX.source, 'gi'),
                '[MetaDSL invalid: ' + reason + ']').trim();
            return { ok: false, code: null, cleanText: cleanText, reason: reason };
        }
        let cleanText = text.replace(new RegExp(this.TAG_REGEX.source, 'gi'),
            '[MetaDSL code executing...]').trim();
        if (openCount > 1) {
            // Reported to the model through cleanText, same channel the reject
            // path uses, so it learns about the violation without blocking.
            const warning = 'Detected ' + openCount
                + ' metadsl blocks in one message, but only 1 is allowed.'
                + ' They were merged in order and executed as a single command.'
                + ' Beware that a return in an earlier block ends the merged'
                + ' script early. Send exactly one metadsl block next time.';
            cleanText = cleanText + '\n[MetaDSL warning: ' + warning + ']';
            return { ok: true, code: code, cleanText: cleanText, warning: warning };
        }
        return { ok: true, code: code, cleanText: cleanText };
    }
};

// Global executor instance
const metadslExecutor = new MetaDSLExecutor();

// ============================================================================
// Cefclient integration
//
// The executor registers with the relay as soon as the page loads (same as
// the single-page agents' relay_transport), so no dsl command is needed to
// start it. The legacy aichat_ready notification and the ws_start_aichat /
// ws_stop_aichat commands are kept for compatibility: ws_start_aichat now
// just triggers the (idempotent) relay registration. callMetaDSL(...) is a
// renderer-provided global (see client_renderer.cc), available without any
// injected module. In a plain browser (no cefclient) neither exists and the
// executor stays disconnected (clear error in the console).
// ============================================================================
(function () {
    if (typeof callMetaDSL === 'function') {
        callMetaDSL('handle_agent_notification', JSON.stringify({
            type: 'aichat_ready',
            data: { url: window.location.href }
        }));
        console.log('[MetaDSL] aichat_ready notification sent');
    } else {
        console.log('[MetaDSL] callMetaDSL not available, running outside cefclient');
    }
})();

// Register with the relay as soon as the page loads.
if (typeof window !== 'undefined') {
    setTimeout(function () {
        metadslExecutor.connect();
    }, 0);

    // Graceful teardown on refresh/navigation: unregister the relay
    // connection (renderer crash without pagehide is covered by the
    // browser dsl zombie reaper).
    window.addEventListener('pagehide', function () {
        try {
            metadslExecutor.disconnect();
        } catch (e) { /* best effort */ }
    });
}

// DSL -> JS commands. The C# side passes the command as a JSON string
// argument (same as google_ai_search.js window.onAgentCommand).
window.onAgentCommand = function (commandJson) {
    try {
        const cmd = JSON.parse(commandJson);
        console.log('[MetaDSL] onAgentCommand:', cmd.command);
        if (cmd.command === 'ws_start_aichat' && cmd.params && cmd.params.port) {
            const port = parseInt(cmd.params.port, 10);
            console.log('[MetaDSL] ws_start_aichat -> port ' + port);
            if (!metadslExecutor.isReady()) {
                metadslExecutor.connect(port);
            }
            return;
        }
        if (cmd.command === 'ws_stop_aichat') {
            console.log('[MetaDSL] ws_stop_aichat -> disconnect');
            metadslExecutor.disconnect();
            return;
        }
        console.warn('[MetaDSL] unhandled agent command: ' + cmd.command);
    } catch (e) {
        console.error('[MetaDSL] onAgentCommand error: ' + e);
    }
};
