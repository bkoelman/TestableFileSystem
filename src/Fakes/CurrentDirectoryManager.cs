using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class CurrentDirectoryManager
    {
        [NotNull]
        private readonly VolumeContainer container;

        [NotNull]
        private AbsolutePath path;

        public CurrentDirectoryManager([NotNull] VolumeContainer container)
        {
            Guard.NotNull(container, nameof(container));
            this.container = container;

            ICollection<VolumeEntry> drives = container.FilterDrives();
            AssertFileSystemContainsDrives(drives);

            VolumeEntry drive = drives.First();
            path = new AbsolutePath(drive.Name);
        }

        [AssertionMethod]
        private static void AssertFileSystemContainsDrives([NotNull] [ItemNotNull] IEnumerable<VolumeEntry> drives)
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
            var resolver = new DirectoryResolver(container);
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
