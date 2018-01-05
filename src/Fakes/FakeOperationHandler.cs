﻿using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal abstract class FakeOperationHandler<TArguments, TResult>
    {
        [NotNull]
        protected DirectoryEntry Root { get; }

        [NotNull]
        protected FakeFileSystemChangeTracker ChangeTracker { get; }

        protected FakeOperationHandler([NotNull] DirectoryEntry root, [NotNull] FakeFileSystemChangeTracker changeTracker)
        {
            Guard.NotNull(root, nameof(root));
            Guard.NotNull(changeTracker, nameof(changeTracker));

            Root = root;
            ChangeTracker = changeTracker;
        }

        [NotNull]
        public abstract TResult Handle([NotNull] TArguments arguments);
    }
}
