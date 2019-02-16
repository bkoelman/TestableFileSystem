using System;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakePath : IPath
    {
        [NotNull]
        private readonly FakeFileSystem owner;

        internal FakePath([NotNull] FakeFileSystem owner)
        {
            Guard.NotNull(owner, nameof(owner));
            this.owner = owner;
        }

        public string GetTempPath()
        {
            return owner.TempDirectory;
        }

        public string GetTempFileName()
        {
            // TODO: Implement fake for System.IO.Path.GetTempFileName
            throw new NotImplementedException();
        }
    }
}
