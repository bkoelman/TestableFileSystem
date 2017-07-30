﻿using System;
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
        public Func<string, Exception> ErrorLastDirectoryFoundAsFile { get; set; }

        [NotNull]
        public Func<string, Exception> ErrorDirectoryNotFound { get; set; }

        public DirectoryResolver([NotNull] DirectoryEntry root)
        {
            Guard.NotNull(root, nameof(root));
            this.root = root;

            ErrorNetworkShareNotFound = ErrorFactory.NetworkPathNotFound;
            ErrorDirectoryFoundAsFile = ErrorFactory.DirectoryNotFound;
            ErrorLastDirectoryFoundAsFile = ErrorFactory.DirectoryNotFound;
            ErrorDirectoryNotFound = ErrorFactory.DirectoryNotFound;
        }

        [NotNull]
        public DirectoryEntry ResolveDirectory([NotNull] AbsolutePath path, [NotNull] string incomingPath)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(incomingPath, nameof(incomingPath));

            DirectoryEntry directory = root;

            foreach ((string name, int offset) in path.Components.Select((c, i) => (c, i)))
            {
                if (offset == 0 && !path.IsOnLocalDrive && !directory.Directories.ContainsKey(name))
                {
                    throw ErrorNetworkShareNotFound();
                }

                if (directory.Files.ContainsKey(name))
                {
                    throw offset == path.Components.Count - 1
                        ? ErrorLastDirectoryFoundAsFile(incomingPath)
                        : ErrorDirectoryFoundAsFile(incomingPath);
                }

                if (!directory.Directories.ContainsKey(name))
                {
                    throw ErrorDirectoryNotFound(incomingPath);
                }

                directory = directory.Directories[name];
            }

            return directory;
        }
    }
}