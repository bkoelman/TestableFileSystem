using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed partial class FakeFileSystemChangeTracker
    {
        public void NotifyFileCreated([NotNull] IPathFormatter formatter)
        {
            ProcessFileCreated(formatter);
        }

        partial void ProcessFileCreated([NotNull] IPathFormatter formatter);

        public void NotifyFileDeleted([NotNull] IPathFormatter formatter)
        {
            ProcessFileDeleted(formatter);
        }

        partial void ProcessFileDeleted([NotNull] IPathFormatter formatter);

        public void NotifyFileChanged([NotNull] IPathFormatter formatter)
        {
            ProcessFileChanged(formatter);
        }

        partial void ProcessFileChanged([NotNull] IPathFormatter formatter);
    }

    internal sealed partial class FakeFileSystemChangeTracker
    {
#if !NETSTANDARD1_3
        public event EventHandler<FakeSystemChangeEventArgs> FileSystemChanged;

        partial void ProcessFileCreated(IPathFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            var args = new FakeSystemChangeEventArgs(WatcherChangeTypes.Created, formatter, null);
            OnFileSystemChanged(args);
        }

        partial void ProcessFileDeleted(IPathFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            var args = new FakeSystemChangeEventArgs(WatcherChangeTypes.Deleted, formatter, null);
            OnFileSystemChanged(args);
        }

        partial void ProcessFileChanged(IPathFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            var args = new FakeSystemChangeEventArgs(WatcherChangeTypes.Changed, formatter, null);
            OnFileSystemChanged(args);
        }

        private void OnFileSystemChanged([NotNull] FakeSystemChangeEventArgs args)
        {
            FileSystemChanged?.Invoke(this, args);
        }
#endif
    }
}
