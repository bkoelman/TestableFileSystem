using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class FileReplaceArguments
    {
        [NotNull]
        public AbsolutePath SourcePath { get; }

        [NotNull]
        public AbsolutePath DestinationPath { get; }

        [CanBeNull]
        public AbsolutePath BackupDestinationPath { get; }

        public FileReplaceArguments([NotNull] AbsolutePath sourcePath, [NotNull] AbsolutePath destinationPath,
            [CanBeNull] AbsolutePath backupDestinationPath)
        {
            Guard.NotNull(sourcePath, nameof(sourcePath));
            Guard.NotNull(destinationPath, nameof(destinationPath));

            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            BackupDestinationPath = backupDestinationPath;
        }
    }
}
