using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class PathGetTempFileNameArguments
    {
        [NotNull]
        public AbsolutePath TempDirectory { get; }

        public PathGetTempFileNameArguments([NotNull] AbsolutePath tempDirectory)
        {
            Guard.NotNull(tempDirectory, nameof(tempDirectory));
            TempDirectory = tempDirectory;
        }
    }
}
