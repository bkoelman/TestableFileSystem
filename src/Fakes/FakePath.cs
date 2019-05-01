using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Handlers;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    internal sealed class FakePath : IPath
    {
        [NotNull]
        private readonly VolumeContainer container;

        [NotNull]
        private readonly FakeFileSystem owner;

        internal FakePath([NotNull] VolumeContainer container, [NotNull] FakeFileSystem owner)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNull(owner, nameof(owner));

            this.container = container;
            this.owner = owner;
        }

        public string GetFullPath(string path)
        {
            Guard.NotNull(path, nameof(path));

            AbsolutePath absolutePath = owner.ToAbsolutePath(path);
            return absolutePath.GetText();
        }

        public string GetTempPath()
        {
            return owner.TempDirectory;
        }

        public string GetTempFileName()
        {
            string tempDirectory = GetTempPath();
            AbsolutePath absoluteTempDirectory = owner.ToAbsolutePath(tempDirectory);

            var handler = new PathGetTempFileNameHandler(container, owner.RandomNumberGenerator);
            var arguments = new PathGetTempFileNameArguments(absoluteTempDirectory);

            AbsolutePath tempFilePath = handler.Handle(arguments);
            return tempFilePath.GetText();
        }
    }
}
