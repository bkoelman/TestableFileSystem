using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileSystem : IFileSystem
    {
        public IDirectory Directory { get; }
        public IFile File { get; }

        [NotNull]
        internal readonly object TreeLock = new object();

        [NotNull]
        internal CurrentDirectoryManager CurrentDirectory { get; }

        public FakeFileSystem([NotNull] DirectoryEntry rootEntry)
        {
            Guard.NotNull(rootEntry, nameof(rootEntry));

            CurrentDirectory = new CurrentDirectoryManager(rootEntry);
            Directory = new DirectoryOperationLocker(this, new FakeDirectory(rootEntry, this));
            File = new FileOperationLocker(this, new FakeFile(rootEntry, this));
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
            // TODO: How to handle when caller passes a path like "e:file.txt"?

            if (string.IsNullOrWhiteSpace(path))
            {
                throw ErrorFactory.PathIsNotLegal(nameof(path));
            }

            DirectoryEntry baseDirectory = CurrentDirectory.GetValue();
            string basePath = baseDirectory.GetAbsolutePath();

            string rooted = Path.Combine(basePath, path.TrimEnd());
            return new AbsolutePath(rooted);
        }
    }
}
