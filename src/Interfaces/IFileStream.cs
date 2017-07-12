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
        bool CanTimeout { get; }

        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }

        [NotNull]
        string Name { get; }

        long Length { get; }
        long Position { get; set; }

        bool IsAsync { get; }

        [CanBeNull]
        SafeFileHandle SafeFileHandle { get; }

        long Seek(long offset, SeekOrigin origin);

        void SetLength(long value);

        void Flush(bool flushToDisk = false);

        [NotNull]
        Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken));

        int ReadByte();
        int Read([NotNull] byte[] array, int offset, int count);

        [NotNull]
        Task<int> ReadAsync([NotNull] byte[] buffer, int offset, int count,
            CancellationToken cancellationToken = default(CancellationToken));

        void WriteByte(byte value);
        void Write([NotNull] byte[] array, int offset, int count);

        [NotNull]
        Task WriteAsync([NotNull] byte[] buffer, int offset, int count,
            CancellationToken cancellationToken = default(CancellationToken));

        void CopyTo([NotNull] Stream destination, int bufferSize = 81920);

        [NotNull]
        Task CopyToAsync([NotNull] Stream destination, int bufferSize = 81920,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
