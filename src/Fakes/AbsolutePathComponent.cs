using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class AbsolutePathComponent
    {
        [NotNull]
        private readonly AbsolutePath path;

        private readonly int offset;

        public bool IsAtStart => offset == 0;

        public bool IsAtEnd => offset == path.Components.Count - 1;

        [NotNull]
        public string Name => path.Components[offset];

        public AbsolutePathComponent([NotNull] AbsolutePath path, int offset)
        {
            Guard.NotNull(path, nameof(path));
            Guard.InRangeInclusive(offset, nameof(offset), 0, path.Components.Count - 1);

            this.path = path;
            this.offset = offset;
        }
    }
}