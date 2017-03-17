using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    public sealed class FileStreamWrapper : IFileStream
    {
        [NotNull]
        private readonly FileStream source;

        public Stream InnerStream => source;

        public bool CanRead => source.CanRead;

        public bool CanSeek => source.CanSeek;

        public bool CanWrite => source.CanWrite;

        public string Name => source.Name;

        public long Length => source.Length;

        public long Position
        {
            get
            {
                return source.Position;
            }
            set
            {
                source.Position = value;
            }
        }

        public bool IsAsync => source.IsAsync;

        public SafeFileHandle SafeFileHandle => source.SafeFileHandle;

        public FileStreamWrapper([NotNull] FileStream source)
        {
            Guard.NotNull(source, nameof(source));
            this.source = source;
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return source.Seek(offset, origin);
        }

        public void SetLength(long value)
        {
            source.SetLength(value);
        }

        public void Flush(bool flushToDisk = false)
        {
            source.Flush(flushToDisk);
        }

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            return source.FlushAsync(cancellationToken);
        }

        public int ReadByte()
        {
            return source.ReadByte();
        }

        public int Read(byte[] array, int offset, int count)
        {
            return source.Read(array, offset, count);
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return source.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public void WriteByte(byte value)
        {
            source.WriteByte(value);
        }

        public void Write(byte[] array, int offset, int count)
        {
            source.Write(array, offset, count);
        }

        public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return source.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public void Dispose()
        {
            source.Dispose();
        }
    }
}
