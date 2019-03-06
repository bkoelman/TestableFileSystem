#if !NETSTANDARD1_3
using System;
using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    [Serializable]
    internal sealed class FakeDriveInfo : IDriveInfo
    {
        [NotNull]
        private readonly VolumeContainer container;

        [NotNull]
        private readonly FakeFileSystem owner;

        private readonly char driveLetter;

        public string Name => driveLetter + Path.VolumeSeparatorChar.ToString() + Path.DirectorySeparatorChar;
        public bool IsReady => TryGetVolume() != null;

        // TODO: Update free space during various file system operations. Throw on insufficient space.

        public long AvailableFreeSpace => GetVolume().FreeSpaceInBytes;
        public long TotalFreeSpace => GetVolume().FreeSpaceInBytes;
        public long TotalSize => GetVolume().CapacityInBytes;

        public DriveType DriveType
        {
            get
            {
                VolumeEntry volume = TryGetVolume();
                return volume?.Type ?? DriveType.NoRootDirectory;
            }
        }

        public string DriveFormat => GetVolume().Format;

        public string VolumeLabel
        {
            get
            {
                lock (owner.TreeLock)
                {
                    return GetVolume().Label;
                }
            }
            set
            {
                lock (owner.TreeLock)
                {
                    VolumeEntry volume = GetVolume();
                    volume.Label = value;
                }
            }
        }

        public IDirectoryInfo RootDirectory => owner.ConstructDirectoryInfo(Name);

        public FakeDriveInfo([NotNull] VolumeContainer container, [NotNull] FakeFileSystem owner, [NotNull] string driveName)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNull(owner, nameof(owner));
            Guard.NotNull(driveName, nameof(driveName));
            AssertDriveNameIsNotEmpty(driveName);
            AssertDriveNameIsValid(driveName);

            this.container = container;
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

        [CanBeNull]
        private VolumeEntry TryGetVolume()
        {
            string driveName = driveLetter + Path.VolumeSeparatorChar.ToString();
            return container.ContainsVolume(driveName) ? container.GetVolume(driveName) : null;
        }

        [NotNull]
        private VolumeEntry GetVolume()
        {
            VolumeEntry volume = TryGetVolume();

            if (volume == null)
            {
                throw ErrorFactory.System.CouldNotFindDrive(Name);
            }

            return volume;
        }
    }
}
#endif
