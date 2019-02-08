using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    public sealed class FileSystemWrapper : IFileSystem
    {
        [NotNull]
        public static readonly IFileSystem Default = new FileSystemWrapper();

        public IFile File => new FileWrapper();

        public IDirectory Directory => new DirectoryWrapper();

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

        public IDriveInfo[] GetDrives()
        {
            return DriveInfo.GetDrives().Select(x => (IDriveInfo)new DriveInfoWrapper(x)).ToArray();
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
