using System;
using System.Collections.Generic;
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
        [NotNull]
        private readonly Random randomNumberGenerator;

        public FileReplaceHandler([NotNull] VolumeContainer container, [NotNull] Random randomNumberGenerator)
            : base(container)
        {
            Guard.NotNull(randomNumberGenerator, nameof(randomNumberGenerator));
            this.randomNumberGenerator = randomNumberGenerator;
        }

        public override Missing Handle(FileReplaceArguments arguments)
        {
            FileEntry sourceFile = ResolveExistingFile(arguments.SourcePath);

            AssertNoDuplicatePaths(arguments);
            AssertAllOnSameDrive(arguments);

            FileEntry destinationFile = ResolveExistingFile(arguments.DestinationPath);

            Container.ChangeTracker.NotifyContentsAccessed(sourceFile.PathFormatter, FileAccessKinds.Create);
            NotifyDirectoriesChanged(false, destinationFile.Parent, sourceFile.Parent);

            BackupFileInfo backupFileInfo = BackupFileInfo.FromPath(arguments.BackupDestinationPath, this);
            if (backupFileInfo != null && !backupFileInfo.Exists)
            {
                string destinationFileName = destinationFile.Name;
                DirectoryEntry destinationDirectory = destinationFile.Parent;

                MoveSingleFile(destinationFile, backupFileInfo.Directory, backupFileInfo.FileName, destinationFile.PathFormatter,
                    false);

                if (destinationDirectory != backupFileInfo.Directory)
                {
                    NotifyDirectoriesChanged(false, destinationDirectory);
                }

                Container.ChangeTracker.NotifyContentsAccessed(sourceFile.PathFormatter, FileAccessKinds.Security);

                MoveSingleFile(sourceFile, destinationDirectory, destinationFileName, sourceFile.PathFormatter, true);
            }
            else
            {
                var spaceTracker = new VolumeSpaceTracker(sourceFile.Parent.Root);

                if (backupFileInfo == null)
                {
                    FileEntry tempBackupFile = CreateTempBackupFile(destinationFile);
                    NotifyDirectoriesChanged(false, tempBackupFile.Parent);

                    backupFileInfo = BackupFileInfo.FromTempFile(tempBackupFile);
                }

                FileEntry backupFile = backupFileInfo.GetExistingFile();

                if (backupFile.Parent != destinationFile.Parent && backupFile.Parent != sourceFile.Parent)
                {
                    NotifyDirectoriesChanged(false, backupFile.Parent);
                }

                TransferDestinationContentsToExistingBackupFile(destinationFile, backupFile, spaceTracker);

                destinationFile.Parent.DeleteFile(destinationFile.Name, true);

                Container.ChangeTracker.NotifyContentsAccessed(backupFile.PathFormatter, FileAccessKinds.All);
                NotifyDirectoriesChanged(backupFileInfo.IsTempFile, backupFile.Parent, destinationFile.Parent);

                TransferSourceContentsToDestinationFile(sourceFile, destinationFile);

                spaceTracker.ApplyVolumeSpaceChange();

                backupFileInfo.Cleanup();
            }

            return Missing.Value;
        }

        private void NotifyDirectoriesChanged(bool skipNotifyLastAccessOnLastDirectory,
            [NotNull] [ItemNotNull] params DirectoryEntry[] directories)
        {
            List<DirectoryEntry> uniqueDirectories = directories.Distinct().ToList();

            while (uniqueDirectories.Any())
            {
                FileAccessKinds accessKinds = uniqueDirectories.Count == 1 && skipNotifyLastAccessOnLastDirectory
                    ? FileAccessKinds.Write
                    : FileAccessKinds.WriteRead;

                DirectoryEntry directory = uniqueDirectories.First();
                Container.ChangeTracker.NotifyContentsAccessed(directory.PathFormatter, accessKinds);

                uniqueDirectories.Remove(directory);
            }
        }

        [NotNull]
        private FileEntry CreateTempBackupFile([NotNull] FileEntry destinationFile)
        {
            DirectoryEntry directory = destinationFile.Parent;
            string tempFileName = $"{destinationFile.Name}~RF{randomNumberGenerator.Next(1, 0xFFFFFF):x}.TMP";

            if (directory.ContainsFile(tempFileName) || directory.ContainsDirectory(tempFileName))
            {
                throw ErrorFactory.System.UnableToRemoveFileToBeReplaced();
            }

            return directory.CreateFile(tempFileName);
        }

        private void TransferDestinationContentsToExistingBackupFile([NotNull] FileEntry destinationFile,
            [NotNull] FileEntry backupFile, [NotNull] VolumeSpaceTracker spaceTracker)
        {
            AssertBackupFileIsNotReadOnly(backupFile);
            AssertHasExclusiveAccessToBackupFile(backupFile);

            spaceTracker.SetBackupFileSizeChange(destinationFile.Size - backupFile.Size);

            backupFile.TransferFrom(destinationFile);
        }

        private void TransferSourceContentsToDestinationFile([NotNull] FileEntry sourceFile, [NotNull] FileEntry destinationFile)
        {
            Container.ChangeTracker.NotifyContentsAccessed(sourceFile.PathFormatter, FileAccessKinds.Security);

            MoveSingleFile(sourceFile, destinationFile.Parent, destinationFile.Name, sourceFile.PathFormatter, true);

            sourceFile.CopyPropertiesFrom(destinationFile);
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
                sourceFile.Parent.RenameFile(sourceFile.Name, destinationFileName, sourcePathFormatterNonLazy,
                    skipNotifyLastAccess);
            }
            else
            {
                sourceFile.Parent.DeleteFile(sourceFile.Name, true);
                destinationDirectory.MoveFileToHere(sourceFile, destinationFileName, skipNotifyLastAccess);
            }
        }

        private sealed class BackupFileInfo
        {
            [NotNull]
            public string FileName { get; }

            [NotNull]
            public DirectoryEntry Directory { get; }

            public bool Exists => Directory.ContainsFile(FileName);

            public bool IsTempFile { get; }

            private BackupFileInfo([NotNull] FileEntry tempBackupFile)
            {
                Guard.NotNull(tempBackupFile, nameof(tempBackupFile));

                FileName = tempBackupFile.Name;
                Directory = tempBackupFile.Parent;
                IsTempFile = true;
            }

            private BackupFileInfo([NotNull] AbsolutePath backupPath, [NotNull] FileReplaceHandler owner)
            {
                Guard.NotNull(backupPath, nameof(backupPath));
                Guard.NotNull(owner, nameof(owner));

                FileName = backupPath.Components.Last();
                Directory = ResolveBackupDirectory(backupPath, owner);
                IsTempFile = false;
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

            [NotNull]
            public static BackupFileInfo FromTempFile([NotNull] FileEntry backupFile)
            {
                Guard.NotNull(backupFile, nameof(backupFile));
                return new BackupFileInfo(backupFile);
            }

            [CanBeNull]
            public static BackupFileInfo FromPath([CanBeNull] AbsolutePath backupPath, [NotNull] FileReplaceHandler owner)
            {
                Guard.NotNull(owner, nameof(owner));
                return backupPath == null ? null : new BackupFileInfo(backupPath, owner);
            }

            [NotNull]
            public FileEntry GetExistingFile()
            {
                return Directory.GetFile(FileName);
            }

            public void Cleanup()
            {
                if (IsTempFile)
                {
                    FileEntry backupFile = GetExistingFile();
                    backupFile.Parent.DeleteFile(backupFile.Name, true);
                }
            }
        }

        private sealed class VolumeSpaceTracker
        {
            [NotNull]
            private readonly VolumeEntry volume;

            private long destinationToBackupFileSizeDelta;

            public VolumeSpaceTracker([NotNull] VolumeEntry volume)
            {
                Guard.NotNull(volume, nameof(volume));
                this.volume = volume;
            }

            public void SetBackupFileSizeChange(long bytes)
            {
                destinationToBackupFileSizeDelta = bytes;
            }

            public void ApplyVolumeSpaceChange()
            {
                if (destinationToBackupFileSizeDelta != 0)
                {
                    if (!volume.TryAllocateSpace(destinationToBackupFileSizeDelta))
                    {
                        throw ErrorFactory.Internal.UnknownError(
                            $"Disk space on volume '{volume.Name}' ({volume.FreeSpaceInBytes} bytes) would become negative.");
                    }
                }
            }
        }
    }
}
