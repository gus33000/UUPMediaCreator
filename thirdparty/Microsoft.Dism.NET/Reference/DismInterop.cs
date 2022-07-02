using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Dism.Reference
{
    public enum LogLevel
    {
        Errors,
        Warnings,
        WarningsInfo,
        Debug
    }

    public enum ImageIdentifier
    {
        ImageIndex,
        ImageName
    }

    public enum ImageHealthState
    {
        Healthy,
        Repairable,
        NonRepairable
    }

    public enum PackageIdentifier
    {
        None,
        Name,
        Path
    }

    public enum StubPackageOption
    {
        None,
        InstallFull,
        InstallStub,
        UserPreference
    }

    public enum DismRegistryHive
    {
        DismRegistrySoftware,
        DismRegistrySystem,
        DismRegistrySecurity,
        DismRegistrySAM,
        DismRegistryDefault,
        DismRegistryHKCU,
        DismRegistryComponents,
        DismRegistryDrivers
    }

    public enum PackageFeatureState
    {
        NotPresent,
        UninstallPending,
        Staged,
        Resolved = Removed,
        Removed = 3,
        Installed,
        InstallPending,
        Superseded,
        PartiallyInstalled
    }

    public enum ReleaseType
    {
        CriticalUpdate,
        Driver,
        FeaturePack,
        Hotfix,
        SecurityUpdate,
        SoftwareUpdate,
        Update,
        UpdateRollup,
        LanguagePack,
        Foundation,
        ServicePack,
        Product,
        LocalPack,
        Other,
        OnDemandPack
    }

    public enum RestartType
    {
        No,
        Possible,
        Required
    }

    public enum CompletelyOfflineCapableType
    {
        Yes,
        No,
        Undetermined
    }

    public enum DismImageType
    {
        Unsupported = -1,
        Wim,
        Vhd
    }

    public enum MountMode
    {
        ReadWrite,
        ReadOnly
    }

    public enum MountStatus
    {
        Ok,
        NeedsRemount,
        Invalid
    }

    public enum ImageBootable
    {
        Yes,
        No,
        Unknown
    }

    public enum DriverSignature
    {
        Unknown,
        Unsigned,
        Signed
    }

    internal class DismInterop
    {
        [DllImport("dismapi.dll")]
        public static extern int DismInitialize(
            LogLevel LogLevel,
            [MarshalAs(UnmanagedType.LPWStr)] string LogFilePath,
            [MarshalAs(UnmanagedType.LPWStr)] string ScratchDirectory
        );

        [DllImport("dismapi.dll")]
        public static extern int DismShutdown();

        [DllImport("dismapi.dll")]
        public static extern int DismOpenSession(
            [MarshalAs(UnmanagedType.LPWStr)] string ImagePath,
            [MarshalAs(UnmanagedType.LPWStr)] string WindowsDirectory,
            [MarshalAs(UnmanagedType.LPWStr)] string SystemDrive,
            out uint Session
        );

        [DllImport("dismapi.dll")]
        public static extern int DismMountImage(
            [MarshalAs(UnmanagedType.LPWStr)] string ImagePath,
            [MarshalAs(UnmanagedType.LPWStr)] string MountPath,
            uint ImageIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string ImageName,
            ImageIdentifier ImageIdentifier,
            uint MountFlags,
            SafeWaitHandle CancelHandle,
            ProgressCallback Progress,
            IntPtr UserData
        );

        [DllImport("dismapi.dll")]
        public static extern int DismCloseSession(uint Session);

        [DllImport("dismapi.dll")]
        public static extern int DismUnmountImage(
            [MarshalAs(UnmanagedType.LPWStr)] string MountPath,
            uint UnmountFlags,
            SafeWaitHandle CancelHandle,
            ProgressCallback Progress,
            IntPtr UserData
        );

        [DllImport("dismapi.dll")]
        public static extern int DismRemountImage([MarshalAs(UnmanagedType.LPWStr)] string MountPath);

        [DllImport("dismapi.dll")]
        public static extern int DismCommitImage(uint Session, uint Flags, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int DismGetImageInfo([MarshalAs(UnmanagedType.LPWStr)] string ImageFilePath, out IntPtr ImageInfoBufPtr, out uint ImageInfoCount);

        [DllImport("dismapi.dll")]
        public static extern int DismGetMountedImageInfo(out IntPtr MountedImageInfoBufPtr, out uint MountedImageInfoCount);

        [DllImport("dismapi.dll")]
        public static extern int DismCleanupMountpoints();

        [DllImport("dismapi.dll")]
        public static extern int DismDelete(IntPtr DismStructure);

        [DllImport("dismapi.dll")]
        public static extern int DismGetLastErrorMessage(out IntPtr ErrorMessage);

        [DllImport("dismapi.dll")]
        public static extern int DismCheckImageHealth(uint Session, bool ScanImage, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData, out ImageHealthState ImageHealth);

        [DllImport("dismapi.dll")]
        public static extern int DismRestoreImageHealth(uint Session, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)] string[] SourcePaths, uint SourcePathCount, bool LimitAccess, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int DismAddPackage(
            uint Session,
            [MarshalAs(UnmanagedType.LPWStr)] string PackagePath,
            bool IgnoreCheck,
            bool PreventPending,
            SafeWaitHandle CancelHandle,
            ProgressCallback Progress,
            IntPtr UserData
        );

        [DllImport("dismapi.dll")]
        public static extern int DismRemovePackage(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string Identifier, PackageIdentifier PackageIdentifier, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int DismEnableFeature(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string FeatureName, [MarshalAs(UnmanagedType.LPWStr)] string Identifier, PackageIdentifier PackageIdentifier, bool LimitAccess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 6)] string[] SourcePaths, uint SourcePathCount, bool EnableAll, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int DismDisableFeature(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string FeatureName, [MarshalAs(UnmanagedType.LPWStr)] string PackageName, bool RemovePayload, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int DismGetPackages(uint Session, out IntPtr PackageBufPtr, out uint PackageCount);

        [DllImport("dismapi.dll")]
        public static extern int DismGetPackageInfo(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string Identifier, PackageIdentifier PackageIdentifier, out IntPtr PackageInfoPtr);

        [DllImport("dismapi.dll")]
        public static extern int DismGetPackageInfoEx(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string Identifier, PackageIdentifier PackageIdentifier, out IntPtr PackageInfoExPtr);

        [DllImport("dismapi.dll")]
        public static extern int DismGetFeatures(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string Identifier, PackageIdentifier PackageIdentifier, out IntPtr FeatureBufPtr, out uint FeatureCount);

        [DllImport("dismapi.dll")]
        public static extern int DismGetFeatureInfo(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string FeatureName, [MarshalAs(UnmanagedType.LPWStr)] string Identifier, PackageIdentifier PackageIdentifier, out IntPtr FeatureInfoPtr);

        [DllImport("dismapi.dll")]
        public static extern int DismApplyUnattend(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string UnattendFile, bool SingleSession);

        [DllImport("dismapi.dll")]
        public static extern int DismAddDriver(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string DriverPath, bool ForceUnsigned);

        [DllImport("dismapi.dll")]
        public static extern int DismRemoveDriver(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string DriverPath);

        [DllImport("dismapi.dll")]
        public static extern int DismGetDrivers(uint Session, bool AllDrivers, out IntPtr DriverPackageBufPtr, out uint DriverPackageCount);

        [DllImport("dismapi.dll")]
        public static extern int DismGetDriverInfo(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string DriverPath, out IntPtr DriverBufPtr, out uint DriverCount, out IntPtr DriverPackageBufPtr);

        [DllImport("dismapi.dll")]
        public static extern int _DismExportDriver(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string Destination, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int _DismGetOsInfo(uint Session, out IntPtr BufPtr);

        [DllImport("dismapi.dll")]
        public static extern int _DismSetEdition(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string EditionID, [MarshalAs(UnmanagedType.LPWStr)] string ProductKey, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int _DismSetProductKey(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string ProductKey);

        [DllImport("dismapi.dll")]
        public static extern int _DismGetCurrentEdition(uint Session, out IntPtr EditionIdStringBuf);

        [DllImport("dismapi.dll")]
        public static extern int _DismGetTargetEditions(uint Session, out IntPtr EditionIdStringBuf, out uint EditionIdCount);

        [DllImport("dismapi.dll")]
        public static extern int _DismGetCompositionEditions(uint Session, out IntPtr EditionIdStringBuf, out uint EditionIdCount);

        [DllImport("dismapi.dll")]
        public static extern int _DismGetVirtualEditions(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string CompositionEditionId, out IntPtr EditionIdStringBuf, out uint EditionIdCount);

        [DllImport("dismapi.dll")]
        public static extern int _DismOptimizeImage(uint Session, uint Flags, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int _DismAddProvisionedAppxPackage(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string AppPath, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 3)] string[] DependencyPackages, uint DependencyPackageCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 5)] string[] OptionalPackages, uint OptionalPackageCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 7)] string[] LicensePaths, uint LicensePathCount, bool SkipLicense, [MarshalAs(UnmanagedType.LPWStr)] string CustomDataPath, [MarshalAs(UnmanagedType.LPWStr)] string Regions, StubPackageOption stubPackageOption);

        [DllImport("dismapi.dll")]
        public static extern int _DismRemoveProvisionedAppxPackage(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string PackageName);

        [DllImport("dismapi.dll")]
        public static extern int _DismRemoveProvisionedAppxPackageAllUsers(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string PackageName, ref bool ErrorsEncountered);

        [DllImport("dismapi.dll")]
        public static extern int _DismGetProvisionedAppxPackages(uint Session, out IntPtr PackageBufPtr, out uint PackageCount);

        [DllImport("dismapi.dll")]
        public static extern int _DismSetAppXProvisionedDataFile(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string PackageName, [MarshalAs(UnmanagedType.LPWStr)] string DataFilePath);

        [DllImport("dismapi.dll")]
        public static extern int _DismOptimizeProvisionedAppxPackages(uint Session);

        [DllImport("dismapi.dll")]
        public static extern int _DismExportSource(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string RecipeFile, [MarshalAs(UnmanagedType.LPWStr)] string SourcePath, [MarshalAs(UnmanagedType.LPWStr)] string TargetPath, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int _DismExportSourceEx(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string RecipeFile, [MarshalAs(UnmanagedType.LPWStr)] string SourcePath, [MarshalAs(UnmanagedType.LPWStr)] string TargetPath, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int DismAddCapability(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string FeatureName, bool LimitAccess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 5)] string[] SourcePaths, uint SourcePathCount, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int _DismAddCapabilityEx(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string RecipeFile, bool LimitAccess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 5)] string[] SourcePaths, uint SourcePathCount, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int DismRemoveCapability(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string FeatureName, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int DismGetCapabilities(uint Session, out IntPtr CapabilityBufPtr, out uint CapabilityCount);

        [DllImport("dismapi.dll")]
        public static extern int _DismGetCapabilitiesEx(uint Session, bool LimitAccess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 4)] string[] SourcePaths, uint SourcePathCount, out IntPtr CapabilityBufPtr, out uint CapabilityCount);

        [DllImport("dismapi.dll")]
        public static extern int DismGetCapabilityInfo(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string FeatureName, out IntPtr CapabilityInfoPtr);

        [DllImport("dismapi.dll")]
        public static extern int _DismGetCapabilityInfoEx(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string FeatureName, bool LimitAccess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 5)] string[] SourcePaths, uint SourcePathCount, out IntPtr CapabilityInfoPtr);

        [DllImport("dismapi.dll")]
        public static extern int _DismAddPackageFamilyToUninstallBlocklist(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string packageFamily);

        [DllImport("dismapi.dll")]
        public static extern int _DismRemovePackageFamilyFromUninstallBlocklist(uint Session, [MarshalAs(UnmanagedType.LPWStr)] string packageFamily);

        [DllImport("dismapi.dll")]
        public static extern int _DismGetNonRemovableAppsPolicy(uint Session, out IntPtr PackageFamilyBufPtr, out uint PackageFamilyCount);

        [DllImport("dismapi.dll")]
        public static extern int _DismApplyCustomDataImage([MarshalAs(UnmanagedType.LPWStr)] string CustomDataImage, [MarshalAs(UnmanagedType.LPWStr)] string ImagePath, uint Flags, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int _DismCleanImage(uint Session, uint Type, uint Flags, SafeWaitHandle CancelHandle, ProgressCallback Progress, IntPtr UserData);

        [DllImport("dismapi.dll")]
        public static extern int _DismGetRegistryMountPoint(uint Session, DismRegistryHive RegistryHive, out IntPtr RegistryMountPointDismString);

        [DllImport("dismapi.dll")]
        public static extern int _DismInitiateOSUninstall(uint Session, uint Reason);

        [DllImport("dismapi.dll")]
        public static extern int DismGetReservedStorageState(uint Session, out uint State);

        [DllImport("dismapi.dll")]
        public static extern int DismSetReservedStorageState(uint Session, uint State);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetProcessHeap();

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint FormatMessage(uint Flags, IntPtr Source, uint MessageId, uint LanguageId, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer, uint Size, IntPtr Arguments);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint GetLastError();

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetModuleHandleEx(uint Flags, string ModuleName, ref IntPtr ModuleHandle);

        [DllImport("Advapi32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint Flags, out IntPtr TokenHandle);

        [DllImport("Advapi32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "LookupPrivilegeValueW", SetLastError = true)]
        public static extern bool LookupPrivilegeValue([MarshalAs(UnmanagedType.LPWStr)] string SystemName, [MarshalAs(UnmanagedType.LPWStr)] string Name, out long Luid);

        [DllImport("Advapi32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, uint BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        [DllImport("Advapi32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "InitiateSystemShutdownExW", SetLastError = true)]
        public static extern bool InitiateSystemShutdownEx([MarshalAs(UnmanagedType.LPWStr)] string MachineName, [MarshalAs(UnmanagedType.LPWStr)] string Message, uint Timeout, [MarshalAs(UnmanagedType.Bool)] bool ForceAppsClosed, [MarshalAs(UnmanagedType.Bool)] bool RebootAfterShutdown, uint Reason);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern long RtlGetVersion(ref RTL_OSVERSIONINFOEXW osVersionInfo);

        public static string DismOnlineImage = "DISM_{53BFAE52-B167-4E2F-A258-0A37B57FF845}";

        public static uint DismMountReadWriteFlag = 0U;

        public static uint DismMountReadOnlyFlag = 1U;

        public static uint DismMountOptimizedFlag = 2U;

        public static uint DismMountCheckIntegrityFlag = 4U;

        public static uint DismMountSupportEaFlag = 8U;

        public static uint DismCommitImageFlag = 0U;

        public static uint DismDiscardImageFlag = 1U;

        public static uint DismOptimizeImageWIMBootFlag = 1U;

        public static uint DismOptimizeImageBootFlag = 2U;

        public static uint DismCommitGenerateIntegrityFlag = 65536U;

        public static uint DismCommitAppendFlag = 131072U;

        public static uint DismCommitSupportEaFlag = 262144U;

        public static uint DismCleanupTypeNone = 0U;

        public static uint DismCleanupTypeWindowsUpdate = 1U;

        public static uint DismCleanupTypeServicePack = 2U;

        public static uint DismCleanupTypeComponent = 4U;

        public static uint DismCleanupFlagComponentResetBaseDefault = 1U;

        public static uint DismCleanupFlagComponentResetBaseDefer = 2U;

        public static uint DismRestartNeeded = 3010U;

        public static string SeShutdownPrivilege = "SeShutdownPrivilege";

        public const uint TOKEN_PRIVILEGES_SIZE = 16U;

        public const uint ERROR_SUCCESS = 0U;

        public const uint ERROR_ALREADY_EXISTS = 183U;

        public const uint ERROR_NOT_READY = 21U;

        public const uint ERROR_SHUTDOWN_IN_PROGRESS = 1115U;

        public const uint ERROR_MACHINE_LOCKED = 1271U;

        public const uint ERROR_ELEVATION_REQUIRED = 2147943140U;

        public const uint ERROR_ELEVATION_REQUIRED_WIN32_CODE = 740U;

        public const uint TOKEN_ADJUST_PRIVILEGES = 32U;

        public const uint TOKEN_QUERY = 8U;

        public const uint SE_PRIVILEGE_ENABLED = 2U;

        public const uint SHTDN_REASON_MAJOR_OPERATINGSYSTEM = 131072U;

        public const uint SHTDN_REASON_FLAG_PLANNED = 2147483648U;

        public const uint SHTDN_REASON_MINOR_UPGRADE = 3U;

        public const uint STATUS_SUCCESS = 0U;

        public const uint DISM_APPLY_CUSTOM_DATA_IMAGE_FLAG_SINGLE_INSTANCE = 1U;

        public const uint GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT = 2U;

        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 512U;

        public const uint FORMAT_MESSAGE_FROM_HMODULE = 2048U;

        public const uint FORMAT_MESSAGE_FROM_SYSTEM = 4096U;

        public const uint LANGID_NEUTRAL_DEFAULT = 1024U;

        public delegate void ProgressCallback(uint Current, uint Total, IntPtr UserData);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismOsInfo
        {
            public int OsState;

            public uint Architecture;

            public uint MajorVersion;

            public uint MinorVersion;

            public uint Build;

            public uint RevisionNumber;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string WindowsDirectory;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string BootDrive;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class SYSTEMTIME
        {
            public ushort wYear;

            public ushort wMonth;

            public ushort wDayOfWeek;

            public ushort wDay;

            public ushort wHour;

            public ushort wMinute;

            public ushort wSecond;

            public ushort wMilliseconds;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismPackage
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string PackageName;

            public PackageFeatureState PackageState;

            public ReleaseType ReleaseType;

            public SYSTEMTIME InstallTime;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismCustomProperty
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Name;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Value;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Path;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismFeature
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string FeatureName;

            public PackageFeatureState State;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismCapability
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Name;

            public PackageFeatureState State;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismPackageInfo
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string PackageName;

            public PackageFeatureState PackageState;

            public ReleaseType ReleaseType;

            public SYSTEMTIME InstallTime;

            public bool Applicable;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Copyright;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Company;

            public SYSTEMTIME CreationTime;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string DisplayName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Description;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string InstallClient;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string InstallPackageName;

            public SYSTEMTIME LastUpdateTime;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ProductName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ProductVersion;

            public RestartType RestartRequired;

            public CompletelyOfflineCapableType CompletelyOfflineCapable;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string SupportInformation;

            public IntPtr CustomPropertyBuf;

            public uint CustomPropertyCount;

            public IntPtr FeatureBuf;

            public uint FeatureCount;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string CapabilityId;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismFeatureInfo
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string FeatureName;

            public PackageFeatureState State;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string DisplayName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Description;

            public RestartType RestartRequired;

            public IntPtr CustomPropertyBuf;

            public uint CustomPropertyCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismCapabilityInfo
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Name;

            public PackageFeatureState State;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string DisplayName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Description;

            public uint DownloadSize;

            public uint InstallSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismString
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Value;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismLanguage
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Value;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismWimCustomizedInfo
        {
            public uint Size;

            public uint DirectoryCount;

            public uint FileCount;

            public SYSTEMTIME CreatedTime;

            public SYSTEMTIME ModifiedTime;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismImageInfo
        {
            public DismImageType ImageType;

            public uint ImageIndex;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ImageName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ImageDescription;

            public ulong ImageSize;

            public uint Architecture;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ProductName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string EditionId;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string InstallationType;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Hal;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ProductType;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ProductSuite;

            public uint MajorVersion;

            public uint MinorVersion;

            public uint Build;

            public uint SpBuild;

            public uint SpLevel;

            public ImageBootable Bootable;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string SystemRoot;

            public IntPtr Languages;

            public uint LanguageCount;

            public uint DefaultLanguageIndex;

            public IntPtr CustomizedInfo;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismMountedImageInfo
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string MountPath;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ImageFilePath;

            public uint ImageIndex;

            public MountMode MountMode;

            public MountStatus MountStatus;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismDriverPackage
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string PublishedName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string OriginalFileName;

            public bool InBox;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string CatalogFile;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ClassName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ClassGuid;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ClassDescription;

            public bool BootCritical;

            public DriverSignature DriverSignature;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ProviderName;

            public SYSTEMTIME Date;

            public uint MajorVersion;

            public uint MinorVersion;

            public uint Build;

            public uint Revision;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismDriver
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ManufacturerName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string HardwareDescription;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string HardwareId;

            public uint Architecture;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ServiceName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string CompatibleIds;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ExcludeIds;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DismAppxPackage
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string PackageName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string DisplayName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string PublisherId;

            public uint MajorVersion;

            public uint MinorVersion;

            public uint Build;

            public uint Revision;

            public uint Architecture;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string ResourceId;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string InstallLocation;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Regions;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct RTL_OSVERSIONINFOEXW
        {
            public uint OSVersionInfoSize;

            public uint MajorVersion;

            public uint MinorVersion;

            public uint BuildNumber;

            public uint PlatformId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string CSDVersion;

            public ushort ServicePackMajor;

            public ushort ServicePackMinor;

            public ushort SuiteMask;

            public byte ProductType;

            public byte Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;

            public long Luid;

            public uint Attributes;
        }
    }
}