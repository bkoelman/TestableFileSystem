using System;
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
        public Func<Exception> OnNetworkShareNotFound
        {
            get => directoryResolver.ErrorNetworkShareNotFound;
            set => directoryResolver.ErrorNetworkShareNotFound = value;
        }

        [NotNull]
        public Func<string, Exception> OnDirectoryFoundAsFile
        {
            get => directoryResolver.ErrorDirectoryFoundAsFile;
            set => directoryResolver.ErrorDirectoryFoundAsFile = value;
        }

        [NotNull]
        public Func<string, Exception> ErrorFileFoundAsDirectory { get; set; }

        [NotNull]
        public Func<string, Exception> OnDirectoryNotFound
        {
            get => directoryResolver.ErrorDirectoryNotFound;
            set => directoryResolver.ErrorDirectoryNotFound = value;
        }

        [NotNull]
        public Func<string, Exception> OnFileNotFound { get; set; }

        [NotNull]
        public Func<string, Exception> OnFileExists { get; set; }

        public FileResolver([NotNull] DirectoryEntry root)
        {
            Guard.NotNull(root, nameof(root));
            this.root = root;
            directoryResolver = new DirectoryResolver(root);

            ErrorFileFoundAsDirectory = ErrorFactory.UnauthorizedAccess;
            OnFileNotFound = ErrorFactory.FileNotFound;
            OnFileExists = ErrorFactory.CannotCreateBecauseFileAlreadyExists;
        }

        [NotNull]
        public FileEntry ResolveExistingFile([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath parentPath = path.TryGetParentPath();
            if (parentPath == null)
            {
                throw new Exception("TODO");
            }

            DirectoryEntry directory = directoryResolver.ResolveDirectory(parentPath, path.GetText());

            string fileName = path.Components[path.Components.Count - 1];

            if (directory.Directories.ContainsKey(fileName))
            {
                throw ErrorFileFoundAsDirectory(path.GetText());
            }

            if (!directory.Files.ContainsKey(fileName))
            {
                throw OnFileNotFound(path.GetText());
            }

            return directory.Files[fileName];
        }

        [NotNull]
        public DirectoryEntry ResolveParentDirectoryForMissingFile([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath parentPath = path.TryGetParentPath();
            if (parentPath == null)
            {
                throw new Exception("TODO");
            }

            string fileName = path.Components[path.Components.Count - 1];
            DirectoryEntry parentDirectory = directoryResolver.ResolveDirectory(parentPath, path.GetText());

            if (parentDirectory.Directories.ContainsKey(fileName))
            {
                throw ErrorFileFoundAsDirectory(path.GetText());
            }

            if (parentDirectory.Files.ContainsKey(fileName))
            {
                throw OnFileExists(path.GetText());
            }

            return parentDirectory;
        }

        public (DirectoryEntry parentDirectory, FileEntry existingFile, string fileName) TryResolveFile([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath parentPath = path.TryGetParentPath();
            if (parentPath == null)
            {
                throw new Exception("TODO");
            }

            DirectoryEntry directory = directoryResolver.ResolveDirectory(parentPath, path.GetText());

            string fileName = path.Components[path.Components.Count - 1];

            if (directory.Directories.ContainsKey(fileName))
            {
                throw ErrorFileFoundAsDirectory(path.GetText());
            }

            FileEntry fileEntry = !directory.Files.ContainsKey(fileName) ? null : directory.Files[fileName];
            return (directory, fileEntry, fileName);
        }
    }
}
