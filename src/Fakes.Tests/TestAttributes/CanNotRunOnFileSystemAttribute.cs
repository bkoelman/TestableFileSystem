using System;

namespace TestableFileSystem.Fakes.Tests.TestAttributes
{
    internal sealed class CanNotRunOnFileSystemAttribute : Attribute
    {
        public FileSystemSkipReason Reason { get; }

        public CanNotRunOnFileSystemAttribute(FileSystemSkipReason reason)
        {
            Reason = reason;
        }
    }
}
