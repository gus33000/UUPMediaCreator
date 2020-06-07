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
        /// Gets information about a mounted image of the specified mounted image handle.
        /// </summary>
        /// <param name="imageHandle">A <see cref="WimHandle"/> of an image that has been mounted.</param>
        /// <returns>A <see cref="WimMountInfo"/> object containing information about the mounted image.</returns>
        /// <exception cref="ArgumentNullException">imageHandle is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static WimMountInfo GetMountedImageInfoFromHandle(WimHandle imageHandle)
        {
            // See if imageHandle is null
            if (imageHandle == null)
            {
                throw new ArgumentNullException(nameof(imageHandle));
            }

            // Calculate the size of the buffer needed
            int mountInfoSize = Marshal.SizeOf(typeof(WimgApi.WIM_MOUNT_INFO_LEVEL1));

            // Allocate a buffer for the native function
            IntPtr mountInfoPtr = Marshal.AllocHGlobal(mountInfoSize);

            try
            {
                // Call the native function (the buffer may be too small)
                if (!WimgApi.NativeMethods.WIMGetMountedImageInfoFromHandle(imageHandle, WimMountInfo.MountInfoLevel, mountInfoPtr, (DWORD)mountInfoSize, out DWORD returnLength))
                {
                    // See if the return value isn't ERROR_INSUFFICIENT_BUFFER
                    if (Marshal.GetLastWin32Error() != 122)
                    {
                        throw new Win32Exception();
                    }

                    // Re-allocate the buffer to the correct size
                    Marshal.ReAllocHGlobal(mountInfoPtr, (IntPtr)returnLength);

                    // Call the native function a second time so it can fill buffer with a struct
                    if (!WimgApi.NativeMethods.WIMGetMountedImageInfoFromHandle(imageHandle, WimMountInfo.MountInfoLevel, mountInfoPtr, returnLength, out returnLength))
                    {
                        // Throw a Win32Exception based on the last error code
                        throw new Win32Exception();
                    }
                }

                // Return a WimMountInfo object which will marshal the pointer to a struct
                return new WimMountInfo(mountInfoPtr);
            }
            finally
            {
                // Free the native memory
                Marshal.FreeHGlobal(mountInfoPtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Queries the state of a mounted image handle.
            /// </summary>
            /// <param name="hImage">A handle to an image that has been mounted</param>
            /// <param name="fInfoLevelId">A class of attribute information to retrieve.</param>
            /// <param name="pMountInfo">
            /// Pointer to a variable that receives mounted image structures. The size of the information
            /// written varies depending on the type of structured defined by the fInfoLevelId.
            /// </param>
            /// <param name="cbMountInfoLength">The size of the buffer pointed to by the pMountInfo parameter, in bytes</param>
            /// <param name="pcbReturnLength">
            /// A pointer to a variable which contains the result of a function call that returns the
            /// size of the requested information. If the function was successful, this is the size of the information written to the
            /// buffer pointed to by the pMountInfo parameter; if the buffer was too small, then this is the minimum size of buffer
            /// needed to receive the information successfully.
            /// </param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero. If the function fails, then the return value is
            /// zero. To obtain extended error information, call the GetLastError function. If the buffer specified by the
            /// cbMountInfoLength parameter is not large enough to hold the data, the function sets the value of LastError to
            /// ERROR_INSUFFICIENT_BUFFER and stores the required buffer size in the variable pointed to by pcbReturnLength.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMGetMountedImageInfoFromHandle(WimHandle hImage, WimMountedImageInfoLevels fInfoLevelId, IntPtr pMountInfo, DWORD cbMountInfoLength, out DWORD pcbReturnLength);
        }
    }
}