using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

// TODO: Remove this after converting all specs.
[assembly: InternalsVisibleTo("TestableFileSystem.Fakes.Tests")]

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
        internal CurrentDirectoryManager CurrentDirectoryManager { get; }

        [NotNull]
        private readonly RelativePathConverter relativePathConverter;

        [NotNull]
        internal WaitIndicator CopyWaitIndicator { get; }

        internal FakeFileSystem([NotNull] DirectoryEntry root, [NotNull] WaitIndicator copyWaitIndicator)
        {
            Guard.NotNull(root, nameof(root));

            File = new FileOperationLocker<FakeFile>(this, new FakeFile(root, this));
            Directory = new DirectoryOperationLocker<FakeDirectory>(this, new FakeDirectory(root, this));
            CurrentDirectoryManager = new CurrentDirectoryManager(root);
            relativePathConverter = new RelativePathConverter(CurrentDirectoryManager);
            CopyWaitIndicator = copyWaitIndicator;
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
            return relativePathConverter.ToAbsolutePath(path);
        }

        internal long GetFileSize([NotNull] string path)
        {
            return File.ExecuteOnFile(f => f.GetSize(path));
        }
    }
}
