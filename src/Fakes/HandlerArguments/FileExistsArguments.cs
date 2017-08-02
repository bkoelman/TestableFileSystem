using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.HandlerArguments
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
