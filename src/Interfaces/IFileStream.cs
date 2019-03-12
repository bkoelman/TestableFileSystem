using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Provides a <see cref="Stream" /> for a file, supporting both synchronous and asynchronous read and write operations.
    /// </summary>
    [PublicAPI]
    public interface IFileStream : IDisposable
    {
        /// <inheritdoc cref="FileStream.CanRead" />
        bool CanRead { get; }

        /// <inheritdoc cref="FileStream.CanSeek" />
        bool CanSeek { get; }

        /// <inheritdoc cref="FileStream.CanWrite" />
        bool CanWrite { get; }

        /// <inheritdoc cref="Stream.CanTimeout" />
        bool CanTimeout { get; }

        /// <inheritdoc cref="Stream.ReadTimeout" />
        int ReadTimeout { get; set; }

        /// <inheritdoc cref="Stream.WriteTimeout" />
        int WriteTimeout { get; set; }

        /// <inheritdoc cref="FileStream.Name" />
        [NotNull]
        string Name { get; }

        /// <inheritdoc cref="FileStream.Length" />
        long Length { get; }

        /// <inheritdoc cref="FileStream.Position" />
        long Position { get; set; }

        /// <inheritdoc cref="FileStream.IsAsync" />
        bool IsAsync { get; }

        // TODO: Add obsolete Handle (IntPtr) property.

        /// <inheritdoc cref="FileStream.SafeFileHandle" />
        [NotNull]
        SafeFileHandle SafeFileHandle { get; }

        /// <inheritdoc cref="FileStream.Seek" />
        long Seek(long offset, SeekOrigin origin);

        /// <inheritdoc cref="FileStream.SetLength" />
        void SetLength(long value);

        /// <inheritdoc cref="FileStream.Flush(bool)" />
        void Flush(bool flushToDisk = false);

        /// <inheritdoc cref="FileStream.FlushAsync(CancellationToken)" />
        [NotNull]
        Task FlushAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc cref="FileStream.ReadByte" />
        int ReadByte();

        /// <inheritdoc cref="FileStream.Read" />
        int Read([NotNull] byte[] array, int offset, int count);

        /// <inheritdoc cref="FileStream.ReadAsync(byte[],int,int,CancellationToken)" />
        [NotNull]
        Task<int> ReadAsync([NotNull] byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="FileStream.WriteByte" />
        void WriteByte(byte value);

        /// <inheritdoc cref="FileStream.Write" />
        void Write([NotNull] byte[] array, int offset, int count);

        /// <inheritdoc cref="FileStream.WriteAsync(byte[],int,int,CancellationToken)" />
        [NotNull]
        Task WriteAsync([NotNull] byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);

#if !NETSTANDARD1_3
        /// <inheritdoc cref="FileStream.BeginRead" />
        [NotNull]
        IAsyncResult BeginRead([NotNull] byte[] array, int offset, int numBytes, [CanBeNull] AsyncCallback userCallback,
            [CanBeNull] object stateObject);

        /// <inheritdoc cref="FileStream.EndRead" />
        int EndRead([NotNull] IAsyncResult asyncResult);

        /// <inheritdoc cref="FileStream.BeginWrite" />
        [NotNull]
        IAsyncResult BeginWrite([NotNull] byte[] array, int offset, int numBytes, [CanBeNull] AsyncCallback userCallback,
            [CanBeNull] object stateObject);

        /// <inheritdoc cref="FileStream.EndWrite" />
        void EndWrite([NotNull] IAsyncResult asyncResult);

        /// <inheritdoc cref="FileStream.Lock" />
        void Lock(long position, long length);

        /// <inheritdoc cref="FileStream.Unlock" />
        void Unlock(long position, long length);

        /// <inheritdoc cref="Stream.Close" />
        void Close();

#endif
        /// <inheritdoc cref="Stream.CopyTo(Stream,int)" />
        void CopyTo([NotNull] Stream destination, int bufferSize = 81920);

        /// <inheritdoc cref="Stream.CopyToAsync(Stream,int,CancellationToken)" />
        [NotNull]
        Task CopyToAsync([NotNull] Stream destination, int bufferSize = 81920, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the underlying <see cref="Stream" /> instance.
        /// </summary>
        /// <returns>
        /// The underlying <see cref="Stream" /> instance.
        /// </returns>
        [NotNull]
        Stream AsStream();
    }
}
