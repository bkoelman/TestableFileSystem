using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    // TODO: When a file in directory changes, its directory timestamps must also be updated. Ensure we have tests for that.

    public sealed class FileTimeUpdatesSpecs
    {
        private const string DefaultContents = "ABC";

        [Fact]
        private void When_updating_file_attributes_it_must_not_update_file_timings()
        {
            // Arrange
            const string path = @"C:\folder\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            DateTime changeTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => changeTimeUtc;

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Directory | FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_opening_existing_file_it_must_not_update_file_timings()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(path, DefaultContents)
                .Build();

            DateTime changeTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => changeTimeUtc;

            // Act
            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
            }

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_reading_from_existing_file_it_must_update_file_timings()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(path, DefaultContents)
                .Build();

            DateTime changeTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => changeTimeUtc;

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open))
            {
                stream.ReadByte();
            }

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(changeTimeUtc);
        }

        [Fact]
        private void When_writing_to_existing_file_it_must_update_file_timings()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            DateTime changeTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => changeTimeUtc;

            // Act
            fileSystem.File.WriteAllBytes(path, BufferFactory.SingleByte(0xFF));

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(changeTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(changeTimeUtc);
        }

        [Fact]
        private void When_appending_to_existing_file_it_must_update_file_timings()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            DateTime changeTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => changeTimeUtc;

            // Act
            fileSystem.File.AppendAllText(path, DefaultContents);

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(changeTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(changeTimeUtc);
        }

        [Fact]
        private void When_truncating_existing_file_it_must_update_file_timings()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(path, DefaultContents)
                .Build();

            DateTime changeTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => changeTimeUtc;

            // Act
            using (fileSystem.File.Open(path, FileMode.Truncate))
            {
            }

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(changeTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(changeTimeUtc);
        }

        [Fact]
        private void When_creating_file_it_must_update_file_timings()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            using (fileSystem.File.Create(path))
            {
            }

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_overwriting_existing_file_it_must_update_file_timings()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingTextFile(path, DefaultContents)
                .Build();

            DateTime changeTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => changeTimeUtc;

            // Act
            fileSystem.File.WriteAllBytes(path, BufferFactory.SingleByte(0xFF));

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(changeTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(changeTimeUtc);
        }

        [Fact]
        private void When_renaming_file_it_must_update_file_timings()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\newname.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"c:\some")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(sourcePath, DefaultContents);

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.File.ReadAllText(sourcePath);

            DateTime moveTimeUtc = 20.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => moveTimeUtc;

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.GetCreationTimeUtc(destinationPath).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(destinationPath).Should().Be(lastWriteTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(destinationPath).Should().Be(lastAccessTimeUtc);
        }

        [Fact]
        private void When_moving_file_it_must_update_file_timings()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\other\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"c:\other")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(sourcePath, DefaultContents);

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.File.ReadAllText(sourcePath);

            DateTime moveTimeUtc = 20.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => moveTimeUtc;

            // Act
            fileSystem.File.Move(sourcePath, destinationPath);

            // Assert
            fileSystem.File.GetCreationTimeUtc(destinationPath).Should().Be(creationTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(destinationPath).Should().Be(lastWriteTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(destinationPath).Should().Be(lastAccessTimeUtc);
        }

        [Fact]
        private void When_copying_file_it_must_update_file_timings()
        {
            // Arrange
            const string sourcePath = @"c:\file.txt";
            const string destinationPath = @"c:\copy.txt";

            var copyWaitIndicator = new WaitIndicator();

            DateTime sourceCreationTimeUtc = 4.January(2017).At(7, 52, 01).AsUtc();
            var clock = new SystemClock(() => sourceCreationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .WithCopyWaitIndicator(copyWaitIndicator)
                .IncludingEmptyFile(sourcePath)
                .Build();

            DateTime sourceLastWriteTimeUtc = 10.January(2017).At(3, 12, 34).AsUtc();
            clock.UtcNow = () => sourceLastWriteTimeUtc;

            fileSystem.File.WriteAllText(sourcePath, DefaultContents);

            DateTime destinationCreationTimeUtc = 12.January(2017).At(11, 23, 45).AsUtc();
            clock.UtcNow = () => destinationCreationTimeUtc;

            // Act
            var copyThread = new Thread(() => { fileSystem.File.Copy(sourcePath, destinationPath); });
            copyThread.Start();

            copyWaitIndicator.WaitForStart();

            try
            {
                // Assert (copy started)
                fileSystem.File.GetCreationTimeUtc(destinationPath).Should().Be(destinationCreationTimeUtc);
                fileSystem.File.GetLastAccessTimeUtc(destinationPath).Should().Be(destinationCreationTimeUtc);
                fileSystem.File.GetLastWriteTimeUtc(destinationPath).Should().Be(sourceLastWriteTimeUtc);

                DateTime destinationCompletedTimeUtc = 12.January(2017).At(11, 27, 36).AsUtc();
                clock.UtcNow = () => destinationCompletedTimeUtc;

                copyWaitIndicator.SetCompleted();
                copyThread.Join();

                // Assert (copy completed)
                fileSystem.File.GetCreationTimeUtc(destinationPath).Should().Be(destinationCreationTimeUtc);
                fileSystem.File.GetLastAccessTimeUtc(destinationPath).Should().Be(destinationCompletedTimeUtc);
                fileSystem.File.GetLastWriteTimeUtc(destinationPath).Should().Be(sourceLastWriteTimeUtc);

                fileSystem.File.GetCreationTimeUtc(sourcePath).Should().Be(sourceCreationTimeUtc);
                fileSystem.File.GetLastAccessTimeUtc(sourcePath).Should().Be(destinationCompletedTimeUtc);
                fileSystem.File.GetLastWriteTimeUtc(sourcePath).Should().Be(sourceLastWriteTimeUtc);
            }
            finally
            {
                copyWaitIndicator.SetCompleted();
                copyThread.Join();
            }
        }

        [Fact]
        private void When_copying_file_overwriting_existing_file_it_must_update_file_timings()
        {
            // Arrange
            const string sourcePath = @"c:\file.txt";
            const string destinationPath = @"c:\copy.txt";

            var copyWaitIndicator = new WaitIndicator();

            DateTime creationTimeUtc = 4.January(2017).At(7, 52, 01).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .WithCopyWaitIndicator(copyWaitIndicator)
                .IncludingEmptyFile(sourcePath)
                .IncludingEmptyFile(destinationPath)
                .Build();

            DateTime sourceLastWriteTimeUtc = 10.January(2017).At(3, 12, 34).AsUtc();
            clock.UtcNow = () => sourceLastWriteTimeUtc;

            fileSystem.File.WriteAllText(sourcePath, DefaultContents);

            DateTime destinationLastWriteTimeUtc = 12.January(2017).At(11, 23, 45).AsUtc();
            clock.UtcNow = () => destinationLastWriteTimeUtc;

            // Act
            var copyThread = new Thread(() => { fileSystem.File.Copy(sourcePath, destinationPath, true); });
            copyThread.Start();

            copyWaitIndicator.WaitForStart();

            try
            {
                // Assert (copy started)
                fileSystem.File.GetCreationTimeUtc(destinationPath).Should().Be(creationTimeUtc);
                fileSystem.File.GetLastAccessTimeUtc(destinationPath).Should().Be(destinationLastWriteTimeUtc);
                fileSystem.File.GetLastWriteTimeUtc(destinationPath).Should().Be(sourceLastWriteTimeUtc);

                DateTime destinationCompletedTimeUtc = 12.January(2017).At(11, 27, 36).AsUtc();
                clock.UtcNow = () => destinationCompletedTimeUtc;

                copyWaitIndicator.SetCompleted();
                copyThread.Join();

                // Assert (copy completed)
                fileSystem.File.GetCreationTimeUtc(destinationPath).Should().Be(creationTimeUtc);
                fileSystem.File.GetLastAccessTimeUtc(destinationPath).Should().Be(destinationCompletedTimeUtc);
                fileSystem.File.GetLastWriteTimeUtc(destinationPath).Should().Be(sourceLastWriteTimeUtc);

                fileSystem.File.GetCreationTimeUtc(sourcePath).Should().Be(creationTimeUtc);
                fileSystem.File.GetLastAccessTimeUtc(sourcePath).Should().Be(destinationCompletedTimeUtc);
                fileSystem.File.GetLastWriteTimeUtc(sourcePath).Should().Be(sourceLastWriteTimeUtc);
            }
            finally
            {
                copyWaitIndicator.SetCompleted();
                copyThread.Join();
            }
        }

        // TODO: Add extra specs for operations added in NetStandard20.
    }
}
