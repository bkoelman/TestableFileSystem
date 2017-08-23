using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileCopyHandler
        : FakeOperationHandler<FileCopyArguments, (FileEntry sourceFile, Stream sourceStream, FileEntry destinationFile, Stream
            destinationStream)>
    {
        private const FileAttributes HiddenReadOnlyMask = FileAttributes.Hidden | FileAttributes.ReadOnly;

        public FileCopyHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override (FileEntry sourceFile, Stream sourceStream, FileEntry destinationFile, Stream destinationStream) Handle(
            FileCopyArguments arguments)
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

                return (sourceFile, sourceStream.AsStream(), destinationFile, destinationStream.AsStream());
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

            return sourceResolver.ResolveExistingFile(sourcePath);
        }

        [NotNull]
        private FileEntry ResolveDestinationFile([NotNull] AbsolutePath destinationPath, bool overwrite,
            [NotNull] FileEntry sourceFile)
        {
            var destinationResolver = new FileResolver(Root)
            {
                ErrorFileFoundAsDirectory = ErrorFactory.System.TargetIsNotFile
            };

            (DirectoryEntry destinationDirectory, FileEntry destinationFileOrNull, string fileName) =
                destinationResolver.TryResolveFile(destinationPath);

            DateTime now = Root.SystemClock.UtcNow();

            FileEntry destinationFile;
            if (destinationFileOrNull != null)
            {
                AssertCanOverwriteFile(overwrite, destinationPath);
                AssertIsNotHiddenOrReadOnly(destinationFileOrNull, destinationPath);

                destinationFile = destinationDirectory.Files[fileName];
            }
            else
            {
                destinationFile = destinationDirectory.CreateFile(fileName);
                destinationFile.CreationTimeUtc = now;
            }

            using (IFileStream createStream = destinationFile.Open(FileMode.Truncate, FileAccess.Write, destinationPath))
            {
                createStream.SetLength(sourceFile.Size);
            }

            destinationFile.Attributes = sourceFile.Attributes;

            destinationFile.LastAccessTimeUtc = now;
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
