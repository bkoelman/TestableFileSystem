using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileMoveArguments
    {
        [NotNull]
        public AbsolutePath SourcePath { get; }

        [NotNull]
        public AbsolutePath DestinationPath { get; }

        public FileMoveArguments([NotNull] AbsolutePath sourcePath, [NotNull] AbsolutePath destinationPath)
        {
            Guard.NotNull(sourcePath, nameof(sourcePath));
            Guard.NotNull(destinationPath, nameof(destinationPath));

            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }
    }
}
