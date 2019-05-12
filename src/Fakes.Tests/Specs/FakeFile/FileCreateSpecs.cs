using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileCreateSpecs
    {
        [Theory]
        [CanRunOnFileSystem]
        private void When_creating_file_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.File.Create(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_creating_file_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.Create(string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("Empty path name is not legal.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_creating_file_for_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.Create(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_file_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_file_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create("some?.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_local_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Create(path))
            {
                stream.WriteByte(0xFF);
            }

            // Assert
            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Exists.Should().BeTrue();
            info.Attributes.Should().Be(FileAttributes.Archive);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_local_file_for_random_access_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            using (fileSystem.File.Create(@"c:\doc.txt", 1, FileOptions.RandomAccess))
            {
                // Assert
                fileSystem.File.Exists(@"c:\doc.txt").Should().BeTrue();
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_local_file_with_asynchronous_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\doc.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Create(path, 1, FileOptions.Asynchronous))
            {
                // Assert
                stream.IsAsync.Should().BeTrue();
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_local_file_with_encryption_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\doc.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            using (fileSystem.File.Create(path, 1, FileOptions.Encrypted))
            {
            }

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Encrypted | FileAttributes.Archive);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_local_file_with_delete_on_close_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\doc.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path)
                .Build();

            // Act
            using (fileSystem.File.Create(path, 1, FileOptions.DeleteOnClose))
            {
                // Assert
                fileSystem.File.Exists(path).Should().BeTrue();
            }

            fileSystem.File.Exists(path).Should().BeFalse();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_unable_to_create_local_file_with_delete_on_close_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\doc.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "Text")
                .Build();

            using (fileSystem.File.OpenRead(path))
            {
                // Act
                try
                {
                    using (fileSystem.File.Create(path, 1, FileOptions.DeleteOnClose))
                    {
                    }
                }
                catch (IOException)
                {
                }
            }

            // Assert
            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_existing_local_file_it_must_overwrite()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "existing data")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Create(path))
            {
                // Assert
                stream.Length.Should().Be(0);
            }

            IFileInfo info = fileSystem.ConstructFileInfo(path);
            info.Exists.Should().BeTrue();
            info.Attributes.Should().Be(FileAttributes.Archive);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_file_that_overwrites_existing_local_hidden_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.Hidden)
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(path);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>().WithMessage(
                @"Access to the path 'C:\some\file.txt' is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_file_that_overwrites_existing_local_readonly_file_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(path, FileAttributes.ReadOnly)
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(path);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>().WithMessage(
                @"Access to the path 'C:\some\file.txt' is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_existing_local_file_with_different_casing_it_must_overwrite()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(@"C:\some\FILE.txt", "existing data")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Create(@"c:\SOME\file.TXT"))
            {
                // Assert
                stream.Length.Should().Be(0);
            }

            fileSystem.File.Exists(@"C:\some\FILE.txt").Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_local_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            // Act
            using (fileSystem.File.Create(@"C:\some\file.txt  "))
            {
            }

            // Assert
            fileSystem.File.Exists(@"C:\some\file.txt").Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_file_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingEmptyFile(@"C:\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            using (fileSystem.File.Create(@"\file.txt"))
            {
            }

            // Assert
            fileSystem.File.Exists(@"C:\file.txt").Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_relative_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            using (fileSystem.File.Create("file.txt"))
            {
            }

            // Assert
            fileSystem.File.Exists(@"C:\some\file.txt").Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_local_file_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(@"C:\some\subfolder");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'C:\some\subfolder'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_local_file_for_file_that_exists_as_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\subfolder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(path)
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(path);

            // Assert
            action.Should().ThrowExactly<UnauthorizedAccessException>().WithMessage(
                @"Access to the path 'C:\some\subfolder' is denied.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_local_file_for_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(@"c:\some\file.txt\nested.txt");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt'.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_local_file_for_parent_parent_directory_that_exists_as_file_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"c:\some\file.txt")
                .Build();

            // Act
            Action action = () => fileSystem.File.Create(@"c:\some\file.txt\nested.txt\more.txt");

            // Assert
            action.Should().ThrowExactly<DirectoryNotFoundException>().WithMessage(
                @"Could not find a part of the path 'c:\some\file.txt\nested.txt\more.txt'.");
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_creating_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkShare(), false);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.Create(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_creating_remote_file_on_missing_network_share_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkFileAtDepth(1), false);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.File.Create(path);

                // Assert
                action.Should().ThrowExactly<IOException>().WithMessage($"The network path was not found. : '{path}'");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_creating_remote_file_on_existing_network_share_it_must_succeed(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkFileAtDepth(1));
                string parentPath = factory.MapPath(PathFactory.NetworkShare());

                IFileSystem fileSystem = factory.Create()
                    .IncludingDirectory(parentPath)
                    .Build();

                // Act
                using (fileSystem.File.Create(path))
                {
                }

                // Assert
                fileSystem.File.Exists(path).Should().BeTrue();
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_existing_remote_file_it_must_overwrite()
        {
            // Arrange
            string path = PathFactory.NetworkFileAtDepth(1);

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "existing data")
                .Build();

            // Act
            using (IFileStream stream = fileSystem.File.Create(path))
            {
                // Assert
                stream.Length.Should().Be(0);
            }

            fileSystem.File.Exists(path).Should().BeTrue();
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_file_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.File.Create("COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_creating_extended_local_file_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\?\C:\folder")
                .Build();

            // Act
            using (fileSystem.File.Create(@"\\?\C:\folder\file.txt"))
            {
            }

            // Assert
            fileSystem.File.Exists(@"C:\folder\file.txt").Should().BeTrue();
        }
    }
}
