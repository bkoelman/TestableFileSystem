using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileMoveArguments
    {
        [NotNull]
        public string SourceFileName { get; }

        [NotNull]
        public string DestinationFileName { get; }

        public FileMoveArguments([NotNull] string sourceFileName, [NotNull] string destinationFileName)
        {
            Guard.NotNull(sourceFileName, nameof(sourceFileName));
            Guard.NotNull(destinationFileName, nameof(destinationFileName));

            SourceFileName = sourceFileName;
            DestinationFileName = destinationFileName;
        }
    }
}
