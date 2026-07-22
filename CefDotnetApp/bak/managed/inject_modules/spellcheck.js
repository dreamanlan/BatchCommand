// ============================================================================
// Spellcheck Management
// ============================================================================
/**
 * Disable spellcheck globally to prevent Blink rendering crashes
 *
 * Background:
 * - Blink's spellcheck rendering (HighlightPainter::FastPaintSpellingGrammarDecorations)
 *   can crash with DCHECK errors when accessing DOM during layout/paint phase
 * - This happens especially during paste operations or rapid DOM mutations
 * - Disabling spellcheck prevents Blink from entering the problematic code path
 *
 * This is a more reliable solution than trying to disable our monitor during
 * Blink's internal rendering phases, which we cannot fully control.
/**
 * Globally disable spellcheck on all contenteditable elements to prevent Blink rendering crashes
 * This is a critical workaround for crashes in HighlightPainter::FastPaintSpellingGrammarDecorations
 */
function disableSpellcheckGlobally() {
  logger.info('Disabling spellcheck globally to prevent Blink crashes...');

  // Disable spellcheck on all existing contenteditable elements
  const editableElements = document.querySelectorAll('[contenteditable="true"]');
  editableElements.forEach(element => {
    element.setAttribute('spellcheck', 'false');
    logger.debug('Disabled spellcheck on element', { id: element.id, className: element.className, tagName: element.tagName });
  });

  // Monitor for new contenteditable elements and disable spellcheck on them
  const spellcheckObserver = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
      mutation.addedNodes.forEach((node) => {
        if (node.nodeType === Node.ELEMENT_NODE) {
          // Check if the node itself is contenteditable
          if (node.getAttribute('contenteditable') === 'true') {
            node.setAttribute('spellcheck', 'false');
            logger.debug('Disabled spellcheck on new element', { id: node.id, className: node.className, tagName: node.tagName });
          }

          // Check for contenteditable descendants
          const editableDescendants = node.querySelectorAll('[contenteditable="true"]');
          editableDescendants.forEach(element => {
            element.setAttribute('spellcheck', 'false');
            logger.debug('Disabled spellcheck on new descendant', { id: element.id, className: element.className, tagName: element.tagName });
          });
        }
      });

      // Also check for attribute changes (element becoming contenteditable)
      if (mutation.type === 'attributes' && mutation.attributeName === 'contenteditable') {
        const element = mutation.target;
        if (element.getAttribute('contenteditable') === 'true') {
          element.setAttribute('spellcheck', 'false');
          logger.debug('Disabled spellcheck on modified element', { id: element.id, className: element.className, tagName: element.tagName });
        }
      }
    });
  });

  // Start observing
  spellcheckObserver.observe(document.body, {
    childList: true,
    subtree: true,
    attributes: true,
    attributeFilter: ['contenteditable']
  });

  logger.info('Spellcheck globally disabled, monitoring for new contenteditable elements');
}
