using System;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Resolvers
{
    internal sealed class EntryResolver
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        private readonly DirectoryResolver directoryResolver;

        [NotNull]
        public Func<string, Exception> ErrorNetworkShareNotFound
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
        public Func<string, Exception> ErrorDirectoryNotFound
        {
            get => directoryResolver.ErrorDirectoryNotFound;
            set => directoryResolver.ErrorDirectoryNotFound = value;
        }

        public EntryResolver([NotNull] DirectoryEntry root)
        {
            Guard.NotNull(root, nameof(root));

            this.root = root;
            directoryResolver = new DirectoryResolver(root);
        }

        [CanBeNull]
        public BaseEntry SafeResolveEntry([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            DirectoryEntry directory = SafeResolveContainingDirectory(path);
            if (directory != null)
            {
                string entryName = path.Components.Last();

                if (directory.ContainsFile(entryName))
                {
                    return directory.Files[entryName];
                }

                if (directory.ContainsDirectory(entryName))
                {
                    return directory.Directories[entryName];
                }
            }

            return null;
        }

        [NotNull]
        public BaseEntry ResolveEntry([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            DirectoryEntry directory = ResolveContainingDirectory(path);
            string entryName = path.Components.Last();

            if (directory.ContainsFile(entryName))
            {
                return directory.Files[entryName];
            }

            if (directory.ContainsDirectory(entryName))
            {
                return directory.Directories[entryName];
            }

            throw ErrorFactory.System.FileNotFound(path.GetText());
        }

        [CanBeNull]
        private DirectoryEntry SafeResolveContainingDirectory([NotNull] AbsolutePath path)
        {
            AbsolutePath parentPath = path.TryGetParentPath();
            return parentPath == null ? root : directoryResolver.SafeResolveDirectory(parentPath, path.GetText());
        }

        [NotNull]
        private DirectoryEntry ResolveContainingDirectory([NotNull] AbsolutePath path)
        {
            AbsolutePath parentPath = path.TryGetParentPath();
            return parentPath == null ? root : directoryResolver.ResolveDirectory(parentPath, path.GetText());
        }
    }
}
