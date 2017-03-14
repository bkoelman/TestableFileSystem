using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class MemoryFileSystem : IFileSystem
    {
        [NotNull]
        private readonly DirectoryEntry root;

        public IDirectory Directory => new MemoryDirectory(root, this);

        public IFile File => new MemoryFile(root, this);

        [NotNull]
        internal string CurrentDirectory { get; set; }

        public MemoryFileSystem()
            : this(DirectoryEntry.CreateRoot())
        {
        }

        public MemoryFileSystem([NotNull] DirectoryEntry rootEntry)
        {
            Guard.NotNull(rootEntry, nameof(rootEntry));
            AssertIsTopLevel(rootEntry);
            AssertHasDrives(rootEntry);

            root = rootEntry;
            CurrentDirectory = GetPathToFirstDriveLetter(rootEntry);
        }

        [NotNull]
        private static string GetPathToFirstDriveLetter([NotNull] DirectoryEntry rootEntry)
        {
            return rootEntry.Directories.First(x => x.Key.IndexOf(Path.VolumeSeparatorChar) != -1).Key + Path.DirectorySeparatorChar;
        }

        [AssertionMethod]
        private void AssertIsTopLevel([NotNull] DirectoryEntry rootEntry)
        {
            if (rootEntry.Parent != null)
            {
                throw new ArgumentException(nameof(rootEntry));
            }
        }

        [AssertionMethod]
        private void AssertHasDrives([NotNull] DirectoryEntry rootEntry)
        {
            if (rootEntry.Directories.All(x => x.Key.IndexOf(Path.VolumeSeparatorChar) == -1))
            {
                throw new InvalidOperationException("System contains no drives.");
            }
        }
    }
}
