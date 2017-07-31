using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class DirectoryExistsArguments
    {
        [CanBeNull]
        public string Path { get; }

        public DirectoryExistsArguments([CanBeNull] string path)
        {
            Path = path;
        }
    }
}
