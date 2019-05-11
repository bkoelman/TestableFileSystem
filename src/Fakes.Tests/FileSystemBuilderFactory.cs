using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using TestableFileSystem.Fakes.Builders;
using TestableFileSystem.Fakes.Tests.DiskOperations;
using TestableFileSystem.Utilities;
using TestableFileSystem.Wrappers;

namespace TestableFileSystem.Fakes.Tests
{
    internal sealed class FileSystemBuilderFactory : IDisposable
    {
        private readonly bool useFakeFileSystem;

        [NotNull]
        private readonly Dictionary<string, string> fakeToHostUncMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        [NotNull]
        private readonly Dictionary<string, ShareCreationStatus> networkShares =
            new Dictionary<string, ShareCreationStatus>(StringComparer.OrdinalIgnoreCase);

        [NotNull]
        private readonly string rootDirectory =
            Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), "FS", Guid.NewGuid().ToString());

        public FileSystemBuilderFactory(bool useFakeFileSystem)
        {
            this.useFakeFileSystem = useFakeFileSystem;
        }

        [NotNull]
        public IFileSystemBuilder Create()
        {
            return useFakeFileSystem
                ? (IFileSystemBuilder)new FakeFileSystemBuilder()
                : new WrapperFileSystemBuilder(FileSystemWrapper.Default, this);
        }

        [NotNull]
        public string MapPath([NotNull] string path, bool mapUncToCurrentHost = true)
        {
            Guard.NotNull(path, nameof(path));

            if (useFakeFileSystem)
            {
                return path;
            }

            string pathOnDrive = TryMapPathOnDrive(path);
            if (pathOnDrive != null)
            {
                return pathOnDrive;
            }

            NetworkPath pathOnNetwork = NetworkPath.TryParse(path);
            if (pathOnNetwork != null)
            {
                return MapPathOnNetworkShare(pathOnNetwork, mapUncToCurrentHost);
            }

            throw new NotSupportedException($"Path '{path}' must be an absolute path, including drive letter or UNC share.");
        }

        [CanBeNull]
        private string TryMapPathOnDrive([NotNull] string path)
        {
            int offset = path.StartsWith(@"\\?\", StringComparison.Ordinal) ? 4 : 0;

            if (path.Length < offset + 2 || path[offset + 1] != Path.VolumeSeparatorChar)
            {
                return null;
            }

            char driveLetter = char.ToUpperInvariant(path[offset]);

            string mapped = path.Length > offset + 3
                ? Path.Combine(rootDirectory, "Drive" + driveLetter, path.Substring(offset + 3))
                : Path.Combine(rootDirectory, "Drive" + driveLetter);

            if (offset != 0)
            {
                mapped = @"\\?\" + mapped;
            }

            return mapped;
        }

        [NotNull]
        private string MapPathOnNetworkShare([NotNull] NetworkPath networkPath, bool mapToCurrentHost)
        {
            string fakeKey = networkPath.ServerName + @"\" + networkPath.ShareName;
            EnsureEntryInFakeToHostUncMap(fakeKey, networkPath, mapToCurrentHost);

            string hostKey = fakeToHostUncMap[fakeKey];
            ShareCreationStatus shareStatus = networkShares[hostKey];

            string serverShareName = networkPath.IsExtended
                ? @"\\?\UNC\" + shareStatus.ServerName + @"\" + shareStatus.ShareName
                : @"\\" + shareStatus.ServerName + @"\" + shareStatus.ShareName;

            return Path.Combine(serverShareName, networkPath.RelativePath);
        }

        private void EnsureEntryInFakeToHostUncMap([NotNull] string fakeKey, [NotNull] NetworkPath networkPath,
            bool mapToCurrentHost)
        {
            if (fakeToHostUncMap.ContainsKey(fakeKey))
            {
                return;
            }

            string serverName = mapToCurrentHost ? Environment.MachineName : networkPath.ServerName;
            string shareName = mapToCurrentHost ? Guid.NewGuid().ToString("N") : networkPath.ShareName;

            string hostKey = "Share-" + serverName + "-" + shareName;
            fakeToHostUncMap[fakeKey] = hostKey;

            networkShares.Add(hostKey, new ShareCreationStatus(serverName, shareName, mapToCurrentHost));
        }

        public void EnsureNetworkShareExists([NotNull] string path)
        {
            NetworkPath networkPath = NetworkPath.TryParse(path);
            if (networkPath == null)
            {
                return;
            }

            string hostKey = "Share-" + networkPath.ServerName + "-" + networkPath.ShareName;

            if (!networkShares.ContainsKey(hostKey) || networkShares[hostKey].IsCreated)
            {
                return;
            }

            if (!networkShares[hostKey].CanBeCreated)
            {
                throw new InvalidOperationException("Cannot create share on remote host.");
            }

            CreateNetworkShare(networkShares[hostKey]);

            networkShares[hostKey].IsCreated = true;
        }

        private void CreateNetworkShare([NotNull] ShareCreationStatus shareStatus)
        {
            string backingDirectory =
                Path.Combine(rootDirectory, "Share-" + shareStatus.ServerName + "-" + shareStatus.ShareName);

            Directory.CreateDirectory(backingDirectory);
            NetworkShareManager.CreateShare(backingDirectory, shareStatus.ShareName);
        }

        public void Dispose()
        {
            if (!useFakeFileSystem && Directory.Exists(rootDirectory))
            {
                CleanupWithRetry();
            }
        }

        private void CleanupWithRetry()
        {
            try
            {
                Cleanup();
            }
            catch (IOException ex) when (ex.Message.Contains("The process cannot access the file"))
            {
                Thread.Sleep(100);
                Cleanup();
            }
        }

        private void Cleanup()
        {
            DetachNetworkShares();

            try
            {
                Directory.Delete(rootDirectory, true);
            }
            catch (UnauthorizedAccessException)
            {
                RemoveReadOnlyAttributes();

                Directory.Delete(rootDirectory, true);
            }
        }

        private void DetachNetworkShares()
        {
            while (networkShares.Any())
            {
                KeyValuePair<string, ShareCreationStatus> pair = networkShares.First();
                ShareCreationStatus shareStatus = pair.Value;

                if (shareStatus.IsCreated)
                {
                    NetworkShareManager.RemoveShare(shareStatus.ServerName, shareStatus.ShareName);
                }

                networkShares.Remove(pair.Key);
            }
        }

        private void RemoveReadOnlyAttributes()
        {
            foreach (string path in Directory.GetFiles(rootDirectory, "*.*", SearchOption.AllDirectories))
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }

            foreach (string path in Directory.GetDirectories(rootDirectory, "*.*", SearchOption.AllDirectories))
            {
                File.SetAttributes(path, FileAttributes.Directory);
            }
        }

        private sealed class NetworkPath
        {
            [NotNull]
            public string ServerName { get; }

            [NotNull]
            public string ShareName { get; }

            [NotNull]
            public string RelativePath { get; }

            public bool IsExtended { get; }

            private NetworkPath([NotNull] string serverName, [NotNull] string shareName, [NotNull] string relativePath,
                bool isExtended)
            {
                ServerName = serverName;
                ShareName = shareName;
                RelativePath = relativePath;
                IsExtended = isExtended;
            }

            [CanBeNull]
            public static NetworkPath TryParse([NotNull] string path)
            {
                Guard.NotNull(path, nameof(path));

                int offset = path.StartsWith(@"\\?\UNC\", StringComparison.Ordinal) ? 8 :
                    path.StartsWith(@"\\", StringComparison.Ordinal) ? 2 : 0;

                if (offset == 0)
                {
                    return null;
                }

                int serverShareSeparatorIndex = path.IndexOfAny(new[]
                {
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar
                }, offset);

                if (serverShareSeparatorIndex == -1)
                {
                    return null;
                }

                int sharePathSeparatorIndex = path.IndexOfAny(new[]
                {
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar
                }, serverShareSeparatorIndex + 1);

                string serverName = path.Substring(offset, serverShareSeparatorIndex - offset);

                string shareName = sharePathSeparatorIndex == -1
                    ? path.Substring(serverShareSeparatorIndex + 1)
                    : path.Substring(serverShareSeparatorIndex + 1,
                        sharePathSeparatorIndex - serverShareSeparatorIndex - 1);

                string relativePath = sharePathSeparatorIndex == -1
                    ? string.Empty
                    : path.Substring(sharePathSeparatorIndex + 1);

                return new NetworkPath(serverName, shareName, relativePath, offset == 8);
            }
        }

        private sealed class ShareCreationStatus
        {
            [NotNull]
            public string ServerName { get; }

            [NotNull]
            public string ShareName { get; }

            public bool CanBeCreated { get; }

            public bool IsCreated { get; set; }

            public ShareCreationStatus([NotNull] string serverName, [NotNull] string shareName, bool canBeCreated)
            {
                Guard.NotNull(serverName, nameof(serverName));
                Guard.NotNull(shareName, nameof(shareName));

                ServerName = serverName;
                ShareName = shareName;
                CanBeCreated = canBeCreated;
            }
        }
    }
}
