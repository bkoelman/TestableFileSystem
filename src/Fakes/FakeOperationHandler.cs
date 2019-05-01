using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal abstract class FakeOperationHandler<TArguments, TResult>
    {
        [NotNull]
        protected VolumeContainer Container { get; }

        protected FakeOperationHandler([NotNull] VolumeContainer container)
        {
            Guard.NotNull(container, nameof(container));
            Container = container;
        }

        [NotNull]
        public abstract TResult Handle([NotNull] TArguments arguments);
    }
}
