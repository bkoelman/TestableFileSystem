using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class AbsolutePathComponent
    {
        private readonly int offset;

        [NotNull]
        public AbsolutePath Path { get; }

        [NotNull]
        public string Name => Path.Components[offset];

        public bool IsAtStart => offset == 0;

        public bool IsAtEnd => offset == Path.Components.Count - 1;

        public AbsolutePathComponent([NotNull] AbsolutePath path, int offset)
        {
            Guard.NotNull(path, nameof(path));
            Guard.InRangeInclusive(offset, nameof(offset), 0, path.Components.Count - 1);

            Path = path;
            this.offset = offset;
        }

        [NotNull]
        public AbsolutePath GetPathUpToHere()
        {
            return Path.GetAncestorPath(offset);
        }
    }
}
