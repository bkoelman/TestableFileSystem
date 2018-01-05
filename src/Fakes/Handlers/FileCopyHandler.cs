using System;
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

        public FileCopyHandler([NotNull] DirectoryEntry root, [NotNull] FakeFileSystemChangeTracker changeTracker)
            : base(root, changeTracker)
        {
        }

        public override FileCopyResult Handle(FileCopyArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            FileEntry sourceFile = ResolveSourceFile(arguments.SourcePath);
            AssertHasExclusiveAccess(sourceFile);

            FileEntry destinationFile = ResolveDestinationFile(arguments.DestinationPath, arguments.Overwrite, sourceFile);

            IFileStream sourceStream = null;
            IFileStream destinationStream = null;

            try
            {
                sourceStream = sourceFile.Open(FileMode.Open, FileAccess.ReadWrite, arguments.SourcePath);
                destinationStream = destinationFile.Open(FileMode.Truncate, FileAccess.Write, arguments.DestinationPath);

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
            var sourceResolver = new FileResolver(Root) { ErrorPathIsVolumeRoot = ErrorFactory.System.DirectoryNotFound };

            return sourceResolver.ResolveExistingFile(sourcePath);
        }

        [NotNull]
        private FileEntry ResolveDestinationFile([NotNull] AbsolutePath destinationPath, bool overwrite,
            [NotNull] FileEntry sourceFile)
        {
            var destinationResolver = new FileResolver(Root) { ErrorFileFoundAsDirectory = ErrorFactory.System.TargetIsNotFile };
            FileResolveResult resolveResult = destinationResolver.TryResolveFile(destinationPath);

            DateTime utcNow = Root.SystemClock.UtcNow();

            FileEntry destinationFile;
            if (resolveResult.ExistingFileOrNull != null)
            {
                AssertCanOverwriteFile(overwrite, destinationPath);
                AssertIsNotHiddenOrReadOnly(resolveResult.ExistingFileOrNull, destinationPath);

                destinationFile = resolveResult.ContainingDirectory.Files[resolveResult.FileName];
            }
            else
            {
                destinationFile = resolveResult.ContainingDirectory.CreateFile(resolveResult.FileName);
                destinationFile.CreationTimeUtc = utcNow;
            }

            using (IFileStream createStream = destinationFile.Open(FileMode.Truncate, FileAccess.Write, destinationPath))
            {
                createStream.SetLength(sourceFile.Size);
            }

            destinationFile.Attributes = sourceFile.Attributes;

            destinationFile.LastAccessTimeUtc = utcNow;
            destinationFile.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;

            return destinationFile;
        }

        private static void AssertHasExclusiveAccess([NotNull] FileEntry file)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.System.FileIsInUse();
            }
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
