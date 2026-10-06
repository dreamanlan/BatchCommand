// vedas_responder.js - injected into vedas.woa.com file-preview subframes.
// Purpose: answer code-fetch requests from the parent chat page (ai.woa.com)
// so truncated code blocks in the chat body can be backfilled from the
// full code embedded in this preview iframe.
(function () {
  'use strict';
  if (window.vedasResponderInstalled) return;
  window.vedasResponderInstalled = true; window.vedasResponderVer = 'v2_20261006a';

  function readCodeText(el) {
    // Try line-span format first (markdown renderers split per line).
    var lineSpans = el.querySelectorAll('.code-line');
    if (lineSpans && lineSpans.length > 0) {
      var parts = [];
      for (var i = 0; i < lineSpans.length; i++) {
        parts.push(lineSpans[i].textContent || '');
      }
      return parts.join('');
    }
    return el.textContent || '';
  }

  function collectCode() {
    var codes = [];
    var pres = document.querySelectorAll('pre');
    for (var i = 0; i < pres.length; i++) {
      var text = readCodeText(pres[i]);
      if (text && text.trim()) codes.push(text);
    }
    return codes;
  }

  window.addEventListener('message', function (ev) {
    var data = ev.data;
    if (!data || typeof data !== 'object') return;
    if (data.type !== 'venusllmfetchcode') return;
    var payload = {
      type: 'venusllmfetchcode_result',
      reqId: data.reqId,
      href: location.href,
      codes: collectCode()
    };
    try {
      if (ev.source) ev.source.postMessage(payload, '*');
    } catch (e) { /* ignore cross-origin reply failure */ }
  });
})();
