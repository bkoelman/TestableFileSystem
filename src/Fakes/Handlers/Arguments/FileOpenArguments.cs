﻿using System.IO;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes.Handlers.Arguments
{
    internal sealed class FileOpenArguments
    {
        [NotNull]
        public string Path { get; }

        public FileMode Mode { get; }

        [CanBeNull]
        public FileAccess? Access { get; }

        public FileOpenArguments([NotNull] string path, FileMode mode, [CanBeNull] FileAccess? access)
        {
            Guard.NotNull(path, nameof(path));

            Path = path;
            Mode = mode;
            Access = access;
        }
    }
}