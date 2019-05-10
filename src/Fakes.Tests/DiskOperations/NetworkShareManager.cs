using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes.Tests.DiskOperations
{
    internal static class NetworkShareManager
    {
        public static void CreateShare([NotNull] string path, [NotNull] string shareName)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotNull(shareName, nameof(shareName));

            var buffer = new NativeMethods.ShareInfo2
            {
                netname = shareName,
                type = NativeMethods.ShareTypeDiskTree,
                permissions = NativeMethods.PermissionAccessAll,
                maxUses = -1,
                path = path
            };

            uint errorCode = NativeMethods.NetShareAdd(null, NativeMethods.BufferLevel, ref buffer, out _);
            ThrowForNonZeroErrorCode(errorCode);
        }

        public static void RemoveShare([NotNull] string hostName, [NotNull] string shareName)
        {
            Guard.NotNull(shareName, nameof(shareName));

            uint errorCode = NativeMethods.NetShareDel(hostName, shareName, 0);
            ThrowForNonZeroErrorCode(errorCode);
        }

        private static void ThrowForNonZeroErrorCode(uint errorCode)
        {
            if (errorCode != 0)
            {
                throw new Win32Exception((int)errorCode);
            }
        }

        private static class NativeMethods
        {
            public const uint ShareTypeDiskTree = 0;
            public const int PermissionAccessAll = 0x7F;
            public const int BufferLevel = 2;

            [DllImport("Netapi32.dll")]
            public static extern uint NetShareAdd([MarshalAs(UnmanagedType.LPWStr)] [CanBeNull]
                string server,
                int level,
                ref ShareInfo2 buffer,
                out uint errorIndex);

            [DllImport("netapi32.dll", SetLastError = true)]
            public static extern uint NetShareDel([MarshalAs(UnmanagedType.LPWStr)] [CanBeNull]
                string server,
                [MarshalAs(UnmanagedType.LPWStr)] [NotNull]
                string netName,
                int reserved);

            [StructLayout(LayoutKind.Sequential)]
            public struct ShareInfo2
            {
                [MarshalAs(UnmanagedType.LPWStr)]
                [NotNull]
                public string netname;

                public uint type;

                [MarshalAs(UnmanagedType.LPWStr)]
                [CanBeNull]
                public readonly string remark;

                public int permissions;

                public int maxUses;

                public readonly int currentUses;

                [MarshalAs(UnmanagedType.LPWStr)]
                [NotNull]
                public string path;

                [MarshalAs(UnmanagedType.LPWStr)]
                [CanBeNull]
                public readonly string password;
            }
        }
    }
}
