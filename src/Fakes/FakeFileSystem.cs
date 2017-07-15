using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileSystem : IFileSystem
    {
        [NotNull]
        public FileOperationLocker<FakeFile> File { get; }

        IFile IFileSystem.File => File;

        [NotNull]
        public DirectoryOperationLocker<FakeDirectory> Directory { get; }

        IDirectory IFileSystem.Directory => Directory;

        [NotNull]
        internal readonly object TreeLock = new object();

        [NotNull]
        internal CurrentDirectoryManager CurrentDirectory { get; }

        internal FakeFileSystem([NotNull] DirectoryEntry rootEntry)
        {
            Guard.NotNull(rootEntry, nameof(rootEntry));

            CurrentDirectory = new CurrentDirectoryManager(rootEntry);
            Directory = new DirectoryOperationLocker<FakeDirectory>(this, new FakeDirectory(rootEntry, this));
            File = new FileOperationLocker<FakeFile>(this, new FakeFile(rootEntry, this));
        }

        [NotNull]
        public FakeFileInfo ConstructFileInfo([NotNull] string fileName)
        {
            AbsolutePath absolutePath = ToAbsolutePath(fileName);
            return new FakeFileInfo(this, absolutePath.GetText());
        }

        IFileInfo IFileSystem.ConstructFileInfo(string fileName) => ConstructFileInfo(fileName);

        [NotNull]
        public FakeDirectoryInfo ConstructDirectoryInfo([NotNull] string path)
        {
            AbsolutePath absolutePath = ToAbsolutePath(path);
            return new FakeDirectoryInfo(this, absolutePath.GetText());
        }

        IDirectoryInfo IFileSystem.ConstructDirectoryInfo(string path) => ConstructDirectoryInfo(path);

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

            string rooted = Path.Combine(basePath, path);
            return new AbsolutePath(rooted);
        }

        internal long GetFileSize([NotNull] string path)
        {
            return File.ExecuteOnFile(f => f.GetSize(path));
        }
    }
}
