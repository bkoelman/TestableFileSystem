using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileExistsHandler : FakeOperationHandler<EntryExistsArguments, bool>
    {
        public FileExistsHandler([NotNull] DirectoryEntry root, [NotNull] FileSystemChangeTracker changeTracker)
            : base(root, changeTracker)
        {
        }

        public override bool Handle(EntryExistsArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            if (arguments.Path == null)
            {
                return false;
            }

            var resolver = new FileResolver(Root);
            FileResolveResult resolveResult = resolver.TryResolveFile(arguments.Path);

            return resolveResult.ExistingFileOrNull != null;
        }
    }
}
