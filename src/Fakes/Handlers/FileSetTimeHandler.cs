using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileSetTimeHandler : FakeOperationHandler<EntrySetTimeArguments, Missing>
    {
        [NotNull]
        private readonly FakeFileSystemChangeTracker changeTracker;

        public FileSetTimeHandler([NotNull] DirectoryEntry root, [NotNull] FakeFileSystemChangeTracker changeTracker)
            : base(root)
        {
            Guard.NotNull(changeTracker, nameof(changeTracker));
            this.changeTracker = changeTracker;
        }

        public override Missing Handle(EntrySetTimeArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));
            AssertTimeValueIsInRange(arguments);
            AssertIsNotVolumeRoot(arguments.Path);

            FileEntry file = ResolveFile(arguments);

            AssertIsNotReadOnly(file, arguments.Path);
            AssertIsNotExternallyEncrypted(file, arguments.Path);
            AssertHasExclusiveAccessToFile(file, arguments.Path);

            switch (arguments.Kind)
            {
                case FileTimeKind.CreationTime:
                {
                    if (arguments.IsInUtc)
                    {
                        file.CreationTimeUtc = arguments.TimeValue;
                    }
                    else
                    {
                        file.CreationTime = arguments.TimeValue;
                    }

                    changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Create);
                    break;
                }
                case FileTimeKind.LastWriteTime:
                {
                    if (arguments.IsInUtc)
                    {
                        file.LastWriteTimeUtc = arguments.TimeValue;
                    }
                    else
                    {
                        file.LastWriteTime = arguments.TimeValue;
                    }

                    changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Write);
                    break;
                }
                case FileTimeKind.LastAccessTime:
                {
                    if (arguments.IsInUtc)
                    {
                        file.LastAccessTimeUtc = arguments.TimeValue;
                    }
                    else
                    {
                        file.LastAccessTime = arguments.TimeValue;
                    }

                    changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Read);
                    break;
                }
                default:
                {
                    throw ErrorFactory.Internal.EnumValueUnsupported(arguments.Kind);
                }
            }

            return Missing.Value;
        }

        private static void AssertTimeValueIsInRange([NotNull] EntrySetTimeArguments arguments)
        {
            DateTime minTime = arguments.IsInUtc ? PathFacts.ZeroFileTimeUtc : PathFacts.ZeroFileTime;

            if (arguments.TimeValue < minTime)
            {
                throw ErrorFactory.System.FileTimeOutOfRange(nameof(arguments.Path));
            }
        }

        private void AssertIsNotVolumeRoot([NotNull] AbsolutePath path)
        {
            if (path.IsVolumeRoot)
            {
                if (path.IsOnLocalDrive)
                {
                    throw ErrorFactory.System.PathMustNotBeDrive(nameof(path));
                }

                throw ErrorFactory.System.UnauthorizedAccess(path.GetText());
            }
        }

        [NotNull]
        private FileEntry ResolveFile([NotNull] EntrySetTimeArguments arguments)
        {
            var resolver = new EntryResolver(Root);
            BaseEntry entry = resolver.ResolveEntry(arguments.Path);

            AssertIsNotDirectory(entry, arguments.Path);

            return (FileEntry)entry;
        }

        [AssertionMethod]
        private void AssertIsNotDirectory([NotNull] BaseEntry entry, [NotNull] AbsolutePath path)
        {
            if (entry is DirectoryEntry)
            {
                throw ErrorFactory.System.DirectoryNotFound(path.GetText());
            }
        }

        [AssertionMethod]
        private void AssertIsNotReadOnly([NotNull] FileEntry file, [NotNull] AbsolutePath absolutePath)
        {
            if (file.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.System.UnauthorizedAccess(absolutePath.GetText());
            }
        }

        [AssertionMethod]
        private static void AssertIsNotExternallyEncrypted([NotNull] FileEntry file, [NotNull] AbsolutePath absolutePath)
        {
            if (file.IsExternallyEncrypted)
            {
                throw ErrorFactory.System.UnauthorizedAccess(absolutePath.GetText());
            }
        }

        private static void AssertHasExclusiveAccessToFile([NotNull] FileEntry file, [NotNull] AbsolutePath absolutePath)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.System.FileIsInUse(absolutePath.GetText());
            }
        }
    }
}
