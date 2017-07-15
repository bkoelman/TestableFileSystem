using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileSystem : IFileSystem
    {
        [NotNull]
        private readonly FileOperationLocker<FakeFile> innerFile;

        public IDirectory Directory { get; }
        public IFile File => innerFile;

        [NotNull]
        internal readonly object TreeLock = new object();

        [NotNull]
        internal CurrentDirectoryManager CurrentDirectory { get; }

        public FakeFileSystem([NotNull] DirectoryEntry rootEntry)
        {
            Guard.NotNull(rootEntry, nameof(rootEntry));

            CurrentDirectory = new CurrentDirectoryManager(rootEntry);
            Directory = new DirectoryOperationLocker(this, new FakeDirectory(rootEntry, this));
            innerFile = new FileOperationLocker<FakeFile>(this, new FakeFile(rootEntry, this));
        }

        public IFileInfo ConstructFileInfo(string fileName)
        {
            return new FakeFileInfo(this, fileName);
        }

        public IDirectoryInfo ConstructDirectoryInfo(string path)
        {
            return new FakeDirectoryInfo(this, path);
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

        internal long GetFileSize([NotNull] string path)
        {
            return innerFile.ExecuteOnFile(f => f.GetSize(path));
        }
    }
}
