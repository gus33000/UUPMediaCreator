// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Gets a <see cref="WimHandle"/> for the .wim file and a <see cref="WimHandle"/> for the image corresponding to a mounted image directory.
        /// </summary>
        /// <param name="mountPath">The full file path of the directory to which the .wim file has been mounted.</param>
        /// <param name="readOnly"><c>true</c> to get a handle that cannot commit changes, regardless of the access level requested at mount time, otherwise <c>false</c>.</param>
        /// <param name="imageHandle">A <see cref="WimHandle"/>corresponding to the image mounted at the specified path.</param>
        /// <returns>A <see cref="WimHandle"/>corresponding to the .wim file mounted at the specified path.</returns>
        /// <exception cref="ArgumentNullException">mountPath is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static WimHandle GetMountedImageHandle(string mountPath, bool readOnly, out WimHandle imageHandle)
        {
            // See if mountPath is null
            if (mountPath == null)
            {
                throw new ArgumentNullException(nameof(mountPath));
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMGetMountedImageHandle(mountPath, readOnly ? WimgApi.WIM_FLAG_MOUNT_READONLY : 0, out WimHandle wimHandle, out imageHandle))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }

            // Return the WIM handle
            return wimHandle;
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Returns a WIM handle and an image handle corresponding to a mounted image directory.
            /// </summary>
            /// <param name="pszMountPath">
            /// A pointer to the full file path of the directory to which the .wim file has been mounted.
            /// This parameter is required and cannot be NULL.
            /// </param>
            /// <param name="dwFlags">Specifies how the file is to be treated and what features are to be used.</param>
            /// <param name="phWimHandle">
            /// Pointer to receive a WIM handle corresponding to the image mounted at the specified path.
            /// This parameter is required and cannot be NULL.
            /// </param>
            /// <param name="phImageHandle">
            /// Pointer to receive an WIM handle corresponding to the image mounted at the specified path.
            /// This parameter is required and cannot be NULL.
            /// </param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            /// <remarks>Use the WIMUnmountImageHandle function to unmount the image from the mount directory.</remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMGetMountedImageHandle(string pszMountPath, DWORD dwFlags, out WimHandle phWimHandle, out WimHandle phImageHandle);
        }
    }
}