using JetBrains.Annotations;
using System;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    public static class Utilities
    {
        [CanBeNull]
        public static TOut WrapOrNull<TIn, TOut>([CanBeNull] TIn source, [NotNull] Func<TIn, TOut> wrapper)
            where TIn : class
            where TOut : class
        {
            Guard.NotNull(wrapper, nameof(wrapper));
            return source == null ? null : wrapper(source);
        }
    }
}
