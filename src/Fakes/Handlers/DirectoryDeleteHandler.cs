using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Handlers.Arguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryDeleteHandler : FakeOperationHandler<DirectoryDeleteArguments, object>
    {
        [NotNull]
        private readonly CurrentDirectoryManager currentDirectoryManager;

        public DirectoryDeleteHandler([NotNull] DirectoryEntry root, [NotNull] CurrentDirectoryManager currentDirectoryManager)
            : base(root)
        {
            Guard.NotNull(currentDirectoryManager, nameof(currentDirectoryManager));

            this.currentDirectoryManager = currentDirectoryManager;
        }

        public override object Handle(DirectoryDeleteArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            AssertNotDeletingDriveOrNetworkShare(arguments);

            DirectoryEntry directory = ResolveDirectory(arguments.Path);

            AssertNoConflictWithCurrentDirectory(directory);
            AssertIsEmptyOrRecursive(directory, arguments.Recursive);
            AssertIsNotReadOnly(directory);
            AssertDirectoryContainsNoOpenFiles(directory);

            directory.Parent?.DeleteDirectory(directory.Name);

            return Missing.Value;
        }

        [AssertionMethod]
        private static void AssertNotDeletingDriveOrNetworkShare([NotNull] DirectoryDeleteArguments arguments)
        {
            if (arguments.Path.IsVolumeRoot)
            {
                if (!arguments.Path.IsOnLocalDrive)
                {
                    throw ErrorFactory.FileIsInUse(arguments.Path.GetText());
                }

                if (arguments.Recursive)
                {
                    throw ErrorFactory.FileNotFound(arguments.Path.GetText());
                }

                throw ErrorFactory.DirectoryIsNotEmpty();
            }
        }

        [AssertionMethod]
        private void AssertNoConflictWithCurrentDirectory([NotNull] DirectoryEntry directory)
        {
            if (currentDirectoryManager.IsAtOrAboveCurrentDirectory(directory))
            {
                throw ErrorFactory.FileIsInUse(directory.GetAbsolutePath());
            }
        }

        [NotNull]
        private DirectoryEntry ResolveDirectory([NotNull] AbsolutePath path)
        {
            var resolver = new DirectoryResolver(Root)
            {
                ErrorLastDirectoryFoundAsFile = incomingPath => ErrorFactory.DirectoryNameIsInvalid()
            };

            return resolver.ResolveDirectory(path, path.GetText());
        }

        [AssertionMethod]
        private static void AssertIsEmptyOrRecursive([NotNull] DirectoryEntry directory, bool recursive)
        {
            if (!recursive && !directory.IsEmpty)
            {
                throw ErrorFactory.DirectoryIsNotEmpty();
            }
        }

        [AssertionMethod]
        private static void AssertIsNotReadOnly([NotNull] DirectoryEntry directory)
        {
            if (directory.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.AccessDenied(directory.GetAbsolutePath());
            }

            foreach (FileEntry file in directory.Files.Values)
            {
                AssertFileIsNotReadOnly(file);
            }

            foreach (DirectoryEntry subdirectory in directory.Directories.Values)
            {
                AssertIsNotReadOnly(subdirectory);
            }
        }

        [AssertionMethod]
        private static void AssertFileIsNotReadOnly([NotNull] FileEntry file)
        {
            if (file.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                throw ErrorFactory.UnauthorizedAccess(file.Name);
            }
        }

        [AssertionMethod]
        private static void AssertDirectoryContainsNoOpenFiles([NotNull] DirectoryEntry directory)
        {
            FileEntry openFile = TryGetFirstOpenFile(directory);
            if (openFile != null)
            {
                throw ErrorFactory.FileIsInUse(openFile.GetAbsolutePath());
            }
        }

        [CanBeNull]
        private static FileEntry TryGetFirstOpenFile([NotNull] DirectoryEntry directory)
        {
            FileEntry file = directory.Files.Values.FirstOrDefault(x => x.IsOpen());

            if (file == null)
            {
                foreach (DirectoryEntry subdirectory in directory.Directories.Values)
                {
                    file = TryGetFirstOpenFile(subdirectory);
                    if (file != null)
                    {
                        break;
                    }
                }
            }

            return file;
        }
    }
}
