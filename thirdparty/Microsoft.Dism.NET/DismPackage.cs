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
        /// Describes basic information about a package, including the date and time that the package was installed.
        /// </summary>
        /// <remarks>
        /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824763.aspx" />
        /// typedef struct _DismPackage
        /// {
        ///     PCWSTR PackageName;
        ///     DismPackageFeatureState PackageState;
        ///     DismReleaseType ReleaseType;
        ///     SYSTEMTIME InstallTime;
        /// } DismPackage;
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
        internal struct DismPackage_
        {
            /// <summary>
            /// The package name.
            /// </summary>
            public string PackageName;

            /// <summary>
            /// A DismPackageFeatureState Enumeration value, for example, DismStateResolved.
            /// </summary>
            public DismPackageFeatureState PackageState;

            /// <summary>
            /// A DismReleaseType Enumeration value, for example, DismReleaseTypeDriver.
            /// </summary>
            public DismReleaseType ReleaseType;

            /// <summary>
            /// The date and time that the package was installed. This field is local time relative to the servicing host computer.
            /// </summary>
            public SystemTime InstallTime;
        }
    }

    /// <summary>
    /// Represents basic information about a package, including the date and time that the package was installed.
    /// </summary>
    public sealed class DismPackage : IEquatable<DismPackage>
    {
        private readonly DismApi.DismPackage_ _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="DismPackage" /> class.
        /// </summary>
        /// <param name="package">A <see cref="DismApi.DismPackage_" /> structure.</param>
        internal DismPackage(DismApi.DismPackage_ package)
        {
            _package = package;

            InstallTime = _package.InstallTime;
        }

        /// <summary>
        /// Gets the date and time the package was installed.
        /// </summary>
        public DateTime InstallTime { get; }

        /// <summary>
        /// Gets the package name.
        /// </summary>
        public string PackageName => _package.PackageName;

        /// <summary>
        /// Gets the state of the package.
        /// </summary>
        public DismPackageFeatureState PackageState => _package.PackageState;

        /// <summary>
        /// Gets the release type of the package.
        /// </summary>
        public DismReleaseType ReleaseType => _package.ReleaseType;

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as DismPackage);
        }

        /// <summary>
        /// Determines whether the specified <see cref="DismPackage" /> is equal to the current <see cref="DismPackage" />.
        /// </summary>
        /// <param name="other">The <see cref="DismPackage" /> object to compare with the current object.</param>
        /// <returns>true if the specified <see cref="DismPackage" /> is equal to the current <see cref="DismPackage" />; otherwise, false.</returns>
        public bool Equals(DismPackage other)
        {
            return other != null
                   && InstallTime == other.InstallTime
                   && PackageName == other.PackageName
                   && PackageState == other.PackageState
                   && ReleaseType == other.ReleaseType;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="T:System.Object" />.</returns>
        public override int GetHashCode()
        {
            return InstallTime.GetHashCode()
                   ^ (string.IsNullOrEmpty(PackageName) ? 0 : PackageName.GetHashCode())
                   ^ PackageState.GetHashCode()
                   ^ ReleaseType.GetHashCode();
        }
    }
}