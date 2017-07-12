using System;
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
        private readonly Func<string> getName;

        [NotNull]
        private readonly Func<bool> getIsAsync;

        [NotNull]
        private readonly Func<SafeFileHandle> getSafeFileHandle;

        [NotNull]
        private readonly Action<bool> doFlush;

        public Stream InnerStream { get; }

        public bool CanRead => InnerStream.CanRead;

        public bool CanSeek => InnerStream.CanSeek;

        public bool CanWrite => InnerStream.CanWrite;

        public string Name => getName();

        public long Length => InnerStream.Length;

        public long Position
        {
            get => InnerStream.Position;
            set => InnerStream.Position = value;
        }

        public bool IsAsync => getIsAsync();

        public SafeFileHandle SafeFileHandle => getSafeFileHandle();

        public FileStreamWrapper([NotNull] FileStream source)
            : this(source, () => source.Name, () => source.IsAsync, () => source.SafeFileHandle, source.Flush)
        {
        }

        public FileStreamWrapper([NotNull] Stream source, [NotNull] Func<string> getName, [NotNull] Func<bool> getIsAsync,
            [NotNull] Func<SafeFileHandle> getSafeFileHandle, [NotNull] Action<bool> doFlush)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(getName, nameof(getName));
            Guard.NotNull(getIsAsync, nameof(getIsAsync));
            Guard.NotNull(getSafeFileHandle, nameof(getSafeFileHandle));
            Guard.NotNull(doFlush, nameof(doFlush));

            InnerStream = source;
            this.getName = getName;
            this.getIsAsync = getIsAsync;
            this.getSafeFileHandle = getSafeFileHandle;
            this.doFlush = doFlush;
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
            doFlush(flushToDisk);
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
