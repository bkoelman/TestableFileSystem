using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes
{
    internal static class PathCombiner
    {
        [NotNull]
        public static string Combine([NotNull] string path1, [NotNull] string path2)
        {
            if (path2.Length == 0)
            {
                return path1;
            }

            if (path1.Length == 0 || IsPathRooted(path2))
            {
                return path2;
            }

            char lastChar1 = path1[path1.Length - 1];
            if (!PathFacts.DirectorySeparatorChars.Contains(lastChar1) && lastChar1 != Path.VolumeSeparatorChar)
            {
                return path1 + Path.DirectorySeparatorChar + path2;
            }

            return path1 + path2;
        }

        private static bool IsPathRooted([NotNull] string path)
        {
            return StartsWithPathSeparator(path) || StartsWithDriveLetter(path);
        }

        private static bool StartsWithPathSeparator([NotNull] string path)
        {
            return path.Length >= 1 && PathFacts.DirectorySeparatorChars.Contains(path[0]);
        }

        private static bool StartsWithDriveLetter([NotNull] string path)
        {
            return path.Length >= 2 && path[1] == Path.VolumeSeparatorChar;
        }
    }
}
