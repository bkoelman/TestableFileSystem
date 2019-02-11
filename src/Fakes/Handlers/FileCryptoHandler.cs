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
        // - encrypted files cannot be accessed by another user (must impersonate)
        // - encrypted folders can be accessed by another user (you can add your own encrypted files to it)
        // - when creating new file (or copy an unencrypted file) in an encrypted folder, the file will also be encrypted
        //   - rename/move-into-on-same-drive does not change file encryption status; move into-from-other-drive behaves the same as copy
        // - calling encrypt on file/folder multiple times (as encrypting user) does nothing
        // - calling decrypt on non-encrypted file/folder does nothing
        // - calling encrypt/decrypt on file that was encrypted by another user throws System.UnauthorizedAccessException: 'Access to the path 'd:\FileSystemTests\file.txt' is denied.'
        // - calling encrypt/decrypt on folder that was encrypted by another user just works (encrypt: does not change who encrypted the folder)
        // - an encrypted folder is accessible by everyone
        // - when adding file to a directory that was encrypted by another user, your file will be encrypted with you as crypto owner
        // - reading/writing from/to file that was encrypted by another user throws System.UnauthorizedAccessException: 'Access to the path 'd:\FileSystemTests\file.txt' is denied.'
        // - file attributes includes Encrypted flag for encrypted file/folder
        // - the type of exception thrown for missing drive varies per runtime
        // - when encrypting a file that is in use (even if file was already encrypted), throws System.IO.IOException: 'The process cannot access the file 'd:\FileSystemTests\file.txt' because it is being used by another process.'
        // - when encrypting/decrypting a directory, sets/clears Encrypted attribute (but does not touch existing files or subdirectories in it)
        // - when encrypting readonly directory, it succeeds
        // - when encrypting readonly file, it throws System.IO.IOException: "The specified file is read only."

        // TODO: Check for empty path etc...
        // TODO: Test the various operations on files/folders, such as Delete, Create, Copy etc...

        public override Missing Handle(FileCryptoArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            var resolver = new FileResolver(Root);
            FileEntry fileEntry = resolver.ResolveExistingFile(arguments.Path);

            fileEntry.EncryptorAccountName = arguments.IsEncrypt ? fileEntry.LoggedOnAccount.UserName : null;

            return Missing.Value;
        }
    }
}
