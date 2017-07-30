using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal abstract class FakeOperationHandler<TArguments, TResult>
    {
        [NotNull]
        protected FakeFileSystem FileSystem { get; }

        [NotNull]
        protected DirectoryEntry Root { get; }

        protected FakeOperationHandler([NotNull] FakeFileSystem fileSystem, [NotNull] DirectoryEntry root)
        {
            Guard.NotNull(fileSystem, nameof(fileSystem));
            Guard.NotNull(fileSystem, nameof(fileSystem));

            FileSystem = fileSystem;
            Root = root;
        }

        [NotNull]
        public abstract TResult Handle([NotNull] TArguments arguments);
    }
}
