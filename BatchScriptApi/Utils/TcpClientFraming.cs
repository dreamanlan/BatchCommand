using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BatchCommand.Utils
{
    // The manager owns the stream and serializes all writes for each session.
    internal static class TcpClientFraming
    {
        // Null means EOF between frames. EOF inside a frame is a protocol error.
        internal static async Task<byte[]?> ReadAsync(
            Stream stream, int maxPayloadBytes, CancellationToken cancellationToken)
        {
            if (maxPayloadBytes <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maxPayloadBytes));
            }
            var header = new byte[4];
            if (!await ReadExactlyAsync(stream, header, true, cancellationToken).ConfigureAwait(false)) {
                return null;
            }
            uint length = BinaryPrimitives.ReadUInt32BigEndian(header);
            if (length > (uint)maxPayloadBytes) {
                throw new InvalidDataException("TCP frame exceeds the payload limit.");
            }
            if (length == 0) {
                return Array.Empty<byte>();
            }
            var payload = new byte[(int)length];
            await ReadExactlyAsync(stream, payload, false, cancellationToken).ConfigureAwait(false);
            return payload;
        }

        internal static async Task WriteAsync(
            Stream stream, byte[] payload, int maxPayloadBytes, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(payload);
            if (maxPayloadBytes <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maxPayloadBytes));
            }
            if (payload.Length > maxPayloadBytes) {
                throw new InvalidDataException("TCP frame exceeds the payload limit.");
            }
            var header = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(header, (uint)payload.Length);
            await stream.WriteAsync(header.AsMemory(), cancellationToken).ConfigureAwait(false);
            if (payload.Length > 0) {
                await stream.WriteAsync(payload.AsMemory(), cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<bool> ReadExactlyAsync(
            Stream stream, byte[] buffer, bool allowInitialEof, CancellationToken cancellationToken)
        {
            int offset = 0;
            while (offset < buffer.Length) {
                int count = await stream.ReadAsync(
                    buffer.AsMemory(offset), cancellationToken).ConfigureAwait(false);
                if (count == 0) {
                    if (allowInitialEof && offset == 0) {
                        return false;
                    }
                    throw new EndOfStreamException("TCP connection ended inside a length32_be frame.");
                }
                offset += count;
            }
            return true;
        }
    }
}
