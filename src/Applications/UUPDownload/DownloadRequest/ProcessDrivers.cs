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
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnifiedUpdatePlatform.Services.Composition.Database;
using UnifiedUpdatePlatform.Services.WindowsUpdate;
using UUPDownload.Downloading;

namespace UUPDownload.DownloadRequest
{
    public static class ProcessDrivers
    {
        private static readonly string RepoLocation = @"C:\Users\gus33\Documents\GitHub\WOA-Project\Reference\Qualcomm-Reference-Drivers";
        private static readonly string ConfigLocation = @"C:\Users\gus33\Documents\GitHub\UUPMediaCreator\DriverConfig.json";

        private static readonly DriverPlan[] plans =
        [
            new() // Snapdragon 8cx Gen 1 (Pre-release) Clamshell Reference Design
            {
                outputFolder = @"\1000_CLS",
                Manufacturer = "Qualcomm",
                Family = "SDM1000",
                Product = "CLS",
                Sku = "6",
                filteredIds = [],
                excludedIds = []
            },
            new() // Snapdragon 7c Gen 1/2 Clamshell Reference Design
            {
                outputFolder = @"\7180_CLS",
                Manufacturer = "Qualcomm", Family = "SC7180", Product = "CLS", Sku = "6",
                filteredIds = [],
                excludedIds = [1, 9, 10]
            },
            new() // Snapdragon 7c+ Gen 3 (Pre-release) Clamshell Reference Design
            {
                outputFolder = @"\7280_CLS",
                Manufacturer = "Qualcomm", Family = "SC_KODIAK", Product = "CLS", Sku = "6",
                filteredIds = [],
                excludedIds = []
            },
            new() // Snapdragon 7c+ Gen 3 Clamshell Reference Design
            {
                outputFolder = @"\7280_WINDOWS_CLS",
                Manufacturer = "Qualcomm", Family = "SC_KODIAK_WINDOWS", Product = "CLS", Sku = "6",
                filteredIds = [],
                excludedIds = []
            },
            new() // Snapdragon 8cx Gen 1/2 Clamshell Reference Design
            {
                outputFolder = @"\8180_CLS",
                Manufacturer = "Qualcomm", Family = "SC8180X", Product = "CLS", Sku = "6",
                filteredIds = [],
                excludedIds = []
            },
            new() // Snapdragon 8cx Gen 3 Clamshell Reference Design
            {
                outputFolder = @"\8280_QRD",
                Manufacturer = "Qualcomm", Family = "SCP_MAKENA", Product = "QRD", Sku = "6",
                filteredIds = [],
                excludedIds = []
            },
            new() // Snapdragon X Elite Clamshell Reference Design
            {
                outputFolder = @"\8380_CRD",
                Manufacturer = "Qualcomm", Family = "SCP_HAMOA", Product = "CRD", Sku = "6",
                filteredIds = [],
                excludedIds = []
            },
            new() // Snapdragon X Clamshell Reference Design
            {
                outputFolder = @"\8380PA_CRD",
                Manufacturer = "Qualcomm", Family = "SCP_PURWA", Product = "CRD", Sku = "6",
                filteredIds = [],
                excludedIds = []
            },
            new() // Surface Pro X with SQ1 processor
            {
                outputFolder = @"\Surface\8180_CAM",
                Manufacturer = "Microsoft Corporation", Family = "Surface", Product = "Surface Pro X", Sku = "Surface_Pro_X_1876",
                filteredIds = [],
                excludedIds = []
            },
            new() // Surface Pro X with SQ2 processor
            {
                outputFolder = @"\Surface\8180_CAR",
                Manufacturer = "Microsoft Corporation", Family = "Surface", Product = "Surface Pro X", Sku = "Surface_Pro_X_H_1876",
                filteredIds = [],
                excludedIds = []
            },
            new() // Surface Pro X (Wi-Fi)
            {
                outputFolder = @"\Surface\8180_CAS",
                Manufacturer = "Microsoft Corporation", Family = "Surface", Product = "Surface Pro X", Sku = "Surface_Pro_X_2010",
                filteredIds = [],
                excludedIds = []
            },
            new() // Surface Pro 9 with 5G (outside of U.S.)
            {
                outputFolder = @"\Surface\8280_ARC_1996",
                Manufacturer = "Microsoft Corporation", Family = "Surface", Product = "Surface Pro 9", Sku = "Surface_Pro_9_With_5G_1996",
                filteredIds = [],
                excludedIds = []
            },
            new() // Surface Pro 9 with 5G (U.S.)
            {
                outputFolder = @"\Surface\8280_ARC_1997",
                Manufacturer = "Microsoft Corporation", Family = "Surface", Product = "Surface Pro 9", Sku = "Surface_Pro_9_With_5G_1997",
                filteredIds = [],
                excludedIds = []
            },
            new() // Windows Dev Kit 2023
            {
                outputFolder = @"\Surface\8280_BLK",
                Manufacturer = "Microsoft Corporation", Family = "Surface", Product = "Windows Dev Kit 2023", Sku = "2043",
                filteredIds = [],
                excludedIds = []
            },
            new() // Surface Laptop 7 (13 inch)
            {
                outputFolder = @"\Surface\8380_ROM_2036",
                Manufacturer = "Microsoft Corporation", Family = "Surface", Product = "Microsoft Surface Laptop, 7th Edition", Sku = "Surface_Laptop_7th_Edition_2036",
                filteredIds = [],
                excludedIds = []
            },
            new() // Surface Laptop 7 (15 inch)
            {
                outputFolder = @"\Surface\8380_ROM_2037",
                Manufacturer = "Microsoft Corporation", Family = "Surface", Product = "Microsoft Surface Laptop, 7th Edition", Sku = "Surface_Laptop_7th_Edition_2037",
                filteredIds = [],
                excludedIds = []
            },
            new() // Surface Pro 11
            {
                outputFolder = @"\Surface\8380_DEN",
                Manufacturer = "Microsoft Corporation", Family = "Surface", Product = "Microsoft Surface Pro, 11th Edition", Sku = "Surface_Pro_11th_Edition_2076",
                filteredIds = [],
                excludedIds = []
            },
            /*new()
            {
                outputFolder = @"\Samsung\GalaxyBook2_VZW_Legacy",
                guid = "{4ddc74f1-1cba-50ac-96c4-baeaf09a117d}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBook2_VZW",
                Manufacturer = "SAMSUNG ELECTRONICS CO., LTD.", Family = "Galaxy Book Series", Product = "Galaxy Book2", Sku = "GALAXY A5A5-PAHD",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBook2_Open",
                Manufacturer = "SAMSUNG ELECTRONICS CO., LTD.", Family = "Galaxy Book Series", Product = "Galaxy Book2", Sku = "GALAXY A5A5-PHAG",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBook2_Unknown",
                Manufacturer = "SAMSUNG ELECTRONICS CO., LTD.", Family = "Galaxy Book Series", Product = "Galaxy Book2", Sku = "GALAXY A5A5-PAHG",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBook2_Unknown2",
                Manufacturer = "SAMSUNG ELECTRONICS CO., LTD.", Family = "Galaxy Book Series", Product = "Galaxy Book2", Sku = "GALAXY A5A5-PAHF",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\HP\EnvyX2",
                guid = "{79511a83-8e29-5b28-babd-ee57d65eeea2}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Asus\NovaGo",
                guid = "{e2c6ff8b-f787-5c14-a0cb-6d6723e870ec}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Lenovo\YogaC630",
                guid = "{43b71948-9c47-5372-a5cb-18db47bb873f}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },*/

            /*new()
            {
                outputFolder = @"\Samsung\GalaxyBook1",
                guid = "{466a496a-c462-554a-89d3-227ff4f51027}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBook2",
                guid = "{1555c469-8cd9-57dd-bcde-382e459133e3}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBook3",
                guid = "{b7771242-6905-5b4b-8d5a-b38393aef42d}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBook4",
                guid = "{0a5f0248-70a0-55b0-a173-8e366ec0ea26}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBook5",
                guid = "{66df7025-3411-57d4-a799-db22811cf9c9}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBook6",
                guid = "{c2c614b5-1308-595a-8b42-d8004540acd0}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBook7",
                guid = "{454e004b-b209-5153-a438-388009a9e1ef}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBook8",
                guid = "{cfd56d38-8276-5b4d-9e18-611eb33c2ae2}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBook9",
                guid = "{c565fa23-7336-5fa1-bb05-cf1d85873f7e}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBookA",
                guid = "{2f1af45d-3918-5580-a25f-adc0e774751f}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBookB",
                guid = "{1a445de1-9ea7-58ee-ba9f-89679de2e6cd}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBookC",
                guid = "{f139ecb6-912d-56a7-b140-9ba326833643}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBookD",
                guid = "{e54f3804-b7ef-5676-bfa1-1d127d594316}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBookE",
                guid = "{03ec6d4b-c75d-5d99-b541-96f3b0bcb5f8}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Samsung\GalaxyBookF",
                guid = "{ca0158e4-038b-5a84-8037-1b3cbf948d8a}",
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new()
            {
                outputFolder = @"\Testing",
                Manufacturer = "Acer", Family = "Aspire 1", Product = "Aspire A114-61", Sku = "0000000000000000",
                filteredIds = [],
                excludedIds = []
            },*/
        ];

        internal static void ParseDownloadOptions(DownloadRequestOptions opts)
        {
            CheckAndDownloadUpdates(
                opts.ReportingSku,
                opts.ReportingVersion,
                opts.MachineType,
                opts.FlightRing,
                opts.FlightingBranchName,
                opts.BranchReadinessLevel,
                opts.CurrentBranch,
                opts.ReleaseType,
                opts.SyncCurrentVersionOnly,
                opts.ContentType,
                opts.Mail,
                opts.Password,
                opts.Language,
                opts.Edition).Wait();
        }

        private static async Task<CompDB> GenerateChangelog(UpdateData update, string changelogOutput, int i, CompDB pevCompDB)
        {
            File.AppendAllLines(changelogOutput, ["", $"## {update.Xml.LocalizedProperties.Title} - 200.0.{i}.0"]);

            HashSet<CompDB> compDBs = await update.GetCompDBsAsync();
            CompDB curCompDB = compDBs.First();

            List<string> added = new();
            List<string> updated = new();
            List<string> removed = new();
            List<string> modified = new();

            if (pevCompDB != null)
            {
                IEnumerable<string> prevPackageList = pevCompDB.Packages.Package.Select(pkg => $"| {pkg.Version} | {pkg.ID.Split("-")[1].Replace(".inf", ".cab", StringComparison.InvariantCultureIgnoreCase)} |");
                IEnumerable<string> curPackageList = curCompDB.Packages.Package.Select(pkg => $"| {pkg.Version} | {pkg.ID.Split("-")[1].Replace(".inf", ".cab", StringComparison.InvariantCultureIgnoreCase)} |");

                foreach (string pkg in prevPackageList)
                {
                    string version = pkg.Split("|")[1];
                    string id = pkg.Split("|")[2];

                    bool existsInNewer = curPackageList.Any(x => id.Equals(x.Split("|")[2], StringComparison.InvariantCultureIgnoreCase));

                    if (!existsInNewer)
                    {
                        removed.Add(pkg);
                    }
                }

                foreach (Package package in curCompDB.Packages.Package)
                {
                    string pkg = $"| {package.Version} | {package.ID.Split("-")[1].Replace(".inf", ".cab", StringComparison.InvariantCultureIgnoreCase)} |";

                    string version = pkg.Split("|")[1];
                    string id = pkg.Split("|")[2];

                    bool existsInOlder = prevPackageList.Any(x => id.Equals(x.Split("|")[2], StringComparison.InvariantCultureIgnoreCase));

                    if (existsInOlder)
                    {
                        bool hasSameVersion = prevPackageList.Any(x => version.Equals(x.Split("|")[1], StringComparison.InvariantCultureIgnoreCase) && id.Equals(x.Split("|")[2], StringComparison.InvariantCultureIgnoreCase));

                        if (!hasSameVersion)
                        {
                            updated.Add(pkg);
                        }
                        else
                        {
                            bool hasSameHash = pevCompDB.Packages.Package.Any(x => x.Payload.PayloadItem[0].PayloadHash == package.Payload.PayloadItem[0].PayloadHash);
                            if (!hasSameHash)
                            {
                                modified.Add(pkg);
                            }
                        }
                    }
                    else
                    {
                        added.Add(pkg);
                    }
                }
            }
            else
            {
                foreach (Package pkg in curCompDB.Packages.Package)
                {
                    added.Add($"| {pkg.Version} | {pkg.ID.Split("-")[1].Replace(".inf", ".cab", StringComparison.CurrentCultureIgnoreCase)} |");
                }
            }

            added.Sort();
            updated.Sort();
            modified.Sort();
            removed.Sort();

            if (added.Count > 0)
            {
                File.AppendAllLines(changelogOutput, ["", $"### Added", ""]);
                File.AppendAllLines(changelogOutput, ["| Driver version | Package |"]);
                File.AppendAllLines(changelogOutput, ["|----------------|---------|"]);
                File.AppendAllLines(changelogOutput, added);
            }


            if (updated.Count > 0)
            {
                File.AppendAllLines(changelogOutput, ["", $"### Updated", ""]);
                File.AppendAllLines(changelogOutput, ["| Driver version | Package |"]);
                File.AppendAllLines(changelogOutput, ["|----------------|---------|"]);
                File.AppendAllLines(changelogOutput, updated);
            }


            if (modified.Count > 0)
            {
                File.AppendAllLines(changelogOutput, ["", $"### Modified", ""]);
                File.AppendAllLines(changelogOutput, ["| Driver version | Package |"]);
                File.AppendAllLines(changelogOutput, ["|----------------|---------|"]);
                File.AppendAllLines(changelogOutput, modified);
            }


            if (removed.Count > 0)
            {
                File.AppendAllLines(changelogOutput, ["", $"### Removed", ""]);
                File.AppendAllLines(changelogOutput, ["| Driver version | Package |"]);
                File.AppendAllLines(changelogOutput, ["|----------------|---------|"]);
                File.AppendAllLines(changelogOutput, removed);
            }

            return curCompDB;
        }

        private static async Task CheckAndDownloadUpdates(OSSkuId ReportingSku,
                    string ReportingVersion,
                    MachineType MachineType,
                    string FlightRing,
                    string FlightingBranchName,
                    string BranchReadinessLevel,
                    string CurrentBranch,
                    string ReleaseType,
                    bool SyncCurrentVersionOnly,
                    string ContentType,
                    string Mail,
                    string Password,
                    string Language,
                    string Edition)
        {
            System.Text.Json.JsonSerializerOptions jsonSerializerOptions = new();
            jsonSerializerOptions.WriteIndented = true;
            var json = System.Text.Json.JsonSerializer.Serialize(plans, jsonSerializerOptions);
            File.WriteAllText(ConfigLocation, json);
            return;

            /*DriverPlan[] plans = System.Text.Json.JsonSerializer.Deserialize<DriverPlan[]>(File.ReadAllText(ConfigLocation));

            foreach (DriverPlan plan in plans)
            {
                Logging.Log(plan.outputFolder);
                await ProcessDriverPlan(plan, ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, ContentType, Mail, Password, Language, Edition);
            }

            if (Debugger.IsAttached)
            {
                _ = Console.ReadLine();
            }

            Logging.Log("Completed.");*/
        }

        private static async Task ProcessDriverPlan(DriverPlan DriverPlan,
                    OSSkuId ReportingSku,
                    string ReportingVersion,
                    MachineType MachineType,
                    string FlightRing,
                    string FlightingBranchName,
                    string BranchReadinessLevel,
                    string CurrentBranch,
                    string ReleaseType,
                    bool SyncCurrentVersionOnly,
                    string ContentType,
                    string Mail,
                    string Password,
                    string Language,
                    string Edition)
        {
            CompDB previousCompositionDatabase = null;

            Logging.Log("Checking for updates...");

            CTAC NewestDriverProductCTAC = new(ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, ContentType: ContentType);
            string token = string.Empty;
            if (!string.IsNullOrEmpty(Mail) && !string.IsNullOrEmpty(Password))
            {
                token = await MBIHelper.GenerateMicrosoftAccountTokenAsync(Mail, Password);
            }

            string ProductGUID = DriverPlan.guid;
            if (!string.IsNullOrEmpty(ProductGUID))
            {
                ProductGUID = CTAC.GenerateDeviceId(DriverPlan.Manufacturer, DriverPlan.Family, DriverPlan.Product, DriverPlan.Sku);
            }

            NewestDriverProductCTAC.Products += $"PN={ProductGUID}_{MachineType}&V=0.0.0.0&Source=SMBIOS;";

            IEnumerable<UpdateData> NewestDriverProductUpdateData = await FE3Handler.GetUpdates(null, NewestDriverProductCTAC, token, FileExchangeV3UpdateFilter.ProductRelease);

            string outputFolder = RepoLocation + DriverPlan.outputFolder;

            if (!NewestDriverProductUpdateData.Any())
            {
                Logging.Log("No updates found that matched the specified criteria.", Logging.LoggingLevel.Error);
                return;
            }

            Logging.Log($"Found {NewestDriverProductUpdateData.Count()} update(s):");

            string newestDriverVersion = "0.0.0.0";

            for (int i = 0; i < NewestDriverProductUpdateData.Count(); i++)
            {
                UpdateData update = NewestDriverProductUpdateData.ElementAt(i);

                if (update.Xml.LocalizedProperties.Title.Contains("Windows"))
                {
                    continue;
                }

                Logging.Log($"{i}: Title: {update.Xml.LocalizedProperties.Title}");
                Logging.Log($"{i}: Description: {update.Xml.LocalizedProperties.Description}");

                Logging.Log("Gathering update metadata...");

                HashSet<CompDB> compDBs = await update.GetCompDBsAsync();

                newestDriverVersion = compDBs.First().UUPProductVersion;
            }

            string newestDriverOutput = $"{outputFolder}\\{newestDriverVersion}";
            if (Directory.Exists(newestDriverOutput) && Directory.EnumerateFiles(newestDriverOutput).Any())
            {
                return;
            }

            string changelogOutput = $"{outputFolder}\\CHANGELOG.md";

            if (File.Exists(changelogOutput))
            {
                File.Delete(changelogOutput);
            }

            int newestDriverBuildNumber = int.Parse(newestDriverVersion.Split(".")[2]);

            for (int driverBuildNumber = 0; driverBuildNumber <= newestDriverBuildNumber; driverBuildNumber++)
            {
                if (DriverPlan.filteredIds.Length != 0 && !DriverPlan.filteredIds.Contains(driverBuildNumber))
                {
                    continue;
                }

                if (DriverPlan.excludedIds.Length != 0 && DriverPlan.excludedIds.Contains(driverBuildNumber))
                {
                    continue;
                }

                CTAC PreciseDriverProductVersionCTAC = new(ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, ContentType: ContentType);

                PreciseDriverProductVersionCTAC.Products += $"PN={ProductGUID}_{MachineType}&V=200.0.{driverBuildNumber}.0&Source=SMBIOS;";
                PreciseDriverProductVersionCTAC.SyncCurrentVersionOnly = true;

                Logging.Log($"Checking for updates... 200.0.{driverBuildNumber}.0 / {newestDriverVersion}");
                IEnumerable<UpdateData> PreciseDriverProductVersionUpdateData = await FE3Handler.GetUpdates(null, PreciseDriverProductVersionCTAC, token, FileExchangeV3UpdateFilter.ProductRelease);

                if (!PreciseDriverProductVersionUpdateData.Any())
                {
                    Logging.Log("No updates found that matched the specified criteria.", Logging.LoggingLevel.Error);
                }
                else
                {
                    Logging.Log($"Found {PreciseDriverProductVersionUpdateData.Count()} update(s):");

                    for (int UpdateIndex = 0; UpdateIndex < PreciseDriverProductVersionUpdateData.Count(); UpdateIndex++)
                    {
                        UpdateData update = PreciseDriverProductVersionUpdateData.ElementAt(UpdateIndex);

                        if (update.Xml.LocalizedProperties.Title.Contains("Windows"))
                        {
                            continue;
                        }

                        Logging.Log($"{UpdateIndex}: Title: {update.Xml.LocalizedProperties.Title}");
                        Logging.Log($"{UpdateIndex}: Description: {update.Xml.LocalizedProperties.Description}");
                    }

                    foreach (UpdateData update in PreciseDriverProductVersionUpdateData)
                    {
                        if (update.Xml.LocalizedProperties.Title.Contains("Windows"))
                        {
                            continue;
                        }

                        Logging.Log("Title: " + update.Xml.LocalizedProperties.Title);
                        Logging.Log("Description: " + update.Xml.LocalizedProperties.Description);

                        string fwOutput = $"{outputFolder}\\200.0.{driverBuildNumber}.0";
                        if (!Directory.Exists(fwOutput) || !Directory.EnumerateFiles(fwOutput).Any())
                        {
                            _ = await UnifiedUpdatePlatform.Services.WindowsUpdate.Downloads.UpdateUtils.ProcessUpdateAsync(update, fwOutput, MachineType, new ReportProgress(), Language, Edition, false, false);
                        }

                        previousCompositionDatabase = await GenerateChangelog(update, changelogOutput, driverBuildNumber, previousCompositionDatabase);
                    }
                }
            }
        }
    }
}
