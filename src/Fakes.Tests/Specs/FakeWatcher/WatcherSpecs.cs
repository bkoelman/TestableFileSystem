﻿namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public abstract class WatcherSpecs
    {
        protected const int NotifyWaitTimeoutMilliseconds = 1000;
        protected const int SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop = 250;

        // TODO: Add specs for File.Encrypt/Decrypt, File.Lock/Unlock, File.Replace, Begin+EndRead/Write
        // TODO: Add specs for Directory.GetLogicalDrives
        // TODO: Add specs for Drive.GetDrives
        // TODO: Add specs for Path.GetTempPath, Path.GetTempFileName
    }
}
