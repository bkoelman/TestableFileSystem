using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoConstructSpecs
    {
        private const FileAttributes MissingEntryAttributes = (FileAttributes)(-1);
        private static readonly DateTime ZeroFileTimeUtc = 1.January(1601).AsUtc();

        [Fact]
        private void When_constructing_directory_info_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.ConstructDirectoryInfo(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_constructing_directory_info_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructDirectoryInfo(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_constructing_directory_info_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructDirectoryInfo(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_constructing_directory_info_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructDirectoryInfo("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_constructing_directory_info_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructDirectoryInfo("some?.txt");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_constructing_directory_info_for_missing_subdirectory_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be("folder");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(path);
            dirInfo.Exists.Should().BeFalse();
            dirInfo.Attributes.Should().Be(MissingEntryAttributes);

            dirInfo.CreationTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(ZeroFileTimeUtc);
            dirInfo.LastAccessTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
            dirInfo.LastWriteTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"c:\some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"c:\");
        }

        [Fact]
        private void When_constructing_directory_info_for_existing_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path + @"\file.txt", "X");

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.Directory.GetFiles(path);

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be("folder");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(path);
            dirInfo.Exists.Should().BeTrue();
            dirInfo.Attributes.Should().Be(FileAttributes.Directory);

            dirInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            dirInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            dirInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"c:\some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"c:\");
        }

        [Fact]
        private void When_constructing_directory_info_for_existing_local_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\SOME\folDER";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"C:\some\FOLder", FileAttributes.Hidden)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path + @"\file.txt", "X");

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.Directory.GetFiles(path);

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be("folDER");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(path);
            dirInfo.Exists.Should().BeTrue();
            dirInfo.Attributes.Should().Be(FileAttributes.Directory | FileAttributes.Hidden);

            dirInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            dirInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            dirInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"c:\SOME");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"c:\");
        }

        [Fact]
        private void When_constructing_directory_info_for_existing_local_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder  ";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(@"c:\some\folder\file.txt", "X");

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.Directory.GetFiles(path);

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be("folder");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(@"c:\some\folder");
            dirInfo.Exists.Should().BeTrue();
            dirInfo.Attributes.Should().Be(FileAttributes.Directory);

            dirInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            dirInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            dirInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"c:\some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"c:\");
        }

        [Fact]
        private void When_constructing_directory_info_for_root_of_drive_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be(@"c:\");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(path);
            dirInfo.Exists.Should().BeTrue();
            dirInfo.Attributes.Should().Be(FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory);

            dirInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            dirInfo.LastAccessTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(creationTimeUtc);
            dirInfo.LastWriteTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(creationTimeUtc);

            dirInfo.Parent.Should().BeNull();
            dirInfo.Root.FullName.Should().Be(@"c:\");
        }

        [Fact]
        private void When_constructing_directory_info_for_directory_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\other")
                .IncludingDirectory(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path + @"\file.txt", "X");

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.Directory.GetFiles(path);

            fileSystem.Directory.SetCurrentDirectory(@"C:\other");

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(@"\some\folder");

            // Assert
            dirInfo.Name.Should().Be("folder");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(@"C:\some\folder");
            dirInfo.Exists.Should().BeTrue();
            dirInfo.Attributes.Should().Be(FileAttributes.Directory);

            dirInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            dirInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            dirInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"C:\some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"C:\");
        }

        [Fact]
        private void When_constructing_directory_info_for_existing_relative_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path + @"\file.txt", "X");

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.Directory.GetFiles(path);

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(@"folder");

            // Assert
            dirInfo.Name.Should().Be("folder");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(@"C:\some\folder");
            dirInfo.Exists.Should().BeTrue();
            dirInfo.Attributes.Should().Be(FileAttributes.Directory);

            dirInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            dirInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            dirInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"C:\some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"C:\");
        }

        [Fact]
        private void When_constructing_directory_info_for_existing_local_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be("file.txt");
            dirInfo.Extension.Should().Be(".txt");
            dirInfo.FullName.Should().Be(path);
            dirInfo.Exists.Should().BeFalse();
            dirInfo.Attributes.Should().Be(FileAttributes.Archive);

            dirInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            dirInfo.LastAccessTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(creationTimeUtc);
            dirInfo.LastWriteTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(creationTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"c:\some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"c:\");
        }

        [Fact]
        private void When_constructing_directory_info_for_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt\nested";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be("nested");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(path);
            dirInfo.Exists.Should().BeFalse();
            dirInfo.Attributes.Should().Be(MissingEntryAttributes);

            dirInfo.CreationTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(ZeroFileTimeUtc);
            dirInfo.LastAccessTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
            dirInfo.LastWriteTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"C:\some\file.txt");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"C:\");
        }

        [Fact]
        private void When_constructing_directory_info_for_parent_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt\nested.html\more";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be("more");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(path);
            dirInfo.Exists.Should().BeFalse();
            dirInfo.Attributes.Should().Be(MissingEntryAttributes);

            dirInfo.CreationTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(ZeroFileTimeUtc);
            dirInfo.LastAccessTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
            dirInfo.LastWriteTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"C:\some\file.txt\nested.html");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"C:\");
        }

        [Fact]
        private void When_constructing_directory_info_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be("folder");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(path);
            dirInfo.Exists.Should().BeFalse();
            dirInfo.Attributes.Should().Be(MissingEntryAttributes);

            dirInfo.CreationTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(ZeroFileTimeUtc);
            dirInfo.LastAccessTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
            dirInfo.LastWriteTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"C:\some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"C:\");
        }

        [Fact]
        private void When_constructing_directory_info_for_missing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be("folder");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(path);
            dirInfo.Exists.Should().BeFalse();
            ActionFactory.IgnoreReturnValue(() => dirInfo.Attributes)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");

            ActionFactory.IgnoreReturnValue(() => dirInfo.CreationTime)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");
            ActionFactory.IgnoreReturnValue(() => dirInfo.CreationTimeUtc)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");
            ActionFactory.IgnoreReturnValue(() => dirInfo.LastAccessTime)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");
            ActionFactory.IgnoreReturnValue(() => dirInfo.LastAccessTimeUtc)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");
            ActionFactory.IgnoreReturnValue(() => dirInfo.LastWriteTime)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");
            ActionFactory.IgnoreReturnValue(() => dirInfo.LastWriteTimeUtc)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"\\server\share");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"\\server\share");
        }

        [Fact]
        private void When_constructing_directory_info_for_missing_remote_directory_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\folder";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be("folder");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(path);
            dirInfo.Exists.Should().BeFalse();
            dirInfo.Attributes.Should().Be(MissingEntryAttributes);

            dirInfo.CreationTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(ZeroFileTimeUtc);
            dirInfo.LastAccessTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
            dirInfo.LastWriteTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"\\server\share");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"\\server\share");
        }

        [Fact]
        private void When_constructing_directory_info_for_existing_remote_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path + @"\file.txt", "X");

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.Directory.GetFiles(path);

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be("folder");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(path);
            dirInfo.Exists.Should().BeTrue();
            dirInfo.Attributes.Should().Be(FileAttributes.Directory);

            dirInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            dirInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            dirInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"\\server\share");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"\\server\share");
        }

        [Fact]
        private void When_constructing_directory_info_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructDirectoryInfo("COM1");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_constructing_directory_info_for_existing_extended_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path + @"\file.txt", "X");

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.Directory.GetFiles(path);

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(@"\\?\c:\some\folder");

            // Assert
            dirInfo.Name.Should().Be("folder");
            dirInfo.Extension.Should().BeEmpty();
            dirInfo.FullName.Should().Be(@"\\?\c:\some\folder");
            dirInfo.Exists.Should().BeTrue();
            dirInfo.Attributes.Should().Be(FileAttributes.Directory);

            dirInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            dirInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            dirInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.FullName.Should().Be(@"\\?\c:\some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.FullName.Should().Be(@"\\?\c:\");
        }
    }
}
