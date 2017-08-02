using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class DirectoryDeleteArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public bool Recursive { get; }

        public DirectoryDeleteArguments([NotNull] AbsolutePath path, bool recursive)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            Recursive = recursive;
        }
    }
}
