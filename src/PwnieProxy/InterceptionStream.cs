using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PwnieProxy.Handlers;

namespace PwnieProxy
{
    public class InterceptionStream : Stream
    {
        private Stream InnerStream { get; }
        private Stream? CopyStream { get; }
        private Queue<byte[]> queue = new Queue<byte[]>();
        private readonly List<IHandler> handlers = new List<IHandler>();

        public void AddHandler(IHandler handler) => handlers.Add(handler);

        public InterceptionStream(Stream innerStream, Stream? copyStream = null)
        {
            InnerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
            CopyStream = copyStream;
        }

        public override void Flush()
        {
            InnerStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            InnerStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = InnerStream.Read(buffer, offset, count);
            if (CopyStream == null || bytesRead == 0)
            {
                return bytesRead;
            }

            CopyStream.Write(buffer, offset, bytesRead);
            return bytesRead;
        }

        public void QueueMessage(byte[] message)
        {
            queue.Enqueue(message);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] packet = buffer[offset..count];
            foreach (var handler in handlers)
            {
                packet = handler.Handle(packet);
            }
            if (queue.TryDequeue(out byte[]? result) && result != null)
            {
                packet = packet.Concat(result).ToArray();
            }
            InnerStream.Write(packet, 0, packet.Length);
            CopyStream?.Write(packet, 0, packet.Length);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return InnerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override bool CanRead => InnerStream.CanRead;

        public override bool CanSeek => InnerStream.CanSeek;

        public override bool CanWrite => InnerStream.CanWrite;

        public override long Length => InnerStream.Length;

        public override long Position
        {
            get => InnerStream.Position;
            set => InnerStream.Position = value;
        }

        protected override void Dispose(bool disposing)
        {
            InnerStream.Dispose();
        }
    }
}