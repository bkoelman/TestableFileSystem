using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Exposes methods for creating, moving, and enumerating through directories and subdirectories.
    /// </summary>
    [PublicAPI]
    public interface IDirectory
    {
        /// <inheritdoc cref="Directory.GetParent" />
        [CanBeNull]
        IDirectoryInfo GetParent([NotNull] string path);

        /// <inheritdoc cref="Directory.GetDirectoryRoot" />
        [NotNull]
        string GetDirectoryRoot([NotNull] string path);

        /// <inheritdoc cref="Directory.GetFiles(string,string,SearchOption)" />
        [NotNull]
        [ItemNotNull]
        string[] GetFiles([NotNull] string path, [NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="Directory.EnumerateFiles(string,string,SearchOption)" />
        [NotNull]
        [ItemNotNull]
        IEnumerable<string> EnumerateFiles([NotNull] string path, [NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="Directory.GetDirectories(string,string,SearchOption)" />
        [NotNull]
        [ItemNotNull]
        string[] GetDirectories([NotNull] string path, [NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="Directory.EnumerateDirectories(string,string,SearchOption)" />
        [NotNull]
        [ItemNotNull]
        IEnumerable<string> EnumerateDirectories([NotNull] string path, [NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="Directory.GetFileSystemEntries(string,string,SearchOption)" />
        [NotNull]
        [ItemNotNull]
        string[] GetFileSystemEntries([NotNull] string path, [NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="Directory.EnumerateFileSystemEntries(string,string,SearchOption)" />
        [NotNull]
        [ItemNotNull]
        IEnumerable<string> EnumerateFileSystemEntries([NotNull] string path, [NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="Directory.Exists" />
        bool Exists([CanBeNull] string path);

        /// <inheritdoc cref="Directory.CreateDirectory(string)" />
        [NotNull]
        IDirectoryInfo CreateDirectory([NotNull] string path);

        /// <inheritdoc cref="Directory.Delete(string,bool)" />
        void Delete([NotNull] string path, bool recursive = false);

        /// <inheritdoc cref="Directory.Move" />
        void Move([NotNull] string sourceDirName, [NotNull] string destDirName);

        /// <inheritdoc cref="Directory.GetCurrentDirectory" />
        [NotNull]
        string GetCurrentDirectory();

        /// <inheritdoc cref="Directory.SetCurrentDirectory" />
        void SetCurrentDirectory([NotNull] string path);

        /// <inheritdoc cref="Directory.GetCreationTime" />
        DateTime GetCreationTime([NotNull] string path);

        /// <inheritdoc cref="Directory.GetCreationTimeUtc" />
        DateTime GetCreationTimeUtc([NotNull] string path);

        /// <inheritdoc cref="Directory.SetCreationTime" />
        void SetCreationTime([NotNull] string path, DateTime creationTime);

        /// <inheritdoc cref="Directory.SetCreationTimeUtc" />
        void SetCreationTimeUtc([NotNull] string path, DateTime creationTimeUtc);

        /// <inheritdoc cref="Directory.GetLastAccessTime" />
        DateTime GetLastAccessTime([NotNull] string path);

        /// <inheritdoc cref="Directory.GetLastAccessTimeUtc" />
        DateTime GetLastAccessTimeUtc([NotNull] string path);

        /// <inheritdoc cref="Directory.SetLastAccessTime" />
        void SetLastAccessTime([NotNull] string path, DateTime lastAccessTime);

        /// <inheritdoc cref="Directory.SetLastAccessTimeUtc" />
        void SetLastAccessTimeUtc([NotNull] string path, DateTime lastAccessTimeUtc);

        /// <inheritdoc cref="Directory.GetLastWriteTime" />
        DateTime GetLastWriteTime([NotNull] string path);

        /// <inheritdoc cref="Directory.GetLastWriteTimeUtc" />
        DateTime GetLastWriteTimeUtc([NotNull] string path);

        /// <inheritdoc cref="Directory.SetLastWriteTime" />
        void SetLastWriteTime([NotNull] string path, DateTime lastWriteTime);

        /// <inheritdoc cref="Directory.SetLastWriteTimeUtc" />
        void SetLastWriteTimeUtc([NotNull] string path, DateTime lastWriteTimeUtc);

#if !NETSTANDARD1_3
        /// <inheritdoc cref="Directory.GetLogicalDrives" />
        [NotNull]
        [ItemNotNull]
        string[] GetLogicalDrives();
#endif
    }
}
