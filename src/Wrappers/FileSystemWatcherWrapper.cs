﻿#if !NETSTANDARD1_3
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Wrappers
{
    internal sealed class FileSystemWatcherWrapper : IFileSystemWatcher
    {
        [NotNull]
        private readonly FileSystemWatcher source;

        public string Path
        {
            get => source.Path;
            set => source.Path = value;
        }

        public string Filter
        {
            get => source.Filter;
            set => source.Filter = value;
        }

        public NotifyFilters NotifyFilter
        {
            get => source.NotifyFilter;
            set => source.NotifyFilter = value;
        }

        public bool IncludeSubdirectories
        {
            get => source.IncludeSubdirectories;
            set => source.IncludeSubdirectories = value;
        }

        public bool EnableRaisingEvents
        {
            get => source.EnableRaisingEvents;
            set => source.EnableRaisingEvents = value;
        }

        public int InternalBufferSize
        {
            get => source.InternalBufferSize;
            set => source.InternalBufferSize = value;
        }

        public event FileSystemEventHandler Deleted
        {
            add => source.Deleted += value;
            remove => source.Deleted -= value;
        }

        public event FileSystemEventHandler Created
        {
            add => source.Created += value;
            remove => source.Created -= value;
        }

        public event FileSystemEventHandler Changed
        {
            add => source.Changed += value;
            remove => source.Changed -= value;
        }

        public event RenamedEventHandler Renamed
        {
            add => source.Renamed += value;
            remove => source.Renamed -= value;
        }

        public event ErrorEventHandler Error
        {
            add => source.Error += value;
            remove => source.Error -= value;
        }

        public FileSystemWatcherWrapper([NotNull] FileSystemWatcher source)
        {
            Guard.NotNull(source, nameof(source));
            this.source = source;
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout = -1)
        {
            return source.WaitForChanged(changeType, timeout);
        }

        public void Dispose()
        {
            source.Dispose();
        }
    }
}
#endif
