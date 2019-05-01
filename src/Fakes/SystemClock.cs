using System;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    public sealed class SystemClock
    {
        [NotNull]
        public static readonly Func<DateTime> UseHardwareClock = () => DateTime.UtcNow;

        [NotNull]
        public Func<DateTime> UtcNow { get; set; }

        public SystemClock([NotNull] Func<DateTime> utcNow)
        {
            Guard.NotNull(utcNow, nameof(utcNow));
            UtcNow = utcNow;
        }
    }
}
