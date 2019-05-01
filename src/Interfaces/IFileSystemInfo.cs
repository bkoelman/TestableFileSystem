using System;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Provides the base interface for both <see cref="IFileInfo" /> and <see cref="IDirectoryInfo" /> objects.
    /// </summary>
    [PublicAPI]
    public interface IFileSystemInfo
    {
        /// <inheritdoc cref="FileSystemInfo.Name" />
        [NotNull]
        string Name { get; }

        /// <inheritdoc cref="FileSystemInfo.Extension" />
        [NotNull]
        string Extension { get; }

        /// <inheritdoc cref="FileSystemInfo.FullName" />
        [NotNull]
        string FullName { get; }

        /// <inheritdoc cref="FileSystemInfo.Attributes" />
        FileAttributes Attributes { get; set; }

        /// <inheritdoc cref="FileSystemInfo.CreationTime" />
        DateTime CreationTime { get; set; }

        /// <inheritdoc cref="FileSystemInfo.CreationTimeUtc" />
        DateTime CreationTimeUtc { get; set; }

        /// <inheritdoc cref="FileSystemInfo.LastAccessTime" />
        DateTime LastAccessTime { get; set; }

        /// <inheritdoc cref="FileSystemInfo.LastAccessTimeUtc" />
        DateTime LastAccessTimeUtc { get; set; }

        /// <inheritdoc cref="FileSystemInfo.LastWriteTime" />
        DateTime LastWriteTime { get; set; }

        /// <inheritdoc cref="FileSystemInfo.LastWriteTimeUtc" />
        DateTime LastWriteTimeUtc { get; set; }

        /// <inheritdoc cref="FileSystemInfo.Exists" />
        bool Exists { get; }

        /// <inheritdoc cref="FileSystemInfo.Refresh" />
        void Refresh();

        /// <inheritdoc cref="FileSystemInfo.Delete" />
        void Delete();
    }
}
