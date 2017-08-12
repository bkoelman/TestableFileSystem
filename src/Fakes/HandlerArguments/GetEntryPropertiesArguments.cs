using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class GetEntryPropertiesArguments
    {
        [NotNull]
        public AbsolutePath Path { get; }

        public GetEntryPropertiesArguments([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
        }
    }
}
