using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileMoveHandler : FakeOperationHandler<EntryMoveArguments, Missing>
    {
        public bool IsCopyRequired { get; private set; }
        public bool IsSourceReadOnly { get; private set; }

        public FileMoveHandler([NotNull] VolumeContainer container)
            : base(container)
        {
        }

        public override Missing Handle(EntryMoveArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            FileEntry sourceFile = ResolveSourceFile(arguments.SourcePath);
            AssertHasExclusiveAccess(sourceFile);

            DirectoryEntry destinationDirectory =
                ResolveDestinationDirectory(arguments.SourcePath, arguments.DestinationPath, sourceFile);

            IsCopyRequired = !arguments.SourcePath.IsOnSameVolume(arguments.DestinationPath);
            IsSourceReadOnly = sourceFile.Attributes.HasFlag(FileAttributes.ReadOnly);

            if (!IsCopyRequired)
            {
                string destinationFileName = arguments.DestinationPath.Components.Last();
                MoveFile(sourceFile, destinationDirectory, destinationFileName, arguments.SourcePath.Formatter);
            }

            return Missing.Value;
        }

        [NotNull]
        private FileEntry ResolveSourceFile([NotNull] AbsolutePath sourcePath)
        {
            var sourceResolver = new FileResolver(Container)
            {
                ErrorFileFoundAsDirectory = ErrorFactory.System.FileNotFound,
                ErrorDirectoryFoundAsFile = ErrorFactory.System.FileNotFound,
                ErrorLastDirectoryFoundAsFile = ErrorFactory.System.FileNotFound,
                ErrorDirectoryNotFound = ErrorFactory.System.FileNotFound,
                ErrorPathIsVolumeRoot = ErrorFactory.System.FileNotFound,
                ErrorNetworkShareNotFound = ErrorFactory.System.FileNotFound
            };

            return sourceResolver.ResolveExistingFile(sourcePath);
        }

        [NotNull]
        private DirectoryEntry ResolveDestinationDirectory([NotNull] AbsolutePath sourcePath,
            [NotNull] AbsolutePath destinationPath, [NotNull] FileEntry sourceFile)
        {
            if (IsFileCasingChangeOnly(sourcePath, destinationPath))
            {
                return sourceFile.Parent;
            }

            var destinationResolver = new FileResolver(Container)
            {
                ErrorFileFoundAsDirectory = _ => ErrorFactory.System.CannotCreateFileBecauseFileAlreadyExists(),
                ErrorDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNotFound(),
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.System.ParameterIsIncorrect(),
                ErrorDirectoryNotFound = _ => ErrorFactory.System.DirectoryNotFound(),
                ErrorPathIsVolumeRoot = _ => ErrorFactory.System.FileOrDirectoryOrVolumeIsIncorrect(),
                ErrorFileExists = _ => ErrorFactory.System.CannotCreateFileBecauseFileAlreadyExists()
            };

            return destinationResolver.ResolveContainingDirectoryForMissingFile(destinationPath);
        }

        private static bool IsFileCasingChangeOnly([NotNull] AbsolutePath sourcePath, [NotNull] AbsolutePath destinationPath)
        {
            return sourcePath.Components.SequenceEqual(destinationPath.Components, StringComparer.OrdinalIgnoreCase);
        }

        private static void AssertHasExclusiveAccess([NotNull] FileEntry file)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.System.FileIsInUse();
            }
        }

        private void MoveFile([NotNull] FileEntry sourceFile, [NotNull] DirectoryEntry destinationDirectory,
            [NotNull] string destinationFileName, [NotNull] IPathFormatter sourcePathFormatter)
        {
            if (sourceFile.Parent == destinationDirectory)
            {
                if (sourceFile.Name != destinationFileName)
                {
                    sourceFile.Parent.RenameFile(sourceFile.Name, destinationFileName, sourcePathFormatter);
                }
            }
            else
            {
                sourceFile.Parent.DeleteFile(sourceFile.Name);
                destinationDirectory.MoveFileToHere(sourceFile, destinationFileName);
            }
        }
    }
}
