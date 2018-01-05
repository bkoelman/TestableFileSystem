#if !NETCOREAPP1_1
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Tests
{
    internal sealed class FileSystemWatcherEventListener : IDisposable
    {
        private const string CreateEventName = "Created";
        private const string DeleteEventName = "Deleted";
        private const string ChangeEventName = "Changed";
        private const string RenameEventName = "Renamed";
        private const string ErrorEventName = "Error";

        [NotNull]
        private readonly IFileSystemWatcher watcher;

        [NotNull]
        [ItemNotNull]
        public ConcurrentBag<EventDetails> EventsCollected { get; } = new ConcurrentBag<EventDetails>();

        [NotNull]
        [ItemNotNull]
        public IEnumerable<FileSystemEventArgs> DeleteEventArgsCollected
        {
            get
            {
                return EventsCollected.Where(x => x.EventName == DeleteEventName).Select(x => x.Args).Cast<FileSystemEventArgs>();
            }
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<FileSystemEventArgs> CreateEventArgsCollected
        {
            get
            {
                return EventsCollected.Where(x => x.EventName == CreateEventName).Select(x => x.Args).Cast<FileSystemEventArgs>();
            }
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<FileSystemEventArgs> ChangeEventArgsCollected
        {
            get
            {
                return EventsCollected.Where(x => x.EventName == ChangeEventName).Select(x => x.Args).Cast<FileSystemEventArgs>();
            }
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<RenamedEventArgs> RenameEventArgsCollected
        {
            get
            {
                return EventsCollected.Where(x => x.EventName == RenameEventName).Select(x => x.Args).Cast<RenamedEventArgs>();
            }
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<ErrorEventArgs> ErrorEventArgsCollected
        {
            get
            {
                return EventsCollected.Where(x => x.EventName == ErrorEventName).Select(x => x.Args).Cast<ErrorEventArgs>();
            }
        }

        public FileSystemWatcherEventListener([NotNull] IFileSystemWatcher watcher)
        {
            Guard.NotNull(watcher, nameof(watcher));
            this.watcher = watcher;

            AttachEventHandlers();
        }

        private void AttachEventHandlers()
        {
            watcher.Deleted += WatcherOnDeleted;
            watcher.Created += WatcherOnCreated;
            watcher.Changed += WatcherOnChanged;
            watcher.Renamed += WatcherOnRenamed;
            watcher.Error += WatcherOnError;
        }

        public void Dispose()
        {
            DetachEventHandlers();
        }

        private void DetachEventHandlers()
        {
            watcher.Deleted -= WatcherOnDeleted;
            watcher.Created -= WatcherOnCreated;
            watcher.Changed -= WatcherOnChanged;
            watcher.Renamed -= WatcherOnRenamed;
            watcher.Error -= WatcherOnError;
        }

        private void WatcherOnDeleted([CanBeNull] object sender, [NotNull] FileSystemEventArgs args)
        {
            EventsCollected.Add(new EventDetails(DeleteEventName, args));
        }

        private void WatcherOnCreated([CanBeNull] object sender, [NotNull] FileSystemEventArgs args)
        {
            EventsCollected.Add(new EventDetails(CreateEventName, args));
        }

        private void WatcherOnChanged([CanBeNull] object sender, [NotNull] FileSystemEventArgs args)
        {
            EventsCollected.Add(new EventDetails(ChangeEventName, args));
        }

        private void WatcherOnRenamed([CanBeNull] object sender, [NotNull] RenamedEventArgs args)
        {
            EventsCollected.Add(new EventDetails(RenameEventName, args));
        }

        private void WatcherOnError([CanBeNull] object sender, [NotNull] ErrorEventArgs args)
        {
            EventsCollected.Add(new EventDetails(ErrorEventName, args));
        }
    }
}
#endif
