using System;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes
{
    public static class SystemClock
    {
        [NotNull]
        public static Func<DateTime> Now = () => DateTime.Now;

        [NotNull]
        public static Func<DateTime> UtcNow = () => DateTime.UtcNow;
    }
}
