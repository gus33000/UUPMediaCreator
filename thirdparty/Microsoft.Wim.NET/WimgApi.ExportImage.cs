// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.Wim
{
    /// <summary>
    /// Specifies options when exporting an image.
    /// </summary>
    [Flags]
    public enum WimExportImageOptions : uint
    {
        /// <summary>
        /// The image will be exported to the destination .wim file even if it is already stored in that .wim file.
        /// </summary>
        AllowDuplicates = WimgApi.WIM_EXPORT_ALLOW_DUPLICATES,

        /// <summary>
        /// Image resources and XML information are exported to the destination .wim file and no supporting file resources are included.
        /// </summary>
        MetadataOnly = WimgApi.WIM_EXPORT_ONLY_METADATA,

        /// <summary>
        /// No options are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// File resources will be exported to the destination .wim file and no image resources or XML information will be included.
        /// </summary>
        ResourcesOnly = WimgApi.WIM_EXPORT_ONLY_RESOURCES,
    }

    public static partial class WimgApi
    {
        /// <summary>
        /// Transfers the data of an image from one Windows® image (.wim) file to another.
        /// </summary>
        /// <param name="imageHandle">A <see cref="WimHandle"/> opened by the <see cref="LoadImage"/> method.</param>
        /// <param name="wimHandle">A <see cref="WimHandle"/> returned by the <see cref="CreateFile"/> method.  This handle must have <see cref="WimFileAccess.Write"/> access to accept the exported image. Split .wim files are not supported.</param>
        /// <param name="options">Specifies how the image will be exported to the destination .wim file.</param>
        /// <exception cref="ArgumentNullException">imageHandle or wimHandle is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>You must call the <see cref="SetTemporaryPath"/> method for both the source and the destination .wim files before calling the ExportImage method.</remarks>
        public static void ExportImage(WimHandle imageHandle, WimHandle wimHandle, WimExportImageOptions options)
        {
            // See if imageHandle is null
            if (imageHandle == null)
            {
                throw new ArgumentNullException(nameof(imageHandle));
            }

            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMExportImage(imageHandle, wimHandle, (UInt32)options))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Transfers the data of an image from one Windows® image (.wim) file to another.
            /// </summary>
            /// <param name="hImage">A handle to an image opened by the WIMLoadImage function.</param>
            /// <param name="hWim">
            /// A handle to a .wim file returned by the WIMCreateFile function. This handle must have
            /// WIM_GENERIC_WRITE access to accept the exported image. Split .wim files are not supported.
            /// </param>
            /// <param name="dwFlags">Specifies how the image will be exported to the destination .wim file.</param>
            /// <returns>
            /// If the function succeeds, the return value is nonzero.
            /// If the function fails, the return value is zero. To obtain extended error information, call the GetLastError function.
            /// </returns>
            /// <remarks>
            /// You must call the WIMSetTemporaryPath function for both the source and the destination .wim files before calling the
            /// WIMExportImage function.
            /// If zero is passed in for the dwFlags parameter and the image is already stored in the destination, the function will
            /// return FALSE and set the LastError to ERROR_ALREADY_EXISTS.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMExportImage(WimHandle hImage, WimHandle hWim, UInt32 dwFlags);
        }
    }
}