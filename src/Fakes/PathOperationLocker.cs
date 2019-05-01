using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class PathOperationLocker<TPath> : IPath
        where TPath : class, IPath
    {
        [NotNull]
        private readonly FileSystemLock fileSystemLock;

        [NotNull]
        private readonly TPath target;

        public PathOperationLocker([NotNull] FileSystemLock fileSystemLock, [NotNull] TPath target)
        {
            Guard.NotNull(fileSystemLock, nameof(fileSystemLock));
            Guard.NotNull(target, nameof(target));

            this.fileSystemLock = fileSystemLock;
            this.target = target;
        }

        public string GetFullPath(string path)
        {
            return fileSystemLock.ExecuteInLock(() => target.GetFullPath(path));
        }

        public string GetTempPath()
        {
            return fileSystemLock.ExecuteInLock(() => target.GetTempPath());
        }

        public string GetTempFileName()
        {
            return fileSystemLock.ExecuteInLock(() => target.GetTempFileName());
        }
    }
}
