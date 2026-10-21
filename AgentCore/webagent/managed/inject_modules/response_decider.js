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
      // @execute blocks
      const hasMetaDsl =
        this.containsAll(msg, '//', '@execute') ||
        this.containsAll(msg, '#', '@execute');

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
        // Trigger the syntax reminder only when a code fence (```) appears
        // BEFORE the @execute marker, i.e. the @execute block is wrapped in a
        // markdown code fence. A fence located after @execute does not count.
        const fenceIdx = msg.indexOf('```');
        const execIdx = msg.indexOf('@execute');
        if (fenceIdx >= 0 && execIdx >= 0 && fenceIdx < execIdx) {
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
