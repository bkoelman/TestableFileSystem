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

        // TODO: Add replacement for System.IO.Path.GetTempFileName() - added in NetStandard 1.3
        // TODO: Update analyzer for proper redirection of GetTempFileName().

#if !NETSTANDARD1_3
        [NotNull]
        IDriveInfo ConstructDriveInfo([NotNull] string driveName);

        [NotNull]
        [ItemNotNull]
        IDriveInfo[] GetDrives();

        [NotNull]
        IFileSystemWatcher ConstructFileSystemWatcher([NotNull] string path = "", [NotNull] string filter = "*.*");
#endif
    }
}
