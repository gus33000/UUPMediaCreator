// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageIndex">The index of the image in the WIM file that you want to mount. For a VHD file, you must specify an index of 1.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void MountImage(string imageFilePath, string mountPath, int imageIndex)
        {
            MountImage(imageFilePath, mountPath, imageIndex, false);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageIndex">The index of the image in the WIM file that you want to mount. For a VHD file, you must specify an index of 1.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void MountImage(string imageFilePath, string mountPath, int imageIndex, bool readOnly)
        {
            MountImage(imageFilePath, mountPath, imageIndex, readOnly, DismMountImageOptions.None);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageIndex">The index of the image in the WIM file that you want to mount. For a VHD file, you must specify an index of 1.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <param name="options">Specifies options to use when mounting an image.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void MountImage(string imageFilePath, string mountPath, int imageIndex, bool readOnly, DismMountImageOptions options)
        {
            MountImage(imageFilePath, mountPath, imageIndex, readOnly, options, null);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageIndex">The index of the image in the WIM file that you want to mount. For a VHD file, you must specify an index of 1.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static void MountImage(string imageFilePath, string mountPath, int imageIndex, bool readOnly, Dism.DismProgressCallback progressCallback)
        {
            MountImage(imageFilePath, mountPath, imageIndex, readOnly, DismMountImageOptions.None, progressCallback);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageIndex">The index of the image in the WIM file that you want to mount. For a VHD file, you must specify an index of 1.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <param name="options">Specifies options to use when mounting an image.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static void MountImage(string imageFilePath, string mountPath, int imageIndex, bool readOnly, DismMountImageOptions options, Dism.DismProgressCallback progressCallback)
        {
            MountImage(imageFilePath, mountPath, imageIndex, readOnly, options, progressCallback, null);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageIndex">The index of the image in the WIM file that you want to mount. For a VHD file, you must specify an index of 1.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static void MountImage(string imageFilePath, string mountPath, int imageIndex, bool readOnly, Dism.DismProgressCallback progressCallback, object userData)
        {
            MountImage(imageFilePath, mountPath, imageIndex, readOnly, DismMountImageOptions.None, progressCallback, userData);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageIndex">The index of the image in the WIM file that you want to mount. For a VHD file, you must specify an index of 1.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <param name="options">Specifies options to use when mounting an image.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static void MountImage(string imageFilePath, string mountPath, int imageIndex, bool readOnly, DismMountImageOptions options, Dism.DismProgressCallback progressCallback, object userData)
        {
            MountImage(imageFilePath, mountPath, imageIndex, null, DismImageIdentifier.ImageIndex, readOnly, options, progressCallback, userData);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageName">The name of the image that you want to mount.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void MountImage(string imageFilePath, string mountPath, string imageName)
        {
            MountImage(imageFilePath, mountPath, imageName, false);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageName">The name of the image that you want to mount.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void MountImage(string imageFilePath, string mountPath, string imageName, bool readOnly)
        {
            MountImage(imageFilePath, mountPath, imageName, readOnly, DismMountImageOptions.None);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageName">The name of the image that you want to mount.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <param name="options">Specifies options to use when mounting an image.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void MountImage(string imageFilePath, string mountPath, string imageName, bool readOnly, DismMountImageOptions options)
        {
            MountImage(imageFilePath, mountPath, imageName, readOnly, options, null);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageName">The name of the image that you want to mount.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static void MountImage(string imageFilePath, string mountPath, string imageName, bool readOnly, Dism.DismProgressCallback progressCallback)
        {
            MountImage(imageFilePath, mountPath, imageName, readOnly, DismMountImageOptions.None, progressCallback);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageName">The name of the image that you want to mount.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <param name="options">Specifies options to use when mounting an image.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static void MountImage(string imageFilePath, string mountPath, string imageName, bool readOnly, DismMountImageOptions options, Dism.DismProgressCallback progressCallback)
        {
            MountImage(imageFilePath, mountPath, imageName, readOnly, options, progressCallback, null);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageName">The name of the image that you want to mount.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static void MountImage(string imageFilePath, string mountPath, string imageName, bool readOnly, Dism.DismProgressCallback progressCallback, object userData)
        {
            MountImage(imageFilePath, mountPath, imageName, readOnly, DismMountImageOptions.None, progressCallback, userData);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageName">The name of the image that you want to mount.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <param name="options">Specifies options to use when mounting an image.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        public static void MountImage(string imageFilePath, string mountPath, string imageName, bool readOnly, DismMountImageOptions options, Dism.DismProgressCallback progressCallback, object userData)
        {
            MountImage(imageFilePath, mountPath, 0, imageName, DismImageIdentifier.ImageName, readOnly, options, progressCallback, userData);
        }

        /// <summary>
        /// Mounts a WIM or VHD image file to a specified location.
        /// </summary>
        /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
        /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
        /// <param name="imageIndex">The index of the image in the WIM file that you want to mount. For a VHD file, you must specify an index of 1.</param>
        /// <param name="imageName">The name of the image that you want to mount.</param>
        /// <param name="imageIdentifier">A DismImageIdentifier Enumeration value such as DismImageIndex.</param>
        /// <param name="readOnly">Specifies if the image should be mounted in read-only mode.</param>
        /// <param name="options">Specifies options to use when mounting an image.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        private static void MountImage(string imageFilePath, string mountPath, int imageIndex, string imageName, DismImageIdentifier imageIdentifier, bool readOnly, DismMountImageOptions options, Microsoft.Dism.DismProgressCallback progressCallback, object userData)
        {
            // Determine the flags to pass to the native call
            uint flags = (readOnly ? DISM_MOUNT_READONLY : DISM_MOUNT_READWRITE) | (uint)options;

            // Create a DismProgress object to wrap the callback and allow cancellation
            DismProgress progress = new DismProgress(progressCallback, userData);

            int hresult = NativeMethods.DismMountImage(imageFilePath, mountPath, (uint)imageIndex, imageName, imageIdentifier, flags, progress.EventHandle, progress.DismProgressCallbackNative, IntPtr.Zero);

            DismUtilities.ThrowIfFail(hresult);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Mounts a WIM or VHD image file to a specified location.
            /// </summary>
            /// <param name="imageFilePath">The path to the WIM or VHD file on the local computer. A .wim, .vhd, or .vhdx file name extension is required.</param>
            /// <param name="mountPath">The path of the location where the image should be mounted. This mount path must already exist on the computer. The Windows image in a .wim, .vhd, or .vhdx file can be mounted to an empty folder on an NTFS formatted drive. A Windows image in a .vhd or .vhdx file can also be mounted to an unassigned drive letter. You cannot mount an image to the root of the existing drive.</param>
            /// <param name="imageIndex">The index of the image in the WIM file that you want to mount. For a VHD file, you must specify an index of 1.</param>
            /// <param name="imageName">Optional. The name of the image that you want to mount.</param>
            /// <param name="imageIdentifier">A DismImageIdentifier Enumeration value such as DismImageIndex.</param>
            /// <param name="flags">The mount flags to use for this operation. For more information about mount flags, see DISM API Constants.</param>
            /// <param name="cancelEvent">Optional. You can set a CancelEvent for this function in order to cancel the operation in progress when signaled by the client. If the CancelEvent is received at a stage when the operation cannot be canceled, the operation will continue and return a success code. If the CancelEvent is received and the operation is canceled, the image state is unknown. You should verify the image state before continuing or discard the changes and start again.</param>
            /// <param name="progress">Optional. A pointer to a client-defined DismProgressCallback Function.</param>
            /// <param name="userData">Optional. User defined custom data.</param>
            /// <returns>Returns OK on success.
            ///
            /// Returns E_INVALIDARG if any of the paths are not well-formed or if MountPath or ImageFilePath does not exist or is invalid.
            ///
            /// Returns a Win32 error code mapped to an HRESULT for other errors.</returns>
            /// <remarks>After mounting an image, use the DismOpenSession Function to start a servicing session. For more information, see Using the DISM API.
            ///
            /// Mounting an image from a WIM or VHD file that is stored on the network is not supported. You must specify a file on the local computer.
            ///
            /// To mount an image from a VHD file, you must specify an ImageIndex of 1.
            ///
            /// The MountPath must be a file path that already exists on the computer. Images in WIM and VHD files can be mounted to an empty folder on an NTFS formatted drive. You can also mount an image from a VHD file to an unassigned drive letter. You cannot mount an image to the root of the existing drive.
            ///
            /// When mounting an image in a WIM file, the image can either be identified by the image index number specified by ImageIndex, or the name of the image specified by ImageName. ImageIdentifier specifies whether to use the ImageIndex or ImageName parameter to identify the image.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824731.aspx" />
            /// HRESULT WINAPI DismMountImage (_In_ PCWSTR ImageFilePath, _In_ PCWSTR MountPath, _In_ UINT ImageIndex, _In_opt_ PCWSTR ImageName, _In_ DismImageIdentifier ImageIdentifier, _In_ DWORD Flags, _In_opt_ HANDLE CancelEvent, _In_opt_ DISM_PROGRESS_CALLBACK Progress, _In_opt_ PVOID UserData);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismMountImage(string imageFilePath, string mountPath, UInt32 imageIndex, string imageName, DismImageIdentifier imageIdentifier, UInt32 flags, SafeWaitHandle cancelEvent, DismProgressCallback progress, IntPtr userData);
        }
    }
}