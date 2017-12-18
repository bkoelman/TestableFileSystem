using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryDeleteHandler : FakeOperationHandler<DirectoryDeleteArguments, object>
    {
        [NotNull]
        private readonly CurrentDirectoryManager currentDirectoryManager;

        public DirectoryDeleteHandler([NotNull] DirectoryEntry root, [NotNull] FileSystemChangeTracker changeTracker,
            [NotNull] CurrentDirectoryManager currentDirectoryManager)
            : base(root, changeTracker)
        {
            Guard.NotNull(currentDirectoryManager, nameof(currentDirectoryManager));

            this.currentDirectoryManager = currentDirectoryManager;
        }

        public override object Handle(DirectoryDeleteArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            AssertNotDeletingVolumeRoot(arguments);

            DirectoryEntry directory = ResolveDirectory(arguments.Path);

            AssertNoConflictWithCurrentDirectory(directory, arguments.Path);
            AssertIsEmptyOrRecursive(directory, arguments.Recursive);
            AssertIsNotReadOnly(directory, arguments.Path);
            AssertDirectoryContainsNoOpenFiles(directory, arguments.Path);

            directory.Parent?.DeleteDirectory(directory.Name);

            return Missing.Value;
        }

        [AssertionMethod]
        private static void AssertNotDeletingVolumeRoot([NotNull] DirectoryDeleteArguments arguments)
        {
            if (arguments.Path.IsVolumeRoot)
            {
                if (!arguments.Path.IsOnLocalDrive)
                {
                    throw ErrorFactory.System.FileIsInUse(arguments.Path.GetText());
                }

                if (arguments.Recursive)
                {
                    throw ErrorFactory.System.FileNotFound(arguments.Path.GetText());
                }

                throw ErrorFactory.System.DirectoryIsNotEmpty();
            }
        }

        [AssertionMethod]
        private void AssertNoConflictWithCurrentDirectory([NotNull] DirectoryEntry directory, [NotNull] AbsolutePath path)
        {
            if (currentDirectoryManager.IsAtOrAboveCurrentDirectory(directory))
            {
                throw ErrorFactory.System.FileIsInUse(path.GetText());
            }
        }

        [NotNull]
        private DirectoryEntry ResolveDirectory([NotNull] AbsolutePath path)
        {
            var resolver = new DirectoryResolver(Root)
            {
                ErrorLastDirectoryFoundAsFile = incomingPath => ErrorFactory.System.DirectoryNameIsInvalid()
            };

            return resolver.ResolveDirectory(path);
        }

        [AssertionMethod]
        private static void AssertIsEmptyOrRecursive([NotNull] DirectoryEntry directory, bool recursive)
        {
            if (!recursive && !directory.IsEmpty)
            {
                throw ErrorFactory.System.DirectoryIsNotEmpty();
            }
        }

        [AssertionMethod]
        private static void AssertIsNotReadOnly([NotNull] DirectoryEntry directory, [NotNull] AbsolutePath directoryPath)
        {
            if (directory.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.System.AccessDenied(directoryPath.GetText());
            }

            foreach (FileEntry file in directory.Files.Values)
            {
                AssertFileIsNotReadOnly(file);
            }

            foreach (DirectoryEntry subdirectory in directory.Directories.Values)
            {
                AbsolutePath subdirectoryPath = directoryPath.Append(subdirectory.Name);
                AssertIsNotReadOnly(subdirectory, subdirectoryPath);
            }
        }

        [AssertionMethod]
        private static void AssertFileIsNotReadOnly([NotNull] FileEntry file)
        {
            if (file.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.System.UnauthorizedAccess(file.Name);
            }
        }

        [AssertionMethod]
        private static void AssertDirectoryContainsNoOpenFiles([NotNull] DirectoryEntry directory, [NotNull] AbsolutePath path)
        {
            AbsolutePath openFilePath = directory.TryGetPathOfFirstOpenFile(path);
            if (openFilePath != null)
            {
                throw ErrorFactory.System.FileIsInUse(openFilePath.GetText());
            }
        }
    }
}
