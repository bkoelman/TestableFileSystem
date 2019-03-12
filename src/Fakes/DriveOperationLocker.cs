using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class DriveOperationLocker<TDrive> : IDrive
        where TDrive : class, IDrive
    {
        [NotNull]
        private readonly FileSystemLock fileSystemLock;

        [NotNull]
        private readonly TDrive target;

        public DriveOperationLocker([NotNull] FileSystemLock fileSystemLock, [NotNull] TDrive target)
        {
            Guard.NotNull(fileSystemLock, nameof(fileSystemLock));
            Guard.NotNull(target, nameof(target));

            this.fileSystemLock = fileSystemLock;
            this.target = target;
        }

#if !NETSTANDARD1_3
        public IDriveInfo[] GetDrives()
        {
            return fileSystemLock.ExecuteInLock(() => target.GetDrives());
        }
#endif
    }
}
