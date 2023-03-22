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
using System.Collections.Generic;
using System.Linq;

namespace WindowsUpdateLib
{
    public class CTAC
    {
        public string DeviceAttributes { get; set; }
        public string CallerAttributes { get; set; }
        public string Products { get; set; }
        public bool SyncCurrentVersionOnly { get; set; }

        private Dictionary<string, string> deviceAttributes = new Dictionary<string, string>();

        public CTAC() { }

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
                    string ContentType = "Mainline") : base()
        {
            BuildCTAC(ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, IsStore, ContentType);
        }

        private static string BuildDeviceAttributes(ICollection<KeyValuePair<string, string>> attributes) => "E:" + string.Join('&', attributes.Select(x => $"{x.Key}={x.Value}"));

        private void AddDeviceAttribute(string key, string value)
        {
            if (!deviceAttributes.ContainsKey(key))
            {
                deviceAttributes.Add(key, value);
            }
            else
            {
                deviceAttributes[key] = value;
            }
        }

        private void AddDeviceAttribute(string key, int value) => AddDeviceAttribute(key, value.ToString());

        private void AddDeviceAttribute(string key, bool value) => AddDeviceAttribute(key, value.ToString());

        public void AddDeviceAttributeAndRebuild(string key, string value)
        {
            AddDeviceAttribute(key, value);

            // Rebuild the device attributes
            DeviceAttributes = BuildDeviceAttributes(deviceAttributes);
        }

        private static (string InstallType, string ReportingPFN, string DeviceFamily) GetSkuSpecificParameters(OSSkuId ReportingSku)
        {
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

            return (InstallType, ReportingPFN, DeviceFamily);
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
            string content = "Mainline"
        )
        {
            string App = IsStore ? "WU_STORE" : "WU_OS";
            (string InstallType, string ReportingPFN, string DeviceFamily) = GetSkuSpecificParameters(ReportingSku);

            AddDeviceAttribute("ActivationChannel", "Retail");
            AddDeviceAttribute("AllowInPlaceUpgrade", 1);
            AddDeviceAttribute("AllowUpgradesWithUnsupportedTPMOrCPU", 1);
            AddDeviceAttribute("App", App);
            AddDeviceAttribute("AppVer", "0.0.0.0");
            AddDeviceAttribute("AttrDataVer", 177);
            AddDeviceAttribute("Bios", 2019);
            AddDeviceAttribute("BranchReadinessLevel", BranchReadinessLevel);
            AddDeviceAttribute("CloudPBR", 1);
            AddDeviceAttribute("CurrentBranch", CurrentBranch);
            AddDeviceAttribute("DataExpDateEpoch_19H1", 1593425114);
            AddDeviceAttribute("DataExpDateEpoch_20H1", 1593425114);
            AddDeviceAttribute("DataExpDateEpoch_21H1", 1593425114);
            AddDeviceAttribute("DataExpDateEpoch_21H2", 1593425114);
            AddDeviceAttribute("DataExpDateEpoch_CO21H2", 1593425114);
            AddDeviceAttribute("DataExpDateEpoch_CO21H2Setup", 1593425114);
            AddDeviceAttribute("DataVer_RS5", 2000000000);
            AddDeviceAttribute("DchuAmdGrfxExists", 1);
            AddDeviceAttribute("DchuAmdGrfxVen", 4098);
            AddDeviceAttribute("DefaultUserRegion", 244);
            AddDeviceAttribute("DeviceFamily", DeviceFamily);
            AddDeviceAttribute("DUScan", 1);
            AddDeviceAttribute("EKB19H2InstallCount", 1);
            AddDeviceAttribute("EKB19H2InstallTimeEpoch", 1255000000);
            AddDeviceAttribute("FirmwareVersion", 7704);
            AddDeviceAttribute("FlightContent", content);
            AddDeviceAttribute("FlightingBranchName", FlightingBranchName);
            AddDeviceAttribute("FlightRing", FlightRing);
            AddDeviceAttribute("Free", "gt64");
            AddDeviceAttribute("GenTelRunTimestamp_19H1", 1593425114);
            AddDeviceAttribute("GStatus_19H1", 2);
            AddDeviceAttribute("GStatus_19H1Setup", 2);
            AddDeviceAttribute("GStatus_20H1", 2);
            AddDeviceAttribute("GStatus_20H1Setup", 2);
            AddDeviceAttribute("GStatus_21H2", 2);
            AddDeviceAttribute("GStatus_CO21H2", 2);
            AddDeviceAttribute("GStatus_CO21H2Setup", 2);
            AddDeviceAttribute("GStatus_RS5", 2);
            AddDeviceAttribute("HidOverGattReg", "C:\\WINDOWS\\System32\\DriverStore\\FileRepository\\hidbthle.inf_amd64_0fc6b7cd4ccbc55c\\Microsoft.Bluetooth.Profiles.HidOverGatt.dll");
            AddDeviceAttribute("InstallationType", InstallType);
            AddDeviceAttribute("InstallDate", 1577722757);
            AddDeviceAttribute("InstallLanguage", "en-US");
            AddDeviceAttribute("IsAutopilotRegistered", 0);
            AddDeviceAttribute("IsCloudDomainJoined", 0);
            AddDeviceAttribute("IsContainerMgrInstalled", 1);
            AddDeviceAttribute("IsDeviceRetailDemo", 0);
            AddDeviceAttribute("IsEdgeWithChromiumInstalled", 1);
            AddDeviceAttribute("IsFlightingEnabled", FlightRing == "Retail" ? 0 : 1);
            AddDeviceAttribute("IsMDMEnrolled", 0);
            AddDeviceAttribute("IsRetailOS", FlightRing == "Retail");
            AddDeviceAttribute("MediaBranch", "");
            AddDeviceAttribute("MediaVersion", ReportingVersion);
            AddDeviceAttribute("MobileOperatorCommercialized", "000-88");
            AddDeviceAttribute("OEMModel", "RM-1085_1045");
            AddDeviceAttribute("OEMModelBaseBoard", "OEM Board Name");
            AddDeviceAttribute("OEMName_Uncleaned", "MICROSOFTMDG");
            //AddDeviceAttribute("OSArchitecture", MachineType.ToString().ToUpper());
            AddDeviceAttribute("OSArchitecture", "AMD64");
            AddDeviceAttribute("OSSkuId", (int)ReportingSku);
            AddDeviceAttribute("OSUILocale", "en-US");
            AddDeviceAttribute("OSVersion", ReportingVersion);
            AddDeviceAttribute("PhoneTargetingName", "Lumia 950 XL");
            AddDeviceAttribute("ProcessorIdentifier", "Intel64 Family 6 Model 151 Stepping 2");
            AddDeviceAttribute("ProcessorManufacturer", "GenuineIntel");
            AddDeviceAttribute("ProcessorModel", "12th Gen Intel(R) Core(TM) i9-12900K");
            AddDeviceAttribute("ReleaseType", ReleaseType);
            AddDeviceAttribute("SdbVer_19H1", 2000000000);
            AddDeviceAttribute("SdbVer_20H1", 2000000000);
            AddDeviceAttribute("SecureBootCapable", 1);
            AddDeviceAttribute("Steam", "URL:steam protocol");
            AddDeviceAttribute("TelemetryLevel", 3);
            AddDeviceAttribute("TencentReg", "79 d0 01 d7 9f 54 d5 01");
            AddDeviceAttribute("TencentType", 1);
            AddDeviceAttribute("TimestampEpochString_19H1", 1593425114);
            AddDeviceAttribute("TimestampEpochString_20H1", 1593425114);
            AddDeviceAttribute("TimestampEpochString_21H1", 1593425114);
            AddDeviceAttribute("TimestampEpochString_21H2", 1593425114);
            AddDeviceAttribute("TimestampEpochString_CO21H2", 1593425114);
            AddDeviceAttribute("TimestampEpochString_CO21H2Setup", 1593425114);
            AddDeviceAttribute("TPMVersion", 2);
            AddDeviceAttribute("UpdateManagementGroup", 2);
            AddDeviceAttribute("UpdateOfferedDays", 0);
            AddDeviceAttribute("UpgEx_19H1", "Green");
            AddDeviceAttribute("UpgEx_20H1", "Green");
            AddDeviceAttribute("UpgEx_21H1", "Green");
            AddDeviceAttribute("UpgEx_21H2", "Green");
            AddDeviceAttribute("UpgEx_CO21H2", "Green");
            AddDeviceAttribute("UpgEx_NI22H2", "Green");
            AddDeviceAttribute("UpgEx_RS5", "Green");
            AddDeviceAttribute("UpgradeEligible", 1);
            AddDeviceAttribute("Version_RS5", 2000000000);
            AddDeviceAttribute("WuClientVer", ReportingVersion);

            if (ReportingSku is OSSkuId.EnterpriseS or OSSkuId.EnterpriseSN || ReportingSku.ToString().Contains("Server", StringComparison.InvariantCultureIgnoreCase))
            {
                AddDeviceAttribute("BlockFeatureUpdates", 1);
            }

            if (ReportingSku == OSSkuId.Holographic)
            {
                AddDeviceAttribute("OneCoreFwV", ReportingVersion);
                AddDeviceAttribute("OneCoreSwV", ReportingVersion);
                AddDeviceAttribute("OneCoreManufacturerModelName", "Hololens");
                AddDeviceAttribute("OneCoreManufacturer", "Microsoft Corporation");
                AddDeviceAttribute("OneCoreOperatorName", "000-88");
            }
            else if (ReportingSku == OSSkuId.HubOS)
            {
                AddDeviceAttribute("OneCoreFwV", ReportingVersion);
                AddDeviceAttribute("OneCoreSwV", ReportingVersion);
                AddDeviceAttribute("OneCoreManufacturerModelName", "Surface Hub 2X");
                AddDeviceAttribute("OneCoreManufacturer", "Microsoft Corporation");
                AddDeviceAttribute("OneCoreOperatorName", "000-88");
            }
            else if (ReportingSku == OSSkuId.Andromeda)
            {
                AddDeviceAttribute("OneCoreFwV", ReportingVersion);
                AddDeviceAttribute("OneCoreSwV", ReportingVersion);
                AddDeviceAttribute("OneCoreManufacturerModelName", "Andromeda");
                AddDeviceAttribute("OneCoreManufacturer", "Microsoft Corporation");
                AddDeviceAttribute("OneCoreOperatorName", "000-88");
            }
            else if (ReportingSku == OSSkuId.Lite)
            {
                AddDeviceAttribute("Product", "ModernPC");

                AddDeviceAttribute("OneCoreFwV", ReportingVersion);
                AddDeviceAttribute("OneCoreSwV", ReportingVersion);
                AddDeviceAttribute("OneCoreManufacturerModelName", "Santorini");
                AddDeviceAttribute("OneCoreManufacturer", "Microsoft Corporation");
                AddDeviceAttribute("OneCoreOperatorName", "000-88");
            }

            DeviceAttributes = BuildDeviceAttributes(deviceAttributes);

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
