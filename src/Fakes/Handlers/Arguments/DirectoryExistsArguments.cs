using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class DirectoryExistsArguments
    {
        [CanBeNull]
        public AbsolutePath Path { get; }

        public DirectoryExistsArguments([CanBeNull] AbsolutePath path)
        {
            Path = path;
        }
    }
}
