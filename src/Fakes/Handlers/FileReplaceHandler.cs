using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileReplaceHandler : FakeOperationHandler<FileReplaceArguments, Missing>
    {
        public FileReplaceHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override Missing Handle(FileReplaceArguments arguments)
        {
            FileEntry sourceFile = ResolveExistingFile(arguments.SourcePath);

            AssertNoDuplicatePaths(arguments);
            AssertAllOnSameDrive(arguments);

            FileEntry destinationFile = ResolveExistingFile(arguments.DestinationPath);

            DirectoryEntry beforeDestinationDirectory = destinationFile.Parent;
            string beforeDestinationFileName = destinationFile.Name;

            MoveDestinationFileToBackupFile(destinationFile, arguments.BackupDestinationPath);
            MoveSourceFileToDestinationFile(sourceFile, beforeDestinationDirectory, beforeDestinationFileName);

            return Missing.Value;
        }

        private void MoveDestinationFileToBackupFile([NotNull] FileEntry destinationFile, [CanBeNull] AbsolutePath backupPath)
        {
            DirectoryEntry beforeDestinationDirectory = destinationFile.Parent;
            string beforeDestinationFileName = destinationFile.Name;

            if (backupPath != null)
            {
                DirectoryEntry backupDirectory = ResolveBackupDirectory(backupPath);
                string backupFileName = backupPath.Components.Last();

                if (backupDirectory.ContainsFile(backupFileName))
                {
                    backupDirectory.DeleteFile(backupFileName);
                }

                backupDirectory.MoveFileToHere(destinationFile, backupFileName);
            }

            beforeDestinationDirectory.DeleteFile(beforeDestinationFileName);
        }

        private void MoveSourceFileToDestinationFile([NotNull] FileEntry sourceFile,
            [NotNull] DirectoryEntry destinationDirectory, [NotNull] string destinationFileName)
        {
            sourceFile.Parent.DeleteFile(sourceFile.Name);
            destinationDirectory.MoveFileToHere(sourceFile, destinationFileName);
        }

        [NotNull]
        private FileEntry ResolveExistingFile([NotNull] AbsolutePath sourcePath)
        {
            var resolver = new FileResolver(Root)
            {
                ErrorFileNotFound = _ => ErrorFactory.System.UnableToFindSpecifiedFile(),
                ErrorFileFoundAsDirectory = _ => ErrorFactory.System.UnauthorizedAccess(),
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNotFound(),
                ErrorDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNotFound()
            };
            return resolver.ResolveExistingFile(sourcePath);
        }

        [NotNull]
        private DirectoryEntry ResolveBackupDirectory([NotNull] AbsolutePath backupPath)
        {
            AbsolutePath parentDirectory = backupPath.TryGetParentPath();
            if (parentDirectory == null)
            {
                throw new Exception("TODO: Handle missing parent.");
            }

            var backupResolver = new DirectoryResolver(Root)
            {
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.System.UnableToRemoveFileToBeReplaced(),
                ErrorDirectoryFoundAsFile = _ => ErrorFactory.System.UnableToRemoveFileToBeReplaced()
            };
            DirectoryEntry backupDirectory = backupResolver.ResolveDirectory(parentDirectory);

            string backupFileName = backupPath.Components.Last();
            if (backupDirectory.ContainsDirectory(backupFileName))
            {
                throw ErrorFactory.System.UnableToRemoveFileToBeReplaced();
            }

            return backupDirectory;
        }

        private static void AssertNoDuplicatePaths([NotNull] FileReplaceArguments arguments)
        {
            if (AbsolutePath.AreEquivalent(arguments.SourcePath, arguments.DestinationPath))
            {
                throw ErrorFactory.System.FileIsInUse();
            }

            if (AbsolutePath.AreEquivalent(arguments.SourcePath, arguments.BackupDestinationPath))
            {
                throw ErrorFactory.System.UnableToRemoveFileToBeReplaced();
            }

            if (AbsolutePath.AreEquivalent(arguments.DestinationPath, arguments.BackupDestinationPath))
            {
                throw ErrorFactory.System.UnableToMoveReplacementFile();
            }
        }

        private void AssertAllOnSameDrive([NotNull] FileReplaceArguments arguments)
        {
            if (arguments.BackupDestinationPath != null &&
                !arguments.DestinationPath.IsOnSameVolume(arguments.BackupDestinationPath))
            {
                throw ErrorFactory.System.UnableToRemoveFileToBeReplaced();
            }

            if (!arguments.SourcePath.IsOnSameVolume(arguments.DestinationPath))
            {
                throw ErrorFactory.System.UnableToMoveReplacementFile();
            }
        }
    }
}
