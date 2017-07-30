using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileCopyArguments
    {
        [NotNull]
        public string SourceFileName { get; }

        [NotNull]
        public string DestinationFileName { get; }

        public bool Overwrite { get; }

        public FileCopyArguments([NotNull] string sourceFileName, [NotNull] string destinationFileName, bool overwrite)
        {
            Guard.NotNull(sourceFileName, nameof(sourceFileName));
            Guard.NotNull(destinationFileName, nameof(destinationFileName));

            SourceFileName = sourceFileName;
            DestinationFileName = destinationFileName;
            Overwrite = overwrite;
        }
    }
}
