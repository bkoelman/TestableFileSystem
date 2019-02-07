#if !NETSTANDARD1_3
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    public interface IDriveInfo
    {
        [NotNull]
        string Name { get; }

        bool IsReady { get; }

        long AvailableFreeSpace { get; }
        long TotalFreeSpace { get; }
        long TotalSize { get; }

        DriveType DriveType { get; }

        [NotNull]
        string DriveFormat { get; }

        [NotNull]
        string VolumeLabel { get; set; }

        [NotNull]
        IDirectoryInfo RootDirectory { get; }

        [NotNull]
        string ToString();

        // TODO: Provide a way to retrieve all drives in non-static context.
        // Maybe throw with a message that describes where/how to obtain this data.
        // public static DriveInfo[] GetDrives()
    }
}
#endif
