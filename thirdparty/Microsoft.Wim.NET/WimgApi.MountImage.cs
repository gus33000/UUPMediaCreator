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
    /// <summary>
    /// Represents options when mounting an image.
    /// </summary>
    [Flags]
    public enum WimMountImageOptions : uint
    {
        /// <summary>
        /// Mounts the image using a faster operation.
        /// </summary>
        Fast = WimgApi.WIM_FLAG_MOUNT_FAST,

        /// <summary>
        /// Disables capturing security information for directories.
        /// </summary>
        DisableDirectoryAcl = WimgApi.WIM_FLAG_NO_DIRACL,

        /// <summary>
        /// Disables capturing security information for files.
        /// </summary>
        DisableFileAcl = WimgApi.WIM_FLAG_NO_FILEACL,

        /// <summary>
        /// Disables automatic path repairs for junctions and symbolic links.
        /// </summary>
        DisableRPFix = WimgApi.WIM_FLAG_NO_RP_FIX,

        /// <summary>
        /// Mounts the image using a legacy operation.
        /// </summary>
        Legacy = WimgApi.WIM_FLAG_MOUNT_LEGACY,

        /// <summary>
        /// No options are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Mounts the image without the ability to save changes, regardless of WIM access level.
        /// </summary>
        ReadOnly = WimgApi.WIM_FLAG_MOUNT_READONLY,

        /// <summary>
        /// Verifies that files match original data.
        /// </summary>
        Verify = WimgApi.WIM_FLAG_VERIFY,
    }

    public static partial class WimgApi
    {
        /// <summary>
        /// Mounts an image in a Windows® image (.wim) file to the specified directory and does not allow for edits.
        /// </summary>
        /// <param name="mountPath">The full file path of the directory to which the .wim file has to be mounted.</param>
        /// <param name="imagePath">The full file name of the .wim file that has to be mounted.</param>
        /// <param name="imageIndex">An index of the image in the .wim file that has to be mounted.</param>
        /// <exception cref="ArgumentNullException">mountPath or imagePath is null.</exception>
        /// <exception cref="DirectoryNotFoundException">mountPath does not exist.</exception>
        /// <exception cref="FileNotFoundException">imagePath does not exist.</exception>
        /// <exception cref="IndexOutOfRangeException">index is less than 1.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void MountImage(string mountPath, string imagePath, int imageIndex)
        {
            // Call an overload
            WimgApi.MountImage(mountPath, imagePath, imageIndex, tempPath: null);
        }

        /// <summary>
        /// Mounts an image in a Windows® image (.wim) file to the specified directory.
        /// </summary>
        /// <param name="mountPath">The full file path of the directory to which the .wim file has to be mounted.</param>
        /// <param name="imagePath">The full file name of the .wim file that has to be mounted.</param>
        /// <param name="imageIndex">The one-based index of the image in the .wim file that is to be mounted.</param>
        /// <param name="tempPath">The full file path to the temporary directory in which changes to the .wim file can be tracked.  If this parameter is <c>null</c>, the image will not be mounted for edits.</param>
        /// <exception cref="ArgumentNullException">mountPath or imagePath is null.</exception>
        /// <exception cref="DirectoryNotFoundException">mountPath does not exist.</exception>
        /// <exception cref="FileNotFoundException">imagePath does not exist.</exception>
        /// <exception cref="IndexOutOfRangeException">index is less than 1.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void MountImage(string mountPath, string imagePath, int imageIndex, string tempPath)
        {
            // See if mountPath is null
            if (mountPath == null)
            {
                throw new ArgumentNullException(nameof(mountPath));
            }

            // See if imagePath is null
            if (imagePath == null)
            {
                throw new ArgumentNullException(nameof(imagePath));
            }

            // See if the specified index is valid
            if (imageIndex < 1)
            {
                throw new IndexOutOfRangeException($"There is no image at index {imageIndex}.");
            }

            // See if mount path does not exist
            if (!Directory.Exists(mountPath))
            {
                throw new DirectoryNotFoundException($"Could not find a part of the path '{mountPath}'");
            }

            // See if the image does not exist
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Could not find a part of the path '{imagePath}'");
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMMountImage(mountPath, imagePath, (DWORD)imageIndex, tempPath))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Mounts an image in a Windows® image (.wim) file to the specified directory.
        /// </summary>
        /// <param name="imageHandle">A <see cref="WimHandle"/> of a a volume image returned by the <see cref="LoadImage"/> or <see cref="CaptureImage"/> method. The WIM file must have been opened with <see cref="WimFileAccess.Mount"/> flag in call to <see cref="CreateFile"/>.</param>
        /// <param name="mountPath">The full file path of the directory to which the .wim file has to be mounted.</param>
        /// <param name="options">Specifies how the file is to be treated and what features are to be used.</param>
        /// <exception cref="ArgumentNullException">imageHandle or mountPath is null.</exception>
        /// <exception cref="DirectoryNotFoundException">mountPath does not exist.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        /// <remarks>This method maps the contents of the given image in a .wim file to the specified mount directory. After the successful completion of this operation, users or applications can access the contents of the image mapped under the mount directory. The WIM file containing the image must be opened with <see cref="WimFileAccess.Mount"/> access. Use the <see cref="UnmountImage(WimHandle)"/> method to unmount the image from the mount directory.</remarks>
        public static void MountImage(WimHandle imageHandle, string mountPath, WimMountImageOptions options)
        {
            // See if imageHandle is null
            if (imageHandle == null)
            {
                throw new ArgumentNullException(nameof(imageHandle));
            }

            // See if mountPath is null
            if (mountPath == null)
            {
                throw new ArgumentNullException(nameof(mountPath));
            }

            // See if mount path does not exist
            if (!Directory.Exists(mountPath))
            {
                throw new DirectoryNotFoundException($"Could not find a part of the path '{mountPath}'");
            }

            // Call the native function
            if (!WimgApi.NativeMethods.WIMMountImageHandle(imageHandle, mountPath, (DWORD)options))
            {
                // Throw a Win32Exception based on the last error code
                throw new Win32Exception();
            }
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Mounts an image in a Windows® image (.wim) file to the specified directory.
            /// </summary>
            /// <param name="pszMountPath">
            /// A pointer to the full file path of the directory to which the .wim file has to be mounted.
            /// This parameter is required and cannot be NULL. The specified path must not exceed MAX_PATH characters in length.
            /// </param>
            /// <param name="pszWimFileName">
            /// A pointer to the full file name of the .wim file that has to be mounted. This parameter is
            /// required and cannot be NULL.
            /// </param>
            /// <param name="dwImageIndex">An index of the image in the .wim file that has to be mounted.</param>
            /// <param name="pszTempPath">
            /// A pointer to the full file path to the temporary directory in which changes to the .wim file
            /// can be tracked. If this parameter is NULL, the image will not be mounted for edits.
            /// </param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            /// <remarks>
            /// The WIMMountImage function maps the contents of the given image in a .wim file to the specified mount directory. After
            /// the successful completion of this operation, users or applications can access the contents of the image mapped under
            /// the mount directory.
            /// Use the WIMUnmountImage function to unmount the image from the mount directory.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMMountImage(string pszMountPath, string pszWimFileName, DWORD dwImageIndex, [Optional] string pszTempPath);

            /// <summary>
            /// Mounts an image in a Windows® image (.wim) file to the specified directory.
            /// </summary>
            /// <param name="hImage">
            /// A handle to a volume image returned by the WIMLoadImage or WIMCaptureImage function. The WIM file
            /// must have been opened with WIM_GENERIC_MOUNT flag in call to WIMCreateFile.
            /// </param>
            /// <param name="pszMountPath">
            /// A pointer to the full file path of the directory to which the .wim file has been mounted.
            /// This parameter is required and cannot be NULL. The specified path must not exceed MAX_PATH characters in length.
            /// </param>
            /// <param name="dwMountFlags">Specifies how the file is to be treated and what features are to be used.</param>
            /// <returns>
            /// Returns TRUE and sets the LastError to ERROR_SUCCESS on the successful completion of this function. Returns
            /// FALSE in case of a failure and sets the LastError to the appropriate Win32® error value.
            /// </returns>
            /// <remarks>
            /// The WIMMountImageHandle function maps the contents of the given image in a .wim file to the specified mount
            /// directory. After the successful completion of this operation, users or applications can access the contents of the
            /// image mapped under the mount directory. The WIM file containing the image must be opened with WIM_GENERIC_MOUNT access.
            /// Use the WIMUnmountImageHandle function to unmount the image from the mount directory.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMMountImageHandle(WimHandle hImage, string pszMountPath, DWORD dwMountFlags);
        }
    }
}