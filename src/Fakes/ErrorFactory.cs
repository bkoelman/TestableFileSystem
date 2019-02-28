using System;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes
{
    internal static class ErrorFactory
    {
        public static class System
        {
            [NotNull]
            public static Exception FileIsInUse()
            {
                return new IOException("The process cannot access the file because it is being used by another process.");
            }

            [NotNull]
            public static Exception FileIsInUse([NotNull] string path)
            {
                string message = $"The process cannot access the file '{path}' because it is being used by another process.";
                return new IOException(message);
            }

            [NotNull]
            public static Exception FileAlreadyExists([NotNull] string path)
            {
                return new IOException($"The file '{path}' already exists.");
            }

            [NotNull]
            public static Exception CannotCreateFileBecauseFileAlreadyExists()
            {
                return new IOException("Cannot create a file when that file already exists");
            }

            [NotNull]
            public static Exception CannotCreateBecauseFileOrDirectoryAlreadyExists([NotNull] string path)
            {
                return new IOException($"Cannot create '{path}' because a file or directory with the same name already exists.");
            }

            [NotNull]
            public static Exception FileOrDirectoryOrVolumeIsIncorrect()
            {
                return new IOException("The filename, directory name, or volume label syntax is incorrect.");
            }

            [NotNull]
            public static Exception DirectoryIsNotEmpty()
            {
                return new IOException("The directory is not empty.");
            }

            [NotNull]
            public static Exception NetworkPathNotFound()
            {
                return new IOException("The network path was not found.");
            }

            [NotNull]
            public static Exception DirectoryNotFound()
            {
                return new DirectoryNotFoundException("Could not find a part of the path.");
            }

            [NotNull]
            public static Exception DirectoryNotFound([NotNull] string path)
            {
                return new DirectoryNotFoundException($"Could not find a part of the path '{path}'.");
            }

            [NotNull]
            public static Exception FileNotFound([NotNull] string path)
            {
                return new FileNotFoundException($"Could not find file '{path}'.", path);
            }

            [NotNull]
            public static Exception AccessDenied([NotNull] string path)
            {
                return new IOException($"Access to the path '{path}' is denied.");
            }

            [NotNull]
            public static Exception UnauthorizedAccess([NotNull] string path)
            {
                return new UnauthorizedAccessException($"Access to the path '{path}' is denied.");
            }

            [NotNull]
            public static Exception UnauthorizedAccess()
            {
                return new UnauthorizedAccessException("Access to the path is denied.");
            }

            [NotNull]
            public static Exception EmptyPathIsNotLegal([NotNull] [InvokerParameterName] string paramName)
            {
                return new ArgumentException("Empty path name is not legal.", paramName);
            }

            [NotNull]
            public static Exception EmptyFileNameIsNotLegal([NotNull] [InvokerParameterName] string paramName)
            {
                return new ArgumentException("Empty file name is not legal.", paramName);
            }

            [NotNull]
            public static Exception PathIsNotLegal([NotNull] [InvokerParameterName] string paramName)
            {
                return new ArgumentException("The path is not of a legal form.", paramName);
            }

            [NotNull]
            public static Exception PathCannotBeEmptyOrWhitespace([NotNull] [InvokerParameterName] string paramName)
            {
                return new ArgumentException("Path cannot be the empty string or all whitespace.", paramName);
            }

            [NotNull]
            public static Exception PathFormatIsNotSupported()
            {
                return new NotSupportedException("The given path's format is not supported.");
            }

            [NotNull]
            public static Exception IllegalCharactersInPath([NotNull] [InvokerParameterName] string paramName)
            {
                return new ArgumentException("Illegal characters in path.", paramName);
            }

            [NotNull]
            public static Exception DriveNameMustBeRootOrLetter()
            {
                throw new ArgumentException(@"Drive name must be a root directory ('C:\\') or a drive letter ('C').*");
            }

#if !NETSTANDARD1_3
            [NotNull]
            public static Exception CouldNotFindDrive([NotNull] string driveName)
            {
                throw new DriveNotFoundException(
                    $"Could not find the drive '{driveName}'. The drive might not be ready or might not be mapped.");
            }

            [NotNull]
            public static Exception TooManyChangesAtOnce([NotNull] string path)
            {
                return new InternalBufferOverflowException($"Too many changes at once in directory:{path}.");
            }
#endif

            [NotNull]
            public static Exception DirectoryNameIsInvalid()
            {
                return new IOException("The directory name is invalid.");
            }

            [NotNull]
            public static Exception DirectoryNameIsInvalid([NotNull] string path)
            {
                return new ArgumentException($"The directory name {path} is invalid.");
            }

            [NotNull]
            public static Exception ErrorReadingTheDirectory([NotNull] string path)
            {
                return new FileNotFoundException($"Error reading the {path} directory.");
            }

            [NotNull]
            public static Exception UncPathIsInvalid()
            {
                return new ArgumentException(@"The UNC path should be of the form \\server\share.");
            }

            [NotNull]
            public static Exception CannotSeekToPositionBeforeAppend()
            {
                return new IOException(
                    "Unable seek backward to overwrite data that previously existed in a file opened in Append mode.");
            }

            [NotNull]
            public static Exception SearchPatternMustNotBeDriveOrUnc([NotNull] [InvokerParameterName] string paramName)
            {
                return new ArgumentException("Second path fragment must not be a drive or UNC name.", paramName);
            }

            [NotNull]
            public static Exception SearchPatternCannotContainParent([NotNull] [InvokerParameterName] string paramName)
            {
                return new ArgumentException(
                    "Search pattern cannot contain '..' to move up directories and can be contained only internally in file/directory names, as in 'a..b'.",
                    paramName);
            }

            [NotNull]
            public static Exception ParameterIsIncorrect()
            {
                return new IOException("The parameter is incorrect");
            }

            [NotNull]
            public static Exception TargetIsNotFile([NotNull] string path)
            {
                return new IOException($"The target file '{path}' is a directory, not a file.");
            }

            [NotNull]
            public static Exception FileTimeOutOfRange([NotNull] [InvokerParameterName] string paramName)
            {
                return new ArgumentOutOfRangeException(paramName,
                    "The UTC time represented when the offset is applied must be between year 0 and 10,000.");
            }

            [NotNull]
            public static Exception FileTimeNotValid([NotNull] [InvokerParameterName] string paramName)
            {
                return new ArgumentOutOfRangeException(paramName, "Not a valid Win32 FileTime.");
            }

            [NotNull]
            public static Exception InvalidOpenCombination(FileMode mode, FileAccess access)
            {
                return new ArgumentException($"Combining FileMode: {mode} with FileAccess: {access} is invalid.", nameof(access));
            }

            [NotNull]
            public static Exception PathMustNotBeDrive([NotNull] [InvokerParameterName] string paramName)
            {
                return new ArgumentException("Path must not be a drive.", paramName);
            }

            [NotNull]
            public static Exception VolumesMustBeIdentical()
            {
                return new IOException(
                    "Source and destination path must have identical roots. Move will not work across volumes.");
            }

            [NotNull]
            public static Exception DestinationMustBeDifferentFromSource()
            {
                return new IOException("Source and destination path must be different.");
            }

            [NotNull]
            public static Exception PathFragmentMustNotBeDriveOrUncName([NotNull] [InvokerParameterName] string paramName)
            {
                return new ArgumentException("Second path fragment must not be a drive or UNC name.", paramName);
            }

            [NotNull]
            public static Exception DirectoryIsNotASubdirectory([NotNull] string path, [NotNull] string absolutePath)
            {
                throw new ArgumentException($@"The directory specified, '{path}', is not a subdirectory of '{absolutePath}'.",
                    nameof(path));
            }

            [NotNull]
            public static Exception CannotAccessFileProcessHasLocked()
            {
                throw new IOException(
                    "The process cannot access the file because another process has locked a portion of the file");
            }

            [NotNull]
            public static Exception SegmentIsAlreadyUnlocked()
            {
                throw new IOException("The segment is already unlocked");
            }

            [NotNull]
            public static Exception FileIsReadOnly()
            {
                throw new IOException("The specified file is read only.");
            }

            [NotNull]
            public static Exception FileExists()
            {
                throw new IOException("The file exists.");
            }

            [NotNull]
            public static Exception UnableToRemoveFileToBeReplaced()
            {
                throw new IOException("Unable to remove the file to be replaced.");
            }

            [NotNull]
            public static Exception UnableToMoveReplacementFile()
            {
                throw new IOException(
                    "Unable to move the replacement file to the file to be replaced. The file to be replaced has retained its original name.");
            }

            [NotNull]
            public static Exception UnableToFindSpecifiedFile()
            {
                throw new FileNotFoundException("Unable to find the specified file.");
            }
        }

        public static class Internal
        {
            [NotNull]
            public static Exception UnknownError([NotNull] string message)
            {
                return new Exception($"Unexpected Internal Error: {message}\r\n\r\n" +
                    "Please notify the author by creating an issue at 'https://github.com/bkoelman/TestableFileSystem'.");
            }

            [NotNull]
            public static Exception EnumValueUnsupported<TEnum>(TEnum value)
                where TEnum : struct
            {
                throw new NotSupportedException($"Unsupported value '{value}' for {typeof(TEnum).Name}.");
            }
        }
    }
}
