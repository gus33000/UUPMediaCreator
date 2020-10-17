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
    /// <summary>
    /// Specifies options when capturing an image.
    /// </summary>
    [Flags]
    public enum WimCaptureImageOptions : uint
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
        /// Disables automatic path fix-ups for junctions and symbolic links.
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
        /// Captures an image from a directory path and stores it in an image file.
        /// </summary>
        /// <param name="wimHandle">The handle to a .wim file returned by <see cref="CreateFile" />.</param>
        /// <param name="path">The root drive or directory path from where the image data is captured.</param>
        /// <param name="options">Specifies the features to use during the capture.</param>
        /// <returns>A <see cref="WimHandle"/> of the image if the method succeeded, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">wimHandle is null.
        /// -or-
        /// path is null.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="path"/> does not exist.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static WimHandle CaptureImage(WimHandle wimHandle, string path, WimCaptureImageOptions options)
        {
            // See if the handle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if path is null
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // See if path doesn't exist
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Could not find part of the path '{path}'");
            }

            // Call the native function
            WimHandle imageHandle = WimgApi.NativeMethods.WIMCaptureImage(wimHandle, path, (DWORD)options);

            // See if the handle returned is valid
            if (imageHandle == null || imageHandle.IsInvalid)
            {
                // Throw a Win32Exception which will call GetLastError
                throw new Win32Exception();
            }

            // Return the handle to the image
            return imageHandle;
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Captures an image from a directory path and stores it in an image file.
            /// </summary>
            /// <param name="hWim">The handle to a .wim file returned by WIMCreateFile.</param>
            /// <param name="pszPath">
            /// A pointer to a null-terminated string containing the root drive or directory path from where the
            /// image data is captured.
            /// </param>
            /// <param name="dwCaptureFlags">Specifies the features to use during the capture.</param>
            /// <returns>
            /// If the function succeeds, the return value is an open handle to the specified image file.
            /// If the function fails, the return value is NULL. To obtain extended error information, call the GetLastError function.
            /// </returns>
            /// <remarks>To obtain information during an image capture, see the WIMRegisterMessageCallback function.</remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            public static extern WimHandle WIMCaptureImage(WimHandle hWim, string pszPath, DWORD dwCaptureFlags);
        }
    }
}