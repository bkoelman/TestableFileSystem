using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class FileOperationLocker<TFile> : IFile
        where TFile : class, IFile
    {
        [NotNull]
        private readonly FileSystemLock fileSystemLock;

        [NotNull]
        private readonly TFile target;

        public FileOperationLocker([NotNull] FileSystemLock fileSystemLock, [NotNull] TFile target)
        {
            Guard.NotNull(fileSystemLock, nameof(fileSystemLock));
            Guard.NotNull(target, nameof(target));

            this.fileSystemLock = fileSystemLock;
            this.target = target;
        }

        public bool Exists(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.Exists(path));
        }

        public IFileStream Create(string path, int bufferSize = 4096, FileOptions options = FileOptions.None)
        {
            return fileSystemLock.ExecuteInLock(() => target.Create(path, bufferSize, options));
        }

        public IFileStream Open(string path, FileMode mode, FileAccess? access = null, FileShare share = FileShare.None)
        {
            return fileSystemLock.ExecuteInLock(() => target.Open(path, mode, access, share));
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            // Locking is handled by caller.
            target.Copy(sourceFileName, destFileName, overwrite);
        }

        public void Move(string sourceFileName, string destFileName)
        {
            fileSystemLock.ExecuteInLock(() => target.Move(sourceFileName, destFileName));
        }

        public void Delete(string path)
        {
            fileSystemLock.ExecuteInLock(() => target.Delete(path));
        }

        public FileAttributes GetAttributes(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetAttributes(path));
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            fileSystemLock.ExecuteInLock(() => target.SetAttributes(path, fileAttributes));
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
        public void Encrypt(string path)
        {
            fileSystemLock.ExecuteInLock(() => target.Encrypt(path));
        }

        public void Decrypt(string path)
        {
            fileSystemLock.ExecuteInLock(() => target.Decrypt(path));
        }

        public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName,
            bool ignoreMetadataErrors = false)
        {
            fileSystemLock.ExecuteInLock(() =>
                target.Replace(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors));
        }
#endif
    }
}
