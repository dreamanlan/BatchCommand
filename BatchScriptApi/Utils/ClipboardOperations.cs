using System;
using TextCopy;

namespace BatchCommand.Utils
{
    /// <summary>
    /// Clipboard operations backed by the TextCopy library (already a
    /// dependency of this project). Moved from AgentCore.Core; stateless.
    /// </summary>
    public class ClipboardOperations
    {
        public string GetText()
        {
            try {
                return ClipboardService.GetText() ?? string.Empty;
            }
            catch (Exception) {
                return string.Empty;
            }
        }

        public bool SetText(string text)
        {
            if (text == null)
                text = string.Empty;

            try {
                ClipboardService.SetText(text);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public bool Clear()
        {
            try {
                ClipboardService.SetText(string.Empty);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public bool HasText()
        {
            try {
                string? text = ClipboardService.GetText();
                return !string.IsNullOrEmpty(text);
            }
            catch (Exception) {
                return false;
            }
        }
    }
}
