/**
 * js_dialog.js - custom UI for native JavaScript dialogs.
 *
 * Native alert/confirm/prompt dialogs are intercepted on the native side
 * (CefJSDialogHandler -> C# -> DSL). When the DSL takes a dialog over it posts
 * window.__agentDialogShow(payload) into this page; this module renders the
 * dialog and reports the result back through window.cefQuery, which is the only
 * renderer -> browser channel that reaches the DSL instance owning the handle.
 *
 * Payload: { dialogId: string, type: number, message: string, defaultText: string }
 *   type: 0 = alert, 1 = confirm, 2 = prompt, 3 = beforeunload
 *
 * dialogId is a string because it carries a native int64 handle and a JS number
 * only holds 53 bits.
 */
(function () {
  'use strict';

  if (window.AgentDialog) {
    return;
  }

  const TYPE_ALERT = 0;
  const TYPE_CONFIRM = 1;
  const TYPE_PROMPT = 2;
  const TYPE_BEFORE_UNLOAD = 3;

  // dialogId -> { overlay, input }
  const active = {};

  function log(msg) {
    try {
      if (window.console && console.log) {
        console.log('[AgentDialog] ' + msg);
      }
    } catch (e) { /* ignore */ }
  }

  /**
   * Reports the result to the browser process. Completing an unknown handle is
   * a no-op on the native side, so a duplicate report is harmless.
   */
  function report(dialogId, ok, input) {
    const payload = JSON.stringify({
      action: 'js_dialog_result',
      handle: String(dialogId),
      ok: !!ok,
      input: input || ''
    });
    if (typeof window.cefQuery !== 'function') {
      log('cefQuery unavailable, dialog result dropped: ' + payload);
      return;
    }
    window.cefQuery({
      request: payload,
      onSuccess: function () { },
      onFailure: function (code, message) {
        log('cefQuery failed (' + code + '): ' + message);
      }
    });
  }

  function close(dialogId) {
    const entry = active[dialogId];
    if (!entry) {
      return;
    }
    delete active[dialogId];
    if (entry.overlay && entry.overlay.parentNode) {
      entry.overlay.parentNode.removeChild(entry.overlay);
    }
    if (entry.keyHandler) {
      document.removeEventListener('keydown', entry.keyHandler, true);
    }
  }

  function finish(dialogId, ok) {
    const entry = active[dialogId];
    let input = '';
    if (entry && entry.input) {
      input = entry.input.value || '';
    }
    close(dialogId);
    report(dialogId, ok, input);
  }

  function styleOverlay(el) {
    el.style.cssText = [
      'position:fixed', 'left:0', 'top:0', 'right:0', 'bottom:0',
      'background:rgba(0,0,0,0.45)',
      'display:flex', 'align-items:center', 'justify-content:center',
      // Above the injected panels, which use z-index values below 1e6.
      'z-index:2147483000',
      'font-family:-apple-system,Segoe UI,Roboto,sans-serif', 'font-size:14px'
    ].join(';');
  }

  function styleBox(el) {
    el.style.cssText = [
      'min-width:320px', 'max-width:520px',
      'background:#fff', 'color:#222',
      'border-radius:8px', 'box-shadow:0 8px 32px rgba(0,0,0,0.35)',
      'padding:20px 22px 16px 22px'
    ].join(';');
  }

  function styleButton(el, primary) {
    el.style.cssText = [
      'min-width:76px', 'margin-left:8px', 'padding:6px 14px',
      'border-radius:5px', 'cursor:pointer', 'font-size:13px',
      primary ? 'background:#2b6cb0' : 'background:#e2e8f0',
      primary ? 'color:#fff' : 'color:#2d3748',
      'border:1px solid ' + (primary ? '#2b6cb0' : '#cbd5e0')
    ].join(';');
  }

  function titleFor(type) {
    if (type === TYPE_BEFORE_UNLOAD) {
      return 'Leave this page?';
    }
    if (type === TYPE_PROMPT) {
      return 'Input required';
    }
    if (type === TYPE_CONFIRM) {
      return 'Confirm';
    }
    return 'Message';
  }

  /**
   * Renders a dialog. Called from the native side through
   * window.__agentDialogShow.
   */
  function show(opts) {
    if (!opts || !opts.dialogId) {
      log('show called without a dialogId');
      return false;
    }
    const dialogId = String(opts.dialogId);
    const type = typeof opts.type === 'number' ? opts.type : TYPE_CONFIRM;

    // A browser only runs one JS dialog at a time, but be defensive: replace
    // any stale dialog carrying the same id.
    if (active[dialogId]) {
      close(dialogId);
    }
    if (!document.body) {
      log('document.body is not ready, canceling dialog ' + dialogId);
      report(dialogId, false, '');
      return false;
    }

    const overlay = document.createElement('div');
    styleOverlay(overlay);

    const box = document.createElement('div');
    styleBox(box);

    const title = document.createElement('div');
    title.textContent = titleFor(type);
    title.style.cssText = 'font-weight:600;margin-bottom:10px;font-size:15px';
    box.appendChild(title);

    const text = document.createElement('div');
    text.textContent = opts.message || '';
    text.style.cssText = 'white-space:pre-wrap;line-height:1.5;margin-bottom:14px;max-height:40vh;overflow:auto';
    box.appendChild(text);

    let input = null;
    if (type === TYPE_PROMPT) {
      input = document.createElement('input');
      input.type = 'text';
      input.value = opts.defaultText || '';
      input.style.cssText = 'width:100%;box-sizing:border-box;padding:6px 8px;margin-bottom:14px;border:1px solid #cbd5e0;border-radius:4px;font-size:13px';
      box.appendChild(input);
    }

    const buttons = document.createElement('div');
    buttons.style.cssText = 'display:flex;justify-content:flex-end';

    // alert has a single button; every other type is accept/cancel. For
    // beforeunload, accept means "leave the page".
    if (type !== TYPE_ALERT) {
      const cancelBtn = document.createElement('button');
      cancelBtn.textContent = (type === TYPE_BEFORE_UNLOAD) ? 'Stay' : 'Cancel';
      styleButton(cancelBtn, false);
      cancelBtn.addEventListener('click', function () { finish(dialogId, false); });
      buttons.appendChild(cancelBtn);
    }

    const okBtn = document.createElement('button');
    okBtn.textContent = (type === TYPE_BEFORE_UNLOAD) ? 'Leave' : 'OK';
    styleButton(okBtn, true);
    okBtn.addEventListener('click', function () { finish(dialogId, true); });
    buttons.appendChild(okBtn);

    box.appendChild(buttons);
    overlay.appendChild(box);

    // Esc cancels (accepts for alert, which has no cancel state).
    const keyHandler = function (ev) {
      if (ev.key === 'Escape') {
        ev.preventDefault();
        ev.stopPropagation();
        finish(dialogId, type === TYPE_ALERT);
      } else if (ev.key === 'Enter') {
        ev.preventDefault();
        ev.stopPropagation();
        finish(dialogId, true);
      }
    };
    document.addEventListener('keydown', keyHandler, true);

    active[dialogId] = { overlay: overlay, input: input, keyHandler: keyHandler };
    document.body.appendChild(overlay);

    if (input) {
      input.focus();
      input.select();
    } else {
      okBtn.focus();
    }
    log('showing dialog ' + dialogId + ' (type ' + type + ')');
    return true;
  }

  /**
   * Closes a dialog without reporting a result. Used when the native side has
   * already canceled the dialog (navigation reset, browser close).
   */
  function forceClose(dialogId) {
    if (dialogId === undefined || dialogId === null) {
      for (const id in active) {
        if (Object.prototype.hasOwnProperty.call(active, id)) {
          close(id);
        }
      }
      return;
    }
    close(String(dialogId));
  }

  window.AgentDialog = {
    show: show,
    forceClose: forceClose
  };

  // Entry point used by the native side. Kept as a plain global so the DSL can
  // call it with a single expression.
  window.__agentDialogShow = show;

  log('module ready');
})();
