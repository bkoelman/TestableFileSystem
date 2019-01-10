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

        public void NotifyFileMoved([NotNull] IPathFormatter sourceFormatter, [NotNull] IPathFormatter destinationFormatter)
        {
            ProcessFileMoved(sourceFormatter, destinationFormatter);
        }

        partial void ProcessFileMoved([NotNull] IPathFormatter sourceFormatter, [NotNull] IPathFormatter destinationFormatter);

        public void NotifyContentsAccessed([NotNull] IPathFormatter formatter, FileAccessKinds accessKinds)
        {
            ProcessContentsAccessed(formatter, accessKinds);
        }

        partial void ProcessContentsAccessed([NotNull] IPathFormatter formatter, FileAccessKinds accessKinds);
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

        partial void ProcessFileMoved(IPathFormatter sourceFormatter, IPathFormatter destinationFormatter)
        {
            Guard.NotNull(sourceFormatter, nameof(sourceFormatter));
            Guard.NotNull(destinationFormatter, nameof(destinationFormatter));

            var args = new FakeSystemChangeEventArgs(WatcherChangeTypes.Renamed, NotifyFilters.FileName, destinationFormatter, sourceFormatter);
            OnFileSystemChanged(args);
        }

        partial void ProcessContentsAccessed(IPathFormatter formatter, FileAccessKinds accessKinds)
        {
            Guard.NotNull(formatter, nameof(formatter));

            NotifyFilters filters = GetNotifyFiltersForAccessKinds(accessKinds);
            if (filters != 0)
            {
                var args = new FakeSystemChangeEventArgs(WatcherChangeTypes.Changed, filters, formatter, null);
                OnFileSystemChanged(args);
            }
        }

        private static NotifyFilters GetNotifyFiltersForAccessKinds(FileAccessKinds accessKinds)
        {
            NotifyFilters filters = 0;

            if (accessKinds.HasFlag(FileAccessKinds.Read))
            {
                filters |= NotifyFilters.LastAccess;
            }

            if (accessKinds.HasFlag(FileAccessKinds.Write))
            {
                filters |= NotifyFilters.LastWrite;
            }

            if (accessKinds.HasFlag(FileAccessKinds.Resize))
            {
                filters |= NotifyFilters.Size;
            }

            if (accessKinds.HasFlag(FileAccessKinds.Attributes))
            {
                filters |= NotifyFilters.Attributes;
            }

            return filters;
        }

        private void OnFileSystemChanged([NotNull] FakeSystemChangeEventArgs args)
        {
            FileSystemChanged?.Invoke(this, args);
        }
#endif
    }
}
