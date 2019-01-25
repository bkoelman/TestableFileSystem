namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public abstract class WatcherSpecs
    {
        protected const int NotifyWaitTimeoutMilliseconds = 500;
        protected const int SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop = 250;
    }
}
