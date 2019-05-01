using System;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Tests
{
    internal static class ActionFactory
    {
        [NotNull]
        public static Action IgnoreReturnValue<T>([InstantHandle] [CanBeNull] Func<T> func)
        {
            return () => func?.Invoke();
        }
    }
}
