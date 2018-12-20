using System;

namespace TestableFileSystem.Fakes
{
    [Flags]
    internal enum FileAccessKinds
    {
        None = 0,
        Read = 1,
        Write = 2,
        Resize = 4
    }
}
