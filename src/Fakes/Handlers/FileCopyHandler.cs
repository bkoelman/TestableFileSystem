using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileCopyHandler : FakeOperationHandler<FileCopyArguments, FileCopyResult>
    {
        private const FileAttributes HiddenReadOnlyMask = FileAttributes.Hidden | FileAttributes.ReadOnly;

        [NotNull]
        private readonly List<FileAccessKinds> pendingContentChanges = new List<FileAccessKinds>();

        public FileCopyHandler([NotNull] VolumeContainer container)
            : base(container)
        {
        }

        public override FileCopyResult Handle(FileCopyArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            pendingContentChanges.Clear();
            pendingContentChanges.Add(FileAccessKinds.Write);

            FileEntry sourceFile = ResolveSourceFile(arguments.SourcePath, arguments.IsCopyAfterMoveFailed);

            FileResolveResult destinationResolveResult = ResolveDestinationFile(arguments);
            DateTime? existingDestinationLastWriteTimeUtc = destinationResolveResult.ExistingFileOrNull?.LastWriteTimeUtc;
            FileEntry destinationFile = PrepareDestinationFile(destinationResolveResult, sourceFile, arguments);

            foreach (FileAccessKinds change in pendingContentChanges)
            {
                Container.ChangeTracker.NotifyContentsAccessed(destinationFile.PathFormatter, change);
            }

            IFileStream sourceStream = null;
            IFileStream destinationStream = null;

            try
            {
                sourceStream = sourceFile.Open(FileMode.Open, FileAccess.ReadWrite, arguments.SourcePath, false, false, false);
                destinationStream = destinationFile.Open(FileMode.Open, FileAccess.Write, arguments.DestinationPath, false, false,
                    false);

                return new FileCopyResult(sourceFile, sourceStream.AsStream(), destinationFile, destinationStream.AsStream(),
                    existingDestinationLastWriteTimeUtc);
            }
            catch (Exception)
            {
                destinationStream?.Dispose();
                sourceStream?.Dispose();

                throw;
            }
        }

        [NotNull]
        private FileEntry ResolveSourceFile([NotNull] AbsolutePath sourcePath, bool isCopyAfterMoveFailed)
        {
            var sourceResolver = new FileResolver(Container)
            {
                ErrorPathIsVolumeRoot = ErrorFactory.System.DirectoryNotFound
            };

            FileEntry sourceFile = sourceResolver.ResolveExistingFile(sourcePath);

            AssertIsNotExternallyEncrypted(sourceFile, sourcePath, isCopyAfterMoveFailed);
            AssertHasExclusiveAccess(sourceFile);
            AddChangesForSourceFile(sourceFile);

            return sourceFile;
        }

        [AssertionMethod]
        private static void AssertIsNotExternallyEncrypted([NotNull] FileEntry file, [NotNull] AbsolutePath absolutePath,
            bool isCopyAfterMoveFailed)
        {
            if (file.IsExternallyEncrypted)
            {
                throw isCopyAfterMoveFailed
                    ? ErrorFactory.System.UnauthorizedAccess()
                    : ErrorFactory.System.UnauthorizedAccess(absolutePath.GetText());
            }
        }

        private static void AssertHasExclusiveAccess([NotNull] FileEntry file)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.System.FileIsInUse();
            }
        }

        private void AddChangesForSourceFile([NotNull] FileEntry sourceFile)
        {
            if (sourceFile.Size > 0)
            {
                pendingContentChanges.Add(FileAccessKinds.WriteRead | FileAccessKinds.Resize);
            }

            if (sourceFile.Size >= 1024 * 4)
            {
                pendingContentChanges.Add(FileAccessKinds.Resize);
            }
        }

        [NotNull]
        private FileResolveResult ResolveDestinationFile([NotNull] FileCopyArguments arguments)
        {
            var destinationResolver = new FileResolver(Container)
            {
                ErrorFileFoundAsDirectory = ErrorFactory.System.TargetIsNotFile
            };

            return destinationResolver.TryResolveFile(arguments.DestinationPath);
        }

        [NotNull]
        private FileEntry PrepareDestinationFile([NotNull] FileResolveResult destinationResolveResult,
            [NotNull] FileEntry sourceFile, [NotNull] FileCopyArguments arguments)
        {
            DateTime utcNow = Container.SystemClock.UtcNow();

            FileEntry destinationFile;
            bool isNewlyCreated;
            if (destinationResolveResult.ExistingFileOrNull != null)
            {
                destinationFile = destinationResolveResult.ExistingFileOrNull;
                isNewlyCreated = false;

                AssertCanOverwriteFile(arguments.Overwrite, arguments.DestinationPath);
                AssertIsNotHiddenOrReadOnly(destinationFile, arguments.DestinationPath);
                AssertIsNotExternallyEncrypted(destinationFile, arguments.DestinationPath, arguments.IsCopyAfterMoveFailed);
                AssertSufficientDiskSpace(sourceFile.Size - destinationFile.Size,
                    destinationResolveResult.ContainingDirectory.Root);

                AddChangeForExistingDestinationFile(destinationFile);
            }
            else
            {
                AssertSufficientDiskSpace(sourceFile.Size, destinationResolveResult.ContainingDirectory.Root);

                destinationFile = destinationResolveResult.ContainingDirectory.CreateFile(destinationResolveResult.FileName);
                destinationFile.CreationTimeUtc = utcNow;
                isNewlyCreated = true;
            }

            using (IFileStream createStream = destinationFile.Open(FileMode.Truncate, FileAccess.Write, arguments.DestinationPath,
                isNewlyCreated, false, false))
            {
                createStream.SetLength(sourceFile.Size);
            }

            destinationFile.SetAttributes(sourceFile.Attributes);

            destinationFile.LastAccessTimeUtc = utcNow;
            destinationFile.LastWriteTimeUtc = utcNow;

            return destinationFile;
        }

        [AssertionMethod]
        private void AssertSufficientDiskSpace(long extraDiskSpaceNeeded, [NotNull] VolumeEntry volume)
        {
            if (extraDiskSpaceNeeded > 0 && volume.FreeSpaceInBytes < extraDiskSpaceNeeded)
            {
                throw ErrorFactory.System.NotEnoughSpaceOnDisk();
            }
        }

        private void AddChangeForExistingDestinationFile([NotNull] FileEntry destinationFile)
        {
            var accessKinds = FileAccessKinds.WriteRead;

            if (destinationFile.Size > 0)
            {
                accessKinds |= FileAccessKinds.Resize;
            }

            pendingContentChanges.Add(accessKinds);
        }

        [AssertionMethod]
        private void AssertCanOverwriteFile(bool overwrite, [NotNull] AbsolutePath path)
        {
            if (!overwrite)
            {
                throw ErrorFactory.System.FileAlreadyExists(path.GetText());
            }
        }

        [AssertionMethod]
        private void AssertIsNotHiddenOrReadOnly([CanBeNull] FileEntry fileEntry, [NotNull] AbsolutePath absolutePath)
        {
            if (fileEntry != null && (fileEntry.Attributes & HiddenReadOnlyMask) != 0)
            {
                throw ErrorFactory.System.UnauthorizedAccess(absolutePath.GetText());
            }
        }
    }
}
