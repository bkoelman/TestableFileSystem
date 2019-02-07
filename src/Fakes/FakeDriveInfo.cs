#if !NETSTANDARD1_3
using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    [Serializable]
    internal sealed class FakeDriveInfo : IDriveInfo
    {
        private const long OneGigabyte = 1024 * 1024 * 1024;

        [NotNull]
        private readonly FakeFileSystem owner;

        private readonly char driveLetter;

        public string Name => driveLetter + Path.VolumeSeparatorChar.ToString() + Path.DirectorySeparatorChar;
        public bool IsReady => DriveExists;

        private bool DriveExists => owner.Directory.Exists(Name);

        // TODO: Update free space during various file system operations. Throw on insufficient space.
        public long AvailableFreeSpace
        {
            get
            {
                AssertDriveExists();
                return TotalFreeSpace;
            }
        }

        public long TotalFreeSpace
        {
            get
            {
                AssertDriveExists();
                return OneGigabyte;
            }
        }

        public long TotalSize
        {
            get
            {
                AssertDriveExists();
                return OneGigabyte;
            }
        }

        public DriveType DriveType => DriveExists ? DriveType.Fixed : DriveType.NoRootDirectory;

        public string DriveFormat
        {
            get
            {
                AssertDriveExists();
                return "NTFS";
            }
        }

        public string VolumeLabel
        {
            get
            {
                AssertDriveExists();
                return string.Empty;
            }
            set
            {
            }
        }

        public IDirectoryInfo RootDirectory => owner.ConstructDirectoryInfo(Name);

        public FakeDriveInfo([NotNull] FakeFileSystem owner, [NotNull] string driveName)
        {
            Guard.NotNull(owner, nameof(owner));
            Guard.NotNull(driveName, nameof(driveName));
            AssertDriveNameIsNotEmpty(driveName);
            AssertDriveNameIsValid(driveName);

            this.owner = owner;
            driveLetter = driveName[0];
        }

        private static void AssertDriveNameIsNotEmpty([NotNull] string driveName)
        {
            if (driveName == string.Empty)
            {
                throw ErrorFactory.System.PathIsNotLegal(nameof(driveName));
            }
        }

        private static void AssertDriveNameIsValid([NotNull] string driveName)
        {
            string volumeName = driveName.Length == 1 ? driveName + Path.VolumeSeparatorChar : driveName.Substring(0, 2);
            if (!AbsolutePath.IsDriveLetter(volumeName))
            {
                throw ErrorFactory.System.DriveNameMustBeRootOrLetter();
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private void AssertDriveExists()
        {
            if (!DriveExists)
            {
                throw ErrorFactory.System.CouldNotFindDrive(Name);
            }
        }
    }
}
#endif
