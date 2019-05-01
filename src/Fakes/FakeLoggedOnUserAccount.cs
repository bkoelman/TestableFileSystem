using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeLoggedOnUserAccount : ILoggedOnUserAccount
    {
        [NotNull]
        [ItemNotNull]
        private readonly Stack<string> userScopes = new Stack<string>();

        public string UserName => userScopes.Peek();

        public FakeLoggedOnUserAccount([NotNull] string userName)
        {
            Guard.NotNull(userName, nameof(userName));

            userScopes.Push(userName);
        }

        public void RunImpersonated([NotNull] string userName, [NotNull] Action action)
        {
            Guard.NotNull(action, nameof(action));
            Guard.NotNull(userName, nameof(userName));

            userScopes.Push(userName);
            try
            {
                action();
            }
            finally
            {
                userScopes.Pop();
            }
        }
    }
}
