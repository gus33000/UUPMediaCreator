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
        /// Contains basic information for the driver that is associated with the .inf file.
        /// </summary>
        /// <remarks>
        /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824776.aspx" />
        /// typedef struct DismDriverPackage
        /// {
        ///    PCWSTR PublishedName;
        ///    PCWSTR OriginalFileName;
        ///    BOOL InBox;
        ///    PCWSTR CatalogFile;
        ///    PCWSTR ClassName;
        ///    PCWSTR ClassGuid;
        ///    PCWSTR ClassDescription;
        ///    BOOL BootCritical;
        ///    DismDriverSignature DriverSignature;
        ///    PCWSTR ProviderName;
        ///    SYSTEMTIME Date;
        ///    UINT MajorVersion;
        ///    UINT MinorVersion;
        ///    UINT Build;
        ///    UINT Revision
        /// } DismDriver;
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
        internal struct DismDriverPackage_
        {
            /// <summary>
            /// The published driver name.
            /// </summary>
            public string PublishedName;

            /// <summary>
            /// The original file name of the driver.
            /// </summary>
            public string OriginalFileName;

            /// <summary>
            /// TRUE if the driver is included on the Windows distribution media and automatically installed as part of Windows®, otherwise FALSE.
            /// </summary>
            public bool InBox;

            /// <summary>
            /// The catalog file for the driver.
            /// </summary>
            public string CatalogFile;

            /// <summary>
            /// The class name of the driver.
            /// </summary>
            public string ClassName;

            /// <summary>
            /// The class GUID of the driver.
            /// </summary>
            public string ClassGuid;

            /// <summary>
            /// The class description of the driver.
            /// </summary>
            public string ClassDescription;

            /// <summary>
            /// TRUE if the driver is boot-critical, otherwise FALSE.
            /// </summary>
            public bool BootCritical;

            /// <summary>
            /// The driver signature status.
            /// </summary>
            public DismDriverSignature DriverSignature;

            /// <summary>
            /// The provider of the driver.
            /// </summary>
            public string ProviderName;

            /// <summary>
            /// The manufacturer's build date of the driver.
            /// </summary>
            public SystemTime Date;

            /// <summary>
            /// The major version number of the driver.
            /// </summary>
            public UInt32 MajorVersion;

            /// <summary>
            /// The minor version number of the driver.
            /// </summary>
            public UInt32 MinorVersion;

            /// <summary>
            /// The build number of the driver.
            /// </summary>
            public UInt32 Build;

            /// <summary>
            /// The revision number of the driver.
            /// </summary>
            public UInt32 Revision;
        }
    }

    /// <summary>
    /// Represents basic information for the driver that is associated with the .inf file.
    /// </summary>
    public sealed class DismDriverPackage : IEquatable<DismDriverPackage>
    {
        private readonly DismApi.DismDriverPackage_ _driverPackage;

        /// <summary>
        /// Initializes a new instance of the <see cref="DismDriverPackage" /> class.
        /// </summary>
        /// <param name="driverPackage">A native DismDriverPackage_ struct.</param>
        internal DismDriverPackage(DismApi.DismDriverPackage_ driverPackage)
        {
            _driverPackage = driverPackage;

            Date = _driverPackage.Date;

            // Copy data from the struct
            Version = new Version((int)driverPackage.MajorVersion, (int)driverPackage.MinorVersion, (int)driverPackage.Build, (int)driverPackage.Revision);
        }

        /// <summary>
        /// Gets a value indicating whether the driver is boot-critical.
        /// </summary>
        public bool BootCritical => _driverPackage.BootCritical;

        /// <summary>
        /// Gets the catalog file for the driver.
        /// </summary>
        public string CatalogFile => _driverPackage.CatalogFile;

        /// <summary>
        /// Gets the class description of the driver.
        /// </summary>
        public string ClassDescription => _driverPackage.ClassDescription;

        /// <summary>
        /// Gets the class GUID of the driver.
        /// </summary>
        public string ClassGuid => _driverPackage.ClassGuid;

        /// <summary>
        /// Gets the class name of the driver.
        /// </summary>
        public string ClassName => _driverPackage.ClassName;

        /// <summary>
        /// Gets the manufacturer's build date of the driver.
        /// </summary>
        public DateTime Date { get; }

        /// <summary>
        /// Gets the driver signature status.
        /// </summary>
        public DismDriverSignature DriverSignature => _driverPackage.DriverSignature;

        /// <summary>
        /// Gets a value indicating whether the driver is included on the Windows distribution media and automatically installed as part of Windows®.
        /// </summary>
        public bool InBox => _driverPackage.InBox;

        /// <summary>
        /// Gets the original file name of the driver.
        /// </summary>
        public string OriginalFileName => _driverPackage.OriginalFileName;

        /// <summary>
        /// Gets the provider of the driver.
        /// </summary>
        public string ProviderName => _driverPackage.ProviderName;

        /// <summary>
        /// Gets the published driver name.
        /// </summary>
        public string PublishedName => _driverPackage.PublishedName;

        /// <summary>
        /// Gets the major version number of the driver.
        /// </summary>
        public Version Version
        {
            get;
            private set;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj != null && Equals(obj as DismDriverPackage);
        }

        /// <summary>
        /// Determines whether the specified <see cref="DismDriverPackage" /> is equal to the current <see cref="DismDriverPackage" />.
        /// </summary>
        /// <param name="other">The <see cref="DismDriverPackage" /> object to compare with the current object.</param>
        /// <returns>true if the specified <see cref="DismDriverPackage" /> is equal to the current <see cref="DismDriverPackage" />; otherwise, false.</returns>
        public bool Equals(DismDriverPackage other)
        {
            return other != null
                   && BootCritical == other.BootCritical
                   && InBox == other.InBox
                   && CatalogFile == other.CatalogFile
                   && ClassDescription == other.ClassDescription
                   && ClassGuid == other.ClassGuid
                   && ClassName == other.ClassName
                   && Date == other.Date
                   && DriverSignature == other.DriverSignature
                   && OriginalFileName == other.OriginalFileName
                   && ProviderName == other.ProviderName
                   && PublishedName == other.PublishedName;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="T:System.Object" />.</returns>
        public override int GetHashCode()
        {
            return BootCritical.GetHashCode()
                   ^ InBox.GetHashCode()
                   ^ (string.IsNullOrEmpty(CatalogFile) ? 0 : CatalogFile.GetHashCode())
                   ^ (string.IsNullOrEmpty(ClassDescription) ? 0 : ClassDescription.GetHashCode())
                   ^ (string.IsNullOrEmpty(ClassGuid) ? 0 : ClassGuid.GetHashCode())
                   ^ (string.IsNullOrEmpty(ClassName) ? 0 : ClassName.GetHashCode())
                   ^ Date.GetHashCode()
                   ^ DriverSignature.GetHashCode()
                   ^ (string.IsNullOrEmpty(OriginalFileName) ? 0 : OriginalFileName.GetHashCode())
                   ^ (string.IsNullOrEmpty(ProviderName) ? 0 : ProviderName.GetHashCode())
                   ^ (string.IsNullOrEmpty(PublishedName) ? 0 : PublishedName.GetHashCode());
        }
    }
}