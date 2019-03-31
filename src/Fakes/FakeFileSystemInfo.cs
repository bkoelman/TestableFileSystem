using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Handlers;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    public abstract class FakeFileSystemInfo : IFileSystemInfo
    {
        [NotNull]
        private readonly VolumeContainer container;

        [CanBeNull]
        private EntryProperties propertiesSnapshot;

        [NotNull]
        internal FakeFileSystem Owner { get; }

        [NotNull]
        internal AbsolutePath AbsolutePath { get; private set; }

        [NotNull]
        protected string DisplayPath { get; private set; }

        [NotNull]
        internal EntryProperties Properties => propertiesSnapshot ?? (propertiesSnapshot = RetrieveEntryProperties());

        public abstract string Name { get; }

        public string Extension { get; private set; }

        public string FullName { get; private set; }

        public FileAttributes Attributes
        {
            get
            {
                Properties.AssertNoError();
                return Properties.Attributes;
            }
            set
            {
                Owner.File.SetAttributes(FullName, value);
                Invalidate();
            }
        }

        public DateTime CreationTime
        {
            get => CreationTimeUtc.ToLocalTime();
            set => CreationTimeUtc = value.ToUniversalTime();
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                Properties.AssertNoError();
                return Properties.CreationTimeUtc;
            }
            set
            {
                SetTimeUtc(FileTimeKind.CreationTime, value);
                Invalidate();
            }
        }

        public DateTime LastAccessTime
        {
            get => LastAccessTimeUtc.ToLocalTime();
            set => LastAccessTimeUtc = value.ToUniversalTime();
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                Properties.AssertNoError();
                return Properties.LastAccessTimeUtc;
            }
            set
            {
                SetTimeUtc(FileTimeKind.LastAccessTime, value);
                Invalidate();
            }
        }

        public DateTime LastWriteTime
        {
            get => LastWriteTimeUtc.ToLocalTime();
            set => LastWriteTimeUtc = value.ToUniversalTime();
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                Properties.AssertNoError();
                return Properties.LastWriteTimeUtc;
            }
            set
            {
                SetTimeUtc(FileTimeKind.LastWriteTime, value);
                Invalidate();
            }
        }

        public abstract bool Exists { get; }

        internal FakeFileSystemInfo([NotNull] VolumeContainer container, [NotNull] FakeFileSystem owner,
            [NotNull] AbsolutePath path, [CanBeNull] string displayPath)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNull(owner, nameof(owner));
            Guard.NotNull(path, nameof(path));

            this.container = container;
            Owner = owner;
            AbsolutePath = path;

            FullName = path.GetText();
            DisplayPath = displayPath ?? FullName;
            Extension = Path.GetExtension(FullName);
        }

        public void Refresh()
        {
            propertiesSnapshot = RetrieveEntryProperties();
        }

        [NotNull]
        private EntryProperties RetrieveEntryProperties()
        {
            var handler = new EntryGetPropertiesHandler(container);
            var arguments = new EntryGetPropertiesArguments(AbsolutePath);

            return container.FileSystemLock.ExecuteInLock(() => handler.Handle(arguments));
        }

        private void Invalidate()
        {
            propertiesSnapshot = null;
        }

        public abstract void Delete();

        internal void ChangePath([NotNull] AbsolutePath path, [NotNull] string displayPath)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(displayPath, nameof(displayPath));

            AbsolutePath = path;
            DisplayPath = displayPath;
            FullName = path.GetText();
            Extension = Path.GetExtension(FullName);

            Invalidate();
        }

        internal abstract void SetTimeUtc(FileTimeKind kind, DateTime value);

        public override string ToString()
        {
            return DisplayPath;
        }
    }
}
