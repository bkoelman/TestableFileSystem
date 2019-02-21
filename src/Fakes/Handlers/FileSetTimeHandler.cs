using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

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

            var resolver = new EntryResolver(Root);
            BaseEntry entry = resolver.ResolveEntry(arguments.Path);

            AssertIsNotExternallyEncrypted(entry, arguments.Path);
            AssertIsNotDirectory(entry, arguments.Path);
            AssertFileIsNotReadOnly(entry, arguments.Path);
            AssertHasExclusiveAccessToFile(entry, arguments.Path);

            switch (arguments.Kind)
            {
                case FileTimeKind.CreationTime:
                {
                    if (arguments.IsInUtc)
                    {
                        entry.CreationTimeUtc = arguments.TimeValue;
                    }
                    else
                    {
                        entry.CreationTime = arguments.TimeValue;
                    }

                    changeTracker.NotifyContentsAccessed(entry.PathFormatter, FileAccessKinds.Create);
                    break;
                }
                case FileTimeKind.LastWriteTime:
                {
                    if (arguments.IsInUtc)
                    {
                        entry.LastWriteTimeUtc = arguments.TimeValue;
                    }
                    else
                    {
                        entry.LastWriteTime = arguments.TimeValue;
                    }

                    changeTracker.NotifyContentsAccessed(entry.PathFormatter, FileAccessKinds.Write);
                    break;
                }
                case FileTimeKind.LastAccessTime:
                {
                    if (arguments.IsInUtc)
                    {
                        entry.LastAccessTimeUtc = arguments.TimeValue;
                    }
                    else
                    {
                        entry.LastAccessTime = arguments.TimeValue;
                    }

                    changeTracker.NotifyContentsAccessed(entry.PathFormatter, FileAccessKinds.Read);
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

        [AssertionMethod]
        private static void AssertIsNotExternallyEncrypted([NotNull] BaseEntry entry, [NotNull] AbsolutePath absolutePath)
        {
            if (entry.IsExternallyEncrypted)
            {
                throw ErrorFactory.System.UnauthorizedAccess(absolutePath.GetText());
            }
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
        private void AssertFileIsNotReadOnly([NotNull] BaseEntry entry, [NotNull] AbsolutePath absolutePath)
        {
            if (entry is FileEntry file && file.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.System.UnauthorizedAccess(absolutePath.GetText());
            }
        }

        private static void AssertHasExclusiveAccessToFile([NotNull] BaseEntry entry, [NotNull] AbsolutePath absolutePath)
        {
            if (entry is FileEntry file && file.IsOpen())
            {
                throw ErrorFactory.System.FileIsInUse(absolutePath.GetText());
            }
        }
    }
}
