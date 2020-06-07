// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Remounts a Windows image from the .wim or .vhd file that was previously mounted at the path specified by MountPath.  Use the DismOpenSession Function to associate the image with a DISMSession after it is remounted.
        ///
        /// You can use the DismRemountImage function when the image is in the DismMountStatusNeedsRemount state, as described by the DismMountStatus Enumeration. The image may enter this state if it is mounted and then a reboot occurs.
        /// </summary>
        /// <param name="mountPath">A relative or absolute path to the mount directory of the image.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void RemountImage(string mountPath)
        {
            int hresult = NativeMethods.DismRemountImage(mountPath);

            DismUtilities.ThrowIfFail(hresult);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Remounts a Windows image from the .wim or .vhd file that was previously mounted at the path specified by MountPath. Use the DismOpenSession Function to associate the image with a DISMSession after it is remounted.
            ///
            /// You can use the DismRemountImage function when the image is in the DismMountStatusNeedsRemount state, as described by the DismMountStatus Enumeration. The image may enter this state if it is mounted and then a reboot occurs.
            /// </summary>
            /// <param name="mountPath">A relative or absolute path to the mount directory of the image.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824778.aspx" />
            /// HRESULT WINAPI DismRemountImage(_In_ PCWSTR MountPath);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismRemountImage(string mountPath);
        }
    }
}