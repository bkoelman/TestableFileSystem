using System.IO;
using System.Linq;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    public sealed class DriveWrapper : IDrive
    {
#if !NETSTANDARD1_3
        public IDriveInfo[] GetDrives()
        {
            return DriveInfo.GetDrives().Select(x => (IDriveInfo)new DriveInfoWrapper(x)).ToArray();
        }
#endif
    }
}
