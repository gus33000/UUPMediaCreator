/*
 * Copyright (c) Gustave Monce and Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting
{
    public class CTAC
    {
        public string DeviceAttributes
        {
            get; set;
        }
        public string CallerAttributes
        {
            get; set;
        }
        public string Products
        {
            get; set;
        }
        public bool SyncCurrentVersionOnly
        {
            get; set;
        }

        public CTAC()
        {
        }

        public CTAC(OSSkuId ReportingSku,
                    string ReportingVersion,
                    MachineType MachineType,
                    string FlightRing,
                    string FlightingBranchName,
                    string BranchReadinessLevel,
                    string CurrentBranch,
                    string ReleaseType,
                    bool SyncCurrentVersionOnly,
                    bool IsStore = false,
                    string ContentType = "Mainline",
                    bool IsVbsEnabled = true) : base()
        {
            BuildCTAC(ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, IsStore, ContentType, IsVbsEnabled);
        }

        private void BuildCTAC(
            OSSkuId ReportingSku,
            string ReportingVersion,
            MachineType MachineType,
            string FlightRing,
            string FlightingBranchName,
            string BranchReadinessLevel,
            string CurrentBranch,
            string ReleaseType,
            bool SyncCurrentVersionOnly,
            bool IsStore = false,
            string content = "Mainline",
            bool IsVbsEnabled = true
        )
        {
            int flightEnabled = FlightRing == "Retail" ? 0 : 1;
            string App = IsStore ? "WU_STORE" : "WU_OS";

            string InstallType = "Client";
            string ReportingPFN = "Client.OS.rs2";
            string DeviceFamily = "Windows.Desktop";

            if (ReportingSku == OSSkuId.Holographic)
            {
                InstallType = "FactoryOS";
                ReportingPFN = "HOLOLENS.OS.rs2";
                DeviceFamily = "Windows.Holographic";
            }
            else if (ReportingSku == OSSkuId.IoTUAP)
            {
                InstallType = "IoTUAP";
                ReportingPFN = "IoTCore.OS.rs2";
                DeviceFamily = "Windows.IoTUAP";
            }
            else if (ReportingSku == OSSkuId.MobileCore)
            {
                InstallType = "MobileCore";
                ReportingPFN = "Mobile.OS.rs2";
                DeviceFamily = "Windows.Mobile";
            }
            else if (ReportingSku == OSSkuId.Lite)
            {
                InstallType = "FactoryOS";
                ReportingPFN = "WCOSDevice0.OS";
                DeviceFamily = "Windows.Core";
            }
            else if (ReportingSku == OSSkuId.Andromeda)
            {
                InstallType = "FactoryOS";
                ReportingPFN = "WCOSDevice1.OS";
                DeviceFamily = "Windows.Core";
            }
            else if (ReportingSku == OSSkuId.HubOS)
            {
                InstallType = "FactoryOS";
                ReportingPFN = "WCOSDevice2.OS";
                DeviceFamily = "Windows.Core";
            }
            else if (ReportingSku.ToString().Contains("Server", StringComparison.InvariantCultureIgnoreCase) && ReportingSku.ToString().Contains("Core", StringComparison.InvariantCultureIgnoreCase))
            {
                InstallType = "Server Core";
                ReportingPFN = "Server.OS";
                DeviceFamily = "Windows.Server";
            }
            else if (ReportingSku.ToString().Contains("Server", StringComparison.InvariantCultureIgnoreCase) && !ReportingSku.ToString().Contains("Core", StringComparison.InvariantCultureIgnoreCase))
            {
                InstallType = "Server";
                ReportingPFN = "Server.OS";
                DeviceFamily = "Windows.Server";
            }
            else if (ReportingSku == OSSkuId.PPIPro)
            {
                DeviceFamily = "Windows.Team";
            }

            DeviceAttributes = "E:IsContainerMgrInstalled=1&" +
                                    $"FlightRing={FlightRing}&" +
                                    "TelemetryLevel=3&" +
                                    "HidOverGattReg=C:\\WINDOWS\\System32\\DriverStore\\FileRepository\\hidbthle.inf_amd64_0fc6b7cd4ccbc55c\\Microsoft.Bluetooth.Profiles.HidOverGatt.dll&" +
                                    "AppVer=0.0.0.0&" +
                                    "IsAutopilotRegistered=0&" +
                                    "ProcessorIdentifier=Intel64 Family 6 Model 151 Stepping 2&" +
                                    "OEMModel=RM-1085_1045&" +
                                    "ProcessorManufacturer=GenuineIntel&" +
                                    "InstallDate=1577722757&" +
                                    "OEMModelBaseBoard=OEM Board Name&" +
                                    $"BranchReadinessLevel={BranchReadinessLevel}&" +
                                    "DataExpDateEpoch_20H1=1593425114&" +
                                    "IsCloudDomainJoined=0&" +
                                    "Bios=2019&" +
                                    "DchuAmdGrfxVen=4098&" +
                                    "IsDeviceRetailDemo=0&" +
                                    $"FlightingBranchName={FlightingBranchName}&" +
                                    "OSUILocale=en-US&" +
                                    $"DeviceFamily={DeviceFamily}&" +
                                    "UpgEx_20H1=Green&" +
                                    $"WuClientVer={ReportingVersion}&" +
                                    $"IsFlightingEnabled={flightEnabled}&" +
                                    $"OSSkuId={(int)ReportingSku}&" +
                                    "GStatus_20H1=2&" +
                                    $"App={App}&" +
                                    $"CurrentBranch={CurrentBranch}&" +
                                    "InstallLanguage=en-US&" +
                                    "OEMName_Uncleaned=MICROSOFTMDG&" +
                                    $"InstallationType={InstallType}&" +
                                    "AttrDataVer=264&" +
                                    "IsEdgeWithChromiumInstalled=1&" +
                                    "TimestampEpochString_20H1=1593425114&" +
                                    $"OSVersion={ReportingVersion}&" +
                                    "IsMDMEnrolled=0&" +
                                    "TencentType=1&" +
                                    $"FlightContent={content}&" +
                                    "ActivationChannel=Retail&" +
                                    "Steam=URL:steam protocol&" +
                                    "Free=gt64&" +
                                    "TencentReg=79 d0 01 d7 9f 54 d5 01&" +
                                    "FirmwareVersion=7704&" +
                                    "DchuAmdGrfxExists=1&" +
                                    "SdbVer_20H1=2000000000&" +
                                    "UpgEx_CO21H2=Green&" +
                                    //$"OSArchitecture={MachineType.ToString().ToUpper()}&" +
                                    $"OSArchitecture=AMD64&" +
                                    "DefaultUserRegion=244&" +
                                    $"ReleaseType={ReleaseType}&" +
                                    "UpdateManagementGroup=2&" +
                                    "MobileOperatorCommercialized=000-88&" +
                                    "PhoneTargetingName=Lumia 950 XL&" +
                                    "AllowInPlaceUpgrade=1&" +
                                    "AllowUpgradesWithUnsupportedTPMOrCPU=1&" +
                                    "CloudPBR=1&" +
                                    "DataExpDateEpoch_19H1=1593425114&" +
                                    "DataExpDateEpoch_21H1=1593425114&" +
                                    "DataExpDateEpoch_21H2=1593425114&" +
                                    "DataExpDateEpoch_CO21H2=1593425114&" +
                                    "DataExpDateEpoch_CO21H2Setup=1593425114&" +
                                    "DataVer_RS5=2000000000&" +
                                    "DUScan=1&" +
                                    "EKB19H2InstallCount=1&" +
                                    "EKB19H2InstallTimeEpoch=1255000000&" +
                                    "GenTelRunTimestamp_19H1=1593425114&" +
                                    "GStatus_19H1=2&" +
                                    "GStatus_19H1Setup=2&" +
                                    "GStatus_20H1Setup=2&" +
                                    "GStatus_21H2=2&" +
                                    "GStatus_CO21H2=2&" +
                                    "GStatus_CO21H2Setup=2&" +
                                    "GStatus_RS5=2&" +
                                    "MediaBranch=&" +
                                    "ProcessorModel=12th Gen Intel(R) Core(TM) i9-12900K&" +
                                    "SdbVer_19H1=2000000000&" +
                                    "SecureBootCapable=1&" +
                                    "TimestampEpochString_19H1=1593425114&" +
                                    "TimestampEpochString_21H1=1593425114&" +
                                    "TimestampEpochString_21H2=1593425114&" +
                                    "TimestampEpochString_CO21H2=1593425114&" +
                                    "TimestampEpochString_CO21H2Setup=1593425114&" +
                                    "TPMVersion=2&" +
                                    "UpdateOfferedDays=0&" +
                                    "UpgEx_19H1=Green&" +
                                    "UpgEx_21H1=Green&" +
                                    "UpgEx_21H2=Green&" +
                                    "UpgEx_NI22H2=Green&" +
                                    "UpgEx_RS5=Green&" +
                                    "UpgradeEligible=1&" +
                                    "Version_RS5=2000000000&" +
                                    $"IsRetailOS={FlightRing == "Retail"}&" +
                                    $"MediaVersion={ReportingVersion}" +
                                    $"IsVbsEnabled={(IsVbsEnabled ? 1 : 0)}";

            if (ReportingSku is OSSkuId.EnterpriseS or OSSkuId.EnterpriseSN || ReportingSku.ToString().Contains("Server", StringComparison.InvariantCultureIgnoreCase))
            {
                DeviceAttributes += "&BlockFeatureUpdates=1";
            }

            if (ReportingSku == OSSkuId.Holographic)
            {
                DeviceAttributes += $"&OneCoreFwV={ReportingVersion}&" +
                                        $"OneCoreSwV={ReportingVersion}&" +
                                        "OneCoreManufacturerModelName=HoloLens&" +
                                        "OneCoreManufacturer=Microsoft Corporation&" +
                                        "OneCoreOperatorName=000-88";
            }
            else if (ReportingSku == OSSkuId.HubOS)
            {
                DeviceAttributes += $"&OneCoreFwV={ReportingVersion}&" +
                                        $"OneCoreSwV={ReportingVersion}&" +
                                        "OneCoreManufacturerModelName=Surface Hub 2X&" +
                                        "OneCoreManufacturer=Microsoft Corporation&" +
                                        "OneCoreOperatorName=000-88";
            }
            else if (ReportingSku == OSSkuId.Andromeda)
            {
                DeviceAttributes += $"&OneCoreFwV={ReportingVersion}&" +
                                        $"OneCoreSwV={ReportingVersion}&" +
                                        "OneCoreManufacturerModelName=Andromeda&" +
                                        "OneCoreManufacturer=Microsoft Corporation&" +
                                        "OneCoreOperatorName=000-88";
            }
            else if (ReportingSku == OSSkuId.Lite)
            {
                DeviceAttributes += "&Product=ModernPC";
                DeviceAttributes += $"&OneCoreFwV={ReportingVersion}&" +
                                        $"OneCoreSwV={ReportingVersion}&" +
                                        "OneCoreManufacturerModelName=Santorini&" +
                                        "OneCoreManufacturer=Microsoft Corporation&" +
                                        "OneCoreOperatorName=000-88";
            }

            CallerAttributes = "E:Interactive=1&IsSeeker=1&SheddingAware=1&";
            if (IsStore)
            {
                CallerAttributes += "Acquisition=1&Id=Acquisition%3BMicrosoft.WindowsStore_8wekyb3d8bbwe&";
            }
            else
            {
                CallerAttributes += "Id=UpdateOrchestrator&";
            }
            Products = "";
            if (!IsStore)
            {
                Products = $"PN={ReportingPFN}.{MachineType}&Branch={CurrentBranch}&PrimaryOSProduct=1&Repairable=1&V={ReportingVersion};";
            }

            this.SyncCurrentVersionOnly = SyncCurrentVersionOnly;
        }
    }
}
