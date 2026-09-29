using System.Collections.Generic;
using BatchCommand.Utils;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace BatchCommand.Api
{
    public static class UdpClientApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("udpclient_open",
                "udpclient_open(local_address, local_port[, id]) - numeric IP only; port 0 selects an ephemeral port; returns id or empty string; binding is asynchronous",
                new ExpressionFactoryHelper<UdpClientOpenExp>());
            BatchCommand.BatchScript.Register("udpclient_send",
                "udpclient_send(id, bytes, remote_address, remote_port) - byte[] and numeric IP only; true means copied into the bounded queue, not delivered; no retry",
                new ExpressionFactoryHelper<UdpClientSendExp>());
            BatchCommand.BatchScript.Register("udpclient_close",
                "udpclient_close(id) - cancel and discard queued data; an admitted callback may still run",
                new ExpressionFactoryHelper<UdpClientCloseExp>());
            BatchCommand.BatchScript.Register("udpclient_drain_send",
                "udpclient_drain_send(id[, timeout_ms]) - reject new sends and drain queued and in-flight datagrams; default timeout 10000 ms; true means accepted, not completed; keep receiving on success; timeout closes the session",
                new ExpressionFactoryHelper<UdpClientDrainSendExp>());
            BatchCommand.BatchScript.Register("udpclient_drain_send_status",
                "udpclient_drain_send_status(id) - returns state/error/generation; state: not_requested, draining, succeeded, failed, cancelled, timed_out or unknown; succeeded means local sends completed, not delivery; results belong to the current ID generation and become unknown after removal",
                new ExpressionFactoryHelper<UdpClientDrainSendStatusExp>());
            BatchCommand.BatchScript.Register("udpclient_drain_wait",
                "udpclient_drain_wait(id[, wait_timeout_ms_def_1000]) - wait only for an already requested drain; nonnegative timeout, zero checks immediately; returns state/error/generation/wait_timed_out; pins the captured session; does not request draining, cancel network operations or dispatch callbacks; succeeded does not imply delivery",
                new ExpressionFactoryHelper<UdpClientDrainWaitExp>());
            BatchCommand.BatchScript.Register("udpclient_close_all",
                "udpclient_close_all() - cancel all clients without waiting for network tasks; returns closed count",
                new ExpressionFactoryHelper<UdpClientCloseAllExp>());
            BatchCommand.BatchScript.Register("udpclient_count",
                "udpclient_count() - returns managed client count",
                new ExpressionFactoryHelper<UdpClientCountExp>());
            BatchCommand.BatchScript.Register("udpclient_state",
                "udpclient_state(id) - returns binding, bound, closed, failed or unknown",
                new ExpressionFactoryHelper<UdpClientStateExp>());
            BatchCommand.BatchScript.Register("udpclient_local_endpoint",
                "udpclient_local_endpoint(id) - returns actual local IP:port, or [IPv6]:port; empty unless bound",
                new ExpressionFactoryHelper<UdpClientLocalEndPointExp>());
            BatchCommand.BatchScript.Register("udpclient_list",
                "udpclient_list() - returns managed client IDs",
                new ExpressionFactoryHelper<UdpClientListExp>());
            BatchCommand.BatchScript.Register("handle_udpclient_queue",
                "handle_udpclient_queue([max_count_def_100]) - dispatch events on the host thread; concurrent or reentrant drains return zero",
                new ExpressionFactoryHelper<HandleUdpClientQueueExp>());
        }
    }

    internal sealed class UdpClientOpenExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: udpclient_open(local_address, local_port[, id])");
                return BoxedValue.EmptyString;
            }
            string localAddress = operands[0].AsString;
            int localPort = operands[1].GetInt();
            string? id = operands.Count > 2 ? operands[2].AsString : null;
            return BoxedValue.FromString(UdpClientManager.Open(
                localAddress, localPort, string.IsNullOrEmpty(id) ? null : id));
        }
    }

    internal sealed class UdpClientSendExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 4 || !(operands[1].GetObject() is byte[] bytes)) {
                ApiErrorInfo.AppendLine("Expected: udpclient_send(id, bytes, remote_address, remote_port), where bytes is byte[]");
                return BoxedValue.From(false);
            }
            return BoxedValue.From(UdpClientManager.Send(
                operands[0].AsString, bytes, operands[2].AsString, operands[3].GetInt()));
        }
    }

    internal sealed class UdpClientCloseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: udpclient_close(id)");
                return BoxedValue.From(false);
            }
            return BoxedValue.From(UdpClientManager.Close(operands[0].AsString));
        }
    }

    internal sealed class UdpClientDrainSendExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: udpclient_drain_send(id[, timeout_ms])");
                return BoxedValue.From(false);
            }
            int timeoutMs = operands.Count > 1 ? operands[1].GetInt() : 10000;
            return BoxedValue.From(UdpClientManager.DrainSend(
                operands[0].AsString, timeoutMs));
        }
    }

    internal sealed class UdpClientDrainSendStatusExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: udpclient_drain_send_status(id)");
                return BoxedValue.NullObject;
            }
            return BoxedValue.FromObject(
                UdpClientManager.GetSendDrainStatus(operands[0].AsString));
        }
    }

    internal sealed class UdpClientDrainWaitExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: udpclient_drain_wait(id[, wait_timeout_ms])");
                return BoxedValue.NullObject;
            }
            int waitTimeoutMs = operands.Count > 1 ? operands[1].GetInt() : 1000;
            return BoxedValue.FromObject(
                UdpClientManager.WaitSendDrain(operands[0].AsString, waitTimeoutMs));
        }
    }

    internal sealed class UdpClientCloseAllExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                ApiErrorInfo.AppendLine("Expected: udpclient_close_all()");
                return BoxedValue.From(0);
            }
            return BoxedValue.From(UdpClientManager.CloseAll());
        }
    }

    internal sealed class UdpClientCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                ApiErrorInfo.AppendLine("Expected: udpclient_count()");
                return BoxedValue.From(0);
            }
            return BoxedValue.From(UdpClientManager.Count());
        }
    }

    internal sealed class UdpClientStateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: udpclient_state(id)");
                return BoxedValue.FromString("unknown");
            }
            return BoxedValue.FromString(UdpClientManager.GetState(operands[0].AsString));
        }
    }

    internal sealed class UdpClientLocalEndPointExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: udpclient_local_endpoint(id)");
                return BoxedValue.EmptyString;
            }
            return BoxedValue.FromString(UdpClientManager.GetLocalEndPoint(operands[0].AsString));
        }
    }

    internal sealed class UdpClientListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                ApiErrorInfo.AppendLine("Expected: udpclient_list()");
                return BoxedValue.NullObject;
            }
            return BoxedValue.FromObject(UdpClientManager.List());
        }
    }

    internal sealed class HandleUdpClientQueueExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count > 1) {
                ApiErrorInfo.AppendLine("Expected: handle_udpclient_queue([max_count])");
                return BoxedValue.From(0);
            }
            int maxCount = operands.Count > 0 ? operands[0].GetInt() : 100;
            return BoxedValue.From(UdpClientManager.DrainQueue(maxCount));
        }
    }
}
