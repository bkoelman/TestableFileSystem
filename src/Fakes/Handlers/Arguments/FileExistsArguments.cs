using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileExistsArguments
    {
        [CanBeNull]
        public string Path { get; }

        public FileExistsArguments([CanBeNull] string path)
        {
            Path = path;
        }
    }
}
