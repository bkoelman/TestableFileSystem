﻿using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileAttributesSpecs
    {
        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_attributes_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.File.GetAttributes(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_setting_attributes_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.File.SetAttributes(null, FileAttributes.Normal);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(string.Empty);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(string.Empty, FileAttributes.Normal);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(" ");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(" ", FileAttributes.Normal);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes("_:", FileAttributes.Archive);

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_atributes_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"c:\dir?i");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_atributes_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(@"c:\dir?i", FileAttributes.Archive);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_missing_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"C:\some\file.txt");

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_missing_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(@"C:\some\file.txt", FileAttributes.Archive);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_file_to_Normal_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Normal);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_file_to_Archive_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path + "  ", FileAttributes.Archive);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Archive);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_file_in_readonly_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\folder", FileAttributes.ReadOnly)
                .IncludingEmptyFile(@"C:\folder\some.txt")
                .Build();

            // Act
            fileSystem.File.SetAttributes(@"C:\folder\some.txt", FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(@"C:\folder\some.txt").Should().Be(FileAttributes.Hidden);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_file_to_zero_it_must_reset_to_Normal()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, 0);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_file_to_Directory_it_must_discard_and_reset_to_Normal()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Directory);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_file_to_Device_it_must_discard_and_reset_to_Normal()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Device);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_file_to_SparseFile_it_must_discard_and_reset_to_Normal()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.SparseFile);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_file_to_ReparsePoint_it_must_discard_and_reset_to_Normal()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.ReparsePoint);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_file_to_Compressed_it_must_discard_and_reset_to_Normal()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Compressed);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_file_to_Encrypted_it_must_discard_and_reset_to_Normal()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Encrypted);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_file_to_IntegrityStream_it_must_discard_and_reset_to_Normal()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.IntegrityStream);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Normal);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_file_to_all_it_must_filter_and_succeed()
        {
            // Arrange
            const string path = @"C:\some.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path,
                FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory |
                FileAttributes.Archive | FileAttributes.Device | FileAttributes.Normal | FileAttributes.Temporary |
                FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed | FileAttributes.Offline |
                FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.IntegrityStream |
                FileAttributes.NoScrubData);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.ReadOnly | FileAttributes.Hidden |
                FileAttributes.System | FileAttributes.Archive | FileAttributes.Temporary | FileAttributes.Offline |
                FileAttributes.NotContentIndexed | FileAttributes.NoScrubData);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_missing_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes("M:");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'M:\'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_missing_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(@"M:", FileAttributes.Archive);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(@"Could not find a part of the path 'M:\'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_drive_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(path);

            // Assert
            attributes.Should().Be(FileAttributes.Directory | FileAttributes.System | FileAttributes.Hidden);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_drive_it_must_preserve_minimum_set()
        {
            // Arrange
            const string path = @"C:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory | FileAttributes.System |
                FileAttributes.Hidden | FileAttributes.ReadOnly);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_drive_to_Temporary_it_must_fail()
        {
            // Arrange
            const string path = @"C:\";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(path, FileAttributes.Temporary);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Invalid File or Directory attributes value.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_current_directory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\folder")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            fileSystem.File.SetAttributes(@"c:\folder", FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(@"C:\folder").Should().Be(FileAttributes.Hidden | FileAttributes.Directory);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_file_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingEmptyFile(@"C:\file.txt", FileAttributes.Archive)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(@"\file.txt");

            // Assert
            attributes.Should().Be(FileAttributes.Archive);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_file_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingEmptyFile(@"C:\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            fileSystem.File.SetAttributes(@"\file.txt", FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(@"C:\file.txt").Should().Be(FileAttributes.Hidden);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_existing_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\folder\some.txt", FileAttributes.Hidden)
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(@"some.txt");

            // Assert
            attributes.Should().Be(FileAttributes.Hidden);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\folder\some.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\folder");

            // Act
            fileSystem.File.SetAttributes(@"some.txt", FileAttributes.ReadOnly);

            // Assert
            fileSystem.File.GetAttributes(@"C:\folder\some.txt").Should().Be(FileAttributes.ReadOnly);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\FOLDER\some.TXT", FileAttributes.ReadOnly)
                .Build();

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(@"C:\folder\SOME.txt");

            // Assert
            attributes.Should().Be(FileAttributes.ReadOnly);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\FOLDER\some.TXT", FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.File.SetAttributes(@"C:\folder\SOME.txt", FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(@"C:\FOLDER\some.TXT").Should().Be(FileAttributes.Hidden);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"C:\some\file.txt");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\file.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(@"C:\some\file.txt", FileAttributes.Archive);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\file.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"c:\some\file.txt\nested.txt");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(@"c:\some\file.txt\nested.txt", FileAttributes.Archive);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"c:\some\file.txt\nested.txt\more.txt");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt\more.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(@"c:\some\file.txt\nested.txt\more.txt", FileAttributes.Archive);

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt\more.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_on_missing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(path);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The network path was not found.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_on_missing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing.docx";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(path, FileAttributes.Hidden);

            // Assert
            action.Should().ThrowExactly<IOException>().WithMessage("The network path was not found.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_missing_remote_file_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(path);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>()
                .WithMessage(@"Could not find file '\\server\share\missing.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_missing_remote_file_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\missing.docx";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(path, FileAttributes.Hidden);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>()
                .WithMessage(@"Could not find file '\\server\share\missing.docx'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_existing_remote_file_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\personal.docx";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(path);

            // Assert
            attributes.Should().Be(FileAttributes.ReadOnly);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_remote_file_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\personal.docx";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Hidden);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_existing_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(path);

            // Assert
            attributes.Should().Be(FileAttributes.Directory);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_directory_to_Directory_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Directory);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_directory_to_Temporary_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(path, FileAttributes.Temporary);

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Invalid File or Directory attributes value.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_directory_to_all_except_Temporary_it_must_filter_and_succeed()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path,
                FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory |
                FileAttributes.Archive | FileAttributes.Device | FileAttributes.Normal | FileAttributes.SparseFile |
                FileAttributes.ReparsePoint | FileAttributes.Compressed | FileAttributes.Offline |
                FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.IntegrityStream |
                FileAttributes.NoScrubData);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.ReadOnly | FileAttributes.Hidden |
                FileAttributes.System | FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Offline |
                FileAttributes.NotContentIndexed | FileAttributes.NoScrubData | FileAttributes.ReparsePoint);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_directory_to_Device_it_must_discard_and_preserve_directory_attribute()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Device);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void
            When_setting_attributes_for_existing_directory_to_SparseFile_it_must_discard_and_preserve_directory_attribute()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.SparseFile);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_directory_to_Normal_it_must_discard_and_preserve_directory_attribute()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Normal);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void
            When_setting_attributes_for_existing_directory_to_Compressed_it_must_discard_and_preserve_directory_attribute()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Compressed);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void
            When_setting_attributes_for_existing_directory_to_Encrypted_it_must_discard_and_preserve_directory_attribute()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Encrypted);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void
            When_setting_attributes_for_existing_directory_to_IntegrityStream_it_must_discard_and_preserve_directory_attribute()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.IntegrityStream);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_directory_to_Hidden_it_must_preserve_directory_attribute()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Directory | FileAttributes.Hidden);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes("COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes("COM1", FileAttributes.Archive);

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_missing_extended_local_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.GetAttributes(@"\\?\C:\some\missing.txt");

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file '\\?\C:\some\missing.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_missing_extended_local_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            Action action = () => fileSystem.File.SetAttributes(@"\\?\C:\some\missing.txt", FileAttributes.ReadOnly);

            // Assert
            action.Should().ThrowExactly<FileNotFoundException>().WithMessage(@"Could not find file '\\?\C:\some\missing.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_attributes_for_existing_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\other.txt", FileAttributes.ReadOnly)
                .Build();

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(@"\\?\C:\some\other.txt");

            // Assert
            attributes.Should().Be(FileAttributes.ReadOnly);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_setting_attributes_for_existing_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\other.txt")
                .Build();

            // Act
            fileSystem.File.SetAttributes(@"\\?\C:\some\other.txt", FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(@"\\?\C:\some\other.txt").Should().Be(FileAttributes.Hidden);
        }
    }
}
