using System;
using System.Collections.Generic;
using System.Text;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    public class BrowserInteraction : IBrowserInteraction
    {
        private Action<string> _executeJsAction;
        private Action<string, string> _callJsAction;

        public BrowserInteraction(Action<string> executeJsAction = null, Action<string, string> callJsAction = null)
        {
            _executeJsAction = executeJsAction;
            _callJsAction = callJsAction;
        }

        public string BuildQuerySelector(string selector)
        {
            return $"document.querySelector('{EscapeJsString(selector)}')";
        }

        public string BuildQuerySelectorAll(string selector)
        {
            return $"document.querySelectorAll('{EscapeJsString(selector)}')";
        }

        public string BuildGetElementById(string id)
        {
            return $"document.getElementById('{EscapeJsString(id)}')";
        }

        public string BuildGetElementsByClassName(string className)
        {
            return $"document.getElementsByClassName('{EscapeJsString(className)}')";
        }

        public string BuildGetElementsByTagName(string tagName)
        {
            return $"document.getElementsByTagName('{EscapeJsString(tagName)}')";
        }

        public string BuildClickElement(string selector)
        {
            return $"{BuildQuerySelector(selector)}?.click()";
        }

        public string BuildSetValue(string selector, string value)
        {
            return $"{{ const el = {BuildQuerySelector(selector)}; if(el) el.value = '{EscapeJsString(value)}'; }}";
        }

        public string BuildGetValue(string selector)
        {
            return $"{BuildQuerySelector(selector)}?.value";
        }

        public string BuildGetText(string selector)
        {
            return $"{BuildQuerySelector(selector)}?.textContent";
        }

        public string BuildGetInnerHTML(string selector)
        {
            return $"{BuildQuerySelector(selector)}?.innerHTML";
        }

        public string BuildSetInnerHTML(string selector, string html)
        {
            return $"{{ const el = {BuildQuerySelector(selector)}; if(el) el.innerHTML = '{EscapeJsString(html)}'; }}";
        }

        public string BuildGetAttribute(string selector, string attribute)
        {
            return $"{BuildQuerySelector(selector)}?.getAttribute('{EscapeJsString(attribute)}')";
        }

        public string BuildSetAttribute(string selector, string attribute, string value)
        {
            return $"{BuildQuerySelector(selector)}?.setAttribute('{EscapeJsString(attribute)}', '{EscapeJsString(value)}')";
        }

        public string BuildAddClass(string selector, string className)
        {
            return $"{BuildQuerySelector(selector)}?.classList.add('{EscapeJsString(className)}')";
        }

        public string BuildRemoveClass(string selector, string className)
        {
            return $"{BuildQuerySelector(selector)}?.classList.remove('{EscapeJsString(className)}')";
        }

        public string BuildToggleClass(string selector, string className)
        {
            return $"{BuildQuerySelector(selector)}?.classList.toggle('{EscapeJsString(className)}')";
        }

        public string BuildHasClass(string selector, string className)
        {
            return $"{BuildQuerySelector(selector)}?.classList.contains('{EscapeJsString(className)}')";
        }

        public string BuildSetStyle(string selector, string property, string value)
        {
            return $"{{ const el = {BuildQuerySelector(selector)}; if(el) el.style.{property} = '{EscapeJsString(value)}'; }}";
        }

        public string BuildGetStyle(string selector, string property)
        {
            return $"window.getComputedStyle({BuildQuerySelector(selector)})?.{property}";
        }

        public string BuildWaitForElement(string selector, int timeoutMs = 5000)
        {
            return $@"
(function() {{
    return new Promise((resolve, reject) => {{
        const startTime = Date.now();
        const checkElement = () => {{
            const el = {BuildQuerySelector(selector)};
            if (el) {{
                resolve(true);
            }} else if (Date.now() - startTime > {timeoutMs}) {{
                reject(new Error('Timeout waiting for element'));
            }} else {{
                setTimeout(checkElement, 100);
            }}
        }};
        checkElement();
    }});
}})()";
        }

        public string BuildScrollToElement(string selector)
        {
            return $"{BuildQuerySelector(selector)}?.scrollIntoView({{ behavior: 'smooth', block: 'center' }})";
        }

        public string BuildFocusElement(string selector)
        {
            return $"{BuildQuerySelector(selector)}?.focus()";
        }

        public string BuildBlurElement(string selector)
        {
            return $"{BuildQuerySelector(selector)}?.blur()";
        }

        public string BuildIsVisible(string selector)
        {
            return $@"
(function() {{
    const el = {BuildQuerySelector(selector)};
    if (!el) return false;
    const style = window.getComputedStyle(el);
    return style.display !== 'none' && style.visibility !== 'hidden' && style.opacity !== '0';
}})()";
        }

        public string BuildIsEnabled(string selector)
        {
            return $"!{BuildQuerySelector(selector)}?.disabled";
        }

        public string BuildGetElementCount(string selector)
        {
            return $"{BuildQuerySelectorAll(selector)}.length";
        }

        public string BuildGetAllText(string selector)
        {
            return $"Array.from({BuildQuerySelectorAll(selector)}).map(el => el.textContent).join('\\n')";
        }

        public string BuildSimulateKeyPress(string selector, string key)
        {
            return $@"
(function() {{
    const el = {BuildQuerySelector(selector)};
    if (el) {{
        const event = new KeyboardEvent('keydown', {{ key: '{EscapeJsString(key)}', bubbles: true }});
        el.dispatchEvent(event);
    }}
}})()";
        }

        public string BuildSimulateMouseEvent(string selector, string eventType)
        {
            return $@"
(function() {{
    const el = {BuildQuerySelector(selector)};
    if (el) {{
        const event = new MouseEvent('{EscapeJsString(eventType)}', {{ bubbles: true, cancelable: true, view: window }});
        el.dispatchEvent(event);
    }}
}})()";
        }

        public string BuildGetPageTitle()
        {
            return "document.title";
        }

        public string BuildGetPageUrl()
        {
            return "window.location.href";
        }

        public string BuildNavigateTo(string url)
        {
            return $"window.location.href = '{EscapeJsString(url)}'";
        }

        public string BuildReloadPage()
        {
            return "window.location.reload()";
        }

        public string BuildGoBack()
        {
            return "window.history.back()";
        }

        public string BuildGoForward()
        {
            return "window.history.forward()";
        }

        public string BuildExecuteScript(string script)
        {
            return $"(function() {{ {script} }})()";
        }

        public string BuildInjectCSS(string css)
        {
            return $@"
(function() {{
    const style = document.createElement('style');
    style.textContent = '{EscapeJsString(css)}';
    document.head.appendChild(style);
}})()";
        }

        public string BuildRemoveElement(string selector)
        {
            return $"{BuildQuerySelector(selector)}?.remove()";
        }

        public string BuildCreateElement(string tagName, string id = null, string className = null, string innerHTML = null)
        {
            var sb = new StringBuilder();
            sb.Append($"(function() {{ const el = document.createElement('{EscapeJsString(tagName)}');");

            if (!string.IsNullOrEmpty(id))
                sb.Append($" el.id = '{EscapeJsString(id)}';");

            if (!string.IsNullOrEmpty(className))
                sb.Append($" el.className = '{EscapeJsString(className)}';");

            if (!string.IsNullOrEmpty(innerHTML))
                sb.Append($" el.innerHTML = '{EscapeJsString(innerHTML)}';");

            sb.Append(" return el; }})()");
            return sb.ToString();
        }

        public string BuildAppendChild(string parentSelector, string childHtml)
        {
            return $@"
(function() {{
    const parent = {BuildQuerySelector(parentSelector)};
    if (parent) {{
        const temp = document.createElement('div');
        temp.innerHTML = '{EscapeJsString(childHtml)}';
        parent.appendChild(temp.firstChild);
    }}
}})()";
        }

        public string BuildGetFormData(string formSelector)
        {
            return $@"
(function() {{
    const form = {BuildQuerySelector(formSelector)};
    if (!form) return null;
    const formData = new FormData(form);
    const data = {{}};
    for (let [key, value] of formData.entries()) {{
        data[key] = value;
    }}
    return JSON.stringify(data);
}})()";
        }

        public string BuildSetFormData(string formSelector, Dictionary<string, string> data)
        {
            var sb = new StringBuilder();
            sb.Append($@"
(function() {{
    const form = {BuildQuerySelector(formSelector)};
    if (!form) return;
");

            foreach (var kvp in data)
            {
                sb.AppendLine($"    {{ const el = form.querySelector('[name=\"{EscapeJsString(kvp.Key)}\"]'); if(el) el.value = '{EscapeJsString(kvp.Value)}'; }}");
            }

            sb.Append("}})()");
            return sb.ToString();
        }

        public void ExecuteJs(string script)
        {
            _executeJsAction?.Invoke(script);
        }

        public void CallJsFunction(string functionName, string arg)
        {
            _callJsAction?.Invoke(functionName, arg);
        }

        public void SendCommandToInject(string command, object parameters)
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                string paramsJson = System.Text.Json.JsonSerializer.Serialize(parameters, options);
                
                // Build the command object
                var cmd = new
                {
                    command = command,
                    @params = paramsJson
                };
                
                string cmdJson = System.Text.Json.JsonSerializer.Serialize(cmd, options);
                
                // Call inject.js function to handle the command
                _callJsAction?.Invoke("window.onAgentCommand", cmdJson);
            }
            catch (Exception ex)
            {
                // Log error if possible
                System.Diagnostics.Debug.WriteLine($"Error sending command to inject: {ex.Message}");
            }
        }

        // Set or update the execute JavaScript action
        public void SetExecuteJsAction(Action<string> executeJsAction)
        {
            var currentExecuteJs = _executeJsAction;
            _executeJsAction = executeJsAction;
        }

        // Set or update the call JavaScript function action
        public void SetCallJsAction(Action<string, string> callJsAction)
        {
            _callJsAction = callJsAction;
        }


        private string EscapeJsString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str.Replace("\\", "\\\\")
                      .Replace("'", "\\'")
                      .Replace("\"", "\\\"")
                      .Replace("\n", "\\n")
                      .Replace("\r", "\\r")
                      .Replace("\t", "\\t");
        }
    }
}
