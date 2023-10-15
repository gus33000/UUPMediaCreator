// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    /// <summary>
    /// Specifies options when committing an image.
    /// </summary>
    [Flags]
    public enum WimCommitImageOptions : uint
    {
        /// <summary>
        /// Disables capturing security information for directories.
        /// </summary>
        DisableDirectoryAcl = WimgApi.WIM_FLAG_NO_DIRACL,

        /// <summary>
        /// Disables capturing security information for files.
        /// </summary>
        DisableFileAcl = WimgApi.WIM_FLAG_NO_FILEACL,

        /// <summary>
        /// Disables automatic path repairs for junctions and symbolic links.
        /// </summary>
        DisableRPFix = WimgApi.WIM_FLAG_NO_RP_FIX,

        /// <summary>
        /// No options are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Capture verifies single-instance files byte by byte.
        /// </summary>
        Verify = WimgApi.WIM_FLAG_VERIFY,
    }

    public static partial class WimgApi
    {
        /// <summary>
        /// Saves the changes from a mounted image back to the.wim file.
        /// </summary>
        /// <param name="imageHandle">A <see cref="WimHandle"/> opened by the <see cref="LoadImage"/> method. The .wim file must have been opened with a <see cref="WimFileAccess.Mount"/> flag in call to <see cref="CreateFile" />.</param>
        /// <param name="append"><c>true</c> to append the modified image to the .wim file.  <c>false</c> to commit the changes to the original image.</param>
        /// <param name="options">Specifies the features to use during the capture.</param>
        /// <returns>If append is <c>true</c>, a <see cref="WimHandle"/> of the new image, otherwise a null handle.</returns>
        /// <exception cref="ArgumentNullException">imageHandle is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static WimHandle CommitImageHandle(WimHandle imageHandle, bool append, WimCommitImageOptions options)
        {
            // See if the handle is null
            if (imageHandle == null)
            {
                throw new ArgumentNullException(nameof(imageHandle));
            }

            // Call the native function, add the append flag if needed
            if (!WimgApi.NativeMethods.WIMCommitImageHandle(imageHandle, append ? WimgApi.WIM_COMMIT_FLAG_APPEND : 0 | (DWORD)options, out WimHandle newImageHandle))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }

            // Return the new image handle which may or may not contain a handle
            return newImageHandle;
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Saves the changes from a mounted image back to the .wim file.
            /// </summary>
            /// <param name="hImage">
            /// A handle to an image opened by the WIMLoadImage function. The .wim file must have been opened with
            /// a WIM_GENERIC_MOUNT flag in call to WIMCreateFile.
            /// </param>
            /// <param name="dwCommitFlags">Specifies the features to use during the capture.</param>
            /// <param name="phNewImageHandle">
            /// Pointer to receive the new image handle if the WIM_COMMIT_FLAG_APPEND flag is specified.
            /// If this parameter is NULL, the new image will be closed automatically.
            /// </param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            /// <remarks>
            /// The WIMCommitImageHandle function updates the contents of the given image in a .wim file to reflect the
            /// contents of the specified mount directory. After the successful completion of this operation, users or applications can
            /// still access the contents of the image mapped under the mount directory. Use the WIMUnmountImageHandle function to
            /// unmount the image from the mount directory using an image handle.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMCommitImageHandle(WimHandle hImage, DWORD dwCommitFlags, out WimHandle phNewImageHandle);
        }
    }
}