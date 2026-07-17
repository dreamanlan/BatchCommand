using System.Collections.Generic;
using AgentPlugin.Abstractions;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // ---------- Group 1: lifecycle ----------

    /// <summary>
    /// playwright_install([browser])
    /// Download Playwright browser binaries. browser: "chromium" (default), "firefox", "webkit", or "all".
    /// First run may take 1-3 minutes (~150MB for chromium). Returns "ok" or "error: ...".
    /// </summary>
    sealed class PlaywrightInstallExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string browser = operands.Count > 0 ? operands[0].AsString : "chromium";
            try {
                string result = Core.PlaywrightService.Instance.InstallAsync(browser).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_install error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// playwright_start([headless, user_data_dir, cdp_endpoint])
    /// Launch Chromium via Microsoft.Playwright. If cdp_endpoint is non-empty,
    /// connects over CDP (reuses remote browser). Else if user_data_dir non-empty,
    /// uses a persistent context. Idempotent. Returns "ok" or error.
    /// </summary>
    sealed class PlaywrightStartExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            bool headless = operands.Count > 0 ? operands[0].GetBool() : false;
            string userDataDir = operands.Count > 1 ? operands[1].AsString : "";
            string cdpEndpoint = operands.Count > 2 ? operands[2].AsString : "";
            try {
                string result = Core.PlaywrightService.Instance.StartAsync(headless, userDataDir, string.IsNullOrEmpty(cdpEndpoint) ? null : cdpEndpoint).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_start error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// playwright_stop() - close all pages, context and browser. Returns "ok".
    /// </summary>
    sealed class PlaywrightStopExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                string result = Core.PlaywrightService.Instance.StopAsync().GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_stop error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
    /// Returns "ok" or "error: ...".
    /// </summary>
    sealed class PlaywrightNewPageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_new_page requires (page_id)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.NewPageAsync(pageId).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_new_page error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// playwright_close_page(page_id) - close a page. Returns "ok" or error.
    /// </summary>
    sealed class PlaywrightClosePageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_close_page requires (page_id)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.ClosePageAsync(pageId).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_close_page error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// playwright_goto(page_id, url) - navigate. Returns "ok:{status}" or error.
    /// </summary>
    sealed class PlaywrightGotoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_goto requires (page_id, url)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string url = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.GotoAsync(pageId, url).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_goto error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_get_url requires (page_id)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.GetUrlAsync(pageId).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_get_url error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_get_title requires (page_id)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.GetTitleAsync(pageId).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_get_title error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_get_html requires (page_id [, selector])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string selector = operands.Count > 1 ? operands[1].AsString : "";
            try {
                string result = Core.PlaywrightService.Instance.GetHtmlAsync(pageId, selector).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_get_html error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_get_text requires (page_id, selector)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.GetTextAsync(pageId, selector).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_get_text error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_screenshot requires (page_id, save_path)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string savePath = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.ScreenshotAsync(pageId, savePath).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_screenshot error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_evaluate requires (page_id, script)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string script = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.EvaluateAsync(pageId, script).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_evaluate error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_click requires (page_id, selector [, button, click_count])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            string? button = operands.Count > 2 ? operands[2].AsString : null;
            int? clickCount = operands.Count > 3 ? (int?)operands[3].GetInt() : null;
            try {
                string result = Core.PlaywrightService.Instance.ClickAsync(pageId, selector, button, clickCount).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_click error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_type requires (page_id, selector, text [, clear_first])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            string text = operands[2].AsString;
            bool clearFirst = operands.Count > 3 ? operands[3].GetBool() : true;
            try {
                string result = Core.PlaywrightService.Instance.TypeAsync(pageId, selector, text, clearFirst).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_type error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_press_key requires (page_id, key [, selector])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string key = operands[1].AsString;
            string? selector = operands.Count > 2 ? operands[2].AsString : null;
            try {
                string result = Core.PlaywrightService.Instance.PressKeyAsync(pageId, key, selector).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_press_key error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_hover requires (page_id, selector)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.HoverAsync(pageId, selector).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_hover error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_select_option requires (page_id, selector, value_or_json)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            string valueOrJson = operands[2].AsString;
            try {
                string result = Core.PlaywrightService.Instance.SelectOptionAsync(pageId, selector, valueOrJson).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_select_option error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_fill_form requires (page_id, form_json)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string formJson = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.FillFormAsync(pageId, formJson).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_fill_form error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_wait_for requires (page_id, selector [, state, timeout_ms])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            string? state = operands.Count > 2 ? operands[2].AsString : null;
            int? timeoutMs = operands.Count > 3 ? (int?)operands[3].GetInt() : null;
            try {
                string result = Core.PlaywrightService.Instance.WaitForAsync(pageId, selector, state, timeoutMs).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_wait_for error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_navigate_back requires (page_id)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.NavigateBackAsync(pageId).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_navigate_back error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_resize requires (page_id, width, height)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            int width = operands[1].GetInt();
            int height = operands[2].GetInt();
            try {
                string result = Core.PlaywrightService.Instance.ResizeAsync(pageId, width, height).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_resize error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_handle_dialog requires (page_id, action [, prompt_text])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string action = operands[1].AsString;
            string? promptText = operands.Count > 2 ? operands[2].AsString : null;
            try {
                string result = Core.PlaywrightService.Instance.HandleDialogAsync(pageId, action, promptText);
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_handle_dialog error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_tabs error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_console_messages requires (page_id [, max_count])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            int? maxCount = operands.Count > 1 ? (int?)operands[1].GetInt() : null;
            try {
                string result = Core.PlaywrightService.Instance.ConsoleMessagesAsync(pageId, maxCount);
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_console_messages error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_drag requires (page_id, from_selector, to_selector)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string fromSel = operands[1].AsString;
            string toSel = operands[2].AsString;
            try {
                string result = Core.PlaywrightService.Instance.DragAsync(pageId, fromSel, toSel).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_drag error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_file_upload requires (page_id, selector, paths_json)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            string pathsJson = operands[2].AsString;
            try {
                string result = Core.PlaywrightService.Instance.FileUploadAsync(pageId, selector, pathsJson).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_file_upload error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_drop requires (page_id, selector, file_name, mime_type, content)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            string fileName = operands[2].AsString;
            string mimeType = operands[3].AsString;
            string content = operands[4].AsString;
            try {
                string result = Core.PlaywrightService.Instance.DropAsync(pageId, selector, fileName, mimeType, content).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_drop error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_find requires (page_id, selector [, max_count])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string selector = operands[1].AsString;
            int? maxCount = operands.Count > 2 ? (int?)operands[2].GetInt() : null;
            try {
                string result = Core.PlaywrightService.Instance.FindAsync(pageId, selector, maxCount).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_find error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_network_requests requires (page_id [, max_count])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            int? maxCount = operands.Count > 1 ? (int?)operands[1].GetInt() : null;
            try {
                string result = Core.PlaywrightService.Instance.NetworkRequestsAsync(pageId, maxCount);
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_network_requests error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_network_request requires (page_id, url_substring)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string pageId = operands[0].AsString;
            string urlSub = operands[1].AsString;
            try {
                string result = Core.PlaywrightService.Instance.NetworkRequestAsync(pageId, urlSub);
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_network_request error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
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
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_cdp_new_tab error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }
    sealed class PlaywrightCdpCloseTabExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_cdp_close_tab requires (target_id)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string targetId = operands[0].AsString;
            try {
                string result = Core.PlaywrightService.Instance.CdpCloseTabAsync(targetId).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_cdp_close_tab error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }
    sealed class PlaywrightCdpListTargetsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                string result = Core.PlaywrightService.Instance.CdpListTargetsAsync().GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_cdp_list_targets error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    sealed class PlaywrightSearchWebExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_search_web error: need at least page_id and query");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_search_web error: {ex.Message}");
                return BoxedValue.FromObject(new List<BoxedValue>());
            }
        }
    }

    sealed class PlaywrightSearchSetOptionExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_search_set_option error: need key and val");
                return BoxedValue.FromString("error");
            }
            string key = operands[0].AsString;
            string val = operands[1].AsString;
            try {
                Core.PlaywrightService.Instance.SetSearchOption(key, val);
                return BoxedValue.FromString("ok");
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_search_set_option error: {ex.Message}");
                return BoxedValue.FromString("error");
            }
        }
    }

    sealed class PlaywrightSearchGetOptionExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("playwright_search_get_option error: need key");
                return BoxedValue.FromString(string.Empty);
            }
            string key = operands[0].AsString;
            try {
                return BoxedValue.FromString(Core.PlaywrightService.Instance.GetSearchOption(key));
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"playwright_search_get_option error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    // ---------- Registration ----------

    public static class PlaywrightApi
    {
        public static void RegisterApis()
        {
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_install",
                "playwright_install([browser]) - download Playwright browser binaries. browser: 'chromium' (default), 'firefox', 'webkit', or 'all'. First run ~1-3 min. Returns 'ok' or 'error: ...'.",
                new ExpressionFactoryHelper<PlaywrightInstallExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_start",
                "playwright_start([headless, user_data_dir, cdp_endpoint]) - launch Chromium; cdp_endpoint non-empty=connect over CDP (reuse remote), else user_data_dir empty=fresh context, non-empty=persistent context. Returns 'ok' or error.",
                new ExpressionFactoryHelper<PlaywrightStartExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_stop",
                "playwright_stop() - close all pages, context and browser. Returns 'ok'.",
                new ExpressionFactoryHelper<PlaywrightStopExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_is_running",
                "playwright_is_running() - returns bool indicating whether playwright is started.",
                new ExpressionFactoryHelper<PlaywrightIsRunningExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_new_page",
                "playwright_new_page(page_id) - create a new page with the given string handle. Returns 'ok' or 'error: ...'.",
                new ExpressionFactoryHelper<PlaywrightNewPageExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_close_page",
                "playwright_close_page(page_id) - close a page. Returns 'ok' or error.",
                new ExpressionFactoryHelper<PlaywrightClosePageExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_goto",
                "playwright_goto(page_id, url) - navigate the page. Returns 'ok:{status}' or error.",
                new ExpressionFactoryHelper<PlaywrightGotoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_get_url",
                "playwright_get_url(page_id) - return current URL of the page.",
                new ExpressionFactoryHelper<PlaywrightGetUrlExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_get_title",
                "playwright_get_title(page_id) - return document.title.",
                new ExpressionFactoryHelper<PlaywrightGetTitleExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_get_html",
                "playwright_get_html(page_id [, selector]) - full HTML if selector empty; else InnerHTML of selector.",
                new ExpressionFactoryHelper<PlaywrightGetHtmlExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_get_text",
                "playwright_get_text(page_id, selector) - InnerText of the located element.",
                new ExpressionFactoryHelper<PlaywrightGetTextExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_screenshot",
                "playwright_screenshot(page_id, save_path) - full-page PNG. save_path must be absolute.",
                new ExpressionFactoryHelper<PlaywrightScreenshotExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_evaluate",
                "playwright_evaluate(page_id, script) - [Advanced] run JS in page context. Prefer high-level APIs (playwright_click/type/fill/search_web/locator) first; use this only when high-level APIs cannot cover the case.",
                new ExpressionFactoryHelper<PlaywrightEvaluateExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_click",
                "playwright_click(page_id, selector [, button, click_count]) - click element. button: 'left'|'right'|'middle'.",
                new ExpressionFactoryHelper<PlaywrightClickExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_type",
                "playwright_type(page_id, selector, text [, clear_first]) - type text; clear_first default true.",
                new ExpressionFactoryHelper<PlaywrightTypeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_press_key",
                "playwright_press_key(page_id, key [, selector]) - press keyboard key; selector empty=page-level.",
                new ExpressionFactoryHelper<PlaywrightPressKeyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_hover",
                "playwright_hover(page_id, selector) - hover on element.",
                new ExpressionFactoryHelper<PlaywrightHoverExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_select_option",
                "playwright_select_option(page_id, selector, value_or_json) - value single or JSON array of values.",
                new ExpressionFactoryHelper<PlaywrightSelectOptionExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_fill_form",
                "playwright_fill_form(page_id, form_json) - JSON dict {selector: value} to fill multiple fields.",
                new ExpressionFactoryHelper<PlaywrightFillFormExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_wait_for",
                "playwright_wait_for(page_id, selector [, state, timeout_ms]) - state: 'visible'|'hidden'|'attached'|'detached'.",
                new ExpressionFactoryHelper<PlaywrightWaitForExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_navigate_back",
                "playwright_navigate_back(page_id) - navigate history back.",
                new ExpressionFactoryHelper<PlaywrightNavigateBackExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_resize",
                "playwright_resize(page_id, width, height) - resize viewport pixels.",
                new ExpressionFactoryHelper<PlaywrightResizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_handle_dialog",
                "playwright_handle_dialog(page_id, action [, prompt_text]) - action: 'accept'|'dismiss'; prompt_text for accept-with-input.",
                new ExpressionFactoryHelper<PlaywrightHandleDialogExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_tabs",
                "playwright_tabs([refresh]) - list all pages as 'id|url' semicolon separated; refresh=true syncs pages from context (CDP-attach case).",
                new ExpressionFactoryHelper<PlaywrightTabsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_console_messages",
                "playwright_console_messages(page_id [, max_count]) - recent console log entries; max_count default 100.",
                new ExpressionFactoryHelper<PlaywrightConsoleMessagesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_drag",
                "playwright_drag(page_id, from_selector, to_selector) - drag element from source to target selector.",
                new ExpressionFactoryHelper<PlaywrightDragExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_file_upload",
                "playwright_file_upload(page_id, selector, paths_json) - upload files; paths_json JSON array of absolute paths or single path string.",
                new ExpressionFactoryHelper<PlaywrightFileUploadExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_drop",
                "playwright_drop(page_id, selector, file_name, mime_type, content) - simulate file drop via DataTransfer + dispatchEvent.",
                new ExpressionFactoryHelper<PlaywrightDropExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_find",
                "playwright_find(page_id, selector [, max_count]) - locate elements; returns count + first N inner_text lines (default 5, max 20).",
                new ExpressionFactoryHelper<PlaywrightFindExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_network_requests",
                "playwright_network_requests(page_id [, max_count]) - recent network activity (REQ/RES/ERR lines); max_count default 100.",
                new ExpressionFactoryHelper<PlaywrightNetworkRequestsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_network_request",
                "playwright_network_request(page_id, url_substring) - latest network entry containing url_substring.",
                new ExpressionFactoryHelper<PlaywrightNetworkRequestExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_cdp_new_tab",
                "playwright_cdp_new_tab([url]) - open new tab via CDP Target.createTarget (attach mode); returns targetId or error.",
                new ExpressionFactoryHelper<PlaywrightCdpNewTabExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_cdp_close_tab",
                "playwright_cdp_close_tab(target_id) - close tab via CDP Target.closeTarget (attach mode).",
                new ExpressionFactoryHelper<PlaywrightCdpCloseTabExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_cdp_list_targets",
                "playwright_cdp_list_targets() - list CDP targets via HTTP /json (attach mode); returns raw JSON.",
                new ExpressionFactoryHelper<PlaywrightCdpListTargetsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_search_web",
                "playwright_search_web(page_id, query [, page_index, max_results, timeout_ms]) - search on current site of given tab (site auto-detected by host: google/bing/baidu). page_index 0-based (default 0 uses fill+Enter, >0 uses direct URL). max_results default 10, timeout_ms default 30000. Returns List of dict{title,url,snippet}; empty on error.",
                new ExpressionFactoryHelper<PlaywrightSearchWebExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_search_set_option",
                "playwright_search_set_option(key, val) - set search option. key format: '{site}.{field}' (e.g. 'google.page_size', 'bing.item_selector'). fields: host_pattern, query_selector, submit_button_selector, page_url_template, results_wait_selector, item_selector, title_selector, url_selector, snippet_selector, page_size. Returns 'ok' or 'error'.",
                new ExpressionFactoryHelper<PlaywrightSearchSetOptionExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("playwright_search_get_option",
                "playwright_search_get_option(key) - get search option value. key format: '{site}.{field}'. Returns option string or empty on missing/error.",
                new ExpressionFactoryHelper<PlaywrightSearchGetOptionExp>());
        }
    }
}
