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

        [NotNull]
        public AbsolutePath Path { get; }

        [CanBeNull]
        public AbsolutePath PreviousPathInRename { get; }

        public FakeSystemChangeEventArgs(WatcherChangeTypes changeType, [NotNull] AbsolutePath path,
            [CanBeNull] AbsolutePath previousPathInRename)
        {
            Guard.NotNull(path, nameof(path));

            ChangeType = changeType;
            Path = path;
            PreviousPathInRename = previousPathInRename;
        }
    }
}
#endif
