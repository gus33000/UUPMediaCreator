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
        /// Gets extended information about a package.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the OpenImageSession Function.</param>
        /// <param name="packageName">The name of the package to get information about.</param>
        /// <returns>A <see cref="DismPackageInfo" /> object.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismPackageInfo GetPackageInfoByName(DismSession session, string packageName)
        {
            return GetPackageInfo(session, packageName, DismPackageIdentifier.Name);
        }

        /// <summary>
        /// Gets extended information about a package.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the OpenImageSession Function.</param>
        /// <param name="packagePath">An absolute path to a .cab file or to a folder containing an expanded package.</param>
        /// <returns>A <see cref="DismPackageInfo" /> object.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismPackageInfo GetPackageInfoByPath(DismSession session, string packagePath)
        {
            return GetPackageInfo(session, packagePath, DismPackageIdentifier.Path);
        }

        /// <summary>
        /// Gets extended information about a package.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the OpenImageSession Function.</param>
        /// <param name="identifier">Either an absolute path to a .cab file or the package name, depending on the PackageIdentifier parameter value.</param>
        /// <param name="packageIdentifier">A valid DismPackageIdentifier Enumeration value.</param>
        /// <returns>A <see cref="DismPackageInfo" /> object.</returns>
        private static DismPackageInfo GetPackageInfo(DismSession session, string identifier, DismPackageIdentifier packageIdentifier)
        {
            int hresult = NativeMethods.DismGetPackageInfo(session, identifier, packageIdentifier, out IntPtr packageInfoPtr);

            try
            {
                DismUtilities.ThrowIfFail(hresult, session);

                // Return a new DismPackageInfo object with a reference to the pointer
                return new DismPackageInfo(packageInfoPtr);
            }
            finally
            {
                // Clean up
                Delete(packageInfoPtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Retrieves all of the standard package properties as the DismGetPackages Function, as well as more specific package information and custom properties.
            /// </summary>
            /// <param name="dismSession">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="identifier">Either an absolute path to a .cab file or the package name, depending on the PackageIdentifier parameter value.</param>
            /// <param name="packageIdentifier">A valid DismPackageIdentifier Enumeration value.</param>
            /// <param name="packageInfo">A pointer to the address of an array of DismPackageInfo Structure objects.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>You can use this function to determine whether a package is applicable to the specified image. The DismPackageInfo Structure contains an Applicable field, which is a Boolean that returns TRUE if the package is applicable and FALSE if the package is not applicable to the specified image.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824801.aspx" />
            /// HRESULT WINAPI DismGetPackageInfo (_In_ DismSession Session, _In_ PCWSTR Identifier, _In_ DismPackageIdentifier PackageIdentifier, _Out_ DismPackageInfo** PackageInfo);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismGetPackageInfo(DismSession dismSession, string identifier, DismPackageIdentifier packageIdentifier, out IntPtr packageInfo);
        }
    }
}