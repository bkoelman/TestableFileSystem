using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryExistsHandler : FakeOperationHandler<EntryExistsArguments, bool>
    {
        public DirectoryExistsHandler([NotNull] VolumeContainer container)
            : base(container)
        {
        }

        public override bool Handle(EntryExistsArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            if (arguments.Path == null)
            {
                return false;
            }

            var resolver = new DirectoryResolver(Container);
            DirectoryEntry existingDirectoryOrNull = resolver.TryResolveDirectory(arguments.Path);

            return existingDirectoryOrNull != null;
        }
    }
}
