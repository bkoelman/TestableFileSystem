using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FakeFileTimeSpecs
    {
        private static readonly DateTime ZeroFileTime = 1.January(1601).AsUtc().ToLocalTime();
        private static readonly DateTime ZeroFileTimeUtc = 1.January(1601).AsUtc();

        [Fact]
        private void When_getting_file_creation_time_for_missing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            DateTime creationTime = fileSystem.File.GetCreationTime(path);
            DateTime creationTimeUtc = fileSystem.File.GetCreationTimeUtc(path);

            // Assert
            creationTime.Should().Be(ZeroFileTime);
            creationTimeUtc.Should().Be(ZeroFileTimeUtc);
        }

        [Fact(Skip = "TODO")]
        private void When_getting_file_creation_time_in_local_timezone_for_directory_it_must_succeed()
        {
            // Arrange
            DateTime creationTimeLocal = 3.February(2017).At(12, 34, 56, 777).AsLocal();
            SystemClock.Now = () => creationTimeLocal;

            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime timeLocal = fileSystem.File.GetCreationTime(path);

            // Assert
            timeLocal.Should().Be(creationTimeLocal);
        }

        [Fact(Skip = "TODO")]
        private void When_getting_file_creation_time_in_utc_for_directory_it_must_succeed()
        {
            // Arrange
            DateTime creationTimeUtc = 4.February(2017).At(12, 34, 56, 777).AsUtc();
            SystemClock.UtcNow = () => creationTimeUtc;

            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime timeUtc = fileSystem.File.GetCreationTimeUtc(path);

            // Assert
            timeUtc.Should().Be(creationTimeUtc);
        }

        [Fact]
        private void When_getting_file_creation_time_for_file_below_existing_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            DateTime creationTime = fileSystem.File.GetCreationTime(@"c:\some\file.txt\subfolder");
            DateTime creationTimeUtc = fileSystem.File.GetCreationTimeUtc(@"c:\some\file.txt\subfolder");

            // Assert
            creationTime.Should().Be(ZeroFileTime);
            creationTimeUtc.Should().Be(ZeroFileTimeUtc);
        }

        [Fact]
        private void When_setting_file_creation_time_in_local_timezone_it_must_succeed()
        {
            // Arrange
            DateTime creationTime = 21.January(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetCreationTime(path, creationTime);

            // Assert
            fileSystem.File.GetCreationTime(path).Should().Be(creationTime);
            fileSystem.File.GetCreationTimeUtc(path).Should().NotBe(creationTime);
        }

        [Fact]
        private void When_setting_file_creation_time_in_utc_it_must_succeed()
        {
            // Arrange
            DateTime creationTimeUtc = 21.January(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetCreationTimeUtc(path, creationTimeUtc);

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTimeUtc);
            fileSystem.File.GetCreationTime(path).Should().NotBe(creationTimeUtc);
        }

        [Fact]
        private void When_setting_file_creation_time_in_local_timezone_for_directory_it_must_fail()
        {
            // Arrange
            DateTime creationTimeLocal = 5.February(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"c:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetCreationTime(path, creationTimeLocal.AddHours(3));

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'c:\some\subfolder' is denied.");
        }

        [Fact]
        private void When_setting_file_creation_time_in_utc_for_directory_it_must_fail()
        {
            // Arrange
            DateTime creationTimeUtc = 6.February(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"c:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetCreationTimeUtc(path, creationTimeUtc.AddHours(3));

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'c:\some\subfolder' is denied.");
        }

        [Fact]
        private void When_setting_file_creation_time_for_file_below_existing_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetCreationTime(@"c:\some\file.txt\subfolder", SystemClock.UtcNow());

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'c:\some\file.txt\subfolder'.");
        }

        // ***

        [Fact]
        private void When_getting_file_last_write_time_for_missing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            DateTime lastWriteTime = fileSystem.File.GetLastWriteTime(path);
            DateTime lastWriteTimeUtc = fileSystem.File.GetLastWriteTimeUtc(path);

            // Assert
            lastWriteTime.Should().Be(ZeroFileTime);
            lastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);
        }

        [Fact(Skip = "TODO")]
        private void When_getting_file_last_write_time_in_local_timezone_for_directory_it_must_succeed()
        {
            // Arrange
            DateTime lastWriteTimeLocal = 7.February(2017).At(12, 34, 56, 777).AsLocal();
            SystemClock.Now = () => lastWriteTimeLocal;

            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime timeLocal = fileSystem.File.GetLastWriteTime(path);

            // Assert
            timeLocal.Should().Be(lastWriteTimeLocal);
        }

        [Fact(Skip = "TODO")]
        private void When_getting_file_last_write_time_in_utc_for_directory_it_must_succeed()
        {
            // Arrange
            DateTime lastWriteTimeUtc = 8.February(2017).At(12, 34, 56, 777).AsUtc();
            SystemClock.UtcNow = () => lastWriteTimeUtc;

            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime timeUtc = fileSystem.File.GetLastWriteTimeUtc(path);

            // Assert
            timeUtc.Should().Be(lastWriteTimeUtc);
        }

        [Fact]
        private void When_setting_file_last_write_time_in_local_timezone_it_must_succeed()
        {
            // Arrange
            DateTime lastWriteTime = 22.January(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTime(path, lastWriteTime);

            // Assert
            fileSystem.File.GetLastWriteTime(path).Should().Be(lastWriteTime);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().NotBe(lastWriteTime);
        }

        [Fact]
        private void When_setting_file_last_write_time_in_utc_it_must_succeed()
        {
            // Arrange
            DateTime lastWriteTimeUtc = 22.January(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(lastWriteTimeUtc);
            fileSystem.File.GetLastWriteTime(path).Should().NotBe(lastWriteTimeUtc);
        }

        [Fact]
        private void When_setting_file_last_write_time_in_local_timezone_for_directory_it_must_fail()
        {
            // Arrange
            DateTime lastWriteTimeLocal = 9.February(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"c:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTime(path, lastWriteTimeLocal.AddHours(3));

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'c:\some\subfolder' is denied.");
        }

        [Fact]
        private void When_setting_file_last_write_time_in_utc_for_directory_it_must_fail()
        {
            // Arrange
            DateTime lastWriteTimeUtc = 10.February(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"c:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastWriteTimeUtc(path, lastWriteTimeUtc.AddHours(3));

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'c:\some\subfolder' is denied.");
        }

        // ***

        [Fact]
        private void When_getting_file_last_access_time_for_missing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            DateTime lastAccessTime = fileSystem.File.GetLastAccessTime(path);
            DateTime lastAccessTimeUtc = fileSystem.File.GetLastAccessTimeUtc(path);

            // Assert
            lastAccessTime.Should().Be(ZeroFileTime);
            lastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
        }

        [Fact(Skip = "TODO")]
        private void When_getting_file_last_access_time_in_local_timezone_for_directory_it_must_succeed()
        {
            // Arrange
            DateTime lastAccessTimeLocal = 11.February(2017).At(12, 34, 56, 777).AsLocal();
            SystemClock.Now = () => lastAccessTimeLocal;

            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime timeLocal = fileSystem.File.GetLastAccessTime(path);

            // Assert
            timeLocal.Should().Be(lastAccessTimeLocal);
        }

        [Fact(Skip = "TODO")]
        private void When_getting_file_last_access_time_in_utc_for_directory_it_must_succeed()
        {
            // Arrange
            DateTime lastAccessTimeUtc = 12.February(2017).At(12, 34, 56, 777).AsUtc();
            SystemClock.UtcNow = () => lastAccessTimeUtc;

            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            DateTime timeUtc = fileSystem.File.GetLastAccessTimeUtc(path);

            // Assert
            timeUtc.Should().Be(lastAccessTimeUtc);
        }

        [Fact]
        private void When_setting_file_last_access_time_in_local_timezone_it_must_succeed()
        {
            // Arrange
            DateTime lastAccessTime = 23.January(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastAccessTime(path, lastAccessTime);

            // Assert
            fileSystem.File.GetLastAccessTime(path).Should().Be(lastAccessTime);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().NotBe(lastAccessTime);
        }

        [Fact]
        private void When_setting_file_last_access_time_in_utc_it_must_succeed()
        {
            // Arrange
            DateTime lastAccessTimeUtc = 23.January(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

            // Assert
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(lastAccessTimeUtc);
            fileSystem.File.GetLastAccessTime(path).Should().NotBe(lastAccessTimeUtc);
        }

        [Fact]
        private void When_setting_file_last_access_time_in_local_timezone_for_directory_it_must_fail()
        {
            // Arrange
            DateTime lastAccessTimeLocal = 13.February(2017).At(12, 34, 56, 777).AsLocal();

            const string path = @"c:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastAccessTime(path, lastAccessTimeLocal.AddHours(3));

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'c:\some\subfolder' is denied.");
        }

        [Fact]
        private void When_setting_file_last_access_time_in_utc_for_directory_it_must_fail()
        {
            // Arrange
            DateTime lastAccessTimeUtc = 14.February(2017).At(12, 34, 56, 777).AsUtc();

            const string path = @"c:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetLastAccessTimeUtc(path, lastAccessTimeUtc.AddHours(3));

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'c:\some\subfolder' is denied.");
        }

        // TODO: Add missing specs.

        [Fact]
        private void When_creating_file_it_must_update_timings()
        {
            // Arrange
            const string path = @"C:\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            DateTime createTimeUtc = 1.January(2017).At(22, 14);
            SystemClock.UtcNow = () => createTimeUtc;

            // Act
            using (fileSystem.File.Create(path))
            {
            }

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(createTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(createTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(createTimeUtc);
        }

        [Fact]
        private void When_writing_to_existing_file_it_must_update_properties_only_after_closing()
        {
            // Arrange
            const string path = @"C:\file.txt";

            DateTime createTimeUtc = 2.January(2017).At(22, 14);
            SystemClock.UtcNow = () => createTimeUtc;

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime writeTimeUtc = 3.January(2017).At(23, 11);

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open))
            {
                SystemClock.UtcNow = () => writeTimeUtc;

                stream.WriteByte(0x20);

                // Assert
                fileSystem.File.GetCreationTimeUtc(path).Should().Be(createTimeUtc);
                fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(createTimeUtc);
                fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(createTimeUtc);
                fileInfo.Length.Should().Be(0);
            }

            fileSystem.File.GetCreationTimeUtc(path).Should().Be(createTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(writeTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(writeTimeUtc);
            fileInfo.Length.Should().Be(1);
        }

        [Fact]
        private void When_reading_from_existing_file_it_must_update_properties_only_after_closing()
        {
            // Arrange
            const string path = @"C:\file.txt";

            DateTime createTimeUtc = 4.January(2017).At(22, 14);
            SystemClock.UtcNow = () => createTimeUtc;

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "X")
                .Build();

            DateTime accessTimeUtc = 5.January(2017).At(23, 11);
            SystemClock.UtcNow = () => accessTimeUtc;

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
                stream.ReadByte();

                // Assert
                fileSystem.File.GetCreationTimeUtc(path).Should().Be(createTimeUtc);
                fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(createTimeUtc);
                fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(createTimeUtc);
            }

            fileSystem.File.GetCreationTimeUtc(path).Should().Be(createTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(createTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(accessTimeUtc);
        }

        [Fact]
        private void When_opening_existing_file_it_must_not_update_timings_after_closing()
        {
            // Arrange
            const string path = @"C:\file.txt";

            DateTime createTimeUtc = 4.January(2017).At(22, 14);
            SystemClock.UtcNow = () => createTimeUtc;

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "X")
                .Build();

            DateTime accessTimeUtc = 5.January(2017).At(23, 11);
            SystemClock.UtcNow = () => accessTimeUtc;

            // Act
            using (fileSystem.File.Open(path, FileMode.Open, FileAccess.Read))
            {
            }

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(createTimeUtc);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(createTimeUtc);
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(createTimeUtc);
        }

        [Fact]
        private void When_updating_existing_file_on_background_thread_its_property_changes_must_be_observable_from_main_thread()
        {
            // Arrange
            const string path = @"C:\file.txt";

            DateTime createTimeUtc = 31.January(2017).At(22, 14);
            SystemClock.UtcNow = () => createTimeUtc;

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            DateTime accessTimeUtcBefore = fileSystem.File.GetLastAccessTimeUtc(path);
            DateTime writeTimeUtcBefore = fileSystem.File.GetLastWriteTimeUtc(path);
            long sizeBefore = fileInfo.Length;

            // Act
            Task.Factory.StartNew(() =>
            {
                SystemClock.UtcNow = () => createTimeUtc.AddSeconds(1);
                fileSystem.File.WriteAllText(path, "Y");
            }).Wait();

            SystemClock.UtcNow = () => createTimeUtc.AddSeconds(2);

            DateTime accessTimeUtcAfter = fileSystem.File.GetLastAccessTimeUtc(path);
            DateTime writeTimeUtcAfter = fileSystem.File.GetLastWriteTimeUtc(path);
            long sizeAfter = fileInfo.Length;

            // Assert
            accessTimeUtcAfter.Should().BeAfter(accessTimeUtcBefore);
            writeTimeUtcAfter.Should().BeAfter(writeTimeUtcBefore);
            sizeAfter.Should().BeGreaterThan(sizeBefore);
        }
    }
}
