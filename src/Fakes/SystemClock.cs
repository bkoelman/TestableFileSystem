using System;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes
{
    public sealed class SystemClock
    {
        // TODO: Remove this member.
        [NotNull]
        public Func<DateTime> Now = () => DateTime.Now;

        [NotNull]
        public Func<DateTime> UtcNow = () => DateTime.UtcNow;
    }
}
