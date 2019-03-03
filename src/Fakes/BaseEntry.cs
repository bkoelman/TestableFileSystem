using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal abstract class BaseEntry
    {
        [CanBeNull]
        private string encryptorAccountName;

        private FileAttributes innerAttributes;

        [NotNull]
        public string Name { get; protected set; }

        [NotNull]
        public ILoggedOnUserAccount LoggedOnAccount { get; }

        public bool IsEncrypted => encryptorAccountName != null;

        public bool IsExternallyEncrypted => IsEncrypted && LoggedOnAccount.UserName != encryptorAccountName;

        [NotNull]
        public FakeFileSystemChangeTracker ChangeTracker { get; }

        [NotNull]
        internal abstract IPathFormatter PathFormatter { get; }

        public FileAttributes Attributes
        {
            get => encryptorAccountName != null ? innerAttributes | FileAttributes.Encrypted : innerAttributes;
            private set => innerAttributes = value & ~FileAttributes.Encrypted;
        }

        private long creationTimeStampUtc;
        private long lastWriteTimeStampUtc;
        private long lastAccessTimeStampUtc;

        public DateTime CreationTime
        {
            get => DateTime.FromFileTime(creationTimeStampUtc);
            set => creationTimeStampUtc = value.ToFileTime();
        }

        public DateTime CreationTimeUtc
        {
            get => DateTime.FromFileTimeUtc(creationTimeStampUtc);
            set => creationTimeStampUtc = value.ToFileTimeUtc();
        }

        public DateTime LastAccessTime
        {
            get => DateTime.FromFileTime(lastAccessTimeStampUtc);
            set => lastAccessTimeStampUtc = value.ToFileTime();
        }

        public DateTime LastAccessTimeUtc
        {
            get => DateTime.FromFileTimeUtc(lastAccessTimeStampUtc);
            set => lastAccessTimeStampUtc = value.ToFileTimeUtc();
        }

        public DateTime LastWriteTime
        {
            get => DateTime.FromFileTime(lastWriteTimeStampUtc);
            set => lastWriteTimeStampUtc = value.ToFileTime();
        }

        public DateTime LastWriteTimeUtc
        {
            get => DateTime.FromFileTimeUtc(lastWriteTimeStampUtc);
            set => lastWriteTimeStampUtc = value.ToFileTimeUtc();
        }

        protected BaseEntry([NotNull] string name, FileAttributes attributes, [NotNull] FakeFileSystemChangeTracker changeTracker,
            [NotNull] ILoggedOnUserAccount loggedOnAccount)
        {
            Guard.NotNullNorWhiteSpace(name, nameof(name));
            Guard.NotNull(changeTracker, nameof(changeTracker));
            Guard.NotNull(loggedOnAccount, nameof(loggedOnAccount));

            Name = name;
            Attributes = attributes;
            ChangeTracker = changeTracker;
            LoggedOnAccount = loggedOnAccount;
        }

        public void SetAttributes(FileAttributes newAttributes, FileAccessKinds accessKinds = FileAccessKinds.Attributes)
        {
            FileAttributes beforeAttributes = Attributes;

            Attributes = FilterAttributes(newAttributes);

            if (Attributes != beforeAttributes)
            {
                ChangeTracker.NotifyContentsAccessed(PathFormatter, accessKinds);
            }
        }

        public void SetEncrypted()
        {
            if (encryptorAccountName == null)
            {
                encryptorAccountName = LoggedOnAccount.UserName;
            }
        }

        public void ClearEncrypted()
        {
            encryptorAccountName = null;
        }

        protected void CopyPropertiesFrom([NotNull] BaseEntry otherEntry)
        {
            Guard.NotNull(otherEntry, nameof(otherEntry));

            encryptorAccountName = otherEntry.encryptorAccountName;
            innerAttributes = otherEntry.innerAttributes;

            creationTimeStampUtc = otherEntry.creationTimeStampUtc;
            lastWriteTimeStampUtc = otherEntry.lastWriteTimeStampUtc;
            lastAccessTimeStampUtc = otherEntry.lastAccessTimeStampUtc;
        }

        protected abstract FileAttributes FilterAttributes(FileAttributes attributes);
    }
}
