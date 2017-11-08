using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectory
{
    public sealed class DirectoryTimeUpdatesSpecs
    {
        [Fact]
        private void When_updating_attributes_it_must_not_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Directory | FileAttributes.Hidden);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_creating_file_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.File.WriteAllText(@"c:\folder\file.txt", "X");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastWriteTimeUtc);
        }

        [Fact]
        private void When_deleting_file_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.File.Delete(@"c:\folder\file.txt");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastWriteTimeUtc);
        }

        [Fact]
        private void When_renaming_file_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.File.Move(@"c:\folder\file.txt", @"c:\folder\newname.doc");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastWriteTimeUtc);
        }

        [Fact]
        private void When_copying_file_into_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .IncludingEmptyFile(@"c:\other\file.txt")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.File.Copy(@"c:\other\file.txt", @"c:\folder\file.txt");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastWriteTimeUtc);
        }

        [Fact]
        private void When_moving_file_into_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .IncludingEmptyFile(@"c:\other\file.txt")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.File.Move(@"c:\other\file.txt", @"c:\folder\file.txt");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastWriteTimeUtc);
        }

        [Fact]
        private void When_moving_file_out_of_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.File.Move(@"c:\folder\file.txt", @"c:\file.txt");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastWriteTimeUtc);
        }

        [Fact]
        private void When_updating_file_contents_it_must_not_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.File.AppendAllText(@"c:\folder\file.txt", "X");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_updating_file_attributes_and_time_it_must_not_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\file.txt")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.File.SetCreationTimeUtc(@"c:\folder\file.txt", lastWriteTimeUtc);
            fileSystem.File.SetAttributes(@"c:\folder\file.txt", FileAttributes.ReadOnly);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_creating_subdirectory_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.Directory.CreateDirectory(@"c:\folder\sub");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastWriteTimeUtc);
        }

        [Fact]
        private void When_deleting_subdirectory_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\folder\sub")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.Directory.Delete(@"c:\folder\sub");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastWriteTimeUtc);
        }

        [Fact]
        private void When_renaming_subdirectory_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\folder\sub")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.Directory.Move(@"c:\folder\sub", @"c:\folder\newname");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastWriteTimeUtc);
        }

        [Fact]
        private void When_moving_subdirectory_into_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .IncludingEmptyFile(@"c:\other\file.txt")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.Directory.Move(@"c:\other", @"c:\folder\other");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastWriteTimeUtc);
        }

        [Fact]
        private void When_moving_subdirectory_out_of_directory_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\sub\file.txt")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.Directory.Move(@"c:\folder\sub", @"c:\newname");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastWriteTimeUtc);
        }

        [Fact]
        private void When_updating_subdirectory_attributes_and_time_it_must_not_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\folder\sub")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.File.SetAttributes(@"c:\folder\sub", FileAttributes.Directory | FileAttributes.ReadOnly);
            fileSystem.Directory.SetCreationTimeUtc(@"c:\folder\sub", lastWriteTimeUtc);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_updating_contents_of_subdirectory_it_must_not_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\sub\file.txt")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            // Act
            fileSystem.File.Delete(@"c:\folder\sub\file.txt");
            fileSystem.File.WriteAllText(@"c:\folder\sub\new.txt", "X");
            fileSystem.File.SetAttributes(@"c:\folder\sub\new.txt", FileAttributes.Hidden);
            fileSystem.Directory.CreateDirectory(@"c:\folder\sub\more");

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_enumerating_entries_it_must_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\sub\file.txt")
                .Build();

            DateTime lastAccessTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            // Act
            fileSystem.Directory.GetFileSystemEntries(path);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastAccessTimeUtc);

            fileSystem.Directory.GetCreationTimeUtc(@"c:\folder\sub").Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(@"c:\folder\sub").Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(@"c:\folder\sub").Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_enumerating_entries_it_must_recursively_update_directory_timings()
        {
            // Arrange
            const string path = @"C:\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"c:\folder\sub\file.txt")
                .Build();

            DateTime lastAccessTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            // Act
            fileSystem.Directory.GetFileSystemEntries(path, searchOption: SearchOption.AllDirectories);

            // Assert
            fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(lastAccessTimeUtc);

            fileSystem.Directory.GetCreationTimeUtc(@"c:\folder\sub").Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastWriteTimeUtc(@"c:\folder\sub").Should().Be(creationTimeUtc);
            fileSystem.Directory.GetLastAccessTimeUtc(@"c:\folder\sub").Should().Be(lastAccessTimeUtc);
        }
    }
}
