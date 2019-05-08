using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;
#if !NETSTANDARD1_3
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    internal sealed class FakeDriveInfo : IDriveInfo
    {
        [NotNull]
        private readonly VolumeContainer container;

        [NotNull]
        private readonly FakeFileSystem owner;

        private readonly char driveLetter;

        public string Name => driveLetter + Path.VolumeSeparatorChar.ToString() + Path.DirectorySeparatorChar;
        public bool IsReady => TryGetVolume() != null;

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
            get => GetVolume().Label;
            set => GetVolume().Label = value;
        }

        public IDirectoryInfo RootDirectory => owner.ConstructDirectoryInfo(Name);

        public FakeDriveInfo([NotNull] VolumeContainer container, [NotNull] FakeFileSystem owner, [NotNull] string driveName)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNull(owner, nameof(owner));
            Guard.NotNull(driveName, nameof(driveName));
            AssertDriveNameIsValid(driveName);

            this.container = container;
            this.owner = owner;
            driveLetter = driveName[0];
        }

        private static void AssertDriveNameIsValid([NotNull] string driveName)
        {
            if (driveName != string.Empty)
            {
                string volumeName = driveName.Length == 1 ? driveName + Path.VolumeSeparatorChar : driveName.Substring(0, 2);
                if (AbsolutePath.IsDriveLetter(volumeName))
                {
                    return;
                }
            }

            throw ErrorFactory.System.DriveNameMustBeRootOrLetter(nameof(driveName));
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
