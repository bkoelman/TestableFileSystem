using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class StreamWrapper : IFileStream
    {
        public Stream InnerStream { get; }

        public bool CanRead => InnerStream.CanRead;

        public bool CanSeek => InnerStream.CanSeek;

        public bool CanWrite => InnerStream.CanWrite;

        public string Name { get; }

        public long Length => InnerStream.Length;

        public long Position
        {
            get
            {
                return InnerStream.Position;
            }
            set
            {
                InnerStream.Position = value;
            }
        }

        public bool IsAsync => false;

        public SafeFileHandle SafeFileHandle
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public StreamWrapper([NotNull] Stream innerStream, [NotNull] string name)
        {
            Guard.NotNull(innerStream, nameof(innerStream));
            Guard.NotNull(name, nameof(name));

            InnerStream = innerStream;
            Name = name;
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return InnerStream.Seek(offset, origin);
        }

        public void SetLength(long value)
        {
            InnerStream.SetLength(value);
        }

        public void Flush(bool flushToDisk = false)
        {
            InnerStream.Flush();
        }

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            return InnerStream.FlushAsync(cancellationToken);
        }

        public int ReadByte()
        {
            return InnerStream.ReadByte();
        }

        public int Read(byte[] array, int offset, int count)
        {
            return InnerStream.Read(array, offset, count);
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return InnerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public void WriteByte(byte value)
        {
            InnerStream.WriteByte(value);
        }

        public void Write(byte[] array, int offset, int count)
        {
            InnerStream.Write(array, offset, count);
        }

        public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return InnerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public void Dispose()
        {
            InnerStream.Dispose();
        }
    }
}
