// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Sets the location where temporary imaging files are to be stored.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim file returned by the <see cref="CreateFile"/> method.</param>
        /// <param name="path">The path where temporary image (.wim) files are to be stored during capture or application. This is the directory where the image is captured or applied.</param>
        /// <exception cref="ArgumentNullException">wimHandle or path is null.</exception>
        /// <exception cref="DirectoryNotFoundException">path does not exist.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void SetTemporaryPath(WimHandle wimHandle, string path)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if path is null
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // See if path does not exist
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Could not find part of the path '{path}'");
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMSetTemporaryPath(wimHandle, path))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Sets the location where temporary imaging files are to be stored.
            /// </summary>
            /// <param name="hWim">A handle to a .wim file returned by the WIMCreateFile function.</param>
            /// <param name="pszPath">
            /// A pointer to a null-terminated string, indicating the path where temporary image (.wim) files are
            /// to be stored during capture or application. This is the directory where the image is captured or applied.
            /// </param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call GetLastError.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMSetTemporaryPath(WimHandle hWim, string pszPath);
        }
    }
}