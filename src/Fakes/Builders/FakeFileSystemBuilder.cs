using System;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Handlers;
using TestableFileSystem.Interfaces;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Builders
{
    public sealed class FakeFileSystemBuilder : ITestDataBuilder<FakeFileSystem>
    {
        // TODO: Consider a switch on Builder that drives NtfsDisableLastAccessUpdate registry setting.

        private static readonly DateTime DefaultTestTime = new DateTime(1900, 1, 2, 3, 44, 55);

        [NotNull]
        private static readonly SystemClock DefaultTestClock = new SystemClock(() => DefaultTestTime);

        [NotNull]
        private static readonly FakeLoggedOnUserAccount DefaultUserAccount = new FakeLoggedOnUserAccount("DefaultUser");

        [NotNull]
        private static readonly FakeVolumeInfo DefaultVolumeInfo = new FakeVolumeInfoBuilder().Build();

        private bool includeDriveC = true;

        [NotNull]
        private readonly VolumeContainer container;

        [NotNull]
        private readonly FakeFileSystemChangeTracker changeTracker = new FakeFileSystemChangeTracker();

        [CanBeNull]
        private string tempDirectory;

        [NotNull]
        private WaitIndicator copyWaitIndicator = WaitIndicator.None;

        public FakeFileSystemBuilder()
            : this(DefaultTestClock, DefaultUserAccount)
        {
        }

        public FakeFileSystemBuilder([NotNull] SystemClock systemClock)
            : this(systemClock, DefaultUserAccount)
        {
        }

        public FakeFileSystemBuilder([NotNull] ILoggedOnUserAccount userAccount)
            : this(DefaultTestClock, userAccount)
        {
        }

        public FakeFileSystemBuilder([NotNull] SystemClock systemClock, [NotNull] ILoggedOnUserAccount userAccount)
        {
            Guard.NotNull(systemClock, nameof(systemClock));
            Guard.NotNull(userAccount, nameof(userAccount));

            container = new VolumeContainer(systemClock, changeTracker, userAccount);
        }

        public FakeFileSystem Build()
        {
            if (includeDriveC && !container.ContainsVolume("C:"))
            {
                IncludingVolume("C:", DefaultVolumeInfo);
            }

            string effectiveTempDirectory = CreateTempDirectory();

            copyWaitIndicator.Reset();
            return new FakeFileSystem(container, effectiveTempDirectory, copyWaitIndicator);
        }

        [NotNull]
        private string CreateTempDirectory()
        {
            string directory = tempDirectory ?? GetTempDirectoryOnFirstDrive()?.GetText();
            if (directory != null)
            {
                IncludingDirectory(directory);
            }

            return directory ?? string.Empty;
        }

        [CanBeNull]
        private AbsolutePath GetTempDirectoryOnFirstDrive()
        {
            return container.FilterDrives().Select(drive => drive.PathFormatter.GetPath().Append("Temp")).FirstOrDefault();
        }

        [NotNull]
        public FakeFileSystemBuilder WithoutDefaultDriveC()
        {
            includeDriveC = false;
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingVolume([NotNull] string name, [NotNull] FakeVolumeInfoBuilder builder)
        {
            Guard.NotNull(builder, nameof(builder));

            return IncludingVolume(name, builder.Build());
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingVolume([NotNull] string name, [NotNull] FakeVolumeInfo volumeInfo)
        {
            Guard.NotNullNorWhiteSpace(name, nameof(name));
            Guard.NotNull(volumeInfo, nameof(volumeInfo));

            var path = new AbsolutePath(name);
            AssertIsVolumeRoot(path);

            string volumeName = GetVolumeName(path);
            if (container.ContainsVolume(volumeName))
            {
                throw new InvalidOperationException($"Volume '{name}' has already been created.");
            }

            container.CreateVolume(volumeName, volumeInfo);

            return this;
        }

        [AssertionMethod]
        private static void AssertIsVolumeRoot([NotNull] AbsolutePath path)
        {
            if (!path.IsVolumeRoot)
            {
                throw ErrorFactory.System.PathFormatIsNotSupported();
            }
        }

        [NotNull]
        private static string GetVolumeName([NotNull] AbsolutePath path)
        {
            return path.IsOnLocalDrive ? path.VolumeName.ToUpperInvariant() : path.VolumeName;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingDirectory([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));

            var absolutePath = new AbsolutePath(path);
            DirectoryEntry directory = CreateDirectories(absolutePath);

            if (attributes != null)
            {
                directory.SetAttributes(attributes.Value);
            }

            return this;
        }

        [NotNull]
        private DirectoryEntry CreateDirectories([NotNull] AbsolutePath absolutePath)
        {
            CreateVolume(absolutePath);

            var arguments = new DirectoryCreateArguments(absolutePath);
            var handler = new DirectoryCreateHandler(container);

            return handler.Handle(arguments);
        }

        private void CreateVolume([NotNull] AbsolutePath absolutePath)
        {
            string volumeName = GetVolumeName(absolutePath);

            if (!container.ContainsVolume(volumeName))
            {
                container.CreateVolume(volumeName, DefaultVolumeInfo);
            }
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingEmptyFile([NotNull] string path, [CanBeNull] FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));

            IncludeFile(path, entry => { }, attributes);
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingTextFile([NotNull] string path, [NotNull] string contents,
            [CanBeNull] Encoding encoding = null, [CanBeNull] FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNullNorEmpty(contents, nameof(contents));

            IncludeFile(path, entry => WriteStringToFile(entry, contents, encoding), attributes);
            return this;
        }

        private static void WriteStringToFile([NotNull] IFileStream fileStream, [NotNull] string text,
            [CanBeNull] Encoding encoding)
        {
            Stream stream = fileStream.AsStream();
            using (StreamWriter writer = encoding == null ? new StreamWriter(stream) : new StreamWriter(stream, encoding))
            {
                writer.Write(text);
            }
        }

        [NotNull]
        public FakeFileSystemBuilder IncludingBinaryFile([NotNull] string path, [NotNull] byte[] contents,
            [CanBeNull] FileAttributes? attributes = null)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNullNorEmpty(contents, nameof(contents));

            IncludeFile(path, entry => WriteBytesToFile(entry, contents), attributes);
            return this;
        }

        private static void WriteBytesToFile([NotNull] IFileStream stream, [NotNull] byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }

        private void IncludeFile([NotNull] string path, [NotNull] Action<IFileStream> writeContentsToStream,
            [CanBeNull] FileAttributes? attributes)
        {
            var absolutePath = new AbsolutePath(path);

            DirectoryEntry directory = CreateParentDirectories(absolutePath);

            string fileName = absolutePath.Components.Last();
            AssertIsNotDirectory(fileName, directory, absolutePath);

            FileEntry file = directory.ContainsFile(fileName) ? directory.GetFile(fileName) : directory.CreateFile(fileName);

            using (IFileStream stream = file.Open(FileMode.Truncate, FileAccess.Write, absolutePath, true, false, false))
            {
                writeContentsToStream(stream);
            }

            if (attributes != null)
            {
                file.SetAttributes(attributes.Value);
            }
        }

        [NotNull]
        private DirectoryEntry CreateParentDirectories([NotNull] AbsolutePath absolutePath)
        {
            AbsolutePath parentPath = absolutePath.TryGetParentPath();
            if (parentPath == null)
            {
                throw ErrorFactory.System.DirectoryNotFound(absolutePath.GetText());
            }

            return CreateDirectories(parentPath);
        }

        [AssertionMethod]
        private static void AssertIsNotDirectory([NotNull] string fileName, [NotNull] DirectoryEntry directory,
            [NotNull] AbsolutePath absolutePath)
        {
            if (directory.ContainsDirectory(fileName))
            {
                throw ErrorFactory.System.CannotCreateBecauseFileOrDirectoryAlreadyExists(absolutePath.GetText());
            }
        }

        [NotNull]
        public FakeFileSystemBuilder WithTempDirectory([NotNull] string path)
        {
            Guard.NotNull(path, nameof(path));

            tempDirectory = path;
            return this;
        }

        [NotNull]
        public FakeFileSystemBuilder WithCopyWaitIndicator([NotNull] WaitIndicator waitIndicator)
        {
            Guard.NotNull(waitIndicator, nameof(waitIndicator));

            copyWaitIndicator = waitIndicator;
            return this;
        }
    }
}
