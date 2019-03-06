using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class FileCryptoHandler : FakeOperationHandler<FileCryptoArguments, Missing>
    {
        public FileCryptoHandler([NotNull] VolumeContainer container)
            : base(container)
        {
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
                if (Container.ContainsVolume(arguments.Path.VolumeName))
                {
                    VolumeEntry volume = Container.GetVolume(arguments.Path.VolumeName);
                    if (volume.Format != FakeVolumeInfo.NtFs)
                    {
                        throw new NotSupportedException("File encryption support only works on NTFS partitions.");
                    }
                }
            }
        }

        [NotNull]
        private BaseEntry ResolveEntry([NotNull] FileCryptoArguments arguments)
        {
            EntryResolver resolver = arguments.IsEncrypt
                ? new EntryResolver(Container)
                {
                    ErrorNetworkShareNotFound = _ => ErrorFactory.System.FileOrDirectoryOrVolumeIsIncorrect(),
                    ErrorDirectoryNotFound = path => IsPathOnExistingVolume(path)
                        ? ErrorFactory.System.DirectoryNotFound(path)
                        : ErrorFactory.System.FileNotFound(path)
                }
                : new EntryResolver(Container);

            return resolver.ResolveEntry(arguments.Path);
        }

        private bool IsPathOnExistingVolume([NotNull] string path)
        {
            var absolutePath = new AbsolutePath(path);
            return Container.ContainsVolume(absolutePath.VolumeName);
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
