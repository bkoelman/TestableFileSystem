using System.Collections.Generic;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakeDrive : IDrive
    {
        [NotNull]
        private readonly FakeFileSystem owner;

        internal FakeDrive([NotNull] FakeFileSystem owner)
        {
            Guard.NotNull(owner, nameof(owner));
            this.owner = owner;
        }

#if !NETSTANDARD1_3
        public IDriveInfo[] GetDrives()
        {
            var driveInfos = new List<IDriveInfo>();

            foreach (string driveName in owner.GetDrives())
            {
                IDriveInfo driveInfo = owner.ConstructDriveInfo(driveName);
                driveInfos.Add(driveInfo);
            }

            return driveInfos.ToArray();
        }
#endif
    }
}
