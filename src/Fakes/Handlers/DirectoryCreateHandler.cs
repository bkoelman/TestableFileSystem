using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryCreateHandler : FakeOperationHandler<DirectoryCreateArguments, DirectoryEntry>
    {
        public DirectoryCreateHandler([NotNull] DirectoryEntry root, [NotNull] FileSystemChangeTracker changeTracker)
            : base(root, changeTracker)
        {
        }

        public override DirectoryEntry Handle(DirectoryCreateArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            if (!arguments.CanCreateVolumeRoot)
            {
                AssertVolumeRootExists(arguments.Path);
            }

            DirectoryEntry directory = Root;

            foreach (AbsolutePathComponent component in arguments.Path.EnumerateComponents())
            {
                AssertIsNotFile(component, directory);

                if (!directory.Directories.ContainsKey(component.Name))
                {
                    string name = GetDirectoryName(component);
                    directory = directory.CreateDirectory(name);
                }
                else
                {
                    directory = directory.Directories[component.Name];
                }
            }

            return directory;
        }

        [AssertionMethod]
        private void AssertVolumeRootExists([NotNull] AbsolutePath path)
        {
            if (!Root.Directories.ContainsKey(path.Components.First()))
            {
                if (!path.IsOnLocalDrive && !path.IsVolumeRoot)
                {
                    throw ErrorFactory.System.NetworkPathNotFound();
                }

                throw ErrorFactory.System.DirectoryNotFound(path.GetText());
            }
        }

        [AssertionMethod]
        private static void AssertIsNotFile([NotNull] AbsolutePathComponent component, [NotNull] DirectoryEntry directory)
        {
            if (directory.Files.ContainsKey(component.Name))
            {
                AbsolutePath pathUpToHere = component.GetPathUpToHere();
                throw ErrorFactory.System.CannotCreateBecauseFileOrDirectoryAlreadyExists(pathUpToHere.GetText());
            }
        }

        [NotNull]
        private static string GetDirectoryName([NotNull] AbsolutePathComponent component)
        {
            return component.IsAtStart && component.Path.IsOnLocalDrive ? component.Name.ToUpperInvariant() : component.Name;
        }
    }
}
