using System;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Resolvers
{
    internal sealed class DirectoryResolver
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        public Func<string, Exception> ErrorNetworkShareNotFound { get; set; }

        [NotNull]
        public Func<string, Exception> ErrorDirectoryFoundAsFile { get; set; }

        [NotNull]
        public Func<string, Exception> ErrorLastDirectoryFoundAsFile { get; set; }

        [NotNull]
        public Func<string, Exception> ErrorDirectoryNotFound { get; set; }

        public DirectoryResolver([NotNull] DirectoryEntry root)
        {
            Guard.NotNull(root, nameof(root));
            this.root = root;

            ErrorNetworkShareNotFound = _ => ErrorFactory.NetworkPathNotFound();
            ErrorDirectoryFoundAsFile = ErrorFactory.DirectoryNotFound;
            ErrorLastDirectoryFoundAsFile = ErrorFactory.DirectoryNotFound;
            ErrorDirectoryNotFound = ErrorFactory.DirectoryNotFound;
        }

        [NotNull]
        public DirectoryEntry ResolveDirectory([NotNull] AbsolutePath path, [NotNull] string incomingPath)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(incomingPath, nameof(incomingPath));

            DirectoryEntry directory = TryResolveDirectory(path, incomingPath);
            if (directory == null)
            {
                throw ErrorDirectoryNotFound(incomingPath);
            }

            return directory;
        }

        [CanBeNull]
        public DirectoryEntry TryResolveDirectory([NotNull] AbsolutePath path, [NotNull] string incomingPath)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(incomingPath, nameof(incomingPath));

            DirectoryEntry directory = root;

            foreach (AbsolutePathComponent component in path.EnumerateComponents())
            {
                if (component.IsAtStart && !path.IsOnLocalDrive && !directory.Directories.ContainsKey(component.Name))
                {
                    throw ErrorNetworkShareNotFound(incomingPath);
                }

                if (directory.Files.ContainsKey(component.Name))
                {
                    if (component.IsAtEnd)
                    {
                        throw ErrorLastDirectoryFoundAsFile(incomingPath);
                    }

                    throw ErrorDirectoryFoundAsFile(incomingPath);
                }

                if (!directory.Directories.ContainsKey(component.Name))
                {
                    return null;
                }

                directory = directory.Directories[component.Name];
            }

            return directory;
        }
    }

}
