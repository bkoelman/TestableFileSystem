using System;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    public sealed class FakePath : IPath
    {
        internal FakePath()
        {
        }

        public string GetTempPath()
        {
            // TODO: Implement fake for System.IO.Path.GetTempPath
            throw new NotImplementedException();
        }

        public string GetTempFileName()
        {
            // TODO: Implement fake for System.IO.Path.GetTempFileName
            throw new NotImplementedException();
        }
    }
}
