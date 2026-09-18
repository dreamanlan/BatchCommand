using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using BatchCommand;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// Shared helpers for the web result object apis below: unwrap the
    /// WebCallbacks.Result / byte array object references the web callbacks
    /// exchange with the dsl script.
    /// </summary>
    internal static class WebApiHelpers
    {
        public static Core.WebCallbacks.Result? GetResult(BoxedValue v)
        {
            return v.GetObject() as Core.WebCallbacks.Result;
        }

        public static byte[]? GetBytes(BoxedValue v)
        {
            object? obj = v.GetObject();
            return obj switch {
                byte[] b => b,
                IList<byte> list => list.ToArray(),
                _ => null,
            };
        }
    }

    // http_get_request_headers(request) - the headers of the request object
    // passed to on_proxy_request (upstream HttpRequestMessage) or to a web
    // server dsl page main(request, result) (HttpListenerRequest), as
    // {"Name": ["v1", "v2"], ...} (the listener variant also carries Method
    // and Url entries)
    sealed class HttpGetRequestHeadersExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: http_get_request_headers(request)");
                return BoxedValue.NullObject;
            }
            object? obj = operands[0].GetObject();
            if (obj is HttpRequestMessage msg) {
                return BoxedValue.FromObject(Core.WebCallbacks.RequestHeadersToDict(msg));
            }
            if (obj is HttpListenerRequest lreq) {
                return BoxedValue.FromObject(Core.WebCallbacks.ListenerRequestToDict(lreq));
            }
            AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("http_get_request_headers: first argument must be the request object passed to on_proxy_request / a dsl page main(request, result)");
            return BoxedValue.NullObject;
        }
    }

    // http_get_response_headers(response) - the headers of the upstream
    // response object passed to on_proxy_response (the web server static
    // filter passes the dictionary directly, it is returned as-is)
    sealed class HttpGetResponseHeadersExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: http_get_response_headers(response)");
                return BoxedValue.NullObject;
            }
            object? obj = operands[0].GetObject();
            if (obj is HttpResponseMessage msg) {
                return BoxedValue.FromObject(Core.WebCallbacks.ResponseHeadersToDict(msg));
            }
            if (obj is Dictionary<string, List<string>> dict) {
                return BoxedValue.FromObject(dict);
            }
            AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("http_get_response_headers: first argument must be the response object passed to on_proxy_response / on_webserver_response");
            return BoxedValue.NullObject;
        }
    }

    // new_web_result() - construct an empty web result object
    sealed class NewWebResultExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromObject(new Core.WebCallbacks.Result());
        }
    }

    // set_web_result_status(result, status) - override the response status
    sealed class SetWebResultStatusExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: set_web_result_status(result, status)");
                return BoxedValue.FromBool(false);
            }
            var res = WebApiHelpers.GetResult(operands[0]);
            if (res == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("set_web_result_status: first argument must be a web result object (new_web_result)");
                return BoxedValue.FromBool(false);
            }
            int status = operands[1].GetInt();
            if (status < 100 || status > 599) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"set_web_result_status: invalid status code {status}");
                return BoxedValue.FromBool(false);
            }
            res.Status = status;
            res.HasStatus = true;
            return BoxedValue.FromBool(true);
        }
    }

    // set_web_result_header(result, name, value) - add or override a header
    sealed class SetWebResultHeaderExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: set_web_result_header(result, name, value)");
                return BoxedValue.FromBool(false);
            }
            var res = WebApiHelpers.GetResult(operands[0]);
            if (res == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("set_web_result_header: first argument must be a web result object (new_web_result)");
                return BoxedValue.FromBool(false);
            }
            string name = operands[1].AsString ?? string.Empty;
            if (name.Length == 0) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("set_web_result_header: header name must not be empty");
                return BoxedValue.FromBool(false);
            }
            res.SetHeaders[name] = operands[2].AsString ?? string.Empty;
            return BoxedValue.FromBool(true);
        }
    }

    // del_web_result_header(result, name) - remove a header
    sealed class DelWebResultHeaderExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: del_web_result_header(result, name)");
                return BoxedValue.FromBool(false);
            }
            var res = WebApiHelpers.GetResult(operands[0]);
            if (res == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("del_web_result_header: first argument must be a web result object (new_web_result)");
                return BoxedValue.FromBool(false);
            }
            string name = operands[1].AsString ?? string.Empty;
            if (name.Length == 0) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("del_web_result_header: header name must not be empty");
                return BoxedValue.FromBool(false);
            }
            res.DelHeaders.Add(name);
            return BoxedValue.FromBool(true);
        }
    }

    // set_web_result_body(result, bytes) - replace the body with a byte array
    sealed class SetWebResultBodyExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: set_web_result_body(result, bytes)");
                return BoxedValue.FromBool(false);
            }
            var res = WebApiHelpers.GetResult(operands[0]);
            if (res == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("set_web_result_body: first argument must be a web result object (new_web_result)");
                return BoxedValue.FromBool(false);
            }
            byte[]? bytes = WebApiHelpers.GetBytes(operands[1]);
            if (bytes == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("set_web_result_body: second argument must be a byte array (e.g. the callback body argument or string_to_bytes(...))");
                return BoxedValue.FromBool(false);
            }
            res.Body = bytes;
            res.HasBody = true;
            return BoxedValue.FromBool(true);
        }
    }

    // set_web_result_follow_redirects(result, follow) - proxy only: switch to
    // the manual redirect client (follow=false) for this request
    sealed class SetWebResultFollowRedirectsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: set_web_result_follow_redirects(result, follow)");
                return BoxedValue.FromBool(false);
            }
            var res = WebApiHelpers.GetResult(operands[0]);
            if (res == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("set_web_result_follow_redirects: first argument must be a web result object (new_web_result)");
                return BoxedValue.FromBool(false);
            }
            res.FollowRedirects = operands[1].GetBool();
            res.HasFollowRedirects = true;
            return BoxedValue.FromBool(true);
        }
    }

    // set_web_result_abort(result [, abort]) - answer the request directly
    // (with the result status/headers/body) instead of forwarding / serving;
    // abort defaults to true
    sealed class SetWebResultAbortExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: set_web_result_abort(result [, abort])");
                return BoxedValue.FromBool(false);
            }
            var res = WebApiHelpers.GetResult(operands[0]);
            if (res == null) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("set_web_result_abort: first argument must be a web result object (new_web_result)");
                return BoxedValue.FromBool(false);
            }
            res.Abort = operands.Count > 1 ? operands[1].GetBool() : true;
            return BoxedValue.FromBool(true);
        }
    }

    /// <summary>
    /// Registers the web result object apis (shared by the httpproxy /
    /// webserver filter callbacks and the web server dsl pages - see
    /// Core/WebCallbacks.cs).
    /// </summary>
    public static class WebApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("http_get_request_headers", "http_get_request_headers(request) - the headers of the request object passed to on_proxy_request (upstream HttpRequestMessage) or to a web server dsl page main(request, result) (HttpListenerRequest, also carries Method/Url entries), as a {\"Name\": [\"v1\", ...], ...} dictionary, returns null on a wrong argument",
                new ExpressionFactoryHelper<HttpGetRequestHeadersExp>());
            BatchCommand.BatchScript.Register("http_get_response_headers", "http_get_response_headers(response) - the headers of the upstream response object passed to on_proxy_response (or the dictionary passed to on_webserver_response, returned as-is), as a {\"Name\": [\"v1\", ...], ...} dictionary, returns null on a wrong argument",
                new ExpressionFactoryHelper<HttpGetResponseHeadersExp>());
            BatchCommand.BatchScript.Register("new_web_result", "new_web_result() - construct an empty web result object (fill it with the set_web_result_* apis and return it from on_proxy_request / on_proxy_response / on_webserver_response / a dsl page main(request, result); returning null or an empty string means no modification, returning a plain string replaces the body with that text)",
                new ExpressionFactoryHelper<NewWebResultExp>());
            BatchCommand.BatchScript.Register("set_web_result_status", "set_web_result_status(result, status) - override the response status code in a web result object, returns bool",
                new ExpressionFactoryHelper<SetWebResultStatusExp>());
            BatchCommand.BatchScript.Register("set_web_result_header", "set_web_result_header(result, name, value) - add or override a header in a web result object, returns bool",
                new ExpressionFactoryHelper<SetWebResultHeaderExp>());
            BatchCommand.BatchScript.Register("del_web_result_header", "del_web_result_header(result, name) - remove a header in a web result object, returns bool",
                new ExpressionFactoryHelper<DelWebResultHeaderExp>());
            BatchCommand.BatchScript.Register("set_web_result_body", "set_web_result_body(result, bytes) - replace the body with a byte array in a web result object (only honoured when the body was buffered), returns bool",
                new ExpressionFactoryHelper<SetWebResultBodyExp>());
            BatchCommand.BatchScript.Register("set_web_result_follow_redirects", "set_web_result_follow_redirects(result, follow) - proxy only: manual redirect mode for this request (follow=false passes the upstream 3xx through with the Location rewritten to the proxy), returns bool",
                new ExpressionFactoryHelper<SetWebResultFollowRedirectsExp>());
            BatchCommand.BatchScript.Register("set_web_result_abort", "set_web_result_abort(result [, abort]) - answer the request directly with the result status/headers/body instead of forwarding / serving (abort defaults to true), returns bool",
                new ExpressionFactoryHelper<SetWebResultAbortExp>());
        }
    }
}
