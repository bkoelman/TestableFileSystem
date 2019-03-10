using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileReplaceHandler : FakeOperationHandler<FileReplaceArguments, Missing>
    {
        public FileReplaceHandler([NotNull] VolumeContainer container)
            : base(container)
        {
        }

        public override Missing Handle(FileReplaceArguments arguments)
        {
            FileEntry sourceFile = ResolveExistingFile(arguments.SourcePath);

            AssertNoDuplicatePaths(arguments);
            AssertAllOnSameDrive(arguments);

            FileEntry destinationFile = ResolveExistingFile(arguments.DestinationPath);

            var spaceTracker = new VolumeSpaceTracker(sourceFile, destinationFile);

            if (arguments.BackupDestinationPath != null)
            {
                TransferDestinationFileToBackupFile(destinationFile, arguments.BackupDestinationPath, spaceTracker);
            }

            TransferSourceContentsToDestinationFile(sourceFile, destinationFile);

            spaceTracker.ApplyVolumeSpaceChange();

            return Missing.Value;
        }

        private void TransferDestinationFileToBackupFile([NotNull] FileEntry destinationFile, [NotNull] AbsolutePath backupPath,
            [NotNull] VolumeSpaceTracker spaceTracker)
        {
            DirectoryEntry backupDirectory = ResolveBackupDirectory(backupPath);
            string backupFileName = backupPath.Components.Last();

            FileEntry backupFile;
            long previousBackupFileSize;

            if (backupDirectory.ContainsFile(backupFileName))
            {
                backupFile = backupDirectory.GetFile(backupFileName);
                previousBackupFileSize = backupFile.Size;

                AssertBackupFileIsNotReadOnly(backupFile);
                AssertHasExclusiveAccessToBackupFile(backupFile);
            }
            else
            {
                backupFile = backupDirectory.CreateFile(backupFileName);
                previousBackupFileSize = 0;
            }

            backupFile.TransferFrom(destinationFile);

            spaceTracker.SetBackupFileSizeChange(backupFile.Size - previousBackupFileSize);
        }

        private static void TransferSourceContentsToDestinationFile([NotNull] FileEntry sourceFile,
            [NotNull] FileEntry destinationFile)
        {
            destinationFile.TransferContentsFrom(sourceFile);
            sourceFile.Parent.DeleteFile(sourceFile.Name);
        }

        [NotNull]
        private FileEntry ResolveExistingFile([NotNull] AbsolutePath sourcePath)
        {
            var resolver = new FileResolver(Container)
            {
                ErrorFileNotFound = _ => ErrorFactory.System.UnableToFindSpecifiedFile(),
                ErrorFileFoundAsDirectory = _ => ErrorFactory.System.UnauthorizedAccess(),
                ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNotFound(),
                ErrorDirectoryFoundAsFile = _ => ErrorFactory.System.DirectoryNotFound(),
                ErrorPathIsVolumeRoot = _ => ErrorFactory.System.UnauthorizedAccess()
            };
            FileEntry file = resolver.ResolveExistingFile(sourcePath);

            AssertFileIsNotReadOnly(file);
            AssertHasExclusiveAccess(file);
            AssertIsNotExternallyEncrypted(file);

            return file;
        }

        [AssertionMethod]
        private static void AssertFileIsNotReadOnly([NotNull] FileEntry fileEntry)
        {
            if (fileEntry.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.System.UnauthorizedAccess();
            }
        }

        private static void AssertHasExclusiveAccess([NotNull] FileEntry file)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.System.FileIsInUse();
            }
        }

        [AssertionMethod]
        private static void AssertIsNotExternallyEncrypted([NotNull] FileEntry file)
        {
            if (file.IsExternallyEncrypted)
            {
                throw ErrorFactory.System.UnauthorizedAccess();
            }
        }

        [NotNull]
        private DirectoryEntry ResolveBackupDirectory([NotNull] AbsolutePath backupPath)
        {
            AbsolutePath parentDirectory = backupPath.TryGetParentPath();
            if (parentDirectory == null)
            {
                throw ErrorFactory.System.UnableToRemoveFileToBeReplaced();
            }

            var backupResolver = new DirectoryResolver(Container)
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

        [AssertionMethod]
        private static void AssertBackupFileIsNotReadOnly([NotNull] FileEntry backupFile)
        {
            if (backupFile.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.System.UnableToRemoveFileToBeReplaced();
            }
        }

        private static void AssertHasExclusiveAccessToBackupFile([NotNull] FileEntry backupFile)
        {
            if (backupFile.IsOpen())
            {
                throw ErrorFactory.System.UnableToRemoveFileToBeReplaced();
            }
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

        private static void AssertAllOnSameDrive([NotNull] FileReplaceArguments arguments)
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

        private sealed class VolumeSpaceTracker
        {
            [NotNull]
            private readonly VolumeEntry volume;

            private readonly long sourceToDestinationFileSizeDelta;
            private long destinationToBackupFileSizeDelta;

            public VolumeSpaceTracker([NotNull] FileEntry sourceFile, [NotNull] FileEntry destinationFile)
            {
                Guard.NotNull(sourceFile, nameof(sourceFile));
                Guard.NotNull(destinationFile, nameof(destinationFile));

                volume = sourceFile.Parent.Root;
                sourceToDestinationFileSizeDelta = sourceFile.Size - destinationFile.Size;
            }

            public void SetBackupFileSizeChange(long bytes)
            {
                destinationToBackupFileSizeDelta = bytes;
            }

            public void ApplyVolumeSpaceChange()
            {
                if (destinationToBackupFileSizeDelta + sourceToDestinationFileSizeDelta != 0)
                {
                    if (!volume.TryAllocateSpace(destinationToBackupFileSizeDelta + sourceToDestinationFileSizeDelta))
                    {
                        throw ErrorFactory.Internal.UnknownError(
                            $"Disk space on volume '{volume.Name}' ({volume.FreeSpaceInBytes} bytes) would become negative.");
                    }
                }
            }
        }
    }
}
