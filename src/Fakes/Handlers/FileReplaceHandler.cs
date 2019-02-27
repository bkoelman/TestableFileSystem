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
            FileEntry sourceFile = ResolveSourceFile(arguments.SourcePath);
            FileEntry destinationFile = ResolveDestinationFile(arguments.DestinationPath);

            DirectoryEntry beforeDestinationDirectory = destinationFile.Parent;
            string beforeDestinationFileName = destinationFile.Name;

            MoveDestinationFileToBackupFile(destinationFile, arguments.BackupDestinationPath);
            MoveSourceFileToDestinationFile(sourceFile, beforeDestinationDirectory, beforeDestinationFileName);

            return Missing.Value;
        }

        private void MoveDestinationFileToBackupFile([NotNull] FileEntry destinationFile, [CanBeNull] AbsolutePath backupPath)
        {
            destinationFile.Parent.DeleteFile(destinationFile.Name);

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
        }

        private void MoveSourceFileToDestinationFile([NotNull] FileEntry sourceFile,
            [NotNull] DirectoryEntry destinationDirectory, [NotNull] string destinationFileName)
        {
            sourceFile.Parent.DeleteFile(sourceFile.Name);
            destinationDirectory.MoveFileToHere(sourceFile, destinationFileName);
        }

        [NotNull]
        private FileEntry ResolveSourceFile([NotNull] AbsolutePath sourcePath)
        {
            var sourceResolver = new FileResolver(Root);
            return sourceResolver.ResolveExistingFile(sourcePath);
        }

        [NotNull]
        private FileEntry ResolveDestinationFile([NotNull] AbsolutePath destinationPath)
        {
            var destinationResolver = new FileResolver(Root);
            return destinationResolver.ResolveExistingFile(destinationPath);
        }

        [NotNull]
        private DirectoryEntry ResolveBackupDirectory([NotNull] AbsolutePath backupPath)
        {
            AbsolutePath parentDirectory = backupPath.TryGetParentPath();
            if (parentDirectory == null)
            {
                throw new Exception("TODO: Handle missing parent.");
            }

            var destinationResolver = new DirectoryResolver(Root);
            return destinationResolver.ResolveDirectory(parentDirectory);
        }
    }
}
