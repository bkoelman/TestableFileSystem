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
        private readonly VolumeContainer container;

        [NotNull]
        private readonly FakeFileSystem owner;

        [NotNull]
        private readonly Random randomNumberGenerator;

        internal FakePath([NotNull] VolumeContainer container, [NotNull] FakeFileSystem owner)
        {
            Guard.NotNull(container, nameof(container));
            Guard.NotNull(owner, nameof(owner));

            this.container = container;
            this.owner = owner;
            randomNumberGenerator = CreateRandomNumberGenerator(container.SystemClock);
        }

        [NotNull]
        private static Random CreateRandomNumberGenerator([NotNull] SystemClock systemClock)
        {
            int seed = (int)(systemClock.UtcNow().Ticks % int.MaxValue);
            return new Random(seed);
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

            var handler = new PathGetTempFileNameHandler(container, randomNumberGenerator);
            var arguments = new PathGetTempFileNameArguments(absoluteTempDirectory);

            AbsolutePath tempFilePath = handler.Handle(arguments);
            return tempFilePath.GetText();
        }
    }
}
