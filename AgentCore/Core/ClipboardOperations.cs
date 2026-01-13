using System;
using TextCopy;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    public class ClipboardOperations : IClipboardOperations
    {
        public string GetText()
        {
            try
            {
                return ClipboardService.GetText() ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to get clipboard text", ex);
            }
        }

        public bool SetText(string text)
        {
            try
            {
                if (text == null)
                    text = string.Empty;

                ClipboardService.SetText(text);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to set clipboard text", ex);
            }
        }

        public bool Clear()
        {
            try
            {
                ClipboardService.SetText(string.Empty);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to clear clipboard", ex);
            }
        }

        public bool HasText()
        {
            try
            {
                string text = ClipboardService.GetText();
                return !string.IsNullOrEmpty(text);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
