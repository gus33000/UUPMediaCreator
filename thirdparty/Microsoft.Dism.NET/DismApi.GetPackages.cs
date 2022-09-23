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
        /// Gets a collection of each package in an image and provides basic information about each package, such as the package name and type of package.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DismSession must be associated with an image.</param>
        /// <returns>A <see cref="DismPackageCollection" /> object containing a collection of <see cref="DismPackage" />objects.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismPackageCollection GetPackages(DismSession session)
        {
            int hresult = NativeMethods.DismGetPackages(session, out IntPtr packagePtr, out UInt32 packageCount);

            try
            {
                DismUtilities.ThrowIfFail(hresult, session);

                return new DismPackageCollection(packagePtr, packageCount);
            }
            finally
            {
                // Clean up
                Delete(packagePtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Lists each package in an image and provides basic information about each package, such as the package name and type of package.
            /// </summary>
            /// <param name="dismSession">A valid DISMSession. The DismSession must be associated with an image.</param>
            /// <param name="packageInfo">A pointer to the array of DismPackage Structure objects.</param>
            /// <param name="count">The number of DismPackage structures that are returned.</param>
            /// <returns>Returns S_OK on success.
            ///
            /// Package points to an array of DismPackage Structure objects. You can manipulate this array using normal array notation in order to get information about each package in the image.</returns>
            /// <remarks>When you are finished with the Package array, you must remove it by using the DismDelete Function.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824759.aspx" />
            /// HRESULT WINAPI DismGetPackages (_In_ DismSession Session, _Outptr_result_buffer_(*Count) DismPackage** Package, _Out_ UINT* Count);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismGetPackages(DismSession dismSession, out IntPtr packageInfo, out UInt32 count);
        }
    }
}