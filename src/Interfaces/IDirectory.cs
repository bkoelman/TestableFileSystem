using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    [PublicAPI]
    public interface IDirectory
    {
        [CanBeNull]
        IDirectoryInfo GetParent([NotNull] string path);

        [NotNull]
        string GetDirectoryRoot([NotNull] string path);

        [NotNull]
        [ItemNotNull]
        string[] GetFiles([NotNull] string path, [NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        [NotNull]
        [ItemNotNull]
        IEnumerable<string> EnumerateFiles([NotNull] string path, [NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        [NotNull]
        [ItemNotNull]
        string[] GetDirectories([NotNull] string path, [NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        [NotNull]
        [ItemNotNull]
        IEnumerable<string> EnumerateDirectories([NotNull] string path, [NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        [NotNull]
        [ItemNotNull]
        string[] GetFileSystemEntries([NotNull] string path, [NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        [NotNull]
        [ItemNotNull]
        IEnumerable<string> EnumerateFileSystemEntries([NotNull] string path, [NotNull] string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        bool Exists([CanBeNull] string path);

        [NotNull]
        IDirectoryInfo CreateDirectory([NotNull] string path);

        void Delete([NotNull] string path, bool recursive = false);

        void Move([NotNull] string sourceDirName, [NotNull] string destDirName);

        [NotNull]
        string GetCurrentDirectory();

        void SetCurrentDirectory([NotNull] string path);

        DateTime GetCreationTime([NotNull] string path);
        DateTime GetCreationTimeUtc([NotNull] string path);
        void SetCreationTime([NotNull] string path, DateTime creationTime);
        void SetCreationTimeUtc([NotNull] string path, DateTime creationTimeUtc);

        DateTime GetLastAccessTime([NotNull] string path);
        DateTime GetLastAccessTimeUtc([NotNull] string path);
        void SetLastAccessTime([NotNull] string path, DateTime lastAccessTime);
        void SetLastAccessTimeUtc([NotNull] string path, DateTime lastAccessTimeUtc);

        DateTime GetLastWriteTime([NotNull] string path);
        DateTime GetLastWriteTimeUtc([NotNull] string path);
        void SetLastWriteTime([NotNull] string path, DateTime lastWriteTime);
        void SetLastWriteTimeUtc([NotNull] string path, DateTime lastWriteTimeUtc);

#if !NETSTANDARD1_3
        [NotNull]
        [ItemNotNull]
        string[] GetLogicalDrives();
#endif
    }
}
