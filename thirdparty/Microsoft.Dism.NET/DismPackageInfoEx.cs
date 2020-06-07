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
        /// Describes detailed package information such as the client used to install the package, the date and time that the package was installed, and support information.
        /// </summary>
        /// <remarks>
        /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824774.aspx" />
        /// typedef struct _DismPackageInfoEx
        /// {
        ///     DismPackageInfo;
        ///     PCWSTR CapabilityId;
        /// } DismPackageInfoEx;
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
        internal struct DismPackageInfoEx_
        {
            /// <summary>
            /// An instance of <see cref="DismPackageInfo_" /> containing the package info.
            /// </summary>
            public DismPackageInfo_ PackageInfo;

            /// <summary>
            /// The capability ID of the package.
            /// </summary>
            public string CapabilityId;
        }
    }

    /// <summary>
    /// Represents detailed package information such as the client used to install the package, the date and time that the package was installed, and support information.
    /// </summary>
    public sealed class DismPackageInfoEx : DismPackageInfo, IEquatable<DismPackageInfoEx>
    {
        private readonly DismApi.DismPackageInfoEx_ _packageInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="DismPackageInfoEx" /> class.
        /// </summary>
        /// <param name="packageInfoPtr">A pointer to a native <see cref="DismApi.DismPackageInfoEx_" /> struct.</param>
        internal DismPackageInfoEx(IntPtr packageInfoPtr)
            : this(packageInfoPtr.ToStructure<DismApi.DismPackageInfoEx_>())
        {
        }

        private DismPackageInfoEx(DismApi.DismPackageInfoEx_ packageInfoEx)
            : base(packageInfoEx.PackageInfo)
        {
            _packageInfo = packageInfoEx;
        }

        /// <summary>
        /// Gets the capability of the package.
        /// </summary>
        public string CapabilityId => _packageInfo.CapabilityId;

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as DismPackageInfoEx);
        }

        /// <summary>
        /// Determines whether the specified <see cref="DismPackageInfoEx" /> is equal to the current <see cref="DismPackageInfoEx" />.
        /// </summary>
        /// <param name="other">The <see cref="DismPackageInfoEx" /> object to compare with the current object.</param>
        /// <returns>true if the specified <see cref="DismPackageInfoEx" /> is equal to the current <see cref="DismPackageInfoEx" />; otherwise, false.</returns>
        public bool Equals(DismPackageInfoEx other)
        {
            return other != null
                   && base.Equals(other)
                   && CapabilityId == other.CapabilityId;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="T:System.Object" />.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ CapabilityId.GetHashCode();
        }
    }
}