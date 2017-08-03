using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class CurrentDirectoryManager
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        private AbsolutePath path;

        public CurrentDirectoryManager([NotNull] DirectoryEntry root)
        {
            Guard.NotNull(root, nameof(root));
            this.root = root;

            ICollection<DirectoryEntry> drives = root.FilterDrives();
            AssertFileSystemContainsDrives(drives);

            DirectoryEntry drive = drives.First();
            path = new AbsolutePath(drive.Name);
        }

        [AssertionMethod]
        private static void AssertFileSystemContainsDrives([NotNull] [ItemNotNull] IEnumerable<DirectoryEntry> drives)
        {
            if (!drives.Any())
            {
                throw new InvalidOperationException("System contains no drives.");
            }
        }

        [NotNull]
        public AbsolutePath GetValue()
        {
            return path;
        }

        public void SetValue([NotNull] AbsolutePath directoryPath)
        {
            Guard.NotNull(directoryPath, nameof(directoryPath));

            path = directoryPath;
        }

        public bool IsAtOrAboveCurrentDirectory([NotNull] DirectoryEntry directory)
        {
            var resolver = new DirectoryResolver(root);
            DirectoryEntry current = resolver.ResolveDirectory(path);

            do
            {
                if (directory == current)
                {
                    return true;
                }

                current = current.Parent;
            }
            while (current != null);

            return false;
        }
    }
}
