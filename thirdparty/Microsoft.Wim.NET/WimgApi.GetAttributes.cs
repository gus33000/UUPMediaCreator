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
        /// Returns the number of volume images stored in an image file.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim file returned by <see cref="CreateFile"/>.</param>
        /// <returns>A <see cref="WimInfo"/> object containing information about the image file.</returns>
        /// <exception cref="ArgumentNullException">wimHandle is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static WimInfo GetAttributes(WimHandle wimHandle)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // Calculate the size of the buffer needed
            uint wimInfoSize = (DWORD)Marshal.SizeOf(typeof(WimgApi.WIM_INFO));

            // Allocate a buffer to receive the native struct
            IntPtr wimInfoPtr = Marshal.AllocHGlobal((int)wimInfoSize);

            try
            {
                // Call the native function
                if (!WimgApi.NativeMethods.WIMGetAttributes(wimHandle, wimInfoPtr, wimInfoSize))
                {
                    // Throw a Win32Exception based on the last error code
                    throw new Win32Exception();
                }

                // Return a new instance of a WimInfo class which will marshal the struct
                return new WimInfo(wimInfoPtr);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(wimInfoPtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Returns the number of volume images stored in an image file.
            /// </summary>
            /// <param name="hWim">The handle to a .wim file returned by WIMCreateFile.</param>
            /// <param name="pWimInfo">A pointer to a WIM_INFO structure that is returned with information about the .wim file.</param>
            /// <param name="cbWimInfo">A DWORD value indicating the size of the pWimInfo buffer in which it passes.</param>
            /// <returns>
            /// If the function succeeds, then the return value is non-zero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call GetLastError.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMGetAttributes(WimHandle hWim, IntPtr pWimInfo, DWORD cbWimInfo);
        }
    }
}