using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileCreateArguments
    {
        [NotNull]
        public string Path { get; }

        public FileOptions Options { get; }

        public FileCreateArguments([NotNull] string path, FileOptions options)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            Options = options;
        }
    }
}