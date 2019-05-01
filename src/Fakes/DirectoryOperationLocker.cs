using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class DirectoryOperationLocker<TDirectory> : IDirectory
        where TDirectory : class, IDirectory
    {
        [NotNull]
        private readonly FileSystemLock fileSystemLock;

        [NotNull]
        private readonly TDirectory target;

        public DirectoryOperationLocker([NotNull] FileSystemLock fileSystemLock, [NotNull] TDirectory target)
        {
            Guard.NotNull(fileSystemLock, nameof(fileSystemLock));
            Guard.NotNull(target, nameof(target));

            this.fileSystemLock = fileSystemLock;
            this.target = target;
        }

        public IDirectoryInfo GetParent(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetParent(path));
        }

        public string GetDirectoryRoot(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetDirectoryRoot(path));
        }

        public string[] GetFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetFiles(path, searchPattern, searchOption));
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return fileSystemLock.ExecuteInLock(() => target.EnumerateFiles(path, searchPattern, searchOption));
        }

        public string[] GetDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetDirectories(path, searchPattern, searchOption));
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return fileSystemLock.ExecuteInLock(() => target.EnumerateDirectories(path, searchPattern, searchOption));
        }

        public string[] GetFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetFileSystemEntries(path, searchPattern, searchOption));
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return fileSystemLock.ExecuteInLock(() => target.EnumerateFileSystemEntries(path, searchPattern, searchOption));
        }

        public bool Exists(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.Exists(path));
        }

        public IDirectoryInfo CreateDirectory(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.CreateDirectory(path));
        }

        public void Delete(string path, bool recursive = false)
        {
            fileSystemLock.ExecuteInLock(() => target.Delete(path, recursive));
        }

        public void Move(string sourceDirName, string destDirName)
        {
            fileSystemLock.ExecuteInLock(() => target.Move(sourceDirName, destDirName));
        }

        public string GetCurrentDirectory()
        {
            return fileSystemLock.ExecuteInLock(() => target.GetCurrentDirectory());
        }

        public void SetCurrentDirectory(string path)
        {
            fileSystemLock.ExecuteInLock(() => target.SetCurrentDirectory(path));
        }

        public DateTime GetCreationTime(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetCreationTime(path));
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetCreationTimeUtc(path));
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            fileSystemLock.ExecuteInLock(() => target.SetCreationTime(path, creationTime));
        }

        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            fileSystemLock.ExecuteInLock(() => target.SetCreationTimeUtc(path, creationTimeUtc));
        }

        public DateTime GetLastAccessTime(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetLastAccessTime(path));
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetLastAccessTimeUtc(path));
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            fileSystemLock.ExecuteInLock(() => target.SetLastAccessTime(path, lastAccessTime));
        }

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            fileSystemLock.ExecuteInLock(() => target.SetLastAccessTimeUtc(path, lastAccessTimeUtc));
        }

        public DateTime GetLastWriteTime(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetLastWriteTime(path));
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetLastWriteTimeUtc(path));
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            fileSystemLock.ExecuteInLock(() => target.SetLastWriteTime(path, lastWriteTime));
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            fileSystemLock.ExecuteInLock(() => target.SetLastWriteTimeUtc(path, lastWriteTimeUtc));
        }

#if !NETSTANDARD1_3
        public string[] GetLogicalDrives()
        {
            return fileSystemLock.ExecuteInLock(() => target.GetLogicalDrives());
        }
#endif
    }
}
