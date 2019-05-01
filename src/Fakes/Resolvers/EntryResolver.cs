using System;
using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Resolvers
{
    internal sealed class EntryResolver
    {
        [NotNull]
        private readonly VolumeContainer container;

        [NotNull]
        private readonly DirectoryResolver directoryResolver;

        [NotNull]
        public Func<string, Exception> ErrorNetworkShareNotFound
        {
            get => directoryResolver.ErrorNetworkShareNotFound;
            set => directoryResolver.ErrorNetworkShareNotFound = value;
        }

        [NotNull]
        public Func<string, Exception> ErrorDirectoryFoundAsFile
        {
            get => directoryResolver.ErrorDirectoryFoundAsFile;
            set => directoryResolver.ErrorDirectoryFoundAsFile = value;
        }

        [NotNull]
        public Func<string, Exception> ErrorLastDirectoryFoundAsFile
        {
            get => directoryResolver.ErrorLastDirectoryFoundAsFile;
            set => directoryResolver.ErrorLastDirectoryFoundAsFile = value;
        }

        [NotNull]
        public Func<string, Exception> ErrorDirectoryNotFound
        {
            get => directoryResolver.ErrorDirectoryNotFound;
            set => directoryResolver.ErrorDirectoryNotFound = value;
        }

        public EntryResolver([NotNull] VolumeContainer container)
        {
            Guard.NotNull(container, nameof(container));

            this.container = container;
            directoryResolver = new DirectoryResolver(container);
        }

        [CanBeNull]
        public BaseEntry SafeResolveEntry([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            if (path.IsVolumeRoot)
            {
                return SafeResolveVolume(path);
            }

            DirectoryEntry directory = SafeResolveContainingDirectory(path);
            if (directory != null)
            {
                string entryName = path.Components.Last();

                if (directory.ContainsFile(entryName))
                {
                    return directory.GetFile(entryName);
                }

                if (directory.ContainsDirectory(entryName))
                {
                    return directory.GetDirectory(entryName);
                }
            }

            return null;
        }

        [NotNull]
        public BaseEntry ResolveEntry([NotNull] AbsolutePath path)
        {
            Guard.NotNull(path, nameof(path));

            if (path.IsVolumeRoot)
            {
                return container.GetVolume(path.VolumeName);
            }

            DirectoryEntry directory = ResolveContainingDirectory(path);
            string entryName = path.Components.Last();

            if (directory.ContainsFile(entryName))
            {
                return directory.GetFile(entryName);
            }

            if (directory.ContainsDirectory(entryName))
            {
                return directory.GetDirectory(entryName);
            }

            throw ErrorFactory.System.FileNotFound(path.GetText());
        }

        [CanBeNull]
        private VolumeEntry SafeResolveVolume([NotNull] AbsolutePath path)
        {
            return container.ContainsVolume(path.VolumeName) ? container.GetVolume(path.VolumeName) : null;
        }

        [CanBeNull]
        private DirectoryEntry SafeResolveContainingDirectory([NotNull] AbsolutePath path)
        {
            AbsolutePath parentPath = path.TryGetParentPath();

            // ReSharper disable once AssignNullToNotNullAttribute
            return directoryResolver.SafeResolveDirectory(parentPath, path.GetText());
        }

        [NotNull]
        private DirectoryEntry ResolveContainingDirectory([NotNull] AbsolutePath path)
        {
            AbsolutePath parentPath = path.TryGetParentPath();

            // ReSharper disable once AssignNullToNotNullAttribute
            return directoryResolver.ResolveDirectory(parentPath, path.GetText());
        }
    }
}
