using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Exposes methods for creating, moving, and enumerating through directories and subdirectories.
    /// </summary>
    [PublicAPI]
    public interface IDirectoryInfo : IFileSystemInfo
    {
        /// <inheritdoc cref="DirectoryInfo.Parent" />
        [CanBeNull]
        IDirectoryInfo Parent { get; }

        /// <inheritdoc cref="DirectoryInfo.Root" />
        [NotNull]
        IDirectoryInfo Root { get; }

        /// <inheritdoc cref="DirectoryInfo.GetFiles(string,SearchOption)" />
        [NotNull]
        [ItemNotNull]
        IFileInfo[] GetFiles([NotNull] string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="DirectoryInfo.EnumerateFiles(string,SearchOption)" />
        [NotNull]
        [ItemNotNull]
        IEnumerable<IFileInfo> EnumerateFiles([NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="DirectoryInfo.GetDirectories(string,SearchOption)" />
        [NotNull]
        [ItemNotNull]
        IDirectoryInfo[] GetDirectories([NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="DirectoryInfo.EnumerateDirectories(string,SearchOption)" />
        [NotNull]
        [ItemNotNull]
        IEnumerable<IDirectoryInfo> EnumerateDirectories([NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="DirectoryInfo.GetFileSystemInfos(string,SearchOption)" />
        [NotNull]
        [ItemNotNull]
        IFileSystemInfo[] GetFileSystemInfos([NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="DirectoryInfo.EnumerateFileSystemInfos(string,SearchOption)" />
        [NotNull]
        [ItemNotNull]
        IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos([NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="DirectoryInfo.Create()" />
        void Create();

        /// <inheritdoc cref="DirectoryInfo.CreateSubdirectory(string)" />
        [NotNull]
        IDirectoryInfo CreateSubdirectory([NotNull] string path);

        /// <inheritdoc cref="DirectoryInfo.Delete(bool)" />
        void Delete(bool recursive);

        /// <inheritdoc cref="DirectoryInfo.MoveTo" />
        void MoveTo([NotNull] string destDirName);

        /// <inheritdoc cref="DirectoryInfo.ToString" />
        [NotNull]
        string ToString();
    }
}
