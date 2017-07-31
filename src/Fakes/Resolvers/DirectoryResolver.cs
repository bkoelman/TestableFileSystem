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

            foreach ((string name, int offset) in path.Components.Select((c, i) => (c, i)))
            {
                if (offset == 0 && !path.IsOnLocalDrive && !directory.Directories.ContainsKey(name))
                {
                    throw ErrorNetworkShareNotFound(incomingPath);
                }

                if (directory.Files.ContainsKey(name))
                {
                    if (offset == path.Components.Count - 1)
                    {
                        throw ErrorLastDirectoryFoundAsFile(incomingPath);
                    }

                    throw ErrorDirectoryFoundAsFile(incomingPath);
                }

                if (!directory.Directories.ContainsKey(name))
                {
                    return null;
                }

                directory = directory.Directories[name];
            }

            return directory;
        }
    }
}
