#if !NETSTANDARD1_3
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Provides access to information on a drive.
    /// </summary>
    public interface IDriveInfo
    {
        /// <inheritdoc cref="DriveInfo.Name" />
        [NotNull]
        string Name { get; }

        /// <inheritdoc cref="DriveInfo.IsReady" />
        bool IsReady { get; }

        /// <inheritdoc cref="DriveInfo.AvailableFreeSpace" />
        long AvailableFreeSpace { get; }

        /// <inheritdoc cref="DriveInfo.TotalFreeSpace" />
        long TotalFreeSpace { get; }

        /// <inheritdoc cref="DriveInfo.TotalSize" />
        long TotalSize { get; }

        /// <inheritdoc cref="DriveInfo.DriveType" />
        DriveType DriveType { get; }

        /// <inheritdoc cref="DriveInfo.DriveFormat" />
        [NotNull]
        string DriveFormat { get; }

        /// <inheritdoc cref="DriveInfo.VolumeLabel" />
        [NotNull]
        string VolumeLabel { get; set; }

        /// <inheritdoc cref="DriveInfo.RootDirectory" />
        [NotNull]
        IDirectoryInfo RootDirectory { get; }

        /// <inheritdoc cref="DriveInfo.ToString" />
        [NotNull]
        string ToString();
    }
}
#endif
