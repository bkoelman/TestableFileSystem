using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoCreateSubdirectorySpecs
    {
        [Theory]
        [CanRunOnFileSystem]
        private void When_creating_subdirectory_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(@"d:\some");

                IFileSystem fileSystem = factory.Create()
                    .IncludingDirectory(path)
                    .Build();

                IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => dirInfo.CreateSubdirectory(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_creating_subdirectory_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(@"d:\some");

                IFileSystem fileSystem = factory.Create()
                    .IncludingDirectory(path)
                    .Build();

                IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

                // Act
                Action action = () => dirInfo.CreateSubdirectory(string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("Path cannot be the empty string or all whitespace.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_creating_subdirectory_for_whitespace_it_must_succeed(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(@"c:\some");

                IFileSystem fileSystem = factory.Create()
                    .Build();

                IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

                // Act
                Action action = () => dirInfo.CreateSubdirectory(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("Path cannot be the empty string or all whitespace.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_invalid_drive_it_must_fail()
        {
            // Arrange
            const string path = @"d:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            Action action = () => dirInfo.CreateSubdirectory("_:");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                "Second path fragment must not be a drive or UNC name.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            const string path = @"d:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            Action action = () => dirInfo.CreateSubdirectory("sub?");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_single_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\some";
            const string folderName = "sub";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            IDirectoryInfo subdirInfo = dirInfo.CreateSubdirectory(folderName);

            // Assert
            const string completePath = path + @"\" + folderName;

            subdirInfo.FullName.Should().Be(completePath);
            subdirInfo.Exists.Should().BeTrue();
            subdirInfo.Parent.ShouldNotBeNull().FullName.Should().Be(dirInfo.FullName);

            fileSystem.Directory.Exists(completePath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_relative_path_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\some";
            const string folderName = @"with\nested\subtree";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            IDirectoryInfo subdirInfo = dirInfo.CreateSubdirectory(folderName);

            // Assert
            const string completePath = path + @"\" + folderName;

            subdirInfo.FullName.Should().Be(completePath);
            subdirInfo.Exists.Should().BeTrue();

            IDirectoryInfo topDirectory = subdirInfo.Parent.ShouldNotBeNull().Parent.ShouldNotBeNull().Parent.ShouldNotBeNull();
            topDirectory.FullName.Should().Be(dirInfo.FullName);

            fileSystem.Directory.Exists(completePath).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_absolute_path_it_must_fail()
        {
            // Arrange
            const string path = @"d:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            Action action = () => dirInfo.CreateSubdirectory(@"d:\some\other");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                "Second path fragment must not be a drive or UNC name.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_same_drive_in_subdirectory_it_must_fail()
        {
            // Arrange
            const string path = @"d:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            Action action = () => dirInfo.CreateSubdirectory(@"d:sub");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                "Second path fragment must not be a drive or UNC name.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_remote_path_it_must_fail()
        {
            // Arrange
            const string path = @"d:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            Action action = () => dirInfo.CreateSubdirectory(@"\\server\share\folder");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                "Second path fragment must not be a drive or UNC name.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_absolute_path_without_drive_letter_it_must_fail()
        {
            // Arrange
            const string path = @"d:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            Action action = () => dirInfo.CreateSubdirectory(@"\other");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                "Second path fragment must not be a drive or UNC name.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_path_with_self_reference_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            IDirectoryInfo subdirInfo = dirInfo.CreateSubdirectory(@".\other");

            // Assert
            subdirInfo.FullName.Should().Be(@"d:\some\other");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_parent_path_it_must_fail()
        {
            // Arrange
            const string path = @"d:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            Action action = () => dirInfo.CreateSubdirectory(@"..");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                @"The directory specified, '..', is not a subdirectory of 'd:\some'.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_parent_path_above_directory_it_must_fail()
        {
            // Arrange
            const string path = @"d:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            Action action = () => dirInfo.CreateSubdirectory(@"..\..\more");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                @"The directory specified, '..\..\more', is not a subdirectory of 'd:\some\folder'.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_parent_path_next_to_directory_it_must_fail()
        {
            // Arrange
            const string path = @"d:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            Action action = () => dirInfo.CreateSubdirectory(@"..\some2\more");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(
                @"The directory specified, '..\some2\more', is not a subdirectory of 'd:\some'.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_parent_path_below_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            IDirectoryInfo subdirInfo = dirInfo.CreateSubdirectory(@"..\some\other");

            // Assert
            subdirInfo.FullName.Should().Be(@"d:\some\other");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_parent_path_below_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            IDirectoryInfo subdirInfo = dirInfo.CreateSubdirectory(@"..\SOME\OTHER");

            // Assert
            subdirInfo.FullName.Should().Be(@"d:\SOME\OTHER");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_subdirectory_for_too_many_parent_paths_below_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\some";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Act
            IDirectoryInfo subdirInfo = dirInfo.CreateSubdirectory(@"..\..\..\..\..\..\..\..\..\some\other");

            // Assert
            subdirInfo.FullName.Should().Be(@"d:\some\other");
        }
    }
}
