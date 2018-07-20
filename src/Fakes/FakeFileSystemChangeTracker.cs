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

        public void NotifyFileContentsChanged([NotNull] IPathFormatter formatter, bool hasSizeChanged)
        {
            ProcessFileContentsChanged(formatter, hasSizeChanged);
        }

        partial void ProcessFileContentsChanged([NotNull] IPathFormatter formatter, bool hasSizeChanged);
    }

    internal sealed partial class FakeFileSystemChangeTracker
    {
#if !NETSTANDARD1_3
        public event EventHandler<FakeSystemChangeEventArgs> FileSystemChanged;

        partial void ProcessFileCreated(IPathFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            var args = new FakeSystemChangeEventArgs(WatcherChangeTypes.Created, NotifyFilters.FileName, formatter, null);
            OnFileSystemChanged(args);
        }

        partial void ProcessFileDeleted(IPathFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            var args = new FakeSystemChangeEventArgs(WatcherChangeTypes.Deleted, NotifyFilters.FileName, formatter, null);
            OnFileSystemChanged(args);
        }

        partial void ProcessFileContentsChanged(IPathFormatter formatter, bool hasSizeChanged)
        {
            Guard.NotNull(formatter, nameof(formatter));

            NotifyFilters filters = NotifyFilters.LastWrite | NotifyFilters.LastAccess;
            if (hasSizeChanged)
            {
                filters |= NotifyFilters.Size;
            }

            var args = new FakeSystemChangeEventArgs(WatcherChangeTypes.Changed, filters, formatter, null);
            OnFileSystemChanged(args);
        }

        private void OnFileSystemChanged([NotNull] FakeSystemChangeEventArgs args)
        {
            FileSystemChanged?.Invoke(this, args);
        }
#endif
    }
}
