// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Applies an .ffu image to a specified physical drive.
        /// </summary>
        /// <param name="imagePath">The path to the .ffu image file to apply.</param>
        /// <param name="applyPath">The drive to apply the image to, for example \\.\PhysicalDrive0</param>
        public static void ApplyFfuImage(string imagePath, string applyPath)
        {
            ApplyFfuImage(imagePath, applyPath, null);
        }

        /// <summary>
        /// Applies an .ffu image to a specified physical drive.
        /// </summary>
        /// <param name="imagePath">The path to the .ffu image file to apply.</param>
        /// <param name="applyPath">The drive to apply the image to, for example \\.\PhysicalDrive0</param>
        /// <param name="partPath">An optional pattern for split FFU files that matches the naming convention.</param>
        public static void ApplyFfuImage(string imagePath, string applyPath, string partPath)
        {
            DismUtilities.ThrowIfFail(NativeMethods._DismApplyFfuImage(imagePath, applyPath, partPath));
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Undocumented method to apply an FFU image.
            /// </summary>
            /// <param name="ImagePath">The path to the FFU file to apply.</param>
            /// <param name="ApplyPath">The drive path to apply the image to.</param>
            /// <param name="PartPath">An optional file pattern that matches the names of split FFU images.</param>
            /// <returns>Returns S_OK on success.</returns>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int _DismApplyFfuImage([MarshalAs(UnmanagedType.LPWStr)] string ImagePath, [MarshalAs(UnmanagedType.LPWStr)] string ApplyPath, [MarshalAs(UnmanagedType.LPWStr)] string PartPath);
        }
    }
}