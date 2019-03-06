using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class EntryGetPropertiesHandler : FakeOperationHandler<EntryGetPropertiesArguments, EntryProperties>
    {
        public EntryGetPropertiesHandler([NotNull] VolumeContainer container)
            : base(container)
        {
        }

        public override EntryProperties Handle(EntryGetPropertiesArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            var resolver = new EntryResolver(Container)
            {
                ErrorDirectoryFoundAsFile = ErrorFactory.System.FileNotFound,
                ErrorLastDirectoryFoundAsFile = ErrorFactory.System.FileNotFound,
                ErrorDirectoryNotFound = ErrorFactory.System.FileNotFound
            };

            BaseEntry entry;
            try
            {
                entry = resolver.ResolveEntry(arguments.Path);
            }
            catch (FileNotFoundException)
            {
                return EntryProperties.Default;
            }
            catch (Exception ex)
            {
                return EntryProperties.CreateForError(ex);
            }

            long fileSize = entry is FileEntry fileEntry ? fileEntry.Size : -1;

            return EntryProperties.CreateForSuccess(entry.Attributes, entry.CreationTimeUtc, entry.LastAccessTimeUtc,
                entry.LastWriteTimeUtc, fileSize);
        }
    }
}
