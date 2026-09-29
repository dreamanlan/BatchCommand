using System.Collections.Generic;
using BatchCommand.Utils;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace BatchCommand.Api
{
    public static class TcpClientApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("tcpclient_open",
                "tcpclient_open(host, port[, id[, frame_mode[, reconnect_count]]]) - returns id or empty string; frame_mode: raw or length32_be; reconnect never resends queued data",
                new ExpressionFactoryHelper<TcpClientOpenExp>());
            BatchCommand.BatchScript.Register("tcpclient_send",
                "tcpclient_send(id, bytes) - byte[] only; true means copied into the bounded queue, not delivered",
                new ExpressionFactoryHelper<TcpClientSendExp>());
            BatchCommand.BatchScript.Register("tcpclient_shutdown_send",
                "tcpclient_shutdown_send(id[, timeout_ms_def_10000]) - positive timeout; connected only; true means accepted, not completed; rejects further sends and disables reconnect; drains writes then shuts down send while keeping receive open; timeout closes the session; repeated requests do not reset the deadline; poll tcpclient_shutdown_send_status before closing",
                new ExpressionFactoryHelper<TcpClientShutdownSendExp>());
            BatchCommand.BatchScript.Register("tcpclient_shutdown_send_status",
                "tcpclient_shutdown_send_status(id) - returns object with state, error, generation; states: not_requested, draining, succeeded, failed, cancelled, timed_out, unknown; succeeded means local writes and send shutdown completed, not peer processing; results survive session end but not client removal or ID replacement",
                new ExpressionFactoryHelper<TcpClientShutdownSendStatusExp>());
            BatchCommand.BatchScript.Register("tcpclient_drain_wait",
                "tcpclient_drain_wait(id[, wait_timeout_ms_def_1000]) - wait only for an already requested drain; nonnegative timeout, zero checks immediately; returns state/error/generation/wait_timed_out; pins the captured session; does not request draining, cancel network operations or dispatch callbacks; succeeded does not imply peer processing",
                new ExpressionFactoryHelper<TcpClientDrainWaitExp>());
            BatchCommand.BatchScript.Register("tcpclient_close",
                "tcpclient_close(id) - cancel and discard queued data; an admitted callback may still run",
                new ExpressionFactoryHelper<TcpClientCloseExp>());
            BatchCommand.BatchScript.Register("tcpclient_close_all",
                "tcpclient_close_all() - cancel all clients without waiting for network tasks; returns closed count",
                new ExpressionFactoryHelper<TcpClientCloseAllExp>());
            BatchCommand.BatchScript.Register("tcpclient_count",
                "tcpclient_count() - returns managed client count",
                new ExpressionFactoryHelper<TcpClientCountExp>());
            BatchCommand.BatchScript.Register("tcpclient_state",
                "tcpclient_state(id) - returns connection state or unknown",
                new ExpressionFactoryHelper<TcpClientStateExp>());
            BatchCommand.BatchScript.Register("tcpclient_list",
                "tcpclient_list() - returns managed client IDs",
                new ExpressionFactoryHelper<TcpClientListExp>());
            BatchCommand.BatchScript.Register("handle_tcpclient_queue",
                "handle_tcpclient_queue([max_count_def_100]) - dispatch events on the host thread; concurrent or reentrant drains return zero",
                new ExpressionFactoryHelper<HandleTcpClientQueueExp>());
        }
    }

    internal sealed class TcpClientOpenExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 5) {
                ApiErrorInfo.AppendLine("Expected: tcpclient_open(host, port[, id[, frame_mode[, reconnect_count]]])");
                return BoxedValue.EmptyString;
            }
            string host = operands[0].AsString;
            int port = operands[1].GetInt();
            string? id = operands.Count > 2 ? operands[2].AsString : null;
            string frameMode = operands.Count > 3 ? operands[3].AsString : "raw";
            int reconnectCount = operands.Count > 4 ? operands[4].GetInt() : 0;
            return BoxedValue.FromString(TcpClientManager.Open(
                host, port, string.IsNullOrEmpty(id) ? null : id, frameMode, reconnectCount));
        }
    }

    internal sealed class TcpClientSendExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2 || !(operands[1].GetObject() is byte[] bytes)) {
                ApiErrorInfo.AppendLine("Expected: tcpclient_send(id, bytes), where bytes is byte[]");
                return BoxedValue.From(false);
            }
            return BoxedValue.From(TcpClientManager.Send(operands[0].AsString, bytes));
        }
    }

    internal sealed class TcpClientShutdownSendExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: tcpclient_shutdown_send(id[, timeout_ms])");
                return BoxedValue.From(false);
            }
            int timeoutMs = operands.Count > 1 ? operands[1].GetInt() : 10000;
            return BoxedValue.From(TcpClientManager.ShutdownSend(
                operands[0].AsString, timeoutMs));
        }
    }

    internal sealed class TcpClientShutdownSendStatusExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: tcpclient_shutdown_send_status(id)");
                return BoxedValue.NullObject;
            }
            return BoxedValue.FromObject(
                TcpClientManager.GetSendShutdownStatus(operands[0].AsString));
        }
    }

    internal sealed class TcpClientDrainWaitExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: tcpclient_drain_wait(id[, wait_timeout_ms])");
                return BoxedValue.NullObject;
            }
            int waitTimeoutMs = operands.Count > 1 ? operands[1].GetInt() : 1000;
            return BoxedValue.FromObject(
                TcpClientManager.WaitSendShutdown(operands[0].AsString, waitTimeoutMs));
        }
    }

    internal sealed class TcpClientCloseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: tcpclient_close(id)");
                return BoxedValue.From(false);
            }
            return BoxedValue.From(TcpClientManager.Close(operands[0].AsString));
        }
    }

    internal sealed class TcpClientCloseAllExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                ApiErrorInfo.AppendLine("Expected: tcpclient_close_all()");
                return BoxedValue.From(0);
            }
            return BoxedValue.From(TcpClientManager.CloseAll());
        }
    }

    internal sealed class TcpClientCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                ApiErrorInfo.AppendLine("Expected: tcpclient_count()");
                return BoxedValue.From(0);
            }
            return BoxedValue.From(TcpClientManager.Count());
        }
    }

    internal sealed class TcpClientStateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: tcpclient_state(id)");
                return BoxedValue.FromString("unknown");
            }
            return BoxedValue.FromString(TcpClientManager.GetState(operands[0].AsString));
        }
    }

    internal sealed class TcpClientListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                ApiErrorInfo.AppendLine("Expected: tcpclient_list()");
                return BoxedValue.NullObject;
            }
            return BoxedValue.FromObject(TcpClientManager.List());
        }
    }

    internal sealed class HandleTcpClientQueueExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 1) {
                ApiErrorInfo.AppendLine("Expected: handle_tcpclient_queue([max_count])");
                return BoxedValue.From(0);
            }
            int maxCount = operands.Count > 0 ? operands[0].GetInt() : 100;
            return BoxedValue.From(TcpClientManager.DrainQueue(maxCount));
        }
    }
}
