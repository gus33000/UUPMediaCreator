// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Removes an out-of-box driver from an offline image.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="driverPath">The published file name of the driver that has been added to the image, for example OEM1.inf. You can use the GetDrivers method to get the published name of the driver.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RemoveDriver(DismSession session, string driverPath)
        {
            int hresult = NativeMethods.DismRemoveDriver(session, driverPath);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Removes an out-of-box driver from an offline image.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="driverPath">The published file name of the driver that has been added to the image, for example OEM1.inf. You can use the DismGetDrivers Function to get the published name of the driver.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>This function only supports offline images.
            ///
            /// Important
            /// Removing a boot-critical driver can make the offline Windows image unable to boot.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824729.aspx" />
            /// HRESULT WINAPI DismRemoveDriver (_In_ DismSession Session, _In_ PCWSTR DriverPath);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            public static extern int DismRemoveDriver(DismSession session, string driverPath);
        }
    }
}