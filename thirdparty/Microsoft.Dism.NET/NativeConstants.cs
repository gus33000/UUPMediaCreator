// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace Microsoft.Dism
{
    public static partial class DismApi
    {
#pragma warning disable SA1310 // Field names must not contain underscore
                              /// <summary>
                              /// Indicates to the DismCommitImage Function or the DismUnmountImage Function that changes to the image should be saved.
                              /// </summary>
        public const uint DISM_COMMIT_APPEND = 0x00020000;

        /// <summary>
        /// Indicates to the DismCommitImage Function or the DismUnmountImage Function to set a flag on the image specifying whether the image is corrupted.
        /// </summary>
        public const uint DISM_COMMIT_GENERATE_INTEGRITY = 0x00010000;

        /// <summary>
        /// Indicates to the DismCommitImage Function or the DismUnmountImage Function that changes to the image should be saved.
        /// </summary>
        public const uint DISM_COMMIT_IMAGE = 0x00000000;

        /// <summary>
        /// Indicates to the DismUnmountImage Function that all changes should be saved. This flag is equivalent to using DISM_COMMIT_IMAGE, DISM_COMMIT_GENERATE_INTEGRITY, and DISM_COMMIT_APPEND.
        /// </summary>
        public const uint DISM_COMMIT_MASK = 0xFFFF0000;

        /// <summary>
        /// Indicates to the DismCommitImage Function or the DismUnmountImage Function that changes to the image should not be saved.
        /// </summary>
        public const uint DISM_DISCARD_IMAGE = 0x00000001;

        /// <summary>
        /// Indicates to the DismMountImage Function to set a flag on the image specifying whether the image is corrupted.
        /// </summary>
        public const uint DISM_MOUNT_CHECK_INTEGRITY = 0x00000004;

        /// <summary>
        /// Indicates to the DismMountImage Function that the image should be mounted with optimization. When the optimize option is used, only the top level of the file directory in the image will be mapped to the mount location. The first time that you access a file path that is not initially mapped, that branch of the directory will be mounted. As a result, there may be an increase in the time that is required to access a directory for the first time after mounting an image using the optimize option.
        /// </summary>
        public const uint DISM_MOUNT_OPTIMIZE = 0x00000002;

        /// <summary>
        /// Indicates to the DismMountImage Function that the image should be mounted with read access only.
        /// </summary>
        public const uint DISM_MOUNT_READONLY = 0x00000001;

        /// <summary>
        /// Indicates to the DismMountImage Function that the image should be mounted with both read and write access.
        /// </summary>
        public const uint DISM_MOUNT_READWRITE = 0x00000000;

        /// <summary>
        /// Indicates to the DismOpenSession Function that the online operating system, %windir%, should be associated to the DISMSession for servicing.
        /// </summary>
        public const string DISM_ONLINE_IMAGE = "DISM_{53BFAE52-B167-4E2F-A258-0A37B57FF845}";

        /// <summary>
        /// Represents a default value for a DismSession pointer.
        /// </summary>
        public const uint DISM_SESSION_DEFAULT = 0;

        /// <summary>
        /// The current package and feature servicing infrastructure is busy.  Wait a bit and try the operation again.
        /// </summary>
        public const uint DISMAPI_E_BUSY = 0x800F0902;

        /// <summary>
        /// DISM API was not initialized for this process
        /// </summary>
        public const uint DISMAPI_E_DISMAPI_NOT_INITIALIZED = 0xC0040001;

        /// <summary>
        /// An invalid DismSession handle was passed into a DISMAPI function
        /// </summary>
        public const uint DISMAPI_E_INVALID_DISM_SESSION = 0xC0040004;

        /// <summary>
        /// An invalid image index was specified
        /// </summary>
        public const uint DISMAPI_E_INVALID_IMAGE_INDEX = 0xC0040005;

        /// <summary>
        /// An invalid image name was specified
        /// </summary>
        public const uint DISMAPI_E_INVALID_IMAGE_NAME = 0xC0040006;

        /// <summary>
        /// Failed to gain access to the log file user specified. Logging has been disabled..
        /// </summary>
        public const uint DISMAPI_E_LOGGING_DISABLED = 0xC0040009;

        /// <summary>
        /// The offline image specified is the running system. The macro DISM_ONLINE_IMAGE must be
        /// used instead.
        /// </summary>
        public const uint DISMAPI_E_MUST_SPECIFY_ONLINE_IMAGE = 0xC004000E;

        /// <summary>
        /// The image needs to be remounted before any servicing operation.
        /// </summary>
        public const uint DISMAPI_E_NEEDS_REMOUNT = 0XC1510114;

        /// <summary>
        /// A DismSession with open handles was attempted to be mounted
        /// </summary>
        public const uint DISMAPI_E_OPEN_HANDLES_UNABLE_TO_MOUNT_IMAGE_PATH = 0xC004000B;

        /// <summary>
        /// A DismSession with open handles was attempted to be remounted
        /// </summary>
        public const uint DISMAPI_E_OPEN_HANDLES_UNABLE_TO_REMOUNT_IMAGE_PATH = 0xC004000C;

        /// <summary>
        /// A DismSession with open handles was attempted to be unmounted
        /// </summary>
        public const uint DISMAPI_E_OPEN_HANDLES_UNABLE_TO_UNMOUNT_IMAGE_PATH = 0xC004000A;

        /// <summary>
        /// A DismShutdown was called while there were open DismSession handles
        /// </summary>
        public const uint DISMAPI_E_OPEN_SESSION_HANDLES = 0xC0040003;

        /// <summary>
        /// One or several parent features are disabled so current feature can not be enabled.
        /// Solutions:
        /// 1 Call function DismGetFeatureParent to get all parent features and enable all of them. Or
        /// 2 Set EnableAll to TRUE when calling function DismEnableFeature.
        /// </summary>
        public const uint DISMAPI_E_PARENT_FEATURE_DISABLED = 0xC004000D;

        /// <summary>
        /// A DismSession was being shutdown when another operation was called on it
        /// </summary>
        public const uint DISMAPI_E_SHUTDOWN_IN_PROGRESS = 0xC0040002;

        /// <summary>
        /// An image that is not a mounted WIM or mounted VHD was attempted to be unmounted
        /// </summary>
        public const uint DISMAPI_E_UNABLE_TO_UNMOUNT_IMAGE_PATH = 0xC0040007;

        /// <summary>
        /// The feature is not present in the package.
        /// </summary>
        public const uint DISMAPI_E_UNKNOWN_FEATURE = 0x800f080c;

        /// <summary>
        /// The request was cancelled.
        /// </summary>
        internal const uint ERROR_CANCELLED = 0x000004C7; // 1223

        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        internal const int ERROR_SUCCESS = 0x00000000;

        /// <summary>
        /// Not enough storage is available to complete this operation.
        /// </summary>
        internal const int ERROR_OUTOFMEMORY = 0x0000000E;  // 14

        /// <summary>
        /// The request was aborted.
        /// </summary>
        internal const int ERROR_REQUEST_ABORTED = 0x000004D3;  // 1235

        /// <summary>
        /// The requested operation is successful. Changes will not be effective until the system is rebooted.
        /// </summary>
        internal const int ERROR_SUCCESS_REBOOT_REQUIRED = 0x00000BC2;  // 3010

        /// <summary>
        /// The requested operation is successful. Changes will not be effective until the service is restarted.
        /// </summary>
        internal const int ERROR_SUCCESS_RESTART_REQUIRED = 0x00000BC3;  // 3011

        /// <summary>
        /// The requested operation is successful. The DISM session needs to be reloaded.
        /// </summary>
        internal const int DISMAPI_S_RELOAD_IMAGE_SESSION_REQUIRED = 0x00000001;  // 1

        /// <summary>
        /// The specified package is not applicable.
        /// </summary>
        internal const uint CBS_E_NOT_APPLICABLE = 0x800F081E;
#pragma warning restore SA1310 // Field names must not contain underscore
    }
}