using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class FileOperationLocker<TFile> : OperationLocker, IFile
        where TFile : class, IFile
    {
        [NotNull]
        private readonly TFile target;

        public FileOperationLocker([NotNull] FakeFileSystem owner, [NotNull] TFile target)
            : base(owner)
        {
            Guard.NotNull(target, nameof(target));
            this.target = target;
        }

        public bool Exists(string path)
        {
            return ExecuteInLock(() => target.Exists(path));
        }

        public IFileStream Create(string path, int bufferSize = 4096, FileOptions options = FileOptions.None)
        {
            return ExecuteInLock(() => target.Create(path, bufferSize, options));
        }

        public IFileStream Open(string path, FileMode mode, FileAccess? access = null, FileShare share = FileShare.None)
        {
            return ExecuteInLock(() => target.Open(path, mode, access, share));
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            // Locking is handled by caller.
            target.Copy(sourceFileName, destFileName, overwrite);
        }

        public void Move(string sourceFileName, string destFileName)
        {
            ExecuteInLock(() => target.Move(sourceFileName, destFileName));
        }

        public void Delete(string path)
        {
            ExecuteInLock(() => target.Delete(path));
        }

        public FileAttributes GetAttributes(string path)
        {
            return ExecuteInLock(() => target.GetAttributes(path));
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            ExecuteInLock(() => target.SetAttributes(path, fileAttributes));
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

        [NotNull]
        internal TResult ExecuteOnFile<TResult>([NotNull] Func<TFile, TResult> operation)
        {
            Guard.NotNull(operation, nameof(operation));

            return ExecuteInLock(() => operation(target));
        }
    }
}
