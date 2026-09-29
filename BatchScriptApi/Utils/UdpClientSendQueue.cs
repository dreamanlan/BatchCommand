using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BatchCommand.Utils
{
    // One queue per session, with exactly one sender.
    // Byte limits cover queued payloads, excluding one in-flight datagram.
    // Count limits bound endpoint and entry overhead, including empty payloads.
    internal sealed class UdpClientSendQueue
    {
        internal readonly record struct Datagram(byte[] Data, IPEndPoint RemoteEndPoint);

        private readonly object m_Lock = new object();
        private readonly Queue<Datagram> m_Items = new Queue<Datagram>();
        private readonly SemaphoreSlim m_Ready = new SemaphoreSlim(0);
        private readonly int m_MaxMessages;
        private readonly int m_MaxPayloadBytes;
        private readonly long m_MaxQueuedBytes;
        private long m_QueuedBytes;
        private bool m_Completed;
        private bool m_Draining;

        internal UdpClientSendQueue(int maxMessages, int maxPayloadBytes, long maxQueuedBytes)
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

        // True means accepted locally, not sent or delivered.
        // Callers must not mutate payload or endpoint concurrently with this call.
        // The session validates address-family and socket-specific constraints.
        internal bool TryEnqueue(byte[] payload, IPEndPoint remoteEndPoint)
        {
            ArgumentNullException.ThrowIfNull(payload);
            ArgumentNullException.ThrowIfNull(remoteEndPoint);
            if (remoteEndPoint.Port <= 0 || remoteEndPoint.Port > 65535) {
                throw new ArgumentOutOfRangeException(nameof(remoteEndPoint));
            }
            lock (m_Lock) {
                if (m_Completed || m_Draining || payload.Length > m_MaxPayloadBytes ||
                    m_Items.Count >= m_MaxMessages ||
                    payload.Length > m_MaxQueuedBytes - m_QueuedBytes) {
                    return false;
                }
                var address = remoteEndPoint.Address;
                var addressCopy = address.AddressFamily == AddressFamily.InterNetworkV6
                    ? new IPAddress(address.GetAddressBytes(), address.ScopeId)
                    : new IPAddress(address.GetAddressBytes());
                var endpointCopy = new IPEndPoint(addressCopy, remoteEndPoint.Port);
                var dataCopy = (byte[])payload.Clone();
                m_Items.Enqueue(new Datagram(dataCopy, endpointCopy));
                m_QueuedBytes += dataCopy.Length;
                m_Ready.Release();
                return true;
            }
        }

        // Null means completed or drained; empty payloads remain valid datagrams.
        // Ownership of the copied payload and endpoint transfers to the sender.
        internal async Task<Datagram?> DequeueAsync(CancellationToken cancellationToken)
        {
            await m_Ready.WaitAsync(cancellationToken).ConfigureAwait(false);
            lock (m_Lock) {
                if (m_Completed || (m_Draining && m_Items.Count == 0)) {
                    return null;
                }
                var datagram = m_Items.Dequeue();
                m_QueuedBytes -= datagram.Data.Length;
                return datagram;
            }
        }

        // Reject new datagrams while preserving accepted items for the sender.
        internal void Drain()
        {
            lock (m_Lock) {
                if (m_Completed || m_Draining) {
                    return;
                }
                m_Draining = true;
                m_Ready.Release();
            }
        }

        // Discard unsent datagrams. The session cancels in-flight socket I/O.
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
