using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileSystem : IFileSystem
    {
        [NotNull]
        private readonly DirectoryEntry root;

        public IDirectory Directory { get; }

        public IFile File { get; }

        [NotNull]
        internal readonly object TreeLock = new object();

        [NotNull]
        internal DirectoryEntry CurrentDirectory { get; set; }

        public FakeFileSystem([NotNull] DirectoryEntry rootEntry)
        {
            Guard.NotNull(rootEntry, nameof(rootEntry));
            AssertIsTopLevel(rootEntry);
            AssertHasDrives(rootEntry);

            root = rootEntry;
            Directory = new DirectoryOperationLocker(this, new FakeDirectory(rootEntry, this));
            File = new FileOperationLocker(this, new FakeFile(rootEntry, this));
            CurrentDirectory = GetEntryToFirstDriveLetter(rootEntry);
        }

        [NotNull]
        private static DirectoryEntry GetEntryToFirstDriveLetter([NotNull] DirectoryEntry rootEntry)
        {
            return rootEntry.Directories.First(x => x.Key.IndexOf(Path.VolumeSeparatorChar) != -1).Value;
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

        public IFileInfo ConstructFileInfo(string fileName)
        {
            throw new NotImplementedException();
        }

        public IDirectoryInfo ConstructDirectoryInfo(string path)
        {
            throw new NotImplementedException();
        }

        [NotNull]
        internal AbsolutePath ToAbsolutePath([NotNull] string path)
        {
            Guard.NotNull(path, nameof(path));

            string basePath = CurrentDirectory.GetAbsolutePath();
            string rooted = Path.Combine(basePath, path);
            return new AbsolutePath(rooted);
        }
    }
}
