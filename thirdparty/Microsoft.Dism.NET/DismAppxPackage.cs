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
        /// This struct is currently undocumented.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
        internal struct DismAppxPackage_
        {
            /// <summary>
            /// The name of the package.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string PackageName;

            /// <summary>
            /// The display name of the package.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string DisplayName;

            /// <summary>
            /// The publisher ID.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string PublisherId;

            /// <summary>
            /// The major version.
            /// </summary>
            public UInt32 MajorVersion;

            /// <summary>
            /// The minor version.
            /// </summary>
            public UInt32 MinorVersion;

            /// <summary>
            /// The build version.
            /// </summary>
            public UInt32 Build;

            /// <summary>
            /// The revision version.
            /// </summary>
            public UInt32 Revision;

            /// <summary>
            /// The architecture of the package.
            /// </summary>
            public UInt32 Architecture;

            /// <summary>
            /// The resource ID.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ResourceId;

            /// <summary>
            /// The installation location.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string InstallLocation;

            /// <summary>
            /// The region of the package.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Regions;
        }
    }

    /// <summary>
    /// Represents information about an Appx package.
    /// </summary>
    public sealed class DismAppxPackage : IEquatable<DismAppxPackage>
    {
        private readonly DismApi.DismAppxPackage_ _appxPackage;

        /// <summary>
        /// Initializes a new instance of the <see cref="DismAppxPackage" /> class.
        /// </summary>
        /// <param name="appxPackage">A <see cref="DismApi.DismAppxPackage_" /> structure.</param>
        internal DismAppxPackage(DismApi.DismAppxPackage_ appxPackage)
        {
            _appxPackage = appxPackage;

            Version = new Version((int)appxPackage.MajorVersion, (int)appxPackage.MinorVersion, (int)appxPackage.Build, (int)appxPackage.Revision);
        }

        /// <summary>
        /// Gets the architecture of the package.
        /// </summary>
        public DismProcessorArchitecture Architecture => (DismProcessorArchitecture)_appxPackage.Architecture;

        /// <summary>
        /// Gets the display name of the package.
        /// </summary>
        public string DisplayName => _appxPackage.DisplayName;

        /// <summary>
        /// Gets the installation path of the package.
        /// </summary>
        public string InstallLocation => _appxPackage.InstallLocation;

        /// <summary>
        /// Gets the name of the package.
        /// </summary>
        public string PackageName => _appxPackage.PackageName;

        /// <summary>
        /// Gets the publisher identifier of the package.
        /// </summary>
        public string PublisherId => _appxPackage.PublisherId;

        /// <summary>
        /// Gets the resource identifier of the package.
        /// </summary>
        public string ResourceId => _appxPackage.ResourceId;

        /// <summary>
        /// Gets the version of the package.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as DismAppxPackage);
        }

        /// <summary>
        /// Determines whether the specified <see cref="DismAppxPackage" /> is equal to the current <see cref="DismAppxPackage" />.
        /// </summary>
        /// <param name="other">The <see cref="DismAppxPackage" /> object to compare with the current object.</param>
        /// <returns>true if the specified <see cref="DismAppxPackage" /> is equal to the current <see cref="DismAppxPackage" />; otherwise, false.</returns>
        public bool Equals(DismAppxPackage other)
        {
            return other != null
                   && Architecture == other.Architecture
                   && DisplayName == other.DisplayName
                   && InstallLocation == other.InstallLocation
                   && PackageName == other.PackageName
                   && PublisherId == other.PublisherId
                   && ResourceId == other.ResourceId
                   && Version == other.Version;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="T:System.Object" />.</returns>
        public override int GetHashCode()
        {
            return Architecture.GetHashCode()
                   ^ DisplayName?.GetHashCode() ?? 0
                   ^ InstallLocation?.GetHashCode() ?? 0
                   ^ PackageName?.GetHashCode() ?? 0
                   ^ PublisherId?.GetHashCode() ?? 0
                   ^ ResourceId?.GetHashCode() ?? 0
                   ^ Version.GetHashCode();
        }
    }
}