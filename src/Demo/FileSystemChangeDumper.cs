using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Demo
{
    /// <summary>
    /// Displays incoming changes from <see cref="IFileSystemWatcher" /> on the console.
    /// </summary>
    internal sealed class FileSystemChangeDumper
    {
        private const NotifyFilters NotifyFiltersAll = NotifyFilters.FileName | NotifyFilters.DirectoryName |
            NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess |
            NotifyFilters.CreationTime | NotifyFilters.Security;

        private readonly IFileSystem fileSystem;

        public FileSystemChangeDumper(IFileSystem fileSystem)
        {
            Guard.NotNull(fileSystem, nameof(fileSystem));
            this.fileSystem = fileSystem;
        }

        public void Start(string path, NotifyFilters notifyFilters = NotifyFiltersAll)
        {
            Guard.NotNull(path, nameof(path));

            using (var setupCompletedSignal = new ConcurrentSignal())
            {
                StartMonitorChangesOnDiskLoop(path, notifyFilters, setupCompletedSignal);
            }

            Thread.Sleep(250);
        }

        private void StartMonitorChangesOnDiskLoop(string path, NotifyFilters notifyFilters,
            ConcurrentSignal setupCompletedSignal)
        {
            Task.Run(() => MonitorChangesOnDiskLoop(path, notifyFilters, setupCompletedSignal));
        }

        private void MonitorChangesOnDiskLoop(string path, NotifyFilters notifyFilters, ConcurrentSignal setupCompletedSignal)
        {
            IFileSystemWatcher watcher = Setup(path, notifyFilters);
            setupCompletedSignal.SetComplete();

            while (true)
            {
                GC.KeepAlive(watcher);
            }

            // ReSharper disable once FunctionNeverReturns
        }

        private IFileSystemWatcher Setup(string path, NotifyFilters notifyFilters)
        {
            IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher();

            watcher.Created += (sender, args) => DisplayChange(args.ChangeType, args.Name);
            watcher.Deleted += (sender, args) => DisplayChange(args.ChangeType, args.Name);
            watcher.Changed += (sender, args) => DisplayChange(args.ChangeType, args.Name);
            watcher.Renamed += (sender, args) => DisplayChange(args.ChangeType, args.Name, args.OldName);
            watcher.Error += (sender, args) => DisplayError(args.GetException());

            watcher.IncludeSubdirectories = true;
            watcher.Path = path;
            watcher.NotifyFilter = notifyFilters;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine($"Start monitoring changes on '{path}'");

            return watcher;
        }

        private static void DisplayChange(WatcherChangeTypes changeType, string name, string previousPathInRename = null)
        {
            string symbol = GetChangeSymbol(changeType);

            Console.WriteLine(changeType == WatcherChangeTypes.Renamed
                ? $"{symbol} [{Thread.CurrentThread.ManagedThreadId}] {previousPathInRename} => {name}"
                : $"{symbol} [{Thread.CurrentThread.ManagedThreadId}] {name}");
        }

        private static void DisplayError(Exception exception)
        {
            Console.WriteLine($"ERROR: [{Thread.CurrentThread.ManagedThreadId}] {exception.Message}");
        }

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
                case WatcherChangeTypes.Renamed:
                {
                    return "*";
                }
                default:
                {
                    return "?";
                }
            }
        }

        private sealed class ConcurrentSignal : IDisposable
        {
            private readonly EventWaitHandle waitHandle = new AutoResetEvent(false);

            public void SetComplete()
            {
                waitHandle.Set();
            }

            public void Dispose()
            {
                waitHandle.WaitOne();
            }
        }
    }
}
