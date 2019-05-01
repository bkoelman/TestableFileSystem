using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Represents a file system, which contains volumes, directories and files.
    /// </summary>
    [PublicAPI]
    public interface IFileSystem
    {
        /// <summary>
        /// Provides access to file-based operations on this file system.
        /// </summary>
        [NotNull]
        IFile File { get; }

        /// <summary>
        /// Provides access to directory-based operations on this file system.
        /// </summary>
        [NotNull]
        IDirectory Directory { get; }

        /// <summary>
        /// Provides access to drive-based operations on this file system.
        /// </summary>
        [NotNull]
        IDrive Drive { get; }

        /// <summary>
        /// Provides access to path-based operations on this file system.
        /// </summary>
        [NotNull]
        IPath Path { get; }

        /// <summary>
        /// Creates a new <see cref="IFileInfo" /> instance on this file system.
        /// </summary>
        [NotNull]
        IFileInfo ConstructFileInfo([NotNull] string fileName);

        /// <summary>
        /// Creates a new <see cref="IDirectoryInfo" /> instance on this file system.
        /// </summary>
        [NotNull]
        IDirectoryInfo ConstructDirectoryInfo([NotNull] string path);

#if !NETSTANDARD1_3
        /// <summary>
        /// Creates a new <see cref="IDriveInfo" /> instance on this file system.
        /// </summary>
        [NotNull]
        IDriveInfo ConstructDriveInfo([NotNull] string driveName);

        /// <summary>
        /// Creates a new <see cref="IFileSystemWatcher" /> instance on this file system.
        /// </summary>
        [NotNull]
        IFileSystemWatcher ConstructFileSystemWatcher([NotNull] string path = "", [NotNull] string filter = "*.*");
#endif
    }
}
