using System;

namespace TestableFileSystem.Fakes.Tests.TestAttributes
{
    [Flags]
    internal enum FileSystemRunConditions
    {
        None = 0,
        RequiresAdministrativeRights = 1
    }
}
