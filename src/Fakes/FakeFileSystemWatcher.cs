#if !NETSTANDARD1_3
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeFileSystemWatcher : IFileSystemWatcher
    {
        // How FSW works:
        // - Events may be raised from different threads (likely from the ThreadPool)
        // - Multiple events do not execute concurrently
        // - Exceptions raised from event handlers (on background thread) crash the process

        // Flow control on pause + resume:
        // 1. [main] requests Pause: set state to IsSuspended and increment version
        // 2. [main] discards any incoming operations because state is not Active
        // 3. [consumer] finishes event handler and flushes outdated queue entries
        // 4. [consumer] starts blocking for new work in queue
        // 5. [main] requests Resume: set state to Active
        // 6. [main] enqueues new work because state is Active

        // Flow control scenario on buffer overflow
        // 1. [main] detects that maximum queue length threshold is being exceeded
        // 2. [main] sets HasBufferOverflow and increments version
        // 3. [main] discards any incoming operations because HasBufferOverflow is set
        // 4. [consumer] finishes event handler and flushes outdated queue entries
        // 5. [consumer] detects empty queue and unsets HasBufferOverflow
        // 6. [consumer] starts blocking for new work in queue
        // 7. [main] enqueues new work in queue because HasBufferOverflow is unset

        // Flow control scenario on disposal
        // 1. [main] requests Dispose: set state to IsDisposed, unsubscribe from FS, signal CancellationToken
        // 2. [consumer] finishes event handler, then catches OperationCanceledException and terminates
        // 3. [main] joins consumer thread and disposes queue

        [NotNull]
        private readonly FakeFileSystem owner;

        [NotNull]
        private readonly FakeFileSystemChangeTracker changeTracker;

        [NotNull]
        [ItemNotNull]
        private readonly BlockingCollection<FakeFileSystemVersionedChange> producerConsumerQueue =
            new BlockingCollection<FakeFileSystemVersionedChange>();

        [NotNull]
        private readonly CancellationTokenSource consumerCancellationTokenSource = new CancellationTokenSource();

        [NotNull]
        private readonly Thread consumerThread;

        [NotNull]
        private readonly object lockObject = new object();

        private WatcherState state;
        private int version;
        private bool hasBufferOverflow;
        private bool consumerIsFlushingBuffer;
        private bool consumerHasTerminated;

        [CanBeNull]
        private AbsolutePath targetPath;

        [NotNull]
        private PathFilter pathFilter;

        // TODO: What happens when changing properties on running instance?

        public string Path
        {
            get
            {
                lock (lockObject)
                {
                    return targetPath?.GetText() ?? string.Empty;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        targetPath = null;
                    }
                    else
                    {
                        if (!owner.Directory.Exists(value))
                        {
                            throw ErrorFactory.System.DirectoryNameIsInvalid(value);
                        }

                        targetPath = owner.ToAbsolutePathInLock(value);
                    }
                }
            }
        }

        public string Filter
        {
            get
            {
                lock (lockObject)
                {
                    return pathFilter.Text;
                }
            }
            set
            {
                lock (lockObject)
                {
                    pathFilter = new PathFilter(value, false);
                }
            }
        }

        public NotifyFilters NotifyFilter { get; set; } =
            NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

        public bool IncludeSubdirectories { get; set; }

        public bool EnableRaisingEvents
        {
            get
            {
                lock (lockObject)
                {
                    return state == WatcherState.Active;
                }
            }
            set
            {
                // TODO: Precondition checks (Path must still exist, for example)

                lock (lockObject)
                {
                    if (value)
                    {
                        if (state == WatcherState.Disposed)
                        {
                            throw new ObjectDisposedException(GetType().FullName);
                        }

                        if (state == WatcherState.Suspended)
                        {
                            if (!owner.Directory.Exists(Path))
                            {
                                throw ErrorFactory.System.ErrorReadingTheDirectory(Path);
                            }

                            // TODO: Should we put some kind of lock on the directory being watched?

                            changeTracker.FileSystemChanged += HandleFileSystemChange;

                            version++;
                            state = WatcherState.Active;
                        }
                    }
                    else
                    {
                        if (state == WatcherState.Active)
                        {
                            changeTracker.FileSystemChanged -= HandleFileSystemChange;
                            state = WatcherState.Suspended;
                        }
                    }
                }
            }
        }

        public int InternalBufferSize { get; set; } = 8192;

        public event FileSystemEventHandler Deleted;
        public event FileSystemEventHandler Created;
        public event FileSystemEventHandler Changed;
        public event RenamedEventHandler Renamed;
        public event ErrorEventHandler Error;

        internal FakeFileSystemWatcher([NotNull] FakeFileSystem owner, [NotNull] FakeFileSystemChangeTracker changeTracker,
            [CanBeNull] AbsolutePath path, [NotNull] string filter)
        {
            Guard.NotNull(owner, nameof(owner));
            Guard.NotNull(changeTracker, nameof(changeTracker));
            Guard.NotNull(filter, nameof(filter));

            this.owner = owner;
            this.changeTracker = changeTracker;
            targetPath = path;
            pathFilter = new PathFilter(filter, true);

            consumerThread = new Thread(() => ConsumerLoop(consumerCancellationTokenSource.Token));
            consumerThread.Start();
        }

        private void HandleFileSystemChange([CanBeNull] object sender, [NotNull] FakeSystemChangeEventArgs args)
        {
            Guard.NotNull(args, nameof(args));

            lock (lockObject)
            {
                if (state == WatcherState.Active && !hasBufferOverflow)
                {
                    // TODO: How does InternalBufferSize relate to queue size?

                    if (producerConsumerQueue.Count >= InternalBufferSize)
                    {
                        hasBufferOverflow = true;
                        version++;
                    }
                    else
                    {
                        if (targetPath != null && MatchesFilters(args, targetPath))
                        {
                            producerConsumerQueue.Add(new FakeFileSystemVersionedChange(args, targetPath, version));
                        }
                    }
                }
            }
        }

        private bool MatchesFilters([NotNull] FakeSystemChangeEventArgs args, [NotNull] AbsolutePath watchDirectory)
        {
            if (!MatchesNotifyFilter(args.Filters))
            {
                return false;
            }

            AbsolutePath path = args.PathFormatter.GetPath();

            if (!MatchesIncludeSubdirectories(path, watchDirectory))
            {
                return false;
            }

            if (!MatchesPattern(path))
            {
                return false;
            }

            return true;
        }

        private bool MatchesNotifyFilter(NotifyFilters filters)
        {
            return (NotifyFilter & filters) != 0;
        }

        private bool MatchesIncludeSubdirectories([NotNull] AbsolutePath path, [NotNull] AbsolutePath watchDirectory)
        {
            return IncludeSubdirectories
                ? path.IsDescendantOf(watchDirectory)
                : watchDirectory.IsEquivalentTo(path.TryGetParentPath());
        }

        private bool MatchesPattern([NotNull] AbsolutePath path)
        {
            string fileOrDirectoryName = path.Components.Last();
            return pathFilter.IsMatch(fileOrDirectoryName);
        }

        private void ConsumerLoop(CancellationToken cancellationToken)
        {
            try
            {
                foreach (FakeFileSystemVersionedChange change in producerConsumerQueue.GetConsumingEnumerable(cancellationToken))
                {
                    bool doRaiseBufferOverflowEvent = false;
                    bool doRaiseChangeEvent = false;

                    lock (lockObject)
                    {
                        if (hasBufferOverflow && !consumerIsFlushingBuffer)
                        {
                            doRaiseBufferOverflowEvent = true;
                            consumerIsFlushingBuffer = true;
                        }

                        if (change.Version == version)
                        {
                            doRaiseChangeEvent = true;
                        }
                    }

                    try
                    {
                        if (doRaiseBufferOverflowEvent)
                        {
                            // TODO: What happens when event handler throws?
                            RaiseEventForBufferOverflow();
                        }

                        if (doRaiseChangeEvent)
                        {
                            // TODO: What happens when event handler throws?
                            RaiseEventForChange(change);
                        }
                    }
                    finally
                    {
                        lock (lockObject)
                        {
                            if (!producerConsumerQueue.Any())
                            {
                                hasBufferOverflow = false;
                                consumerIsFlushingBuffer = false;
                            }

                            if (producerConsumerQueue.IsCompleted)
                            {
                                consumerHasTerminated = true;
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void RaiseEventForBufferOverflow()
        {
            Error?.Invoke(this, new ErrorEventArgs(new Exception("TODO: Buffer overflow")));
        }

        private void RaiseEventForChange([NotNull] FakeFileSystemVersionedChange change)
        {
            string rootDirectory = change.RootDirectory.GetText();

            switch (change.ChangeType)
            {
                case WatcherChangeTypes.Created:
                {
                    Created?.Invoke(this, new FileSystemEventArgs(change.ChangeType, rootDirectory, change.RelativePath));
                    break;
                }
                case WatcherChangeTypes.Deleted:
                {
                    Deleted?.Invoke(this, new FileSystemEventArgs(change.ChangeType, rootDirectory, change.RelativePath));
                    break;
                }
                case WatcherChangeTypes.Changed:
                {
                    Changed?.Invoke(this, new FileSystemEventArgs(change.ChangeType, rootDirectory, change.RelativePath));
                    break;
                }
                case WatcherChangeTypes.Renamed:
                {
                    Renamed?.Invoke(this,
                        new RenamedEventArgs(change.ChangeType, rootDirectory, change.RelativePath,
                            change.PreviousRelativePathInRename));
                    break;
                }
            }
        }

        public void WaitForCompleted(int timeout = Timeout.Infinite)
        {
            DateTime endTimeUtc = timeout == Timeout.Infinite ? DateTime.MaxValue : DateTime.UtcNow.AddMilliseconds(timeout);

            producerConsumerQueue.CompleteAdding();

            while (DateTime.UtcNow < endTimeUtc)
            {
                lock (lockObject)
                {
                    if (consumerHasTerminated || state != WatcherState.Active)
                    {
                        return;
                    }
                }

                Thread.Sleep(0);
            }
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout = Timeout.Infinite)
        {
            // TODO: Temporarily subscribe to our own events, then wait and see what happens...

            throw new NotImplementedException();
        }

        public void Dispose()
        {
            bool doCleanup = false;

            lock (lockObject)
            {
                if (state != WatcherState.Disposed)
                {
                    EnableRaisingEvents = false;
                    state = WatcherState.Disposed;
                    doCleanup = true;
                }
            }

            if (doCleanup)
            {
                consumerCancellationTokenSource.Cancel();
                consumerThread.Join();

                consumerCancellationTokenSource.Dispose();
                producerConsumerQueue.Dispose();
            }
        }

        private sealed class PathFilter
        {
            [NotNull]
            private readonly PathPattern.Sequence sequence;

            [NotNull]
            public string Text { get; }

            public PathFilter([CanBeNull] string text, bool allowEmptyString)
            {
                if (allowEmptyString && text == string.Empty)
                {
                    Text = string.Empty;
                    sequence = PathPattern.ParsePattern("*");
                }
                else
                {
                    Text = string.IsNullOrEmpty(text) ? "*.*" : text;
                    sequence = PathPattern.ParsePattern(Text == "*.*" ? "*" : Text);
                }
            }

            public bool IsMatch([NotNull] string text)
            {
                return sequence.IsMatch(text);
            }
        }

        private sealed class FakeFileSystemVersionedChange
        {
            public WatcherChangeTypes ChangeType { get; }

            [NotNull]
            public AbsolutePath RootDirectory { get; }

            [NotNull]
            public string RelativePath { get; }

            [CanBeNull]
            public string PreviousRelativePathInRename { get; }

            public int Version { get; }

            public FakeFileSystemVersionedChange([NotNull] FakeSystemChangeEventArgs args, [NotNull] AbsolutePath basePath,
                int version)
            {
                Guard.NotNull(args, nameof(args));

                ChangeType = args.ChangeType;
                Version = version;
                RootDirectory = basePath;
                RelativePath = args.PathFormatter.GetPath().MakeRelativeTo(basePath);
                PreviousRelativePathInRename = args.PreviousPathInRenameFormatter?.GetPath().MakeRelativeTo(basePath);
            }
        }

        private enum WatcherState
        {
            Suspended,
            Active,
            Disposed
        }
    }
}
#endif
