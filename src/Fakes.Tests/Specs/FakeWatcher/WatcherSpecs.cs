#if !NETCOREAPP1_1
using System;
using System.Threading;
using FluentAssertions;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public abstract class WatcherSpecs
    {
        protected const int MaxTestDurationInMilliseconds = 1000;
        protected const int SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop = 250;

        protected static void BlockUntilChangeProcessed([NotNull] FakeFileSystemWatcher watcher, [NotNull] Action diskOperation)
        {
            using (var operationWaitHandle = new ManualResetEventSlim(false))
            {
                // ReSharper disable AccessToDisposedClosure
                watcher.Deleted += (sender, args) => operationWaitHandle.Set();
                watcher.Created += (sender, args) => operationWaitHandle.Set();
                watcher.Changed += (sender, args) => operationWaitHandle.Set();
                watcher.Renamed += (sender, args) => operationWaitHandle.Set();
                // ReSharper restore AccessToDisposedClosure

                diskOperation();

                bool waitSucceeded = operationWaitHandle.Wait(MaxTestDurationInMilliseconds);
                waitSucceeded.Should().BeTrue();
            }
        }
    }
}
#endif
