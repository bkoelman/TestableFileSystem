using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileSetAttributesHandler : FakeOperationHandler<FileSetAttributesArguments, Missing>
    {
        public FileSetAttributesHandler([NotNull] VolumeContainer container)
            : base(container)
        {
        }

        public override Missing Handle(FileSetAttributesArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            var resolver = new EntryResolver(Container);
            BaseEntry entry = resolver.ResolveEntry(arguments.Path);

            entry.SetAttributes(arguments.Attributes, arguments.AccessKinds);

            return Missing.Value;
        }
    }
}
