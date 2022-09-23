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
        /// Releases resources held by a structure or an array of structures returned by other DISM API functions.
        /// </summary>
        /// <param name="handle">A pointer to the structure, or array of structures, to be deleted. The structure must have been returned by an earlier call to a DISM API function.</param>
        private static void Delete(IntPtr handle)
        {
            // Call the native function
            NativeMethods.DismDelete(handle);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Releases resources held by a structure or an array of structures returned by other DISM API functions.
            /// </summary>
            /// <param name="dismStructure">A pointer to the structure, or array of structures, to be deleted. The structure must have been returned by an earlier call to a DISM API function.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>All structures that are returned by DISM API functions are allocated on the heap. The client must not delete or free these structures directly. Instead, the client should call DismDelete and pass in the pointer that was returned by the earlier DISM API call.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824768.aspx" />
            /// HRESULT WINAPI DismDelete(_In_ VOID* DismStructure);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismDelete(IntPtr dismStructure);
        }
    }
}