using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileDeleteHandler : FakeOperationHandler<FileDeleteArguments, object>
    {
        public FileDeleteHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override object Handle(FileDeleteArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            var resolver = new FileResolver(Root);
            (DirectoryEntry containingDirectory, FileEntry existingFileOrNull, string _) = resolver.TryResolveFile(arguments.Path);

            if (existingFileOrNull != null)
            {
                AssertIsNotReadOnly(existingFileOrNull, arguments.Path);
                AssertHasExclusiveAccess(existingFileOrNull, arguments.Path);

                containingDirectory.DeleteFile(existingFileOrNull);
            }

            return Missing.Value;
        }

        [AssertionMethod]
        private void AssertIsNotReadOnly([NotNull] FileEntry fileEntry, [NotNull] AbsolutePath absolutePath)
        {
            if (fileEntry.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.UnauthorizedAccess(absolutePath.GetText());
            }
        }

        private static void AssertHasExclusiveAccess([NotNull] FileEntry file, [NotNull] AbsolutePath absolutePath)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.FileIsInUse(absolutePath.GetText());
            }
        }
    }
}
