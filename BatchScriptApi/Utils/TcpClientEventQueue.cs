using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BatchCommand.Utils
{
    // The manager validates generations before dispatching on the host thread.
    internal sealed class TcpClientEventQueue
    {
        internal readonly record struct Entry(
            long Generation, string Kind, object Payload, long PayloadBytes);

        private readonly object m_Lock = new object();
        private readonly Queue<Entry> m_Items = new Queue<Entry>();
        private readonly int m_MaxMessages;
        private readonly long m_MaxQueuedBytes;
        private long m_QueuedBytes;
        private bool m_Completed;
        private TaskCompletionSource<bool> m_Changed = CreateSignal();

        internal TcpClientEventQueue(int maxMessages, long maxQueuedBytes)
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

        // Internal producers only. Array ownership transfers to the queue;
        // producers must not mutate or reuse it after calling this method.
        // False means closed; cancellation throws. No user callbacks run here.
        internal async Task<bool> EnqueueAsync(
            long generation, string kind, object payload, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(payload);
            long bytes;
            if (kind == "message" && payload is byte[] data) {
                bytes = data.LongLength;
            }
            else if ((kind == "state" || kind == "error") && payload is string text) {
                bytes = (long)text.Length * sizeof(char);
            }
            else {
                throw new ArgumentException("Invalid TCP event kind or payload.");
            }
            if (bytes > m_MaxQueuedBytes) {
                throw new ArgumentOutOfRangeException(
                    nameof(payload), "TCP event exceeds the queue byte limit.");
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
                // Capture under the lock to avoid a lost capacity notification.
                await changed.WaitAsync(token).ConfigureAwait(false);
            }
        }

        // Dispatch must happen outside this queue's lock.
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

        // Retiring a logical client discards pending events and wakes producers.
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
