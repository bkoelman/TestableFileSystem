using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileOpenHandler : FakeOperationHandler<FileOpenArguments, IFileStream>
    {
        public FileOpenHandler([NotNull] DirectoryEntry root, [NotNull] FakeFileSystemChangeTracker changeTracker)
            : base(root, changeTracker)
        {
        }

        public override IFileStream Handle(FileOpenArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            FileAccess fileAccess = DetectFileAccess(arguments);
            AssertValidCombinationOfModeWithAccess(arguments.Mode, fileAccess);

            var resolver = new FileResolver(Root);
            FileResolveResult resolveResult = resolver.TryResolveFile(arguments.Path);

            return resolveResult.ExistingFileOrNull != null
                ? HandleExistingFile(resolveResult.ExistingFileOrNull, fileAccess, arguments)
                : HandleNewFile(resolveResult.FileName, resolveResult.ContainingDirectory, fileAccess, arguments);
        }

        private static FileAccess DetectFileAccess([NotNull] FileOpenArguments arguments)
        {
            return arguments.Access ?? (arguments.Mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite);
        }

        [AssertionMethod]
        private void AssertValidCombinationOfModeWithAccess(FileMode mode, FileAccess access)
        {
            if (access == FileAccess.Read)
            {
                switch (mode)
                {
                    case FileMode.CreateNew:
                    case FileMode.Create:
                    case FileMode.Truncate:
                    case FileMode.Append:
                        throw ErrorFactory.System.InvalidOpenCombination(mode, access);
                }
            }
        }

        [NotNull]
        private IFileStream HandleExistingFile([NotNull] FileEntry file, FileAccess fileAccess,
            [NotNull] FileOpenArguments arguments)
        {
            if (arguments.Mode == FileMode.CreateNew)
            {
                throw ErrorFactory.System.FileAlreadyExists(arguments.Path.GetText());
            }

            if (fileAccess != FileAccess.Read)
            {
                AssertIsNotReadOnly(file, arguments.Path);

                if (arguments.Mode == FileMode.Create)
                {
                    AssertIsNotHidden(file, arguments.Path);
                }
            }

            return file.Open(arguments.Mode, fileAccess, arguments.Path);
        }

        [AssertionMethod]
        private void AssertIsNotHidden([NotNull] FileEntry fileEntry, [NotNull] AbsolutePath absolutePath)
        {
            if (fileEntry.Attributes.HasFlag(FileAttributes.Hidden))
            {
                throw ErrorFactory.System.UnauthorizedAccess(absolutePath.GetText());
            }
        }

        [AssertionMethod]
        private void AssertIsNotReadOnly([NotNull] FileEntry fileEntry, [NotNull] AbsolutePath absolutePath)
        {
            if (fileEntry.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.System.UnauthorizedAccess(absolutePath.GetText());
            }
        }

        [NotNull]
        private static IFileStream HandleNewFile([NotNull] string fileName, [NotNull] DirectoryEntry containingDirectory,
            FileAccess fileAccess, [NotNull] FileOpenArguments arguments)
        {
            if (arguments.Mode == FileMode.Open || arguments.Mode == FileMode.Truncate)
            {
                throw ErrorFactory.System.FileNotFound(arguments.Path.GetText());
            }

            FileEntry file = containingDirectory.CreateFile(fileName);
            return file.Open(arguments.Mode, fileAccess, arguments.Path);
        }
    }
}
