using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class DriveOperationLocker<TDrive> : OperationLocker, IDrive
        where TDrive : class, IDrive
    {
        [NotNull]
        private readonly TDrive target;

        public DriveOperationLocker([NotNull] object treeLock, [NotNull] TDrive target)
            : base(treeLock)
        {
            Guard.NotNull(target, nameof(target));
            this.target = target;
        }

#if !NETSTANDARD1_3
        public IDriveInfo[] GetDrives()
        {
            return ExecuteInLock(() => target.GetDrives());
        }
#endif
    }
}
