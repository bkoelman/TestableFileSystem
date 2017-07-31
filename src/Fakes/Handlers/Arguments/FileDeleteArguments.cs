using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileDeleteArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public FileDeleteArguments([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));
            Path = path;
        }
    }
}
