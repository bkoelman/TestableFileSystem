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
        public Func<Exception> ErrorNetworkShareNotFound { get; set; }

        [NotNull]
        public Func<string, Exception> ErrorDirectoryFoundAsFile { get; set; }

        [NotNull]
        public Func<string, Exception> ErrorDirectoryNotFound { get; set; }

        public DirectoryResolver([NotNull] DirectoryEntry root)
        {
            Guard.NotNull(root, nameof(root));
            this.root = root;

            ErrorNetworkShareNotFound = ErrorFactory.NetworkPathNotFound;
            ErrorDirectoryFoundAsFile = ErrorFactory.DirectoryNotFound;
            ErrorDirectoryNotFound = ErrorFactory.DirectoryNotFound;
        }

        [NotNull]
        public DirectoryEntry ResolveDirectory([NotNull] AbsolutePath path, [NotNull] string completePath)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(completePath, nameof(completePath));

            DirectoryEntry directory = root;

            foreach ((string name, int offset) in path.Components.Select((c, i) => (c, i)))
            {
                if (offset == 0 && !path.IsOnLocalDrive && !directory.Directories.ContainsKey(name))
                {
                    throw ErrorNetworkShareNotFound();
                }

                if (directory.Files.ContainsKey(name))
                {
                    throw ErrorDirectoryFoundAsFile(completePath);
                }

                if (!directory.Directories.ContainsKey(name))
                {
                    throw ErrorDirectoryNotFound(completePath);
                }

                directory = directory.Directories[name];
            }

            return directory;
        }
    }
}
