using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileSystem : IFileSystem
    {
        [NotNull]
        private readonly VolumeContainer container;

        [NotNull]
        internal CurrentDirectoryManager CurrentDirectoryManager { get; }

        [NotNull]
        private readonly RelativePathConverter relativePathConverter;

        [NotNull]
        internal string TempDirectory { get; private set; }

        [NotNull]
        internal WaitIndicator CopyWaitIndicator { get; }

        public IFile File { get; }
        public IDirectory Directory { get; }
        public IDrive Drive { get; }
        public IPath Path { get; }

        internal FakeFileSystem([NotNull] VolumeContainer container, [NotNull] string tempDirectory,
            [NotNull] WaitIndicator copyWaitIndicator)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNull(copyWaitIndicator, nameof(copyWaitIndicator));

            this.container = container;
            TempDirectory = tempDirectory;
            CopyWaitIndicator = copyWaitIndicator;

            File = new FileOperationLocker<FakeFile>(container.FileSystemLock, new FakeFile(container, this));
            Directory = new DirectoryOperationLocker<FakeDirectory>(container.FileSystemLock, new FakeDirectory(container, this));
            Drive = new DriveOperationLocker<FakeDrive>(container.FileSystemLock, new FakeDrive(container, this));
            Path = new PathOperationLocker<FakePath>(container.FileSystemLock, new FakePath(container, this));
            CurrentDirectoryManager = new CurrentDirectoryManager(container);
            relativePathConverter = new RelativePathConverter(CurrentDirectoryManager);
        }

        [NotNull]
        public FakeFileInfo ConstructFileInfo([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            AbsolutePath absolutePath = ToAbsolutePathInLock(fileName);
            return new FakeFileInfo(container, this, absolutePath, fileName);
        }

        IFileInfo IFileSystem.ConstructFileInfo(string fileName) => ConstructFileInfo(fileName);

        [NotNull]
        public FakeDirectoryInfo ConstructDirectoryInfo([NotNull] string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = ToAbsolutePathInLock(path);
            return new FakeDirectoryInfo(container, this, absolutePath, path);
        }

        [NotNull]
        internal FakeDirectoryInfo ConstructDirectoryInfo([NotNull] AbsolutePath path, [CanBeNull] string displayPath = null)
        {
            Guard.NotNull(path, nameof(path));

            return new FakeDirectoryInfo(container, this, path, displayPath);
        }

        IDirectoryInfo IFileSystem.ConstructDirectoryInfo(string path) => ConstructDirectoryInfo(path);

        [NotNull]
        internal AbsolutePath ToAbsolutePathInLock([NotNull] string path)
        {
            return container.FileSystemLock.ExecuteInLock(() => ToAbsolutePath(path));
        }

        [NotNull]
        internal AbsolutePath ToAbsolutePath([NotNull] string path)
        {
            return relativePathConverter.ToAbsolutePath(path);
        }

#if !NETSTANDARD1_3
        public IDriveInfo ConstructDriveInfo(string driveName)
        {
            Guard.NotNull(driveName, nameof(driveName));

            return new FakeDriveInfo(container, this, driveName);
        }

        [NotNull]
        public FakeFileSystemWatcher ConstructFileSystemWatcher([NotNull] string path = "", [NotNull] string filter = "*.*")
        {
            Guard.NotNull(filter, nameof(filter));
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = null;
            if (!IsDefaultInvocation(path, filter))
            {
                if (!Directory.Exists(path))
                {
                    throw ErrorFactory.System.DirectoryNameIsInvalid(path);
                }

                absolutePath = ToAbsolutePathInLock(path);
            }

            return new FakeFileSystemWatcher(this, container.ChangeTracker, absolutePath, filter);
        }

        private static bool IsDefaultInvocation([NotNull] string path, [NotNull] string filter)
        {
            return path == "" && filter == "*.*";
        }

        IFileSystemWatcher IFileSystem.ConstructFileSystemWatcher(string path, string filter) =>
            ConstructFileSystemWatcher(path, filter);
#endif
    }
}
