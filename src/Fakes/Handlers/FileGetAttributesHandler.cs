using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileGetAttributesHandler : FakeOperationHandler<FileGetAttributesArguments, FileAttributes>
    {
        public FileGetAttributesHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override FileAttributes Handle(FileGetAttributesArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            var resolver = new EntryResolver(Root);
            BaseEntry entry = resolver.ResolveEntry(arguments.Path);

            return entry.Attributes;
        }
    }
}
