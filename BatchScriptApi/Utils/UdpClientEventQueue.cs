using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BatchCommand.Utils
{
    internal sealed class UdpClientEventQueue
    {
        // Internal carrier; the host-facing callback contract is separate.
        internal readonly record struct Datagram(
            byte[] Data, string RemoteAddress, int RemotePort);

        internal readonly record struct Entry(
            long Generation, string Kind, object Payload, long PayloadBytes);

        private readonly object m_Lock = new object();
        private readonly Queue<Entry> m_Items = new Queue<Entry>();
        private readonly int m_MaxMessages;
        private readonly long m_MaxQueuedBytes;
        private long m_QueuedBytes;
        private bool m_Completed;
        private TaskCompletionSource<bool> m_Changed = CreateSignal();

        internal UdpClientEventQueue(int maxMessages, long maxQueuedBytes)
        {
            if (maxMessages <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maxMessages));
            }
            if (maxQueuedBytes <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maxQueuedBytes));
            }
            m_MaxMessages = maxMessages;
            m_MaxQueuedBytes = maxQueuedBytes;
        }

        private static TaskCompletionSource<bool> CreateSignal()
        {
            return new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private void PulseLocked()
        {
            var previous = m_Changed;
            m_Changed = CreateSignal();
            previous.TrySetResult(true);
        }

        // Array ownership transfers to the queue. Internal producers must not
        // mutate or reuse the array after calling this method.
        // Byte accounting covers payload fields, not CLR object overhead.
        // Count limits also bound empty datagrams and per-entry overhead.
        internal async Task<bool> EnqueueAsync(
            long generation, string kind, object payload, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(payload);
            long bytes;
            if (kind == "message" && payload is Datagram datagram) {
                if (datagram.Data == null ||
                    string.IsNullOrWhiteSpace(datagram.RemoteAddress) ||
                    datagram.RemotePort < 0 || datagram.RemotePort > 65535) {
                    throw new ArgumentException("Invalid UDP datagram.");
                }
                bytes = datagram.Data.LongLength +
                    (long)datagram.RemoteAddress.Length * sizeof(char) + sizeof(int);
            }
            else if ((kind == "state" || kind == "error") && payload is string text) {
                bytes = (long)text.Length * sizeof(char);
            }
            else {
                throw new ArgumentException("Invalid UDP event kind or payload.");
            }
            if (bytes > m_MaxQueuedBytes) {
                throw new ArgumentOutOfRangeException(
                    nameof(payload), "UDP event exceeds the queue byte limit.");
            }

            while (true) {
                token.ThrowIfCancellationRequested();
                Task changed;
                lock (m_Lock) {
                    token.ThrowIfCancellationRequested();
                    if (m_Completed) {
                        return false;
                    }
                    if (m_Items.Count < m_MaxMessages &&
                        bytes <= m_MaxQueuedBytes - m_QueuedBytes) {
                        m_Items.Enqueue(new Entry(generation, kind, payload, bytes));
                        m_QueuedBytes += bytes;
                        return true;
                    }
                    changed = m_Changed.Task;
                }
                // Local backpressure cannot prevent UDP packet loss in the OS.
                await changed.WaitAsync(token).ConfigureAwait(false);
            }
        }

        // The manager validates generations and dispatches outside this lock.
        internal bool TryDequeue(out Entry entry)
        {
            lock (m_Lock) {
                if (m_Items.Count == 0) {
                    entry = default;
                    return false;
                }
                entry = m_Items.Dequeue();
                m_QueuedBytes -= entry.PayloadBytes;
                PulseLocked();
                return true;
            }
        }

        internal void Complete()
        {
            lock (m_Lock) {
                if (m_Completed) {
                    return;
                }
                m_Completed = true;
                m_Items.Clear();
                m_QueuedBytes = 0;
                PulseLocked();
            }
        }
    }
}
