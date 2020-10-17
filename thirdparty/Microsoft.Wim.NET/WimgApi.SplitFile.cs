// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Enables a large Windows® image (.wim) file to be split into smaller parts for replication or storage on smaller forms of media.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim file returned by <see cref="CreateFile"/>.</param>
        /// <param name="partPath">The path of the first file piece of the spanned set.</param>
        /// <param name="partSize">The size of the initial piece of the spanned set. This value will also be the default size used for subsequent pieces.</param>
        /// <exception cref="ArgumentNullException">wimHandle or partPath is null.</exception>
        /// <exception cref="DirectoryNotFoundException">Directory of partPath does not exist.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void SplitFile(WimHandle wimHandle, string partPath, long partSize)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if partPath is null
            if (partPath == null)
            {
                throw new ArgumentNullException(nameof(partPath));
            }

            // See if the directory of partPath does not exist
            //
            // ReSharper disable once AssignNullToNotNullAttribute
            if (!Directory.Exists(Path.GetDirectoryName(partPath)))
            {
                throw new DirectoryNotFoundException($"Could not find part of the path '{Path.GetDirectoryName(partPath)}'");
            }

            // Create a copy of part size so it can be safely passed by reference
            long partSizeCopy = partSize;

            // Call the native function
            if (!WimgApi.NativeMethods.WIMSplitFile(wimHandle, partPath, ref partSizeCopy, 0))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Gets the minimum size needed to to create a split WIM.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle"/> of a .wim file returned by <see cref="CreateFile"/>.</param>
        /// <param name="partPath">The path of the first file piece of the spanned set.</param>
        /// <returns>The minimum space required to split the WIM.</returns>
        /// <exception cref="ArgumentNullException">wimHandle or partPath is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static long SplitFile(WimHandle wimHandle, string partPath)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if partPath is null
            if (partPath == null)
            {
                throw new ArgumentNullException(nameof(partPath));
            }

            // Declare a part size as zero
            long partSize = 0;

            // Call the WIMSplitFile function which should return false and set partSize to the minimum size needed
            if (!WimgApi.NativeMethods.WIMSplitFile(wimHandle, partPath, ref partSize, 0))
            {
                // See if the return code was not ERROR_MORE_DATA
                if (Marshal.GetLastWin32Error() != WimgApi.ERROR_MORE_DATA)
                {
                    // Throw a Win32Exception based on the last error code
                    throw new Win32Exception();
                }
            }

            return partSize;
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Enables a large Windows® image (.wim) file to be split into smaller parts for replication or storage on smaller forms
            /// of media.
            /// </summary>
            /// <param name="hWim">A handle to a .wim file returned by WIMCreateFile.</param>
            /// <param name="pszPartPath">
            /// A pointer to a null-terminated string containing the path of the first file piece of the
            /// spanned set.
            /// </param>
            /// <param name="pliPartSize">
            /// A pointer to a LARGE_INTEGER, specifying the size of the initial piece of the spanned set.
            /// This value will also be the default size used for subsequent pieces, unless altered by a response to the WIM_MSG_SPLIT
            /// message. If the size specified is insufficient to create the first part of the spanned .wim file, the value is filled
            /// in with the minimum space required. If a single file is larger than the value specified, one of the split .swm files
            /// that results will be larger than the specified value in order to accommodate the large file. See Remarks.
            /// </param>
            /// <param name="dwFlags">Reserved. Must be zero.</param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero.
            /// If the function fails, then the return value is zero. To obtain extended error information, call GetLastError.
            /// </returns>
            /// <remarks>
            /// To obtain the minimum space required for the initial .wim file, set the contents of the pliPartSize parameter to zero
            /// and call the WIMSplitFile function. The function will return FALSE and set the LastError function to ERROR_MORE_DATA,
            /// and the contents of the pliPartSize parameter will be set to the minimum space required.
            /// This function creates many parts that are required to contain the resources of the original .wim file. The calling
            /// application may alter the path and size of subsequent pieces by responding to the WIM_MSG_SPLIT message.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMSplitFile(WimHandle hWim, string pszPartPath, ref long pliPartSize, DWORD dwFlags);
        }
    }
}