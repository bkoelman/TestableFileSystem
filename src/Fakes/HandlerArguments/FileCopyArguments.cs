using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class FileCopyArguments
    {
        [NotNull]
        public AbsolutePath SourcePath { get; }

        [NotNull]
        public AbsolutePath DestinationPath { get; }

        public bool Overwrite { get; }
        public bool IsCopyAfterMoveFailed { get; }

        public FileCopyArguments([NotNull] AbsolutePath sourcePath, [NotNull] AbsolutePath destinationPath, bool overwrite,
            bool isCopyAfterMoveFailed)
        {
            Guard.NotNull(sourcePath, nameof(sourcePath));
            Guard.NotNull(destinationPath, nameof(destinationPath));

            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            Overwrite = overwrite;
            IsCopyAfterMoveFailed = isCopyAfterMoveFailed;
        }
    }
}
