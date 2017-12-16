#if !NETSTANDARD1_3
using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileSystemWatcher : IFileSystemWatcher
    {
        public string Path { get; set; }
        public string Filter { get; set; }
        public NotifyFilters NotifyFilter { get; set; }
        public bool IncludeSubdirectories { get; set; }
        public bool EnableRaisingEvents { get; set; }

        public event FileSystemEventHandler Deleted;
        public event FileSystemEventHandler Created;
        public event FileSystemEventHandler Changed;
        public event RenamedEventHandler Renamed;
        public event ErrorEventHandler Error;

        internal FakeFileSystemWatcher([NotNull] IFileSystem fileSystem, [NotNull] string path, [NotNull] string filter)
        {
            Guard.NotNull(fileSystem, nameof(fileSystem));
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(filter, nameof(filter));

            // Satisfy the compiler, for the moment.
            Deleted?.Invoke(null, null);
            Created?.Invoke(null, null);
            Changed?.Invoke(null, null);
            Renamed?.Invoke(null, null);
            Error?.Invoke(null, null);

            Path = path;
            Filter = filter;
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout = -1)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
#endif
