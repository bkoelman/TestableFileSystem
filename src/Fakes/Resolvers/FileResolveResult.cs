using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Resolvers
{
    internal sealed class FileResolveResult
    {
        [NotNull]
        public DirectoryEntry ContainingDirectory { get; }

        [CanBeNull]
        public FileEntry ExistingFileOrNull { get; }

        [NotNull]
        public string FileName { get; }

        public FileResolveResult([NotNull] DirectoryEntry containingDirectory, [CanBeNull] FileEntry existingFileOrNull,
            [NotNull] string fileName)
        {
            Guard.NotNull(containingDirectory, nameof(containingDirectory));
            Guard.NotNull(fileName, nameof(fileName));
            ContainingDirectory = containingDirectory;
            ExistingFileOrNull = existingFileOrNull;
            FileName = fileName;
        }
    }
}
