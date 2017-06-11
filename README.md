# Testable FileSystem

[![Build status](https://ci.appveyor.com/api/projects/status/wiekgd62kq1o27kw?svg=true)](https://ci.appveyor.com/project/bkoelman/testablefilesystem/branch/master)

Provides abstractions for `System.IO.*` including in-memory fakes, intended for unit-testing.
Requires [NetStandard](https://github.com/dotnet/standard/blob/master/docs/versions.md) 1.3 or higher.

Building from source requires Visual Studio 2017. Although the interfaces and wrappers are considered stable, the in-memory fake filesystem is still a work in progress.

# Features of the fake filesystem
* Concurrent access to the in-memory filesystem is thread-safe
* You'll get appropriate exceptions for files that are in use
* Fails on changing readonly files and directories
* Supports absolute and relative paths, based on settable current directory
* Supports local and UNC (Universal Naming Convention) network paths
* Paths are case-insensitive

# Limitations of the fake filesystem
* Limitations around MAXPATH do not apply (paths starting with `\\?\` are allowed)
* Device namespaces (for example: `\\.\COM56`) are not supported
* Exceptions may have slightly different messages (but matching type)
* Exception messages are always in US-English (not localized)
* Some file attributes, such as Compressed/Encrypted you will never get (because they are set by nonstandard APIs)
* NTFS permissions are not implemented
* A file cannot be opened by multiple writers at the same time
* Hard links, junctions and symbolic links (reparse points) are not implemented
* 8.3 aliases for file names are not implemented

For a better understanding of the Windows filesystem, see https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx.
