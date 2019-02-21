using System;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

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

            ErrorNetworkShareNotFound = _ => ErrorFactory.System.NetworkPathNotFound();
            ErrorDirectoryFoundAsFile = ErrorFactory.System.DirectoryNotFound;
            ErrorLastDirectoryFoundAsFile = ErrorFactory.System.DirectoryNotFound;
            ErrorDirectoryNotFound = ErrorFactory.System.DirectoryNotFound;
        }

        [NotNull]
        public DirectoryEntry ResolveDirectory([NotNull] AbsolutePath path, [CanBeNull] string incomingPath = null)
        {
            Guard.NotNull(path, nameof(path));

            string completePath = incomingPath ?? path.GetText();

            DirectoryEntry directory = TryResolveDirectory(path, completePath);
            if (directory == null)
            {
                throw ErrorDirectoryNotFound(completePath);
            }

            return directory;
        }

        [CanBeNull]
        public DirectoryEntry TryResolveDirectory([NotNull] AbsolutePath path, [CanBeNull] string incomingPath = null)
        {
            Guard.NotNull(path, nameof(path));

            string completePath = incomingPath ?? path.GetText();
            AssertNetworkShareExists(path, completePath);

            DirectoryEntry directory = root;

            foreach (AbsolutePathComponent component in path.EnumerateComponents())
            {
                AssertIsNotFile(component, directory, completePath);

                if (!directory.ContainsDirectory(component.Name))
                {
                    return null;
                }

                directory = directory.GetDirectory(component.Name);
            }

            return directory;
        }

        [CanBeNull]
        public DirectoryEntry SafeResolveDirectory([NotNull] AbsolutePath path, [CanBeNull] string incomingPath = null)
        {
            Guard.NotNull(path, nameof(path));

            string completePath = incomingPath ?? path.GetText();
            AssertNetworkShareExists(path, completePath);

            DirectoryEntry directory = root;

            foreach (AbsolutePathComponent component in path.EnumerateComponents())
            {
                if (!directory.ContainsDirectory(component.Name))
                {
                    return null;
                }

                directory = directory.GetDirectory(component.Name);
            }

            return directory;
        }

        [AssertionMethod]
        private void AssertNetworkShareExists([NotNull] AbsolutePath path, [NotNull] string incomingPath)
        {
            if (IsNetworkShareMissing(path))
            {
                throw ErrorNetworkShareNotFound(incomingPath);
            }
        }

        private bool IsNetworkShareMissing([NotNull] AbsolutePath path)
        {
            return !path.IsOnLocalDrive && !root.ContainsDirectory(path.Components.First());
        }

        [AssertionMethod]
        private void AssertIsNotFile([NotNull] AbsolutePathComponent component, [NotNull] DirectoryEntry directory,
            [NotNull] string incomingPath)
        {
            if (directory.ContainsFile(component.Name))
            {
                if (component.IsAtEnd)
                {
                    throw ErrorLastDirectoryFoundAsFile(incomingPath);
                }

                throw ErrorDirectoryFoundAsFile(incomingPath);
            }
        }
    }
}
