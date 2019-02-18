using System;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal abstract class OperationLocker
    {
        [NotNull]
        private readonly object treeLock;

        protected OperationLocker([NotNull] object treeLock)
        {
            Guard.NotNull(treeLock, nameof(treeLock));
            this.treeLock = treeLock;
        }

        protected void ExecuteInLock([NotNull] Action operation)
        {
            Guard.NotNull(operation, nameof(operation));

            lock (treeLock)
            {
                operation();
            }
        }

        [NotNull]
        protected TResult ExecuteInLock<TResult>([NotNull] Func<TResult> operation)
        {
            Guard.NotNull(operation, nameof(operation));

            lock (treeLock)
            {
                return operation();
            }
        }
    }
}
