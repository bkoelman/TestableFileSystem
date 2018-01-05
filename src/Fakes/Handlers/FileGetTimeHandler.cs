using System;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileGetTimeHandler : FakeOperationHandler<FileGetTimeArguments, DateTime>
    {
        public FileGetTimeHandler([NotNull] DirectoryEntry root, [NotNull] FakeFileSystemChangeTracker changeTracker)
            : base(root, changeTracker)
        {
        }

        public override DateTime Handle(FileGetTimeArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            var resolver = new EntryResolver(Root);
            BaseEntry entry = resolver.SafeResolveEntry(arguments.Path);

            if (entry == null)
            {
                return arguments.IsInUtc ? PathFacts.ZeroFileTimeUtc : PathFacts.ZeroFileTime;
            }

            switch (arguments.Kind)
            {
                case FileTimeKind.CreationTime:
                    return arguments.IsInUtc ? entry.CreationTimeUtc : entry.CreationTime;
                case FileTimeKind.LastWriteTime:
                    return arguments.IsInUtc ? entry.LastWriteTimeUtc : entry.LastWriteTime;
                case FileTimeKind.LastAccessTime:
                    return arguments.IsInUtc ? entry.LastAccessTimeUtc : entry.LastAccessTime;
                default:
                    throw ErrorFactory.Internal.EnumValueUnsupported(arguments.Kind);
            }
        }
    }
}
