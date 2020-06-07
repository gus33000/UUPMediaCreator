// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    /// <summary>
    /// Specifies options when creating a .wim file.
    /// </summary>
    [Flags]
    public enum WimCreateFileOptions : uint
    {
        /// <summary>
        /// No options are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allow cross-file WIM like ESD.
        /// </summary>
        Chunked = WimgApi.WIM_FLAG_CHUNKED,

        /// <summary>
        /// Opens the .wim file in a mode that enables simultaneous reading and writing.
        /// </summary>
        ShareWrite = WimgApi.WIM_FLAG_SHARE_WRITE,

        /// <summary>
        /// Generates data integrity information for new files. Verifies and updates existing files.
        /// </summary>
        Verify = WimgApi.WIM_FLAG_VERIFY,
    }

    /// <summary>
    /// Specifies which action to take when creating .wim and the file exists, and which action to take when file does not exist.
    /// </summary>
    public enum WimCreationDisposition : uint
    {
        /// <summary>
        /// Makes a new image file. If the file exists, the function overwrites the file.
        /// </summary>
        CreateAlways = WimgApi.WIM_CREATE_ALWAYS,

        /// <summary>
        /// Makes a new image file. If the specified file already exists, the function fails.
        /// </summary>
        CreateNew = WimgApi.WIM_CREATE_NEW,

        /// <summary>
        /// Opens the image file if it exists. If the file does not exist and the caller requests <see cref="WimFileAccess.Write"/> access, the function makes the file.
        /// </summary>
        OpenAlways = WimgApi.WIM_OPEN_ALWAYS,

        /// <summary>
        /// Opens the image file. If the file does not exist, the function fails.
        /// </summary>
        OpenExisting = WimgApi.WIM_OPEN_EXISTING,
    }

    /// <summary>
    /// Represents the result of creating an image.
    /// </summary>
    public enum WimCreationResult : uint
    {
        /// <summary>
        /// The file did not exist and was created.
        /// </summary>
        CreatedNew = 0,

        /// <summary>
        /// The file existed and was opened for access.
        /// </summary>
        OpenedExisting = 1
    }

    /// <summary>
    /// Defines constants for read, write, or mount access to a .wim file.
    /// </summary>
    [Flags]
    public enum WimFileAccess : uint
    {
        /// <summary>
        /// Specifies mount access to the image file.
        /// </summary>
        Mount = WimgApi.WIM_GENERIC_MOUNT,

        /// <summary>
        /// Specifies query access to the file. An application can query image information without accessing the images.
        /// </summary>
        Query = 0,

        /// <summary>
        /// Specifies read-only access to the image file. Enables images to be applied from the file. Combine with WimFileAccess.Write for read/write (append) access.
        /// </summary>
        Read = WimgApi.WIM_GENERIC_READ,

        /// <summary>
        /// Specifies write access to the image file. Enables images to be captured to the file. Includes WimFileAccess.Read access to enable apply and append operations with existing images.
        /// </summary>
        Write = WimgApi.WIM_GENERIC_WRITE,
    }

    public static partial class WimgApi
    {
        /// <summary>
        /// Makes a new image file or opens an existing image file.
        /// </summary>
        /// <param name="path">The name of the file to create or to open.</param>
        /// <param name="desiredAccess">The type of <see cref="WimFileAccess"/> to the object. An application can obtain read access, write access, read/write access, or device query access.</param>
        /// <param name="creationDisposition">The <see cref="WimCreationDisposition"/> to take on files that exist, and which action to take when files do not exist.</param>
        /// <param name="options"><see cref="WimCreateFileOptions"/> to be used for the specified file.</param>
        /// <param name="compressionType">The <see cref="WimCompressionType"/> to be used for a newly created image file.  If the file already exists, then this value is ignored.</param>
        /// <returns>A <see cref="WimHandle"/> object representing the file.</returns>
        /// <exception cref="ArgumentNullException">path is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static WimHandle CreateFile(string path, WimFileAccess desiredAccess, WimCreationDisposition creationDisposition, WimCreateFileOptions options, WimCompressionType compressionType)
        {
            // See if destinationFile is null
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // Call the native function
            WimHandle wimHandle = WimgApi.NativeMethods.WIMCreateFile(path, (DWORD)desiredAccess, (DWORD)creationDisposition, (DWORD)options, (DWORD)compressionType, out _);

            // See if the handle returned is valid
            if (wimHandle == null || wimHandle.IsInvalid)
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }

            // Return the handle to the wim
            return wimHandle;
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Makes a new image file or opens an existing image file.
            /// </summary>
            /// <param name="pszWimPath">
            /// A pointer to a null-terminated string that specifies the name of the file to create or to
            /// open.
            /// </param>
            /// <param name="dwDesiredAccess">
            /// Specifies the type of access to the object. An application can obtain read access, write
            /// access, read/write access, or device query access.
            /// </param>
            /// <param name="dwCreationDisposition">
            /// Specifies which action to take on files that exist, and which action to take when
            /// files do not exist.
            /// </param>
            /// <param name="dwFlagsAndAttributes">Specifies special actions to be taken for the specified file.</param>
            /// <param name="dwCompressionType">
            /// Specifies the compression mode to be used for a newly created image file. If the file
            /// already exists, then this value is ignored.
            /// </param>
            /// <param name="pdwCreationResult">
            /// A pointer to a variable that receives one of the following creation-result values. If
            /// this information is not required, specify NULL.
            /// </param>
            /// <returns>
            /// If the function succeeds, the return value is an open handle to the specified image file.
            /// If the function fails, the return value is NULL. To obtain extended error information, call the GetLastError function.
            /// </returns>
            /// <remarks>Use the WIMCloseHandle function to close the handle returned by the WIMCreateFile function.</remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            public static extern WimHandle WIMCreateFile(string pszWimPath, DWORD dwDesiredAccess, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, DWORD dwCompressionType, out WimCreationResult pdwCreationResult);
        }
    }
}