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
        /// Gets all the features in an image, regardless of whether the features are enabled or disabled.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <returns>A <see cref="DismFeatureCollection" /> object containing a collection of <see cref="DismFeature" /> objects.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismFeatureCollection GetFeatures(DismSession session)
        {
            return GetFeatures(session, string.Empty, DismPackageIdentifier.None);
        }

        /// <summary>
        /// Gets all the features in an image, regardless of whether the features are enabled or disabled.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="packageName">The name of the package to get features of.</param>
        /// <returns>A <see cref="DismFeatureCollection" /> object containing a collection of <see cref="DismFeature" /> objects.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismFeatureCollection GetFeaturesByPackageName(DismSession session, string packageName)
        {
            return GetFeatures(session, packageName, DismPackageIdentifier.Name);
        }

        /// <summary>
        /// Gets all the features in an image, regardless of whether the features are enabled or disabled.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// /// <param name="packagePath">The path of the package to get features of.</param>
        /// <returns>A <see cref="DismFeatureCollection" /> object containing a collection of <see cref="DismFeature" /> objects.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismFeatureCollection GetFeaturesByPackagePath(DismSession session, string packagePath)
        {
            return GetFeatures(session, packagePath, DismPackageIdentifier.Path);
        }

        /// <summary>
        /// Gets all the features in an image, regardless of whether the features are enabled or disabled.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="identifier">Optional. Either an absolute path to a .cab file or the package name, depending on the packageIdentifier parameter value.</param>
        /// <param name="packageIdentifier">A valid DismPackageIdentifier Enumeration value.</param>
        /// <returns>A <see cref="DismFeatureCollection" /> object containing a collection of <see cref="DismFeature" /> objects.</returns>
        private static DismFeatureCollection GetFeatures(DismSession session, string identifier, DismPackageIdentifier packageIdentifier)
        {
            int hresult = NativeMethods.DismGetFeatures(session, identifier, packageIdentifier, out IntPtr featurePtr, out UInt32 featureCount);

            try
            {
                DismUtilities.ThrowIfFail(hresult, session);

                return new DismFeatureCollection(featurePtr, featureCount);
            }
            finally
            {
                // Clean up
                Delete(featurePtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Gets all the features in an image, regardless of whether the features are enabled or disabled.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="identifier">Optional. Either an absolute path to a .cab file or the package name, depending on the PackageIdentifier parameter value.</param>
            /// <param name="packageIdentifier">Optional. A valid DismPackageIdentifier Enumeration value.</param>
            /// <param name="feature">A pointer to the address of an array of DismFeature Structure objects.</param>
            /// <param name="count">The number of DismFeature structures that were returned.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824771.aspx" />
            /// HRESULT WINAPI DismGetFeatures (_In_ DismSession Session, _In_opt_ PCWSTR Identifier, _In_opt_ DismPackageIdentifier PackageIdentifier, _Outptr_result_buffer_(*Count) DismFeature** Feature, _Out_ UINT* Count);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismGetFeatures(DismSession session, string identifier, DismPackageIdentifier packageIdentifier, out IntPtr feature, out UInt32 count);
        }
    }
}