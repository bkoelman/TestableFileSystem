using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    [PublicAPI]
    public interface IFileSystem
    {
        [NotNull]
        IFile File { get; }

        [NotNull]
        IDirectory Directory { get; }

        [NotNull]
        IFileInfo ConstructFileInfo([NotNull] string fileName);

        [NotNull]
        IDirectoryInfo ConstructDirectoryInfo([NotNull] string path);

#if !NETSTANDARD1_3
        [NotNull]
        IDriveInfo ConstructDriveInfo([NotNull] string driveName);

        // TODO: Update analyzer to redirect from static DriveInfo.GetDrives() to here.
        [NotNull]
        [ItemNotNull]
        IDriveInfo[] GetDrives();

        [NotNull]
        IFileSystemWatcher ConstructFileSystemWatcher([NotNull] string path = "", [NotNull] string filter = "*.*");
#endif
    }
}
