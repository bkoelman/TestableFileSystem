using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class DirectoryCreateArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public DirectoryCreateArguments([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
        }
    }
}
