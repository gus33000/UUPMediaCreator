// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Removes an app package (.appx) from a Windows image.
        /// </summary>
        /// <param name="session">A valid DISM Session.</param>
        /// <param name="packageName">Specifies the name of the app package (.appx) to remove from the Windows image.</param>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RemoveProvisionedAppxPackage(DismSession session, string packageName)
        {
            int hresult = NativeMethods._DismRemoveProvisionedAppxPackage(session, packageName);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Removes a provisioned appx package.
            /// </summary>
            /// <param name="session">A valid DISM Session.</param>
            /// <param name="packageName">The package name.</param>
            /// <returns>Returns S_OK on success.</returns>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int _DismRemoveProvisionedAppxPackage(DismSession session, [MarshalAs(UnmanagedType.LPWStr)] string packageName);
        }
    }
}