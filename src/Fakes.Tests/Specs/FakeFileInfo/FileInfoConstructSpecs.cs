using System;
using System.IO;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFileInfo
{
    public sealed class FileInfoConstructSpecs
    {
        private const string DefaultContents = "ABC";
        private const FileAttributes MissingEntryAttributes = (FileAttributes)(-1);
        private static readonly DateTime ZeroFileTimeUtc = 1.January(1601).AsUtc();

        [Fact]
        private void When_constructing_file_info_for_null_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            Action action = () => fileSystem.ConstructFileInfo(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        private void When_constructing_file_info_for_empty_string_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(string.Empty);

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_constructing_file_info_for_whitespace_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo(" ");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("The path is not of a legal form.*");
        }

        [Fact]
        private void When_constructing_file_info_for_invalid_root_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo("::");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact]
        private void When_constructing_file_info_for_invalid_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo("some?.txt");

            // Assert
            action.ShouldThrow<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact]
        private void When_constructing_file_info_for_missing_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"c:\some")
                .Build();

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Assert
            fileInfo.Name.Should().Be("file.txt");
            fileInfo.Extension.Should().Be(".txt");
            fileInfo.FullName.Should().Be(path);
            fileInfo.DirectoryName.Should().Be(@"c:\some");
            fileInfo.Exists.Should().BeFalse();
            ActionFactory.IgnoreReturnValue(() => fileInfo.Length)
                .ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'c:\some\file.txt'.");
            fileInfo.IsReadOnly.Should().BeTrue();
            fileInfo.Attributes.Should().Be(MissingEntryAttributes);

            fileInfo.CreationTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(ZeroFileTimeUtc);
            fileInfo.LastAccessTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
            fileInfo.LastWriteTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"c:\some");
        }

        [Fact]
        private void When_constructing_file_info_for_existing_local_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path, DefaultContents);

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.File.ReadAllText(path);

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Assert
            fileInfo.Name.Should().Be("file.txt");
            fileInfo.Extension.Should().Be(".txt");
            fileInfo.FullName.Should().Be(path);
            fileInfo.DirectoryName.Should().Be(@"c:\some");
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().Be(DefaultContents.Length);
            fileInfo.IsReadOnly.Should().BeFalse();
            fileInfo.Attributes.Should().Be(FileAttributes.Archive);

            fileInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            fileInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            fileInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"c:\some");
        }

        [Fact]
        private void When_constructing_file_info_for_existing_local_file_with_different_casing_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\SOME\file.TXT";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(@"C:\some\FILE.txt", FileAttributes.Hidden)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path, DefaultContents);

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.File.ReadAllText(path);

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Assert
            fileInfo.Name.Should().Be("file.TXT");
            fileInfo.Extension.Should().Be(".TXT");
            fileInfo.FullName.Should().Be(path);
            fileInfo.DirectoryName.Should().Be(@"c:\SOME");
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().Be(DefaultContents.Length);
            fileInfo.IsReadOnly.Should().BeFalse();
            fileInfo.Attributes.Should().Be(FileAttributes.Hidden);

            fileInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            fileInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            fileInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"c:\SOME");
        }

        [Fact]
        private void When_constructing_file_info_for_existing_local_file_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt  ";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path, DefaultContents);

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.File.ReadAllText(path);

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Assert
            fileInfo.Name.Should().Be("file.txt  ");
            fileInfo.Extension.Should().Be(".txt");
            fileInfo.FullName.Should().Be(@"c:\some\file.txt");
            fileInfo.DirectoryName.Should().Be(@"c:\some");
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().Be(DefaultContents.Length);
            fileInfo.IsReadOnly.Should().BeFalse();
            fileInfo.Attributes.Should().Be(FileAttributes.Archive);

            fileInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            fileInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            fileInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"c:\some");
        }

        [Fact]
        private void When_constructing_file_info_for_root_of_drive_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Assert
            fileInfo.Name.Should().BeEmpty();
            fileInfo.Extension.Should().BeEmpty();
            fileInfo.FullName.Should().Be(path);
            fileInfo.DirectoryName.Should().BeNull();
            fileInfo.Exists.Should().BeFalse();
            ActionFactory.IgnoreReturnValue(() => fileInfo.Length)
                .ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'c:\'.");
            fileInfo.IsReadOnly.Should().BeFalse();
            fileInfo.Attributes.Should().Be(FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory);

            fileInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            fileInfo.LastAccessTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(creationTimeUtc);
            fileInfo.LastWriteTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(creationTimeUtc);

            fileInfo.Directory.Should().BeNull();
        }

        [Fact]
        private void When_constructing_file_info_for_file_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\other")
                .IncludingEmptyFile(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path, DefaultContents);

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.File.ReadAllText(path);

            fileSystem.Directory.SetCurrentDirectory(@"C:\other");

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(@"\some\file.txt");

            // Assert
            fileInfo.Name.Should().Be("file.txt");
            fileInfo.Extension.Should().Be(".txt");
            fileInfo.FullName.Should().Be(@"C:\some\file.txt");
            fileInfo.DirectoryName.Should().Be(@"C:\some");
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().Be(DefaultContents.Length);
            fileInfo.IsReadOnly.Should().BeFalse();
            fileInfo.Attributes.Should().Be(FileAttributes.Archive);

            fileInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            fileInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            fileInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"C:\some");
        }

        [Fact]
        private void When_constructing_file_info_for_existing_relative_local_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path, DefaultContents);

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.File.ReadAllText(path);

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(@"file.txt");

            // Assert
            fileInfo.Name.Should().Be("file.txt");
            fileInfo.Extension.Should().Be(".txt");
            fileInfo.FullName.Should().Be(@"C:\some\file.txt");
            fileInfo.DirectoryName.Should().Be(@"C:\some");
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().Be(DefaultContents.Length);
            fileInfo.IsReadOnly.Should().BeFalse();
            fileInfo.Attributes.Should().Be(FileAttributes.Archive);

            fileInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            fileInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            fileInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"C:\some");
        }

        [Fact]
        private void When_constructing_file_info_for_existing_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(path)
                .Build();

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Assert
            fileInfo.Name.Should().Be("folder");
            fileInfo.Extension.Should().Be(string.Empty);
            fileInfo.FullName.Should().Be(path);
            fileInfo.DirectoryName.Should().Be(@"c:\some");
            fileInfo.Exists.Should().BeFalse();
            ActionFactory.IgnoreReturnValue(() => fileInfo.Length)
                .ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'c:\some\folder'.");
            fileInfo.IsReadOnly.Should().BeFalse();
            fileInfo.Attributes.Should().Be(FileAttributes.Directory);

            fileInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            fileInfo.LastAccessTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(creationTimeUtc);
            fileInfo.LastWriteTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(creationTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"c:\some");
        }

        [Fact]
        private void When_constructing_file_info_for_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt\nested.html";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Assert
            fileInfo.Name.Should().Be("nested.html");
            fileInfo.Extension.Should().Be(".html");
            fileInfo.FullName.Should().Be(path);
            fileInfo.DirectoryName.Should().Be(@"C:\some\file.txt");
            fileInfo.Exists.Should().BeFalse();
            ActionFactory.IgnoreReturnValue(() => fileInfo.Length)
                .ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt\nested.html'.");
            fileInfo.IsReadOnly.Should().BeTrue();
            fileInfo.Attributes.Should().Be(MissingEntryAttributes);

            fileInfo.CreationTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(ZeroFileTimeUtc);
            fileInfo.LastAccessTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
            fileInfo.LastWriteTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"C:\some\file.txt");
        }

        [Fact]
        private void When_constructing_file_info_for_parent_parent_directory_that_exists_as_file_it_must_succeed()
        {
            // Arrange
            const string path = @"C:\some\file.txt\nested.html\more.docx";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingEmptyFile(@"C:\some\file.txt")
                .Build();

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Assert
            fileInfo.Name.Should().Be("more.docx");
            fileInfo.Extension.Should().Be(".docx");
            fileInfo.FullName.Should().Be(path);
            fileInfo.DirectoryName.Should().Be(@"C:\some\file.txt\nested.html");
            fileInfo.Exists.Should().BeFalse();
            ActionFactory.IgnoreReturnValue(() => fileInfo.Length)
                .ShouldThrow<FileNotFoundException>()
                .WithMessage(@"Could not find file 'C:\some\file.txt\nested.html\more.docx'.");
            fileInfo.IsReadOnly.Should().BeTrue();
            fileInfo.Attributes.Should().Be(MissingEntryAttributes);

            fileInfo.CreationTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(ZeroFileTimeUtc);
            fileInfo.LastAccessTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
            fileInfo.LastWriteTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"C:\some\file.txt\nested.html");
        }

        [Fact]
        private void When_constructing_file_info_for_missing_parent_directory_it_must_fail()
        {
            // Arrange
            const string path = @"C:\some\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Assert
            fileInfo.Name.Should().Be("file.txt");
            fileInfo.Extension.Should().Be(".txt");
            fileInfo.FullName.Should().Be(path);
            fileInfo.DirectoryName.Should().Be(@"C:\some");
            fileInfo.Exists.Should().BeFalse();
            ActionFactory.IgnoreReturnValue(() => fileInfo.Length)
                .ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file 'C:\some\file.txt'.");
            fileInfo.IsReadOnly.Should().BeTrue();
            fileInfo.Attributes.Should().Be(MissingEntryAttributes);

            fileInfo.CreationTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(ZeroFileTimeUtc);
            fileInfo.LastAccessTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
            fileInfo.LastWriteTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"C:\some");
        }

        [Fact]
        private void When_constructing_file_info_for_missing_network_share_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Assert
            fileInfo.Name.Should().Be("file.txt");
            fileInfo.Extension.Should().Be(".txt");
            fileInfo.FullName.Should().Be(path);
            fileInfo.DirectoryName.Should().Be(@"\\server\share");
            fileInfo.Exists.Should().BeFalse();
            ActionFactory.IgnoreReturnValue(() => fileInfo.Length)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");
            ActionFactory.IgnoreReturnValue(() => fileInfo.IsReadOnly)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");
            ActionFactory.IgnoreReturnValue(() => fileInfo.Attributes)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");

            ActionFactory.IgnoreReturnValue(() => fileInfo.CreationTime)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");
            ActionFactory.IgnoreReturnValue(() => fileInfo.CreationTimeUtc)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");
            ActionFactory.IgnoreReturnValue(() => fileInfo.LastAccessTime)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");
            ActionFactory.IgnoreReturnValue(() => fileInfo.LastAccessTimeUtc)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");
            ActionFactory.IgnoreReturnValue(() => fileInfo.LastWriteTime)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");
            ActionFactory.IgnoreReturnValue(() => fileInfo.LastWriteTimeUtc)
                .ShouldThrow<IOException>().WithMessage("The network path was not found");

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"\\server\share");
        }

        [Fact]
        private void When_constructing_file_info_for_missing_remote_file_it_must_fail()
        {
            // Arrange
            const string path = @"\\server\share\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(@"\\server\share")
                .Build();

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Assert
            fileInfo.Name.Should().Be("file.txt");
            fileInfo.Extension.Should().Be(".txt");
            fileInfo.FullName.Should().Be(path);
            fileInfo.DirectoryName.Should().Be(@"\\server\share");
            fileInfo.Exists.Should().BeFalse();
            ActionFactory.IgnoreReturnValue(() => fileInfo.Length)
                .ShouldThrow<FileNotFoundException>().WithMessage(@"Could not find file '\\server\share\file.txt'.");
            fileInfo.IsReadOnly.Should().BeTrue();
            fileInfo.Attributes.Should().Be(MissingEntryAttributes);

            fileInfo.CreationTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(ZeroFileTimeUtc);
            fileInfo.LastAccessTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(ZeroFileTimeUtc);
            fileInfo.LastWriteTime.Should().Be(ZeroFileTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(ZeroFileTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"\\server\share");
        }

        [Fact]
        private void When_constructing_file_info_for_existing_remote_file_it_must_succeed()
        {
            // Arrange
            const string path = @"\\server\share\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path, DefaultContents);

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.File.ReadAllText(path);

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(path);

            // Assert
            fileInfo.Name.Should().Be("file.txt");
            fileInfo.Extension.Should().Be(".txt");
            fileInfo.FullName.Should().Be(path);
            fileInfo.DirectoryName.Should().Be(@"\\server\share");
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().Be(DefaultContents.Length);
            fileInfo.IsReadOnly.Should().BeFalse();
            fileInfo.Attributes.Should().Be(FileAttributes.Archive);

            fileInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            fileInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            fileInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"\\server\share");
        }

        [Fact]
        private void When_constructing_file_info_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructFileInfo("COM1");

            // Assert
            action.ShouldThrow<NotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact]
        private void When_constructing_file_info_for_existing_extended_local_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock { UtcNow = () => creationTimeUtc };

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingEmptyFile(path)
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(path, DefaultContents);

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.File.ReadAllText(path);

            // Act
            IFileInfo fileInfo = fileSystem.ConstructFileInfo(@"\\?\c:\some\file.txt");

            // Assert
            fileInfo.Name.Should().Be("file.txt");
            fileInfo.Extension.Should().Be(".txt");
            fileInfo.FullName.Should().Be(@"\\?\c:\some\file.txt");
            fileInfo.DirectoryName.Should().Be(@"\\?\c:\some");
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().Be(DefaultContents.Length);
            fileInfo.IsReadOnly.Should().BeFalse();
            fileInfo.Attributes.Should().Be(FileAttributes.Archive);

            fileInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            fileInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            fileInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            fileInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            fileInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            fileInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            IDirectoryInfo directoryInfo = fileInfo.Directory.ShouldNotBeNull();
            directoryInfo.FullName.Should().Be(@"\\?\c:\some");
        }
    }
}
