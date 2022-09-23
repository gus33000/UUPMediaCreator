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
        /// Gets the parent features of a specified feature.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the OpenSession Function.</param>
        /// <param name="featureName">The name of the feature that you want to find the parent of.</param>
        /// <param name="packageName">The name of the package that contains the feature.</param>
        /// <returns>A <see cref="DismFeatureCollection" /> object containing a collection of <see cref="DismFeature" /> objects.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismFeatureCollection GetFeatureParentByName(DismSession session, string featureName, string packageName)
        {
            return GetFeatureParent(session, featureName, packageName, DismPackageIdentifier.Name);
        }

        /// <summary>
        /// Gets the parent features of a specified feature.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the OpenSession Function.</param>
        /// <param name="featureName">The name of the feature that you want to find the parent of.</param>
        /// <param name="packagePath">An absolute path to a .cab file.</param>
        /// <returns>A <see cref="DismFeatureCollection" /> object containing a collection of <see cref="DismFeature" /> objects.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismFeatureCollection GetFeatureParentByPath(DismSession session, string featureName, string packagePath)
        {
            return GetFeatureParent(session, featureName, packagePath, DismPackageIdentifier.Path);
        }

        /// <summary>
        /// Gets the parent features of a specified feature.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the OpenSession Function.</param>
        /// <param name="featureName">The name of the feature that you want to find the parent of.</param>
        /// <param name="identifier">Either an absolute path to a .cab file or the package name, depending on the PackageIdentifier parameter value.</param>
        /// <param name="packageIdentifier">Optional. A valid DismPackageIdentifier Enumeration value.</param>
        /// <returns>A <see cref="DismFeatureCollection" /> object containing a collection of <see cref="DismFeature" /> objects.</returns>
        private static DismFeatureCollection GetFeatureParent(DismSession session, string featureName, string identifier, DismPackageIdentifier packageIdentifier)
        {
            int hresult = NativeMethods.DismGetFeatureParent(session, featureName, identifier, packageIdentifier, out IntPtr featurePtr, out UInt32 featureCount);

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
            /// Gets the parent features of a specified feature.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="featureName">The name of the feature that you want to find the parent of.</param>
            /// <param name="identifier">Optional. Either an absolute path to a .cab file or the package name, depending on the PackageIdentifier parameter value.</param>
            /// <param name="packageIdentifier">Optional. A valid DismPackageIdentifier Enumeration value.</param>
            /// <param name="feature">A pointer to the address of an array of DismFeature Structure objects.</param>
            /// <param name="count">The number of DismFeature structures that were returned.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>For a feature to be enabled, one or more of its parent features must be enabled. You can use this function to enumerate the parent features and determine which parent needs to be enabled.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824798.aspx" />
            /// HRESULT WINAPI DismGetFeatureParent (_In_ DismSession Session, _In_ PCWSTR FeatureName, _In_opt_ PCWSTR Identifier, _In_opt_ DismPackageIdentifier PackageIdentifier, _Outptr_result_buffer_(*Count) DismFeature** Feature, _Out_ UINT* Count);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismGetFeatureParent(DismSession session, string featureName, string identifier, DismPackageIdentifier packageIdentifier, out IntPtr feature, out UInt32 count);
        }
    }
}