using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileCopyResult
    {
        [NotNull]
        public FileEntry SourceFile { get; }

        [NotNull]
        public Stream SourceStream { get; }

        [NotNull]
        public FileEntry DestinationFile { get; }

        [NotNull]
        public Stream DestinationStream { get; }

        [CanBeNull]
        public DateTime? ExistingDestinationLastWriteTimeUtc { get; }

        public FileCopyResult([NotNull] FileEntry sourceFile, [NotNull] Stream sourceStream, [NotNull] FileEntry destinationFile,
            [NotNull] Stream destinationStream, [CanBeNull] DateTime? existingDestinationLastWriteTimeUtc)
        {
            Guard.NotNull(sourceFile, nameof(sourceFile));
            Guard.NotNull(sourceStream, nameof(sourceStream));
            Guard.NotNull(destinationFile, nameof(destinationFile));
            Guard.NotNull(destinationStream, nameof(destinationStream));

            SourceFile = sourceFile;
            SourceStream = sourceStream;
            DestinationFile = destinationFile;
            DestinationStream = destinationStream;
            ExistingDestinationLastWriteTimeUtc = existingDestinationLastWriteTimeUtc;
        }
    }
}
