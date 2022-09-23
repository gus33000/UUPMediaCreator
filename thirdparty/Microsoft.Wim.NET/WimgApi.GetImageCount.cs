// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Returns the number of volume images stored in a Windows® image (.wim) file.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim file returned by the <see cref="CreateFile"/> method.</param>
        /// <returns>The number of images in the .wim file. If this value is zero, then the image file is invalid or does not contain any images that can be applied.</returns>
        public static int GetImageCount(WimHandle wimHandle)
        {
            // Return the value from the native function
            return (int)WimgApi.NativeMethods.WIMGetImageCount(wimHandle);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Returns the number of volume images stored in a Windows® image (.wim) file.
            /// </summary>
            /// <param name="hWim">A handle to a .wim file returned by the WIMCreateFile function.</param>
            /// <returns>
            /// The return value is the number of images in the .wim file. If this value is zero, then the image file is
            /// invalid or does not contain any images that can be applied.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            public static extern DWORD WIMGetImageCount(WimHandle hWim);
        }
    }
}