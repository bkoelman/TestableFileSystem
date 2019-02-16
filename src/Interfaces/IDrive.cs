using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    public interface IDrive
    {
#if !NETSTANDARD1_3
        [NotNull]
        [ItemNotNull]
        IDriveInfo[] GetDrives();
#endif
    }
}
