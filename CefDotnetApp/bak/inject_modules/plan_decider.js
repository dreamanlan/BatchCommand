// ============================================================================
// PlanDecider - Decide what to do when agent needs to plan
// Heuristics ported from Script.dsl's agent_need_to_plan branch.
// Decision output (action):
//   - 'skip'          : do nothing
//   - 'reply'         : reply a text to LLM (text in decision.text)
//   - 'command'       : issue inject command (cmd/start_agent or stop_agent)
//   - 'reply_ref'     : reply with ref-wrapped prompt (text in decision.text)
//   - 'trigger_plan'  : ask DSL to run induction_plan (notify agent_need_to_plan)
//   - 'none'          : fall-through, nothing to do
// ============================================================================
class PlanDecider {
  constructor(logger) {
    this.logger = logger || (typeof window !== 'undefined' && window.logger
      ? window.logger.createLogger('PlanDecider')
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
  //         queuedCount, pageType, count, lockAgent }
  decide(data) {
    const d = data || {};
    const msg = typeof d.lastScannedMessage === 'string' ? d.lastScannedMessage : '';
    const msgLen = msg.length;
    const lastFromLLM = (d.lastFromLLM === true || d.lastFromLLM === 'True' || d.lastFromLLM === 'true');
    const queued = typeof d.queuedCount === 'number' ? d.queuedCount : parseInt(d.queuedCount || 0, 10) || 0;

    // 1. Not the latest response -> skip
    if (d.isLastResponse !== true) {
      return { action: 'skip', reason: 'not latest response' };
    }

    // 2. Still has queued operations -> remind LLM to wait
    if (queued > 0) {
      return {
        action: 'reply',
        text: `还有${queued}个代码要执行，不要再发新代码，回复继续即可`,
      };
    }

    // 3. Last message came from LLM
    if (lastFromLLM) {
      if (this.containsAll(msg, '需要', '继续', '吗') && msgLen > 16) {
        return { action: 'reply', text: '继续' };
      }
      if (msg.indexOf('确定吗') >= 0 || msg.indexOf('确认吗') >= 0 || msg.indexOf('修改吗') >= 0) {
        return { action: 'reply', text: '确定' };
      }
      if (this.containsAll(msg, 'Error', 'Occur')) {
        return { action: 'reply', text: '继续' };
      }
      if (this.containsAny(msg, '继续', '等待') && msgLen <= 32) {
        return { action: 'reply', text: '没有代码要执行了' };
      }
      if (msg.indexOf('启动Agent') >= 0 && msgLen <= 32) {
        return { action: 'command', command: 'start_agent' };
      }
      if (msg.indexOf('停止Agent') >= 0 && msgLen <= 32) {
        return { action: 'command', command: 'stop_agent' };
      }

      // MetaDSL / @execute blocks
      const hasMetaDsl =
        this.containsAll(msg, '//', '@execute') ||
        this.containsAll(msg, 'MetaDSL', '{:', ':}') ||
        this.containsAll(msg, 'MetaDsl', '{:', ':}') ||
        this.containsAll(msg, 'metadsl', '{:', ':}');

      if (hasMetaDsl) {
        if (this.containsAll(msg, 'js_request', 'keep_llm_context')) {
          if (this.enableLlmPM) {
            return {
              action: 'reply',
              text: `ref{:\n${msg}\n:};\n\n已提交上下文更新请求，稍后请阅读context.txt与history.txt了解上下文`,
            };
          }
          return { action: 'skip', reason: 'keep_llm_context without LlmPM' };
        }
        if (this.containsAll(msg, 'js_request', 'reflect')) {
          if (this.enableLlmPM) {
            return {
              action: 'reply',
              text: `ref{:\n${msg}\n:};\n\n已提交反思请求`,
            };
          }
          return { action: 'skip', reason: 'reflect without LlmPM' };
        }
        if (this.containsAny(msg, 'js_request', 'js_eval')) {
          return { action: 'skip', reason: 'js_request/js_eval submitted' };
        }
        return {
          action: 'reply',
          text: `ref{:\n${msg}\n:};\n\n请等待代码结果\n\n或者请检查metadsl代码是否使用了markdown代码块语法，如果没有请重新提交`,
        };
      }

      // Default for lastFromLLM=true: trigger planning (DSL checks plan.txt existence)
      return { action: 'trigger_plan' };
    }

    // 4. Last message not from LLM
    if (this.containsAll(msg, 'MetaDSL', 'hot_reload')) {
      return { action: 'reply', text: '热更完成，继续' };
    }

    // Default for lastFromLLM=false: trigger planning
    return { action: 'trigger_plan' };
  }
}
