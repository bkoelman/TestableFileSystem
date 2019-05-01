using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Demo
{
    /// <summary>
    /// Displays incoming changes from <see cref="IFileSystemWatcher" /> on the console.
    /// </summary>
    internal sealed class FileSystemChangeDumper : IDisposable
    {
        private const NotifyFilters NotifyFiltersAll = NotifyFilters.FileName | NotifyFilters.DirectoryName |
            NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess |
            NotifyFilters.CreationTime | NotifyFilters.Security;

        private readonly IFileSystemWatcher watcher;
        private readonly Stopwatch stopwatch = new Stopwatch();

        public FileSystemChangeDumper(IFileSystem fileSystem)
        {
            watcher = fileSystem.ConstructFileSystemWatcher();
            watcher.Created += (sender, args) => DisplayChange(args.ChangeType, args.Name);
            watcher.Deleted += (sender, args) => DisplayChange(args.ChangeType, args.Name);
            watcher.Changed += (sender, args) => DisplayChange(args.ChangeType, args.Name);
            watcher.Renamed += (sender, args) => DisplayChange(args.ChangeType, args.Name, args.OldName);
            watcher.Error += (sender, args) => DisplayError(args.GetException());
        }

        public void Start(string path, NotifyFilters filters = NotifyFiltersAll)
        {
            watcher.EnableRaisingEvents = false;
            SetupWatcher(path, filters);
            watcher.EnableRaisingEvents = true;

            Thread.Sleep(250);
            stopwatch.Start();
        }

        private void SetupWatcher(string path, NotifyFilters filters)
        {
            Console.WriteLine("Started watching for changes in directory:");
            Console.WriteLine($"  \"{path}\"");

            if (filters != NotifyFiltersAll)
            {
                Console.WriteLine($"Filters: {filters}");
            }

            watcher.Path = path;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = filters;
        }

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            Console.WriteLine("Stopped watching for changes.");
        }

        public void Dispose()
        {
            Stop();
        }

        private void DisplayChange(WatcherChangeTypes changeType, string name, string previousPathInRename = null)
        {
            string symbol = GetChangeSymbol(changeType);

            Console.WriteLine(changeType == WatcherChangeTypes.Renamed
                ? $"[{stopwatch.Elapsed}] {symbol} {previousPathInRename} => {name}"
                : $"[{stopwatch.Elapsed}] {symbol} {name}");
        }

        private static void DisplayError(Exception exception)
        {
            Console.WriteLine($"! {exception.Message}");
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
