using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileCryptoHandler : FakeOperationHandler<FileCryptoArguments, Missing>
    {
        [NotNull]
        private readonly FakeFileSystem owner;

        public FileCryptoHandler([NotNull] DirectoryEntry root, [NotNull] FakeFileSystem owner)
            : base(root)
        {
            Guard.NotNull(owner, nameof(owner));
            this.owner = owner;
        }

        // TODO: How encrypt/decrypt works...
        // - when using on non-NTFS-formatted drive, throws System.NotSupportedException: 'File encryption support only works on NTFS partitions.'
        // - when running on non-windows or Windows Home Edition, throws System.NotSupportedException: "File encryption is not supported on this platform."
        // - the type of exception thrown for missing drive varies per runtime
        // - when encrypting a file that is in use (even if file was already encrypted), throws System.IO.IOException: 'The process cannot access the file 'd:\FileSystemTests\file.txt' because it is being used by another process.'
        // - when encrypting readonly directory, it succeeds
        // - when encrypting readonly file, it throws System.IO.IOException: "The specified file is read only."

        // TODO: Check for empty path etc...

        public override Missing Handle(FileCryptoArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            var resolver = new EntryResolver(Root);
            BaseEntry entry = resolver.ResolveEntry(arguments.Path);

            if (entry is FileEntry fileEntry)
            {
                AssertFileIsNotExternallyEncrypted(fileEntry, arguments.Path);
            }

            if (arguments.IsEncrypt)
            {
                entry.SetEncrypted();
            }
            else
            {
                entry.ClearEncrypted();
            }

            return Missing.Value;
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
