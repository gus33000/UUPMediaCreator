// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// The data area passed to a system call is too small.
        /// </summary>
        internal const int ERROR_INSUFFICIENT_BUFFER = 0x0000007A;

        /// <summary>
        /// More data is available.
        /// </summary>
        internal const int ERROR_MORE_DATA = 0x000000EA;

        /// <summary>
        /// The request was aborted.
        /// </summary>
        internal const int ERROR_REQUEST_ABORTED = 0x000004D3;

        /// <summary>
        /// Indicates an error when calling the WIMRegisterMessageCallback function.
        /// </summary>
        internal const uint INVALID_CALLBACK_VALUE = 0xFFFFFFFF;

        /// <summary>
        /// The .wim file only contains image resources and XML information.
        /// </summary>
        internal const uint WIM_ATTRIBUTE_METADATA_ONLY = 0x00000002;

        /// <summary>
        /// The .wim file does not have any other attributes set.
        /// </summary>
        internal const uint WIM_ATTRIBUTE_NORMAL = 0x00000000;

        /// <summary>
        /// The .wim file is locked and cannot be modified.
        /// </summary>
        internal const uint WIM_ATTRIBUTE_READONLY = 0x00000020;

        /// <summary>
        /// The .wim file only contains file resources and no images or metadata.
        /// </summary>
        internal const uint WIM_ATTRIBUTE_RESOURCE_ONLY = 0x00000001;

        /// <summary>
        /// The .wim file contains one or more images where symbolic link or junction path fixup is enabled.
        /// </summary>
        internal const uint WIM_ATTRIBUTE_RP_FIX = 0x00000008;

        /// <summary>
        /// The .wim file has been split into multiple pieces via WIMSplitFile.
        /// </summary>
        internal const uint WIM_ATTRIBUTE_SPANNED = 0x00000010;

        /// <summary>
        /// The .wim file contains integrity data that can be used by the WIMCopyFile or WIMCreateFile function.
        /// </summary>
        internal const uint WIM_ATTRIBUTE_VERIFY_DATA = 0x00000004;

        /// <summary>
        /// Adds a new image entry to the .wim file. The default is to update the image specified during mount.
        /// </summary>
        internal const uint WIM_COMMIT_FLAG_APPEND = 0x00000001;

        /// <summary>
        /// Automatically retries copy operations in event of failures.
        /// </summary>
        internal const uint WIM_COPY_FILE_RETRY = 0x01000000;

        /// <summary>
        /// Makes a new image file. If the file exists, the function overwrites the file.
        /// </summary>
        internal const uint WIM_CREATE_ALWAYS = 0x00000002;

        /// <summary>
        /// Makes a new image file. If the specified file already exists, the function fails.
        /// </summary>
        internal const uint WIM_CREATE_NEW = 0x00000001;

        /// <summary>
        /// Removes all mounted images, whether actively mounted or not.
        /// </summary>
        internal const uint WIM_DELETE_MOUNTS_ALL = 0x00000001;

        /// <summary>
        /// The image will be exported to the destination .wim file even if it is already stored in that .wim file.
        /// </summary>
        internal const uint WIM_EXPORT_ALLOW_DUPLICATES = 0x00000001;

        /// <summary>
        /// Image resources and XML information are exported to the destination .wim file and no supporting file resources are included.
        /// </summary>
        internal const uint WIM_EXPORT_ONLY_METADATA = 0x00000004;

        /// <summary>
        /// File resources will be exported to the destination .wim file and no image resources or XML information will be included.
        /// </summary>
        internal const uint WIM_EXPORT_ONLY_RESOURCES = 0x00000002;

        /// <summary>
        /// Verifies the destination file.
        /// </summary>
        internal const uint WIM_EXPORT_VERIFY_DESTINATION = 0x00000010;

        /// <summary>
        /// Verifies the source file.
        /// </summary>
        internal const uint WIM_EXPORT_VERIFY_SOURCE = 0x00000008;

        /// <summary>
        /// Create flag to allow cross-file WIM.
        /// </summary>
        internal const uint WIM_FLAG_CHUNKED = 0x20000000;

        /// <summary>
        /// Sends a WIM_MSG_FILEINFO message during the apply operation.
        /// </summary>
        internal const uint WIM_FLAG_FILEINFO = 0x00000080;

        /// <summary>
        /// Specifies that the image is to be sequentially read for caching or performance purposes.
        /// </summary>
        internal const uint WIM_FLAG_INDEX = 0x00000004;

        /// <summary>
        /// Mounts the image using a faster operation.
        /// </summary>
        internal const uint WIM_FLAG_MOUNT_FAST = 0x00000400;

        /// <summary>
        /// Mounts the image using a legacy operation.
        /// </summary>
        internal const uint WIM_FLAG_MOUNT_LEGACY = 0x00000800;

        /// <summary>
        /// Mounts the image without the ability to save changes, regardless of WIM access level.
        /// </summary>
        internal const uint WIM_FLAG_MOUNT_READONLY = 0x00000200;

        /// <summary>
        /// Applies the image without physically creating directories or files. Useful for obtaining a list of files and directories in the image.
        /// </summary>
        internal const uint WIM_FLAG_NO_APPLY = 0x00000008;

        /// <summary>
        /// Disables restoring security information for directories.
        /// </summary>
        internal const uint WIM_FLAG_NO_DIRACL = 0x00000010;

        /// <summary>
        /// Disables restoring security information for files.
        /// </summary>
        internal const uint WIM_FLAG_NO_FILEACL = 0x00000020;

        /// <summary>
        /// Disables automatic path fixups for junctions and symbolic links.
        /// </summary>
        internal const uint WIM_FLAG_NO_RP_FIX = 0x00000100;

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        internal const uint WIM_FLAG_RESERVED = 0x00000001;

        /// <summary>
        /// Opens the .wim file in a mode that enables simultaneous reading and writing.
        /// </summary>
        internal const uint WIM_FLAG_SHARE_WRITE = 0x00000040;

        /// <summary>
        /// Verifies that files match original data.
        /// </summary>
        internal const uint WIM_FLAG_VERIFY = 0x00000002;

        /// <summary>
        /// Specifies mount access to the image file. Enables images to be mounted with WIMMountImageHandle.
        /// </summary>
        internal const uint WIM_GENERIC_MOUNT = 0x20000000;

        /// <summary>
        /// Specifies read-only access to the image file. Enables images to be applied from the file. Combine with WIM_GENERIC_WRITE for read/write (append) access.
        /// </summary>
        internal const uint WIM_GENERIC_READ = 0x80000000;

        /// <summary>
        /// Specifies write access to the image file. Enables images to be captured to the file. Includes WIM_GENERIC_READ access to enable apply and append operations with existing images.
        /// </summary>
        internal const uint WIM_GENERIC_WRITE = 0x40000000;

        /// <summary>
        /// Specifies that the log file should be in UTF8 format.
        /// </summary>
        internal const uint WIM_LOGFILE_UTF8 = 0x00000001;

        /// <summary>
        /// The image mount point is no longer valid.
        /// </summary>
        internal const uint WIM_MOUNT_FLAG_INVALID = 0x00000008;

        /// <summary>
        /// The mount point has been replaced with by a different mounted image.
        /// </summary>
        internal const uint WIM_MOUNT_FLAG_MOUNTDIR_REPLACED = 0x00000040;

        /// <summary>
        /// The image is actively mounted.
        /// </summary>
        internal const uint WIM_MOUNT_FLAG_MOUNTED = 0x00000001;

        /// <summary>
        /// The image is in the process of mounting.
        /// </summary>
        internal const uint WIM_MOUNT_FLAG_MOUNTING = 0x00000002;

        /// <summary>
        /// The image mount point has been removed or replaced.
        /// </summary>
        internal const uint WIM_MOUNT_FLAG_NO_MOUNTDIR = 0x00000020;

        /// <summary>
        /// The WIM file backing the mount point is missing or inaccessible.
        /// </summary>
        internal const uint WIM_MOUNT_FLAG_NO_WIM = 0x00000010;

        /// <summary>
        /// The image has been mounted with read-write access.
        /// </summary>
        internal const uint WIM_MOUNT_FLAG_READWRITE = 0x00000100;

        /// <summary>
        /// The image is not mounted, but is capable of being remounted.
        /// </summary>
        internal const uint WIM_MOUNT_FLAG_REMOUNTABLE = 0x00000004;

        /// <summary>
        /// Cancels an image apply or image capture.
        /// </summary>
        internal const uint WIM_MSG_ABORT_IMAGE = 0xFFFFFFFF;

        /// <summary>
        /// Indicates success and prevents other subscribers from processing the message.
        /// </summary>
        internal const uint WIM_MSG_DONE = 0xFFFFFFF0;

        /// <summary>
        /// Indicates the error can be ignored.
        /// </summary>
        internal const uint WIM_MSG_SKIP_ERROR = 0xFFFFFFFE;

        /// <summary>
        /// Indicates success and to enables other subscribers to process the message.
        /// </summary>
        internal const uint WIM_MSG_SUCCESS = 0x00000000;

        /// <summary>
        /// Opens the image file if it exists. If the file does not exist and the caller requests WIM_GENERIC_WRITE access, the function makes the file.
        /// </summary>
        internal const uint WIM_OPEN_ALWAYS = 0x00000004;

        /// <summary>
        /// Opens the image file. If the file does not exist, the function fails.
        /// </summary>
        internal const uint WIM_OPEN_EXISTING = 0x00000003;

        /// <summary>
        /// The specified .wim file is appended to the current list.
        /// </summary>
        internal const uint WIM_REFERENCE_APPEND = 0x00010000;

        /// <summary>
        /// The specified .wim file becomes the only item in the list.
        /// </summary>
        internal const uint WIM_REFERENCE_REPLACE = 0x00020000;
    }
}