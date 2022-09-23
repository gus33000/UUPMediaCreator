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
        /// Removes an image from within a .wim (Windows image) file so it cannot be accessed. However, the file resources are still available for use by the <see cref="SetReferenceFile"/> method.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> to a .wim file returned by the <see cref="CreateFile"/> method. This handle must have <see cref="WimFileAccess.Write"/> access to delete the image. Split .wim files are not supported and the .wim file cannot have any open images.</param>
        /// <param name="index">The one-based index of the image to delete. A .wim file might have multiple images stored within it.</param>
        /// <exception cref="ArgumentNullException">wimHandle is null.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is less than 1
        /// -or-
        /// <paramref name="index"/> is greater than the number of images in the Windows® imaging file.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>You must call the <see cref="SetTemporaryPath"/> method before calling the <see cref="DeleteImage"/> method so the image metadata for the image can be extracted and processed from the temporary location.</remarks>
        public static void DeleteImage(WimHandle wimHandle, int index)
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
            if (!WimgApi.NativeMethods.WIMDeleteImage(wimHandle, (DWORD)index))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Removes an image from within a .wim (Windows image) file so it cannot be accessed. However, the file resources are
            /// still available for use by the WIMSetReferenceFile function.
            /// </summary>
            /// <param name="hWim">
            /// The handle to a .wim file returned by the WIMCreateFile function. This handle must have
            /// WIM_GENERIC_WRITE access to delete the image. Split .wim files are not supported and the .wim file cannot have any open
            /// images.
            /// </param>
            /// <param name="dwImageIndex">
            /// Specifies the one-based index of the image to delete. A .wim file might have multiple images
            /// stored within it.
            /// </param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call GetLastError.
            /// If there is only one image in the specified .wim file, then the WIMDeleteImage function will fail and set the LastError
            /// to ERROR_ACCESS_DENIED.
            /// </returns>
            /// <remarks>
            /// You must call the WIMSetTemporaryPath function before calling the WIMDeleteImage function so the image
            /// metadata for the image can be extracted and processed from the temporary location.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMDeleteImage(WimHandle hWim, DWORD dwImageIndex);
        }
    }
}