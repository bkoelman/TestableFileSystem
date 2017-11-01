using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class EntryExistsArguments
    {
        [CanBeNull]
        public AbsolutePath Path { get; }

        public EntryExistsArguments([CanBeNull] AbsolutePath path)
        {
            Path = path;
        }
    }
}
