using System.Collections.Generic;
using AbstractAgent;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// start_http_auth_server(listen_port, url, tag)
    /// Starts a generic loopback redirect capturer (provider-agnostic: OAuth 2.0 /
    /// OIDC / any custom sign-in), opens the system browser to the authorization
    /// url (its "%redirect_url%" placeholder is replaced by the URL-encoded redirect_uri
    /// http://127.0.0.1:{port}/), and returns the actual listening port (<=0 on
    /// error). Result arrives via the http_auth_callback CEF message:
    /// (url, tag, token_or_response), where token_or_response is the full raw
    /// query string captured from the redirect (e.g. "code=..&state=..") for the
    /// caller to parse. The server self-destroys once captured (or on timeout /
    /// stop). Use response_type=code: implicit-flow fragments never reach a server.
    /// </summary>
    sealed class StartHttpAuthServerExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("start_http_auth_server requires (listen_port, url, tag)");
                return BoxedValue.From(-1);
            }
            int listenPort = operands[0].GetInt();
            string url = operands[1].AsString;
            string tag = operands[2].AsString;
            int port = AgentCore.Core.HttpAuthServerService.Instance.Start(listenPort, url, tag);
            return BoxedValue.From(port);
        }
    }

    /// <summary>
    /// stop_http_auth_server(port)
    /// Stops a running auth server previously started for the given port.
    /// Delivers "error: cancelled" to the callback. Returns true if found.
    /// </summary>
    sealed class StopHttpAuthServerExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("stop_http_auth_server requires (port)");
                return BoxedValue.FromBool(false);
            }
            int port = operands[0].GetInt();
            return BoxedValue.FromBool(AgentCore.Core.HttpAuthServerService.Instance.Stop(port));
        }
    }

    /// <summary>
    /// Registers HTTP auth server DSL APIs.
    /// </summary>
    public static class HttpAuthServerApi
    {
        public static void RegisterApis()
        {
            AgentFrameworkService.Instance.DslEngine!.Register("start_http_auth_server",
                "start_http_auth_server(listen_port, url, tag) - start a generic loopback redirect capturer (OAuth/OIDC/custom); listen_port=0 auto-picks a free port; url is the authorization URL whose '%redirect_url%' placeholder is replaced by the URL-encoded redirect_uri; opens the system browser; returns the actual port (<=0 on error). Result arrives via cef message http_auth_callback(url, tag, token_or_response) where token_or_response is the full raw redirect query string; the server self-destroys once captured.",
                new ExpressionFactoryHelper<StartHttpAuthServerExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stop_http_auth_server",
                "stop_http_auth_server(port) - stop a running auth server started by start_http_auth_server, delivers 'error: cancelled' to the callback",
                new ExpressionFactoryHelper<StopHttpAuthServerExp>());
        }
    }
}
