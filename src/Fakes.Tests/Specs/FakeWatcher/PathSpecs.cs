#if !NETCOREAPP1_1
using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeWatcher
{
    public sealed class PathSpecs : WatcherSpecs
    {
        [Fact]
        private void When_setting_path_to_null_it_must_fail()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                watcher.Path = null;

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.EnableRaisingEvents = true;

                // Assert
                watcher.Path.Should().Be(string.Empty);
                action.Should().Throw<FileNotFoundException>().WithMessage("Error reading the  directory.");
            }
        }

        [Fact]
        private void When_setting_path_to_empty_string_it_must_fail()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                watcher.Path = string.Empty;

                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.EnableRaisingEvents = true;

                // Assert
                watcher.Path.Should().Be(string.Empty);
                action.Should().Throw<FileNotFoundException>().WithMessage("Error reading the  directory.");
            }
        }

        [Fact]
        private void When_setting_path_to_whitespace_it_must_fail()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.Path = " ";

                // Assert
                action.Should().Throw<ArgumentException>().WithMessage("The directory name   is invalid.");
            }
        }

        [Fact]
        private void When_setting_path_to_invalid_path_it_must_fail()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.Path = "::";

                // Assert
                action.Should().Throw<ArgumentException>().WithMessage("The directory name :: is invalid.");
            }
        }

        [Fact]
        private void When_setting_path_to_invalid_characters_it_must_fail()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.Path = @"c:\?";

                // Assert
                action.Should().Throw<ArgumentException>().WithMessage(@"The directory name c:\? is invalid.");
            }
        }

        [Fact]
        private void When_setting_path_to_missing_directory_it_must_fail()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.Path = @"c:\missing";

                // Assert
                action.Should().Throw<ArgumentException>().WithMessage(@"The directory name c:\missing is invalid.");
            }
        }

        [Fact]
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

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

        [Fact]
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

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

        [Fact]
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

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

        [Fact]
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

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

        [Fact]
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

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

        [Fact]
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

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

        [Fact]
        private void When_setting_path_to_existing_local_file_it_must_fail()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.Path = path;

                // Assert
                action.Should().Throw<ArgumentException>().WithMessage(@"The directory name c:\some\file.txt is invalid.");
            }
        }

        [Fact]
        private void When_setting_path_to_local_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.Path = path + @"\nested";

                // Assert
                action.Should().Throw<ArgumentException>().WithMessage(@"The directory name c:\some\file.txt\nested is invalid.");
            }
        }

        [Fact]
        private void When_setting_path_to_missing_local_directory_tree_it_must_fail()
        {
            // Arrange
            const string path = @"c:\some\folder";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.Path = path;

                // Assert
                action.Should().Throw<ArgumentException>().WithMessage(@"The directory name c:\some\folder is invalid.");
            }
        }

        [Fact]
        private void When_setting_path_to_missing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.Path = path;

                // Assert
                action.Should().Throw<ArgumentException>().WithMessage(@"The directory name \\server\share is invalid.");
            }
        }

        [Fact]
        private void When_setting_path_to_existing_network_share_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"\\server\share";
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

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

        [Fact]
        private void When_setting_path_to_missing_remote_directory_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\MissingFolder";

            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.Path = path;

                // Assert
                action.Should().Throw<ArgumentException>().WithMessage(@"The directory name \\server\share\MissingFolder is invalid.");
            }
        }

        [Fact]
        private void When_setting_path_to_existing_remote_directory_it_must_raise_events()
        {
            // Arrange
            const string directoryToWatch = @"\\server\share\Subfolder";
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

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

        [Fact]
        private void When_setting_path_to_reserved_name_it_must_fail()
        {
            // Arrange
            FakeFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            using (FakeFileSystemWatcher watcher = fileSystem.ConstructFileSystemWatcher())
            {
                // Act
                // ReSharper disable once AccessToDisposedClosure
                Action action = () => watcher.Path = "LPT1";

                // Assert
                action.Should().Throw<ArgumentException>().WithMessage(@"The directory name LPT1 is invalid.");
            }
        }

        [Fact]
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

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

        [Fact]
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(path);
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact]
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(@"c:\file.txt");
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact]
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

                    // Assert
                    listener.EventsCollected.Should().HaveCount(1);

                    FileSystemEventArgs args = listener.CreateEventArgsCollected.Single();
                    args.ChangeType.Should().Be(WatcherChangeTypes.Created);
                    args.FullPath.Should().Be(path);
                    args.Name.Should().Be("file.txt");
                }
            }
        }

        [Fact]
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

                    watcher.WaitForCompleted(NotifyWaitTimeoutMilliseconds);

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
