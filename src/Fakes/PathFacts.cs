using System;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes
{
    internal static class PathFacts
    {
        public static readonly DateTime ZeroFileTime = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
        public static readonly DateTime ZeroFileTimeUtc = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [NotNull]
        public static readonly char[] DirectorySeparatorChars = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
    }
}
