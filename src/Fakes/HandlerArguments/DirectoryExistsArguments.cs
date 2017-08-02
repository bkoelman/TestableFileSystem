using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.HandlerArguments
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
