using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class EntryGetPropertiesArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public EntryGetPropertiesArguments([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
        }
    }
}
