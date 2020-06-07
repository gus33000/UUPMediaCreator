// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Reactivates a mounted image that was previously mounted to the specified directory.
        /// </summary>
        /// <param name="mountPath">The full file path of the directory to which the .wim file must be remounted.</param>
        /// <exception cref="ArgumentNullException">mountPath is null.</exception>
        /// <exception cref="DirectoryNotFoundException">mountPath does not exist.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void RemountImage(string mountPath)
        {
            // See if mountPath is null
            if (mountPath == null)
            {
                throw new ArgumentNullException(nameof(mountPath));
            }

            // See if mount path does not exist
            if (!Directory.Exists(mountPath))
            {
                throw new DirectoryNotFoundException($"Could not find a part of the path '{mountPath}'");
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMRemountImage(mountPath, 0))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Reactivates a mounted image that was previously mounted to the specified directory.
            /// </summary>
            /// <param name="pszMountPath">
            /// A pointer to the full file path of the directory to which the .wim file must be remounted.
            /// This parameter is required and cannot be NULL.
            /// </param>
            /// <param name="dwFlags">Reserved. Must be zero.</param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            /// <remarks>
            /// The WIMRemountImage function maps the contents of the given image in a .wim file to the specified mount
            /// directory. After the successful completion of this operation, users or applications can access the contents of the
            /// image mapped under the mount directory. Use the WIMUnmountImage function to unmount the image from the mount directory.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMRemountImage(string pszMountPath, DWORD dwFlags);
        }
    }
}