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
        /// Marks the image with the given image index as bootable.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a Windows® image (.wim) file returned by the <see cref="CreateFile"/> method.</param>
        /// <param name="imageIndex">The one-based index of the image to load. An image file can store multiple images.</param>
        /// <exception cref="ArgumentNullException">wimHandle is null.</exception>
        /// <exception cref="IndexOutOfRangeException">index is less than 1 or greater than the number of images in the Windows® image file.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>If imageIndex is zero, then none of the images in the .wim file are marked for boot. At any time, only one image in a .wim file can be set to be bootable.</remarks>
        public static void SetBootImage(WimHandle wimHandle, int imageIndex)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if the specified index is valid
            if (imageIndex < 1 || imageIndex > WimgApi.GetImageCount(wimHandle))
            {
                throw new IndexOutOfRangeException($"There is no image at index {imageIndex}.");
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMSetBootImage(wimHandle, (DWORD)imageIndex))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Marks the image with the given image index as bootable.
            /// </summary>
            /// <param name="hWim">A handle to a Windows® image (.wim) file returned by the WIMCreateFile function.</param>
            /// <param name="dwImageIndex">The one-based index of the image to load. An image file can store multiple images.</param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call the GetLastError
            /// function.
            /// </returns>
            /// <remarks>
            /// If the input value for the dwImageIndex is zero, then none of the images in the .wim file are marked for boot.
            /// At any time, only one image in a .wim file can be set to be bootable.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMSetBootImage(WimHandle hWim, DWORD dwImageIndex);
        }
    }
}