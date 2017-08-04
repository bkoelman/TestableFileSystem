using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileExistsHandler : FakeOperationHandler<DirectoryOrFileExistsArguments, bool>
    {
        public FileExistsHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override bool Handle(DirectoryOrFileExistsArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            if (arguments.Path == null)
            {
                return false;
            }

            var resolver = new FileResolver(Root);
            (DirectoryEntry _, FileEntry existingFileOrNull, string _) = resolver.TryResolveFile(arguments.Path);

            return existingFileOrNull != null;
        }
    }
}
