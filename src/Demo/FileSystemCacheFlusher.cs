using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace TestableFileSystem.Demo
{
    /// <summary>
    /// Flushes internal buffers of the operating system, which prevent correct behavior of the <see cref="FileSystemWatcher" />. See
    /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/4465cafb-f4ed-434f-89d8-c85ced6ffaa8/filesystemwatcher-reliability?forum=netfxbcl.
    /// </summary>
    /// <remarks>
    /// Requires administrator privileges to run.
    /// </remarks>
    internal static class FileSystemCacheFlusher
    {
        public static void FlushVolume(char driveLetter)
        {
            string volumeName = "\\\\.\\" + driveLetter + ":\0";

            IntPtr volumeHandle = NativeMethods.CreateFileW(volumeName, FileAccess.Write, FileShare.Write, IntPtr.Zero,
                FileMode.Open, 0, IntPtr.Zero);

            if (volumeHandle == NativeMethods.InvalidHandleValue)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            try
            {
                if (!NativeMethods.FlushFileBuffers(volumeHandle))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (!NativeMethods.CloseHandle(volumeHandle))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        private static class NativeMethods
        {
            public static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr CreateFileW([MarshalAs(UnmanagedType.LPWStr)] string filename,
                [MarshalAs(UnmanagedType.U4)] FileAccess access, [MarshalAs(UnmanagedType.U4)] FileShare share,
                IntPtr securityAttributes, [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes, IntPtr templateFile);

            [DllImport("kernel32.dll")]
            public static extern bool FlushFileBuffers(IntPtr hFile);

            [DllImport("kernel32.dll")]
            public static extern bool CloseHandle(IntPtr hObject);
        }
    }
}
