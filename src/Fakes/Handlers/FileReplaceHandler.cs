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

            Container.ChangeTracker.NotifyContentsAccessed(sourceFile.PathFormatter, FileAccessKinds.Create);
            NotifyDirectoriesChanged(destinationFile.Parent, sourceFile.Parent);

            BackupFileInfo backupFileInfo = BackupFileInfo.Create(arguments.BackupDestinationPath, this);
            if (backupFileInfo != null && !backupFileInfo.Exists)
            {
                string destinationFileName = destinationFile.Name;
                DirectoryEntry destinationDirectory = destinationFile.Parent;

                MoveSingleFile(destinationFile, backupFileInfo.Directory, backupFileInfo.FileName, destinationFile.PathFormatter, false);

                if (destinationDirectory != backupFileInfo.Directory)
                {
                    Container.ChangeTracker.NotifyContentsAccessed(destinationDirectory.PathFormatter, FileAccessKinds.Write);
                }

                Container.ChangeTracker.NotifyContentsAccessed(sourceFile.PathFormatter, FileAccessKinds.Security);

                MoveSingleFile(sourceFile, destinationDirectory, destinationFileName, sourceFile.PathFormatter, true);
            }
            else
            {
                var spaceTracker = new VolumeSpaceTracker(sourceFile, destinationFile);

                if (backupFileInfo != null)
                {
                    FileEntry backupFile = backupFileInfo.GetExistingFile();
                    TransferDestinationContentsToExistingBackupFile(destinationFile, backupFile, spaceTracker);
                }

                TransferSourceContentsToDestinationFile(sourceFile, destinationFile);

                spaceTracker.ApplyVolumeSpaceChange();
            }

            return Missing.Value;
        }

        private void NotifyDirectoriesChanged([NotNull] DirectoryEntry directory1, [NotNull] DirectoryEntry directory2)
        {
            if (directory1 == directory2)
            {
                Container.ChangeTracker.NotifyContentsAccessed(directory1.PathFormatter, FileAccessKinds.WriteRead);
            }
            else
            {
                Container.ChangeTracker.NotifyContentsAccessed(directory1.PathFormatter, FileAccessKinds.WriteRead);
                Container.ChangeTracker.NotifyContentsAccessed(directory2.PathFormatter, FileAccessKinds.WriteRead);
            }
        }

        private void TransferDestinationContentsToExistingBackupFile([NotNull] FileEntry destinationFile,
            [NotNull] FileEntry backupFile, [NotNull] VolumeSpaceTracker spaceTracker)
        {
            AssertBackupFileIsNotReadOnly(backupFile);
            AssertHasExclusiveAccessToBackupFile(backupFile);

            long previousBackupFileSize = backupFile.Size;

            backupFile.TransferFrom(destinationFile);

            spaceTracker.SetBackupFileSizeChange(backupFile.Size - previousBackupFileSize);
        }

        private static void TransferSourceContentsToDestinationFile([NotNull] FileEntry sourceFile,
            [NotNull] FileEntry destinationFile)
        {
            destinationFile.TransferContentsFrom(sourceFile);
            sourceFile.Parent.DeleteFile(sourceFile.Name, false);
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

        private void MoveSingleFile([NotNull] FileEntry sourceFile, [NotNull] DirectoryEntry destinationDirectory,
            [NotNull] string destinationFileName, [NotNull] IPathFormatter sourcePathFormatter, bool skipNotifyLastAccess)
        {
            if (sourceFile.Parent == destinationDirectory)
            {
                IPathFormatter sourcePathFormatterNonLazy = sourcePathFormatter.GetPath().Formatter;
                sourceFile.Parent.RenameFile(sourceFile.Name, destinationFileName, sourcePathFormatterNonLazy, skipNotifyLastAccess);
            }
            else
            {
                sourceFile.Parent.DeleteFile(sourceFile.Name, true);
                destinationDirectory.MoveFileToHere(sourceFile, destinationFileName);
            }
        }

        private sealed class BackupFileInfo
        {
            [NotNull]
            public string FileName { get; }

            [NotNull]
            public DirectoryEntry Directory { get; }

            public bool Exists => Directory.ContainsFile(FileName);

            private BackupFileInfo([NotNull] AbsolutePath backupPath, [NotNull] FileReplaceHandler owner)
            {
                Guard.NotNull(backupPath, nameof(backupPath));
                Guard.NotNull(owner, nameof(owner));

                FileName = backupPath.Components.Last();
                Directory = ResolveBackupDirectory(backupPath, owner);
            }

            [NotNull]
            private DirectoryEntry ResolveBackupDirectory([NotNull] AbsolutePath backupPath, [NotNull] FileReplaceHandler owner)
            {
                AbsolutePath parentDirectoryPath = backupPath.TryGetParentPath();
                if (parentDirectoryPath == null)
                {
                    throw ErrorFactory.System.UnableToRemoveFileToBeReplaced();
                }

                var backupResolver = new DirectoryResolver(owner.Container)
                {
                    ErrorLastDirectoryFoundAsFile = _ => ErrorFactory.System.UnableToRemoveFileToBeReplaced(),
                    ErrorDirectoryFoundAsFile = _ => ErrorFactory.System.UnableToRemoveFileToBeReplaced()
                };
                DirectoryEntry backupDirectory = backupResolver.ResolveDirectory(parentDirectoryPath);

                if (backupDirectory.ContainsDirectory(FileName))
                {
                    throw ErrorFactory.System.UnableToRemoveFileToBeReplaced();
                }

                return backupDirectory;
            }

            [CanBeNull]
            public static BackupFileInfo Create([CanBeNull] AbsolutePath backupPath, [NotNull] FileReplaceHandler owner)
            {
                Guard.NotNull(owner, nameof(owner));
                return backupPath == null ? null : new BackupFileInfo(backupPath, owner);
            }

            [NotNull]
            public FileEntry GetExistingFile()
            {
                return Directory.GetFile(FileName);
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
