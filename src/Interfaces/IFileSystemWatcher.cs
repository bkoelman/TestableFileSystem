#if !NETSTANDARD1_3
using System;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Interfaces
{
    /// <summary>
    /// Listens to the file system change notifications and raises events when a directory, or file in a directory, changes.
    /// </summary>
    public interface IFileSystemWatcher : IDisposable
    {
        /// <inheritdoc cref="FileSystemWatcher.Path" />
        [NotNull]
        string Path { get; set; }

        /// <inheritdoc cref="FileSystemWatcher.Filter" />
        [NotNull]
        string Filter { get; set; }

        /// <inheritdoc cref="FileSystemWatcher.NotifyFilter" />
        NotifyFilters NotifyFilter { get; set; }

        /// <inheritdoc cref="FileSystemWatcher.IncludeSubdirectories" />
        bool IncludeSubdirectories { get; set; }

        /// <inheritdoc cref="FileSystemWatcher.EnableRaisingEvents" />
        bool EnableRaisingEvents { get; set; }

        /// <inheritdoc cref="FileSystemWatcher.InternalBufferSize" />
        int InternalBufferSize { get; set; }

        /// <inheritdoc cref="FileSystemWatcher.Deleted" />
        event FileSystemEventHandler Deleted;

        /// <inheritdoc cref="FileSystemWatcher.Created" />
        event FileSystemEventHandler Created;

        /// <inheritdoc cref="FileSystemWatcher.Changed" />
        event FileSystemEventHandler Changed;

        /// <inheritdoc cref="FileSystemWatcher.Renamed" />
        event RenamedEventHandler Renamed;

        /// <inheritdoc cref="FileSystemWatcher.Error" />
        event ErrorEventHandler Error;

        /// <inheritdoc cref="FileSystemWatcher.WaitForChanged(WatcherChangeTypes,int)" />
        WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout = -1);
    }
}
#endif
