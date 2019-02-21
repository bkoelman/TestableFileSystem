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
        private readonly DirectoryEntry root;

        [NotNull]
        private readonly FakeFileSystem owner;

        internal FakeDrive([NotNull] DirectoryEntry root, [NotNull] FakeFileSystem owner)
        {
            Guard.NotNull(root, nameof(root));
            Guard.NotNull(owner, nameof(owner));

            this.root = root;
            this.owner = owner;
        }

#if !NETSTANDARD1_3
        public IDriveInfo[] GetDrives()
        {
            var driveInfos = new List<IDriveInfo>();

            ICollection<DirectoryEntry> drives = root.FilterDrives();
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
