#if !NETSTANDARD1_3
using System;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    public interface IFileSystemWatcher : IDisposable
    {
        [NotNull]
        string Path { get; set; }

        [NotNull]
        string Filter { get; set; }

        NotifyFilters NotifyFilter { get; set; }
        bool IncludeSubdirectories { get; set; }
        bool EnableRaisingEvents { get; set; }
        int InternalBufferSize { get; set; }

        event FileSystemEventHandler Deleted;
        event FileSystemEventHandler Created;
        event FileSystemEventHandler Changed;
        event RenamedEventHandler Renamed;
        event ErrorEventHandler Error;

        WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout = -1);
    }
}
#endif
