using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed partial class FakeFileSystemChangeTracker
    {
        public void NotifyFileCreated([NotNull] AbsolutePath path)
        {
            ProcessFileCreated(path);
        }

        partial void ProcessFileCreated([NotNull] AbsolutePath path);

        public void NotifyFileDeleted([NotNull] AbsolutePath path)
        {
            ProcessFileDeleted(path);
        }

        partial void ProcessFileDeleted([NotNull] AbsolutePath path);
    }

    internal sealed partial class FakeFileSystemChangeTracker
    {
#if !NETSTANDARD1_3
        public event EventHandler<FakeSystemChangeEventArgs> FileSystemChanged;

        partial void ProcessFileCreated(AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            var args = new FakeSystemChangeEventArgs(WatcherChangeTypes.Created, path, null);
            OnFileSystemChanged(args);
        }

        partial void ProcessFileDeleted(AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            var args = new FakeSystemChangeEventArgs(WatcherChangeTypes.Deleted, path, null);
            OnFileSystemChanged(args);
        }

        private void OnFileSystemChanged([NotNull] FakeSystemChangeEventArgs args)
        {
            FileSystemChanged?.Invoke(this, args);
        }
#endif
    }
}
