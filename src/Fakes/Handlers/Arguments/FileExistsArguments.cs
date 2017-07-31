using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileExistsArguments
    {
        [CanBeNull]
        public AbsolutePath Path { get; }

        public FileExistsArguments([CanBeNull] AbsolutePath path)
        {
            Path = path;
        }
    }
}
