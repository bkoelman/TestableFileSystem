using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileGetTimeArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public FileTimeKind Kind { get; }
        public bool IsInUtc { get; }

        public FileGetTimeArguments([NotNull] AbsolutePath path, FileTimeKind kind, bool isInUtc)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            Kind = kind;
            IsInUtc = isInUtc;
        }
    }
}
