using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class DriveOperationLocker<TDrive> : OperationLocker, IDrive
        where TDrive : class, IDrive
    {
        [NotNull]
        private readonly TDrive target;

        public DriveOperationLocker([NotNull] FakeFileSystem owner, [NotNull] TDrive target)
            : base(owner)
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
