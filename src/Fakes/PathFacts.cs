using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes
{
    internal static class PathFacts
    {
        [NotNull]
        public static readonly char[] DirectorySeparatorChars = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        [NotNull]
        public static readonly string TwoDirectorySeparators = new string(Path.DirectorySeparatorChar, 2);
    }
}
