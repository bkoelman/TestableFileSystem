using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers;
using TestableFileSystem.Fakes.Handlers.Arguments;
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
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(searchPattern, nameof(searchPattern));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new DirectoryEnumerateEntriesHandler(root);
            var arguments =
                new DirectoryEnumerateEntriesArguments(absolutePath, path, searchPattern, searchOption, EnumerationFilter.Files);

            return handler.Handle(arguments);
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
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(searchPattern, nameof(searchPattern));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new DirectoryEnumerateEntriesHandler(root);
            var arguments = new DirectoryEnumerateEntriesArguments(absolutePath, path, searchPattern, searchOption,
                EnumerationFilter.Directories);

            return handler.Handle(arguments);
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
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(searchPattern, nameof(searchPattern));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new DirectoryEnumerateEntriesHandler(root);
            var arguments =
                new DirectoryEnumerateEntriesArguments(absolutePath, path, searchPattern, searchOption, EnumerationFilter.All);

            return handler.Handle(arguments);
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Not using deferred execution because we are running inside a lock.
            return GetFileSystemEntries(path, searchPattern, searchOption);
        }

        public bool Exists(string path)
        {
            try
            {
                AbsolutePath absolutePath = string.IsNullOrWhiteSpace(path) ? null : owner.ToAbsolutePath(path);

                var handler = new DirectoryExistsHandler(root);
                var arguments = new DirectoryExistsArguments(absolutePath);

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

        public IDirectoryInfo CreateDirectory(string path)
        {
            Guard.NotNull(path, nameof(path));
            AssertPathIsNotWhiteSpace(path);

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);

            var handler = new DirectoryCreateHandler(root);
            var arguments = new DirectoryCreateArguments(absolutePath);

            handler.Handle(arguments);
            return owner.ConstructDirectoryInfo(path);
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

            var handler = new DirectoryDeleteHandler(root, owner.CurrentDirectory);
            var arguments = new DirectoryDeleteArguments(absolutePath, recursive);

            handler.Handle(arguments);
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
