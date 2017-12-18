using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed partial class FileSystemChangeTracker
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

    internal sealed partial class FileSystemChangeTracker
    {
#if !NETSTANDARD1_3
        public event EventHandler<SystemChangeEventArgs> FileSystemChanged;

        partial void ProcessFileCreated(AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            var args = new SystemChangeEventArgs(WatcherChangeTypes.Created, path, null);
            OnFileSystemChanged(args);
        }

        partial void ProcessFileDeleted(AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            var args = new SystemChangeEventArgs(WatcherChangeTypes.Deleted, path, null);
            OnFileSystemChanged(args);
        }

        private void OnFileSystemChanged([NotNull] SystemChangeEventArgs args)
        {
            FileSystemChanged?.Invoke(this, args);
        }
#endif
    }
}
