﻿#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileEncryptSpecs
    {
        #region Multi-user operations on files

        [Fact]
        private void When_getting_existence_of_file_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                bool found = fileSystem.File.Exists(path);

                // Assert
                found.Should().BeTrue();
            });
        }

        [Fact]
        private void When_recreating_file_that_was_encrypted_by_other_user_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.Create(path);

                // Assert
                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
            });
        }

        [Fact]
        private void When_recreating_file_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            using (IFileStream stream = fileSystem.File.Create(path))
            {
                // Assert
                stream.Length.Should().Be(0);
            }
        }

        [Fact]
        private void When_reading_from_file_that_was_encrypted_by_other_user_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.OpenRead(path);

                // Assert
                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
            });
        }

        [Fact]
        private void When_reading_from_file_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            using (IFileStream stream = fileSystem.File.OpenRead(path))
            {
                // Assert
                stream.CanRead.Should().BeTrue();
            }
        }

        [Fact]
        private void When_writing_to_file_that_was_encrypted_by_other_user_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.OpenWrite(path);

                // Assert
                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
            });
        }

        [Fact]
        private void When_writing_to_file_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            using (IFileStream stream = fileSystem.File.OpenWrite(path))
            {
                // Assert
                stream.CanWrite.Should().BeTrue();
            }
        }

        [Fact]
        private void When_appending_to_file_that_was_encrypted_by_other_user_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.Open(path, FileMode.Append);

                // Assert
                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
            });
        }

        [Fact]
        private void When_appending_to_file_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            using (IFileStream stream = fileSystem.File.Open(path, FileMode.Append))
            {
                // Assert
                stream.CanWrite.Should().BeTrue();
            }
        }

        [Fact]
        private void When_copying_file_that_was_encrypted_by_other_user_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"c:\folder\source.txt";
            const string targetPath = @"c:\folder\target.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(sourcePath, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(sourcePath);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.Copy(sourcePath, targetPath);

                // Assert
                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\source.txt' is denied.");
            });
        }

        [Fact]
        private void When_copying_file_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\folder\source.txt";
            const string targetPath = @"c:\folder\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(sourcePath);

            // Act
            fileSystem.File.Copy(sourcePath, targetPath);

            // Assert
            fileSystem.File.Exists(targetPath).Should().BeTrue();
        }

        [Fact]
        private void When_copying_over_file_that_was_encrypted_by_other_user_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"c:\folder\source.txt";
            const string targetPath = @"c:\folder\target.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(sourcePath, "ExampleData")
                .IncludingTextFile(targetPath, "ExistingData")
                .Build();

            fileSystem.File.Encrypt(targetPath);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.Copy(sourcePath, targetPath, true);

                // Assert
                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\target.txt' is denied.");
            });
        }

        [Fact]
        private void When_copying_over_file_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\folder\source.txt";
            const string targetPath = @"c:\folder\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "ExampleData")
                .IncludingTextFile(targetPath, "ExistingData")
                .Build();

            fileSystem.File.Encrypt(targetPath);

            // Act
            fileSystem.File.Copy(sourcePath, targetPath, true);

            // Assert
            fileSystem.File.ReadAllText(targetPath).Should().Be("ExampleData");
        }

        [Fact]
        private void When_renaming_file_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\folder\source.txt";
            const string targetPath = @"c:\folder\target.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(sourcePath, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(sourcePath);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.File.Move(sourcePath, targetPath);

                // Assert
                fileSystem.File.Exists(sourcePath).Should().BeFalse();
                fileSystem.File.Exists(targetPath).Should().BeTrue();
            });
        }

        [Fact]
        private void When_moving_file_that_was_encrypted_by_other_user_to_same_volume_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\folder\source.txt";
            const string targetPath = @"c:\folder\subfolder\target.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(sourcePath, "ExampleData")
                .IncludingDirectory(@"c:\folder\subfolder")
                .Build();

            fileSystem.File.Encrypt(sourcePath);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.File.Move(sourcePath, targetPath);

                // Assert
                fileSystem.File.Exists(sourcePath).Should().BeFalse();
                fileSystem.File.Exists(targetPath).Should().BeTrue();
            });
        }

        [Fact]
        private void When_moving_file_that_was_encrypted_by_other_user_to_different_volume_it_must_fail()
        {
            // Arrange
            const string sourcePath = @"c:\folder\source.txt";
            const string targetPath = @"d:\target.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(sourcePath, "ExampleData")
                .IncludingDirectory(@"d:\")
                .Build();

            fileSystem.File.Encrypt(sourcePath);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.Move(sourcePath, targetPath);

                // Assert
                action.Should().Throw<UnauthorizedAccessException>().WithMessage(@"Access to the path is denied.");
            });
        }

        [Fact]
        private void When_moving_file_that_was_encrypted_by_self_to_different_volume_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\folder\source.txt";
            const string targetPath = @"d:\target.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(sourcePath, "ExampleData")
                .IncludingDirectory(@"d:\")
                .Build();

            fileSystem.File.Encrypt(sourcePath);

            // Act
            fileSystem.File.Move(sourcePath, targetPath);

            // Assert
            fileSystem.File.Exists(targetPath).Should().BeTrue();
        }

        [Fact]
        private void When_deleting_file_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.File.Delete(path);

                // Assert
                fileSystem.File.Exists(path).Should().BeFalse();
            });
        }

        [Fact]
        private void When_getting_attributes_of_file_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData", attributes: FileAttributes.System)
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                FileAttributes attributes = fileSystem.File.GetAttributes(path);

                // Assert
                attributes.Should().Be(FileAttributes.System | FileAttributes.Encrypted);
            });
        }

        [Fact]
        private void When_changing_attributes_of_file_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.File.SetAttributes(path, FileAttributes.Hidden);

                // Assert
                fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Hidden | FileAttributes.Encrypted);
            });
        }

        [Fact]
        private void When_getting_creation_time_in_UTC_of_file_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var clock = new SystemClock(() => 1.January(2002));

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock, userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                DateTime time = fileSystem.File.GetCreationTimeUtc(path);

                // Assert
                time.Should().Be(1.January(2002));
            });
        }

        [Fact]
        private void When_setting_creation_time_in_UTC_of_file_that_was_encrypted_by_other_user_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.SetCreationTimeUtc(path, 1.January(2002));

                // Assert
                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
            });
        }

        [Fact]
        private void When_setting_creation_time_in_UTC_of_file_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.SetCreationTimeUtc(path, 1.January(2002));

            // Assert
            fileSystem.File.GetCreationTimeUtc(path).Should().Be(1.January(2002));
        }

        [Fact]
        private void When_getting_last_access_time_in_UTC_of_file_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var clock = new SystemClock(() => 1.January(2002));

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock, userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                DateTime time = fileSystem.File.GetLastAccessTimeUtc(path);

                // Assert
                time.Should().Be(1.January(2002));
            });
        }

        [Fact]
        private void When_setting_last_access_time_in_UTC_of_file_that_was_encrypted_by_other_user_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.SetLastAccessTimeUtc(path, 1.January(2002));

                // Assert
                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
            });
        }

        [Fact]
        private void When_setting_last_access_time_in_UTC_of_file_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.SetLastAccessTimeUtc(path, 1.January(2002));

            // Assert
            fileSystem.File.GetLastAccessTimeUtc(path).Should().Be(1.January(2002));
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_of_file_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var clock = new SystemClock(() => 1.January(2002));

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock, userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                DateTime time = fileSystem.File.GetLastWriteTimeUtc(path);

                // Assert
                time.Should().Be(1.January(2002));
            });
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_of_file_that_was_encrypted_by_other_user_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.SetLastWriteTimeUtc(path, 1.January(2002));

                // Assert
                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
            });
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_of_file_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.SetLastWriteTimeUtc(path, 1.January(2002));

            // Assert
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(1.January(2002));
        }

        [Fact]
        private void When_encrypting_file_that_was_encrypted_by_other_user_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.Encrypt(path);

                // Assert
                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
            });
        }

        [Fact]
        private void When_encrypting_file_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.Encrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void When_decrypting_file_that_was_encrypted_by_other_user_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.Decrypt(path);

                // Assert
                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
            });
        }

        [Fact]
        private void When_decrypting_file_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            IFileSystem fileSystem = new FakeFileSystemBuilder()
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.Decrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        #endregion

        #region Multi-user operations on files in encrypted directories

        [Fact]
        private void When_creating_file_in_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(@"c:\folder")
                .Build();

            fileSystem.File.Encrypt(@"c:\folder");

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                using (fileSystem.File.Create(path))
                {
                }

                // Assert
                fileSystem.File.Exists(path).Should().BeTrue();
                fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
                fileSystem.File.ReadAllText(path).Should().BeEmpty();
            });
        }

        [Fact]
        private void When_recreating_file_in_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(@"c:\folder");

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                using (fileSystem.File.Create(path))
                {
                }

                // Assert
                fileSystem.File.Exists(path).Should().BeTrue();
                fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
                fileSystem.File.ReadAllText(path).Should().BeEmpty();
            });
        }

        [Fact]
        private void When_copying_file_into_directory_that_was_encrypted_by_other_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\folder\source.txt";
            const string targetPath = @"c:\folder\target.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(sourcePath, "ExampleData")
                .Build();

            userAccount.RunImpersonated("SecondaryUser", () => { fileSystem.File.Encrypt(@"c:\folder"); });

            // Act
            fileSystem.File.Copy(sourcePath, targetPath);

            // Assert
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.GetAttributes(targetPath).Should().HaveFlag(FileAttributes.Encrypted);
            fileSystem.File.ReadAllText(targetPath).Should().NotBeEmpty();
        }

        [Fact]
        private void When_copying_over_file_into_directory_that_was_encrypted_by_other_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\folder\source.txt";
            const string targetPath = @"c:\folder\target.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(sourcePath, "ExampleData")
                .IncludingEmptyFile(targetPath)
                .Build();

            userAccount.RunImpersonated("SecondaryUser", () => { fileSystem.File.Encrypt(@"c:\folder"); });

            // Act
            fileSystem.File.Copy(sourcePath, targetPath, true);

            // Assert
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.GetAttributes(targetPath).Should().NotHaveFlag(FileAttributes.Encrypted);
            fileSystem.File.ReadAllText(targetPath).Should().NotBeEmpty();
        }

        [Fact]
        private void When_moving_file_into_directory_on_different_volume_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\sourceFolder\sourceFile.txt";
            const string targetFolder = @"d:\targetFolder";
            const string targetPath = @"d:\targetFolder\targetFile.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(sourcePath, "ExampleData")
                .IncludingDirectory(targetFolder)
                .Build();

            userAccount.RunImpersonated("SecondaryUser", () => { fileSystem.File.Encrypt(targetFolder); });

            // Act
            fileSystem.File.Move(sourcePath, targetPath);

            // Assert
            fileSystem.File.Exists(targetPath).Should().BeTrue();
            fileSystem.File.GetAttributes(targetPath).Should().HaveFlag(FileAttributes.Encrypted);
            fileSystem.File.ReadAllText(targetPath).Should().NotBeEmpty();
        }

        [Fact]
        private void When_deleting_self_encrypted_file_from_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            userAccount.RunImpersonated("SecondaryUser", () => { fileSystem.File.Encrypt(@"c:\folder"); });

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.Delete(path);

            // Assert
            fileSystem.File.Exists(path).Should().BeFalse();
        }

        [Fact]
        private void When_getting_attributes_of_file_in_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData", attributes: FileAttributes.System)
                .Build();

            userAccount.RunImpersonated("SecondaryUser", () => { fileSystem.File.Encrypt(@"c:\folder"); });

            // Act
            FileAttributes attributes = fileSystem.File.GetAttributes(path);

            // Assert
            attributes.Should().Be(FileAttributes.System);
        }

        [Fact]
        private void When_changing_attributes_of_file_in_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            userAccount.RunImpersonated("SecondaryUser", () => { fileSystem.File.Encrypt(@"c:\folder"); });

            // Act
            fileSystem.File.SetAttributes(path, FileAttributes.Hidden);

            // Assert
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Hidden);
        }

        [Fact]
        private void
            When_getting_creation_time_in_local_zone_of_file_in_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            DateTime creationTime = 1.January(2002).AsLocal();
            var clock = new SystemClock(() => creationTime);

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock, userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            userAccount.RunImpersonated("SecondaryUser", () => { fileSystem.File.Encrypt(@"c:\folder"); });

            // Act
            DateTime time = fileSystem.File.GetCreationTime(path);

            // Assert
            time.Should().Be(creationTime);
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void
            When_setting_creation_time_in_local_zone_of_file_in_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";
            DateTime creationTime = 1.January(2002).AsLocal();

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            userAccount.RunImpersonated("SecondaryUser", () => { fileSystem.File.Encrypt(@"c:\folder"); });

            // Act
            fileSystem.File.SetCreationTime(path, creationTime);

            // Assert
            fileSystem.File.GetCreationTime(path).Should().Be(creationTime);
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void
            When_getting_last_access_time_in_local_zone_of_file_in_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            DateTime lastAccessTime = 1.January(2002).AsLocal();
            var clock = new SystemClock(() => lastAccessTime);

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock, userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            userAccount.RunImpersonated("SecondaryUser", () => { fileSystem.File.Encrypt(@"c:\folder"); });

            // Act
            DateTime time = fileSystem.File.GetLastAccessTime(path);

            // Assert
            time.Should().Be(lastAccessTime);
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void
            When_setting_last_access_time_in_local_zone_of_file_in_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";
            DateTime lastAccessTime = 1.January(2002).AsLocal();

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            userAccount.RunImpersonated("SecondaryUser", () => { fileSystem.File.Encrypt(@"c:\folder"); });

            // Act
            fileSystem.File.SetLastAccessTime(path, lastAccessTime);

            // Assert
            fileSystem.File.GetLastAccessTime(path).Should().Be(lastAccessTime);
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void
            When_getting_last_write_time_in_local_zone_of_file_in_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            DateTime lastWriteTime = 1.January(2002).AsLocal();
            var clock = new SystemClock(() => lastWriteTime);

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock, userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            userAccount.RunImpersonated("SecondaryUser", () => { fileSystem.File.Encrypt(@"c:\folder"); });

            // Act
            DateTime time = fileSystem.File.GetLastWriteTime(path);

            // Assert
            time.Should().Be(lastWriteTime);
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void
            When_setting_last_write_time_in_local_zone_of_file_in_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";
            DateTime lastWriteTime = 1.January(2002).AsLocal();

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            userAccount.RunImpersonated("SecondaryUser", () => { fileSystem.File.Encrypt(@"c:\folder"); });

            // Act
            fileSystem.File.SetLastWriteTime(path, lastWriteTime);

            // Assert
            fileSystem.File.GetLastWriteTime(path).Should().Be(lastWriteTime);
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
        }

        #endregion

        #region Multi-user operations on directories

        // TODO: Existence of directory encrypted by other => returns True
        // TODO: Enumerate directory encrypted by other => works (shows unencrypted entries, encrypted entries by me and encrypted entries by other)
        // TODO: Change attributes of directory encrypted by other => updates attributes, but folder remains encrypted by other user
        // TODO: Get/set timings of directory encrypted by other => Get works, set FAILS!
        // TODO: Rename/move where source directory is encrypted by other (same drive) => works (no changes in crypto owner)
        // TODO: Delete directory encrypted by other => deletes directory (when recursive, even works if it contains files encrypted by other)
        // TODO: Create subdirectory in directory encrypted by other => gets encrypted for me
        // TODO: Encrypt directory encrypted by other => works, but does nothing (not change owner)
        // TODO: Encrypt directory encrypted by self => works, but does nothing
        // TODO: Decrypt directory encrypted by other => un-marks directory encrypted (does not touch files/subdirectories)
        // TODO: Decrypt directory encrypted by self => un-marks directory encrypted (does not touch files/subdirectories)

        #endregion

        // TODO: Add more tests...
    }
}
#endif
