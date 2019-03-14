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
        [NotNull]
        private readonly FakeFileSystemChangeTracker changeTracker;

        public FileCryptoHandler([NotNull] VolumeContainer container, [NotNull] FakeFileSystemChangeTracker changeTracker)
            : base(container)
        {
            Guard.NotNull(changeTracker, nameof(changeTracker));
            this.changeTracker = changeTracker;
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
                bool hasEncrypted = entry.SetEncrypted();

                NotifyEncrypted(entry, hasEncrypted);
            }
            else
            {
                bool hasDecrypted = entry.ClearEncrypted();

                NotifyDecrypted(entry, hasDecrypted);
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

        private void NotifyEncrypted([NotNull] BaseEntry entry, bool hasEncrypted)
        {
            if (entry is FileEntry file)
            {
                NotifyFileEncrypted(file, hasEncrypted);
            }
            else if (entry is DirectoryEntry directory)
            {
                NotifyDirectoryEncrypted(directory, hasEncrypted);
            }
        }

        private void NotifyFileEncrypted([NotNull] FileEntry file, bool hasEncrypted)
        {
            changeTracker.NotifyContentsAccessed(file.Parent.PathFormatter, FileAccessKinds.Write | FileAccessKinds.Read);

            if (hasEncrypted)
            {
                AbsolutePath tempFilePath = GetTempFilePath(file);

                changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Write);
                changeTracker.NotifyFileCreated(tempFilePath.Formatter);
                changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Attributes);
                changeTracker.NotifyContentsAccessed(tempFilePath.Formatter, FileAccessKinds.Resize | FileAccessKinds.Write);
                changeTracker.NotifyContentsAccessed(tempFilePath.Formatter, FileAccessKinds.Write | FileAccessKinds.Read);
                changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Resize);
                changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Resize);
                changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Read);
                changeTracker.NotifyFileDeleted(tempFilePath.Formatter);
            }
        }

        private void NotifyDirectoryEncrypted([NotNull] DirectoryEntry directory, bool hasEncrypted)
        {
            if (!hasEncrypted)
            {
                return;
            }

            changeTracker.NotifyContentsAccessed(directory.PathFormatter, FileAccessKinds.Write | FileAccessKinds.Read);

            if (!directory.IsEmpty && directory.Parent != null)
            {
                changeTracker.NotifyContentsAccessed(directory.Parent.PathFormatter,
                    FileAccessKinds.Write | FileAccessKinds.Read);
            }

            changeTracker.NotifyContentsAccessed(directory.PathFormatter, FileAccessKinds.Attributes);

            if (!directory.IsEmpty)
            {
                changeTracker.NotifyContentsAccessed(directory.PathFormatter, FileAccessKinds.Write);
            }

            changeTracker.NotifyContentsAccessed(directory.PathFormatter, FileAccessKinds.Attributes);
        }

        private void NotifyDecrypted([NotNull] BaseEntry entry, bool hasDecrypted)
        {
            if (entry is FileEntry file)
            {
                NotifyFileDecrypted(hasDecrypted, file);
            }
            else if (entry is DirectoryEntry directory)
            {
                NotifyDirectoryDecrypted(directory);
            }
        }

        private void NotifyFileDecrypted(bool hasDecrypted, [NotNull] FileEntry file)
        {
            if (!hasDecrypted)
            {
                return;
            }

            AbsolutePath tempFilePath = GetTempFilePath(file);

            changeTracker.NotifyContentsAccessed(file.Parent.PathFormatter, FileAccessKinds.Write | FileAccessKinds.Read);
            changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Write);
            changeTracker.NotifyFileCreated(tempFilePath.Formatter);
            changeTracker.NotifyContentsAccessed(tempFilePath.Formatter, FileAccessKinds.Resize | FileAccessKinds.Write);
            changeTracker.NotifyContentsAccessed(tempFilePath.Formatter, FileAccessKinds.Write | FileAccessKinds.Read);
            changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Resize);
            changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Resize);
            changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Read);
            changeTracker.NotifyContentsAccessed(file.PathFormatter, FileAccessKinds.Attributes);
            changeTracker.NotifyFileDeleted(tempFilePath.Formatter);
        }

        private void NotifyDirectoryDecrypted([NotNull] DirectoryEntry directory)
        {
            changeTracker.NotifyContentsAccessed(directory.PathFormatter, FileAccessKinds.Attributes);
            changeTracker.NotifyContentsAccessed(directory.PathFormatter, FileAccessKinds.Write);
        }

        [NotNull]
        private static AbsolutePath GetTempFilePath([NotNull] FileEntry file)
        {
            AbsolutePath directoryPath = file.Parent.PathFormatter.GetPath();
            return directoryPath.Append("EFS0.TMP");
        }
    }
}
