using System;
using System.IO;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Tests
{
    internal static class PathFactory
    {
        private const string UncServerName = "ServerName";
        private const string UncShareName = "ShareName";
        public const string DirectoryNameAtDepth1 = "TopFolder";
        private const string DirectoryNameAtDepth2 = "SubFolder";
        private const string DirectoryNameAtDepth3 = "SubSubFolder";
        public const string FileName = "File.txt";
        public const string FileExtension = ".txt";

        [NotNull]
        public static string NetworkHostWithoutShare(bool isExtended = false)
        {
            return isExtended ? $@"\\?\UNC\{UncServerName}" : $@"\\{UncServerName}";
        }

        [NotNull]
        public static string NetworkShare(bool isExtended = false)
        {
            return isExtended ? $@"\\?\UNC\{UncServerName}\{UncShareName}" : $@"\\{UncServerName}\{UncShareName}";
        }

        [NotNull]
        public static string AltNetworkShare(bool isExtended = false)
        {
            return isExtended ? $@"\\?\UNC\{UncServerName}2\{UncShareName}2" : $@"\\{UncServerName}2\{UncShareName}2";
        }

        [NotNull]
        public static string NetworkDirectoryAtDepth(int depth, bool isExtended = false)
        {
            AssertDepthInRange(depth);

            string networkShare = NetworkShare(isExtended);

            if (depth == 1)
            {
                return Path.Combine(networkShare, DirectoryNameAtDepth1);
            }

            if (depth == 2)
            {
                return Path.Combine(networkShare, DirectoryNameAtDepth1, DirectoryNameAtDepth2);
            }

            return Path.Combine(networkShare, DirectoryNameAtDepth1, DirectoryNameAtDepth2, DirectoryNameAtDepth3);
        }

        [NotNull]
        public static string AltNetworkDirectoryAtDepth(int depth, bool isExtended = false)
        {
            AssertDepthInRange(depth);

            string networkShare = NetworkShare(isExtended);

            if (depth == 1)
            {
                return Path.Combine(networkShare, DirectoryNameAtDepth1 + "2");
            }

            if (depth == 2)
            {
                return Path.Combine(networkShare, DirectoryNameAtDepth1 + "2", DirectoryNameAtDepth2 + "2");
            }

            return Path.Combine(networkShare, DirectoryNameAtDepth1 + "2", DirectoryNameAtDepth2 + "2",
                DirectoryNameAtDepth3 + "2");
        }

        [NotNull]
        public static string NetworkFileAtDepth(int depth, bool isExtended = false)
        {
            AssertDepthInRange(depth);

            string networkShare = NetworkShare(isExtended);

            if (depth == 1)
            {
                return Path.Combine(networkShare, FileName);
            }

            if (depth == 2)
            {
                return Path.Combine(networkShare, DirectoryNameAtDepth1, FileName);
            }

            return Path.Combine(networkShare, DirectoryNameAtDepth1, DirectoryNameAtDepth2, FileName);
        }

        [NotNull]
        public static string AltNetworkFileAtDepth(int depth, bool isExtended = false)
        {
            AssertDepthInRange(depth);

            string networkShare = NetworkShare(isExtended);

            if (depth == 1)
            {
                return Path.Combine(networkShare, FileName + "2");
            }

            if (depth == 2)
            {
                return Path.Combine(networkShare, DirectoryNameAtDepth1 + "2", FileName + "2");
            }

            return Path.Combine(networkShare, DirectoryNameAtDepth1 + "2", DirectoryNameAtDepth2 + "2", FileName + "2");
        }

        [NotNull]
        public static string AltAltNetworkFileAtDepth(int depth, bool isExtended = false)
        {
            AssertDepthInRange(depth);

            string networkShare = NetworkShare(isExtended);

            if (depth == 1)
            {
                return Path.Combine(networkShare, FileName + "3");
            }

            if (depth == 2)
            {
                return Path.Combine(networkShare, DirectoryNameAtDepth1 + "3", FileName + "3");
            }

            return Path.Combine(networkShare, DirectoryNameAtDepth1 + "3", DirectoryNameAtDepth2 + "3", FileName + "3");
        }

        private static void AssertDepthInRange(int depth)
        {
            if (depth < 1 || depth > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(depth));
            }
        }
    }
}
