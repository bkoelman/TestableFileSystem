# Testable File System

[![Build status](https://ci.appveyor.com/api/projects/status/wiekgd62kq1o27kw?svg=true)](https://ci.appveyor.com/project/bkoelman/testablefilesystem)
[![codecov](https://codecov.io/gh/bkoelman/TestableFileSystem/branch/master/graph/badge.svg)](https://codecov.io/gh/bkoelman/TestableFileSystem)

This project provides a blazingly fast in-memory file system. With an API surface that is practically identical to the `System.IO` library from the .NET Framework and .NET Core. Using the provided interfaces makes it easy to toggle between the real file system and the fake one. For example, in unit tests.

To help transition existing codebases, an analyzer is included that runs in Visual Studio and highlights potential locations where `System.IO` calls can be replaced with interface-based calls.

## Features of the fake filesystem
* Concurrent access to the in-memory filesystem is thread-safe
* Throws appropriate exceptions for files that are in use
* Fails on changing readonly files and directories
* Supports absolute and relative paths, based on settable current directory
* Supports local and UNC (Universal Naming Convention) network paths
* Paths are case-insensitive
* Mimics NTFS behavior with [NtfsDisableLastAccessUpdate](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/fsutil-behavior) disabled
* Full support for [FileSystemWatcher](https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?view=netstandard-2.0) and async I/O in NetStandard 2.0

## Limitations of the fake filesystem
* A file cannot be opened by multiple writers at the same time (multiple readers are allowed)
* Exception messages are always in US-English (not localized, but matching type)
* Limitations around `MAXPATH` do not apply (paths starting with `\\?\` are allowed)
* Device namespaces (`\\.\COM56`) and ports (`COM1`) are not supported
* Some file attributes, such as Compressed/Offline/NotContentIndexed you will never get (because they are set by nonstandard APIs)
* NTFS permissions are not implemented
* Hard links, junctions and symbolic links (reparse points) are not implemented
* 8.3 aliases for file names are not implemented

## Get started

The NuGet packages provide assemblies that target [NetStandard](https://github.com/dotnet/standard/blob/master/docs/versions.md) 2.0, 1.3 and .NET Framework 4.5.

* From the NuGet package manager console:

  `Install-Package TestableFileSystem.Wrappers -project YourProductName`

  `Install-Package TestableFileSystem.Fakes -project YourProductUnitTests`

See [Demo/Program.cs](https://github.com/bkoelman/TestableFileSystem/blob/master/src/Demo/Program.cs) and [documentation](doc/Overview.md) for example usage.

## Trying out the latest build

After each commit, new prerelease NuGet packages are automatically published to AppVeyor at https://ci.appveyor.com/project/bkoelman/testablefilesystem/branch/master/artifacts. To try it out, follow the next steps:

* In Visual Studio: **Tools**, **NuGet Package Manager**, **Package Manager Settings**, **Package Sources**
    * Click **+**
    * Name: **AppVeyor TestableFileSystem**, Source: **https://ci.appveyor.com/nuget/testablefilesystem**
    * Click **Update**, **Ok**
* Open the NuGet package manager console (**Tools**, **NuGet Package Manager**, **Package Manager Console**)
    * Select **AppVeyor TestableFileSystem** as package source
    * Run command: `Install-Package TestableFileSystem.Wrappers -pre` or `Install-Package TestableFileSystem.Fakes -pre`

Building this project from source requires Visual Studio 2017 or later.
