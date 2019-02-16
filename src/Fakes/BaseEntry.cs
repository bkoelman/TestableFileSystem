using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

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

        // TODO: Move identical implementations into base class.
        public abstract DateTime CreationTime { get; set; }
        public abstract DateTime CreationTimeUtc { get; set; }
        public abstract DateTime LastAccessTime { get; set; }
        public abstract DateTime LastAccessTimeUtc { get; set; }
        public abstract DateTime LastWriteTime { get; set; }
        public abstract DateTime LastWriteTimeUtc { get; set; }

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

        protected abstract FileAttributes FilterAttributes(FileAttributes attributes);
    }
}
