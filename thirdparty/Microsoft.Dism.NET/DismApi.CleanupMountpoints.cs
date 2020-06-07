// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Removes files and releases resources associated with corrupted or invalid mount paths.
        /// </summary>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void CleanupMountpoints()
        {
            int hresult = NativeMethods.DismCleanupMountpoints();

            DismUtilities.ThrowIfFail(hresult);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Removes files and releases resources associated with corrupted or invalid mount paths.
            /// </summary>
            /// <returns>Returns S_OK on success.</returns>
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824743.aspx" />
            /// HRESULT WINAPI DismCleanupMountpoints( );
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismCleanupMountpoints();
        }
    }
}