using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class DirectoryOperationLocker : IDirectory
    {
        [NotNull]
        private readonly MemoryFileSystem owner;

        [NotNull]
        private readonly IDirectory target;

        public DirectoryOperationLocker([NotNull] MemoryFileSystem owner, [NotNull] IDirectory target)
        {
            Guard.NotNull(owner, nameof(owner));
            Guard.NotNull(target, nameof(target));

            this.owner = owner;
            this.target = target;
        }

        public IDirectoryInfo GetParent(string path)
        {
            lock (owner.TreeLock)
            {
                return target.GetParent(path);
            }
        }

        public string GetDirectoryRoot(string path)
        {
            lock (owner.TreeLock)
            {
                return target.GetDirectoryRoot(path);
            }
        }

        public string[] GetFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            lock (owner.TreeLock)
            {
                return target.GetFiles(path, searchPattern, searchOption);
            }
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            lock (owner.TreeLock)
            {
                return target.EnumerateFiles(path, searchPattern, searchOption);
            }
        }

        public string[] GetDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            lock (owner.TreeLock)
            {
                return target.GetDirectories(path, searchPattern, searchOption);
            }
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            lock (owner.TreeLock)
            {
                return target.EnumerateDirectories(path, searchPattern, searchOption);
            }
        }

        public string[] GetFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            lock (owner.TreeLock)
            {
                return target.GetFileSystemEntries(path, searchPattern, searchOption);
            }
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            lock (owner.TreeLock)
            {
                return target.EnumerateFileSystemEntries(path, searchPattern, searchOption);
            }
        }

        public bool Exists(string path)
        {
            lock (owner.TreeLock)
            {
                return target.Exists(path);
            }
        }

        public IDirectoryInfo CreateDirectory(string path)
        {
            lock (owner.TreeLock)
            {
                return target.CreateDirectory(path);
            }
        }

        public void Delete(string path, bool recursive = false)
        {
            lock (owner.TreeLock)
            {
                target.Delete(path, recursive);
            }
        }

        public void Move(string sourceDirName, string destDirName)
        {
            lock (owner.TreeLock)
            {
                target.Move(sourceDirName, destDirName);
            }
        }

        public string GetCurrentDirectory()
        {
            lock (owner.TreeLock)
            {
                return target.GetCurrentDirectory();
            }
        }

        public void SetCurrentDirectory(string path)
        {
            lock (owner.TreeLock)
            {
                target.SetCurrentDirectory(path);
            }
        }

        public DateTime GetCreationTime(string path)
        {
            lock (owner.TreeLock)
            {
                return target.GetCreationTime(path);
            }
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            lock (owner.TreeLock)
            {
                return target.GetCreationTimeUtc(path);
            }
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            lock (owner.TreeLock)
            {
                target.SetCreationTime(path, creationTime);
            }
        }

        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            lock (owner.TreeLock)
            {
                target.SetCreationTimeUtc(path, creationTimeUtc);
            }
        }

        public DateTime GetLastAccessTime(string path)
        {
            lock (owner.TreeLock)
            {
                return target.GetLastAccessTime(path);
            }
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            lock (owner.TreeLock)
            {
                return target.GetLastAccessTimeUtc(path);
            }
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            lock (owner.TreeLock)
            {
                target.SetLastAccessTime(path, lastAccessTime);
            }
        }

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            lock (owner.TreeLock)
            {
                target.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
            }
        }

        public DateTime GetLastWriteTime(string path)
        {
            lock (owner.TreeLock)
            {
                return target.GetLastWriteTime(path);
            }
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            lock (owner.TreeLock)
            {
                return target.GetLastWriteTimeUtc(path);
            }
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            lock (owner.TreeLock)
            {
                target.SetLastWriteTime(path, lastWriteTime);
            }
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            lock (owner.TreeLock)
            {
                target.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
            }
        }
    }
}
