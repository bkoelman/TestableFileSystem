using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class VolumeContainer
    {
        [NotNull]
        internal readonly FileSystemLock FileSystemLock = new FileSystemLock();

        [NotNull]
        public SystemClock SystemClock { get; }

        [NotNull]
        private readonly FakeFileSystemChangeTracker changeTracker;

        [NotNull]
        private readonly ILoggedOnUserAccount loggedOnAccount;

        [NotNull]
        private readonly IDictionary<string, VolumeEntry> volumes =
            new Dictionary<string, VolumeEntry>(StringComparer.OrdinalIgnoreCase);

        public VolumeContainer([NotNull] SystemClock systemClock, [NotNull] FakeFileSystemChangeTracker changeTracker,
            [NotNull] ILoggedOnUserAccount loggedOnAccount)
        {
            Guard.NotNull(systemClock, nameof(systemClock));
            Guard.NotNull(changeTracker, nameof(changeTracker));
            Guard.NotNull(loggedOnAccount, nameof(loggedOnAccount));

            SystemClock = systemClock;
            this.changeTracker = changeTracker;
            this.loggedOnAccount = loggedOnAccount;
        }

        public bool ContainsVolume([NotNull] string volumeName)
        {
            Guard.NotNullNorWhiteSpace(volumeName, nameof(volumeName));

            return volumes.ContainsKey(volumeName);
        }

        [NotNull]
        public VolumeEntry GetVolume([NotNull] string name)
        {
            Guard.NotNullNorWhiteSpace(name, nameof(name));
            AssertVolumeExists(name);

            return volumes[name];
        }

        [AssertionMethod]
        private void AssertVolumeExists([NotNull] string name)
        {
            if (!ContainsVolume(name))
            {
                throw ErrorFactory.Internal.UnknownError($"Expected to find an existing volume named '{name}'.");
            }
        }

        public void CreateVolume([NotNull] string name, [NotNull] FakeVolumeInfo volumeInfo)
        {
            Guard.NotNullNorWhiteSpace(name, nameof(name));
            Guard.NotNull(volumeInfo, nameof(volumeInfo));
            AssertVolumeDoesNotExist(name);

            var volumeEntry = new VolumeEntry(name, volumeInfo.CapacityInBytes, volumeInfo.FreeSpaceInBytes, volumeInfo.Type,
                volumeInfo.Format, volumeInfo.Label, FileSystemLock, SystemClock, changeTracker, loggedOnAccount);
            volumes[name] = volumeEntry;
        }

        [AssertionMethod]
        private void AssertVolumeDoesNotExist([NotNull] string name)
        {
            if (ContainsVolume(name))
            {
                throw ErrorFactory.Internal.UnknownError($"Expected not to find an existing volume named '{name}'.");
            }
        }

        [NotNull]
        [ItemNotNull]
        public ICollection<VolumeEntry> FilterDrives()
        {
            return volumes.Values.Where(x => AbsolutePath.IsDriveLetter(x.Name)).OrderBy(x => x.Name).ToArray();
        }
    }
}
