using System;

namespace TestableFileSystem.Fakes
{
    [Flags]
    internal enum FileAccessKinds
    {
        None = 0,
        Attributes = 1,
        Resize = 2,
        Write = 4,
        Read = 8,
        Create = 16,
        Security = 32,

        WriteRead = Write | Read
    }
}
