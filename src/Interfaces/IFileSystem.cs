using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    // TODO: Copy documentation comments from MSDN and add to public API surface (for convenience).

    [PublicAPI]
    public interface IFileSystem
    {
        [NotNull]
        IFile File { get; }

        [NotNull]
        IDirectory Directory { get; }

        [NotNull]
        IDrive Drive { get; }

        [NotNull]
        IPath Path { get; }

        [NotNull]
        IFileInfo ConstructFileInfo([NotNull] string fileName);

        [NotNull]
        IDirectoryInfo ConstructDirectoryInfo([NotNull] string path);

#if !NETSTANDARD1_3
        [NotNull]
        IDriveInfo ConstructDriveInfo([NotNull] string driveName);

        [NotNull]
        IFileSystemWatcher ConstructFileSystemWatcher([NotNull] string path = "", [NotNull] string filter = "*.*");
#endif
    }
}
