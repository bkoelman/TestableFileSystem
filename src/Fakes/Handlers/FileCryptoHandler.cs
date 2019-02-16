using System;
using System.IO;
using System.Linq;
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

        public override Missing Handle(FileCryptoArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));
            AssertIsEncryptingOnNtFsVolume(arguments);

            BaseEntry entry = ResolveEntry(arguments);

            if (entry is FileEntry fileEntry)
            {
                AssertFileIsNotExternallyEncrypted(fileEntry, arguments.Path);
                AssertHasExclusiveAccess(fileEntry, arguments.Path);
                AssertFileIsNotReadOnly(fileEntry);
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

        private void AssertIsEncryptingOnNtFsVolume([NotNull] FileCryptoArguments arguments)
        {
            if (arguments.IsEncrypt)
            {
                string volumeRoot = arguments.Path.Components.First();
                FakeVolume volume = owner.GetVolume(volumeRoot);
                if (volume != null && volume.Format != FakeVolume.NtFs)
                {
                    throw new NotSupportedException("File encryption support only works on NTFS partitions.");
                }
            }
        }

        [NotNull]
        private BaseEntry ResolveEntry([NotNull] FileCryptoArguments arguments)
        {
            EntryResolver resolver = arguments.IsEncrypt
                ? new EntryResolver(Root)
                {
                    ErrorNetworkShareNotFound = _ => ErrorFactory.System.FileOrDirectoryOrVolumeIsIncorrect(),
                    ErrorDirectoryNotFound = path => IsPathOnExistingVolume(path)
                        ? ErrorFactory.System.DirectoryNotFound(path)
                        : ErrorFactory.System.FileNotFound(path)
                }
                : new EntryResolver(Root);
            return resolver.ResolveEntry(arguments.Path);
        }

        private bool IsPathOnExistingVolume([NotNull] string path)
        {
            var absolutePath = new AbsolutePath(path);
            string volumeRoot = absolutePath.Components.First();
            FakeVolume volume = owner.GetVolume(volumeRoot);
            return volume != null;
        }

        [AssertionMethod]
        private static void AssertFileIsNotReadOnly([NotNull] FileEntry fileEntry)
        {
            if (fileEntry.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.System.FileIsReadOnly();
            }
        }

        private static void AssertHasExclusiveAccess([NotNull] FileEntry file, [NotNull] AbsolutePath absolutePath)
        {
            if (file.IsOpen())
            {
                throw ErrorFactory.System.FileIsInUse(absolutePath.GetText());
            }
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
