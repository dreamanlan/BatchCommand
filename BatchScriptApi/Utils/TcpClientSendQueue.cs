using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BatchCommand.Utils
{
    // One queue per connection attempt; never reuse across reconnects.
    // Limits cover queued data, excluding the single in-flight send.
    internal sealed class TcpClientSendQueue
    {
        private readonly object m_Lock = new object();
        private readonly Queue<byte[]> m_Items = new Queue<byte[]>();
        private readonly SemaphoreSlim m_Ready = new SemaphoreSlim(0);
        private readonly int m_MaxMessages;
        private readonly int m_MaxPayloadBytes;
        private readonly long m_MaxQueuedBytes;
        private long m_QueuedBytes;
        private bool m_Draining;
        private bool m_Completed;

        internal TcpClientSendQueue(int maxMessages, int maxPayloadBytes, long maxQueuedBytes)
        {
            if (maxMessages <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maxMessages));
            }
            if (maxPayloadBytes <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maxPayloadBytes));
            }
            if (maxQueuedBytes <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maxQueuedBytes));
            }
            m_MaxMessages = maxMessages;
            m_MaxPayloadBytes = maxPayloadBytes;
            m_MaxQueuedBytes = maxQueuedBytes;
        }

        // True means accepted locally, not delivered to the peer.
        // The caller must not mutate payload concurrently with this call.
        internal bool TryEnqueue(byte[] payload)
        {
            ArgumentNullException.ThrowIfNull(payload);
            lock (m_Lock) {
                if (m_Completed || m_Draining || payload.Length > m_MaxPayloadBytes ||
                    m_Items.Count >= m_MaxMessages ||
                    payload.Length > m_MaxQueuedBytes - m_QueuedBytes) {
                    return false;
                }
                m_Items.Enqueue((byte[])payload.Clone());
                m_QueuedBytes += payload.Length;
                m_Ready.Release();
                return true;
            }
        }

        // Exactly one sender consumes this queue. Null means completed.
        internal async Task<byte[]?> DequeueAsync(CancellationToken cancellationToken)
        {
            await m_Ready.WaitAsync(cancellationToken).ConfigureAwait(false);
            lock (m_Lock) {
                if (m_Completed || (m_Draining && m_Items.Count == 0)) {
                    return null;
                }
                var payload = m_Items.Dequeue();
                m_QueuedBytes -= payload.Length;
                return payload;
            }
        }

        // Reject new payloads; the single sender consumes all accepted data.
        internal void Drain()
        {
            lock (m_Lock) {
                if (m_Completed || m_Draining) {
                    return;
                }
                m_Draining = true;
                // One extra wakeup marks the end after all queued payloads.
                m_Ready.Release();
            }
        }

        // Discard unsent data; the session also cancels in-flight socket I/O.
        internal void Complete()
        {
            lock (m_Lock) {
                if (m_Completed) {
                    return;
                }
                m_Completed = true;
                m_Items.Clear();
                m_QueuedBytes = 0;
                m_Ready.Release();
            }
        }
    }
}
