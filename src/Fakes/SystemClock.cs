using System;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class SystemClock
    {
        [NotNull]
        public static readonly SystemClock Default = new SystemClock();

        [NotNull]
        public Func<DateTime> UtcNow = () => DateTime.UtcNow;

        private SystemClock()
        {
        }

        public SystemClock([NotNull] Func<DateTime> utcNow)
        {
            Guard.NotNull(utcNow, nameof(utcNow));
            UtcNow = utcNow;
        }
    }
}
