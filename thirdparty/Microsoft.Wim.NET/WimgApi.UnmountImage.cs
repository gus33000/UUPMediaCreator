// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Unmounts a mounted image in a Windows® image (.wim) file from the specified directory.
        /// </summary>
        /// <param name="mountPath">The full file path of the directory to which the .wim file was mounted.</param>
        /// <param name="imagePath">The full file name of the .wim file that must be unmounted.</param>
        /// <param name="imageIndex">Specifies the index of the image in the .wim file that must be unmounted.</param>
        /// <param name="commitChanges"><c>true</c> to commit changes made to the .wim file if any, otherwise <c>false</c> to discard changes.  This parameter has no effect if the .wim file was mounted not to enable edits.</param>
        /// <exception cref="ArgumentNullException">mountPath or imagePath is null.</exception>
        /// <exception cref="DirectoryNotFoundException">mountPath does not exist.</exception>
        /// <exception cref="FileNotFoundException">imagePath does not exist.</exception>
        /// <exception cref="IndexOutOfRangeException">index is less than 1.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>This method unmaps the contents of the given image in the .wim file from the specified mount directory. After the successful completion of this operation, users or applications will not be able to access the contents of the image previously mapped under the mount directory.</remarks>
        public static void UnmountImage(string mountPath, string imagePath, int imageIndex, bool commitChanges)
        {
            // See if mountPath is null
            if (mountPath == null)
            {
                throw new ArgumentNullException(nameof(mountPath));
            }

            // See if imagePath is null
            if (imagePath == null)
            {
                throw new ArgumentNullException(nameof(imagePath));
            }

            // See if the specified index is valid
            if (imageIndex < 1)
            {
                throw new IndexOutOfRangeException($"There is no image at index {imageIndex}.");
            }

            // See if mount path does not exist
            if (!Directory.Exists(mountPath))
            {
                throw new DirectoryNotFoundException($"Could not find a part of the path '{mountPath}'");
            }

            // See if the image does not exist
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Could not find a part of the path '{imagePath}'");
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMUnmountImage(mountPath, imagePath, (DWORD)imageIndex, commitChanges))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Unmounts a mounted image in a Windows® image (.wim) file from the specified directory.
        /// </summary>
        /// <param name="imageHandle">A <see cref="WimHandle"/> of an image previously mounted with <see cref="MountImage(WimHandle, string, WimMountImageOptions)"/>.</param>
        /// <exception cref="ArgumentNullException">imageHandle is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void UnmountImage(WimHandle imageHandle)
        {
            // See if imageHandle is null
            if (imageHandle == null)
            {
                throw new ArgumentNullException(nameof(imageHandle));
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMUnmountImageHandle(imageHandle, 0))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }

            // Close the image handle
            imageHandle.Close();
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Unmounts a mounted image in a Windows® image (.wim) file from the specified directory.
            /// </summary>
            /// <param name="pszMountPath">
            /// A pointer to the full file path of the directory to which the .wim file was mounted. This
            /// parameter is required and cannot be NULL.
            /// </param>
            /// <param name="pszWimFileName">
            /// A pointer to the full file name of the .wim file that must be unmounted. This parameter is
            /// required and cannot be NULL.
            /// </param>
            /// <param name="dwImageIndex">Specifies the index of the image in the .wim file that must be unmounted.</param>
            /// <param name="bCommitChanges">
            /// A flag that indicates whether changes (if any) to the .wim file must be committed before
            /// unmounting the .wim file. This flag has no effect if the .wim file was mounted not to enable edits.
            /// </param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            /// <remarks>
            /// The WIMUnmountImage function unmaps the contents of the given image in the .wim file from the specified mount
            /// directory. After the successful completion of this operation, users or applications will not be able to access the
            /// contents of the image previously mapped under the mount directory.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMUnmountImage(string pszMountPath, string pszWimFileName, DWORD dwImageIndex, [MarshalAs(UnmanagedType.Bool)] bool bCommitChanges);

            /// <summary>
            /// Unmounts an image from a Windows® image (.wim) that was previously mounted with the WIMMountImageHandle function.
            /// </summary>
            /// <param name="hImage">A handle to an image previously mounted with WIMMountImageHandle.</param>
            /// <param name="dwUnmountFlags">Reserved. Must be zero.</param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            /// <remarks>
            /// The WIMUnmountImageHandle function unmaps the contents of the given image in the .wim file from the specified
            /// mount directory. After the successful completion of this operation, users or applications will not be able to access
            /// the contents of the image previously mapped under the mount directory.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMUnmountImageHandle(WimHandle hImage, DWORD dwUnmountFlags);
        }
    }
}