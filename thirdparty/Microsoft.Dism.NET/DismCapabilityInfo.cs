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
        /// Describes information about a capability.
        /// </summary>
        /// <remarks>
        /// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/mt684922.aspx" />
        /// typedef struct _DismCapabilityInfo {
        ///     PCWSTR Name;
        ///     DismPackageFeatureState State;
        ///     PCWSTR DisplayName;
        ///     PCWSTR Description;
        ///     DWORD DownloadSize;
        ///     DWORD InstallSize;
        /// } DismCapabilityInfo;
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
        internal struct DismCapabilityInfo_
        {
            /// <summary>
            /// The name of the capability.
            /// </summary>
            public string Name;

            /// <summary>
            /// The state of the capability.
            /// </summary>
            public DismPackageFeatureState State;

            /// <summary>
            /// The display name of the capability.
            /// </summary>
            public string DisplayName;

            /// <summary>
            /// The description of the capability.
            /// </summary>
            public string Description;

            /// <summary>
            /// The download size of the capability in bytes.
            /// </summary>
            public UInt32 DownloadSize;

            /// <summary>
            /// The install size of the capability in bytes.
            /// </summary>
            public UInt32 InstallSize;
        }
    }

    /// <summary>
    /// Describes information about a capability.
    /// </summary>
    public sealed class DismCapabilityInfo : IEquatable<DismCapabilityInfo>
    {
        private readonly DismApi.DismCapabilityInfo_ _capabilityInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="DismCapabilityInfo" /> class.
        /// </summary>
        /// <param name="capabilityPtr">An <see cref="IntPtr" /> of a <see cref="DismApi.DismCapabilityInfo_" /> structure.</param>
        internal DismCapabilityInfo(IntPtr capabilityPtr)
        {
            _capabilityInfo = capabilityPtr.ToStructure<DismApi.DismCapabilityInfo_>();
        }

        /// <summary>
        /// Gets the name of the capability.
        /// </summary>
        public string Name => _capabilityInfo.Name;

        /// <summary>
        /// Gets the state of the capability.
        /// </summary>
        public DismPackageFeatureState State => _capabilityInfo.State;

        /// <summary>
        /// Gets the display name of the capability.
        /// </summary>
        public string DisplayName => _capabilityInfo.DisplayName;

        /// <summary>
        /// Gets the description of the capability.
        /// </summary>
        public string Description => _capabilityInfo.Description;

        /// <summary>
        /// Gets the download size of the capability in bytes.
        /// </summary>
        public int DownloadSize => (int)_capabilityInfo.DownloadSize;

        /// <summary>
        /// Gets the install size of the capability in bytes.
        /// </summary>
        public int InstallSize => (int)_capabilityInfo.InstallSize;

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as DismCapabilityInfo);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(DismCapabilityInfo other)
        {
            return other != null
                && Name == other.Name
                && State == other.State
                && DisplayName == other.DisplayName
                && Description == other.Description
                && DownloadSize == other.DownloadSize
                && InstallSize == other.InstallSize;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return (string.IsNullOrEmpty(Name) ? 0 : Name.GetHashCode())
                ^ State.GetHashCode()
                ^ (string.IsNullOrEmpty(DisplayName) ? 0 : DisplayName.GetHashCode())
                ^ (string.IsNullOrEmpty(Description) ? 0 : Description.GetHashCode())
                ^ DownloadSize
                ^ InstallSize;
        }
    }
}
