using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class CurrentDirectoryManager
    {
        [NotNull]
        private DirectoryEntry directory;

        public CurrentDirectoryManager([NotNull] DirectoryEntry rootEntry)
        {
            Guard.NotNull(rootEntry, nameof(rootEntry));
            AssertIsTopLevel(rootEntry);

            ICollection<DirectoryEntry> drives = rootEntry.FilterDrives();
            AssertFileSystemContainsVolumes(drives);

            directory = drives.First();
        }

        [AssertionMethod]
        private void AssertIsTopLevel([NotNull] DirectoryEntry rootEntry)
        {
            if (rootEntry.Parent != null)
            {
                throw new ArgumentException("Expected root of filesystem.", nameof(rootEntry));
            }
        }

        [AssertionMethod]
        private static void AssertFileSystemContainsVolumes([NotNull] [ItemNotNull] IEnumerable<DirectoryEntry> drives)
        {
            if (!drives.Any())
            {
                throw new InvalidOperationException("System contains no drives.");
            }
        }

        [NotNull]
        public DirectoryEntry GetValue()
        {
            return directory;
        }

        public void SetValue([NotNull] DirectoryEntry entry)
        {
            Guard.NotNull(entry, nameof(entry));

            directory = entry;
        }

        public bool IsAtOrAboveCurrentDirectory([NotNull] DirectoryEntry entry)
        {
            DirectoryEntry current = directory;
            while (current != null)
            {
                if (current == entry)
                {
                    return true;
                }

                current = current.Parent;
            }

            return false;
        }
    }
}
