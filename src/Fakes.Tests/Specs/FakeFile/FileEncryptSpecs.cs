#if !NETCOREAPP1_1
using System;
using FluentAssertions;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeFile
{
    public sealed class FileEncryptSpecs
    {
        // TODO: Add more tests...

        [Fact]
        private void When_reading_file_that_was_encrypted_by_other_user_it_must_fail()
        {
            // Arrange
            const string path = @"c:\folder\file.txt";

            var userScope = new FakeLoggedOnUserAccount("PrimaryUser");

            IFileSystem fileSystem = new FakeFileSystemBuilder(userScope)
                .IncludingTextFile(path, "ExampleData")
                .Build();

            fileSystem.File.Encrypt(path);

            userScope.RunImpersonated("SecondaryUser", () =>
            {
                // Act
                Action action = () => fileSystem.File.OpenRead(path);

                // Assert
                action.Should().Throw<UnauthorizedAccessException>()
                    .WithMessage(@"Access to the path 'c:\folder\file.txt' is denied.");
            });
        }
    }
}
#endif
