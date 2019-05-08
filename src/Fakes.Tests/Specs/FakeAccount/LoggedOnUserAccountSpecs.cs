using FluentAssertions;
using TestableFileSystem.Fakes.Tests.TestAttributes;
using Xunit;

namespace TestableFileSystem.Fakes.Tests.Specs.FakeAccount
{
    public sealed class LoggedOnUserAccountSpecs
    {
        [Fact, InvestigateRunOnFileSystem]
        private void When_impersonating_user_it_must_succeed()
        {
            // Arrange
            const string userJohn = "John";
            const string userJane = "Jane";
            const string userJack = "Jack";

            var userAccount = new FakeLoggedOnUserAccount(userJohn);

            userAccount.UserName.Should().Be(userJohn);

            // Act
            userAccount.RunImpersonated(userJane, () =>
            {
                userAccount.UserName.Should().Be(userJane);

                userAccount.RunImpersonated(userJack, () =>
                {
                    // Assert
                    userAccount.UserName.Should().Be(userJack);
                });

                userAccount.UserName.Should().Be(userJane);
            });

            userAccount.UserName.Should().Be(userJohn);
        }
    }
}
