using System;
using System.IO;
using JetBrains.Annotations;
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

            AssertDoesNotExistAsDirectory(absolutePath);
            AssertParentIsDirectoryOrMissing(absolutePath);

            var navigator = new PathNavigator(absolutePath);
            FileEntry newFile = root.GetOrCreateFile(navigator, false);

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
            AbsolutePath parentPath = path.GetParentPath();
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

            FileAccess fileAccess = access ?? (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite);

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            AssertDoesNotExistAsDirectory(absolutePath);
            AssertParentIsDirectoryOrMissing(absolutePath);

            var navigator = new PathNavigator(absolutePath);
            FileEntry existingFile = root.TryGetExistingFile(navigator);

            if (existingFile != null)
            {
                if (mode == FileMode.CreateNew)
                {
                    throw ErrorFactory.CannotCreateBecauseFileAlreadyExists(path);
                }

                return existingFile.Open(mode, fileAccess);
            }

            if (mode == FileMode.Open || mode == FileMode.Truncate)
            {
                throw ErrorFactory.FileNotFound(path);
            }

            FileEntry newFile = root.GetOrCreateFile(navigator, false);
            return newFile.Open(mode, fileAccess);
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            // TODO: Implement timings - https://support.microsoft.com/en-us/help/299648/description-of-ntfs-date-and-time-stamps-for-files-and-folders

            throw new NotImplementedException();
        }

        public void Move(string sourceFileName, string destFileName)
        {
            // TODO: Implement timings - https://support.microsoft.com/en-us/help/299648/description-of-ntfs-date-and-time-stamps-for-files-and-folders

            Guard.NotNullNorWhiteSpace(sourceFileName, nameof(sourceFileName));
            Guard.NotNullNorWhiteSpace(destFileName, nameof(destFileName));

            FileEntry sourceFile = GetMoveSource(sourceFileName);
            if (sourceFile.IsOpen())
            {
                // TODO: Copy

                throw ErrorFactory.FileIsInUse();
            }

            AbsolutePath destinationPath = owner.ToAbsolutePath(destFileName);
            var destinationNavigator = new PathNavigator(destinationPath);
            root.MoveFile(sourceFile, destinationNavigator);
        }

        [NotNull]
        private FileEntry GetMoveSource([NotNull] string sourceFileName)
        {
            AbsolutePath absoluteSourceFilePath = owner.ToAbsolutePath(sourceFileName);
            var sourceNavigator = new PathNavigator(absoluteSourceFilePath);

            FileEntry sourceFile = root.TryGetExistingFile(sourceNavigator);
            if (sourceFile == null)
            {
                throw ErrorFactory.FileNotFound(absoluteSourceFilePath.GetText());
            }

            return sourceFile;
        }

        public void Delete(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            AssertParentIsDirectoryOrMissing(absolutePath);

            var navigator = new PathNavigator(absolutePath);
            root.DeleteFile(navigator);
        }

        public FileAttributes GetAttributes(string path)
        {
            Guard.NotNull(path, nameof(path));

            BaseEntry entry = GetExistingEntry(path);
            return entry.Attributes;
        }

        [NotNull]
        private BaseEntry GetExistingEntry([NotNull] string path)
        {
            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            AssertParentIsDirectoryOrMissing(absolutePath);

            var navigator = new PathNavigator(absolutePath);
            BaseEntry entry = root.TryGetExistingFile(navigator);

            if (entry == null)
            {
                entry = root.TryGetExistingDirectory(navigator);
                if (entry == null)
                {
                    var parentPath = absolutePath.GetParentPath();

                    DirectoryEntry parentDirectory = parentPath != null
                        ? root.TryGetExistingDirectory(new PathNavigator(parentPath))
                        : null;

                    if (parentDirectory == null)
                    {
                        throw ErrorFactory.DirectoryNotFound(path);
                    }

                    throw ErrorFactory.FileNotFound(path);
                }
            }
            return entry;
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            Guard.NotNull(path, nameof(path));

            BaseEntry entry = GetExistingEntry(path);
            entry.Attributes = fileAttributes;
        }

        public DateTime GetCreationTime(string path)
        {
            Guard.NotNull(path, nameof(path));

            BaseEntry entry = TryGetExistingEntry(path);
            return entry?.CreationTime ?? ZeroFileTime;
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            BaseEntry entry = TryGetExistingEntry(path);
            return entry?.CreationTimeUtc ?? ZeroFileTimeUtc;
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            Guard.NotNull(path, nameof(path));

            FileEntry entry = GetExistingFile(path);
            entry.CreationTime = creationTime;
        }

        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            Guard.NotNull(path, nameof(path));

            FileEntry entry = GetExistingFile(path);
            entry.CreationTimeUtc = creationTimeUtc;
        }

        public DateTime GetLastAccessTime(string path)
        {
            Guard.NotNull(path, nameof(path));

            BaseEntry entry = TryGetExistingEntry(path);
            return entry?.LastAccessTime ?? ZeroFileTime;
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            BaseEntry entry = TryGetExistingEntry(path);
            return entry?.LastAccessTimeUtc ?? ZeroFileTimeUtc;
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            Guard.NotNull(path, nameof(path));

            FileEntry entry = GetExistingFile(path);
            entry.LastAccessTime = lastAccessTime;
        }

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            Guard.NotNull(path, nameof(path));

            FileEntry entry = GetExistingFile(path);
            entry.LastAccessTimeUtc = lastAccessTimeUtc;
        }

        public DateTime GetLastWriteTime(string path)
        {
            Guard.NotNull(path, nameof(path));

            BaseEntry entry = TryGetExistingEntry(path);
            return entry?.LastWriteTime ?? ZeroFileTime;
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            BaseEntry entry = TryGetExistingEntry(path);
            return entry?.LastWriteTimeUtc ?? ZeroFileTimeUtc;
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            Guard.NotNull(path, nameof(path));

            FileEntry entry = GetExistingFile(path);
            entry.LastWriteTime = lastWriteTime;
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            Guard.NotNull(path, nameof(path));

            FileEntry entry = GetExistingFile(path);
            entry.LastWriteTimeUtc = lastWriteTimeUtc;
        }

        internal long GetSize([NotNull] string path)
        {
            Guard.NotNull(path, nameof(path));
            AssertPathIsNotEmpty(path);

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
        private FileEntry GetExistingFile([NotNull] string path)
        {
            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
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
        private BaseEntry TryGetExistingEntry([NotNull] string path)
        {
            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            AbsolutePath parentPath = absolutePath.GetParentPath();
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
