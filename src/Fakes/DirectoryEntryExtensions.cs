using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal static class DirectoryEntryExtensions
    {
        [CanBeNull]
        public static AbsolutePath TryGetPathOfFirstOpenFile([NotNull] this DirectoryEntry directory, [NotNull] AbsolutePath path)
        {
            Guard.NotNull(directory, nameof(directory));
            Guard.NotNull(path, nameof(path));

            FileEntry file = directory.Files.Values.FirstOrDefault(x => x.IsOpen());

            if (file != null)
            {
                return path.Append(file.Name);
            }

            foreach (DirectoryEntry subdirectory in directory.Directories.Values)
            {
                AbsolutePath subdirectoryPath = path.Append(subdirectory.Name);

                AbsolutePath openFilePath = TryGetPathOfFirstOpenFile(subdirectory, subdirectoryPath);
                if (openFilePath != null)
                {
                    return openFilePath;
                }
            }

            return null;
        }
    }
}
