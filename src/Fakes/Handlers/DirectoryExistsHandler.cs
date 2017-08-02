using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryExistsHandler : FakeOperationHandler<DirectoryExistsArguments, bool>
    {
        public DirectoryExistsHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override bool Handle(DirectoryExistsArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            if (arguments.Path == null)
            {
                return false;
            }

            var resolver = new DirectoryResolver(Root);
            DirectoryEntry existingDirectoryOrNull = resolver.TryResolveDirectory(arguments.Path);

            return existingDirectoryOrNull != null;
        }
    }
}
