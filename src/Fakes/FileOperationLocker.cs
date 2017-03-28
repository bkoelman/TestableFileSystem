using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class FileOperationLocker : IFile
    {
        [NotNull]
        private readonly MemoryFileSystem owner;

        [NotNull]
        private readonly IFile target;

        public FileOperationLocker([NotNull] MemoryFileSystem owner, [NotNull] IFile target)
        {
            Guard.NotNull(owner, nameof(owner));
            Guard.NotNull(target, nameof(target));

            this.owner = owner;
            this.target = target;
        }

        public bool Exists(string path)
        {
            lock (owner.TreeLock)
            {
                return target.Exists(path);
            }
        }

        public IFileStream Create(string path, int bufferSize = 4096, FileOptions options = FileOptions.None)
        {
            lock (owner.TreeLock)
            {
                return target.Create(path, bufferSize, options);
            }
        }

        public IFileStream Open(string path, FileMode mode, FileAccess? access = null, FileShare share = FileShare.None)
        {
            lock (owner.TreeLock)
            {
                return target.Open(path, mode, access, share);
            }
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            lock (owner.TreeLock)
            {
                target.Copy(sourceFileName, destFileName, overwrite);
            }
        }

        public void Move(string sourceFileName, string destFileName)
        {
            lock (owner.TreeLock)
            {
                target.Move(sourceFileName, destFileName);
            }
        }

        public void Delete(string path)
        {
            lock (owner.TreeLock)
            {
                target.Delete(path);
            }
        }

        public FileAttributes GetAttributes(string path)
        {
            lock (owner.TreeLock)
            {
                return target.GetAttributes(path);
            }
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            lock (owner.TreeLock)
            {
                target.SetAttributes(path, fileAttributes);
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
