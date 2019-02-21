#if NET45
using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using TestableFileSystem.Utilities;

namespace TestableFileSystem.Fakes
{
    public sealed partial class WindowsLoggedOnUserAccount : ILoggedOnUserAccount
    {
        public void RunImpersonated([NotNull] NetworkCredential credentials, [NotNull] Action action)
        {
            Guard.NotNull(credentials, nameof(credentials));
            Guard.NotNull(action, nameof(action));

            bool succeeded = NativeMethods.LogonUser(credentials.UserName, credentials.Domain, credentials.Password,
                NativeMethods.Logon32LogonInteractive, NativeMethods.Logon32ProviderDefault,
                out SafeAccessTokenHandle safeAccessTokenHandle);

            if (!succeeded)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            using (safeAccessTokenHandle)
            {
                using (WindowsIdentity.Impersonate(safeAccessTokenHandle.DangerousGetHandle()))
                {
                    action();
                }
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private sealed class SafeAccessTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeAccessTokenHandle()
                : base(true)
            {
            }

            [DllImport("kernel32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }
    }
}
#endif
