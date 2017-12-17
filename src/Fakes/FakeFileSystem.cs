using System;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileSystem : IFileSystem
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        internal readonly object TreeLock = new object();

        [NotNull]
        internal CurrentDirectoryManager CurrentDirectoryManager { get; }

        [NotNull]
        private readonly RelativePathConverter relativePathConverter;

        [NotNull]
        internal WaitIndicator CopyWaitIndicator { get; }

        public IFile File { get; }
        public IDirectory Directory { get; }

#if !NETSTANDARD1_3
        internal event EventHandler<SystemChangeEventArgs> FileSystemChanged;
#endif

        internal FakeFileSystem([NotNull] DirectoryEntry root, [NotNull] WaitIndicator copyWaitIndicator)
        {
            Guard.NotNull(root, nameof(root));
            this.root = root;

            File = new FileOperationLocker<FakeFile>(this, new FakeFile(root, this));
            Directory = new DirectoryOperationLocker<FakeDirectory>(this, new FakeDirectory(root, this));
            CurrentDirectoryManager = new CurrentDirectoryManager(root);
            relativePathConverter = new RelativePathConverter(CurrentDirectoryManager);
            CopyWaitIndicator = copyWaitIndicator;
        }

        [NotNull]
        public FakeFileInfo ConstructFileInfo([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            AbsolutePath absolutePath = ToAbsolutePath(fileName);
            return new FakeFileInfo(root, this, absolutePath);
        }

        IFileInfo IFileSystem.ConstructFileInfo(string fileName) => ConstructFileInfo(fileName);

        [NotNull]
        public FakeDirectoryInfo ConstructDirectoryInfo([NotNull] string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = ToAbsolutePath(path);
            return ConstructDirectoryInfo(absolutePath);
        }

        [NotNull]
        internal FakeDirectoryInfo ConstructDirectoryInfo([NotNull] AbsolutePath directoryPath)
        {
            Guard.NotNull(directoryPath, nameof(directoryPath));

            return new FakeDirectoryInfo(root, this, directoryPath);
        }

        IDirectoryInfo IFileSystem.ConstructDirectoryInfo(string path) => ConstructDirectoryInfo(path);

        [NotNull]
        internal AbsolutePath ToAbsolutePath([NotNull] string path)
        {
            return relativePathConverter.ToAbsolutePath(path);
        }

#if !NETSTANDARD1_3
        [NotNull]
        public FakeFileSystemWatcher ConstructFileSystemWatcher([NotNull] string path = "", [NotNull] string filter = "*.*")
        {
            Guard.NotNull(filter, nameof(filter));
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = path.Length > 0 ? ToAbsolutePath(path) : null;
            return new FakeFileSystemWatcher(this, absolutePath, filter);
        }

        IFileSystemWatcher IFileSystem.ConstructFileSystemWatcher(string path, string filter) =>
            ConstructFileSystemWatcher(path, filter);

        internal void NotifyFileSystemChange([NotNull] SystemChangeEventArgs args)
        {
            Guard.NotNull(args, nameof(args));

            FileSystemChanged?.Invoke(this, args);
        }
#endif
    }
}
