using System.Collections.Generic;

using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace AgentCore.ScriptApi
{
    // Maps a Playwright service status text to a semantic api return value.
    // Statuses: "ok" (success), "ok:<data>" (success with payload), anything else is an error.
    // Errors go to the api error info channel, never into the return value.
    static class PwApi
    {
        public static BoxedValue Bool(string status)
        {
            if (status == null)
                return BoxedValue.From(false);
            if (status == "ok")
                return BoxedValue.From(true);
            AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine(status);
            return BoxedValue.From(false);
        }

        public static BoxedValue Text(string status)
        {
            if (status == null)
                return BoxedValue.FromString(string.Empty);
            if (status == "ok")
                return BoxedValue.FromString(string.Empty);
            int idx = status.IndexOf("ok:", System.StringComparison.Ordinal);
            if (idx == 0)
                return BoxedValue.FromString(status.Substring(3).TrimStart('\n'));
            AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine(status);
            return BoxedValue.FromString(string.Empty);
        }
    }

    // ---------- Group 1: lifecycle ----------

    /// <summary>
    /// playwright_install([browser])
    /// Download Playwright browser binaries. browser: "chromium" (default), "firefox", "webkit", or "all".
    /// First run may take 1-3 minutes (~150MB for chromium). Returns true/false; errors go to the api error info.
    /// </summary>
    sealed class PlaywrightInstallExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string browser = operands.Count > 0 ? operands[0].AsString : "chromium";
            try {
                string result = Core.PlaywrightService.Instance.InstallAsync(browser).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_install error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_start([headless, user_data_dir])
    /// Launch Chromium via Microsoft.Playwright. If user_data_dir non-empty,
    /// uses a persistent context. Idempotent. Returns true/false; errors go to the api error info.
    /// </summary>
    sealed class PlaywrightStartExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            bool headless = operands.Count > 0 ? operands[0].GetBool() : false;
            string userDataDir = operands.Count > 1 ? operands[1].AsString : "";
            try {
                string result = Core.PlaywrightService.Instance.StartAsync(headless, userDataDir, null).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_start error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_cdp_start(cdp_endpoint)
    /// Launch Chromium via Microsoft.Playwright and connects over CDP (reuses remote browser).
    /// Returns true/false; errors go to the api error info.
    /// </summary>
    sealed class PlaywrightCdpStartExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: playwright_cdp_start(cdp_endpoint)");
                return BoxedValue.From(false);
            }
            string cdpEndpoint = operands[0].AsString;
            if (string.IsNullOrEmpty(cdpEndpoint)) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_cdp_start: cdp_endpoint is empty");
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_cdp_start: cdp_endpoint is empty");
                return BoxedValue.From(false);
            }
            try {
                string result = Core.PlaywrightService.Instance.StartAsync(false, string.Empty, cdpEndpoint).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_start error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_stop() - close all pages, context and browser. Returns true/false; errors go to the api error info.
    /// </summary>
    sealed class PlaywrightStopExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                string result = Core.PlaywrightService.Instance.StopAsync().GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_stop error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_is_running() - returns bool.
    /// </summary>
    sealed class PlaywrightIsRunningExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromBool(Core.PlaywrightService.Instance.IsRunning());
        }
    }

    // ---------- Group 2: page ops ----------

    /// <summary>
    /// playwright_new_page(page_id) - create a new page with the given handle.
    /// Returns true/false; errors go to the api error info.
    /// </summary>
    sealed class PlaywrightNewPageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_new_page requires (page_id)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.NewPageAsync(pageId).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_new_page error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_close_page(page_id) - close a page. Returns true/false; errors go to the api error info.
    /// </summary>
    sealed class PlaywrightClosePageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_close_page requires (page_id)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.ClosePageAsync(pageId).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_close_page error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_goto(page_id, url) - navigate. Returns the HTTP status code; errors go to the api error info.
    /// </summary>
    sealed class PlaywrightGotoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_goto requires (page_id, url)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string url = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.GotoAsync(pageId, url).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_goto error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    // ---------- Group 3: content read ----------

    /// <summary>
    /// playwright_get_url(page_id) - return current URL of the page.
    /// </summary>
    sealed class PlaywrightGetUrlExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_get_url requires (page_id)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.GetUrlAsync(pageId).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_url error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// playwright_get_title(page_id) - return document.title.
    /// </summary>
    sealed class PlaywrightGetTitleExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_get_title requires (page_id)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.GetTitleAsync(pageId).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_title error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// playwright_get_html(page_id [, selector])
    /// If selector is empty, returns page.ContentAsync() (full HTML).
    /// Otherwise returns InnerHTML of the located element.
    /// </summary>
    sealed class PlaywrightGetHtmlExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_get_html requires (page_id [, selector])");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string selector = operands.Count > 1 ? operands[1].AsString : "";
            try {
                string result = Core.PlaywrightService.Instance.GetHtmlAsync(pageId, selector).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_html error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// playwright_get_text(page_id, selector) - InnerText of the located element.
    /// </summary>
    sealed class PlaywrightGetTextExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_get_text requires (page_id, selector)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.GetTextAsync(pageId, selector).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_text error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// playwright_screenshot(page_id, save_path) - full-page PNG. save_path must be absolute.
    /// </summary>
    sealed class PlaywrightScreenshotExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_screenshot requires (page_id, save_path)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string savePath = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.ScreenshotAsync(pageId, savePath).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_screenshot error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// playwright_evaluate(page_id, script) - run JS in page context, return result as string.
    /// </summary>
    sealed class PlaywrightEvaluateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_evaluate requires (page_id, script)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string script = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.EvaluateAsync(pageId, script).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_evaluate error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// playwright_click(page_id, selector [, button, click_count]) - click element.
    /// </summary>
    sealed class PlaywrightClickExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_click requires (page_id, selector [, button, click_count])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            string? button = operands.Count > 2 ? operands[2].AsString : null;
            int? clickCount = operands.Count > 3 ? (int?)operands[3].GetInt() : null;
            try {
                string result = Core.PlaywrightService.Instance.ClickAsync(pageId, selector, button, clickCount).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_click error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_type(page_id, selector, text [, clear_first]) - type text into element.
    /// </summary>
    sealed class PlaywrightTypeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_type requires (page_id, selector, text [, clear_first])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            string text = operands[2].AsString;
            bool clearFirst = operands.Count > 3 ? operands[3].GetBool() : true;
            try {
                string result = Core.PlaywrightService.Instance.TypeAsync(pageId, selector, text, clearFirst).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_type error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_press_key(page_id, key [, selector]) - press keyboard key.
    /// </summary>
    sealed class PlaywrightPressKeyExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_press_key requires (page_id, key [, selector])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string key = operands[1].AsString;
            string? selector = operands.Count > 2 ? operands[2].AsString : null;
            try {
                string result = Core.PlaywrightService.Instance.PressKeyAsync(pageId, key, selector).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_press_key error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_hover(page_id, selector) - hover on element.
    /// </summary>
    sealed class PlaywrightHoverExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_hover requires (page_id, selector)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.HoverAsync(pageId, selector).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_hover error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_select_option(page_id, selector, value_or_json) - select dropdown option(s).
    /// </summary>
    sealed class PlaywrightSelectOptionExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_select_option requires (page_id, selector, value_or_json)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            string valueOrJson = operands[2].AsString;
            try {
                string result = Core.PlaywrightService.Instance.SelectOptionAsync(pageId, selector, valueOrJson).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_select_option error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// playwright_fill_form(page_id, form_json) - fill form fields from JSON dict.
    /// </summary>
    sealed class PlaywrightFillFormExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_fill_form requires (page_id, form_json)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string formJson = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.FillFormAsync(pageId, formJson).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_fill_form error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_wait_for(page_id, selector [, state, timeout_ms]) - wait for selector state.
    /// </summary>
    sealed class PlaywrightWaitForExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_wait_for requires (page_id, selector [, state, timeout_ms])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            string? state = operands.Count > 2 ? operands[2].AsString : null;
            int? timeoutMs = operands.Count > 3 ? (int?)operands[3].GetInt() : null;
            try {
                string result = Core.PlaywrightService.Instance.WaitForAsync(pageId, selector, state, timeoutMs).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_wait_for error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_navigate_back(page_id) - navigate history back.
    /// </summary>
    sealed class PlaywrightNavigateBackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_navigate_back requires (page_id)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.NavigateBackAsync(pageId).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_navigate_back error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// playwright_resize(page_id, width, height) - resize viewport.
    /// </summary>
    sealed class PlaywrightResizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_resize requires (page_id, width, height)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            int width = operands[1].GetInt();
            int height = operands[2].GetInt();
            try {
                string result = Core.PlaywrightService.Instance.ResizeAsync(pageId, width, height).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_resize error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_handle_dialog(page_id, action [, prompt_text]) - preset dialog handler.
    /// </summary>
    sealed class PlaywrightHandleDialogExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_handle_dialog requires (page_id, action [, prompt_text])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string action = operands[1].AsString;
            string? promptText = operands.Count > 2 ? operands[2].AsString : null;
            try {
                string result = Core.PlaywrightService.Instance.HandleDialogAsync(pageId, action, promptText);
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_handle_dialog error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_tabs() - list all pages as 'id|url' semicolon separated.
    /// </summary>
    sealed class PlaywrightTabsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                bool refresh = operands.Count > 0 && operands[0].GetBool();
                string result = Core.PlaywrightService.Instance.TabsAsync(refresh).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_tabs error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// playwright_console_messages(page_id [, max_count]) - return recent console log entries.
    /// </summary>
    sealed class PlaywrightConsoleMessagesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_console_messages requires (page_id [, max_count])");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            int? maxCount = operands.Count > 1 ? (int?)operands[1].GetInt() : null;
            try {
                string result = Core.PlaywrightService.Instance.ConsoleMessagesAsync(pageId, maxCount);
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_console_messages error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }


    /// <summary>
    /// playwright_drag(page_id, from_selector, to_selector)
    /// </summary>
    sealed class PlaywrightDragExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_drag requires (page_id, from_selector, to_selector)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string fromSel = operands[1].AsString;
            string toSel = operands[2].AsString;
            try {
                string result = Core.PlaywrightService.Instance.DragAsync(pageId, fromSel, toSel).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_drag error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_file_upload(page_id, selector, paths_json)
    /// </summary>
    sealed class PlaywrightFileUploadExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_file_upload requires (page_id, selector, paths_json)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            string pathsJson = operands[2].AsString;
            try {
                string result = Core.PlaywrightService.Instance.FileUploadAsync(pageId, selector, pathsJson).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_file_upload error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_drop(page_id, selector, file_name, mime_type, content)
    /// </summary>
    sealed class PlaywrightDropExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 5) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_drop requires (page_id, selector, file_name, mime_type, content)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            string fileName = operands[2].AsString;
            string mimeType = operands[3].AsString;
            string content = operands[4].AsString;
            try {
                string result = Core.PlaywrightService.Instance.DropAsync(pageId, selector, fileName, mimeType, content).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_drop error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    /// <summary>
    /// playwright_find(page_id, selector [, max_count])
    /// </summary>
    sealed class PlaywrightFindExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_find requires (page_id, selector [, max_count])");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            int? maxCount = operands.Count > 2 ? (int?)operands[2].GetInt() : null;
            try {
                string result = Core.PlaywrightService.Instance.FindAsync(pageId, selector, maxCount).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_find error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// playwright_network_requests(page_id [, max_count])
    /// </summary>
    sealed class PlaywrightNetworkRequestsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_network_requests requires (page_id [, max_count])");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            int? maxCount = operands.Count > 1 ? (int?)operands[1].GetInt() : null;
            try {
                string result = Core.PlaywrightService.Instance.NetworkRequestsAsync(pageId, maxCount);
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_network_requests error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// playwright_network_request(page_id, url_substring)
    /// </summary>
    sealed class PlaywrightNetworkRequestExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_network_request requires (page_id, url_substring)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string urlSub = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.NetworkRequestAsync(pageId, urlSub);
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_network_request error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightCdpNewTabExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string url = operands.Count > 0 ? operands[0].AsString : "about:blank";
            try {
                string result = Core.PlaywrightService.Instance.CdpNewTabAsync(url).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_cdp_new_tab error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightCdpCloseTabExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_cdp_close_tab requires (target_id)");
                return BoxedValue.From(false);
            }
            string targetId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.CdpCloseTabAsync(targetId).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_cdp_close_tab error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightCdpListTargetsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                string result = Core.PlaywrightService.Instance.CdpListTargetsAsync().GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_cdp_list_targets error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightCdpEvaluateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_cdp_evaluate requires (target_id, script [, timeout_ms])");
                return BoxedValue.FromString(string.Empty);
            }
            string targetId = operands[0].AsString;
            string script = operands[1].AsString;
            int timeoutMs = operands.Count > 2 ? operands[2].GetInt() : 30000;
            try {
                string result = Core.PlaywrightService.Instance.CdpEvaluateAsync(targetId, script, timeoutMs).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_cdp_evaluate error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightFramesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_frames requires (page_id)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.FramesAsync(pageId).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_frames error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightFrameEvaluateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_frame_evaluate requires (page_id, frame_url_regex, script)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string frameUrlRegex = operands[1].AsString;
            string script = operands[2].AsString;
            try {
                string result = Core.PlaywrightService.Instance.FrameEvaluateAsync(pageId, frameUrlRegex, script).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_frame_evaluate error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightInputValueExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_input_value requires (page_id, selector)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.InputValueAsync(pageId, selector).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_input_value error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightWaitForLoadStateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_wait_for_load_state requires (page_id [, state, timeout_ms])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string? state = operands.Count > 1 ? operands[1].AsString : null;
            int? timeoutMs = operands.Count > 2 ? operands[2].GetInt() : (int?)null;
            try {
                string result = Core.PlaywrightService.Instance.WaitForLoadStateAsync(pageId, state, timeoutMs).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_wait_for_load_state error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightBringToFrontExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_bring_to_front requires (page_id)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.BringToFrontAsync(pageId).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_bring_to_front error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightGetCookiesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string? urlFilter = operands.Count > 0 ? operands[0].AsString : null;
            try {
                string result = Core.PlaywrightService.Instance.GetCookiesAsync(urlFilter).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_cookies error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightSetCookiesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_set_cookies requires (cookies_json)");
                return BoxedValue.From(false);
            }
            string cookiesJson = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.SetCookiesAsync(cookiesJson).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_set_cookies error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightGetByRoleExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_get_by_role requires (page_id, role [, name, timeout_ms])");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string role = operands[1].AsString;
            string? name = operands.Count > 2 ? operands[2].AsString : null;
            int? timeoutMs = operands.Count > 3 ? operands[3].GetInt() : (int?)null;
            try {
                string result = Core.PlaywrightService.Instance.GetByRoleAsync(pageId, role, name, timeoutMs).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_by_role error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightMouseWheelExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_mouse_wheel requires (page_id, dx, dy)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            float dx = (float)operands[1].GetInt();
            float dy = (float)operands[2].GetInt();
            try {
                string result = Core.PlaywrightService.Instance.MouseWheelAsync(pageId, dx, dy).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_mouse_wheel error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightAddInitScriptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_add_init_script requires (page_id, script)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string script = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.AddInitScriptAsync(pageId, script).GetAwaiter().GetResult();
                return PwApi.Bool(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_add_init_script error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightGetByTextExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_get_by_text requires (page_id, text [, exact, timeout_ms])");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string text = operands[1].AsString;
            bool exact = operands.Count > 2 && operands[2].GetBool();
            int? timeoutMs = operands.Count > 3 ? operands[3].GetInt() : (int?)null;
            try {
                string result = Core.PlaywrightService.Instance.GetByTextAsync(pageId, text, exact, timeoutMs).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_by_text error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightGetByLabelExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_get_by_label requires (page_id, label [, exact, timeout_ms])");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string label = operands[1].AsString;
            bool exact = operands.Count > 2 && operands[2].GetBool();
            int? timeoutMs = operands.Count > 3 ? operands[3].GetInt() : (int?)null;
            try {
                string result = Core.PlaywrightService.Instance.GetByLabelAsync(pageId, label, exact, timeoutMs).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_by_label error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightGetByPlaceholderExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_get_by_placeholder requires (page_id, placeholder [, exact, timeout_ms])");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string placeholder = operands[1].AsString;
            bool exact = operands.Count > 2 && operands[2].GetBool();
            int? timeoutMs = operands.Count > 3 ? operands[3].GetInt() : (int?)null;
            try {
                string result = Core.PlaywrightService.Instance.GetByPlaceholderAsync(pageId, placeholder, exact, timeoutMs).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_by_placeholder error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightGetByTestIdExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_get_by_testid requires (page_id, testid [, timeout_ms])");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string testId = operands[1].AsString;
            int? timeoutMs = operands.Count > 2 ? operands[2].GetInt() : (int?)null;
            try {
                string result = Core.PlaywrightService.Instance.GetByTestIdAsync(pageId, testId, timeoutMs).GetAwaiter().GetResult();
                return PwApi.Text(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_by_testid error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightFrameClickExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_frame_click requires (page_id, frame_url_regex, selector [, button, click_count])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string frameUrlRegex = operands[1].AsString;
            string selector = operands[2].AsString;
            string? button = operands.Count > 3 ? operands[3].AsString : null;
            int? clickCount = operands.Count > 4 ? operands[4].GetInt() : (int?)null;
            try {
                var res = Core.PlaywrightService.Instance.FrameClickAsync(pageId, frameUrlRegex, selector, button, clickCount).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_frame_click error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightFrameTypeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 4) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_frame_type requires (page_id, frame_url_regex, selector, text [, clear_first])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string frameUrlRegex = operands[1].AsString;
            string selector = operands[2].AsString;
            string text = operands[3].AsString;
            bool clearFirst = operands.Count > 4 && operands[4].GetBool();
            try {
                var res = Core.PlaywrightService.Instance.FrameTypeAsync(pageId, frameUrlRegex, selector, text, clearFirst).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_frame_type error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightFrameFillExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 4) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_frame_fill requires (page_id, frame_url_regex, selector, value)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string frameUrlRegex = operands[1].AsString;
            string selector = operands[2].AsString;
            string value = operands[3].AsString;
            try {
                var res = Core.PlaywrightService.Instance.FrameFillAsync(pageId, frameUrlRegex, selector, value).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_frame_fill error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightRouteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_route requires (page_id, url_pattern, action [, body])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string urlPattern = operands[1].AsString;
            string action = operands[2].AsString;
            string? body = operands.Count > 3 ? operands[3].AsString : null;
            try {
                var res = Core.PlaywrightService.Instance.RouteAsync(pageId, urlPattern, action, body).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_route error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightUnrouteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_unroute requires (page_id, url_pattern)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string urlPattern = operands[1].AsString;
            try {
                var res = Core.PlaywrightService.Instance.UnrouteAsync(pageId, urlPattern).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_unroute error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightClearCookiesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                var res = Core.PlaywrightService.Instance.ClearCookiesAsync().GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_clear_cookies error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightKeyboardDownExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_keyboard_down requires (page_id, key)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string key = operands[1].AsString;
            try {
                var res = Core.PlaywrightService.Instance.KeyboardDownAsync(pageId, key).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_keyboard_down error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightKeyboardUpExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_keyboard_up requires (page_id, key)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string key = operands[1].AsString;
            try {
                var res = Core.PlaywrightService.Instance.KeyboardUpAsync(pageId, key).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_keyboard_up error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightKeyboardPressExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_keyboard_press requires (page_id, key[, delay_ms])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string key = operands[1].AsString;
            int? delayMs = operands.Count > 2 ? (int?)operands[2].GetInt() : null;
            try {
                var res = Core.PlaywrightService.Instance.KeyboardPressAsync(pageId, key, delayMs).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_keyboard_press error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightKeyboardTypeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_keyboard_type requires (page_id, text[, delay_ms])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string text = operands[1].AsString;
            int? delayMs = operands.Count > 2 ? (int?)operands[2].GetInt() : null;
            try {
                var res = Core.PlaywrightService.Instance.KeyboardTypeAsync(pageId, text, delayMs).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_keyboard_type error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightGetLocalStorageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_get_local_storage requires (page_id)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            try {
                var res = Core.PlaywrightService.Instance.GetLocalStorageAsync(pageId).GetAwaiter().GetResult();
                return PwApi.Text(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_local_storage error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightSetLocalStorageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_set_local_storage requires (page_id, key, value)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string key = operands[1].AsString;
            string value = operands[2].AsString;
            try {
                var res = Core.PlaywrightService.Instance.SetLocalStorageAsync(pageId, key, value).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_set_local_storage error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightClearLocalStorageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_clear_local_storage requires (page_id)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            try {
                var res = Core.PlaywrightService.Instance.ClearLocalStorageAsync(pageId).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_clear_local_storage error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightGetSessionStorageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_get_session_storage requires (page_id)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            try {
                var res = Core.PlaywrightService.Instance.GetSessionStorageAsync(pageId).GetAwaiter().GetResult();
                return PwApi.Text(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_session_storage error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightClearSessionStorageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_clear_session_storage requires (page_id)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            try {
                var res = Core.PlaywrightService.Instance.ClearSessionStorageAsync(pageId).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_clear_session_storage error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightMouseMoveExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_mouse_move requires (page_id, x, y)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            float x = (float)operands[1].GetDouble();
            float y = (float)operands[2].GetDouble();
            try {
                var res = Core.PlaywrightService.Instance.MouseMoveAsync(pageId, x, y).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_mouse_move error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightMouseDownExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_mouse_down requires (page_id [, button])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string? button = operands.Count > 1 ? operands[1].AsString : null;
            try {
                var res = Core.PlaywrightService.Instance.MouseDownAsync(pageId, button).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_mouse_down error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightMouseUpExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_mouse_up requires (page_id [, button])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string? button = operands.Count > 1 ? operands[1].AsString : null;
            try {
                var res = Core.PlaywrightService.Instance.MouseUpAsync(pageId, button).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_mouse_up error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightMouseClickExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_mouse_click requires (page_id, x, y [, button, click_count])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            float x = (float)operands[1].GetDouble();
            float y = (float)operands[2].GetDouble();
            string? button = operands.Count > 3 ? operands[3].AsString : null;
            int? clickCount = operands.Count > 4 ? (int?)operands[4].GetInt() : null;
            try {
                var res = Core.PlaywrightService.Instance.MouseClickAsync(pageId, x, y, button, clickCount).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_mouse_click error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightMouseDblclickExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_mouse_dblclick requires (page_id, x, y)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            float x = (float)operands[1].GetDouble();
            float y = (float)operands[2].GetDouble();
            try {
                var res = Core.PlaywrightService.Instance.MouseDblClickAsync(pageId, x, y).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_mouse_dblclick error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightWaitForUrlExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_wait_for_url requires (page_id, url_regex [, timeout_ms])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string urlRegex = operands[1].AsString;
            int? timeoutMs = operands.Count > 2 ? (int?)operands[2].GetInt() : null;
            try {
                var res = Core.PlaywrightService.Instance.WaitForUrlAsync(pageId, urlRegex, timeoutMs).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_wait_for_url error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightWaitForTimeoutExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_wait_for_timeout requires (page_id, timeout_ms)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            int timeoutMs = operands[1].GetInt();
            try {
                var res = Core.PlaywrightService.Instance.WaitForTimeoutAsync(pageId, timeoutMs).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_wait_for_timeout error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightSetViewportSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_set_viewport_size requires (page_id, width, height)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            int width = operands[1].GetInt();
            int height = operands[2].GetInt();
            try {
                var res = Core.PlaywrightService.Instance.SetViewportSizeAsync(pageId, width, height).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_set_viewport_size error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightGetViewportSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_get_viewport_size requires (page_id)");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            try {
                var res = Core.PlaywrightService.Instance.GetViewportSizeAsync(pageId).GetAwaiter().GetResult();
                return PwApi.Text(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_get_viewport_size error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightFocusExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_focus requires (page_id, selector)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            try {
                var res = Core.PlaywrightService.Instance.FocusAsync(pageId, selector).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_focus error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightBlurExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_blur requires (page_id, selector)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            try {
                var res = Core.PlaywrightService.Instance.BlurAsync(pageId, selector).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_blur error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightCdpSendExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_cdp_send requires (page_id, method [, params_json])");
                return BoxedValue.FromString(string.Empty);
            }
            string pageId = operands[0].AsString;
            string method = operands[1].AsString;
            string? paramsJson = operands.Count > 2 ? operands[2].AsString : null;
            try {
                var res = Core.PlaywrightService.Instance.CdpSendAsync(pageId, method, paramsJson).GetAwaiter().GetResult();
                return PwApi.Text(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_cdp_send error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }
    sealed class PlaywrightSessionStorageSetExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_session_storage_set requires (page_id, key, value)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string key = operands[1].AsString;
            string value = operands[2].AsString;
            try {
                var res = Core.PlaywrightService.Instance.SessionStorageSetAsync(pageId, key, value).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_session_storage_set error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightWaitForResponseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_wait_for_response requires (page_id, url_regex [, timeout_ms])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string urlRegex = operands[1].AsString;
            int? timeoutMs = operands.Count > 2 ? (int?)operands[2].GetInt() : null;
            try {
                var res = Core.PlaywrightService.Instance.WaitForResponseAsync(pageId, urlRegex, timeoutMs).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_wait_for_response error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightEmulateMediaExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_emulate_media requires (page_id [, media, color_scheme])");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string? media = operands.Count > 1 ? operands[1].AsString : null;
            string? colorScheme = operands.Count > 2 ? operands[2].AsString : null;
            try {
                var res = Core.PlaywrightService.Instance.EmulateMediaAsync(pageId, media, colorScheme).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_emulate_media error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
    sealed class PlaywrightScrollIntoViewExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_scroll_into_view requires (page_id, selector)");
                return BoxedValue.From(false);
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            try {
                var res = Core.PlaywrightService.Instance.ScrollIntoViewAsync(pageId, selector).GetAwaiter().GetResult();
                return PwApi.Bool(res);
            }
            catch (Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_scroll_into_view error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    sealed class PlaywrightSearchWebExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_search_web error: need at least page_id and query");
                return BoxedValue.FromObject(new List<BoxedValue>());
            }
            string pageId = operands[0].AsString;
            string query = operands[1].AsString;
            int pageIndex = operands.Count > 2 ? operands[2].GetInt() : 0;
            int maxResults = operands.Count > 3 ? operands[3].GetInt() : 10;
            int? timeoutMs = operands.Count > 4 ? (int?)operands[4].GetInt() : null;
            try {
                var rows = Core.PlaywrightService.Instance.SearchWebAsync(pageId, query, pageIndex, maxResults, timeoutMs).GetAwaiter().GetResult();
                var list = new List<BoxedValue>(rows.Count);
                foreach (var row in rows) {
                    var dict = new Dictionary<string, BoxedValue>();
                    foreach (var kv in row) dict[kv.Key] = BoxedValue.FromString(kv.Value);
                    list.Add(BoxedValue.FromObject(dict));
                }
                return BoxedValue.FromObject(list);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_search_web error: {ex.Message}");
                return BoxedValue.FromObject(new List<BoxedValue>());
            }
        }
    }

    sealed class PlaywrightSearchSetOptionExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_search_set_option error: need key and val");
                return BoxedValue.From(false);
            }
            string key = operands[0].AsString;
            string val = operands[1].AsString;
            try {
                Core.PlaywrightService.Instance.SetSearchOption(key, val);
                return BoxedValue.From(true);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_search_set_option error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    sealed class PlaywrightSearchGetOptionExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("playwright_search_get_option error: need key");
                return BoxedValue.FromString(string.Empty);
            }
            string key = operands[0].AsString;
            try {
                return BoxedValue.FromString(Core.PlaywrightService.Instance.GetSearchOption(key));
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"playwright_search_get_option error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    // ---------- Registration ----------

    public static class PlaywrightApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("playwright_install",
                "playwright_install([browser]) - download Playwright browser binaries. browser: 'chromium' (default), 'firefox', 'webkit', or 'all'. First run ~1-3 min. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightInstallExp>());
            BatchCommand.BatchScript.Register("playwright_start",
                "playwright_start([headless, user_data_dir]) - Launch Chromium; if `headless` is `true`, headless mode is used (default is `false`). If `user_data_dir` is empty (default), create a fresh context, or if it is not empty, use a persistent context. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightStartExp>());
            BatchCommand.BatchScript.Register("playwright_cdp_start",
                "playwright_cdp_start(cdp_endpoint) - Launch Chromium; connect via CDP (reusing a remote instance). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightCdpStartExp>());
            BatchCommand.BatchScript.Register("playwright_stop",
                "playwright_stop() - close all pages, context and browser. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightStopExp>());
            BatchCommand.BatchScript.Register("playwright_is_running",
                "playwright_is_running() - returns bool indicating whether playwright is started.",
                new ExpressionFactoryHelper<PlaywrightIsRunningExp>());
            BatchCommand.BatchScript.Register("playwright_new_page",
                "playwright_new_page(page_id) - create a new page with the given string handle. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightNewPageExp>());
            BatchCommand.BatchScript.Register("playwright_close_page",
                "playwright_close_page(page_id) - close a page. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightClosePageExp>());
            BatchCommand.BatchScript.Register("playwright_goto",
                "playwright_goto(page_id, url) - navigate the page. Returns the HTTP status code; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightGotoExp>());
            BatchCommand.BatchScript.Register("playwright_get_url",
                "playwright_get_url(page_id) - return current URL of the page.",
                new ExpressionFactoryHelper<PlaywrightGetUrlExp>());
            BatchCommand.BatchScript.Register("playwright_get_title",
                "playwright_get_title(page_id) - return document.title.",
                new ExpressionFactoryHelper<PlaywrightGetTitleExp>());
            BatchCommand.BatchScript.Register("playwright_get_html",
                "playwright_get_html(page_id [, selector]) - full HTML if selector empty; else InnerHTML of selector.",
                new ExpressionFactoryHelper<PlaywrightGetHtmlExp>());
            BatchCommand.BatchScript.Register("playwright_get_text",
                "playwright_get_text(page_id, selector) - InnerText of the located element.",
                new ExpressionFactoryHelper<PlaywrightGetTextExp>());
            BatchCommand.BatchScript.Register("playwright_screenshot",
                "playwright_screenshot(page_id, save_path) - full-page PNG. save_path must be absolute.",
                new ExpressionFactoryHelper<PlaywrightScreenshotExp>());
            BatchCommand.BatchScript.Register("playwright_evaluate",
                "playwright_evaluate(page_id, script) - [Advanced] run JS in page context. Prefer high-level APIs (playwright_click/type/fill/search_web/locator) first; use this only when high-level APIs cannot cover the case.",
                new ExpressionFactoryHelper<PlaywrightEvaluateExp>());
            BatchCommand.BatchScript.Register("playwright_click",
                "playwright_click(page_id, selector [, button, click_count]) - click element. button: 'left'|'right'|'middle'.",
                new ExpressionFactoryHelper<PlaywrightClickExp>());
            BatchCommand.BatchScript.Register("playwright_type",
                "playwright_type(page_id, selector, text [, clear_first]) - type text; clear_first default true.",
                new ExpressionFactoryHelper<PlaywrightTypeExp>());
            BatchCommand.BatchScript.Register("playwright_press_key",
                "playwright_press_key(page_id, key [, selector]) - press keyboard key; selector empty=page-level.",
                new ExpressionFactoryHelper<PlaywrightPressKeyExp>());
            BatchCommand.BatchScript.Register("playwright_hover",
                "playwright_hover(page_id, selector) - hover on element.",
                new ExpressionFactoryHelper<PlaywrightHoverExp>());
            BatchCommand.BatchScript.Register("playwright_select_option",
                "playwright_select_option(page_id, selector, value_or_json) - value single or JSON array of values.",
                new ExpressionFactoryHelper<PlaywrightSelectOptionExp>());
            BatchCommand.BatchScript.Register("playwright_fill_form",
                "playwright_fill_form(page_id, form_json) - JSON dict {selector: value} to fill multiple fields.",
                new ExpressionFactoryHelper<PlaywrightFillFormExp>());
            BatchCommand.BatchScript.Register("playwright_wait_for",
                "playwright_wait_for(page_id, selector [, state, timeout_ms]) - state: 'visible'|'hidden'|'attached'|'detached'.",
                new ExpressionFactoryHelper<PlaywrightWaitForExp>());
            BatchCommand.BatchScript.Register("playwright_navigate_back",
                "playwright_navigate_back(page_id) - navigate history back.",
                new ExpressionFactoryHelper<PlaywrightNavigateBackExp>());
            BatchCommand.BatchScript.Register("playwright_resize",
                "playwright_resize(page_id, width, height) - resize viewport pixels.",
                new ExpressionFactoryHelper<PlaywrightResizeExp>());
            BatchCommand.BatchScript.Register("playwright_handle_dialog",
                "playwright_handle_dialog(page_id, action [, prompt_text]) - action: 'accept'|'dismiss'; prompt_text for accept-with-input.",
                new ExpressionFactoryHelper<PlaywrightHandleDialogExp>());
            BatchCommand.BatchScript.Register("playwright_tabs",
                "playwright_tabs([refresh]) - list all pages as 'id|url' semicolon separated; refresh=true syncs pages from context (CDP-attach case).",
                new ExpressionFactoryHelper<PlaywrightTabsExp>());
            BatchCommand.BatchScript.Register("playwright_console_messages",
                "playwright_console_messages(page_id [, max_count]) - recent console log entries; max_count default 100.",
                new ExpressionFactoryHelper<PlaywrightConsoleMessagesExp>());
            BatchCommand.BatchScript.Register("playwright_drag",
                "playwright_drag(page_id, from_selector, to_selector) - drag element from source to target selector.",
                new ExpressionFactoryHelper<PlaywrightDragExp>());
            BatchCommand.BatchScript.Register("playwright_file_upload",
                "playwright_file_upload(page_id, selector, paths_json) - upload files; paths_json JSON array of absolute paths or single path string.",
                new ExpressionFactoryHelper<PlaywrightFileUploadExp>());
            BatchCommand.BatchScript.Register("playwright_drop",
                "playwright_drop(page_id, selector, file_name, mime_type, content) - simulate file drop via DataTransfer + dispatchEvent.",
                new ExpressionFactoryHelper<PlaywrightDropExp>());
            BatchCommand.BatchScript.Register("playwright_find",
                "playwright_find(page_id, selector [, max_count]) - locate elements; returns count + first N inner_text lines (default 5, max 20).",
                new ExpressionFactoryHelper<PlaywrightFindExp>());
            BatchCommand.BatchScript.Register("playwright_network_requests",
                "playwright_network_requests(page_id [, max_count]) - recent network activity (REQ/RES/ERR lines); max_count default 100.",
                new ExpressionFactoryHelper<PlaywrightNetworkRequestsExp>());
            BatchCommand.BatchScript.Register("playwright_network_request",
                "playwright_network_request(page_id, url_substring) - latest network entry containing url_substring.",
                new ExpressionFactoryHelper<PlaywrightNetworkRequestExp>());
            BatchCommand.BatchScript.Register("playwright_cdp_new_tab",
                "playwright_cdp_new_tab([url]) - open new tab via CDP Target.createTarget (attach mode); returns the targetId; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightCdpNewTabExp>());
            BatchCommand.BatchScript.Register("playwright_cdp_close_tab",
                "playwright_cdp_close_tab(target_id) - close tab via CDP Target.closeTarget (attach mode).",
                new ExpressionFactoryHelper<PlaywrightCdpCloseTabExp>());
            BatchCommand.BatchScript.Register("playwright_cdp_list_targets",
                "playwright_cdp_list_targets() - list CDP targets via HTTP /json (attach mode); returns raw JSON.",
                new ExpressionFactoryHelper<PlaywrightCdpListTargetsExp>());
            BatchCommand.BatchScript.Register("playwright_cdp_evaluate",
                "playwright_cdp_evaluate(target_id, script [, timeout_ms]) - evaluate JS in specific CDP target (attach mode, bypasses cross-origin isolation for iframes/webviews). timeout_ms default 30000. Returns JSON result string.",
                new ExpressionFactoryHelper<PlaywrightCdpEvaluateExp>());
            BatchCommand.BatchScript.Register("playwright_frames",
                "playwright_frames(page_id) - list all frames of a page. Returns JSON array of {url, name, is_main}.",
                new ExpressionFactoryHelper<PlaywrightFramesExp>());
            BatchCommand.BatchScript.Register("playwright_frame_evaluate",
                "playwright_frame_evaluate(page_id, frame_url_regex, script) - evaluate JS in first frame whose URL matches regex. Returns JSON result string.",
                new ExpressionFactoryHelper<PlaywrightFrameEvaluateExp>());
            BatchCommand.BatchScript.Register("playwright_input_value",
                "playwright_input_value(page_id, selector) - get current value of an input/select/textarea element. Returns the string value.",
                new ExpressionFactoryHelper<PlaywrightInputValueExp>());
            BatchCommand.BatchScript.Register("playwright_wait_for_load_state",
                "playwright_wait_for_load_state(page_id [, state, timeout_ms]) - wait for page load state. state: 'load'|'domcontentloaded'|'networkidle' (default 'load'). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightWaitForLoadStateExp>());
            BatchCommand.BatchScript.Register("playwright_bring_to_front",
                "playwright_bring_to_front(page_id) - bring a tab/page to front (focus). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightBringToFrontExp>());
            BatchCommand.BatchScript.Register("playwright_get_cookies",
                "playwright_get_cookies([url_filter]) - get cookies at context level. Optional url_filter narrows to matching URL. Returns JSON array.",
                new ExpressionFactoryHelper<PlaywrightGetCookiesExp>());
            BatchCommand.BatchScript.Register("playwright_set_cookies",
                "playwright_set_cookies(cookies_json) - set cookies at context level. cookies_json: JSON array of cookie objects. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightSetCookiesExp>());
            BatchCommand.BatchScript.Register("playwright_get_by_role",
                "playwright_get_by_role(page_id, role [, name, timeout_ms]) - locate first element by ARIA role, optional accessible name. Returns 'count|html' text; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightGetByRoleExp>());
            BatchCommand.BatchScript.Register("playwright_mouse_wheel",
                "playwright_mouse_wheel(page_id, dx, dy) - scroll by mouse wheel delta pixels. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightMouseWheelExp>());
            BatchCommand.BatchScript.Register("playwright_add_init_script",
                "playwright_add_init_script(page_id, script) - inject an init script that runs on every navigation (only affects subsequent navigations). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightAddInitScriptExp>());
            BatchCommand.BatchScript.Register("playwright_get_by_text",
                "playwright_get_by_text(page_id, text [, exact, timeout_ms]) - locate first element by visible text. exact default false. Returns 'count|html'; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightGetByTextExp>());
            BatchCommand.BatchScript.Register("playwright_get_by_label",
                "playwright_get_by_label(page_id, label [, exact, timeout_ms]) - locate form field by associated label text. exact default false. Returns 'count|html'; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightGetByLabelExp>());
            BatchCommand.BatchScript.Register("playwright_get_by_placeholder",
                "playwright_get_by_placeholder(page_id, placeholder [, exact, timeout_ms]) - locate input by placeholder attribute. exact default false. Returns 'count|html'; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightGetByPlaceholderExp>());
            BatchCommand.BatchScript.Register("playwright_get_by_testid",
                "playwright_get_by_testid(page_id, testid [, timeout_ms]) - locate element by data-testid attribute (or configured test id). Returns 'count|html'; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightGetByTestIdExp>());
            BatchCommand.BatchScript.Register("playwright_frame_click",
                "playwright_frame_click(page_id, frame_url_regex, selector [, button, click_count]) - click element inside iframe matched by url regex. button: left|right|middle (default left). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightFrameClickExp>());
            BatchCommand.BatchScript.Register("playwright_frame_type",
                "playwright_frame_type(page_id, frame_url_regex, selector, text [, clear_first]) - type text into element inside iframe. clear_first=true uses fill, false uses press_sequentially. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightFrameTypeExp>());
            BatchCommand.BatchScript.Register("playwright_frame_fill",
                "playwright_frame_fill(page_id, frame_url_regex, selector, value) - fill form field inside iframe (replaces existing value). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightFrameFillExp>());
            BatchCommand.BatchScript.Register("playwright_route",
                "playwright_route(page_id, url_pattern, action [, body]) - intercept network requests matching url_pattern (glob or regex). action: abort|fulfill|continue. body used with fulfill (status 200). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightRouteExp>());
            BatchCommand.BatchScript.Register("playwright_unroute",
                "playwright_unroute(page_id, url_pattern) - remove previously registered route handler. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightUnrouteExp>());
            BatchCommand.BatchScript.Register("playwright_clear_cookies",
                "playwright_clear_cookies() - clear all cookies of default browser context. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightClearCookiesExp>());
            BatchCommand.BatchScript.Register("playwright_keyboard_down",
                "playwright_keyboard_down(page_id, key) - dispatch a keydown at page level (no selector, no focus change). key like 'Shift','Control','A'. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightKeyboardDownExp>());
            BatchCommand.BatchScript.Register("playwright_keyboard_up",
                "playwright_keyboard_up(page_id, key) - dispatch a keyup at page level. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightKeyboardUpExp>());
            BatchCommand.BatchScript.Register("playwright_keyboard_press",
                "playwright_keyboard_press(page_id, key [, delay_ms]) - press+release a key at page level (down+up). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightKeyboardPressExp>());
            BatchCommand.BatchScript.Register("playwright_keyboard_type",
                "playwright_keyboard_type(page_id, text [, delay_ms]) - type text at page level (no selector, uses current focus). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightKeyboardTypeExp>());
            BatchCommand.BatchScript.Register("playwright_get_local_storage",
                "playwright_get_local_storage(page_id) - read all localStorage entries as JSON string {key:value,...}. Returns JSON text (empty on error); errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightGetLocalStorageExp>());
            BatchCommand.BatchScript.Register("playwright_set_local_storage",
                "playwright_set_local_storage(page_id, key, value) - set a single localStorage entry. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightSetLocalStorageExp>());
            BatchCommand.BatchScript.Register("playwright_clear_local_storage",
                "playwright_clear_local_storage(page_id) - clear all localStorage of current origin. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightClearLocalStorageExp>());
            BatchCommand.BatchScript.Register("playwright_get_session_storage",
                "playwright_get_session_storage(page_id) - read all sessionStorage entries as JSON string {key:value,...}. Returns JSON text (empty on error); errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightGetSessionStorageExp>());
            BatchCommand.BatchScript.Register("playwright_clear_session_storage",
                "playwright_clear_session_storage(page_id) - clear all sessionStorage of current origin. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightClearSessionStorageExp>());
            BatchCommand.BatchScript.Register("playwright_mouse_move",
                "playwright_mouse_move(page_id, x, y) - move mouse to (x,y). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightMouseMoveExp>());
            BatchCommand.BatchScript.Register("playwright_mouse_down",
                "playwright_mouse_down(page_id [, button]) - press mouse button (left/right/middle, default left) at current position. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightMouseDownExp>());
            BatchCommand.BatchScript.Register("playwright_mouse_up",
                "playwright_mouse_up(page_id [, button]) - release mouse button (left/right/middle, default left) at current position. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightMouseUpExp>());
            BatchCommand.BatchScript.Register("playwright_mouse_click",
                "playwright_mouse_click(page_id, x, y [, button, click_count]) - click at (x,y) with button (default left) and click_count (default 1). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightMouseClickExp>());
            BatchCommand.BatchScript.Register("playwright_mouse_dblclick",
                "playwright_mouse_dblclick(page_id, x, y) - double-click at (x,y). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightMouseDblclickExp>());
            BatchCommand.BatchScript.Register("playwright_wait_for_url",
                "playwright_wait_for_url(page_id, url_regex [, timeout_ms]) - wait until page URL matches regex (default timeout 30000). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightWaitForUrlExp>());
            BatchCommand.BatchScript.Register("playwright_wait_for_timeout",
                "playwright_wait_for_timeout(page_id, timeout_ms) - wait for specified milliseconds. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightWaitForTimeoutExp>());
            BatchCommand.BatchScript.Register("playwright_set_viewport_size",
                "playwright_set_viewport_size(page_id, width, height) - set viewport size in pixels. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightSetViewportSizeExp>());
            BatchCommand.BatchScript.Register("playwright_get_viewport_size",
                "playwright_get_viewport_size(page_id) - get viewport size as JSON {width,height}. Returns JSON text (empty on error); errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightGetViewportSizeExp>());
            BatchCommand.BatchScript.Register("playwright_focus",
                "playwright_focus(page_id, selector) - focus the element matching selector. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightFocusExp>());
            BatchCommand.BatchScript.Register("playwright_blur",
                "playwright_blur(page_id, selector) - blur the element matching selector. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightBlurExp>());
            BatchCommand.BatchScript.Register("playwright_cdp_send",
                "playwright_cdp_send(page_id, method [, params_json]) - send raw CDP command via new CDP session. Returns JSON text; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightCdpSendExp>());
            BatchCommand.BatchScript.Register("playwright_session_storage_set",
                "playwright_session_storage_set(page_id, key, value) - set sessionStorage item. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightSessionStorageSetExp>());
            BatchCommand.BatchScript.Register("playwright_wait_for_response",
                "playwright_wait_for_response(page_id, url_regex [, timeout_ms]) - wait for a network response whose URL matches regex. Returns JSON text; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightWaitForResponseExp>());
            BatchCommand.BatchScript.Register("playwright_emulate_media",
                "playwright_emulate_media(page_id [, media, color_scheme]) - emulate CSS media (screen/print) and color scheme (light/dark/no-preference). Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightEmulateMediaExp>());
            BatchCommand.BatchScript.Register("playwright_scroll_into_view",
                "playwright_scroll_into_view(page_id, selector) - scroll the element matching selector into view if needed. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightScrollIntoViewExp>());
            BatchCommand.BatchScript.Register("playwright_search_web",
                "playwright_search_web(page_id, query [, page_index, max_results, timeout_ms]) - search on current site of given tab (site auto-detected by host: google/bing/baidu). page_index 0-based (default 0 uses fill+Enter, >0 uses direct URL). max_results default 10, timeout_ms default 30000. Returns List of dict{title,url,snippet}; empty on error.",
                new ExpressionFactoryHelper<PlaywrightSearchWebExp>());
            BatchCommand.BatchScript.Register("playwright_search_set_option",
                "playwright_search_set_option(key, val) - set search option. key format: '{site}.{field}' (e.g. 'google.page_size', 'bing.item_selector'). fields: host_pattern, query_selector, submit_button_selector, page_url_template, results_wait_selector, item_selector, title_selector, url_selector, snippet_selector, page_size. Returns true/false; errors go to the api error info.",
                new ExpressionFactoryHelper<PlaywrightSearchSetOptionExp>());
            BatchCommand.BatchScript.Register("playwright_search_get_option",
                "playwright_search_get_option(key) - get search option value. key format: '{site}.{field}'. Returns option string or empty on missing/error.",
                new ExpressionFactoryHelper<PlaywrightSearchGetOptionExp>());
        }
    }
}
