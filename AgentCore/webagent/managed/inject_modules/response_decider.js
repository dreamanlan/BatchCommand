// ============================================================================
// ResponseDecider - Decide how to respond when agent needs to plan
// Heuristics ported from Script.dsl's agent_need_to_decide branch.
// Decision output (action):
//   - 'skip'          : do nothing
//   - 'reply'         : reply a text to LLM (text in decision.text)
//   - 'command'       : issue inject command (cmd/start_auto_plan or stop_auto_plan)
//   - 'reply_ref'     : reply with ref-wrapped prompt (text in decision.text)
//   - 'trigger_decision' : ask DSL to run induction_decision (notify agent_need_to_decide)
//   - 'none'          : fall-through, nothing to do
// ============================================================================
class ResponseDecider {
  constructor(logger) {
    this.logger = logger || (typeof window !== 'undefined' && window.logger
      ? window.logger.createLogger('ResponseDecider')
      : console);
    // Mirror DSL's @EnableLlmPM. Kept as JS-local flag; planning still runs in DSL.
    this.enableLlmPM = true;
  }

  // Core helpers ------------------------------------------------------------
  containsAll(text, ...keys) {
    if (typeof text !== 'string') return false;
    for (const k of keys) {
      if (text.indexOf(k) < 0) return false;
    }
    return true;
  }

  containsAny(text, ...keys) {
    if (typeof text !== 'string') return false;
    for (const k of keys) {
      if (text.indexOf(k) >= 0) return true;
    }
    return false;
  }

  // MetaDSL structure helpers ------------------------------------------------
  // Mirror metadsl_monitor's marker rule (CONFIG.metadslMarkers): a marker
  // line is a line whose trimmed text STARTS with '// @execute' / '# @execute'.
  getMetadslMarkers() {
    try {
      if (typeof CONFIG !== 'undefined' && CONFIG && CONFIG.metadslMarkers
        && CONFIG.metadslMarkers.length) {
        return CONFIG.metadslMarkers;
      }
    } catch (e) { /* fall through to hardcoded defaults */ }
    return ['// @execute', '# @execute'];
  }

  isMetadslMarkerLine(line) {
    const t = String(line || '').trim();
    if (!t) return false;
    return this.getMetadslMarkers().some(marker => t.startsWith(marker));
  }

  // Split markdown text into {type:'fence'|'text', content} segments.
  // A fence opens on a line that is (aside from indentation) solely a 3+
  // backtick/tilda run plus an optional whitespace-free info string, and
  // closes on a line of the same char at least as long with nothing after it
  // (CommonMark rules, matching page_adapter._makeFence output). Prose that
  // merely mentions ``` mid-sentence never opens a fence.
  parseMarkdownSegments(text) {
    const segments = [];
    const lines = String(text || '').replace(/\r\n?/g, '\n').split('\n');
    let buf = [];
    let inFence = false;
    let fenceChar = '';
    let fenceLen = 0;
    for (const line of lines) {
      if (inFence) {
        const close = line.match(/^[ \t]*(`{3,}|~{3,})[ \t]*$/);
        if (close && close[1][0] === fenceChar && close[1].length >= fenceLen) {
          segments.push({ type: 'fence', content: buf.join('\n') });
          buf = [];
          inFence = false;
          continue;
        }
        buf.push(line);
      } else {
        // Opening fence: backtick run + optional info string WITHOUT spaces.
        // The no-space rule keeps prose like "``` 单独占行" out of fences.
        const open = line.match(/^[ \t]*(`{3,}|~{3,})[^\s`]*[ \t]*$/);
        if (open) {
          segments.push({ type: 'text', content: buf.join('\n') });
          buf = [];
          inFence = true;
          fenceChar = open[1][0];
          fenceLen = open[1].length;
          continue;
        }
        buf.push(line);
      }
    }
    // Unterminated fence (still streaming / malformed): treat the tail as
    // fenced content, mirroring how the monitor waits for the closing fence.
    segments.push({ type: inFence ? 'fence' : 'text', content: buf.join('\n') });
    return segments;
  }

  // Structural MetaDSL detection, aligned with metadsl_monitor:
  //   - fencedMetadsl: fenced code blocks whose first non-blank line is a
  //     marker line (the exact rule extractMetaDSLCode uses to run code);
  //   - textMetadsl: marker lines in plain text (unfenced raw code).
  // Prose that only MENTIONS ``` or // @execute mid-sentence (e.g. the
  // model's own syntax self-check confirmation) matches neither.
  analyzeMetadslStructure(msg) {
    const result = { fencedMetadsl: 0, textMetadsl: 0 };
    for (const seg of this.parseMarkdownSegments(msg)) {
      const lines = seg.content.split('\n');
      if (seg.type === 'fence') {
        const first = lines.find(l => l.trim() !== '');
        if (first && this.isMetadslMarkerLine(first)) result.fencedMetadsl++;
      } else {
        for (const l of lines) {
          if (this.isMetadslMarkerLine(l)) result.textMetadsl++;
        }
      }
    }
    return result;
  }

  // Main decision -----------------------------------------------------------
  // data: { state, lastFromLLM, lastScannedMessage, isLastResponse,
  //         pageType, count, autoPlan, lockAgent, force }
  decide(data, panel) {
    const d = data || {};
    const msg = typeof d.lastScannedMessage === 'string' ? d.lastScannedMessage : '';
    const msgLen = msg.length;
    const lastFromLLM = (d.lastFromLLM === true || d.lastFromLLM === 'True' || d.lastFromLLM === 'true');
    // Auto plan disabled: the default trigger_decision paths would fire an LLM
    // PM request from the DSL side on every state machine cycle (this tripped
    // the company rate limit). Force (manual needToPlan button) still passes.
    // Only an explicit false disables: a missing field keeps the old behavior.
    const autoPlanDisabled = (d.autoPlan === false || d.autoPlan === 'false' || d.autoPlan === 'False');
    const force = d.force === true;
    const queued = panel?.metadslMonitor?.operationQueue?.length || 0;
    const send = panel?.metadslWorker?.getSendQueueCount?.() || 0;
    const receive = panel?.metadslWorker?.getReceiveQueueCount?.() || 0;

    // 1. Not the latest response -> skip
    if (d.isLastResponse !== true) {
      return { action: 'skip', reason: 'not latest response' };
    }

    // 2. Still has queued operations -> remind LLM to wait
    // Not while the user is composing: the reminder would land right after
    // their own message once the input clears, and they are already present.
    if (queued > 0) {
      const pa = panel?.pageAdapter;
      if (pa && typeof pa.hasUserDraft === 'function' && pa.hasUserDraft()) {
        return { action: 'skip', reason: 'user is composing' };
      }
      return {
        action: 'reply',
        text: `还有${queued}个代码要执行，${send}个请求要发送，${receive}个结果要接收，不要再发新代码，回复继续即可`,
      };
    }

    // 3. Last message came from LLM
    if (lastFromLLM) {
      // The newest reply has not been through a completed scan yet (e.g. the
      // scan timeout found its blocks still changing): its code may still be
      // queued. Replying now (format reminder, planning) would tell the model
      // its code was not recognized while it is only late. Bounded inside
      // isLatestReplyScanPending, which also arms a rescan.
      const monitor = panel?.metadslMonitor;
      if (monitor && typeof monitor.isLatestReplyScanPending === 'function'
        && monitor.isLatestReplyScanPending()) {
        return { action: 'skip', reason: 'latest reply scan pending' };
      }
      if (this.containsAll(msg, 'Error', 'Occur')) {
        return { action: 'reply', text: '继续' };
      }
      if (msg.indexOf('启动自动计划') >= 0 && msgLen <= 32) {
        return { action: 'command', command: 'start_auto_plan' };
      }
      if (msg.indexOf('停止自动计划') >= 0 && msgLen <= 32) {
        return { action: 'command', command: 'stop_auto_plan' };
      }

      // MetaDSL Result
      const hasMetaDslResult =
        this.containsAll(msg, 'MetaDSL', '<{:>', '<:}>') ||
        this.containsAll(msg, 'MetaDsl', '<{:>', '<:}>') ||
        this.containsAll(msg, 'metadsl', '<{:>', '<:}>');
      // @execute blocks: structural detection. A real submission is either a
      // fenced code block whose first non-blank line is a marker line, or a
      // marker line in plain text (unfenced raw code). Prose that merely
      // mentions ``` or // @execute mid-sentence (the model's own syntax
      // self-check, e.g. "首行为 // @execute 识别标签") does not count.
      const structure = this.analyzeMetadslStructure(msg);
      const hasMetaDsl = structure.fencedMetadsl > 0 || structure.textMetadsl > 0;

      if (hasMetaDslResult) {
        return {
          action: 'reply',
          text: `ref <{:>\n${msg}\n<:}>;\n\nmetadsl代码需要使用markdown代码块语法`,
        };
      }
      else if (hasMetaDsl) {
        if (this.containsAll(msg, 'js_request', 'reflect')) {
          if (this.enableLlmPM) {
            return {
              action: 'reply',
              text: `ref <{:>\n${msg}\n<:}>;\n\n已提交反思请求`,
            };
          }
          return { action: 'skip', reason: 'reflect without LlmPM' };
        }
        if (this.containsAny(msg, 'js_request')) {
          return { action: 'skip', reason: 'js_request submitted' };
        }
        if (this.containsAny(msg, 'sleep(')) {
          return { action: 'skip', reason: 'sleep submitted' };
        }
        // Trigger the syntax reminder only when the marker lives INSIDE a
        // markdown fence (structure.fencedMetadsl): same semantics as the old
        // "fence before @execute" positional check, but immune to prose that
        // mentions a fence earlier and the marker later in running text.
        if (structure.fencedMetadsl > 0) {
          return {
            action: 'reply',
            text: `ref <{:>\n${msg}\n<:}>;\n\n请检查metadsl代码是否正确使用了markdown代码块语法，如果没有请重新提交;确认正确请等待执行结果`,
          };
        }
        return { action: 'skip', reason: 'metadsl submitted' };
      }

      // Default for lastFromLLM=true: trigger planning (DSL checks backlog.txt existence).
      // Auto plan disabled and not forced: planning would spam the PM LLM,
      // hold the reply channel still instead.
      if (autoPlanDisabled && !force) {
        return { action: 'skip', reason: 'auto plan disabled' };
      }
      return { action: 'trigger_decision' };
    }

    // 4. Last message not from LLM
    if (this.containsAll(msg, 'MetaDSL', 'hot_reload')) {
      return { action: 'reply', text: '热更完成，继续' };
    }

    // Default for lastFromLLM=false: trigger planning.
    // Auto plan disabled and not forced: same PM spam guard as above.
    if (autoPlanDisabled && !force) {
      return { action: 'skip', reason: 'auto plan disabled' };
    }
    return { action: 'trigger_decision' };
  }
}
