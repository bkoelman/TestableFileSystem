using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileCreateHandler : FakeOperationHandler<FileCreateArguments, IFileStream>
    {
        public FileCreateHandler([NotNull] FakeFileSystem fileSystem, [NotNull] DirectoryEntry root)
            : base(fileSystem, root)
        {
        }

        public override IFileStream Handle(FileCreateArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            AbsolutePath absolutePath = FileSystem.ToAbsolutePath(arguments.Path);
            AssertValidCreationOptions(arguments.Options, absolutePath);

            var resolver = new FileResolver(Root);
            (DirectoryEntry containingDirectory, FileEntry _, string fileName) = resolver.TryResolveFile(absolutePath);

            FileEntry newFile = containingDirectory.GetOrCreateFile(fileName);

            if ((arguments.Options & FileOptions.DeleteOnClose) != 0)
            {
                newFile.EnableDeleteOnClose();
            }

            return newFile.Open(FileMode.Create, FileAccess.ReadWrite);
        }

        [AssertionMethod]
        private static void AssertValidCreationOptions(FileOptions options, [NotNull] AbsolutePath absolutePath)
        {
            if (options.HasFlag(FileOptions.Encrypted))
            {
                throw ErrorFactory.UnauthorizedAccess(absolutePath.GetText());
            }
        }
    }
}
