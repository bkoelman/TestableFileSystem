﻿using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileGetAttributesHandler : FakeOperationHandler<FileGetAttributesArguments, FileAttributes>
    {
        public FileGetAttributesHandler([NotNull] VolumeContainer container)
            : base(container)
        {
        }

        public override FileAttributes Handle(FileGetAttributesArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            var resolver = new EntryResolver(Container);
            BaseEntry entry = resolver.ResolveEntry(arguments.Path);

            return entry.Attributes;
        }
    }
}
