#if !NETSTANDARD1_3
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
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
        // 7. [main] enqueues new work because HasBufferOverflow is unset

        // Flow control scenario on disposal
        // 1. [main] requests Dispose: set state to IsDisposed, unsubscribe from FS, signal CancellationToken
        // 2. [consumer] finishes event handler, then catches OperationCanceledException and terminates
        // 3. [main] joins consumer thread and disposes queue

        private const int MinBufferSize = 4096;

        private static readonly WaitForChangedResult WaitForChangeTimedOut = new WaitForChangedResult
        {
            TimedOut = true
        };

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

        // Fields below must only be accessed within lock.

        private WatcherState state;
        private int version;
        private bool hasBufferOverflow;
        private bool consumerIsFlushingBuffer;
        private bool consumerHasTerminated;

        [CanBeNull]
        private AbsolutePath directoryToWatch;

        [NotNull]
        private PathFilter pathFilter;

        private NotifyFilters notifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
        private bool includeSubdirectories;
        private int internalBufferSize = 8192;

        [NotNull]
        private readonly object waitForChangeLockObject = new object();

        [CanBeNull]
        private WaitForChangedResult? firstChangeResult;

        public string Path
        {
            get
            {
                lock (lockObject)
                {
                    return directoryToWatch?.GetText() ?? string.Empty;
                }
            }
            set
            {
                lock (lockObject)
                {
                    AbsolutePath newDirectoryToWatch;

                    if (string.IsNullOrEmpty(value))
                    {
                        newDirectoryToWatch = null;
                    }
                    else
                    {
                        if (!owner.Directory.Exists(value))
                        {
                            throw ErrorFactory.System.DirectoryNameIsInvalid(value);
                        }

                        newDirectoryToWatch = owner.ToAbsolutePathInLock(value);
                    }

                    if (!AbsolutePath.AreEquivalent(directoryToWatch, newDirectoryToWatch))
                    {
                        directoryToWatch = newDirectoryToWatch;
                        Restart();
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

        public NotifyFilters NotifyFilter
        {
            get
            {
                lock (lockObject)
                {
                    return notifyFilter;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (notifyFilter != value)
                    {
                        notifyFilter = value;
                        Restart();
                    }
                }
            }
        }

        public bool IncludeSubdirectories
        {
            get
            {
                lock (lockObject)
                {
                    return includeSubdirectories;
                }
            }
            set
            {
                lock (lockObject)
                {
                    if (includeSubdirectories != value)
                    {
                        includeSubdirectories = value;
                        Restart();
                    }
                }
            }
        }

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
                lock (lockObject)
                {
                    if (value)
                    {
                        Start();
                    }
                    else
                    {
                        Stop();
                    }
                }
            }
        }

        public int InternalBufferSize
        {
            get
            {
                lock (lockObject)
                {
                    return internalBufferSize;
                }
            }
            set
            {
                lock (lockObject)
                {
                    int valueInRange = value < MinBufferSize ? MinBufferSize : value;

                    if (internalBufferSize != valueInRange)
                    {
                        internalBufferSize = valueInRange;
                        Restart();
                    }
                }
            }
        }

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
            directoryToWatch = path;
            pathFilter = new PathFilter(filter, true);

            consumerThread = new Thread(() => ConsumerLoop(consumerCancellationTokenSource.Token))
            {
                IsBackground = true
            };
        }

        private void Start()
        {
            if (state == WatcherState.Disposed)
            {
                throw new ObjectDisposedException("FileSystemWatcher");
            }

            if (state == WatcherState.Suspended || state == WatcherState.Unstarted)
            {
                if (!owner.Directory.Exists(Path))
                {
                    throw ErrorFactory.System.ErrorReadingTheDirectory(Path);
                }

                if (state == WatcherState.Unstarted)
                {
                    consumerThread.Start();
                }

                changeTracker.FileSystemChanged += HandleFileSystemChange;

                version++;
                state = WatcherState.Active;
            }
        }

        private bool Stop()
        {
            if (state == WatcherState.Active)
            {
                changeTracker.FileSystemChanged -= HandleFileSystemChange;
                state = WatcherState.Suspended;

                return true;
            }

            return false;
        }

        private void Restart()
        {
            if (Stop())
            {
                Start();
            }
        }

        private void HandleFileSystemChange([CanBeNull] object sender, [NotNull] FakeSystemChangeEventArgs args)
        {
            Guard.NotNull(args, nameof(args));

            lock (lockObject)
            {
                if (state == WatcherState.Active)
                {
                    AbsolutePath pathInArgs = args.PathFormatter.GetPath();

                    AbsolutePath directoryToWatchSnapshot = directoryToWatch;
                    if (directoryToWatchSnapshot != null)
                    {
                        if (IsDeleteOfWatcherDirectory(args, pathInArgs))
                        {
                            producerConsumerQueue.Add(new FakeFileSystemVersionedChange(args, pathInArgs,
                                directoryToWatchSnapshot, version, true));
                        }

                        if (!hasBufferOverflow)
                        {
                            // TODO: How does InternalBufferSize relate to queue size?
                            // https://docs.microsoft.com/en-us/windows/desktop/api/winnt/ns-winnt-_file_notify_information
                            // DWORD: A 32-bit unsigned integer. The range is 0 through 4294967295 decimal.

                            // https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher.internalbuffersize?view=netframework-4.7.2

                            // For the moment, assume each entry represents 100 bytes.
                            if (producerConsumerQueue.Count >= InternalBufferSize / 100)
                            {
                                hasBufferOverflow = true;
                                version++;
                            }
                            else
                            {
                                if (MatchesFilters(args, pathInArgs, directoryToWatchSnapshot))
                                {
                                    producerConsumerQueue.Add(new FakeFileSystemVersionedChange(args, pathInArgs,
                                        directoryToWatchSnapshot, version, false));
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool IsDeleteOfWatcherDirectory([NotNull] FakeSystemChangeEventArgs args, [NotNull] AbsolutePath pathInArgs)
        {
            return args.ChangeType == WatcherChangeTypes.Deleted && args.Filters.HasFlag(NotifyFilters.DirectoryName) &&
                AbsolutePath.AreEquivalent(pathInArgs, directoryToWatch);
        }

        private bool MatchesFilters([NotNull] FakeSystemChangeEventArgs args, [NotNull] AbsolutePath pathInArgs,
            [NotNull] AbsolutePath watchDirectory)
        {
            if (!MatchesNotifyFilter(args.Filters))
            {
                return false;
            }

            if (!MatchesIncludeSubdirectories(pathInArgs, watchDirectory))
            {
                return false;
            }

            if (!MatchesPattern(pathInArgs))
            {
                return false;
            }

            return true;
        }

        private bool MatchesNotifyFilter(NotifyFilters filters)
        {
            return (notifyFilter & filters) != 0;
        }

        private bool MatchesIncludeSubdirectories([NotNull] AbsolutePath path, [NotNull] AbsolutePath watchDirectory)
        {
            return includeSubdirectories
                ? path.IsDescendantOf(watchDirectory)
                : AbsolutePath.AreEquivalent(watchDirectory, path.TryGetParentPath());
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
                        if (change.IsDeleteOfWatcherDirectory)
                        {
                            RaiseEventForWatcherDirectoryDeleted();
                            EnableRaisingEvents = false;
                        }
                        else
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
                    }
                    finally
                    {
                        // When an event handler throws, the process terminates (because of unhandled exception on background thread) by default.
                        // This can be overruled with <legacyUnhandledExceptionPolicy enabled="1" /> in app.config, in which case we'll be broken.
                        // Because after the first uncaught exception, this loop terminates and we no longer raise subsequent events.
                        // Such scenario is so rare though, that we ignore the problem for now.

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

        private void RaiseEventForWatcherDirectoryDeleted()
        {
            Error?.Invoke(this, new ErrorEventArgs(new Win32Exception(5)));
        }

        private void RaiseEventForBufferOverflow()
        {
            Exception error = ErrorFactory.System.TooManyChangesAtOnce(Path);
            Error?.Invoke(this, new ErrorEventArgs(error));
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
                    if (consumerHasTerminated || state == WatcherState.Unstarted || state == WatcherState.Disposed)
                    {
                        return;
                    }
                }

                Thread.Sleep(0);
            }
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout = Timeout.Infinite)
        {
            try
            {
                lock (waitForChangeLockObject)
                {
                    bool wasRunning = EnsureStarted();

                    firstChangeResult = null;
                    AttachHandlersForChangeTypes(changeType);
                    WaitForFirstChangeOrTimeout(timeout);

                    EnsureStopped(wasRunning);

                    return firstChangeResult ?? WaitForChangeTimedOut;
                }
            }
            finally
            {
                DetachHandlersForChangeTypes(changeType);
            }
        }

        private bool EnsureStarted()
        {
            lock (lockObject)
            {
                bool wasRunning = state == WatcherState.Active;
                if (!wasRunning)
                {
                    Start();
                }

                return wasRunning;
            }
        }

        private void EnsureStopped(bool wasRunning)
        {
            if (!wasRunning)
            {
                lock (lockObject)
                {
                    Stop();
                }
            }
        }

        private void WaitForFirstChangeOrTimeout(int timeout)
        {
            if (timeout == Timeout.Infinite)
            {
                while (firstChangeResult == null)
                {
                    Monitor.Wait(waitForChangeLockObject);
                }
            }
            else
            {
                Monitor.Wait(waitForChangeLockObject, timeout);
            }
        }

        private void AttachHandlersForChangeTypes(WatcherChangeTypes changeType)
        {
            if (changeType.HasFlag(WatcherChangeTypes.Created))
            {
                Created += WaitForChangeHandler;
            }

            if (changeType.HasFlag(WatcherChangeTypes.Deleted))
            {
                Deleted += WaitForChangeHandler;
            }

            if (changeType.HasFlag(WatcherChangeTypes.Changed))
            {
                Changed += WaitForChangeHandler;
            }

            if (changeType.HasFlag(WatcherChangeTypes.Renamed))
            {
                Renamed += WaitForChangeRenameHandler;
            }
        }

        private void DetachHandlersForChangeTypes(WatcherChangeTypes changeType)
        {
            if (changeType.HasFlag(WatcherChangeTypes.Created))
            {
                Created -= WaitForChangeHandler;
            }

            if (changeType.HasFlag(WatcherChangeTypes.Deleted))
            {
                Deleted -= WaitForChangeHandler;
            }

            if (changeType.HasFlag(WatcherChangeTypes.Changed))
            {
                Changed -= WaitForChangeHandler;
            }

            if (changeType.HasFlag(WatcherChangeTypes.Renamed))
            {
                Renamed -= WaitForChangeRenameHandler;
            }
        }

        private void WaitForChangeHandler([NotNull] object sender, [NotNull] FileSystemEventArgs args)
        {
            lock (waitForChangeLockObject)
            {
                if (firstChangeResult == null)
                {
                    firstChangeResult = new WaitForChangedResult
                    {
                        ChangeType = args.ChangeType,
                        Name = args.Name
                    };

                    Monitor.Pulse(waitForChangeLockObject);
                }
            }
        }

        private void WaitForChangeRenameHandler([NotNull] object sender, [NotNull] RenamedEventArgs args)
        {
            lock (waitForChangeLockObject)
            {
                if (firstChangeResult == null)
                {
                    firstChangeResult = new WaitForChangedResult
                    {
                        ChangeType = args.ChangeType,
                        Name = args.Name,
                        OldName = args.OldName
                    };

                    Monitor.Pulse(waitForChangeLockObject);
                }
            }
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

                if (!consumerThread.ThreadState.HasFlag(ThreadState.Unstarted))
                {
                    consumerThread.Join();
                }

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

            public bool IsDeleteOfWatcherDirectory { get; }

            public FakeFileSystemVersionedChange([NotNull] FakeSystemChangeEventArgs args, [NotNull] AbsolutePath pathInArgs,
                [NotNull] AbsolutePath basePath, int version, bool isDeleteOfWatcherDirectory)
            {
                Guard.NotNull(args, nameof(args));

                ChangeType = args.ChangeType;
                Version = version;
                IsDeleteOfWatcherDirectory = isDeleteOfWatcherDirectory;
                RootDirectory = basePath;
                RelativePath = pathInArgs.MakeRelativeTo(basePath);
                PreviousRelativePathInRename = args.PreviousPathInRenameFormatter?.GetPath().MakeRelativeTo(basePath);
            }
        }

        private enum WatcherState
        {
            Unstarted,
            Suspended,
            Active,
            Disposed
        }
    }
}
#endif
