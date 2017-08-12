using System;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Tests
{
    internal static class ActionFactory
    {
        [NotNull]
        public static Action IgnoreReturnValue<T>([CanBeNull] Func<T> func)
        {
            return () => func?.Invoke();
        }
    }
}
