﻿using System;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectorySetTimeHandler : FakeOperationHandler<EntrySetTimeArguments, Missing>
    {
        public DirectorySetTimeHandler([NotNull] VolumeContainer container)
            : base(container)
        {
        }

        public override Missing Handle(EntrySetTimeArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));
            AssertTimeValueIsInRange(arguments);
            AssertIsNotDriveRoot(arguments.Path);

            var resolver = new EntryResolver(Container);
            BaseEntry entry = resolver.ResolveEntry(arguments.Path);

            if (entry is FileEntry fileEntry)
            {
                AssertFileIsNotExternallyEncrypted(fileEntry, arguments.Path);
            }

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

                    Container.ChangeTracker.NotifyContentsAccessed(entry.PathFormatter, FileAccessKinds.Create);
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

                    Container.ChangeTracker.NotifyContentsAccessed(entry.PathFormatter, FileAccessKinds.Write);
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

                    Container.ChangeTracker.NotifyContentsAccessed(entry.PathFormatter, FileAccessKinds.Read);
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
                throw ErrorFactory.System.FileTimeNotValid(nameof(arguments.Path));
            }
        }

        private void AssertIsNotDriveRoot([NotNull] AbsolutePath path)
        {
            if (path.IsVolumeRoot && path.IsOnLocalDrive)
            {
                throw ErrorFactory.System.PathMustNotBeDrive(nameof(path));
            }
        }

        [AssertionMethod]
        private static void AssertFileIsNotExternallyEncrypted([NotNull] FileEntry file, [NotNull] AbsolutePath absolutePath)
        {
            if (file.IsExternallyEncrypted)
            {
                throw ErrorFactory.System.UnauthorizedAccess(absolutePath.GetText());
            }
        }
    }
}
