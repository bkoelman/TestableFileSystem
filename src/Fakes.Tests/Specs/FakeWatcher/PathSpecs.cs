#if !NETCOREAPP1_1
using System;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class PathSpecs : WatcherSpecs
    {
        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_path_to_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    watcher.Path = null;

                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => watcher.EnableRaisingEvents = true;

                    // Assert
                    watcher.Path.Should().Be(string.Empty);
                    action.Should().ThrowExactly<FileNotFoundException>().WithMessage("Error reading the  directory.");
                }
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_path_to_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
                {
                    watcher.Path = string.Empty;

                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => watcher.EnableRaisingEvents = true;

                    // Assert
                    watcher.Path.Should().Be(string.Empty);
                    action.Should().ThrowExactly<FileNotFoundException>().WithMessage("Error reading the  directory.");
                }
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_path_to_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => watcher.Path = " ";

                    // Assert
                    action.Should().ThrowExactly<ArgumentException>().WithMessage("The directory name ' ' does not exist.*");
                }
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_path_to_invalid_drive_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => watcher.Path = "_:";

                    // Assert
                    action.Should().ThrowExactly<ArgumentException>().WithMessage("The directory name '_:' does not exist.*");
                }
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_path_to_wildcard_characters_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => watcher.Path = @"c:\?";

                    // Assert
                    action.Should().ThrowExactly<ArgumentException>().WithMessage(@"The directory name 'c:\?' does not exist.*");
                }
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_path_to_missing_directory_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(@"c:\missing");

                IFileSystem fileSystem = factory.Create()
                    .Build();

                using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => watcher.Path = path;

                    // Assert
                    action.Should().ThrowExactly<ArgumentException>().WithMessage($@"The directory name '{path}' does not exist.*");
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_path_to_existing_local_directory_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                // Act
                watcher.Path = directoryToWatch;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToUpdate);
                    args.Name.Should().Be(fileNameToUpdate);
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_path_to_drive_root_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\";
            const string fileNameToUpdate = "file.txt";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                // Act
                watcher.Path = directoryToWatch;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToUpdate);
                    args.Name.Should().Be(fileNameToUpdate);
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_path_to_existing_local_directory_with_trailing_whitespace_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                // Act
                watcher.Path = directoryToWatch + " ";

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToUpdate);
                    args.Name.Should().Be(fileNameToUpdate);
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_path_to_existing_local_directory_with_different_casing_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"C:\SOME";
            const string pathToFileToUpdate = @"c:\some\FILE.TXT";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                // Act
                watcher.Path = directoryToWatch;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(@"C:\SOME\file.txt");
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_path_to_absolute_path_without_drive_letter_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\");

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                // Act
                watcher.Path = @"\some";

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToUpdate);
                    args.Name.Should().Be(fileNameToUpdate);
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_path_to_relative_local_directory_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";
            const string fileNameToUpdate = "file.txt";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\");

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                // Act
                watcher.Path = "some";

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToUpdate);
                    args.Name.Should().Be(fileNameToUpdate);
                }
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_path_to_existing_local_file_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(@"c:\some\file.txt");

                IFileSystem fileSystem = factory.Create()
                    .IncludingEmptyFile(path)
                    .Build();

                using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => watcher.Path = path;

                    // Assert
                    action.Should().ThrowExactly<ArgumentException>().WithMessage($@"The directory name '{path}' does not exist.*");
                }
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_path_to_local_parent_directory_that_exists_as_file_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(@"c:\some\file.txt");

                IFileSystem fileSystem = factory.Create()
                    .IncludingEmptyFile(path)
                    .Build();

                using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => watcher.Path = path + @"\nested";

                    // Assert
                    action.Should().ThrowExactly<ArgumentException>().WithMessage($@"The directory name '{path}\nested' does not exist.*");
                }
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_path_to_missing_local_directory_tree_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(@"c:\some\folder");

                IFileSystem fileSystem = factory.Create()
                    .Build();

                using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => watcher.Path = path;

                    // Assert
                    action.Should().ThrowExactly<ArgumentException>().WithMessage($@"The directory name '{path}' does not exist.*");
                }
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_path_to_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = PathFactory.NetworkShare();

                IFileSystem fileSystem = factory.Create()
                    .Build();

                using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => watcher.Path = path;

                    // Assert
                    action.Should().ThrowExactly<ArgumentException>().WithMessage($"The directory name '{path}' does not exist.*");
                }
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_path_to_file_below_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkFileAtDepth(1), false);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => watcher.Path = path;

                    // Assert
                    action.Should().ThrowExactly<ArgumentException>().WithMessage($"The directory name '{path}' does not exist.*");
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_path_to_existing_network_share_it_must_raise_events()
        {
            // Arrange
            string directoryToWatch = PathFactory.NetworkShare();
            const string fileNameToUpdate = "file.txt";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                // Act
                watcher.Path = directoryToWatch;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToUpdate);
                    args.Name.Should().Be(fileNameToUpdate);
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_path_to_missing_remote_directory_it_must_fail()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(PathFactory.NetworkShare())
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.Path = path;

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage(
                    $"The directory name '{path}' does not exist.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_path_to_existing_remote_directory_it_must_raise_events()
        {
            // Arrange
            string directoryToWatch = PathFactory.NetworkDirectoryAtDepth(1);
            const string fileNameToUpdate = "file.txt";

            string pathToFileToUpdate = Path.Combine(directoryToWatch, fileNameToUpdate);

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                // Act
                watcher.Path = directoryToWatch;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(pathToFileToUpdate);
                    args.Name.Should().Be(fileNameToUpdate);
                }
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_path_to_reserved_name_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                using (IFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
                {
                    // Act
                    // ReSharper disable once AccessToDisposedClosure
                    Action action = () => watcher.Path = "LPT1";

                    // Assert
                    action.Should().ThrowExactly<ArgumentException>().WithMessage(@"The directory name 'LPT1' does not exist.*");
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_path_to_extended_local_directory_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"\\?\c:\some";
            const string pathToFileToUpdate = @"c:\some\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;

                // Act
                watcher.Path = directoryToWatch;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    fileSystem.File.SetAttributes(pathToFileToUpdate, FileAttributes.ReadOnly);

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);
                    listener.ChangeEventArgsCollected.Should().HaveSameCount(listener.EventsCollected);

                    FileSystemEventArgs args = listener.ChangeEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Changed);
                    args.FullPath.Should().Be(@"\\?\c:\some\file.txt");
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_changing_path_on_running_watcher_it_must_discard_old_notifications_and_restart()
        {
            // Arrange
            const string directoryToWatch1 = @"c:\some";
            const string directoryToWatch2 = @"c:\some\deeper";

            string pathToFileToUpdate1 = Path.Combine(directoryToWatch2, "file1.txt");
            string pathToFileToUpdate2 = Path.Combine(directoryToWatch2, "file2.txt");

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(pathToFileToUpdate1)
                .IncludingEmptyFile(pathToFileToUpdate2)
                .Build();

            var lockObject = new object();
            bool isFirstEventInvocation = true;
            FileSystemEventArgs argsAfterRestart = null;

            var resumeEventHandlerWaitHandle = new ManualResetEventSlim(false);
            var testCompletionWaitHandle = new ManualResetEventSlim(false);

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher(directoryToWatch1))
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.IncludeSubdirectories = true;
                watcher.Changed += (sender, args) =>
                {
                    lock (lockObject)
                    {
                        if (isFirstEventInvocation)
                        {
                            // Wait for all change notifications on file1.txt and file2.txt to queue up.
                            resumeEventHandlerWaitHandle.Wait(MaxTestDurationInMilliseconds);
                            isFirstEventInvocation = false;
                        }
                        else
                        {
                            // After event handler for first change on file1 has completed, no additional
                            // changes on file1.txt should be raised because they have become outdated.
                            argsAfterRestart = args;
                            testCompletionWaitHandle.Set();
                        }
                    }
                };
                watcher.EnableRaisingEvents = true;

                fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.Hidden);
                fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.ReadOnly);
                fileSystem.File.SetAttributes(pathToFileToUpdate1, FileAttributes.System);
                Thread.Sleep(SleepTimeToEnsureOperationHasArrivedAtWatcherConsumerLoop);

                // Act
                watcher.Path = directoryToWatch2;

                fileSystem.File.SetAttributes(pathToFileToUpdate2, FileAttributes.Hidden);

                resumeEventHandlerWaitHandle.Set();
                testCompletionWaitHandle.Wait(MaxTestDurationInMilliseconds);

                lock (lockObject)
                {
                    // Assert
                    FileSystemEventArgs argsAfterRestartNotNull = argsAfterRestart.ShouldNotBeNull();
                    argsAfterRestartNotNull.Name.Should().Be("file2.txt");
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_path_on_disposed_watcher_it_must_succeed()
        {
            // Arrange
            const string directoryToWatch = @"c:\some";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directoryToWatch)
                .Build();

            FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher();
            watcher.Dispose();

            // Act
            watcher.Path = directoryToWatch;

            // Assert
            watcher.Path.Should().Be(directoryToWatch);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_local_file_with_trailing_whitespace_it_must_raise_event()
        {
            // Arrange
            const string directory = @"c:\some";
            const string path = @"c:\some\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.Path = directory;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(path + "  "))
                    {
                    }

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(path);
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_file_using_absolute_path_without_drive_letter_it_must_raise_event()
        {
            // Arrange
            const string directory = @"c:\some";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directory)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(directory);

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.Path = @"c:\";

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(@"\file.txt"))
                    {
                    }

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(@"c:\file.txt");
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_relative_local_file_it_must_raise_event()
        {
            // Arrange
            const string directory = @"c:\some";
            const string path = @"c:\some\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directory)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(directory);

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.Path = directory;

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create("file.txt"))
                    {
                    }

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(path);
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_extended_local_file_it_must_raise_event()
        {
            // Arrange
            const string directory = @"\\?\C:\folder";
            const string path = @"\\?\C:\folder\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(directory)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.NotifyFilter = TestNotifyFilters.All;
                watcher.Path = @"C:\folder";

                using (var listener = new FileSystemWatcherEventListener(watcher))
                {
                    // Act
                    using (fileSystem.File.Create(path))
                    {
                    }

                    watcher.FinishAndWaitForFlushed(MaxTestDurationInMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(@"C:\folder\file.txt");
                    args.Name.Should().Be("file.txt");
                }
            }
        }
    }
}
#endif
