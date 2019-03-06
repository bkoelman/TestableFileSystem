using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class RelativePathConverter
    {
        [NotNull]
        private readonly CurrentDirectoryManager currentDirectoryManager;

        public RelativePathConverter([NotNull] CurrentDirectoryManager currentDirectoryManager)
        {
            Guard.NotNull(currentDirectoryManager, nameof(currentDirectoryManager));

            this.currentDirectoryManager = currentDirectoryManager;
        }

        [NotNull]
        public AbsolutePath ToAbsolutePath([NotNull] string path)
        {
            AssertNotNullNorWhiteSpace(path);

            string currentVolumeRoot = GetCurrentVolumeRoot();

            path = CompensatePathForRelativeDriveReference(path, currentVolumeRoot);
            path = EnsureAbsolutePathIncludesVolumeRoot(path, currentVolumeRoot);

            string currentDirectoryPath = currentDirectoryManager.GetValue().GetText();

            string rooted = CombinePaths(currentDirectoryPath, path);
            return new AbsolutePath(rooted);
        }

        [NotNull]
        private static string CombinePaths([NotNull] string path1, [NotNull] string path2)
        {
            // TODO: We should not be depending on System.IO.Path.Combine() but have our own implementation to prevent issue below.

            // NETCORE20 contains a bugfix for incorrect detection of rooted paths. The fix causes a different exception
            // to be thrown at a later time when path2 contains "_:". To preserve our existing behavior, we check for
            // the fixed scenario here.

            if (path2.Length >= 2 && path2[1] == Path.VolumeSeparatorChar && !AbsolutePath.IsDriveLetter(path2.Substring(0, 2)))
            {
                // NETCORE20 combines both paths because it considers path2 non-rooted. That may be more correct, however
                // it is not the behavior we used to get (and came to rely on).
                return path2;
            }

            return Path.Combine(path1, path2);
        }

        [AssertionMethod]
        private static void AssertNotNullNorWhiteSpace([NotNull] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw ErrorFactory.System.PathIsNotLegal(nameof(path));
            }
        }

        [NotNull]
        private string GetCurrentVolumeRoot()
        {
            return currentDirectoryManager.GetValue().VolumeName;
        }

        [NotNull]
        private static string EnsureAbsolutePathIncludesVolumeRoot([NotNull] string path, [NotNull] string volumeRoot)
        {
            return IsAbsolutePathWithoutVolumeRoot(path) ? volumeRoot + path : path;
        }

        private static bool IsAbsolutePathWithoutVolumeRoot([NotNull] string path)
        {
            if (path.Length > 0 && PathFacts.DirectorySeparatorChars.Contains(path[0]))
            {
                return path.Length == 1 || !PathFacts.DirectorySeparatorChars.Contains(path[1]);
            }

            return false;
        }

        [NotNull]
        private static string CompensatePathForRelativeDriveReference([NotNull] string path, [NotNull] string currentVolumeRoot)
        {
            if (!IsPathWithRelativeDriveReference(path))
            {
                return path;
            }

            string pathDriveLetter = path[0].ToString();
            string currentDriveLetter = currentVolumeRoot[0].ToString();

            return string.Equals(pathDriveLetter, currentDriveLetter, StringComparison.OrdinalIgnoreCase)
                ? path.Substring(2)
                : path.Substring(0, 2).ToUpperInvariant() + Path.DirectorySeparatorChar + path.Substring(2);
        }

        private static bool IsPathWithRelativeDriveReference([NotNull] string path)
        {
            bool hasVolumeSeparator = path.Length >= 2 && path[1] == Path.VolumeSeparatorChar;
            return path.Length >= 3 ? hasVolumeSeparator && !IsPathSeparator(path[2]) : hasVolumeSeparator;
        }

        private static bool IsPathSeparator(char ch)
        {
            return PathFacts.DirectorySeparatorChars.Contains(ch);
        }

        [NotNull]
        public static AbsolutePath Combine([NotNull] AbsolutePath basePath, [NotNull] string relativePath)
        {
            Guard.NotNull(basePath, nameof(basePath));
            Guard.NotNull(relativePath, nameof(relativePath));

            return CombinePathComponents(basePath, relativePath);
        }

        [NotNull]
        private static AbsolutePath CombinePathComponents([NotNull] AbsolutePath basePath, [NotNull] string relativePath)
        {
            AbsolutePath resultPath = basePath;

            foreach (string component in relativePath.Split(PathFacts.DirectorySeparatorChars))
            {
                resultPath = resultPath.Append(component);
            }

            return resultPath;
        }
    }
}
