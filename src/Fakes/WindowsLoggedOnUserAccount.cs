using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Fakes
{
    // ReSharper disable once PartialTypeWithSinglePart
    public sealed partial class WindowsLoggedOnUserAccount : ILoggedOnUserAccount
    {
        public string UserName => WindowsIdentity.GetCurrent().Name;

#if !NET45
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
                WindowsIdentity.RunImpersonated(safeAccessTokenHandle, action);
            }
        }
#endif

        private static class NativeMethods
        {
            public const int Logon32ProviderDefault = 0;
            public const int Logon32LogonInteractive = 2;

            [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool LogonUser([NotNull] string lpszUsername, [CanBeNull] string lpszDomain,
                [NotNull] string lpszPassword, int dwLogonType, int dwLogonProvider, [NotNull] out SafeAccessTokenHandle phToken);
        }
    }
}
