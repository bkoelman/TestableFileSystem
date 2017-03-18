using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class MemoryDirectory : IDirectory
    {
        // TODO: Guard that a name cannot occur both as a file -and- a directory.

        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        private readonly MemoryFileSystem owner;

        internal MemoryDirectory([NotNull] DirectoryEntry root, [NotNull] MemoryFileSystem owner)
        {
            Guard.NotNull(root, nameof(root));
            Guard.NotNull(owner, nameof(owner));

            this.root = root;
            this.owner = owner;
        }

        public IDirectoryInfo GetParent(string path)
        {
            throw new NotImplementedException();
        }

        public string GetDirectoryRoot(string path)
        {
            throw new NotImplementedException();
        }

        public string[] GetFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Not using deferred execution because we are running inside a lock.
            return GetFiles(path, searchPattern, searchOption);
        }

        public string[] GetDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Not using deferred execution because we are running inside a lock.
            return GetDirectories(path, searchPattern, searchOption);
        }

        public string[] GetFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Not using deferred execution because we are running inside a lock.
            return GetFileSystemEntries(path, searchPattern, searchOption);
        }

        public bool Exists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            AbsolutePath absolutePath = ToAbsolutePath(path);
            DirectoryEntry directory = root.TryGetExistingDirectory(absolutePath);

            return directory != null;
        }

        public IDirectoryInfo CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public void Delete(string path, bool recursive = false)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = ToAbsolutePath(path);

            DirectoryEntry directoryToDelete = root.GetExistingDirectory(absolutePath);
            AssertNotDeletingDrive(directoryToDelete, recursive);
            AssertNoConflictWithCurrentDirectory(directoryToDelete);

            root.DeleteDirectory(absolutePath, recursive);
        }

        [AssertionMethod]
        private static void AssertNotDeletingDrive([NotNull] DirectoryEntry directoryToDelete, bool isRecursive)
        {
            if (directoryToDelete.Parent?.Parent == null)
            {
                if (isRecursive)
                {
                    string path = directoryToDelete.GetAbsolutePath();
                    throw ErrorFactory.FileNotFound(path);
                }

                throw ErrorFactory.DirectoryIsNotEmpty();
            }
        }

        [AssertionMethod]
        private void AssertNoConflictWithCurrentDirectory([NotNull] DirectoryEntry directory)
        {
            DirectoryEntry entry = owner.CurrentDirectory;
            while (entry != null)
            {
                if (entry == directory)
                {
                    string path = directory.GetAbsolutePath();
                    throw ErrorFactory.FileIsInUse(path);
                }

                entry = entry.Parent;
            }
        }

        public void Move(string sourceDirName, string destDirName)
        {
            throw new NotImplementedException();
        }

        public string GetCurrentDirectory()
        {
            return owner.CurrentDirectory.GetAbsolutePath();
        }

        public void SetCurrentDirectory(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = ToAbsolutePath(path);
            if (!absolutePath.IsLocalDrive)
            {
                throw ErrorFactory.PathIsInvalid();
            }

            DirectoryEntry directory = root.GetExistingDirectory(absolutePath);
            owner.CurrentDirectory = directory;
        }

        public DateTime GetCreationTime(string path)
        {
            throw new NotImplementedException();
        }

        public DateTime GetCreationTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            throw new NotImplementedException();
        }

        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastAccessTime(string path)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastAccessTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            throw new NotImplementedException();
        }

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastWriteTime(string path)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastWriteTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            throw new NotImplementedException();
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            throw new NotImplementedException();
        }

        [NotNull]
        private AbsolutePath ToAbsolutePath([NotNull] string path)
        {
            string basePath = owner.CurrentDirectory.GetAbsolutePath();
            string rooted = Path.Combine(basePath, path);
            return new AbsolutePath(rooted);
        }
    }
}
