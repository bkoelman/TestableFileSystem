using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class PathOperationLocker<TPath> : OperationLocker, IPath
        where TPath : class, IPath
    {
        [NotNull]
        private readonly TPath target;

        public PathOperationLocker([NotNull] FakeFileSystem owner, [NotNull] TPath target)
            : base(owner)
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
