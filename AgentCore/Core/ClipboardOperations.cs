using System;
using TextCopy;

using AgentPlugin.Abstractions;

namespace CefDotnetApp.AgentCore.Core
{
    public class ClipboardOperations
    {
        public string GetText()
        {
            return ClipboardService.GetText() ?? string.Empty;
        }

        public bool SetText(string text)
        {
            if (text == null)
                text = string.Empty;

            ClipboardService.SetText(text);
            return true;
        }

        public bool Clear()
        {
            ClipboardService.SetText(string.Empty);
            return true;
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
