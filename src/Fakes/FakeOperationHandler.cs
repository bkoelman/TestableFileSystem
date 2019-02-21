using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal abstract class FakeOperationHandler<TArguments, TResult>
    {
        [NotNull]
        protected DirectoryEntry Root { get; }

        protected FakeOperationHandler([NotNull] DirectoryEntry root)
        {
            Guard.NotNull(root, nameof(root));
            Root = root;
        }

        [NotNull]
        public abstract TResult Handle([NotNull] TArguments arguments);
    }
}
