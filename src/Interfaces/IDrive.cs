using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Provides access to drives on a computer.
    /// </summary>
    public interface IDrive
    {
#if !NETSTANDARD1_3
        /// <inheritdoc cref="DriveInfo.GetDrives" />
        [NotNull]
        [ItemNotNull]
        IDriveInfo[] GetDrives();
#endif
    }
}
