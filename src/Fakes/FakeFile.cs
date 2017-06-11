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

            AbsolutePath absolutePath;
            try
            {
                absolutePath = owner.ToAbsolutePath(path.TrimEnd());
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }

            FileEntry file = root.TryGetExistingFile(absolutePath);
            return file != null;
        }

        public IFileStream Create(string path, int bufferSize = 4096, FileOptions options = FileOptions.None)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            AssertValidOptions(options, absolutePath);

            FileEntry newFile = root.GetOrCreateFile(absolutePath, false);
            return newFile.Open(FileMode.Create, FileAccess.ReadWrite, bufferSize);
        }

        [AssertionMethod]
        private static void AssertValidOptions(FileOptions options, [NotNull] AbsolutePath absolutePath)
        {
            if ((options & FileOptions.Encrypted) != 0)
            {
                throw new UnauthorizedAccessException($"Access to the path '{absolutePath.GetText()}' is denied.");
            }

            if ((options & FileOptions.DeleteOnClose) != 0)
            {
                throw new NotImplementedException("Option 'DeleteOnClose' is not supported.");
            }
        }

        public IFileStream Open(string path, FileMode mode, FileAccess? access = null, FileShare share = FileShare.None)
        {
            Guard.NotNull(path, nameof(path));

            FileAccess fileAccess = access ?? (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite);

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            FileEntry existingFile = root.TryGetExistingFile(absolutePath);

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

            FileEntry newFile = root.GetOrCreateFile(absolutePath, false);
            return newFile.Open(mode, fileAccess);
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            throw new NotImplementedException();
        }

        public void Move(string sourceFileName, string destFileName)
        {
            Guard.NotNullNorWhiteSpace(sourceFileName, nameof(sourceFileName));
            Guard.NotNullNorWhiteSpace(destFileName, nameof(destFileName));

            FileEntry sourceFile = GetMoveSource(sourceFileName);
            if (sourceFile.IsOpen())
            {
                // TODO: Copy

                throw ErrorFactory.FileIsInUse();
            }

            AbsolutePath destinationPath = owner.ToAbsolutePath(destFileName);
            root.MoveFile(sourceFile, destinationPath);
        }

        [NotNull]
        private FileEntry GetMoveSource([NotNull] string sourceFileName)
        {
            AbsolutePath absoluteSourceFilePath = owner.ToAbsolutePath(sourceFileName);

            FileEntry sourceFile = root.TryGetExistingFile(absoluteSourceFilePath);
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
            root.DeleteFile(absolutePath);
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
            BaseEntry entry = root.TryGetExistingFile(absolutePath);

            if (entry == null)
            {
                entry = root.TryGetExistingDirectory(absolutePath);
                if (entry == null)
                {
                    string parentName = Path.GetDirectoryName(path);
                    DirectoryEntry parentDirectory = root.TryGetExistingDirectory(new AbsolutePath(parentName));
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

            FileEntry entry = GetExistingFile(path);
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

        [NotNull]
        private FileEntry GetExistingFile([NotNull] string path)
        {
            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            FileEntry existingFile = root.TryGetExistingFile(absolutePath);

            if (existingFile == null)
            {
                DirectoryEntry directory = root.TryGetExistingDirectory(absolutePath);
                if (directory != null)
                {
                    throw ErrorFactory.UnauthorizedAccess(absolutePath.GetText());
                }

                throw ErrorFactory.FileNotFound(absolutePath.GetText());
            }
            return existingFile;
        }

        [CanBeNull]
        private BaseEntry TryGetExistingEntry([NotNull] string path)
        {
            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            return root.TryGetExistingFile(absolutePath) ?? (BaseEntry)root.TryGetExistingDirectory(absolutePath);
        }
    }
}
