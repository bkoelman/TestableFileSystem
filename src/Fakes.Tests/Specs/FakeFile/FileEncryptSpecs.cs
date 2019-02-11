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
    public sealed class FileEncryptSpecs
    {
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
        private void When_creating_file_that_was_encrypted_by_other_user_it_must_fail()
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
        private void When_creating_file_that_was_encrypted_by_self_it_must_succeed()
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
                .IncludingTextFile(path, "ExampleData", attributes: FileAttributes.System)
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
                .IncludingTextFile(path, "ExampleData", attributes: FileAttributes.System)
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
                .IncludingTextFile(path, "ExampleData", attributes: FileAttributes.System)
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
                .IncludingTextFile(path, "ExampleData", attributes: FileAttributes.System)
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
                .IncludingTextFile(path, "ExampleData", attributes: FileAttributes.System)
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
                .IncludingTextFile(path, "ExampleData", attributes: FileAttributes.System)
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
                .IncludingTextFile(path, "ExampleData", attributes: FileAttributes.System)
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
                .IncludingTextFile(path, "ExampleData", attributes: FileAttributes.System)
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
                .IncludingTextFile(path, "ExampleData", attributes: FileAttributes.System)
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
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Encrypted | FileAttributes.Archive);
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
            fileSystem.File.GetAttributes(path).Should().Be(FileAttributes.Archive);
        }

        // TODO: Investigate directory operations.

        // TODO: Add more tests...
    }
}
#endif
