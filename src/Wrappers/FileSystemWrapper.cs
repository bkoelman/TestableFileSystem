using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    public sealed class FileSystemWrapper : IFileSystem
    {
        [NotNull]
        public static readonly IFileSystem Default = new FileSystemWrapper();

        public IFile File { get; } = new FileWrapper();

        public IDirectory Directory { get; } = new DirectoryWrapper();

        public IDrive Drive { get; } = new DriveWrapper();

        public IPath Path { get; } = new PathWrapper();

        public IFileInfo ConstructFileInfo(string fileName)
        {
            return new FileInfoWrapper(new FileInfo(fileName));
        }

        public IDirectoryInfo ConstructDirectoryInfo(string path)
        {
            return new DirectoryInfoWrapper(new DirectoryInfo(path));
        }

#if !NETSTANDARD1_3
        public IDriveInfo ConstructDriveInfo(string driveName)
        {
            return new DriveInfoWrapper(new DriveInfo(driveName));
        }

        public IFileSystemWatcher ConstructFileSystemWatcher(string path = "", string filter = "*.*")
        {
            FileSystemWatcher watcher = path == string.Empty ? new FileSystemWatcher() : new FileSystemWatcher(path, filter);
            return new FileSystemWatcherWrapper(watcher);
        }
#endif

        private FileSystemWrapper()
        {
        }
    }
}
