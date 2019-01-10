﻿using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileCopyHandler : FakeOperationHandler<FileCopyArguments, FileCopyResult>
    {
        private const FileAttributes HiddenReadOnlyMask = FileAttributes.Hidden | FileAttributes.ReadOnly;

        [NotNull]
        private readonly FakeFileSystemChangeTracker changeTracker;

        [NotNull]
        private readonly List<FileAccessKinds> pendingContentChanges = new List<FileAccessKinds>();

        public FileCopyHandler([NotNull] DirectoryEntry root, [NotNull] FakeFileSystemChangeTracker changeTracker)
            : base(root)
        {
            this.changeTracker = changeTracker;
        }

        public override FileCopyResult Handle(FileCopyArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            pendingContentChanges.Clear();
            pendingContentChanges.Add(FileAccessKinds.Write);

            FileEntry sourceFile = ResolveSourceFile(arguments.SourcePath);

            FileEntry destinationFile = ResolveDestinationFile(arguments.DestinationPath, arguments.Overwrite, sourceFile);

            foreach (FileAccessKinds change in pendingContentChanges)
            {
                changeTracker.NotifyContentsAccessed(destinationFile.PathFormatter, change);
            }

            IFileStream sourceStream = null;
            IFileStream destinationStream = null;

            try
            {
                sourceStream = sourceFile.Open(FileMode.Open, FileAccess.ReadWrite, arguments.SourcePath, false, false);
                destinationStream =
                    destinationFile.Open(FileMode.Open, FileAccess.Write, arguments.DestinationPath, false, false);

                return new FileCopyResult(sourceFile, sourceStream.AsStream(), destinationFile, destinationStream.AsStream());
            }
            catch (Exception)
            {
                destinationStream?.Dispose();
                sourceStream?.Dispose();

                throw;
            }
        }

        [NotNull]
        private FileEntry ResolveSourceFile([NotNull] AbsolutePath sourcePath)
        {
            var sourceResolver = new FileResolver(Root)
            {
                ErrorPathIsVolumeRoot = ErrorFactory.System.DirectoryNotFound
            };

            FileEntry sourceFile = sourceResolver.ResolveExistingFile(sourcePath);

            AssertHasExclusiveAccess(sourceFile);
            AddChangesForSourceFile(sourceFile);

            return sourceFile;
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
                pendingContentChanges.Add(FileAccessKinds.Write | FileAccessKinds.Read | FileAccessKinds.Resize);
            }

            if (sourceFile.Size >= 1024 * 4)
            {
                pendingContentChanges.Add(FileAccessKinds.Resize);
            }
        }

        [NotNull]
        private FileEntry ResolveDestinationFile([NotNull] AbsolutePath destinationPath, bool overwrite,
            [NotNull] FileEntry sourceFile)
        {
            var destinationResolver = new FileResolver(Root)
            {
                ErrorFileFoundAsDirectory = ErrorFactory.System.TargetIsNotFile
            };
            FileResolveResult resolveResult = destinationResolver.TryResolveFile(destinationPath);

            DateTime utcNow = Root.SystemClock.UtcNow();

            FileEntry destinationFile;
            bool isNewlyCreated;
            if (resolveResult.ExistingFileOrNull != null)
            {
                AssertCanOverwriteFile(overwrite, destinationPath);
                AssertIsNotHiddenOrReadOnly(resolveResult.ExistingFileOrNull, destinationPath);

                destinationFile = ResolveExistingDestinationFile(resolveResult);
                isNewlyCreated = false;
            }
            else
            {
                destinationFile = resolveResult.ContainingDirectory.CreateFile(resolveResult.FileName);
                destinationFile.CreationTimeUtc = utcNow;
                isNewlyCreated = true;
            }

            using (IFileStream createStream =
                destinationFile.Open(FileMode.Truncate, FileAccess.Write, destinationPath, isNewlyCreated, false))
            {
                createStream.SetLength(sourceFile.Size);
            }

            destinationFile.SetAttributes(sourceFile.Attributes);

            destinationFile.LastAccessTimeUtc = utcNow;
            destinationFile.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;

            return destinationFile;
        }

        [NotNull]
        private FileEntry ResolveExistingDestinationFile([NotNull] FileResolveResult resolveResult)
        {
            FileEntry destinationFile = resolveResult.ContainingDirectory.Files[resolveResult.FileName];

            AddChangeForExistingDestinationFile(destinationFile);

            return destinationFile;
        }

        private void AddChangeForExistingDestinationFile([NotNull] FileEntry destinationFile)
        {
            FileAccessKinds accessKinds = FileAccessKinds.Write | FileAccessKinds.Read;

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
