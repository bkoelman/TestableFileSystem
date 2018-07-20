#if !NETSTANDARD1_3
using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class FakeSystemChangeEventArgs : EventArgs
    {
        public WatcherChangeTypes ChangeType { get; }

        public NotifyFilters Filters { get; }

        [NotNull]
        public IPathFormatter PathFormatter { get; }

        [CanBeNull]
        public IPathFormatter PreviousPathInRenameFormatter { get; }

        public FakeSystemChangeEventArgs(WatcherChangeTypes changeType, NotifyFilters filters,
            [NotNull] IPathFormatter pathFormatter, [CanBeNull] IPathFormatter previousPathInRenameFormatter)
        {
            Guard.NotNull(pathFormatter, nameof(pathFormatter));

            ChangeType = changeType;
            Filters = filters;
            PathFormatter = pathFormatter;
            PreviousPathInRenameFormatter = previousPathInRenameFormatter;
        }
    }
}
#endif
