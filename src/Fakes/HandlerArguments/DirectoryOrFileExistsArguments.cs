using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class DirectoryOrFileExistsArguments
    {
        [CanBeNull]
        public AbsolutePath Path { get; }

        public DirectoryOrFileExistsArguments([CanBeNull] AbsolutePath path)
        {
            Path = path;
        }
    }
}
