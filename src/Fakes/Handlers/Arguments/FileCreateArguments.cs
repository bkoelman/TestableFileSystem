using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileCreateArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public FileOptions Options { get; }

        public FileCreateArguments([NotNull] AbsolutePath path, FileOptions options)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            Options = options;
        }
    }
}
