using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileDeleteHandler : FakeOperationHandler<FileDeleteArguments, object>
    {
        public FileDeleteHandler([NotNull] FakeFileSystem fileSystem, [NotNull] DirectoryEntry root)
            : base(fileSystem, root)
        {
        }

        public override object Handle(FileDeleteArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            AbsolutePath absolutePath = FileSystem.ToAbsolutePath(arguments.Path);

            var resolver = new FileResolver(Root);
            (DirectoryEntry containingDirectory, FileEntry existingFileOrNull, string _) = resolver.TryResolveFile(absolutePath);

            if (existingFileOrNull != null)
            {
                AssertIsNotReadOnly(existingFileOrNull, absolutePath);

                // Block deletion when file is in use.
                using (existingFileOrNull.Open(FileMode.Open, FileAccess.ReadWrite))
                {
                    containingDirectory.DeleteFile(existingFileOrNull);
                }
            }

            return new object();
        }

        [AssertionMethod]
        private void AssertIsNotReadOnly([NotNull] FileEntry fileEntry, [NotNull] AbsolutePath absolutePath)
        {
            if (fileEntry.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.UnauthorizedAccess(absolutePath.GetText());
            }
        }
    }
}
