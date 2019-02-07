using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class DirectoryOperationLocker<TDirectory> : OperationLocker, IDirectory
        where TDirectory : class, IDirectory
    {
        [NotNull]
        private readonly TDirectory target;

        public DirectoryOperationLocker([NotNull] FakeFileSystem owner, [NotNull] TDirectory target)
            : base(owner)
        {
            Guard.NotNull(target, nameof(target));
            this.target = target;
        }

        public IDirectoryInfo GetParent(string path)
        {
            return ExecuteInLock(() => target.GetParent(path));
        }

        public string GetDirectoryRoot(string path)
        {
            return ExecuteInLock(() => target.GetDirectoryRoot(path));
        }

        public string[] GetFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return ExecuteInLock(() => target.GetFiles(path, searchPattern, searchOption));
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return ExecuteInLock(() => target.EnumerateFiles(path, searchPattern, searchOption));
        }

        public string[] GetDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return ExecuteInLock(() => target.GetDirectories(path, searchPattern, searchOption));
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return ExecuteInLock(() => target.EnumerateDirectories(path, searchPattern, searchOption));
        }

        public string[] GetFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return ExecuteInLock(() => target.GetFileSystemEntries(path, searchPattern, searchOption));
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return ExecuteInLock(() => target.EnumerateFileSystemEntries(path, searchPattern, searchOption));
        }

        public bool Exists(string path)
        {
            return ExecuteInLock(() => target.Exists(path));
        }

        public IDirectoryInfo CreateDirectory(string path)
        {
            return ExecuteInLock(() => target.CreateDirectory(path));
        }

        public void Delete(string path, bool recursive = false)
        {
            ExecuteInLock(() => target.Delete(path, recursive));
        }

        public void Move(string sourceDirName, string destDirName)
        {
            ExecuteInLock(() => target.Move(sourceDirName, destDirName));
        }

        public string GetCurrentDirectory()
        {
            return ExecuteInLock(() => target.GetCurrentDirectory());
        }

        public void SetCurrentDirectory(string path)
        {
            ExecuteInLock(() => target.SetCurrentDirectory(path));
        }

        public DateTime GetCreationTime(string path)
        {
            return ExecuteInLock(() => target.GetCreationTime(path));
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            return ExecuteInLock(() => target.GetCreationTimeUtc(path));
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            ExecuteInLock(() => target.SetCreationTime(path, creationTime));
        }

        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            ExecuteInLock(() => target.SetCreationTimeUtc(path, creationTimeUtc));
        }

        public DateTime GetLastAccessTime(string path)
        {
            return ExecuteInLock(() => target.GetLastAccessTime(path));
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            return ExecuteInLock(() => target.GetLastAccessTimeUtc(path));
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            ExecuteInLock(() => target.SetLastAccessTime(path, lastAccessTime));
        }

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            ExecuteInLock(() => target.SetLastAccessTimeUtc(path, lastAccessTimeUtc));
        }

        public DateTime GetLastWriteTime(string path)
        {
            return ExecuteInLock(() => target.GetLastWriteTime(path));
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            return ExecuteInLock(() => target.GetLastWriteTimeUtc(path));
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            ExecuteInLock(() => target.SetLastWriteTime(path, lastWriteTime));
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            ExecuteInLock(() => target.SetLastWriteTimeUtc(path, lastWriteTimeUtc));
        }

#if !NETSTANDARD1_3
        public string[] GetLogicalDrives()
        {
            return ExecuteInLock(() => target.GetLogicalDrives());
        }
#endif
    }
}
