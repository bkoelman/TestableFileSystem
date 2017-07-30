using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFile : IFile
    {
        private static readonly DateTime ZeroFileTime = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
        private static readonly DateTime ZeroFileTimeUtc = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                AbsolutePath absolutePath = owner.ToAbsolutePath(path);
                var navigator = new PathNavigator(absolutePath);

                FileEntry file = root.TryGetExistingFile(navigator);
                return file != null;
            }
            catch (IOException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        public IFileStream Create(string path, int bufferSize = 4096, FileOptions options = FileOptions.None)
        {
            Guard.NotNull(path, nameof(path));
            AssertPathIsNotEmpty(path);

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            AssertValidCreationOptions(options, absolutePath);

            var resolver = new FileResolver(root);

            (DirectoryEntry containingDirectory, FileEntry _, string fileName) = resolver.TryResolveFile(absolutePath);

            FileEntry newFile = containingDirectory.GetOrCreateFile(fileName);

            if ((options & FileOptions.DeleteOnClose) != 0)
            {
                newFile.EnableDeleteOnClose();
            }

            return newFile.Open(FileMode.Create, FileAccess.ReadWrite);
        }

        private static void AssertPathIsNotEmpty([NotNull] string path)
        {
            if (path.Length == 0)
            {
                throw ErrorFactory.EmptyPathIsNotLegal(nameof(path));
            }
        }

        [AssertionMethod]
        private static void AssertValidCreationOptions(FileOptions options, [NotNull] AbsolutePath absolutePath)
        {
            if ((options & FileOptions.Encrypted) != 0)
            {
                throw new UnauthorizedAccessException($"Access to the path '{absolutePath.GetText()}' is denied.");
            }
        }

        private void AssertDoesNotExistAsDirectory([NotNull] AbsolutePath path)
        {
            var navigator = new PathNavigator(path);
            DirectoryEntry directory = root.TryGetExistingDirectory(navigator);
            if (directory != null)
            {
                throw ErrorFactory.UnauthorizedAccess(path.GetText());
            }
        }

        private void AssertParentIsDirectoryOrMissing([NotNull] AbsolutePath path)
        {
            AbsolutePath parentPath = path.TryGetParentPath();
            if (parentPath == null)
            {
                return;
            }

            var navigator = new PathNavigator(parentPath);
            DirectoryEntry directory = root.TryGetExistingDirectory(navigator);
            if (directory != null)
            {
                return;
            }

            throw ErrorFactory.DirectoryNotFound(path.GetText());
        }

        public IFileStream Open(string path, FileMode mode, FileAccess? access = null, FileShare share = FileShare.None)
        {
            Guard.NotNull(path, nameof(path));
            AssertPathIsNotEmpty(path);

            var handler = new FileOpenHandler(owner, root);
            var arguments = new FileOpenArguments(path, mode, access);

            return handler.Handle(arguments);
        }

        private void AssertNetworkShareOrDriveExists([NotNull] AbsolutePath absolutePath)
        {
            if (!root.Directories.ContainsKey(absolutePath.Components[0]))
            {
                if (absolutePath.IsOnLocalDrive)
                {
                    throw ErrorFactory.DirectoryNotFound(absolutePath.GetText());
                }

                throw ErrorFactory.NetworkPathNotFound();
            }
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            // TODO: Implement timings - https://support.microsoft.com/en-us/help/299648/description-of-ntfs-date-and-time-stamps-for-files-and-folders

            Guard.NotNull(sourceFileName, nameof(sourceFileName));
            Guard.NotNull(destFileName, nameof(destFileName));

            AssertFileNameIsNotEmpty(sourceFileName);
            AssertFileNameIsNotEmpty(destFileName);

            AbsolutePath sourcePath = owner.ToAbsolutePath(sourceFileName);
            AbsolutePath destinationPath = owner.ToAbsolutePath(destFileName);

            throw new NotImplementedException();
        }

        public void Move(string sourceFileName, string destFileName)
        {
            // TODO: Implement timings - https://support.microsoft.com/en-us/help/299648/description-of-ntfs-date-and-time-stamps-for-files-and-folders

            Guard.NotNull(sourceFileName, nameof(sourceFileName));
            Guard.NotNull(destFileName, nameof(destFileName));

            AssertFileNameIsNotEmpty(sourceFileName);
            AssertFileNameIsNotEmpty(destFileName);

            AbsolutePath sourcePath = owner.ToAbsolutePath(sourceFileName);
            AbsolutePath destinationPath = owner.ToAbsolutePath(destFileName);

            AssertNetworkShareOrDriveExists(destinationPath);
            AssertParentDirectoryDoesNotExistAsFile(destinationPath);

            FileEntry sourceFile = GetMoveSource(sourcePath);
            AssertHasExclusiveAccess(sourceFile);

            var destinationNavigator = new PathNavigator(destinationPath);
            root.MoveFile(sourceFile, destinationNavigator);
        }

        private static void AssertFileNameIsNotEmpty([NotNull] string path)
        {
            if (path.Length == 0)
            {
                throw ErrorFactory.EmptyFileNameIsNotLegal(nameof(path));
            }
        }

        private void AssertParentDirectoryDoesNotExistAsFile([NotNull] AbsolutePath absolutePath)
        {
            AbsolutePath parentPath = absolutePath.TryGetParentPath();
            if (parentPath != null)
            {
                var parentNavigator = new PathNavigator(parentPath);

                if (root.TryGetExistingFile(parentNavigator) != null)
                {
                    throw ErrorFactory.ParameterIsIncorrect();
                }
            }
        }

        private static void AssertHasExclusiveAccess([NotNull] FileEntry file)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.FileIsInUse();
            }
        }

        [NotNull]
        private FileEntry GetMoveSource([NotNull] AbsolutePath sourcePath)
        {
            var sourceNavigator = new PathNavigator(sourcePath);

            FileEntry sourceFile = root.TryGetExistingFile(sourceNavigator);
            if (sourceFile == null)
            {
                throw ErrorFactory.FileNotFound(sourcePath.GetText());
            }

            return sourceFile;
        }

        public void Delete(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            AssertNetworkShareOrDriveExists(absolutePath);
            AssertParentIsDirectoryOrMissing(absolutePath);

            var navigator = new PathNavigator(absolutePath);
            root.DeleteFile(navigator);
        }

        public FileAttributes GetAttributes(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            AssertNetworkShareOrDriveExists(absolutePath);

            BaseEntry entry = GetExistingEntry(absolutePath);
            return entry.Attributes;
        }

        [NotNull]
        private BaseEntry GetExistingEntry([NotNull] AbsolutePath absolutePath)
        {
            AssertParentIsDirectoryOrMissing(absolutePath);

            var navigator = new PathNavigator(absolutePath);
            BaseEntry entry = root.TryGetExistingFile(navigator);

            if (entry == null)
            {
                entry = root.TryGetExistingDirectory(navigator);
                if (entry == null)
                {
                    AbsolutePath parentPath = absolutePath.TryGetParentPath();

                    DirectoryEntry parentDirectory = parentPath != null
                        ? root.TryGetExistingDirectory(new PathNavigator(parentPath))
                        : null;

                    if (parentDirectory == null)
                    {
                        throw ErrorFactory.DirectoryNotFound(absolutePath.GetText());
                    }

                    throw ErrorFactory.FileNotFound(absolutePath.GetText());
                }
            }
            return entry;
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            AssertNetworkShareOrDriveExists(absolutePath);

            BaseEntry entry = GetExistingEntry(absolutePath);
            entry.Attributes = fileAttributes;
        }

        public DateTime GetCreationTime(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            BaseEntry entry = TryGetExistingEntry(absolutePath);
            return entry?.CreationTime ?? ZeroFileTime;
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            BaseEntry entry = TryGetExistingEntry(absolutePath);
            return entry?.CreationTimeUtc ?? ZeroFileTimeUtc;
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            FileEntry entry = GetExistingFile(absolutePath);
            entry.CreationTime = creationTime;
        }

        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            FileEntry entry = GetExistingFile(absolutePath);
            entry.CreationTimeUtc = creationTimeUtc;
        }

        public DateTime GetLastAccessTime(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            BaseEntry entry = TryGetExistingEntry(absolutePath);
            return entry?.LastAccessTime ?? ZeroFileTime;
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            BaseEntry entry = TryGetExistingEntry(absolutePath);
            return entry?.LastAccessTimeUtc ?? ZeroFileTimeUtc;
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            FileEntry entry = GetExistingFile(absolutePath);
            entry.LastAccessTime = lastAccessTime;
        }

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            FileEntry entry = GetExistingFile(absolutePath);
            entry.LastAccessTimeUtc = lastAccessTimeUtc;
        }

        public DateTime GetLastWriteTime(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            BaseEntry entry = TryGetExistingEntry(absolutePath);
            return entry?.LastWriteTime ?? ZeroFileTime;
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            BaseEntry entry = TryGetExistingEntry(absolutePath);
            return entry?.LastWriteTimeUtc ?? ZeroFileTimeUtc;
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            FileEntry entry = GetExistingFile(absolutePath);
            entry.LastWriteTime = lastWriteTime;
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            FileEntry entry = GetExistingFile(absolutePath);
            entry.LastWriteTimeUtc = lastWriteTimeUtc;
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

        [NotNull]
        private FileEntry GetExistingFile([NotNull] AbsolutePath absolutePath)
        {
            AssertParentIsDirectoryOrMissing(absolutePath);

            var navigator = new PathNavigator(absolutePath);
            FileEntry existingFile = root.TryGetExistingFile(navigator);
            if (existingFile == null)
            {
                AssertDoesNotExistAsDirectory(absolutePath);

                throw ErrorFactory.FileNotFound(absolutePath.GetText());
            }
            return existingFile;
        }

        [CanBeNull]
        private BaseEntry TryGetExistingEntry([NotNull] AbsolutePath absolutePath)
        {
            AbsolutePath parentPath = absolutePath.TryGetParentPath();
            if (parentPath == null)
            {
                return null;
            }

            var parentNavigator = new PathNavigator(parentPath);
            DirectoryEntry directory = root.TryGetExistingDirectory(parentNavigator);
            if (directory == null)
            {
                return null;
            }

            var navigator = new PathNavigator(absolutePath);
            return root.TryGetExistingFile(navigator) ?? (BaseEntry)root.TryGetExistingDirectory(navigator);
        }
    }
}
