using System;
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
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string selector = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildQuerySelector(selector);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildQuerySelector error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build click element script
    sealed class BuildClickElementExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string selector = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildClickElement(selector);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildClickElement error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build set value script
sealed class BuildSetValueExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string selector = operands[0].AsString;
                string value = operands[1].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildSetValue(selector, value);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildSetValue error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build get value script
sealed class BuildGetValueExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string selector = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildGetValue(selector);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildGetValue error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build get text script
sealed class BuildGetTextExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string selector = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildGetText(selector);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildGetText error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build set innerHTML script
sealed class BuildSetInnerHTMLExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string selector = operands[0].AsString;
                string html = operands[1].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildSetInnerHTML(selector, html);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildSetInnerHTML error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build wait for element script
sealed class BuildWaitForElementExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string selector = operands[0].AsString;
                int timeout = operands.Count > 1 ? operands[1].GetInt() : 5000;
                string result = Core.AgentCore.Instance.BrowserOps.BuildWaitForElement(selector, timeout);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildWaitForElement error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build scroll to element script
sealed class BuildScrollToElementExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string selector = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildScrollToElement(selector);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildScrollToElement error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build is visible script
sealed class BuildIsVisibleExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string selector = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildIsVisible(selector);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildIsVisible error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build add class script
sealed class BuildAddClassExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string selector = operands[0].AsString;
                string className = operands[1].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildAddClass(selector, className);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildAddClass error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build remove class script
sealed class BuildRemoveClassExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string selector = operands[0].AsString;
                string className = operands[1].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildRemoveClass(selector, className);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildRemoveClass error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build set style script
sealed class BuildSetStyleExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string selector = operands[0].AsString;
                string property = operands[1].AsString;
                string value = operands[2].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildSetStyle(selector, property, value);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildSetStyle error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build inject CSS script
sealed class BuildInjectCSSExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string css = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildInjectCSS(css);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildInjectCSS error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Build navigate to script
sealed class BuildNavigateToExp : BrowserInteractionExpBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            if (!CheckInitialized())
                return BoxedValue.NullObject;

            try
            {
                string url = operands[0].AsString;
                string result = Core.AgentCore.Instance.BrowserOps.BuildNavigateTo(url);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"BuildNavigateTo error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }
}
