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
        /// Gets a collection of images contained in the specified .wim or .vhd file.
        /// </summary>
        /// <param name="imageFilePath">// Clean up</param>
        /// <returns>A <see cref="DismImageInfoCollection" /> object containing a collection of <see cref="DismImageInfo" /> objects.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismImageInfoCollection GetImageInfo(string imageFilePath)
        {
            int hresult = NativeMethods.DismGetImageInfo(imageFilePath, out IntPtr imageInfoPtr, out UInt32 imageInfoCount);

            try
            {
                DismUtilities.ThrowIfFail(hresult);

                return new DismImageInfoCollection(imageInfoPtr, imageInfoCount);
            }
            finally
            {
                // Clean up
                Delete(imageInfoPtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Returns an array of DismImageInfo Structure elements that describe the images in a .wim or .vhd file.
            /// </summary>
            /// <param name="imageFilePath">A relative or absolute path to a .wim or .vhd file.</param>
            /// <param name="imageInfo">A pointer to the address of an array of DismImageInfo Structure objects.</param>
            /// <param name="count">The number of DismImageInfo structures that are returned.</param>
            /// <returns>S_OK on success.</returns>
            /// <remarks>The array of DismImageInfo structures are allocated by DISM API on the heap.
            ///
            /// Important
            /// You must call the DismDelete Function, passing the ImageInfo pointer, to free the resources associated with the DismImageInfo structures.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824767.aspx" />
            /// HRESULT WINAPI DismGetImageInfo(_In_ PCWSTR ImageFilePath, _Outptr_result_buffer_(*Count) DismImageInfo** ImageInfo, _Out_ UINT* Count);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismGetImageInfo(string imageFilePath, out IntPtr imageInfo, out UInt32 count);
        }
    }
}