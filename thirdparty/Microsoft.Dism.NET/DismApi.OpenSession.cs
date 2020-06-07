// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Associates an offline Windows image with a DISMSession.
        /// </summary>
        /// <param name="imagePath">An absolute or relative path to the root directory of an offline Windows image or an absolute or relative path to the root directory of a mounted Windows image.</param>
        /// <returns>A <see cref="DismSession" /> object.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismSession OpenOfflineSession(string imagePath)
        {
            return OpenOfflineSession(imagePath, null, null);
        }

        /// <summary>
        /// Associates an offline Windows image with a DISMSession.
        /// </summary>
        /// <param name="imagePath">An absolute or relative path to the root directory of an offline Windows image or an absolute or relative path to the root directory of a mounted Windows image.</param>
        /// <param name="windowsDirectory">A relative or absolute path to the Windows directory. The path is relative to the mount point.</param>
        /// <param name="systemDrive">The letter of the system drive that contains the boot manager. If SystemDrive is NULL, the default value of the drive containing the mount point is used.</param>
        /// <returns>A <see cref="DismSession" /> object.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismSession OpenOfflineSession(string imagePath, string windowsDirectory, string systemDrive)
        {
            return OpenSession(imagePath, windowsDirectory, systemDrive);
        }

        /// <summary>
        /// Associates an online Windows image with a DISMSession.
        /// </summary>
        /// <returns>A <see cref="DismSession" /> object.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismSession OpenOnlineSession()
        {
            return OpenSession(DISM_ONLINE_IMAGE, null, null);
        }

        /// <summary>
        /// Associates an offline or online Windows image with a DISMSession.
        /// </summary>
        /// <param name="imagePath">An absolute or relative path to the root directory of an offline Windows image, an absolute or relative path to the root directory of a mounted Windows image, or DISM_ONLINE_IMAGE to associate with the online Windows installation.</param>
        /// <param name="windowsDirectory">A relative or absolute path to the Windows directory. The path is relative to the mount point.</param>
        /// <param name="systemDrive">The letter of the system drive that contains the boot manager. If SystemDrive is NULL, the default value of the drive containing the mount point is used.</param>
        /// <returns>A <see cref="DismSession" /> object.</returns>
        private static DismSession OpenSession(string imagePath, string windowsDirectory, string systemDrive)
        {
            return new DismSession(imagePath, windowsDirectory, systemDrive);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Associates an offline or online Windows image with a DISMSession.
            /// </summary>
            /// <param name="imagePath">Set ImagePath to one of the following values:
            ///
            /// An absolute or relative path to the root directory of an offline Windows image.
            ///
            /// An absolute or relative path to the root directory of a mounted Windows image. You can mount the image before calling DismOpenSession by using an external tool or by using the DismMountImage Function.
            ///
            /// DISM_ONLINE_IMAGE to associate the DISMSession with the online Windows installation.</param>
            /// <param name="windowsDirectory">Optional. A relative or absolute path to the Windows directory. The path is relative to the mount point. If the value of WindowsDirectory is NULL, the default value of Windows is used.
            ///
            /// The WindowsDirectory parameter cannot be used when the ImagePath parameter is set to DISM_ONLINE_IMAGE.</param>
            /// <param name="systemDrive">Optional. The letter of the system drive that contains the boot manager. If SystemDrive is NULL, the default value of the drive containing the mount point is used.
            ///
            /// The SystemDrive parameter cannot be used when the ImagePath parameter is set to DISM_ONLINE_IMAGE.</param>
            /// <param name="session">A pointer to a valid DISMSession. The DISMSession will be associated with the image after this call is successfully completed.</param>
            /// <returns>Returns S_OK on success.
            ///
            /// Returns DISMAPI_E_ALREADY_ASSOCIATED if the DISMSession already has an image associated with it.
            ///
            /// Returns a Win32 error code mapped to an HRESULT for other errors.</returns>
            /// <remarks>The DISMSession can be used to service the image after the DISMOpenSession call is successfully completed. The DISMSession must be shut down by calling the DismCloseSession Function.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824800.aspx" />
            /// HRESULT WINAPI DismOpenSession(_In_ PCWSTR ImagePath, _In_opt_ PCWSTR WindowsDirectory, _In_opt_ WCHAR* SystemDrive, _Out_ DismSession* Session);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismOpenSession(string imagePath, string windowsDirectory, string systemDrive, out IntPtr session);
        }
    }
}