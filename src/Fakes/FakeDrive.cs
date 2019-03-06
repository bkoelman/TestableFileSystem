using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeDrive : IDrive
    {
        [NotNull]
        private readonly VolumeContainer container;

        [NotNull]
        private readonly FakeFileSystem owner;

        internal FakeDrive([NotNull] VolumeContainer container, [NotNull] FakeFileSystem owner)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNull(owner, nameof(owner));

            this.container = container;
            this.owner = owner;
        }

#if !NETSTANDARD1_3
        public IDriveInfo[] GetDrives()
        {
            var driveInfos = new List<IDriveInfo>();

            ICollection<VolumeEntry> drives = container.FilterDrives();
            foreach (string driveName in drives.Select(x => x.Name + Path.DirectorySeparatorChar))
            {
                IDriveInfo driveInfo = owner.ConstructDriveInfo(driveName);
                driveInfos.Add(driveInfo);
            }

            return driveInfos.ToArray();
        }
#endif
    }
}
