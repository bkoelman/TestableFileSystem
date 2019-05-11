using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakePath
{
    public sealed class PathGetFullPathSpecs
    {
        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_full_path_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.Path.GetFullPath(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_full_path_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Path.GetFullPath(string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_getting_full_path_for_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.Path.GetFullPath(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Path.GetFullPath("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Path.GetFullPath("some?.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"d:\")
                .Build();

            // Act
            string fullPath = fileSystem.Path.GetFullPath(@"d:");

            // Assert
            fullPath.Should().Be(@"D:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_root_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"d:\")
                .Build();

            // Act
            string fullPath = fileSystem.Path.GetFullPath(@"d:\");

            // Assert
            fullPath.Should().Be(@"d:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_absolute_path_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\some\folder\sub\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string fullPath = fileSystem.Path.GetFullPath(path);

            // Assert
            fullPath.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_absolute_path_with_multiple_separators_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string fullPath = fileSystem.Path.GetFullPath(@"d:\\\\some\folder\\\\\sub\file.txt");

            // Assert
            fullPath.Should().Be(@"d:\some\folder\sub\file.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_absolute_path_with_mixed_separators_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string fullPath = fileSystem.Path.GetFullPath(@"d:\some/folder\sub/file.txt");

            // Assert
            fullPath.Should().Be(@"d:\some\folder\sub\file.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_absolute_path_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"d:\some\folder\sub\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string fullPath = fileSystem.Path.GetFullPath(path + "  ");

            // Assert
            fullPath.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_relative_path_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some\folder\sub")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\some");

            // Act
            string fullPath = fileSystem.Path.GetFullPath(@"folder\sub\file.txt");

            // Assert
            fullPath.Should().Be(@"c:\some\folder\sub\file.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_relative_path_with_different_casing_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\SOME\FOLDER\SUB")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"C:\Some");

            // Act
            string fullPath = fileSystem.Path.GetFullPath(@"folder\sub\file.txt");

            // Assert
            fullPath.Should().Be(@"C:\Some\folder\sub\file.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .IncludingEmptyFile(@"c:\file.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\some");

            // Act
            string fullPath = fileSystem.Path.GetFullPath(@"\file.txt");

            // Assert
            fullPath.Should().Be(@"c:\file.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_relative_path_with_self_and_parent_references_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"c:\some");

            // Act
            string fullPath = fileSystem.Path.GetFullPath(@".\folder\sub\skipped\..\file.txt");

            // Assert
            fullPath.Should().Be(@"c:\some\folder\sub\file.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_absolute_path_with_parent_references_above_root_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string fullPath = fileSystem.Path.GetFullPath(@"c:\folder\sub\skipped\..\..\..\..\..\..\..\..\..\file.txt");

            // Assert
            fullPath.Should().Be(@"c:\file.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_relative_path_on_different_drive_in_root_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"C:\some")
                .IncludingDirectory(@"D:\other")
                .IncludingEmptyFile(@"D:\child.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"d:\other");
            fileSystem.Directory.SetCurrentDirectory(@"c:\some");

            // Act
            string fullPath = fileSystem.Path.GetFullPath("D:child.txt");

            // Assert
            fullPath.Should().Be(@"D:\child.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_relative_path_on_same_drive_in_subdirectory_it_must_succeed()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"D:\other\child.txt")
                .Build();

            fileSystem.Directory.SetCurrentDirectory(@"d:\other");

            // Act
            string fullPath = fileSystem.Path.GetFullPath("D:child.txt");

            // Assert
            fullPath.Should().Be(@"d:\other\child.txt");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_remote_path_it_must_succeed()
        {
            // Arrange
            const string path = @"\\hostname\share\folder\sub\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string fullPath = fileSystem.Path.GetFullPath(path);

            // Assert
            fullPath.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_network_share_it_must_succeed()
        {
            // Arrange
            const string path = @"\\hostname\share";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string fullPath = fileSystem.Path.GetFullPath(path);

            // Assert
            fullPath.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_network_host_without_share_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Path.GetFullPath(@"\\hostname");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage(@"The UNC path should be of the form \\server\share.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Path.GetFullPath(@"COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_extended_local_path_it_must_succeed()
        {
            // Arrange
            const string path = @"\\?\c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string fullPath = fileSystem.Path.GetFullPath(path);

            // Assert
            fullPath.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_extended_remote_path_it_must_succeed()
        {
            // Arrange
            const string path = @"\\?\UNC\hostname\share\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            string fullPath = fileSystem.Path.GetFullPath(path);

            // Assert
            fullPath.Should().Be(path);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_win32_device_namespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Path.GetFullPath(@"\\.\c:\folder\file.txt");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("Only Win32 File Namespaces are supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_getting_full_path_for_NT_namespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.Path.GetFullPath(@"\\?\GLOBALROOT");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("Only Win32 File Namespaces are supported.");
        }
    }
}
