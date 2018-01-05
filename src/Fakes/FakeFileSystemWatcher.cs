#if !NETSTANDARD1_3
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly FakeFileSystemChangeTracker changeTracker;

        [NotNull]
        [ItemNotNull]
        private readonly BlockingCollection<FakeFileSystemVersionedChange> producerConsumerQueue =
            new BlockingCollection<FakeFileSystemVersionedChange>();

        [NotNull]
        private readonly CancellationTokenSource consumerCancellationTokenSource = new CancellationTokenSource();

        [NotNull]
        private readonly Task consumerTask;

        [NotNull]
        private readonly object lockObject = new object();

        private WatcherState state;
        private int version;
        private bool hasBufferUnderflow;
        private bool hasBufferOverflow;
        private bool consumerIsFlushingBuffer;

        [CanBeNull]
        private readonly AbsolutePath targetPath;

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
                // TODO: Restart when changing path on running instance.

                lock (lockObject)
                {
                    throw new NotImplementedException();
                }
            }
        }

        // TODO: What happens when changing these properties on running instance?

        public string Filter { get; set; }
        public NotifyFilters NotifyFilter { get; set; }
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
                // TODO: Precondition checks

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

        internal FakeFileSystemWatcher([NotNull] FakeFileSystemChangeTracker changeTracker, [CanBeNull] AbsolutePath path,
            [NotNull] string filter)
        {
            Guard.NotNull(changeTracker, nameof(changeTracker));
            Guard.NotNull(filter, nameof(filter));

            this.changeTracker = changeTracker;

            CancellationToken cancellationToken = consumerCancellationTokenSource.Token;
            consumerTask = Task.Run(() => ConsumerLoop(cancellationToken));

            // TODO: Throw when non-null path does not exist.

            targetPath = path;
            Filter = filter;
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
                        if (MatchesFilters(args))
                        {
                            producerConsumerQueue.Add(new FakeFileSystemVersionedChange(args, version));
                        }
                    }
                }
            }
        }

        private bool MatchesFilters([NotNull] FakeSystemChangeEventArgs args)
        {
            // TODO: Match against change type, path and file mask.

            return true;
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
                        hasBufferUnderflow = false;

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
                            RaiseEventForBufferOverflow();
                        }

                        if (doRaiseChangeEvent)
                        {
                            RaiseEventForChange(change);
                        }
                    }
                    finally
                    {
                        lock (lockObject)
                        {
                            if (!producerConsumerQueue.Any())
                            {
                                hasBufferUnderflow = true;
                                hasBufferOverflow = false;
                                consumerIsFlushingBuffer = false;
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
            string rootDirectory = change.Path.TryGetParentPath()?.GetText() ?? change.Path.GetText();

            switch (change.ChangeType)
            {
                case WatcherChangeTypes.Created:
                {
                    Created?.Invoke(this,
                        new FileSystemEventArgs(change.ChangeType, rootDirectory, change.Path.Components.Last()));
                    break;
                }
                case WatcherChangeTypes.Deleted:
                {
                    Deleted?.Invoke(this,
                        new FileSystemEventArgs(change.ChangeType, rootDirectory, change.Path.Components.Last()));
                    break;
                }
                case WatcherChangeTypes.Changed:
                {
                    Changed?.Invoke(this,
                        new FileSystemEventArgs(change.ChangeType, rootDirectory, change.Path.Components.Last()));
                    break;
                }
                case WatcherChangeTypes.Renamed:
                {
                    Renamed?.Invoke(this, new RenamedEventArgs(change.ChangeType, rootDirectory, change.Path.Components.Last(),
                        // ReSharper disable once PossibleNullReferenceException
                        change.PreviousPathInRename.Components.Last()));
                    break;
                }
            }
        }

        public void WaitForEventDispatcherIdle(int timeout = Timeout.Infinite)
        {
            DateTime endTimeUtc = timeout == Timeout.Infinite ? DateTime.MaxValue : DateTime.UtcNow.AddMilliseconds(timeout);

            while (DateTime.UtcNow < endTimeUtc)
            {
                lock (lockObject)
                {
                    if (hasBufferUnderflow || state != WatcherState.Active)
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
                consumerTask.Wait();

                consumerTask.Dispose();
                consumerCancellationTokenSource.Dispose();
                producerConsumerQueue.Dispose();
            }
        }

        private sealed class FakeFileSystemVersionedChange
        {
            [NotNull]
            private readonly FakeSystemChangeEventArgs args;

            public WatcherChangeTypes ChangeType => args.ChangeType;

            [NotNull]
            public AbsolutePath Path => args.Path;

            [CanBeNull]
            public AbsolutePath PreviousPathInRename => args.PreviousPathInRename;

            public int Version { get; }

            public FakeFileSystemVersionedChange([NotNull] FakeSystemChangeEventArgs args, int version)
            {
                Guard.NotNull(args, nameof(args));

                this.args = args;
                Version = version;
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
