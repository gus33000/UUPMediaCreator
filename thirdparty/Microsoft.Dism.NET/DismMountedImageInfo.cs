// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Describes the metadata of a mounted image.
        /// </summary>
        /// <remarks>
        /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824755.aspx" />
        /// typedef struct _DismMountedImageInfo
        /// {
        ///     PCWSTR MountPath;
        ///     PCWSTR ImageFilePath;
        ///     UINT ImageIndex;
        ///     DismMountMode MountMode;
        ///     DismMountStatus MountStatus;
        /// } DismMountedImageInfo;
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
        internal struct DismMountedImageInfo_
        {
            /// <summary>
            /// A relative or absolute path to the mounted image.
            /// </summary>
            public string MountPath;

            /// <summary>
            /// A relative or absolute path to the image file.
            /// </summary>
            public string ImageFilePath;

            /// <summary>
            /// The index number of the image. Index numbering starts at 1.
            /// </summary>
            public UInt32 ImageIndex;

            /// <summary>
            /// A <a href="DismMountMode" /> Enumeration value representing whether the image is DismReadWrite or DismReadOnly.
            /// </summary>
            public DismMountMode MountMode;

            /// <summary>
            /// A <a href="DismMountStatus" /> Enumeration value such as DismMountStatusOk.
            /// </summary>
            public DismMountStatus MountStatus;
        }
    }

    /// <summary>
    /// Represents a mounted image.
    /// </summary>
    public sealed class DismMountedImageInfo : IEquatable<DismMountedImageInfo>
    {
        private readonly DismApi.DismMountedImageInfo_ _mountedImageInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="DismMountedImageInfo" /> class.
        /// </summary>
        /// <param name="mountedImageInfo">A <see cref="DismApi.DismMountedImageInfo_" /> structure.</param>
        internal DismMountedImageInfo(DismApi.DismMountedImageInfo_ mountedImageInfo)
        {
            _mountedImageInfo = mountedImageInfo;
        }

        /// <summary>
        /// Gets the relative or absolute path to the image file.
        /// </summary>
        public string ImageFilePath => _mountedImageInfo.ImageFilePath;

        /// <summary>
        /// Gets the index number of the image. Index numbering starts at 1.
        /// </summary>
        public int ImageIndex => (int)_mountedImageInfo.ImageIndex;

        /// <summary>
        /// Gets the mount mode of the image.
        /// </summary>
        public DismMountMode MountMode => _mountedImageInfo.MountMode;

        /// <summary>
        /// Gets the relative or absolute path to the mounted image.
        /// </summary>
        public string MountPath => _mountedImageInfo.MountPath;

        /// <summary>
        /// Gets the status of the mounted image.
        /// </summary>
        public DismMountStatus MountStatus => _mountedImageInfo.MountStatus;

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as DismMountedImageInfo);
        }

        /// <summary>
        /// Determines whether the specified <see cref="DismMountedImageInfo" /> is equal to the current <see cref="DismMountedImageInfo" />.
        /// </summary>
        /// <param name="other">The <see cref="DismMountedImageInfo" /> object to compare with the current object.</param>
        /// <returns>true if the specified <see cref="DismMountedImageInfo" /> is equal to the current <see cref="DismMountedImageInfo" />; otherwise, false.</returns>
        public bool Equals(DismMountedImageInfo other)
        {
            return other != null
                   && ImageFilePath == other.ImageFilePath
                   && MountMode == other.MountMode
                   && MountPath == other.MountPath
                   && MountStatus == other.MountStatus;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="T:System.Object" />.</returns>
        public override int GetHashCode()
        {
            return (string.IsNullOrEmpty(ImageFilePath) ? 0 : ImageFilePath.GetHashCode())
                   ^ MountMode.GetHashCode()
                   ^ (string.IsNullOrEmpty(MountPath) ? 0 : MountPath.GetHashCode())
                   ^ MountStatus.GetHashCode();
        }
    }
}