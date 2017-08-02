using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileCreateHandler : FakeOperationHandler<FileCreateArguments, IFileStream>
    {
        public FileCreateHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override IFileStream Handle(FileCreateArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));
            AssertValidCreationOptions(arguments);

            var resolver = new FileResolver(Root);
            (DirectoryEntry containingDirectory, FileEntry _, string fileName) = resolver.TryResolveFile(arguments.Path);

            FileEntry newFile = containingDirectory.GetOrCreateFile(fileName);

            if ((arguments.Options & FileOptions.DeleteOnClose) != 0)
            {
                newFile.EnableDeleteOnClose();
            }

            return newFile.Open(FileMode.Create, FileAccess.ReadWrite);
        }

        [AssertionMethod]
        private static void AssertValidCreationOptions([NotNull] FileCreateArguments arguments)
        {
            if (arguments.Options.HasFlag(FileOptions.Encrypted))
            {
                throw ErrorFactory.UnauthorizedAccess(arguments.Path.GetText());
            }
        }
    }
}
