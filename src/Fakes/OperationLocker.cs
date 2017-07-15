using System;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public abstract class OperationLocker
    {
        [NotNull]
        private readonly FakeFileSystem owner;

        protected OperationLocker([NotNull] FakeFileSystem owner)
        {
            this.owner = owner;
            Guard.NotNull(owner, nameof(owner));
        }

        protected void ExecuteInLock([NotNull] Action operation)
        {
            Guard.NotNull(operation, nameof(operation));

            lock (owner.TreeLock)
            {
                operation();
            }
        }

        [NotNull]
        protected TResult ExecuteInLock<TResult>([NotNull] Func<TResult> operation)
        {
            Guard.NotNull(operation, nameof(operation));

            lock (owner.TreeLock)
            {
                return operation();
            }
        }
    }
}
