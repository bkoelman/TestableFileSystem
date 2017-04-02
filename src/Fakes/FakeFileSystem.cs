using System;
using System.Collections.Generic;
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

        // TODO: Keep track of current directory per drive, to allow paths like "C:work\doc.txt" to resolve properly.
        [NotNull]
        internal DirectoryEntry CurrentDirectory { get; set; }

        public FakeFileSystem([NotNull] DirectoryEntry rootEntry)
        {
            Guard.NotNull(rootEntry, nameof(rootEntry));
            AssertIsTopLevel(rootEntry);

            ICollection<DirectoryEntry> drives = rootEntry.GetDrives();
            if (!drives.Any())
            {
                throw new InvalidOperationException("System contains no drives.");
            }

            root = rootEntry;
            Directory = new DirectoryOperationLocker(this, new FakeDirectory(rootEntry, this));
            File = new FileOperationLocker(this, new FakeFile(rootEntry, this));
            CurrentDirectory = drives.First();
        }

        [AssertionMethod]
        private void AssertIsTopLevel([NotNull] DirectoryEntry rootEntry)
        {
            if (rootEntry.Parent != null)
            {
                throw new ArgumentException(nameof(rootEntry));
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
