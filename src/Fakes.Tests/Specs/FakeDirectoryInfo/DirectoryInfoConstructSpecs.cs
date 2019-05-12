using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeDirectoryInfo
{
    public sealed class DirectoryInfoConstructSpecs
    {
        private const FileAttributes MissingEntryAttributes = (FileAttributes)(-1);
        private static readonly DateTime ZeroFileTimeUtc = 1.January(1601).AsUtc();

        [Theory]
        [CanRunOnFileSystem]
        private void When_constructing_directory_info_for_null_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                Action action = () => fileSystem.ConstructDirectoryInfo(null);

                // Assert
                action.Should().ThrowExactly<ArgumentNullException>();
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_constructing_directory_info_for_empty_string_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.ConstructDirectoryInfo(string.Empty);

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_constructing_directory_info_for_whitespace_it_must_fail(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                Action action = () => fileSystem.ConstructDirectoryInfo(" ");

                // Assert
                action.Should().ThrowExactly<ArgumentException>().WithMessage("The path is empty.*");
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_invalid_drive_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructDirectoryInfo("_:");

            // Assert
            action.Should().ThrowExactly<NotSupportedException>().WithMessage("The given path's format is not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_wildcard_characters_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructDirectoryInfo("some?.txt");

            // Assert
            action.Should().ThrowExactly<ArgumentException>().WithMessage("Illegal characters in path.*");
        }

        [Fact, InvestigateRunOnFileSystem]
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

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be("some");
            parentInfo.FullName.Should().Be(@"c:\some");
            parentInfo.ToString().Should().Be("some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"c:\");
            rootInfo.FullName.Should().Be(@"c:\");
            rootInfo.ToString().Should().Be(@"c:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_existing_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

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

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be("some");
            parentInfo.FullName.Should().Be(@"c:\some");
            parentInfo.ToString().Should().Be("some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"c:\");
            rootInfo.FullName.Should().Be(@"c:\");
            rootInfo.ToString().Should().Be(@"c:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_existing_local_directory_with_different_casing_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\SOME\folDER";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

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

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be("SOME");
            parentInfo.FullName.Should().Be(@"c:\SOME");
            parentInfo.ToString().Should().Be("SOME");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"c:\");
            rootInfo.FullName.Should().Be(@"c:\");
            rootInfo.ToString().Should().Be(@"c:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_existing_local_directory_with_trailing_whitespace_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder  ";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

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

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be("some");
            parentInfo.FullName.Should().Be(@"c:\some");
            parentInfo.ToString().Should().Be("some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"c:\");
            rootInfo.FullName.Should().Be(@"c:\");
            rootInfo.ToString().Should().Be(@"c:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_existing_local_directory_with_trailing_separator_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\folder\";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

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
            dirInfo.FullName.Should().Be(@"c:\some\folder\");
            dirInfo.Exists.Should().BeTrue();
            dirInfo.Attributes.Should().Be(FileAttributes.Directory);

            dirInfo.CreationTime.Should().Be(creationTimeUtc.ToLocalTime());
            dirInfo.CreationTimeUtc.Should().Be(creationTimeUtc);
            dirInfo.LastAccessTime.Should().Be(lastAccessTimeUtc.ToLocalTime());
            dirInfo.LastAccessTimeUtc.Should().Be(lastAccessTimeUtc);
            dirInfo.LastWriteTime.Should().Be(lastWriteTimeUtc.ToLocalTime());
            dirInfo.LastWriteTimeUtc.Should().Be(lastWriteTimeUtc);

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be("some");
            parentInfo.FullName.Should().Be(@"c:\some");
            parentInfo.ToString().Should().Be("some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"c:\");
            rootInfo.FullName.Should().Be(@"c:\");
            rootInfo.ToString().Should().Be(@"c:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_existing_local_file_with_self_and_parent_references_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\.\deleted\..\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

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

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be("some");
            parentInfo.FullName.Should().Be(@"c:\some");
            parentInfo.ToString().Should().Be("some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"c:\");
            rootInfo.FullName.Should().Be(@"c:\");
            rootInfo.ToString().Should().Be(@"c:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_root_of_drive_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

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

            dirInfo.ToString().Should().Be(path);

            dirInfo.Parent.Should().BeNull();

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"c:\");
            rootInfo.FullName.Should().Be(@"c:\");
            rootInfo.ToString().Should().Be(@"c:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_directory_using_absolute_path_without_drive_letter_it_must_succeed()
        {
            // Arrange
            const string path = @"\some\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\other")
                .IncludingDirectory(@"c:\some\folder")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(@"c:\some\folder" + @"\file.txt", "X");

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.Directory.GetFiles(@"c:\some\folder");

            fileSystem.Directory.SetCurrentDirectory(@"C:\other");

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

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

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be("some");
            parentInfo.FullName.Should().Be(@"C:\some");
            parentInfo.ToString().Should().Be("some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"C:\");
            rootInfo.FullName.Should().Be(@"C:\");
            rootInfo.ToString().Should().Be(@"C:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_existing_relative_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\some\folder")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(@"c:\some\folder" + @"\file.txt", "X");

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.Directory.GetFiles(@"c:\some\folder");

            fileSystem.Directory.SetCurrentDirectory(@"C:\some");

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

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

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be("some");
            parentInfo.FullName.Should().Be(@"C:\some");
            parentInfo.ToString().Should().Be("some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"C:\");
            rootInfo.FullName.Should().Be(@"C:\");
            rootInfo.ToString().Should().Be(@"C:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_existing_local_file_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\some\file.txt";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

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

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be("some");
            parentInfo.FullName.Should().Be(@"c:\some");
            parentInfo.ToString().Should().Be("some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"c:\");
            rootInfo.FullName.Should().Be(@"c:\");
            rootInfo.ToString().Should().Be(@"c:\");
        }

        [Fact, InvestigateRunOnFileSystem]
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

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be("file.txt");
            parentInfo.FullName.Should().Be(@"C:\some\file.txt");
            parentInfo.ToString().Should().Be("file.txt");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"C:\");
            rootInfo.FullName.Should().Be(@"C:\");
            rootInfo.ToString().Should().Be(@"C:\");
        }

        [Fact, InvestigateRunOnFileSystem]
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

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be("nested.html");
            parentInfo.FullName.Should().Be(@"C:\some\file.txt\nested.html");
            parentInfo.ToString().Should().Be("nested.html");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"C:\");
            rootInfo.FullName.Should().Be(@"C:\");
            rootInfo.ToString().Should().Be(@"C:\");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_missing_parent_directory_it_must_succeed()
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

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be("some");
            parentInfo.FullName.Should().Be(@"C:\some");
            parentInfo.ToString().Should().Be("some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"C:\");
            rootInfo.FullName.Should().Be(@"C:\");
            rootInfo.ToString().Should().Be(@"C:\");
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_constructing_directory_info_for_missing_network_share_it_must_succeed(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkShare(), false);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

                // Assert
                dirInfo.Name.Should().Be(path);
                dirInfo.Extension.Should().BeEmpty();
                dirInfo.FullName.Should().Be(path);
                dirInfo.Exists.Should().BeFalse();
                ActionFactory.IgnoreReturnValue(() => dirInfo.Attributes).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");

                ActionFactory.IgnoreReturnValue(() => dirInfo.CreationTime).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");
                ActionFactory.IgnoreReturnValue(() => dirInfo.CreationTimeUtc).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");
                ActionFactory.IgnoreReturnValue(() => dirInfo.LastAccessTime).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");
                ActionFactory.IgnoreReturnValue(() => dirInfo.LastAccessTimeUtc).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");
                ActionFactory.IgnoreReturnValue(() => dirInfo.LastWriteTime).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");
                ActionFactory.IgnoreReturnValue(() => dirInfo.LastWriteTimeUtc).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");

                dirInfo.ToString().Should().Be(path);

                dirInfo.Parent.Should().BeNull();

                IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
                rootInfo.Name.Should().Be(path);
                rootInfo.FullName.Should().Be(path);
                rootInfo.ToString().Should().Be(path);
            }
        }

        [Theory]
        [CanRunOnFileSystem]
        private void When_constructing_directory_info_for_directory_below_missing_network_share_it_must_succeed(bool useFakes)
        {
            using (var factory = new FileSystemBuilderFactory(useFakes))
            {
                // Arrange
                string path = factory.MapPath(PathFactory.NetworkDirectoryAtDepth(1), false);
                string parentPath = factory.MapPath(PathFactory.NetworkShare(), false);

                IFileSystem fileSystem = factory.Create()
                    .Build();

                // Act
                IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

                // Assert
                dirInfo.Name.Should().Be(PathFactory.DirectoryNameAtDepth1);
                dirInfo.Extension.Should().BeEmpty();
                dirInfo.FullName.Should().Be(path);
                dirInfo.Exists.Should().BeFalse();
                ActionFactory.IgnoreReturnValue(() => dirInfo.Attributes).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");

                ActionFactory.IgnoreReturnValue(() => dirInfo.CreationTime).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");
                ActionFactory.IgnoreReturnValue(() => dirInfo.CreationTimeUtc).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");
                ActionFactory.IgnoreReturnValue(() => dirInfo.LastAccessTime).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");
                ActionFactory.IgnoreReturnValue(() => dirInfo.LastAccessTimeUtc).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");
                ActionFactory.IgnoreReturnValue(() => dirInfo.LastWriteTime).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");
                ActionFactory.IgnoreReturnValue(() => dirInfo.LastWriteTimeUtc).Should().ThrowExactly<IOException>()
                    .WithMessage($"The network path was not found. : '{path}'");

                dirInfo.ToString().Should().Be(path);

                IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
                parentInfo.Name.Should().Be(parentPath);
                parentInfo.FullName.Should().Be(parentPath);
                parentInfo.ToString().Should().Be(parentPath);

                IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
                rootInfo.Name.Should().Be(parentPath);
                rootInfo.FullName.Should().Be(parentPath);
                rootInfo.ToString().Should().Be(parentPath);
            }
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_missing_remote_directory_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);
            string parentPath = PathFactory.NetworkShare();

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingDirectory(parentPath)
                .Build();

            // Act
            IDirectoryInfo dirInfo = fileSystem.ConstructDirectoryInfo(path);

            // Assert
            dirInfo.Name.Should().Be(PathFactory.DirectoryNameAtDepth1);
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

            dirInfo.ToString().Should().Be(path);

            IDirectoryInfo parentInfo = dirInfo.Parent.ShouldNotBeNull();
            parentInfo.Name.Should().Be(parentPath);
            parentInfo.FullName.Should().Be(parentPath);
            parentInfo.ToString().Should().Be(parentPath);

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(parentPath);
            rootInfo.FullName.Should().Be(parentPath);
            rootInfo.ToString().Should().Be(parentPath);
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_existing_remote_directory_it_must_succeed()
        {
            // Arrange
            string path = PathFactory.NetworkDirectoryAtDepth(1);

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

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
            dirInfo.Name.Should().Be(PathFactory.DirectoryNameAtDepth1);
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
            parentInfo.Name.Should().Be(PathFactory.NetworkShare());
            parentInfo.FullName.Should().Be(PathFactory.NetworkShare());
            parentInfo.ToString().Should().Be(PathFactory.NetworkShare());

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(PathFactory.NetworkShare());
            rootInfo.FullName.Should().Be(PathFactory.NetworkShare());
            rootInfo.ToString().Should().Be(PathFactory.NetworkShare());
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_reserved_name_it_must_fail()
        {
            // Arrange
            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .Build();

            // Act
            Action action = () => fileSystem.ConstructDirectoryInfo("COM1");

            // Assert
            action.Should().ThrowExactly<PlatformNotSupportedException>().WithMessage("Reserved names are not supported.");
        }

        [Fact, InvestigateRunOnFileSystem]
        private void When_constructing_directory_info_for_existing_extended_local_directory_it_must_succeed()
        {
            // Arrange
            const string path = @"\\?\c:\some\folder";

            DateTime creationTimeUtc = 17.March(2006).At(14, 03, 53).AsUtc();
            var clock = new SystemClock(() => creationTimeUtc);

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock)
                .IncludingDirectory(@"c:\some\folder")
                .Build();

            DateTime lastWriteTimeUtc = 18.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastWriteTimeUtc;

            fileSystem.File.WriteAllText(@"c:\some\folder" + @"\file.txt", "X");

            DateTime lastAccessTimeUtc = 19.March(2006).At(14, 03, 53).AsUtc();
            clock.UtcNow = () => lastAccessTimeUtc;

            fileSystem.Directory.GetFiles(@"c:\some\folder");

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
            parentInfo.Name.Should().Be("some");
            parentInfo.FullName.Should().Be(@"\\?\c:\some");
            parentInfo.ToString().Should().Be("some");

            IDirectoryInfo rootInfo = dirInfo.Root.ShouldNotBeNull();
            rootInfo.Name.Should().Be(@"\\?\c:\");
            rootInfo.FullName.Should().Be(@"\\?\c:\");
            rootInfo.ToString().Should().Be(@"\\?\c:\");
        }
    }
}
