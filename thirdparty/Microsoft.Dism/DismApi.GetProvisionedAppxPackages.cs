// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Gets information about app packages (.appx) in an image that will be installed for each new user.
        /// </summary>
        /// <param name="session">A valid DISM Session.</param>
        /// <returns>A <see cref="DismAppxPackageCollection" /> object containing a collection of <see cref="DismAppxPackage" /> objects.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static DismAppxPackageCollection GetProvisionedAppxPackages(DismSession session)
        {
            int hresult = NativeMethods._DismGetProvisionedAppxPackages(session, out IntPtr appxPackagesPtr, out UInt32 appxPackagesCount);

            try
            {
                DismUtilities.ThrowIfFail(hresult, session);

                return new DismAppxPackageCollection(appxPackagesPtr, appxPackagesCount);
            }
            finally
            {
                Delete(appxPackagesPtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Gets a provisioned appx package.
            /// </summary>
            /// <param name="session">A valid DISM Session.</param>
            /// <param name="packageBufPtr">Receives the array of packages.</param>
            /// <param name="packageCount">Receives the count of packages.</param>
            /// <returns>Returns S_OK on success.</returns>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int _DismGetProvisionedAppxPackages(DismSession session, out IntPtr packageBufPtr, out UInt32 packageCount);
        }
    }
}