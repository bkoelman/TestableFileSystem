using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFile : IFile
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        private readonly FakeFileSystem owner;

        internal FakeFile([NotNull] DirectoryEntry root, [NotNull] FakeFileSystem owner)
        {
            Guard.NotNull(root, nameof(root));
            Guard.NotNull(owner, nameof(owner));

            this.root = root;
            this.owner = owner;
        }

        public bool Exists(string path)
        {
            try
            {
                AbsolutePath absolutePath = string.IsNullOrWhiteSpace(path) ? null : owner.ToAbsolutePath(path);

                var handler = new FileExistsHandler(root);
                var arguments = new FileExistsArguments(absolutePath);

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

            var handler = new FileCreateHandler(root);
            var arguments = new FileCreateArguments(absolutePath, options);

            return handler.Handle(arguments);
        }

        private static void AssertPathIsNotEmpty([NotNull] string path)
        {
            if (path.Length == 0)
            {
                throw ErrorFactory.EmptyPathIsNotLegal(nameof(path));
            }
        }

        public IFileStream Open(string path, FileMode mode, FileAccess? access = null, FileShare share = FileShare.None)
        {
            Guard.NotNull(path, nameof(path));
            AssertPathIsNotEmpty(path);

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileOpenHandler(root);
            var arguments = new FileOpenArguments(absolutePath, mode, access);

            return handler.Handle(arguments);
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            Guard.NotNull(sourceFileName, nameof(sourceFileName));
            Guard.NotNull(destFileName, nameof(destFileName));

            AssertFileNameIsNotEmpty(sourceFileName);
            AssertFileNameIsNotEmpty(destFileName);

            FileEntry sourceFile;
            FileEntry destinationFile;

            Stream sourceStream = null;
            Stream destinationStream = null;

            try
            {
                lock (owner.TreeLock)
                {
                    AbsolutePath sourcePath = owner.ToAbsolutePath(sourceFileName);
                    AbsolutePath destinationPath = owner.ToAbsolutePath(destFileName);

                    var handler = new FileCopyHandler(root);
                    var arguments = new FileCopyArguments(sourcePath, destinationPath, overwrite);

                    (sourceFile, sourceStream, destinationFile, destinationStream) = handler.Handle(arguments);
                }

                WaitOnIndicator(owner.CopyWaitIndicator);

                sourceStream.CopyTo(destinationStream);
            }
            finally
            {
                destinationStream?.Dispose();
                sourceStream?.Dispose();
            }

            lock (owner.TreeLock)
            {
                destinationFile.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;
            }
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

            var handler = new FileMoveHandler(root);
            var arguments = new FileMoveArguments(sourcePath, destinationPath);

            handler.Handle(arguments);
        }

        private static void AssertFileNameIsNotEmpty([NotNull] string path)
        {
            if (path.Length == 0)
            {
                throw ErrorFactory.EmptyFileNameIsNotLegal(nameof(path));
            }
        }

        public void Delete(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileDeleteHandler(root);
            var arguments = new FileDeleteArguments(absolutePath);

            handler.Handle(arguments);
        }

        public FileAttributes GetAttributes(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetAttributesHandler(root);
            var arguments = new FileGetAttributesArguments(absolutePath);

            return handler.Handle(arguments);
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetAttributesHandler(root);
            var arguments = new FileSetAttributesArguments(absolutePath, fileAttributes);

            handler.Handle(arguments);
        }

        public DateTime GetCreationTime(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetTimeHandler(root);
            var arguments = new FileGetTimeArguments(absolutePath, FileTimeKind.CreationTime, false);

            return handler.Handle(arguments);
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetTimeHandler(root);
            var arguments = new FileGetTimeArguments(absolutePath, FileTimeKind.CreationTime, true);

            return handler.Handle(arguments);
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetTimeHandler(root);
            var arguments = new FileSetTimeArguments(absolutePath, FileTimeKind.CreationTime, false, creationTime);

            handler.Handle(arguments);
        }

        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetTimeHandler(root);
            var arguments = new FileSetTimeArguments(absolutePath, FileTimeKind.CreationTime, true, creationTimeUtc);

            handler.Handle(arguments);
        }

        public DateTime GetLastAccessTime(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetTimeHandler(root);
            var arguments = new FileGetTimeArguments(absolutePath, FileTimeKind.LastAccessTime, false);

            return handler.Handle(arguments);
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetTimeHandler(root);
            var arguments = new FileGetTimeArguments(absolutePath, FileTimeKind.LastAccessTime, true);

            return handler.Handle(arguments);
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetTimeHandler(root);
            var arguments = new FileSetTimeArguments(absolutePath, FileTimeKind.LastAccessTime, false, lastAccessTime);

            handler.Handle(arguments);
        }

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetTimeHandler(root);
            var arguments = new FileSetTimeArguments(absolutePath, FileTimeKind.LastAccessTime, true, lastAccessTimeUtc);

            handler.Handle(arguments);
        }

        public DateTime GetLastWriteTime(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetTimeHandler(root);
            var arguments = new FileGetTimeArguments(absolutePath, FileTimeKind.LastWriteTime, false);

            return handler.Handle(arguments);
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileGetTimeHandler(root);
            var arguments = new FileGetTimeArguments(absolutePath, FileTimeKind.LastWriteTime, true);

            return handler.Handle(arguments);
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetTimeHandler(root);
            var arguments = new FileSetTimeArguments(absolutePath, FileTimeKind.LastWriteTime, false, lastWriteTime);

            handler.Handle(arguments);
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new FileSetTimeHandler(root);
            var arguments = new FileSetTimeArguments(absolutePath, FileTimeKind.LastWriteTime, true, lastWriteTimeUtc);

            handler.Handle(arguments);
        }

        internal long GetSize([NotNull] string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            var navigator = new PathNavigator(absolutePath);

            FileEntry existingFile = root.TryGetExistingFile(navigator);

            if (existingFile == null)
            {
                throw ErrorFactory.FileNotFound(path);
            }

            return existingFile.Size;
        }
    }
}
