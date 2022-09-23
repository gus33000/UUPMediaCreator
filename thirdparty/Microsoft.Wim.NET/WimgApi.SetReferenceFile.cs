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
    /// Specifies the mode to use when setting a reference file.
    /// </summary>
    public enum WimSetReferenceMode : uint
    {
        /// <summary>
        /// The specified .wim file is appended to the current list.
        /// </summary>
        Append = WimgApi.WIM_REFERENCE_APPEND,

        /// <summary>
        /// The specified .wim file becomes the only item in the list.
        /// </summary>
        Replace = WimgApi.WIM_REFERENCE_REPLACE,
    }

    /// <summary>
    /// Represents options when setting a reference file.
    /// </summary>
    [Flags]
    public enum WimSetReferenceOptions : uint
    {
        /// <summary>
        /// No options are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// The .wim file is opened in a mode that enables simultaneous reading and writing.
        /// </summary>
        ShareWrite = WimgApi.WIM_FLAG_SHARE_WRITE,

        /// <summary>
        /// Data integrity information is generated for new files, verified, and updated for existing files.
        /// </summary>
        Verify = WimgApi.WIM_FLAG_VERIFY,
    }

    public static partial class WimgApi
    {
        /// <summary>
        /// Enables the <see cref="ApplyImage"/> and <see cref="CaptureImage"/> methods to use alternate .wim files for file resources. This can enable optimization of storage when multiple images are captured with similar data.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim (Windows image) file returned by the <see cref="CreateFile"/> method.</param>
        /// <param name="path">The path of the .wim file to be added to the reference list.</param>
        /// <param name="mode">Specifies whether the .wim file is added to the reference list or replaces other entries.</param>
        /// <param name="options">Specifies options when adding the .wim file to the reference list.</param>
        /// <exception cref="ArgumentNullException">wimHandle is null
        /// -or-
        /// mode is not WimSetReferenceMode.Replace and path is null.</exception>
        /// <exception cref="FileNotFoundException">path does not exist.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>If <c>null</c> is passed in for the path parameter and <see cref="WimSetReferenceMode.Replace"/> is passed for the mode parameter, then the reference list is completely cleared, and no file resources are extracted during the <see cref="ApplyImage"/> method.</remarks>
        public static void SetReferenceFile(WimHandle wimHandle, string path, WimSetReferenceMode mode, WimSetReferenceOptions options)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if not replacing and path is null
            if (mode != WimSetReferenceMode.Replace && path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // See if path does not exist
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find part of the path '{path}'");
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMSetReferenceFile(wimHandle, path, (DWORD)mode | (DWORD)options))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Enables the WIMApplyImage and WIMCaptureImage functions to use alternate .wim files for file resources. This can enable
            /// optimization of storage when multiple images are captured with similar data.
            /// </summary>
            /// <param name="hWim">A handle to a .wim (Windows image) file returned by the WIMCreateFile function.</param>
            /// <param name="pszPath">
            /// A pointer to a null-terminated string containing the path of the .wim file to be added to the
            /// reference list.
            /// </param>
            /// <param name="dwFlags">
            /// Specifies how the .wim file is added to the reference list. This parameter must include one of
            /// the following two values.
            /// </param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call the GetLastError
            /// function.
            /// </returns>
            /// <remarks>
            /// If NULL is passed in for the pszPath parameter and WIM_REFERENCE_REPLACE is passed for the dwFlags parameter,
            /// then the reference list is completely cleared, and no file resources are extracted during the WIMApplyImage function.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMSetReferenceFile(WimHandle hWim, string pszPath, DWORD dwFlags);
        }
    }
}