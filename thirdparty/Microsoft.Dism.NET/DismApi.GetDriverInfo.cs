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
        /// Gets information about an .inf file in a specified image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the <see cref="OpenOfflineSession(string)" /> method.</param>
        /// <param name="driverPath">A relative or absolute path to the driver .inf file.</param>
        /// <returns>A <see cref="DismDriverCollection" /> object containing a collection of <see cref="DismDriver" /> objects.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismDriverCollection GetDriverInfo(DismSession session, string driverPath)
        {
            int hresult = NativeMethods.DismGetDriverInfo(session, driverPath, out IntPtr driverInfoPtr, out UInt32 driverInfoCount, out IntPtr driverPackagePtr);

            try
            {
                DismUtilities.ThrowIfFail(hresult, session);

                return new DismDriverCollection(driverInfoPtr, driverInfoCount);
            }
            finally
            {
                // Clean up
                Delete(driverInfoPtr);
                Delete(driverPackagePtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Gets information about an .inf file in a specified image.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="driverPath">A relative or absolute path to the driver .inf file.</param>
            /// <param name="driver">A pointer to the address of an array of DismDriver Structure objects.</param>
            /// <param name="count">The number of DismDriver structures that were returned.</param>
            /// <param name="driverPackage">Optional. A pointer to the address of a DismDriverPackage Structure object.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>This function returns information about the .inf file installed on the image. The driver associated with the .inf file may or may not be installed in the image.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824733.aspx" />
            /// HRESULT WINAPI DismGetDriverInfo (_In_ DismSession Session, _In_ PCWSTR DriverPath, _Outptr_result_buffer_(*Count) DismDriver** Driver, _Out_ UINT* Count, _Out_opt_ DismDriverPackage** DriverPackage);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismGetDriverInfo(DismSession session, string driverPath, out IntPtr driver, out UInt32 count, out IntPtr driverPackage);
        }
    }
}