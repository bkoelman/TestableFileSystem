using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class MemoryFile : IFile
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        private readonly MemoryFileSystem owner;

        internal MemoryFile([NotNull] DirectoryEntry root, [NotNull] MemoryFileSystem owner)
        {
            Guard.NotNull(root, nameof(root));
            Guard.NotNull(owner, nameof(owner));

            this.root = root;
            this.owner = owner;
        }

        public bool Exists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            FileEntry file = root.TryGetExistingFile(absolutePath);

            return file != null;
        }

        public IFileStream Create(string path, int bufferSize = 4096, FileOptions options = FileOptions.None)
        {
            Guard.NotNull(path, nameof(path));
            AssertValidOptions(options);

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            FileEntry newFile = root.GetOrCreateFile(absolutePath, false);
            return newFile.Open(FileMode.Create, FileAccess.ReadWrite, bufferSize);
        }

        [AssertionMethod]
        private static void AssertValidOptions(FileOptions options)
        {
            bool hasEncrypted = (options & FileOptions.Encrypted) != 0;
            bool hasDeleteOnClose = (options & FileOptions.DeleteOnClose) != 0;

            if (hasEncrypted || hasDeleteOnClose)
            {
                throw new NotSupportedException($"Set of options '{options}' is not supported.");
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
                    throw ErrorFactory.FileAlreadyExists(path);
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
            throw new NotImplementedException();
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

            FileEntry entry = GetExistingFile(path);
            return entry.Attributes;
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

            FileEntry entry = GetExistingFile(path);
            return entry.CreationTime;
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            FileEntry entry = GetExistingFile(path);
            return entry.CreationTimeUtc;
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

            FileEntry entry = GetExistingFile(path);
            return entry.LastAccessTime;
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            FileEntry entry = GetExistingFile(path);
            return entry.LastAccessTimeUtc;
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

            FileEntry entry = GetExistingFile(path);
            return entry.LastWriteTime;
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            Guard.NotNull(path, nameof(path));

            FileEntry entry = GetExistingFile(path);
            return entry.LastWriteTimeUtc;
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
                throw ErrorFactory.FileNotFound(path);
            }
            return existingFile;
        }
    }
}
