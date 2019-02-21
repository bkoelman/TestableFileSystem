using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.HandlerArguments
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
