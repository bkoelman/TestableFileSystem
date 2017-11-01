﻿using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.HandlerArguments
{
    internal sealed class EntryMoveArguments
    {
        [NotNull]
        public AbsolutePath SourcePath { get; }

        [NotNull]
        public AbsolutePath DestinationPath { get; }

        public EntryMoveArguments([NotNull] AbsolutePath sourcePath, [NotNull] AbsolutePath destinationPath)
        {
            Guard.NotNull(sourcePath, nameof(sourcePath));
            Guard.NotNull(destinationPath, nameof(destinationPath));

            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }
    }
}
