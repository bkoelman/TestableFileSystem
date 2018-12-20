﻿using System;
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

        public void NotifyFileContentsAccessed([NotNull] IPathFormatter formatter, FileAccessKinds accessKinds)
        {
            ProcessFileContentsAccessed(formatter, accessKinds);
        }

        partial void ProcessFileContentsAccessed([NotNull] IPathFormatter formatter, FileAccessKinds accessKinds);

        public void NotifyAttributesChanged([NotNull] IPathFormatter formatter)
        {
            ProcessAttributesChanged(formatter);
        }

        partial void ProcessAttributesChanged([NotNull] IPathFormatter formatter);
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

        partial void ProcessFileContentsAccessed(IPathFormatter formatter, FileAccessKinds accessKinds)
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

            return filters;
        }

        partial void ProcessAttributesChanged(IPathFormatter formatter)
        {
            Guard.NotNull(formatter, nameof(formatter));

            var args = new FakeSystemChangeEventArgs(WatcherChangeTypes.Changed, NotifyFilters.Attributes, formatter, null);
            OnFileSystemChanged(args);
        }

        private void OnFileSystemChanged([NotNull] FakeSystemChangeEventArgs args)
        {
            FileSystemChanged?.Invoke(this, args);
        }
#endif
    }
}
