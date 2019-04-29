using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryDeleteHandler : FakeOperationHandler<DirectoryDeleteArguments, Missing>
    {
        [NotNull]
        private readonly CurrentDirectoryManager currentDirectoryManager;

        public DirectoryDeleteHandler([NotNull] VolumeContainer container,
            [NotNull] CurrentDirectoryManager currentDirectoryManager)
            : base(container)
        {
            Guard.NotNull(currentDirectoryManager, nameof(currentDirectoryManager));
            this.currentDirectoryManager = currentDirectoryManager;
        }

        public override Missing Handle(DirectoryDeleteArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            AssertNotDeletingVolumeRoot(arguments);

            DirectoryEntry directory = ResolveDirectory(arguments.Path);

            AssertNoConflictWithCurrentDirectory(directory, arguments.Path);
            AssertIsEmptyOrRecursive(directory, arguments.IsRecursive);

            using (var recorder = new ErrorRecorder())
            {
                DeleteDirectoryTree(directory, arguments.IsRecursive, recorder);
            }

            return Missing.Value;
        }

        [AssertionMethod]
        private static void AssertNotDeletingVolumeRoot([NotNull] DirectoryDeleteArguments arguments)
        {
            if (arguments.Path.IsVolumeRoot)
            {
                if (!arguments.Path.IsOnLocalDrive)
                {
                    throw ErrorFactory.System.FileIsInUse(arguments.Path.GetText());
                }

                if (arguments.IsRecursive)
                {
                    throw ErrorFactory.System.FileNotFound(arguments.Path.GetText());
                }

                throw ErrorFactory.System.DirectoryIsNotEmpty();
            }
        }

        [AssertionMethod]
        private void AssertNoConflictWithCurrentDirectory([NotNull] DirectoryEntry directory, [NotNull] AbsolutePath path)
        {
            if (currentDirectoryManager.IsAtOrAboveCurrentDirectory(directory))
            {
                throw ErrorFactory.System.FileIsInUse(path.GetText());
            }
        }

        [NotNull]
        private DirectoryEntry ResolveDirectory([NotNull] AbsolutePath path)
        {
            var resolver = new DirectoryResolver(Container)
            {
                ErrorLastDirectoryFoundAsFile = incomingPath => ErrorFactory.System.DirectoryNameIsInvalid()
            };

            return resolver.ResolveDirectory(path);
        }

        [AssertionMethod]
        private static void AssertIsEmptyOrRecursive([NotNull] DirectoryEntry directory, bool recursive)
        {
            if (!recursive && !directory.IsEmpty)
            {
                throw ErrorFactory.System.DirectoryIsNotEmpty();
            }
        }

        [NotNull]
        private DirectoryState DeleteDirectoryTree([NotNull] DirectoryEntry directory, bool isRecursive,
            [NotNull] ErrorRecorder recorder)
        {
            var state = new DirectoryState(recorder);

            if (isRecursive)
            {
                state.MarkHasRead();

                foreach (BaseEntry entry in directory.EnumerateEntries(EnumerationFilter.All).OrderBy(x => x.Name).ToArray())
                {
                    if (entry is FileEntry file)
                    {
                        DeleteSingleFile(file, state);
                    }
                    else if (entry is DirectoryEntry subdirectory)
                    {
                        DirectoryState subState = DeleteDirectoryTree(subdirectory, true, recorder);
                        state.PropagateSubdirectoryState(subState);
                    }
                }
            }

            if (state.AccessKinds != FileAccessKinds.None)
            {
                Container.ChangeTracker.NotifyContentsAccessed(directory.PathFormatter, state.AccessKinds);
            }

            DeleteSingleDirectory(directory, state);

            return state;
        }

        private static void DeleteSingleFile([NotNull] FileEntry file, [NotNull] DirectoryState state)
        {
            if (file.IsOpen())
            {
                string path = file.PathFormatter.GetPath().GetText();
                state.SetError(ErrorFactory.System.FileIsInUse(path));
            }
            else if (file.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                state.SetError(ErrorFactory.System.UnauthorizedAccess(file.Name));
            }
            else
            {
                file.Parent.DeleteFile(file.Name, true);
                state.MarkHasWritten();
            }
        }

        private static void DeleteSingleDirectory([NotNull] DirectoryEntry directory, [NotNull] DirectoryState state)
        {
            if (directory.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                string path = directory.PathFormatter.GetPath().GetText();
                state.SetError(ErrorFactory.System.AccessDenied(path));
            }

            if (state.CanBeDeleted)
            {
                directory.Parent?.DeleteDirectory(directory.Name);
            }
        }

        private sealed class DirectoryState
        {
            [NotNull]
            private readonly ErrorRecorder recorder;

            public FileAccessKinds AccessKinds { get; private set; } = FileAccessKinds.None;
            public bool CanBeDeleted { get; private set; } = true;

            public DirectoryState([NotNull] ErrorRecorder recorder)
            {
                Guard.NotNull(recorder, nameof(recorder));
                this.recorder = recorder;
            }

            public void MarkHasRead()
            {
                AccessKinds |= FileAccessKinds.Read;
            }

            public void MarkHasWritten()
            {
                AccessKinds |= FileAccessKinds.Write;
            }

            public void SetError([NotNull] Exception exception)
            {
                recorder.Add(exception);
                CanBeDeleted = false;
            }

            public void PropagateSubdirectoryState([NotNull] DirectoryState subdirectoryState)
            {
                Guard.NotNull(subdirectoryState, nameof(subdirectoryState));

                if (!subdirectoryState.CanBeDeleted)
                {
                    CanBeDeleted = false;
                }

                if (subdirectoryState.AccessKinds.HasFlag(FileAccessKinds.Write))
                {
                    MarkHasWritten();
                }
            }
        }

        private sealed class ErrorRecorder : IDisposable
        {
            [CanBeNull]
            private Exception firstError;

            public void Add([NotNull] Exception exception)
            {
                if (firstError == null)
                {
                    firstError = exception;
                }
            }

            public void Dispose()
            {
                if (firstError != null)
                {
                    throw firstError;
                }
            }
        }
    }
}
