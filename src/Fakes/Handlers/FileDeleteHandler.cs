using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileDeleteHandler : FakeOperationHandler<FileDeleteArguments, Missing>
    {
        public FileDeleteHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override Missing Handle(FileDeleteArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            var resolver = new FileResolver(Root);
            FileResolveResult resolveResult = resolver.TryResolveFile(arguments.Path);

            if (resolveResult.ExistingFileOrNull != null)
            {
                DeleteFile(resolveResult.ExistingFileOrNull, resolveResult.ContainingDirectory, arguments);
            }

            return Missing.Value;
        }

        private static void DeleteFile([NotNull] FileEntry existingFile, [NotNull] DirectoryEntry containingDirectory,
            [NotNull] FileDeleteArguments arguments)
        {
            AssertIsNotReadOnly(existingFile, arguments.Path);
            AssertHasExclusiveAccess(existingFile, arguments.Path);

            containingDirectory.DeleteFile(existingFile.Name);
        }

        [AssertionMethod]
        private static void AssertIsNotReadOnly([NotNull] FileEntry fileEntry, [NotNull] AbsolutePath absolutePath)
        {
            if (fileEntry.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.System.UnauthorizedAccess(absolutePath.GetText());
            }
        }

        private static void AssertHasExclusiveAccess([NotNull] FileEntry file, [NotNull] AbsolutePath absolutePath)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.System.FileIsInUse(absolutePath.GetText());
            }
        }
    }
}
