// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;

// ReSharper disable InconsistentNaming
namespace Microsoft.Dism
{
    /// <summary>
    /// Specifies the signature status of a driver.
    /// </summary>
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824785.aspx" />
    /// </remarks>
    public enum DismDriverSignature
    {
        /// <summary>
        /// The signature status of the driver is unknown. DISM only checks for a valid signature for boot-critical drivers.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The driver is unsigned.
        /// </summary>
        Unsigned = 1,

        /// <summary>
        /// The driver is signed.
        /// </summary>
        Signed = 2,
    }

    /// <summary>
    /// Specifies whether a package can be installed to an offline image without booting the image.
    /// </summary>
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824791.aspx" />
    /// </remarks>
    public enum DismFullyOfflineInstallableType
    {
        /// <summary>
        /// The package can be installed to an offline image without booting the image.
        /// </summary>
        FullyOfflineInstallable = 0,

        /// <summary>
        /// You must boot into the image in order to complete installation of this package.
        /// </summary>
        FullyOfflineNotInstallable,

        /// <summary>
        /// You may have to boot the image in order to complete the installation of this package.
        /// </summary>
        FullyOfflineInstallableUndetermined,
    }

    /// <summary>
    /// Indicates whether an image is a bootable image type.
    /// </summary>
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824736.aspx" />
    /// </remarks>
    public enum DismImageBootable
    {
        /// <summary>
        /// The image is bootable.
        /// </summary>
        ImageBootableYes = 0,

        /// <summary>
        /// The image is not bootable.
        /// </summary>
        ImageBootableNo,

        /// <summary>
        /// The image type is unknown.
        /// </summary>
        ImageBootableUnknown,
    }

    /// <summary>
    /// Specifies whether an image is corrupted.
    /// </summary>
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824761.aspx" />
    /// </remarks>
    public enum DismImageHealthState
    {
        /// <summary>
        /// The image is not corrupted.
        /// </summary>
        Healthy = 0,

        /// <summary>
        /// The image is corrupted but can be repaired.
        /// </summary>
        Repairable,

        /// <summary>
        /// The image is corrupted and cannot be repaired. Discard the image and start again.
        /// </summary>
        NonRepairable,
    }

    /// <summary>
    /// Specifies the file type of the Windows® image container.
    /// </summary>
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824762.aspx" />
    /// </remarks>
    public enum DismImageType
    {
        /// <summary>
        /// The file type is unsupported. The image must be in a .wim, .vhd, or .vhdx file.
        /// </summary>
        Unsupported = -1,

        /// <summary>
        /// The image is in a .wim file.
        /// </summary>
        Wim = 0,

        /// <summary>
        /// The image is in a .vhd or .vhdx file.
        /// </summary>
        Vhd = 1,
    }

    /// <summary>
    /// Specifies the kind of information that is reported in the log file.
    /// </summary>
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824757.aspx" />
    /// </remarks>
    public enum DismLogLevel
    {
        /// <summary>
        /// Log file only contains errors.
        /// </summary>
        LogErrors = 0,

        /// <summary>
        /// Log file contains errors and warnings.
        /// </summary>
        LogErrorsWarnings,

        /// <summary>
        /// Log file contains errors, warnings, and additional information.
        /// </summary>
        LogErrorsWarningsInfo,

        /// <summary>
        /// Undocumented.
        /// </summary>
        Debug
    }

    /// <summary>
    /// Specifies options when mounting an image.
    /// </summary>
    [Flags]
    public enum DismMountImageOptions : uint
    {
        /// <summary>
        /// Indicates to use no options.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the image should be mounted with optimization. When the optimize option is used, only the top level of the file directory in the image will be mapped to the mount location. The first time that you access a file path that is not initially mapped, that branch of the directory will be mounted. As a result, there may be an increase in the time that is required to access a directory for the first time after mounting an image using the optimize option.
        /// </summary>
        Optimize = DismApi.DISM_MOUNT_OPTIMIZE,

        /// <summary>
        /// Indicates to set a flag on the image specifying whether the image is corrupted.
        /// </summary>
        CheckIntegrity = DismApi.DISM_MOUNT_CHECK_INTEGRITY,
    }

    /// <summary>
    /// Specifies whether an image is mounted as read-only or as read-write.
    /// </summary>
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824752.aspx" />
    /// </remarks>
    public enum DismMountMode : uint
    {
        /// <summary>
        /// Mounts an image in read-write mode.
        /// </summary>
        ReadWrite = DismApi.DISM_MOUNT_READWRITE,

        /// <summary>
        /// Mounts an image in read-only mode.
        /// </summary>
        ReadOnly = DismApi.DISM_MOUNT_READONLY,
    }

    /// <summary>
    /// Indicates whether a mounted image needs to be remounted.
    /// </summary>
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824794.aspx" />
    /// </remarks>
    public enum DismMountStatus
    {
        /// <summary>
        /// Indicates that the mounted image is mounted and ready for servicing.
        /// </summary>
        Ok = 0,

        /// <summary>
        /// Indicates that the mounted image needs to be remounted before being serviced.
        /// </summary>
        NeedsRemount,

        /// <summary>
        /// Indicates that the mounted image is corrupt and is in an invalid state.
        /// </summary>
        Invalid,
    }

    /// <summary>
    /// Specifies the state of a package or a feature.
    /// </summary>
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824765.aspx" />
    /// </remarks>
    public enum DismPackageFeatureState : uint
    {
        /// <summary>
        /// The package or feature is not present.
        /// </summary>
        NotPresent = 0,

        /// <summary>
        /// An uninstall process for the package or feature is pending. Additional processes are pending and must be completed before the package or feature is successfully uninstalled.
        /// </summary>
        UninstallPending,

        /// <summary>
        /// The package or feature is staged.
        /// </summary>
        Staged,

        /// <summary>
        /// Metadata about the package or feature has been added to the system, but the package or feature is not present.
        /// </summary>
        Resolved,

        /// <summary>
        /// Metadata about the package or feature has been added to the system, but the package or feature is not present.
        /// </summary>
        Removed = Resolved,

        /// <summary>
        /// The package or feature is installed.
        /// </summary>
        Installed,

        /// <summary>
        /// The install process for the package or feature is pending. Additional processes are pending and must be completed before the package or feature is successfully installed.
        /// </summary>
        InstallPending,

        /// <summary>
        /// The package or feature has been superseded by a more recent package or feature.
        /// </summary>
        Superseded,

        /// <summary>
        /// The package or feature is partially installed. Some parts of the package or feature have not been installed.
        /// </summary>
        PartiallyInstalled,
    }

    /// <summary>
    /// Specifies the processor architecture of the image.
    /// </summary>
    public enum DismProcessorArchitecture
    {
        /// <summary>
        /// The processor architecture is unknown.
        /// </summary>
        None = -1,

        /// <summary>
        /// The image contains the Intel architecture.
        /// </summary>
        Intel = 0,

        /// <summary>
        /// The image contains the IA64 architecture.
        /// </summary>
        IA64 = 6,

        /// <summary>
        /// The image contains the AMD64 architecture.
        /// </summary>
        AMD64 = 9,

        /// <summary>
        /// The image contains the ARM architecture.
        /// </summary>
        ARM = 5,

        /// <summary>
        /// The image contains the ARM64 architecture.
        /// </summary>
        ARM64 = 12,

        /// <summary>
        /// A neutral processor architecture.
        /// </summary>
        Neutral = 11,
    }

    /// <summary>
    /// Specifies the release type of a package.
    /// </summary>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824783.aspx" />
    public enum DismReleaseType
    {
        /// <summary>
        /// The package is a critical update.
        /// </summary>
        CriticalUpdate = 0,

        /// <summary>
        /// The package is a driver.
        /// </summary>
        Driver,

        /// <summary>
        /// The package is a feature pack.
        /// </summary>
        FeaturePack,

        /// <summary>
        /// The package is a hotfix.
        /// </summary>
        Hotfix,

        /// <summary>
        /// The package is a security update.
        /// </summary>
        SecurityUpdate,

        /// <summary>
        /// The package is a software update.
        /// </summary>
        SoftwareUpdate,

        /// <summary>
        /// The package is a general update.
        /// </summary>
        Update,

        /// <summary>
        /// The package is an update rollup.
        /// </summary>
        UpdateRollup,

        /// <summary>
        /// The package is a language pack.
        /// </summary>
        LanguagePack,

        /// <summary>
        /// The package is a foundation package.
        /// </summary>
        Foundation,

        /// <summary>
        /// The package is a service pack.
        /// </summary>
        ServicePack,

        /// <summary>
        /// The package is a product release.
        /// </summary>
        Product,

        /// <summary>
        /// The package is a local pack.
        /// </summary>
        LocalPack,

        /// <summary>
        /// The package is another type of release.
        /// </summary>
        Other,

        /// <summary>
        /// This package is a feature on demand.
        /// </summary>
        OnDemandPack,
    }

    /// <summary>
    /// Specifies whether a restart is required after enabling a feature or installing a package.
    /// </summary>
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824749.aspx" />
    /// </remarks>
    public enum DismRestartType
    {
        /// <summary>
        /// No restart is required.
        /// </summary>
        No = 0,

        /// <summary>
        /// This package or feature might require a .
        /// </summary>
        Possible,

        /// <summary>
        /// This package or feature always requires a .
        /// </summary>
        Required,
    }

    /// <summary>
    /// Specifies whether an image is identified by name or by index number.
    /// </summary>
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824748.aspx" />
    /// </remarks>
    internal enum DismImageIdentifier
    {
        /// <summary>
        /// Identify the image by index number.
        /// </summary>
        ImageIndex = 0,

        /// <summary>
        /// Identify the image by name.
        /// </summary>
        ImageName,
    }

    /// <summary>
    /// Specifies whether a package is identified by name or by file path.
    /// </summary>
    /// <remarks>
    /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824781.aspx" />
    /// </remarks>
    internal enum DismPackageIdentifier
    {
        /// <summary>
        /// No package is specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// The package is identified by its name.
        /// </summary>
        Name,

        /// <summary>
        /// The package is specified by its path.
        /// </summary>
        Path,
    }
}
