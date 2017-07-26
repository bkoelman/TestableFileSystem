using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeDirectory : IDirectory
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        private readonly FakeFileSystem owner;

        internal FakeDirectory([NotNull] DirectoryEntry root, [NotNull] FakeFileSystem owner)
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
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            return absolutePath.GetRootName();
        }

        public string[] GetFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var handler = new DirectoryEnumerateEntriesHandler(owner, root, path, searchPattern, searchOption, EnumerationFilter.Files);
            return handler.Handle().ToArray();
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
            var handler = new DirectoryEnumerateEntriesHandler(owner, root, path, searchPattern, searchOption, EnumerationFilter.Directories);
            return handler.Handle().ToArray();
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
            var handler = new DirectoryEnumerateEntriesHandler(owner, root, path, searchPattern, searchOption, EnumerationFilter.All);
            return handler.Handle().ToArray();
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Not using deferred execution because we are running inside a lock.
            return GetFileSystemEntries(path, searchPattern, searchOption);
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

                DirectoryEntry directory = root.TryGetExistingDirectory(navigator);
                return directory != null;
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

        public IDirectoryInfo CreateDirectory(string path)
        {
            Guard.NotNull(path, nameof(path));
            AssertPathIsNotWhiteSpace(path);

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            AssertNetworkShareOrDriveExists(absolutePath, true);

            var navigator = new PathNavigator(absolutePath);

            if (absolutePath.IsVolumeRoot && root.TryGetExistingDirectory(navigator) == null)
            {
                throw ErrorFactory.DirectoryNotFound(absolutePath.GetText());
            }

            root.CreateDirectories(navigator);
            return owner.ConstructDirectoryInfo(absolutePath.GetText());
        }

        private void AssertNetworkShareOrDriveExists([NotNull] AbsolutePath absolutePath, bool isCreatingDirectory = false)
        {
            if (!root.Directories.ContainsKey(absolutePath.Components[0]))
            {
                if (absolutePath.IsOnLocalDrive)
                {
                    throw ErrorFactory.DirectoryNotFound(absolutePath.GetText());
                }

                if (isCreatingDirectory && absolutePath.IsVolumeRoot)
                {
                    throw ErrorFactory.DirectoryNotFound(absolutePath.GetText());
                }

                throw ErrorFactory.NetworkPathNotFound();
            }
        }

        private static void AssertPathIsNotWhiteSpace([NotNull] string path)
        {
            if (path.Length == 0)
            {
                throw ErrorFactory.PathCannotBeEmptyOrWhitespace(nameof(path));
            }
        }

        public void Delete(string path, bool recursive = false)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            AssertNetworkShareOrDriveExists(absolutePath);

            var navigator = new PathNavigator(absolutePath);

            DirectoryEntry directoryToDelete = root.GetExistingDirectory(navigator);
            AssertNoConflictWithCurrentDirectory(directoryToDelete);

            root.DeleteDirectory(navigator, recursive);
        }

        [AssertionMethod]
        private void AssertNoConflictWithCurrentDirectory([NotNull] DirectoryEntry directory)
        {
            if (owner.CurrentDirectory.IsAtOrAboveCurrentDirectory(directory))
            {
                throw ErrorFactory.FileIsInUse(directory.GetAbsolutePath());
            }
        }

        public void Move(string sourceDirName, string destDirName)
        {
            throw new NotImplementedException();
        }

        public string GetCurrentDirectory()
        {
            return owner.CurrentDirectory.GetValue().GetAbsolutePath();
        }

        public void SetCurrentDirectory(string path)
        {
            Guard.NotNull(path, nameof(path));
            AssertPathIsNotWhiteSpace(path);

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            AssertNetworkShareOrDriveExists(absolutePath);

            if (!absolutePath.IsOnLocalDrive)
            {
                // TODO: Remove this limitation.
                throw ErrorFactory.PathIsInvalid();
            }

            var navigator = new PathNavigator(absolutePath);
            DirectoryEntry directory = root.GetExistingDirectory(navigator);
            owner.CurrentDirectory.SetValue(directory);
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
    }
}
