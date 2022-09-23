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
        /// Gets detailed information for the specified feature.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that you want to get more information about.</param>
        /// <returns>A <see cref="DismFeatureInfo" /> object.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismFeatureInfo GetFeatureInfo(DismSession session, string featureName)
        {
            return GetFeatureInfo(session, featureName, string.Empty, DismPackageIdentifier.None);
        }

        /// <summary>
        /// Gets detailed information for the specified feature.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that you want to get more information about.</param>
        /// <param name="packageName">The package name.</param>
        /// <returns>A <see cref="DismFeatureInfo" /> object.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismFeatureInfo GetFeatureInfoByPackageName(DismSession session, string featureName, string packageName)
        {
            return GetFeatureInfo(session, featureName, packageName, DismPackageIdentifier.Name);
        }

        /// <summary>
        /// Gets detailed information for the specified feature.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that you want to get more information about.</param>
        /// <param name="packagePath">An absolute path to a package.</param>
        /// <returns>A <see cref="DismFeatureInfo" /> object.</returns>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static DismFeatureInfo GetFeatureInfoByPackagePath(DismSession session, string featureName, string packagePath)
        {
            return GetFeatureInfo(session, featureName, packagePath, DismPackageIdentifier.Path);
        }

        /// <summary>
        /// Gets detailed information for the specified feature.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that you want to get more information about.</param>
        /// <param name="identifier">Either an absolute path to a .cab file or the package name, depending on the packageIdentifier parameter value.</param>
        /// <param name="packageIdentifier">A valid DismPackageIdentifier Enumeration value.</param>
        /// <returns>A <see cref="DismFeatureInfo" /> object.</returns>
        private static DismFeatureInfo GetFeatureInfo(DismSession session, string featureName, string identifier, DismPackageIdentifier packageIdentifier)
        {
            int hresult = NativeMethods.DismGetFeatureInfo(session, featureName, identifier, packageIdentifier, out IntPtr featureInfoPtr);

            try
            {
                DismUtilities.ThrowIfFail(hresult, session);

                // Return a new DismFeatureInfo from the native pointer
                return new DismFeatureInfo(featureInfoPtr);
            }
            finally
            {
                // Clean up the native pointer
                Delete(featureInfoPtr);
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Gets detailed information for the specified feature.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="featureName">The name of the feature that you want to get more information about.</param>
            /// <param name="identifier">Optional. Either an absolute path to a .cab file or the package name, depending on the PackageIdentifier parameter value.</param>
            /// <param name="packageIdentifier">Optional. A valid DismPackageIdentifier Enumeration value.</param>
            /// <param name="featureInfo">A pointer to the address of an array of DismFeatureInfo Structure objects.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>You can use this function to get the custom properties of a feature. If the feature has custom properties, they will be stored in the CustomProperty field as an array. Not all features have custom properties.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824735.aspx" />
            /// HRESULT WINAPI DismGetFeatureInfo (_In_ DismSession Session, _In_ PCWSTR FeatureName, _In_opt_ PCWSTR Identifier, _In_opt_ DismPackageIdentifier PackageIdentifier, _Out_ DismFeatureInfo** FeatureInfo);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismGetFeatureInfo(DismSession session, string featureName, string identifier, DismPackageIdentifier packageIdentifier, out IntPtr featureInfo);
        }
    }
}