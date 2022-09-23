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
        /// Gets the drivers in an image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the <see cref="OpenOfflineSession(string)" /> method.</param>
        /// <param name="allDrivers">true or false to specify to retrieve all drivers or just out-of-box drivers.</param>
        /// <returns>A <see cref="DismDriverPackageCollection" /> object containing a collection of <see cref="DismDriverPackage" /> objects.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismDriverPackageCollection GetDrivers(DismSession session, bool allDrivers)
        {
            int hresult = NativeMethods.DismGetDrivers(session, allDrivers, out IntPtr driverPackagePtr, out UInt32 driverPackageCount);

            try
            {
                DismUtilities.ThrowIfFail(hresult, session);

                return new DismDriverPackageCollection(driverPackagePtr, driverPackageCount);
            }
            finally
            {
                // Clean up
                Delete(driverPackagePtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Lists the drivers in an image.
            /// </summary>
            /// <param name="Session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="AllDrivers">A Boolean value specifying which drivers to retrieve.</param>
            /// <param name="DriverPackage">A pointer to the address of an array of DismDriverPackage Structure objects.</param>
            /// <param name="Count">The number of DismDriverPackage structures that were returned.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824784.aspx" />
            /// HRESULT WINAPI DismGetDrivers (_In_ DismSession Session, _In_ BOOL AllDrivers, _Outptr_result_buffer_(*Count) DismDriverPackage** DriverPackage, _Out_ UINT* Count);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismGetDrivers(DismSession Session, [MarshalAs(UnmanagedType.Bool)] bool AllDrivers, out IntPtr DriverPackage, out UInt32 Count);
        }
    }
}