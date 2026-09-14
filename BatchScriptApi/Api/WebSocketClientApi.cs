using System.Collections.Generic;
using BatchCommand.Utils;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace BatchCommand.Api
{
    /// <summary>
    /// Generic websocket CLIENT api set (shared by all hosts; the manager
    /// core lives in BatchCommand.Utils.WebSocketClientManager):
    ///
    ///   wsclient_open(url[, id])     -> client id ('' on error / id in use)
    ///   wsclient_send(id, message)   -> true when queued
    ///   wsclient_close(id)           -> cooperative close
    ///   wsclient_close_all()         -> closed count
    ///   wsclient_count() / state(id) / list()
    ///   handle_wsclient_queue([n])   -> drain events on the main thread
    ///
    /// Events are dispatched to the optional dsl callbacks
    /// on_wsclient_message(id, message) / on_wsclient_state(id, state) through
    /// the host's Dispatch hook (see WebSocketClientManager).
    /// </summary>
    public static class WebSocketClientApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("wsclient_open",
                "wsclient_open(url[, id]) - open a websocket client connection, returns the client id ('' on error or id in use)",
                new ExpressionFactoryHelper<WsClientOpenExp>());
            BatchCommand.BatchScript.Register("wsclient_send",
                "wsclient_send(id, message) - send a text message over the wsclient connection, returns true when queued",
                new ExpressionFactoryHelper<WsClientSendExp>());
            BatchCommand.BatchScript.Register("wsclient_close",
                "wsclient_close(id) - close a wsclient connection (cooperative), returns bool",
                new ExpressionFactoryHelper<WsClientCloseExp>());
            BatchCommand.BatchScript.Register("wsclient_close_all",
                "wsclient_close_all() - close all wsclient connections, returns the closed count",
                new ExpressionFactoryHelper<WsClientCloseAllExp>());
            BatchCommand.BatchScript.Register("wsclient_count",
                "wsclient_count() - number of managed wsclient connections",
                new ExpressionFactoryHelper<WsClientCountExp>());
            BatchCommand.BatchScript.Register("wsclient_state",
                "wsclient_state(id) - wsclient connection state: connecting|connected|disconnected|failed|unknown",
                new ExpressionFactoryHelper<WsClientStateExp>());
            BatchCommand.BatchScript.Register("wsclient_list",
                "wsclient_list() - list of \"id|state|url\" strings",
                new ExpressionFactoryHelper<WsClientListExp>());
            BatchCommand.BatchScript.Register("handle_wsclient_queue",
                "handle_wsclient_queue([max_count_def_100]) - drain pending wsclient events on the main thread (auto-drained by the host loop too), returns the drained count",
                new ExpressionFactoryHelper<HandleWsClientQueueExp>());
        }
    }

    internal sealed class WsClientOpenExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: wsclient_open(url[, id])");
                return BoxedValue.EmptyString;
            }
            string url = operands[0].AsString;
            string? id = operands.Count > 1 ? operands[1].AsString : null;
            string result = WebSocketClientManager.Open(url, string.IsNullOrEmpty(id) ? null : id);
            return BoxedValue.FromString(result);
        }
    }

    internal sealed class WsClientSendExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                ApiErrorInfo.AppendLine("Expected: wsclient_send(id, message)");
                return BoxedValue.From(false);
            }
            string id = operands[0].AsString;
            string message = operands[1].AsString ?? string.Empty;
            return BoxedValue.From(WebSocketClientManager.Send(id, message));
        }
    }

    internal sealed class WsClientCloseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: wsclient_close(id)");
                return BoxedValue.From(false);
            }
            return BoxedValue.From(WebSocketClientManager.Close(operands[0].AsString));
        }
    }

    internal sealed class WsClientCloseAllExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.From(WebSocketClientManager.CloseAll());
        }
    }

    internal sealed class WsClientCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.From(WebSocketClientManager.Count());
        }
    }

    internal sealed class WsClientStateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: wsclient_state(id)");
                return BoxedValue.FromString("unknown");
            }
            return BoxedValue.FromString(WebSocketClientManager.GetState(operands[0].AsString));
        }
    }

    internal sealed class WsClientListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromObject(WebSocketClientManager.List());
        }
    }

    internal sealed class HandleWsClientQueueExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            int maxCount = operands.Count > 0 ? operands[0].GetInt() : 100;
            return BoxedValue.From(WebSocketClientManager.DrainQueue(maxCount));
        }
    }
}
