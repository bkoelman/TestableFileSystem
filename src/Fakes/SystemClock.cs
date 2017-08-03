using System;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes
{
    public sealed class SystemClock
    {
        [NotNull]
        public Func<DateTime> UtcNow = () => DateTime.UtcNow;
    }
}
