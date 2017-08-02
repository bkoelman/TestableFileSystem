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

            FileEntry destinationFile = ResolveDestinationFile(arguments.DestinationPath, arguments.Overwrite);

            InitializeDestinationFile(destinationFile, sourceFile.LastWriteTimeUtc, sourceFile.Size, sourceFile.Attributes);

            IFileStream sourceStream = null;
            IFileStream destinationStream = null;

            try
            {
                sourceStream = sourceFile.Open(FileMode.Open, FileAccess.ReadWrite);
                destinationStream = destinationFile.Open(FileMode.Truncate, FileAccess.Write);

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
                ErrorPathIsVolumeRoot = incomingPath => ErrorFactory.DirectoryNotFound(incomingPath)
            };

            return sourceResolver.ResolveExistingFile(sourcePath);
        }

        [NotNull]
        private FileEntry ResolveDestinationFile([NotNull] AbsolutePath destinationPath, bool overwrite)
        {
            var destinationResolver = new FileResolver(Root)
            {
                ErrorFileFoundAsDirectory = incomingPath => ErrorFactory.TargetIsNotFile(incomingPath)
            };

            (DirectoryEntry destinationDirectory, FileEntry destinationFileOrNull, string fileName) =
                destinationResolver.TryResolveFile(destinationPath);

            if (destinationFileOrNull != null)
            {
                AssertCanOverwriteFile(overwrite, destinationPath);
                AssertIsNotReadOnly(destinationFileOrNull, destinationPath);
            }

            return destinationFileOrNull ?? destinationDirectory.GetOrCreateFile(fileName);
        }

        private void InitializeDestinationFile([NotNull] FileEntry destinationFile, DateTime sourceLastWriteTimeUtc,
            long sourceLength, FileAttributes sourceFileAttributes)
        {
            using (IFileStream createStream = destinationFile.Open(FileMode.Truncate, FileAccess.Write))
            {
                createStream.SetLength(sourceLength);
            }

            destinationFile.Attributes = sourceFileAttributes;

            DateTime now = Root.SystemClock.UtcNow();
            destinationFile.CreationTimeUtc = now;
            destinationFile.LastAccessTimeUtc = now;
            destinationFile.LastWriteTimeUtc = sourceLastWriteTimeUtc;
        }

        private static void AssertHasExclusiveAccess([NotNull] FileEntry file)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.FileIsInUse();
            }
        }

        [AssertionMethod]
        private void AssertCanOverwriteFile(bool overwrite, [NotNull] AbsolutePath path)
        {
            if (!overwrite)
            {
                throw ErrorFactory.FileAlreadyExists(path.GetText());
            }
        }

        [AssertionMethod]
        private void AssertIsNotReadOnly([NotNull] FileEntry file, [NotNull] AbsolutePath path)
        {
            if (file.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.UnauthorizedAccess(path.GetText());
            }
        }
    }
}
