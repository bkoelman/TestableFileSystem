using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileSetAttributesHandler : FakeOperationHandler<FileSetAttributesArguments, Missing>
    {
        public FileSetAttributesHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override Missing Handle(FileSetAttributesArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            var resolver = new EntryResolver(Root);
            BaseEntry entry = resolver.ResolveEntry(arguments.Path);

            FileAccessKinds accessKinds = arguments.AccessKinds;
            if (entry is DirectoryEntry)
            {
                accessKinds |= FileAccessKinds.Read;
            }

            entry.SetAttributes(arguments.Attributes, accessKinds);

            return Missing.Value;
        }
    }
}
