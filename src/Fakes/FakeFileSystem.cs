﻿using System.Collections.Generic;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileSystem : IFileSystem
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        private readonly IDictionary<string, FakeVolume> volumes;

        [NotNull]
        internal readonly object TreeLock = new object();

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

        [NotNull]
        internal FakeFileSystemChangeTracker ChangeTracker { get; }

        internal FakeFileSystem([NotNull] DirectoryEntry root, [NotNull] IDictionary<string, FakeVolume> volumes,
            [NotNull] FakeFileSystemChangeTracker changeTracker, [NotNull] string tempDirectory,
            [NotNull] WaitIndicator copyWaitIndicator)
        {
            Guard.NotNull(root, nameof(root));
            Guard.NotNull(changeTracker, nameof(changeTracker));
            Guard.NotNull(copyWaitIndicator, nameof(copyWaitIndicator));

            this.root = root;
            this.volumes = volumes;
            ChangeTracker = changeTracker;
            TempDirectory = tempDirectory;
            CopyWaitIndicator = copyWaitIndicator;

            File = new FileOperationLocker<FakeFile>(TreeLock, new FakeFile(root, this));
            Directory = new DirectoryOperationLocker<FakeDirectory>(TreeLock, new FakeDirectory(root, this));
            Drive = new DriveOperationLocker<FakeDrive>(TreeLock, new FakeDrive(root, this));
            Path = new PathOperationLocker<FakePath>(TreeLock, new FakePath(root, this));
            CurrentDirectoryManager = new CurrentDirectoryManager(root);
            relativePathConverter = new RelativePathConverter(CurrentDirectoryManager);
        }

        [NotNull]
        public FakeFileInfo ConstructFileInfo([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            AbsolutePath absolutePath = ToAbsolutePathInLock(fileName);
            return new FakeFileInfo(root, this, absolutePath);
        }

        IFileInfo IFileSystem.ConstructFileInfo(string fileName) => ConstructFileInfo(fileName);

        [NotNull]
        public FakeDirectoryInfo ConstructDirectoryInfo([NotNull] string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = ToAbsolutePathInLock(path);
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
        internal AbsolutePath ToAbsolutePathInLock([NotNull] string path)
        {
            lock (TreeLock)
            {
                return ToAbsolutePath(path);
            }
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

            return new FakeDriveInfo(this, driveName);
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

            return new FakeFileSystemWatcher(this, ChangeTracker, absolutePath, filter);
        }

        private static bool IsDefaultInvocation([NotNull] string path, [NotNull] string filter)
        {
            return path == "" && filter == "*.*";
        }

        IFileSystemWatcher IFileSystem.ConstructFileSystemWatcher(string path, string filter) =>
            ConstructFileSystemWatcher(path, filter);
#endif

        [CanBeNull]
        internal FakeVolume GetVolume([NotNull] string driveName)
        {
            return volumes.ContainsKey(driveName) ? volumes[driveName] : null;
        }
    }
}
