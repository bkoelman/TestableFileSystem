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

        // Flow control on pause + resume:
        // 1. [main] requests Pause: set state to IsSuspended and increment version
        // 2. [main] discards any incoming operations because state is not Active
        // 3. [consumer] finishes event handler and flushes outdated queue entries
        // 4. [consumer] starts blocking for new work in queue
        // 5. [main] requests Resume: set state to Active
        // 6. [main] enqueues new work in queue because state is Active

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
        // 3. [main] joins consumer thread (as part of disposal process) and disposes queue

        [NotNull]
        private readonly FakeFileSystem owner;

        [NotNull]
        [ItemNotNull]
        private readonly BlockingCollection<VersionedSystemChange> producerConsumerQueue =
            new BlockingCollection<VersionedSystemChange>();

        [NotNull]
        private readonly CancellationTokenSource consumerCancellationTokenSource = new CancellationTokenSource();

        [NotNull]
        private readonly Task consumerTask;

        [NotNull]
        private readonly WatcherStatus status = new WatcherStatus();

        [CanBeNull]
        private readonly AbsolutePath targetPath;

        public string Path
        {
            get => targetPath?.GetText() ?? string.Empty;
            // TODO: What happens when changing path on running instance? Restart if it was running.
            set => throw new NotImplementedException();
        }

        public string Filter { get; set; }
        public NotifyFilters NotifyFilter { get; set; }
        public bool IncludeSubdirectories { get; set; }

        public bool EnableRaisingEvents
        {
            get => status.State == WatcherState.Active;
            set
            {
                // TODO: Precondition checks
                if (value)
                {
                    if (status.State == WatcherState.Disposed)
                    {
                        throw new ObjectDisposedException(GetType().FullName);
                    }

                    if (status.State == WatcherState.Suspended)
                    {
                        owner.FileSystemChanged += HandleFileSystemChange;
                        status.State = WatcherState.Active;
                    }
                }
                else
                {
                    if (status.State == WatcherState.Active)
                    {
                        owner.FileSystemChanged -= HandleFileSystemChange;
                        status.State = WatcherState.Suspended;
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

        internal FakeFileSystemWatcher([NotNull] FakeFileSystem fileSystem, [CanBeNull] AbsolutePath path,
            [NotNull] string filter)
        {
            Guard.NotNull(fileSystem, nameof(fileSystem));
            Guard.NotNull(filter, nameof(filter));

            owner = fileSystem;

            CancellationToken cancellationToken = consumerCancellationTokenSource.Token;
            consumerTask = Task.Run(() => ConsumerLoop(cancellationToken));

            // TODO: Throw when non-null path does not exist.

            // Satisfy the compiler, for the moment.
            Error?.Invoke(null, null);

            targetPath = path;
            Filter = filter;
        }

        private void HandleFileSystemChange([CanBeNull] object sender, [NotNull] SystemChangeEventArgs args)
        {
            Guard.NotNull(args, nameof(args));

            if (status.State == WatcherState.Active && !status.HasBufferOverflow)
            {
                // TODO: How does InternalBufferSize relate to queue size?
                if (producerConsumerQueue.Count >= InternalBufferSize)
                {
                    status.HasBufferOverflow = true;
                    status.IncrementVersion();
                }
                else
                {
                    if (MatchesFilters(args))
                    {
                        producerConsumerQueue.Add(new VersionedSystemChange(args, status.Version));
                    }
                }
            }
        }

        private bool MatchesFilters([NotNull] SystemChangeEventArgs args)
        {
            // TODO: Match against change type, path and file mask.
            return true;
        }

        private void ConsumerLoop(CancellationToken cancellationToken)
        {
            try
            {
                foreach (VersionedSystemChange change in producerConsumerQueue.GetConsumingEnumerable(cancellationToken))
                {
                    if (producerConsumerQueue.Count == 0 && status.HasBufferOverflow)
                    {
                        status.HasBufferOverflow = false;
                    }

                    if (change.Version == status.Version)
                    {
                        RaiseEventForChange(change);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void RaiseEventForChange([NotNull] VersionedSystemChange change)
        {
            // TODO: Raise event for buffer overflow.

            AbsolutePath parentPath = change.Path.TryGetParentPath();
            switch (change.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    Created?.Invoke(this,
                        new FileSystemEventArgs(change.ChangeType, parentPath.GetText(), change.Path.Components.Last()));
                    break;
                case WatcherChangeTypes.Deleted:
                    Deleted?.Invoke(this,
                        new FileSystemEventArgs(change.ChangeType, parentPath.GetText(), change.Path.Components.Last()));
                    break;
                case WatcherChangeTypes.Changed:
                    Changed?.Invoke(this,
                        new FileSystemEventArgs(change.ChangeType, parentPath.GetText(), change.Path.Components.Last()));
                    break;
                case WatcherChangeTypes.Renamed:
                    Renamed?.Invoke(this,
                        new RenamedEventArgs(change.ChangeType, parentPath.GetText(), change.Path.Components.Last(),
                            change.PreviousPathInRename.Components.Last()));
                    break;
            }
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout = -1)
        {
            // TODO: Temporarily subscribe to our own events, then wait and see what happens...
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (status.State != WatcherState.Disposed)
            {
                status.State = WatcherState.Disposed;

                EnableRaisingEvents = false;

                consumerCancellationTokenSource.Cancel();
                consumerCancellationTokenSource.Dispose();

                consumerTask.Wait();
                consumerTask.Dispose();

                producerConsumerQueue.Dispose();
            }
        }

        private sealed class VersionedSystemChange
        {
            // TODO: Review for thread safety and race conditions.

            [NotNull]
            private readonly SystemChangeEventArgs args;

            public WatcherChangeTypes ChangeType => args.ChangeType;

            [NotNull]
            public AbsolutePath Path => args.Path;

            [CanBeNull]
            public AbsolutePath PreviousPathInRename => args.PreviousPathInRename;

            public int Version { get; }

            public VersionedSystemChange([NotNull] SystemChangeEventArgs args, int version)
            {
                Guard.NotNull(args, nameof(args));

                this.args = args;
                Version = version;
            }
        }

        private sealed class WatcherStatus
        {
            private int state;
            private int hasBufferOverflow;
            private int version;

            public WatcherState State
            {
                get => (WatcherState)Interlocked.CompareExchange(ref state, 0xFF, 0xFF);
                set => Interlocked.Exchange(ref state, (int)value);
            }

            public int Version => Interlocked.CompareExchange(ref hasBufferOverflow, -1, -1);

            public bool HasBufferOverflow
            {
                get => Interlocked.CompareExchange(ref hasBufferOverflow, 0xFF, 0xFF) == 1;
                set => Interlocked.Exchange(ref hasBufferOverflow, value ? 0 : 1);
            }

            public void IncrementVersion()
            {
                Interlocked.Increment(ref version);
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
