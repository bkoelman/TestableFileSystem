using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace TestableFileSystem.Interfaces
{
    public interface IFileStream : IDisposable
    {
        [NotNull]
        Stream InnerStream { get; }

        bool CanRead { get; }
        bool CanSeek { get; }
        bool CanWrite { get; }

        [CanBeNull]
        string Name { get; }

        long Length { get; }
        long Position { get; set; }

        bool IsAsync { get; }

        [CanBeNull]
        SafeFileHandle SafeFileHandle { get; }

        long Seek(long offset, SeekOrigin origin);

        void SetLength(long value);

        void Flush(bool flushToDisk);

        [NotNull]
        Task FlushAsync(CancellationToken cancellationToken);

        int ReadByte();
        int Read([NotNull] byte[] array, int offset, int count);

        [NotNull]
        Task<int> ReadAsync([NotNull] byte[] buffer, int offset, int count, CancellationToken cancellationToken);

        void WriteByte(byte value);
        void Write([NotNull] byte[] array, int offset, int count);

        [NotNull]
        Task WriteAsync([NotNull] byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    }
}
