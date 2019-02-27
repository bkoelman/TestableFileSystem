#if !NETCOREAPP1_1
using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileCryptoSpecs
    {
        // TODO: Add specs for File.Replace.

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
                Action action1 = () => fileSystem.File.SetCreationTimeUtc(path, 1.January(2002));
                Action action2 = () => fileSystem.Directory.SetCreationTimeUtc(path, 1.January(2002));

                // Assert
                action1.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
                action2.Should().Throw<UnauthorizedAccessException>()
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
                Action action1 = () => fileSystem.File.SetLastAccessTimeUtc(path, 1.January(2002));
                Action action2 = () => fileSystem.Directory.SetLastAccessTimeUtc(path, 1.January(2002));

                // Assert
                action1.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
                action2.Should().Throw<UnauthorizedAccessException>()
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
                Action action1 = () => fileSystem.File.SetLastWriteTimeUtc(path, 1.January(2002));
                Action action2 = () => fileSystem.Directory.SetLastWriteTimeUtc(path, 1.January(2002));

                // Assert
                action1.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
                action2.Should().Throw<UnauthorizedAccessException>()
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
        private void When_recreating_unencrypted_file_in_directory_that_was_encrypted_by_other_user_it_must_succeed()
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
                fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
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

        [Fact]
        private void When_getting_existence_of_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(path)
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                bool found = fileSystem.Directory.Exists(path);

                // Assert
                found.Should().BeTrue();
            });
        }

        [Fact]
        private void When_enumerating_entries_of_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(Path.Combine(path, "subfolder"))
                .IncludingDirectory(Path.Combine(path, "pri-subfolder"))
                .IncludingDirectory(Path.Combine(path, "sec-subfolder"))
                .IncludingEmptyFile(Path.Combine(path, "file.txt"))
                .IncludingEmptyFile(Path.Combine(path, "pri-file.txt"))
                .IncludingEmptyFile(Path.Combine(path, "sec-file.txt"))
                .Build();

            fileSystem.File.Encrypt(path);
            fileSystem.File.Encrypt(Path.Combine(path, "pri-subfolder"));
            fileSystem.File.Encrypt(Path.Combine(path, "pri-file.txt"));

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                fileSystem.File.Encrypt(Path.Combine(path, "sec-subfolder"));
                fileSystem.File.Encrypt(Path.Combine(path, "sec-file.txt"));

                // Act
                string[] entries = fileSystem.Directory.GetFileSystemEntries(path);

                // Assert
                entries.Should().HaveCount(6);
                entries.Should().Contain(Path.Combine(path, "subfolder"));
                entries.Should().Contain(Path.Combine(path, "pri-subfolder"));
                entries.Should().Contain(Path.Combine(path, "sec-subfolder"));
                entries.Should().Contain(Path.Combine(path, "file.txt"));
                entries.Should().Contain(Path.Combine(path, "pri-file.txt"));
                entries.Should().Contain(Path.Combine(path, "sec-file.txt"));
            });
        }

        [Fact]
        private void When_renaming_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\sourceFolder";
            const string targetPath = @"c:\targetFolder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(sourcePath)
                .Build();

            fileSystem.File.Encrypt(sourcePath);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.Directory.Move(sourcePath, targetPath);

                // Assert
                fileSystem.Directory.Exists(sourcePath).Should().BeFalse();
                fileSystem.Directory.Exists(targetPath).Should().BeTrue();
                fileSystem.File.GetAttributes(targetPath).Should().HaveFlag(FileAttributes.Encrypted);
            });
        }

        [Fact]
        private void When_moving_directory_that_was_encrypted_by_other_user_to_same_volume_it_must_succeed()
        {
            // Arrange
            const string sourcePath = @"c:\parent\sourceFolder";
            const string targetPath = @"c:\targetFolder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(sourcePath)
                .Build();

            fileSystem.File.Encrypt(sourcePath);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.Directory.Move(sourcePath, targetPath);

                // Assert
                fileSystem.Directory.Exists(sourcePath).Should().BeFalse();
                fileSystem.Directory.Exists(targetPath).Should().BeTrue();
                fileSystem.File.GetAttributes(targetPath).Should().HaveFlag(FileAttributes.Encrypted);
            });
        }

        [Fact]
        private void When_deleting_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(path)
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.Directory.Delete(path);

                // Assert
                fileSystem.Directory.Exists(path).Should().BeFalse();
            });
        }

        [Fact]
        private void When_deleting_directory_recursively_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(path)
                .IncludingDirectory(Path.Combine(path, "subfolder"))
                .IncludingTextFile(Path.Combine(path, "file.txt"), "Created_by_Primary_User")
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.Directory.Delete(path, true);

                // Assert
                fileSystem.Directory.Exists(path).Should().BeFalse();
            });
        }

        [Fact]
        private void When_creating_subdirectory_in_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string parentFath = @"c:\folder";
            string subFolder = Path.Combine(parentFath, "subfolder");

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(parentFath)
                .Build();

            fileSystem.File.Encrypt(parentFath);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.Directory.CreateDirectory(subFolder);

                // Assert
                fileSystem.Directory.Exists(subFolder).Should().BeTrue();
                fileSystem.File.GetAttributes(subFolder).Should().HaveFlag(FileAttributes.Encrypted);
            });
        }

        [Fact]
        private void When_changing_attributes_of_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(path)
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

                // Assert
                fileSystem.File.GetAttributes(path).Should().Be(
                    FileAttributes.Directory | FileAttributes.Encrypted | FileAttributes.ReadOnly);
            });
        }

        [Fact]
        private void When_getting_creation_time_in_UTC_of_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var clock = new SystemClock(() => 1.January(2002));

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock, userAccount)
                .IncludingDirectory(path)
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                DateTime time = fileSystem.Directory.GetCreationTimeUtc(path);

                // Assert
                time.Should().Be(1.January(2002));
            });
        }

        [Fact]
        private void When_setting_creation_time_in_UTC_of_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(path)
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.Directory.SetCreationTimeUtc(path, 1.January(2002));

                Action action = () => fileSystem.File.SetCreationTimeUtc(path, 1.January(2002));

                // Assert
                fileSystem.Directory.GetCreationTimeUtc(path).Should().Be(1.January(2002));

                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder' is denied.");
            });
        }

        [Fact]
        private void When_getting_last_access_time_in_UTC_of_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var clock = new SystemClock(() => 1.January(2002));

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock, userAccount)
                .IncludingDirectory(path)
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                DateTime time = fileSystem.Directory.GetLastAccessTimeUtc(path);

                // Assert
                time.Should().Be(1.January(2002));
            });
        }

        [Fact]
        private void When_setting_last_access_time_in_UTC_of_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(path)
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.Directory.SetLastAccessTimeUtc(path, 1.January(2002));

                Action action = () => fileSystem.File.SetLastAccessTimeUtc(path, 1.January(2002));

                // Assert
                fileSystem.Directory.GetLastAccessTimeUtc(path).Should().Be(1.January(2002));

                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder' is denied.");
            });
        }

        [Fact]
        private void When_getting_last_write_time_in_UTC_of_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var clock = new SystemClock(() => 1.January(2002));

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(clock, userAccount)
                .IncludingDirectory(path)
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                DateTime time = fileSystem.Directory.GetLastWriteTimeUtc(path);

                // Assert
                time.Should().Be(1.January(2002));
            });
        }

        [Fact]
        private void When_setting_last_write_time_in_UTC_of_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(path)
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.Directory.SetLastWriteTimeUtc(path, 1.January(2002));

                Action action = () => fileSystem.File.SetLastWriteTimeUtc(path, 1.January(2002));

                // Assert
                fileSystem.Directory.GetLastWriteTimeUtc(path).Should().Be(1.January(2002));

                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder' is denied.");
            });
        }

        [Fact]
        private void When_encrypting_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(path)
                .Build();

            fileSystem.File.Encrypt(path);

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.File.Encrypt(path);

                // Assert
                fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
            });
        }

        [Fact]
        private void When_encrypting_directory_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(path)
                .Build();

            fileSystem.File.Encrypt(path);

            // Act
            fileSystem.File.Encrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().HaveFlag(FileAttributes.Encrypted);
        }

        [Fact]
        private void When_decrypting_directory_that_was_encrypted_by_other_user_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(path)
                .IncludingDirectory(Path.Combine(path, "subfolder"))
                .IncludingEmptyFile(Path.Combine(path, "file.txt"))
                .Build();

            fileSystem.File.Encrypt(path);
            fileSystem.File.Encrypt(Path.Combine(path, "subfolder"));
            fileSystem.File.Encrypt(Path.Combine(path, "file.txt"));

            userAccount.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                fileSystem.File.Decrypt(path);

                // Assert
                fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
                fileSystem.File.GetAttributes(Path.Combine(path, "subfolder")).Should().HaveFlag(FileAttributes.Encrypted);
                fileSystem.File.GetAttributes(Path.Combine(path, "file.txt")).Should().HaveFlag(FileAttributes.Encrypted);
            });
        }

        [Fact]
        private void When_decrypting_directory_that_was_encrypted_by_self_it_must_succeed()
        {
            // Arrange
            const string path = @"c:\folder";

            var userAccount = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userAccount)
                .IncludingDirectory(path)
                .IncludingDirectory(Path.Combine(path, "subfolder"))
                .IncludingEmptyFile(Path.Combine(path, "file.txt"))
                .Build();

            fileSystem.File.Encrypt(path);
            fileSystem.File.Encrypt(Path.Combine(path, "subfolder"));
            fileSystem.File.Encrypt(Path.Combine(path, "file.txt"));

            // Act
            fileSystem.File.Decrypt(path);

            // Assert
            fileSystem.File.GetAttributes(path).Should().NotHaveFlag(FileAttributes.Encrypted);
            fileSystem.File.GetAttributes(Path.Combine(path, "subfolder")).Should().HaveFlag(FileAttributes.Encrypted);
            fileSystem.File.GetAttributes(Path.Combine(path, "file.txt")).Should().HaveFlag(FileAttributes.Encrypted);
        }

        #endregion
    }
}
#endif
