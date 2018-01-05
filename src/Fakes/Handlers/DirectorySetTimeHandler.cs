using System;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectorySetTimeHandler : FakeOperationHandler<EntrySetTimeArguments, object>
    {
        public DirectorySetTimeHandler([NotNull] DirectoryEntry root, [NotNull] FakeFileSystemChangeTracker changeTracker)
            : base(root, changeTracker)
        {
        }

        public override object Handle(EntrySetTimeArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));
            AssertTimeValueIsInRange(arguments);
            AssertIsNotDriveRoot(arguments.Path);

            var resolver = new EntryResolver(Root);
            BaseEntry entry = resolver.ResolveEntry(arguments.Path);

            switch (arguments.Kind)
            {
                case FileTimeKind.CreationTime:
                    if (arguments.IsInUtc)
                    {
                        entry.CreationTimeUtc = arguments.TimeValue;
                    }
                    else
                    {
                        entry.CreationTime = arguments.TimeValue;
                    }
                    break;
                case FileTimeKind.LastWriteTime:
                    if (arguments.IsInUtc)
                    {
                        entry.LastWriteTimeUtc = arguments.TimeValue;
                    }
                    else
                    {
                        entry.LastWriteTime = arguments.TimeValue;
                    }
                    break;
                case FileTimeKind.LastAccessTime:
                    if (arguments.IsInUtc)
                    {
                        entry.LastAccessTimeUtc = arguments.TimeValue;
                    }
                    else
                    {
                        entry.LastAccessTime = arguments.TimeValue;
                    }
                    break;
                default:
                    throw ErrorFactory.Internal.EnumValueUnsupported(arguments.Kind);
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
    }
}
