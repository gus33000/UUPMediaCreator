// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Splits an existing .ffu file into multiple read-only split FFU files.
        /// </summary>
        /// <param name="imagePath">The path to the FFU image to split.</param>
        /// <param name="partPath">The path to the split file to create.</param>
        /// <param name="partSize">The maximum size in megabytes (MB) for each created file.</param>
        public static void SplitFfuImage(string imagePath, string partPath, long partSize)
        {
            DismUtilities.ThrowIfFail(NativeMethods._DismSplitFfuImage(imagePath, partPath, (ulong)partSize));
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Undocumented method that splits an existing .ffu file into multiple read-only split FFU files.
            /// </summary>
            /// <param name="ImagePath">The path to an FFU image to split.</param>
            /// <param name="PartPath">The path to the SFU file to create.</param>
            /// <param name="PartSize">The maximum size in megabytes (MB) for each created file.</param>
            /// <returns>Retrusn S_OK on success.</returns>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int _DismSplitFfuImage([MarshalAs(UnmanagedType.LPWStr)] string ImagePath, [MarshalAs(UnmanagedType.LPWStr)] string PartPath, ulong PartSize);
        }
    }
}