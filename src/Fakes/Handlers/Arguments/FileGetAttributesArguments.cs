using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileGetAttributesArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public FileGetAttributesArguments([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
        }
    }
}
