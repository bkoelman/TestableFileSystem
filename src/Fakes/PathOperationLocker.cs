using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class PathOperationLocker<TPath> : OperationLocker, IPath
        where TPath : class, IPath
    {
        [NotNull]
        private readonly TPath target;

        public PathOperationLocker([NotNull] object treeLock, [NotNull] TPath target)
            : base(treeLock)
        {
            Guard.NotNull(target, nameof(target));
            this.target = target;
        }

        public string GetTempPath()
        {
            return ExecuteInLock(() => target.GetTempPath());
        }

        public string GetTempFileName()
        {
            return ExecuteInLock(() => target.GetTempFileName());
        }
    }
}
