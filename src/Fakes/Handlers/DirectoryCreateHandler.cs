using System.Linq;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryCreateHandler : FakeOperationHandler<DirectoryCreateArguments, DirectoryEntry>
    {
        public DirectoryCreateHandler([NotNull] VolumeContainer container)
            : base(container)
        {
        }

        public override DirectoryEntry Handle(DirectoryCreateArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));
            AssertVolumeExists(arguments.Path);

            DirectoryEntry directory = Container.GetVolume(arguments.Path.VolumeName);

            foreach (AbsolutePathComponent component in arguments.Path.EnumerateComponents().Skip(1))
            {
                AssertIsNotFile(component, directory);

                if (!directory.ContainsDirectory(component.Name))
                {
                    string name = GetDirectoryName(component);
                    directory = directory.CreateDirectory(name);
                }
                else
                {
                    directory = directory.GetDirectory(component.Name);
                }
            }

            return directory;
        }

        private void AssertVolumeExists([NotNull] AbsolutePath path)
        {
            if (!Container.ContainsVolume(path.VolumeName))
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
            if (directory.ContainsFile(component.Name))
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
