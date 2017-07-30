using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileDeleteArguments
    {
        [NotNull]
        public string Path { get; }

        public FileDeleteArguments([NotNull] string path)
        {
            Guard.NotNull(path, nameof(path));
            Path = path;
        }
    }
}
