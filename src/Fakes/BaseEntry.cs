using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal abstract class BaseEntry
    {
        [NotNull]
        public string Name { get; protected set; }

        [NotNull]
        public ILoggedOnUserAccount LoggedOnAccount { get; }

        [CanBeNull]
        public string EncryptorAccountName { get; set; }

        [NotNull]
        public FakeFileSystemChangeTracker ChangeTracker { get; }

        [NotNull]
        internal abstract IPathFormatter PathFormatter { get; }

        public FileAttributes Attributes { get; private set; }

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

        protected abstract FileAttributes FilterAttributes(FileAttributes attributes);
    }
}
