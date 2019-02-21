using System;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Handlers;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    public sealed class FakePath : IPath
    {
        [NotNull]
        private readonly DirectoryEntry root;

        [NotNull]
        private readonly FakeFileSystem owner;

        [NotNull]
        private readonly Random randomNumberGenerator;

        internal FakePath([NotNull] DirectoryEntry root, [NotNull] FakeFileSystem owner)
        {
            Guard.NotNull(root, nameof(root));
            Guard.NotNull(owner, nameof(owner));

            this.root = root;
            this.owner = owner;
            randomNumberGenerator = CreateRandomNumberGenerator(root.SystemClock);
        }

        [NotNull]
        private static Random CreateRandomNumberGenerator([NotNull] SystemClock systemClock)
        {
            int seed = (int)(systemClock.UtcNow().Ticks % int.MaxValue);
            return new Random(seed);
        }

        public string GetTempPath()
        {
            return owner.TempDirectory;
        }

        public string GetTempFileName()
        {
            string tempDirectory = GetTempPath();
            AbsolutePath absoluteTempDirectory = owner.ToAbsolutePath(tempDirectory);

            var handler = new PathGetTempFileNameHandler(root, randomNumberGenerator);
            var arguments = new PathGetTempFileNameArguments(absoluteTempDirectory);

            var tempFilePath = handler.Handle(arguments);
            return tempFilePath.GetText();
        }
    }
}
