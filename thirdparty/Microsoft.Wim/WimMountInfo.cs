// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace Microsoft.Wim
{
    /// <summary>
    /// Represents the current state of a mount point.
    /// </summary>
    [Flags]
    public enum WimMountPointState : uint
    {
        /// <summary>
        /// The image mount point is no longer valid.
        /// </summary>
        Invalid = WimgApi.WIM_MOUNT_FLAG_INVALID,

        /// <summary>
        /// The image is actively mounted.
        /// </summary>
        Mounted = WimgApi.WIM_MOUNT_FLAG_MOUNTED,

        /// <summary>
        /// The image is in the process of mounting.
        /// </summary>
        Mounting = WimgApi.WIM_MOUNT_FLAG_MOUNTING,

        /// <summary>
        /// The image mount point has been removed or replaced.
        /// </summary>
        NoMountDir = WimgApi.WIM_MOUNT_FLAG_NO_MOUNTDIR,

        /// <summary>
        /// The mount point has been replaced with by a different mounted image.
        /// </summary>
        MountDirReplaced = WimgApi.WIM_MOUNT_FLAG_MOUNTDIR_REPLACED,

        /// <summary>
        /// The WIM file backing the mount point is missing or inaccessible.
        /// </summary>
        NoWim = WimgApi.WIM_MOUNT_FLAG_NO_WIM,

        /// <summary>
        /// The image has been mounted with read-write access.
        /// </summary>
        ReadWrite = WimgApi.WIM_MOUNT_FLAG_READWRITE,

        /// <summary>
        /// The image is not mounted, but is capable of being remounted.
        /// </summary>
        Remountable = WimgApi.WIM_MOUNT_FLAG_REMOUNTABLE,
    }

    /// <summary>
    /// Represents information about a mounted .wim file.
    /// </summary>
    public sealed class WimMountInfo
    {
        /// <summary>
        /// Represents the current WimMountedImageInfoLevels in use for marshaling.
        /// </summary>
        internal const WimMountedImageInfoLevels MountInfoLevel = WimMountedImageInfoLevels.Level1;

        /// <summary>
        /// The native <see cref="WimgApi.WIM_MOUNT_INFO_LEVEL1" /> struct that contains information about the mounted .wim file.
        /// </summary>
        private readonly WimgApi.WIM_MOUNT_INFO_LEVEL1 _wimMountInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="WimMountInfo"/> class.
        /// </summary>
        /// <param name="wimMountInfoPtr">A pointer to a native <see cref="WimgApi.WIM_MOUNT_INFO_LEVEL1" /> struct.</param>
        internal WimMountInfo(IntPtr wimMountInfoPtr)
            : this((WimgApi.WIM_MOUNT_INFO_LEVEL1)Marshal.PtrToStructure(wimMountInfoPtr, typeof(WimgApi.WIM_MOUNT_INFO_LEVEL1)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WimMountInfo"/> class.
        /// </summary>
        /// <param name="wimMountInfo">
        /// A <see cref="WimgApi.WIM_MOUNT_INFO_LEVEL1" /> that contains information about the mounted
        /// .wim file.
        /// </param>
        internal WimMountInfo(WimgApi.WIM_MOUNT_INFO_LEVEL1 wimMountInfo)
        {
            // Store the WIM_MOUNT_INFO_LEVEL1 struct
            _wimMountInfo = wimMountInfo;
        }

        /// <summary>
        /// Gets the image index within the .wim file specified in <see cref="Path" />.
        /// </summary>
        public int ImageIndex => (int)_wimMountInfo.ImageIndex;

        /// <summary>
        /// Gets the full path to the directory where the image is mounted.
        /// </summary>
        public string MountPath => _wimMountInfo.MountPath;

        /// <summary>
        /// Gets the full path to the .wim file.
        /// </summary>
        public string Path => _wimMountInfo.WimPath;

        /// <summary>
        /// Gets a value indicating whether gets a boolean value indicating if the image was mounted with support for saving changes.
        /// </summary>
        public bool ReadOnly => (State & WimMountPointState.ReadWrite) != WimMountPointState.ReadWrite;

        /// <summary>
        /// Gets the current state of the mount point.
        /// </summary>
        public WimMountPointState State => _wimMountInfo.MountFlags;

        /// <summary>
        /// Gets information about a mounted image.
        /// </summary>
        /// <param name="mountPath">The full file path of the directory to which the .wim file has been mounted.</param>
        /// <returns>A <see cref="WimMountInfo" /> object containing information about the mounted image.</returns>
        public static WimMountInfo GetMountInfo(string mountPath)
        {
            // Stores the handle to the image
            WimHandle imageHandle = null;

            try
            {
                // Get a mounted image handle
                //
                // ReSharper disable once UnusedVariable
                using (WimHandle wimHandle = WimgApi.GetMountedImageHandle(mountPath, true, out imageHandle))
                {
                    // Return the mounted image info from the handle
                    return WimgApi.GetMountedImageInfoFromHandle(imageHandle);
                }
            }
            finally
            {
                // Clean up
                imageHandle?.Dispose();
            }
        }
    }

    /// <summary>
    /// Represents a collection of <see cref="WimMountInfo" /> objects.
    /// </summary>
    public sealed class WimMountInfoCollection : ReadOnlyCollection<WimMountInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WimMountInfoCollection"/> class.
        /// </summary>
        /// <param name="list">A list of <see cref="WimMountInfo" /> objects to wrap as a collection.</param>
        internal WimMountInfoCollection(IList<WimMountInfo> list)
            : base(list)
        {
        }
    }
}