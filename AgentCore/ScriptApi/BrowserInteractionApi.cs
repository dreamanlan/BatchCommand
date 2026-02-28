using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // Base class for browser interaction expressions with initialization check
    abstract class BrowserInteractionExpBase : SimpleExpressionBase
    {
        protected bool CheckInitialized()
        {
            return Core.AgentCore.IsInitialized;
        }
    }

    // Build query selector
    sealed class BuildQuerySelectorExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_query_selector(selector)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string selector = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildQuerySelector(selector);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildQuerySelector error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build click element script
    sealed class BuildClickElementExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_click_element(selector)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string selector = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildClickElement(selector);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildClickElement error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build set value script
    sealed class BuildSetValueExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_set_value(selector, value)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string selector = operands[0].AsString;
                string value = operands[1].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildSetValue(selector, value);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildSetValue error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build get value script
    sealed class BuildGetValueExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_get_value(selector)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string selector = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildGetValue(selector);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildGetValue error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build get text script
    sealed class BuildGetTextExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_get_text(selector)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string selector = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildGetText(selector);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildGetText error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build set innerHTML script
    sealed class BuildSetInnerHTMLExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_set_innerhtml(selector, html)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string selector = operands[0].AsString;
                string html = operands[1].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildSetInnerHTML(selector, html);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildSetInnerHTML error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build wait for element script
    sealed class BuildWaitForElementExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_wait_for_element(selector[, timeout_def_5000ms])");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string selector = operands[0].AsString;
                int timeout = operands.Count > 1 ? operands[1].GetInt() : 5000;
                string result = Core.AgentCore.Instance.BrowserOps.BuildWaitForElement(selector, timeout);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildWaitForElement error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build scroll to element script
    sealed class BuildScrollToElementExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_scroll_to_element(selector)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string selector = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildScrollToElement(selector);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildScrollToElement error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build is visible script
    sealed class BuildIsVisibleExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_is_visible(selector)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string selector = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildIsVisible(selector);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildIsVisible error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build add class script
    sealed class BuildAddClassExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_add_class(selector, className)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string selector = operands[0].AsString;
                string className = operands[1].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildAddClass(selector, className);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildAddClass error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build remove class script
    sealed class BuildRemoveClassExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_remove_class(selector, className)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string selector = operands[0].AsString;
                string className = operands[1].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildRemoveClass(selector, className);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildRemoveClass error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build set style script
    sealed class BuildSetStyleExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_set_style(selector, property, value)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string selector = operands[0].AsString;
                string property = operands[1].AsString;
                string value = operands[2].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildSetStyle(selector, property, value);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildSetStyle error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build inject CSS script
    sealed class BuildInjectCSSExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_inject_css(css)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string css = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildInjectCSS(css);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildInjectCSS error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build navigate to script
    sealed class BuildNavigateToExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: build_navigate_to(url)");
                return BoxedValue.NullObject;
            }

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try {
                string url = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildNavigateTo(url);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"BuildNavigateTo error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Send JavaScript code to the browser (async, no return value)
    sealed class SendJsCodeExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: send_js_code(jscode)");
                return BoxedValue.FromBool(false);
            }

            if (!CheckInitialized())
                return BoxedValue.FromBool(false);

            try {
                string jscode = operands[0].AsString;
                Core.AgentCore.Instance.BrowserOps.SendJsCode(jscode);
                return BoxedValue.FromBool(true);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"SendJsCode error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }

    // Send JavaScript function call to the browser (async, no return value)
    sealed class SendJsCallExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: send_js_call(jsfunc, arg1, arg2, ...)");
                return BoxedValue.FromBool(false);
            }

            if (!CheckInitialized())
                return BoxedValue.FromBool(false);

            try {
                string funcName = operands[0].AsString;
                string[] args = new string[operands.Count - 1];
                for (int i = 1; i < operands.Count; i++) {
                    args[i - 1] = operands[i].AsString;
                }
                Core.AgentCore.Instance.BrowserOps.SendJsCall(funcName, args);
                return BoxedValue.FromBool(true);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"SendJsCall error: {ex.Message}");
                return BoxedValue.FromBool(false);
            }
        }
    }
}
