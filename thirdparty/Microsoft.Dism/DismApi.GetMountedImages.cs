// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Gets a list of images that are currently mounted.
        /// </summary>
        /// <returns>A <see cref="DismMountedImageInfoCollection" /> object containing a collection of <see cref="DismMountedImageInfo" /> objects.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismMountedImageInfoCollection GetMountedImages()
        {
            int hresult = NativeMethods.DismGetMountedImageInfo(out IntPtr mountedImageInfoPtr, out UInt32 mountedImageInfoCount);

            try
            {
                DismUtilities.ThrowIfFail(hresult);

                return new DismMountedImageInfoCollection(mountedImageInfoPtr, mountedImageInfoCount);
            }
            finally
            {
                // Clean up
                Delete(mountedImageInfoPtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Returns an array of DismMountedImageInfo Structure elements that describe the images that are mounted currently.
            /// </summary>
            /// <param name="mountedImageInfo">A pointer to the address of an array of DismMountedImageInfo Structure objects.</param>
            /// <param name="count">The number of DismMountedImageInfo structures that are returned.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>Only images mounted using the DISM infrastructure will be returned. If a .vhd file is mounted outside of DISM, such as with the DiskPart tool, this call will not return information about that image. You must use the DismMountImage Function to mount the image.
            ///
            /// The array of DismMountedImageInfo structures are allocated by the DISM API on the heap.
            ///
            /// You must call the DismDelete Function, passing the ImageInfo pointer, to free the resources associated with the DismImageInfo structures.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824745.aspx" />
            /// HRESULT WINAPI DismGetMountedImageInfo(_Outptr_result_buffer_(*Count) DismMountedImageInfo** MountedImageInfo, _Out_ UINT* Count);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismGetMountedImageInfo(out IntPtr mountedImageInfo, out UInt32 count);
        }
    }
}