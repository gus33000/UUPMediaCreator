// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.ComponentModel;
using System.Runtime.InteropServices;

using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Removes images from all directories where they have been previously mounted.
        /// </summary>
        /// <param name="removeAll"><c>true</c> to removes all mounted images, whether actively mounted or not, otherwise <c>false</c> to remove only images that are not actively mounted.</param>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void DeleteImageMounts(bool removeAll)
        {
            // Call the native function
            if (!WimgApi.NativeMethods.WIMDeleteImageMounts(removeAll ? WimgApi.WIM_DELETE_MOUNTS_ALL : 0))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Removes images from all directories where they have been previously mounted.
            /// </summary>
            /// <param name="dwDeleteFlags">Specifies which types of images are to be removed.</param>
            /// <returns>
            /// If the function succeeds, the return value is nonzero.
            /// If the function fails, the return value is zero. To obtain extended error information, call the GetLastError function.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMDeleteImageMounts(DWORD dwDeleteFlags);
        }
    }
}