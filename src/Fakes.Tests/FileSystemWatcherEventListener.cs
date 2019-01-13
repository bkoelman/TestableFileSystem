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
        private readonly List<EventDetails> eventsCollected = new List<EventDetails>();

        [NotNull]
        private readonly object lockObject = new object();

        [NotNull]
        [ItemNotNull]
        public List<EventDetails> EventsCollected
        {
            get
            {
                lock (lockObject)
                {
                    return eventsCollected.ToList();
                }
            }
        }

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

            watcher.EnableRaisingEvents = true;
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
            lock (lockObject)
            {
                eventsCollected.Add(new EventDetails(DeleteEventName, args));
            }
        }

        private void WatcherOnCreated([CanBeNull] object sender, [NotNull] FileSystemEventArgs args)
        {
            lock (lockObject)
            {
                eventsCollected.Add(new EventDetails(CreateEventName, args));
            }
        }

        private void WatcherOnChanged([CanBeNull] object sender, [NotNull] FileSystemEventArgs args)
        {
            lock (lockObject)
            {
                eventsCollected.Add(new EventDetails(ChangeEventName, args));
            }
        }

        private void WatcherOnRenamed([CanBeNull] object sender, [NotNull] RenamedEventArgs args)
        {
            lock (lockObject)
            {
                eventsCollected.Add(new EventDetails(RenameEventName, args));
            }
        }

        private void WatcherOnError([CanBeNull] object sender, [NotNull] ErrorEventArgs args)
        {
            lock (lockObject)
            {
                eventsCollected.Add(new EventDetails(ErrorEventName, args));
            }
        }

        [NotNull]
        [ItemNotNull]
        public IList<string> GetEventsCollectedAsText()
        {
            var lines = new List<string>();

            foreach (EventDetails details in EventsCollected)
            {
                if (details.Args is FileSystemEventArgs fileArgs)
                {
                    var symbol = GetChangeSymbol(fileArgs.ChangeType);
                    lines.Add(details.Args is RenamedEventArgs renameArgs
                        ? $"{symbol} {renameArgs.OldName} => {fileArgs.Name}"
                        : $"{symbol} {fileArgs.Name}");
                }
                else if (details.Args is ErrorEventArgs errorArgs)
                {
                    lines.Add($"! {errorArgs.GetException().Message}");
                }
            }

            return lines;
        }

        [NotNull]
        private static string GetChangeSymbol(WatcherChangeTypes changeType)
        {
            switch (changeType)
            {
                case WatcherChangeTypes.Created:
                {
                    return "+";
                }
                case WatcherChangeTypes.Deleted:
                {
                    return "-";
                }
                case WatcherChangeTypes.Changed:
                {
                    return "*";
                }
                case WatcherChangeTypes.Renamed:
                {
                    return ">";
                }
                default:
                {
                    return "?";
                }
            }
        }
    }
}
#endif
