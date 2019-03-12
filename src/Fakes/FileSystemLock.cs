using System;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class FileSystemLock
    {
        [NotNull]
        private readonly object treeLock = new object();

        public void ExecuteInLock([NotNull] Action operation)
        {
            Guard.NotNull(operation, nameof(operation));

            lock (treeLock)
            {
                operation();
            }
        }

        [NotNull]
        public TResult ExecuteInLock<TResult>([NotNull] Func<TResult> operation)
        {
            Guard.NotNull(operation, nameof(operation));

            lock (treeLock)
            {
                return operation();
            }
        }
    }
}
