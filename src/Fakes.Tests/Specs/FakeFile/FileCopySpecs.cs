using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileCopySpecs
    {
        [Fact]
        private void When_copying_file_for_null_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.Copy(null, @"c:\destination.txt");

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_copying_file_for_null_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.File.Copy(@"c:\source.txt", null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_copying_file_for_empty_string_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(string.Empty, @"c:\destination.txt");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Empty file name is not legal.*");
        }

        [Fact]
        private void When_copying_file_for_empty_string_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"c:\source.txt", string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Empty file name is not legal.*");
        }

        [Fact]
        private void When_copying_file_for_whitespace_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(" ", @"c:\destination.txt");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_copying_file_for_whitespace_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"c:\source.txt", " ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_copying_file_for_invalid_source_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy("::", @"c:\destination.txt");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_copying_file_for_invalid_destination_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"c:\source.txt", "::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_copying_file_for_invalid_characters_in_source_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy("some?.txt", @"c:\destination.txt");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_copying_file_for_invalid_characters_in_destination_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"c:\source.txt", "some?.txt");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_copying_file_to_same_location_it_must_fail()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(path, path);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The file 'c:\some\file.txt' already exists.");
        }

        [Fact]
        private void When_copying_file_to_overwrite_same_location_it_must_fail()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(path, path, true);

            // Assert
            action.ShouldThrow<IOException>()
                .WithMessage(
                    @"The process cannot access the file 'c:\some\file.txt' because it is being used by another process.");
        }

        [Fact]
        private void When_copying_file_to_same_directory_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\copy.txt";
            const FileAttributes attributes = FileAttributes.Hidden | FileAttributes.System;

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SRC", attributes: attributes)
                .Build();

            // Act
            fileSystem.File.Copy(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeTrue();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
            fileSystem.File.GetAttributes(destinationPath).Should().Be(attributes);
            fileSystem.File.ReadAllText(destinationPath).Should().Be("SRC");

            IFileInfo info = fileSystem.ConstructFileInfo(destinationPath);
            info.Length.Should().Be(3);
        }

        [Fact]
        private void When_copying_file_to_existing_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\copy.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingEmptyFile(destinationPath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(sourcePath, destinationPath);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The file 'C:\some\copy.txt' already exists.");
        }

        [Fact]
        private void When_copying_file_to_same_directory_it_must_overwrite_existing()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\copy.txt";
            const FileAttributes attributes = FileAttributes.Hidden;

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SOURCE", attributes: attributes)
                .IncludingTextFile(destinationPath, "OLD")
                .Build();

            // Act
            fileSystem.File.Copy(sourcePath, destinationPath, true);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeTrue();
            fileSystem.File.GetAttributes(destinationPath).Should().Be(attributes);
            fileSystem.File.ReadAllText(destinationPath).Should().Be("SOURCE");

            IFileInfo info = fileSystem.ConstructFileInfo(destinationPath);
            info.Length.Should().Be(6);
        }

        [Fact]
        private void When_copying_file_to_different_directory_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"X:\path\to\folder\copy.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"X:\path\to\folder")
                .Build();

            // Act
            fileSystem.File.Copy(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeTrue();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_copying_file_to_different_directory_it_must_overwrite_existing()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"X:\path\to\folder\copy.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "SOURCE")
                .IncludingTextFile(destinationPath, "OLD")
                .Build();

            // Act
            fileSystem.File.Copy(sourcePath, destinationPath, true);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeTrue();
            fileSystem.File.ReadAllText(destinationPath).Should().Be("SOURCE");

            IFileInfo info = fileSystem.ConstructFileInfo(destinationPath);
            info.Length.Should().Be(6);
        }

        [Fact]
        private void When_copying_file_to_overwrite_readonly_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\copy.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingEmptyFile(destinationPath, FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(sourcePath, destinationPath, true);

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\copy.txt' is denied.");
        }

        [Fact]
        private void When_copying_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\old.txt")
                .Build();

            // Act
            fileSystem.File.Copy(@"C:\some\old.txt  ", @"C:\some\new.txt  ");

            // Assert
            fileSystem.File.Exists(@"C:\some\old.txt").Should().BeTrue();
            fileSystem.File.Exists(@"C:\some\new.txt").Should().BeTrue();
        }

        [Fact]
        private void When_copying_file_from_relative_path_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"D:\other\folder\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"D:\other\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\");

            // Act
            fileSystem.File.Copy(@"some\file.txt", destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeTrue();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_copying_file_to_relative_path_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"D:\other\folder\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"D:\other\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            fileSystem.File.Copy(sourcePath, @"folder\newname.doc");

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeTrue();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_copying_relative_file_on_different_drive_in_root_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\")
                .IncludingDirectory(@"D:\other")
                .IncludingEmptyFile(@"D:\source.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\");

            // Act
            fileSystem.File.Copy("D:source.txt", "D:destination.txt");

            // Assert
            fileSystem.File.Exists(@"D:\source.txt").Should().BeTrue();
            fileSystem.File.Exists(@"D:\destination.txt").Should().BeTrue();
        }

        [Fact]
        private void When_copying_relative_file_on_same_drive_in_subfolder_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"D:\other\source.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"D:\other");

            // Act
            fileSystem.File.Copy("D:source.txt", "D:destination.txt");

            // Assert
            fileSystem.File.Exists(@"d:\other\source.txt").Should().BeTrue();
            fileSystem.File.Exists(@"d:\other\destination.txt").Should().BeTrue();
        }

        [Fact]
        private void When_copying_file_from_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some\subfolder")
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"C:\some\subfolder", "newname.doc");

            // Assert
            action.ShouldThrow<UnauthorizedAccessException>().WithMessage(@"Access to the path 'C:\some\subfolder' is denied.");
        }

        [Fact]
        private void When_copying_file_to_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationDirectory = @"C:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(destinationDirectory)
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(sourcePath, destinationDirectory);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The target file 'C:\some\folder' is a directory, not a file.");
        }

        [Fact]
        private void When_copying_file_from_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"C:\some\file.txt\other.txt", "newname.doc");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\file.txt\other.txt'.");
        }

        [Fact]
        private void When_copying_file_to_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .IncludingEmptyFile(@"C:\some\newname.doc")
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"C:\some\file.txt", @"C:\some\newname.doc\other.doc");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\newname.doc\other.doc'.");
        }

        [Fact]
        private void When_copying_file_from_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"C:\some\file.txt\other.txt\deeper.txt", "newname.doc");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\file.txt\other.txt\deeper.txt'.");
        }

        [Fact]
        private void When_copying_file_to_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .IncludingEmptyFile(@"C:\some\newname.doc")
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"C:\some\file.txt", @"C:\some\newname.doc\other.doc\deeper.doc");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\newname.doc\other.doc\deeper.doc'.");
        }

        [Fact]
        private void When_copying_file_from_missing_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"C:\some\file.txt", @"C:\some\newname.doc");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\some\file.txt'.");
        }

        [Fact]
        private void When_copying_file_to_missing_directory_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\other\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(sourcePath, destinationPath);

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage(@"Could not find a part of the path 'C:\other\newname.doc'.");
        }

        [Fact]
        private void When_copying_missing_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"c:\some\file.txt", @"c:\newname.doc");

            // Assert
            action.ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'c:\some\file.txt'.");
        }

        [Fact]
        private void When_copying_file_to_parent_directory_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\some\level\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.File.Copy(sourcePath, "copy.nfo");

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeTrue();
            fileSystem.File.Exists(@"C:\some\copy.nfo").Should().BeTrue();
        }

        [Fact]
        private void When_copying_file_that_is_root_of_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"C:\", @"d:\newname.doc");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'C:\'.");
        }

        [Fact]
        private void When_copying_file_to_root_of_drive_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(sourcePath, @"C:\");

            // Assert
            action.ShouldThrow<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'C:\'.");
        }

        [Fact]
        private void When_copying_an_open_file_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\some\file.txt";
            const string destinationPath = @"C:\some\newname.doc";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            using (fileSystem.File.Open(sourcePath, FileMode.Open, FileAccess.Read))
            {
                // Act
                Action action = () => fileSystem.File.Copy(sourcePath, destinationPath);

                // Assert
                action.ShouldThrow<IOException>()
                    .WithMessage("The process cannot access the file because it is being used by another process.");
                fileSystem.File.Exists(sourcePath).Should().BeTrue();
                fileSystem.File.Exists(destinationPath).Should().BeFalse();
            }
        }

        [Fact]
        private void When_copying_file_from_missing_network_share_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"\\teamserver\documents\for-all.txt";
            const string destinationPath = @"C:\docs\mine.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\docs")
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(sourcePath, destinationPath);

            // Assert
            action.ShouldThrow<IOException>().WithMessage(@"The network path was not found");
        }

        [Fact]
        private void When_copying_file_to_missing_network_share_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"C:\docs\mine.txt";
            const string destinationPath = @"\\teamserver\documents\for-all.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(sourcePath, destinationPath);

            // Assert
            action.ShouldThrow<IOException>().WithMessage("The network path was not found");
        }

        [Fact]
        private void When_copying_file_from_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"\\teamserver\documents\for-all.txt";
            const string destinationPath = @"C:\docs\mine.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"C:\docs")
                .Build();

            // Act
            fileSystem.File.Copy(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeTrue();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_copying_file_to_existing_network_share_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"C:\docs\mine.txt";
            const string destinationPath = @"\\teamserver\documents\for-all.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(sourcePath)
                .IncludingDirectory(@"\\teamserver\documents")
                .Build();

            // Act
            fileSystem.File.Copy(sourcePath, destinationPath);

            // Assert
            fileSystem.File.Exists(sourcePath).Should().BeTrue();
            fileSystem.File.Exists(destinationPath).Should().BeTrue();
        }

        [Fact]
        private void When_copying_file_from_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy("com1", @"c:\new.txt");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_copying_file_to_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Copy(@"c:\old.txt", "com1");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_copying_file_from_extended_path_to_extended_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"\\server\share\summary.doc")
                .IncludingDirectory(@"c:\work")
                .Build();

            // Act
            fileSystem.File.Copy(@"\\?\UNC\server\share\summary.doc", @"\\?\c:\work\summary.doc");

            // Assert
            fileSystem.File.Exists(@"\\server\share\summary.doc").Should().BeTrue();
            fileSystem.File.Exists(@"c:\work\summary.doc").Should().BeTrue();
        }

        [Fact]
        private void When_copying_file_it_must_allocate_initial_size_and_update_timings()
        {
            // Arrange
            const string sourcePath = @"c:\file.txt";
            const string destinationPath = @"c:\copy.txt";

            var copyWaitIndicator = new WaitIndicator();

            DateTime sourceCreationTime = 4.January(2017).At(7, 52, 01);
            var clock = new SystemClock { UtcNow = () => sourceCreationTime };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .WithCopyWaitIndicator(copyWaitIndicator)
                .IncludingEmptyFile(sourcePath)
                .Build();

            DateTime sourceLastWriteTimeUtc = 10.January(2017).At(3, 12, 34);
            clock.UtcNow = () => sourceLastWriteTimeUtc;

            fileSystem.File.WriteAllText(sourcePath, "ABCDE");

            DateTime destinationCreationTimeUtc = 12.January(2017).At(11, 23, 45);
            clock.UtcNow = () => destinationCreationTimeUtc;

            // Act
            var copyThread = new Thread(() => { fileSystem.File.Copy(sourcePath, destinationPath); });
            copyThread.Start();

            copyWaitIndicator.WaitForStart();

            try
            {
                // Assert
                IFileInfo destinationInfo = fileSystem.ConstructFileInfo(destinationPath);
                destinationInfo.Exists.Should().BeTrue();
                destinationInfo.Length.Should().Be(5);
                destinationInfo.CreationTimeUtc.Should().Be(destinationCreationTimeUtc);
                destinationInfo.LastAccessTimeUtc.Should().Be(destinationCreationTimeUtc);
                destinationInfo.LastWriteTimeUtc.Should().Be(sourceLastWriteTimeUtc);

                DateTime destinationCompletedTimeUtc = 10.January(2017).At(11, 27, 36);
                clock.UtcNow = () => destinationCompletedTimeUtc;

                copyWaitIndicator.SetCompleted();
                copyThread.Join();

                destinationInfo.CreationTimeUtc.Should().Be(destinationCreationTimeUtc);
                destinationInfo.LastAccessTimeUtc.Should().Be(destinationCompletedTimeUtc);
                destinationInfo.LastWriteTimeUtc.Should().Be(sourceLastWriteTimeUtc);

                IFileInfo sourceInfo = fileSystem.ConstructFileInfo(sourcePath);
                sourceInfo.CreationTimeUtc.Should().Be(sourceCreationTime);
                sourceInfo.LastAccessTimeUtc.Should().Be(destinationCompletedTimeUtc);
                sourceInfo.LastWriteTimeUtc.Should().Be(sourceLastWriteTimeUtc);
            }
            finally
            {
                copyWaitIndicator.SetCompleted();
                copyThread.Join();
            }
        }

        [Fact]
        private void When_copying_file_to_overwrite_it_must_allocate_initial_size_and_update_timings()
        {
            // Arrange
            const string sourcePath = @"c:\file.txt";
            const string destinationPath = @"c:\copy.txt";

            var copyWaitIndicator = new WaitIndicator();

            DateTime creationTimeUtc = 4.January(2017).At(7, 52, 01);
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .WithCopyWaitIndicator(copyWaitIndicator)
                .IncludingEmptyFile(sourcePath)
                .IncludingEmptyFile(destinationPath)
                .Build();

            DateTime sourceLastWriteTimeUtc = 10.January(2017).At(3, 12, 34);
            clock.UtcNow = () => sourceLastWriteTimeUtc;

            fileSystem.File.WriteAllText(sourcePath, "ABCDE");

            DateTime destinationLastWriteTimeUtc = 12.January(2017).At(11, 23, 45);
            clock.UtcNow = () => destinationLastWriteTimeUtc;

            // Act
            var copyThread = new Thread(() => { fileSystem.File.Copy(sourcePath, destinationPath, true); });
            copyThread.Start();

            copyWaitIndicator.WaitForStart();

            try
            {
                // Assert
                IFileInfo destinationInfo = fileSystem.ConstructFileInfo(destinationPath);
                destinationInfo.Exists.Should().BeTrue();
                destinationInfo.Length.Should().Be(5);
                destinationInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
                destinationInfo.LastAccessTimeUtc.Should().Be(destinationLastWriteTimeUtc);
                destinationInfo.LastWriteTimeUtc.Should().Be(sourceLastWriteTimeUtc);

                DateTime destinationCompletedTimeUtc = 10.January(2017).At(11, 27, 36);
                clock.UtcNow = () => destinationCompletedTimeUtc;

                copyWaitIndicator.SetCompleted();
                copyThread.Join();

                destinationInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
                destinationInfo.LastAccessTimeUtc.Should().Be(destinationCompletedTimeUtc);
                destinationInfo.LastWriteTimeUtc.Should().Be(sourceLastWriteTimeUtc);

                IFileInfo sourceInfo = fileSystem.ConstructFileInfo(sourcePath);
                sourceInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
                sourceInfo.LastAccessTimeUtc.Should().Be(destinationCompletedTimeUtc);
                sourceInfo.LastWriteTimeUtc.Should().Be(sourceLastWriteTimeUtc);
            }
            finally
            {
                copyWaitIndicator.SetCompleted();
                copyThread.Join();
            }
        }
    }
}