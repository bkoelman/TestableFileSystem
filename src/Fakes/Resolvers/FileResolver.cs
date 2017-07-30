using System;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Resolvers
{
    internal sealed class FileResolver
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        private readonly DirectoryResolver directoryResolver;

        [NotNull]
        public Func<Exception> ErrorNetworkShareNotFound
        {
            get => directoryResolver.ErrorNetworkShareNotFound;
            set => directoryResolver.ErrorNetworkShareNotFound = value;
        }

        [NotNull]
        public Func<string, Exception> ErrorDirectoryFoundAsFile
        {
            get => directoryResolver.ErrorDirectoryFoundAsFile;
            set => directoryResolver.ErrorDirectoryFoundAsFile = value;
        }

        [NotNull]
        public Func<string, Exception> ErrorLastDirectoryFoundAsFile
        {
            get => directoryResolver.ErrorLastDirectoryFoundAsFile;
            set => directoryResolver.ErrorLastDirectoryFoundAsFile = value;
        }

        [NotNull]
        public Func<string, Exception> ErrorFileFoundAsDirectory { get; set; }

        [NotNull]
        public Func<string, Exception> ErrorDirectoryNotFound
        {
            get => directoryResolver.ErrorDirectoryNotFound;
            set => directoryResolver.ErrorDirectoryNotFound = value;
        }

        [NotNull]
        public Func<string, Exception> ErrorFileNotFound { get; set; }

        [NotNull]
        public Func<string, Exception> ErrorFileExists { get; set; }

        public FileResolver([NotNull] DirectoryEntry root)
        {
            Guard.NotNull(root, nameof(root));
            this.root = root;
            directoryResolver = new DirectoryResolver(root);

            ErrorFileFoundAsDirectory = ErrorFactory.UnauthorizedAccess;
            ErrorFileNotFound = ErrorFactory.FileNotFound;
            ErrorFileExists = ErrorFactory.CannotCreateBecauseFileAlreadyExists;
        }

        [NotNull]
        public FileEntry ResolveExistingFile([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath parentPath = GetParentPath(path);

            DirectoryEntry directory = directoryResolver.ResolveDirectory(parentPath, path.GetText());
            string fileName = path.Components.Last();

            AssertIsNotDirectory(fileName, directory, path);
            AssertFileExists(fileName, directory, path);

            return directory.Files[fileName];
        }

        [NotNull]
        public DirectoryEntry ResolveContainingDirectoryForMissingFile([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath parentPath = GetParentPath(path);

            DirectoryEntry directory = directoryResolver.ResolveDirectory(parentPath, path.GetText());
            string fileName = path.Components.Last();

            AssertIsNotDirectory(fileName, directory, path);
            AssertFileDoesNotExist(fileName, directory, path);

            return directory;
        }

        public (DirectoryEntry containingDirectory, FileEntry existingFileOrNull, string fileName) TryResolveFile(
            [NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath parentPath = GetParentPath(path);

            DirectoryEntry directory = directoryResolver.ResolveDirectory(parentPath, path.GetText());
            string fileName = path.Components.Last();

            AssertIsNotDirectory(fileName, directory, path);

            FileEntry fileEntry = !directory.Files.ContainsKey(fileName) ? null : directory.Files[fileName];
            return (directory, fileEntry, fileName);
        }

        [NotNull]
        private static AbsolutePath GetParentPath([NotNull] AbsolutePath path)
        {
            AbsolutePath parentPath = path.TryGetParentPath();
            if (parentPath == null)
            {
                throw new Exception($"Internal Error: Path '{path}' has no parent.");
            }
            return parentPath;
        }

        private void AssertIsNotDirectory([NotNull] string fileName, [NotNull] DirectoryEntry directory,
            [NotNull] AbsolutePath path)
        {
            if (directory.Directories.ContainsKey(fileName))
            {
                throw ErrorFileFoundAsDirectory(path.GetText());
            }
        }

        private void AssertFileExists([NotNull] string fileName, [NotNull] DirectoryEntry directory, [NotNull] AbsolutePath path)
        {
            if (!directory.Files.ContainsKey(fileName))
            {
                throw ErrorFileNotFound(path.GetText());
            }
        }

        private void AssertFileDoesNotExist([NotNull] string fileName, [NotNull] DirectoryEntry directory,
            [NotNull] AbsolutePath path)
        {
            if (directory.Files.ContainsKey(fileName))
            {
                throw ErrorFileExists(path.GetText());
            }
        }
    }
}
