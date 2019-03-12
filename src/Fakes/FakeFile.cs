using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Handlers;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFile : IFile
    {
        [NotNull]
        private readonly VolumeContainer container;

        [NotNull]
        private readonly FakeFileSystem owner;

        internal FakeFile([NotNull] VolumeContainer container, [NotNull] FakeFileSystem owner)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNull(owner, nameof(owner));

            this.container = container;
            this.owner = owner;
        }

        public bool Exists(string path)
        {
            try
            {
                AbsolutePath absolutePath = string.IsNullOrWhiteSpace(path) ? null : owner.ToAbsolutePath(path);

                var handler = new FileExistsHandler(container);
                var arguments = new EntryExistsArguments(absolutePath);

                return handler.Handle(arguments);
            }
            catch (Exception ex) when (ShouldSuppress(ex))
            {
                return false;
            }
        }

        private static bool ShouldSuppress([NotNull] Exception ex)
        {
            return ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException ||
                ex is NotSupportedException;
        }

        public IFileStream Create(string path, int bufferSize = 4096, FileOptions options = FileOptions.None)
        {
            Guard.NotNull(path, nameof(path));
            AssertPathIsNotEmpty(path);

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileOpenHandler(container);
            var arguments = new FileOpenArguments(absolutePath, FileMode.Create, FileAccess.ReadWrite, options);

            return handler.Handle(arguments);
        }

        public IFileStream Open(string path, FileMode mode, FileAccess? access = null, FileShare share = FileShare.None)
        {
            Guard.NotNull(path, nameof(path));
            AssertPathIsNotEmpty(path);

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileOpenHandler(container);
            var arguments = new FileOpenArguments(absolutePath, mode, access, FileOptions.None);

            return handler.Handle(arguments);
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            Guard.NotNull(sourceFileName, nameof(sourceFileName));
            Guard.NotNull(destFileName, nameof(destFileName));

            AssertFileNameIsNotEmpty(sourceFileName);
            AssertFileNameIsNotEmpty(destFileName);

            InnerCopy(sourceFileName, destFileName, overwrite, false);
        }

        private void InnerCopy([NotNull] string sourceFileName, [NotNull] string destFileName, bool overwrite,
            bool isCopyAfterMoveFailed)
        {
            FileCopyResult copyResult = null;

            try
            {
                container.FileSystemLock.ExecuteInLock(() =>
                {
                    AbsolutePath sourcePath = owner.ToAbsolutePath(sourceFileName);
                    AbsolutePath destinationPath = owner.ToAbsolutePath(destFileName);

                    var handler = new FileCopyHandler(container, owner.ChangeTracker);
                    var arguments = new FileCopyArguments(sourcePath, destinationPath, overwrite, isCopyAfterMoveFailed);

                    copyResult = handler.Handle(arguments);
                });

                WaitOnIndicator(owner.CopyWaitIndicator);

                copyResult.SourceStream.CopyTo(copyResult.DestinationStream);
            }
            finally
            {
                copyResult?.DestinationStream.Dispose();
                copyResult?.SourceStream.Dispose();
            }

            container.FileSystemLock.ExecuteInLock(() =>
            {
                copyResult.DestinationFile.LastWriteTimeUtc = copyResult.SourceFile.LastWriteTimeUtc;
            });
        }

        private void WaitOnIndicator([NotNull] WaitIndicator indicator)
        {
            indicator.SetStarted();
            indicator.WaitForComplete();
        }

        public void Move(string sourceFileName, string destFileName)
        {
            Guard.NotNull(sourceFileName, nameof(sourceFileName));
            Guard.NotNull(destFileName, nameof(destFileName));

            AssertFileNameIsNotEmpty(sourceFileName);
            AssertFileNameIsNotEmpty(destFileName);

            AbsolutePath sourcePath = owner.ToAbsolutePath(sourceFileName);
            AbsolutePath destinationPath = owner.ToAbsolutePath(destFileName);

            var handler = new FileMoveHandler(container);
            var arguments = new EntryMoveArguments(sourcePath, destinationPath);

            handler.Handle(arguments);

            if (handler.IsCopyRequired)
            {
                InnerCopy(sourceFileName, destFileName, false, true);

                if (handler.IsSourceReadOnly)
                {
                    var setAttributesHandler = new FileSetAttributesHandler(container);
                    var setAttributeArguments = new FileSetAttributesArguments(sourcePath, FileAttributes.Normal,
                        FileAccessKinds.Attributes | FileAccessKinds.Read);

                    setAttributesHandler.Handle(setAttributeArguments);
                }

                Delete(sourceFileName);
            }
        }

        public void Delete(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileDeleteHandler(container);
            var arguments = new FileDeleteArguments(absolutePath);

            handler.Handle(arguments);
        }

        public FileAttributes GetAttributes(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetAttributesHandler(container);
            var arguments = new FileGetAttributesArguments(absolutePath);

            return handler.Handle(arguments);
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetAttributesHandler(container);
            var arguments = new FileSetAttributesArguments(absolutePath, fileAttributes);

            handler.Handle(arguments);
        }

        public DateTime GetCreationTime(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetTimeHandler(container);
            var arguments = new FileGetTimeArguments(absolutePath, FileTimeKind.CreationTime, false);

            return handler.Handle(arguments);
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetTimeHandler(container);
            var arguments = new FileGetTimeArguments(absolutePath, FileTimeKind.CreationTime, true);

            return handler.Handle(arguments);
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetTimeHandler(container, owner.ChangeTracker);
            var arguments = new EntrySetTimeArguments(absolutePath, FileTimeKind.CreationTime, false, creationTime);

            handler.Handle(arguments);
        }

        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetTimeHandler(container, owner.ChangeTracker);
            var arguments = new EntrySetTimeArguments(absolutePath, FileTimeKind.CreationTime, true, creationTimeUtc);

            handler.Handle(arguments);
        }

        public DateTime GetLastAccessTime(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetTimeHandler(container);
            var arguments = new FileGetTimeArguments(absolutePath, FileTimeKind.LastAccessTime, false);

            return handler.Handle(arguments);
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetTimeHandler(container);
            var arguments = new FileGetTimeArguments(absolutePath, FileTimeKind.LastAccessTime, true);

            return handler.Handle(arguments);
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetTimeHandler(container, owner.ChangeTracker);
            var arguments = new EntrySetTimeArguments(absolutePath, FileTimeKind.LastAccessTime, false, lastAccessTime);

            handler.Handle(arguments);
        }

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetTimeHandler(container, owner.ChangeTracker);
            var arguments = new EntrySetTimeArguments(absolutePath, FileTimeKind.LastAccessTime, true, lastAccessTimeUtc);

            handler.Handle(arguments);
        }

        public DateTime GetLastWriteTime(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetTimeHandler(container);
            var arguments = new FileGetTimeArguments(absolutePath, FileTimeKind.LastWriteTime, false);

            return handler.Handle(arguments);
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetTimeHandler(container);
            var arguments = new FileGetTimeArguments(absolutePath, FileTimeKind.LastWriteTime, true);

            return handler.Handle(arguments);
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetTimeHandler(container, owner.ChangeTracker);
            var arguments = new EntrySetTimeArguments(absolutePath, FileTimeKind.LastWriteTime, false, lastWriteTime);

            handler.Handle(arguments);
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetTimeHandler(container, owner.ChangeTracker);
            var arguments = new EntrySetTimeArguments(absolutePath, FileTimeKind.LastWriteTime, true, lastWriteTimeUtc);

            handler.Handle(arguments);
        }

#if !NETSTANDARD1_3
        public void Encrypt(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileCryptoHandler(container);
            var arguments = new FileCryptoArguments(absolutePath, true);

            handler.Handle(arguments);
        }

        public void Decrypt(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileCryptoHandler(container);
            var arguments = new FileCryptoArguments(absolutePath, false);

            handler.Handle(arguments);
        }

        public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName,
            bool ignoreMetadataErrors = false)
        {
            Guard.NotNull(sourceFileName, nameof(sourceFileName));
            Guard.NotNull(destinationFileName, nameof(destinationFileName));

            AbsolutePath sourcePath = owner.ToAbsolutePath(sourceFileName);
            AbsolutePath destinationPath = owner.ToAbsolutePath(destinationFileName);
            AbsolutePath backupDestinationPath =
                destinationBackupFileName != null ? owner.ToAbsolutePath(destinationBackupFileName) : null;

            var handler = new FileReplaceHandler(container);
            var arguments = new FileReplaceArguments(sourcePath, destinationPath, backupDestinationPath);

            handler.Handle(arguments);
        }
#endif

        private static void AssertPathIsNotEmpty([NotNull] string path)
        {
            if (path.Length == 0)
            {
                throw ErrorFactory.System.EmptyPathIsNotLegal(nameof(path));
            }
        }

        private static void AssertFileNameIsNotEmpty([NotNull] string path)
        {
            if (path.Length == 0)
            {
                throw ErrorFactory.System.EmptyFileNameIsNotLegal(nameof(path));
            }
        }
    }
}
