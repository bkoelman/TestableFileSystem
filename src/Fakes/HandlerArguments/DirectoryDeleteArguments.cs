using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class DirectoryDeleteArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public bool IsRecursive { get; }

        public DirectoryDeleteArguments([NotNull] AbsolutePath path, bool isRecursive)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            IsRecursive = isRecursive;
        }
    }
}
