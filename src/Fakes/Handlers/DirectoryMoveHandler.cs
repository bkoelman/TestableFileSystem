using System;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryMoveHandler : FakeOperationHandler<DirectoryOrFileMoveArguments, object>
    {
        public DirectoryMoveHandler([NotNull] DirectoryEntry root)
            : base(root)
        {
        }

        public override object Handle(DirectoryOrFileMoveArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            throw new NotImplementedException();
        }
    }
}
