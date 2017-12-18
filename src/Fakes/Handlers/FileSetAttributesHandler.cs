using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileSetAttributesHandler : FakeOperationHandler<FileSetAttributesArguments, object>
    {
        public FileSetAttributesHandler([NotNull] DirectoryEntry root, [NotNull] FileSystemChangeTracker changeTracker)
            : base(root, changeTracker)
        {
        }

        public override object Handle(FileSetAttributesArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            var resolver = new EntryResolver(Root);
            BaseEntry entry = resolver.ResolveEntry(arguments.Path);

            entry.Attributes = arguments.Attributes;

            return Missing.Value;
        }
    }
}
