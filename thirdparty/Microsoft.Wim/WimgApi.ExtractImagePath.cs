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
        /// Extracts a file from within a Windows® image (.wim) file to a specified location.
        /// </summary>
        /// <param name="imageHandle">A <see cref="WimHandle"/> opened by the <see cref="LoadImage"/> method.</param>
        /// <param name="sourceFile">The path to a file inside the image.</param>
        /// <param name="destinationFile">The full file path of the directory where the image path is to be extracted.</param>
        /// <exception cref="ArgumentNullException">imageHandle, sourceFile, or destinationFile is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void ExtractImagePath(WimHandle imageHandle, string sourceFile, string destinationFile)
        {
            // See if imageHandle is null
            if (imageHandle == null)
            {
                throw new ArgumentNullException(nameof(imageHandle));
            }

            // See if sourceFile is null
            if (sourceFile == null)
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            // See if destinationFile is null
            if (destinationFile == null)
            {
                throw new ArgumentNullException(nameof(destinationFile));
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMExtractImagePath(imageHandle, sourceFile, destinationFile, 0))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Extracts a file from within a Windows® image (.wim) file to a specified location.
            /// </summary>
            /// <param name="hImage">A handle to an image opened by the WIMLoadImage function.</param>
            /// <param name="pszImagePath">A pointer to a file path inside the image.</param>
            /// <param name="pszDestinationPath">
            /// A pointer to the full file path of the directory where the image path is to be
            /// extracted.
            /// </param>
            /// <param name="dwExtractFlags">Reserved. Must be zero.</param>
            /// <returns>
            /// If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To
            /// obtain extended error information, call the GetLastError function.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMExtractImagePath(WimHandle hImage, string pszImagePath, string pszDestinationPath, DWORD dwExtractFlags);
        }
    }
}