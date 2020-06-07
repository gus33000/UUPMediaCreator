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
        /// Loads a volume image from a Windows® image (.wim) file.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim file returned by the <see cref="CreateFile"/> method.</param>
        /// <param name="index">The one-based index of the image to load. An image file may store multiple images.</param>
        /// <returns>A <see cref="WimHandle"/> representing the volume image.</returns>
        /// <exception cref="ArgumentNullException">wimHandle is null.</exception>
        /// <exception cref="IndexOutOfRangeException">index is less than 1
        /// -or-
        /// index is greater than the number of images in the Windows® imaging file.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>You must call the <see cref="SetTemporaryPath"/> method before calling the <see cref="LoadImage"/> method so the image metadata can be extracted and processed from the temporary location.</remarks>
        public static WimHandle LoadImage(WimHandle wimHandle, int index)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if the specified index is valid
            if (index < 1 || index > WimgApi.GetImageCount(wimHandle))
            {
                throw new IndexOutOfRangeException($"There is no image at index {index}.");
            }

            // Call the native function
            WimHandle imageHandle = WimgApi.NativeMethods.WIMLoadImage(wimHandle, (DWORD)index);

            if (imageHandle == null || imageHandle.IsInvalid)
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }

            // Return the image handle
            return imageHandle;
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Loads a volume image from a Windows® image (.wim) file.
            /// </summary>
            /// <param name="hWim">A handle to a .wim file returned by the WIMCreateFile function.</param>
            /// <param name="dwImageIndex">Specifies the one-based index of the image to load. An image file may store multiple images.</param>
            /// <returns>
            /// If the function succeeds, then the return value is a handle to an object representing the volume image. If the
            /// function fails, then the return value is NULL. To obtain extended error information, call the GetLastError function.
            /// </returns>
            /// <remarks>
            /// You must call the WIMSetTemporaryPath function before calling the WIMLoadImage function so the image metadata can be
            /// extracted and processed from the temporary location.
            /// Use the WIMCloseHandle function to unload the volume image.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            public static extern WimHandle WIMLoadImage(WimHandle hWim, DWORD dwImageIndex);
        }
    }
}