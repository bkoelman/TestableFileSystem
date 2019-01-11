using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.HandlerArguments;
using TestableFileSystem.Fakes.Resolvers;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers
{
    internal sealed class DirectoryDeleteHandler : FakeOperationHandler<DirectoryDeleteArguments, Missing>
    {
        [NotNull]
        private readonly CurrentDirectoryManager currentDirectoryManager;

        public DirectoryDeleteHandler([NotNull] DirectoryEntry root, [NotNull] CurrentDirectoryManager currentDirectoryManager)
            : base(root)
        {
            Guard.NotNull(currentDirectoryManager, nameof(currentDirectoryManager));

            this.currentDirectoryManager = currentDirectoryManager;
        }

        public override Missing Handle(DirectoryDeleteArguments arguments)
        {
            Guard.NotNull(arguments, nameof(arguments));

            AssertNotDeletingVolumeRoot(arguments);

            DirectoryEntry directory = ResolveDirectory(arguments.Path);

            // TODO: Should we check al these upfront and bail out, or just fail while partially completed?
            AssertNoConflictWithCurrentDirectory(directory, arguments.Path);
            AssertIsEmptyOrRecursive(directory, arguments.Recursive);
            AssertIsNotReadOnly(directory, arguments.Path);
            AssertDirectoryContainsNoOpenFiles(directory, arguments.Path);

            DeleteTree(directory);

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

        private static void DeleteTree([NotNull] DirectoryEntry directory)
        {
            foreach (BaseEntry entry in directory.EnumerateEntries(EnumerationFilter.All).OrderBy(x => x.Name).ToArray())
            {
                if (entry is FileEntry file)
                {
                    file.Parent.DeleteFile(file.Name);
                }
                else if (entry is DirectoryEntry subdirectory)
                {
                    DeleteTree(subdirectory);
                }
            }

            directory.Parent?.DeleteDirectory(directory.Name);
        }
    }
}
