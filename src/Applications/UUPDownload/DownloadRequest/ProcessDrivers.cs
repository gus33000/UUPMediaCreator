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
using System.Diagnostics;
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
            File.AppendAllLines(changelogOutput, new string[] { "", $"## {update.Xml.LocalizedProperties.Title} - 200.0.{i}.0" });

            HashSet<CompDB> compDBs = await update.GetCompDBsAsync();
            var curCompDB = compDBs.First();

            List<string> added = new();
            List<string> updated = new();
            List<string> removed = new();
            List<string> modified = new();

            if (pevCompDB != null)
            {
                var prevPackageList = pevCompDB.Packages.Package.Select(pkg => $"| {pkg.Version} | {pkg.ID.Split("-")[1].Replace(".inf", ".cab", StringComparison.InvariantCultureIgnoreCase)} |");
                var curPackageList = curCompDB.Packages.Package.Select(pkg => $"| {pkg.Version} | {pkg.ID.Split("-")[1].Replace(".inf", ".cab", StringComparison.InvariantCultureIgnoreCase)} |");

                foreach (var pkg in prevPackageList)
                {
                    string version = pkg.Split("|")[1];
                    string id = pkg.Split("|")[2];

                    bool existsInNewer = curPackageList.Any(x => id.Equals(x.Split("|")[2], StringComparison.InvariantCultureIgnoreCase));

                    if (!existsInNewer)
                    {
                        removed.Add(pkg);
                    }
                }

                foreach (var package in curCompDB.Packages.Package)
                {
                    var pkg = $"| {package.Version} | {package.ID.Split("-")[1].Replace(".inf", ".cab", StringComparison.InvariantCultureIgnoreCase)} |";

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
                foreach (var pkg in curCompDB.Packages.Package)
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
                File.AppendAllLines(changelogOutput, new string[] { "", $"### Added", "" });
                File.AppendAllLines(changelogOutput, new string[] { "| Driver version | Package |" });
                File.AppendAllLines(changelogOutput, new string[] { "|----------------|---------|" });
                File.AppendAllLines(changelogOutput, added);
            }


            if (updated.Count > 0)
            {
                File.AppendAllLines(changelogOutput, new string[] { "", $"### Updated", "" });
                File.AppendAllLines(changelogOutput, new string[] { "| Driver version | Package |" });
                File.AppendAllLines(changelogOutput, new string[] { "|----------------|---------|" });
                File.AppendAllLines(changelogOutput, updated);
            }


            if (modified.Count > 0)
            {
                File.AppendAllLines(changelogOutput, new string[] { "", $"### Modified", "" });
                File.AppendAllLines(changelogOutput, new string[] { "| Driver version | Package |" });
                File.AppendAllLines(changelogOutput, new string[] { "|----------------|---------|" });
                File.AppendAllLines(changelogOutput, modified);
            }


            if (removed.Count > 0)
            {
                File.AppendAllLines(changelogOutput, new string[] { "", $"### Removed", "" });
                File.AppendAllLines(changelogOutput, new string[] { "| Driver version | Package |" });
                File.AppendAllLines(changelogOutput, new string[] { "|----------------|---------|" });
                File.AppendAllLines(changelogOutput, removed);
            }

            return curCompDB;
        }

        public struct DriverPlan
        {
            public string outputFolder;
            public string guid;
            public int[] filteredIds;
            public int[] excludedIds;
        }

        private static readonly string RepoLocation = @"D:\a\ID1\Duo\repos\Qualcomm-Reference-Drivers";

        private static readonly DriverPlan[] plans = new DriverPlan[]
        {
            new() // Snapdragon 8cx Gen 1 (Pre-release) Clamshell Reference Design
            {
                outputFolder = RepoLocation + @"\1000_CLS",
                guid = CTAC.GenerateDeviceId("Qualcomm", "SDM1000", "CLS", "6").ToString(),
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new() // Snapdragon 7c Gen 1/2 Clamshell Reference Design
            {
                outputFolder = RepoLocation + @"\7180_CLS",
                guid = CTAC.GenerateDeviceId("Qualcomm", "SC7180", "CLS", "6").ToString(),
                filteredIds = Array.Empty<int>(),
                excludedIds = new int[3] {1, 9, 10}
            },
            new() // Snapdragon 7c+ Gen 3 (Pre-release) Clamshell Reference Design
            {
                outputFolder = RepoLocation + @"\7280_CLS",
                guid = CTAC.GenerateDeviceId("Qualcomm", "SC_KODIAK", "CLS", "6").ToString(),
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new() // Snapdragon 7c+ Gen 3 Clamshell Reference Design
            {
                outputFolder = RepoLocation + @"\7280_WINDOWS_CLS",
                guid = CTAC.GenerateDeviceId("Qualcomm", "SC_KODIAK_WINDOWS", "CLS", "6").ToString(),
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new() // Snapdragon 8cx Gen 1/2 Clamshell Reference Design
            {
                outputFolder = RepoLocation + @"\8180_CLS",
                guid = CTAC.GenerateDeviceId("Qualcomm", "SC8180X", "CLS", "6").ToString(),
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new() // Snapdragon 8cx Gen 3 Clamshell Reference Design
            {
                outputFolder = RepoLocation + @"\8280_QRD",
                guid = CTAC.GenerateDeviceId("Qualcomm", "SCP_MAKENA", "QRD", "6").ToString(),
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new() // Snapdragon X Elite Clamshell Reference Design
            {
                outputFolder = RepoLocation + @"\8380_CRD",
                guid = CTAC.GenerateDeviceId("Qualcomm", "SCP_HAMOA", "CRD", "6").ToString(),
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new() // Surface Pro X with SQ1 processor
            {
                outputFolder = RepoLocation + @"\Surface\8180_CAM",
                guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Pro X", "Surface_Pro_X_1876"),
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new() // Surface Pro X with SQ2 processor
            {
                outputFolder = RepoLocation + @"\Surface\8180_CAR",
                guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Pro X", "Surface_Pro_X_H_1876"),
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new() // Surface Pro X (Wi-Fi)
            {
                outputFolder = RepoLocation + @"\Surface\8180_CAS",
                guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Pro X", "Surface_Pro_X_2010"),
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new() // Surface Pro 9 with 5G (outside of U.S.)
            {
                outputFolder = RepoLocation + @"\Surface\8280_ARC_1996",
                guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Pro 9", "Surface_Pro_9_With_5G_1996"),
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new() // Surface Pro 9 with 5G (U.S.)
            {
                outputFolder = RepoLocation + @"\Surface\8280_ARC_1997",
                guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Pro 9", "Surface_Pro_9_With_5G_1997"),
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
            new() // Windows Dev Kit 2023
            {
                outputFolder = RepoLocation + @"\Surface\8280_BLK",
                guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Windows Dev Kit 2023", "2043"),
                filteredIds = Array.Empty<int>(),
                excludedIds = Array.Empty<int>()
            },
        };

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
            foreach (var plan in plans)
            {
                await ProcessDriverPlan(plan, ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, ContentType, Mail, Password, Language, Edition);
            }

            if (Debugger.IsAttached)
            {
                _ = Console.ReadLine();
            }

            Logging.Log("Completed.");
        }

        private static async Task ProcessDriverPlan(DriverPlan plan,
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
            //string guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Pro 9", "Surface_Pro_9_With_5G_1996");
            //string guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Pro 9", "Surface_Pro_9_With_5G_1997");
            //string guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Pro X", "Surface_Pro_X_1876");
            //string guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Pro X", "Surface_Pro_X_H_1876");
            //string guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Pro X", "Surface_Pro_X_2010");
            //string guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Studio", "Surface_Studio");
            //string guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Studio 2", "Surface_Studio_2_1707_Commercial");
            //string guid = CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Surface Studio 2+", "Surface_Studio_2+_2028");
            //string guid = "{813677fa-6d11-5756-a44d-dde0f552d3f6}";//CTAC.GenerateDeviceId("Microsoft Corporation", "Surface", "Windows Dev Kit 2023", "2023");
            //ctac.Products += $"PN={CTAC.GenerateDeviceId("Qualcomm", "SDM850", "MTP", "6")}_{MachineType}&V=0.0.0.0&Source=SMBIOS;";
            //ctac.Products += $"PN={CTAC.GenerateDeviceId("Qualcomm", "SC8180X", "CLS", "6")}_{MachineType}&V=0.0.0.0&Source=SMBIOS;";
            //ctac.Products += $"PN={CTAC.GenerateDeviceId("Qualcomm", "SC7180", "CLS", "6")}_{MachineType}&V=0.0.0.0&Source=SMBIOS;";
            //ctac.Products += $"PN={CTAC.GenerateDeviceId("Qualcomm", "SCP_MAKENA", "QRD", "6")}_{MachineType}&V=0.0.0.0&Source=SMBIOS;";
            //ctac.Products += $"PN={CTAC.GenerateDeviceId("Qualcomm", "SC_KODIAK", "CLS", "6")}_{MachineType}&V=0.0.0.0&Source=SMBIOS;";
            //ctac.Products += $"PN={CTAC.GenerateDeviceId("Qualcomm", "SC_KODIAK_WINDOWS", "CLS", "6")}_{MachineType}&V=0.0.0.0&Source=SMBIOS;";
            //string guid = CTAC.GenerateDeviceId("Qualcomm", "SC_KODIAK_WINDOWS", "CLS", "6");

            CompDB pevCompDB = null;

            Logging.Log("Checking for updates...");

            CTAC ctac = new(ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, ContentType: ContentType);
            string token = string.Empty;
            if (!string.IsNullOrEmpty(Mail) && !string.IsNullOrEmpty(Password))
            {
                token = await MBIHelper.GenerateMicrosoftAccountTokenAsync(Mail, Password);
            }

            string guid = plan.guid;
            ctac.Products += $"PN={guid}_{MachineType}&V=0.0.0.0&Source=SMBIOS;";

            IEnumerable<UpdateData> data = await FE3Handler.GetUpdates(null, ctac, token, FileExchangeV3UpdateFilter.ProductRelease);
            //data = data.Select(x => UpdateUtils.TrimDeltasFromUpdateData(x));

            string outputFolder = plan.outputFolder;

            string maxVersion = "0.0.0.0";

            if (!data.Any())
            {
                Logging.Log("No updates found that matched the specified criteria.", Logging.LoggingLevel.Error);
            }
            else
            {
                Logging.Log($"Found {data.Count()} update(s):");

                for (int i = 0; i < data.Count(); i++)
                {
                    UpdateData update = data.ElementAt(i);

                    if (update.Xml.LocalizedProperties.Title.Contains("Windows"))
                    {
                        continue;
                    }

                    Logging.Log($"{i}: Title: {update.Xml.LocalizedProperties.Title}");
                    Logging.Log($"{i}: Description: {update.Xml.LocalizedProperties.Description}");

                    Logging.Log("Gathering update metadata...");

                    HashSet<CompDB> compDBs = await update.GetCompDBsAsync();

                    maxVersion = compDBs.First().UUPProductVersion;
                }
            }

            string maxfWOutput = $"{outputFolder}\\{maxVersion}";
            if (Directory.Exists(maxfWOutput) && Directory.EnumerateFiles(maxfWOutput).Any())
            {
                return;
            }

            string changelogOutput = $"{outputFolder}\\CHANGELOG.md";

            if (File.Exists(changelogOutput))
            {
                File.Delete(changelogOutput);
            }

            //200.0.19.0
            int maxVersionint = int.Parse(maxVersion.Split(".")[2]);

            for (int i = 0; i <= maxVersionint; i++)
            {
                if (plan.filteredIds.Length != 0 && !plan.filteredIds.Contains(i))
                {
                    continue;
                }

                if (plan.excludedIds.Length != 0 && plan.excludedIds.Contains(i))
                {
                    continue;
                }

                CTAC ctac2 = new(ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, ContentType: ContentType);

                ctac2.Products += $"PN={guid}_{MachineType}&V=200.0.{i}.0&Source=SMBIOS;";
                ctac2.SyncCurrentVersionOnly = true;

                Logging.Log($"Checking for updates... 200.0.{i}.0 / {maxVersion}");
                IEnumerable<UpdateData> data2 = await FE3Handler.GetUpdates(null, ctac2, token, FileExchangeV3UpdateFilter.ProductRelease);

                if (!data2.Any())
                {
                    Logging.Log("No updates found that matched the specified criteria.", Logging.LoggingLevel.Error);
                }
                else
                {
                    Logging.Log($"Found {data2.Count()} update(s):");

                    for (int j = 0; j < data2.Count(); j++)
                    {
                        UpdateData update = data2.ElementAt(j);

                        if (update.Xml.LocalizedProperties.Title.Contains("Windows"))
                        {
                            continue;
                        }

                        Logging.Log($"{j}: Title: {update.Xml.LocalizedProperties.Title}");
                        Logging.Log($"{j}: Description: {update.Xml.LocalizedProperties.Description}");
                    }

                    foreach (UpdateData update in data2)
                    {
                        if (update.Xml.LocalizedProperties.Title.Contains("Windows"))
                        {
                            continue;
                        }

                        Logging.Log("Title: " + update.Xml.LocalizedProperties.Title);
                        Logging.Log("Description: " + update.Xml.LocalizedProperties.Description);

                        string fwOutput = $"{outputFolder}\\200.0.{i}.0";
                        if (!Directory.Exists(fwOutput) || !Directory.EnumerateFiles(fwOutput).Any())
                        {
                            await ProcessUpdateAsync(update, fwOutput, MachineType, Language, Edition, true);
                        }

                        pevCompDB = await GenerateChangelog(update, changelogOutput, i, pevCompDB);
                    }
                }
            }
        }

        private static async Task ProcessUpdateAsync(UpdateData update, string pOutputFolder, MachineType MachineType, string Language = "", string Edition = "", bool WriteMetadata = true)
        {
            string buildstr = "";
            IEnumerable<string> languages = null;

            Logging.Log("Gathering update metadata...");

            HashSet<CompDB> compDBs = await update.GetCompDBsAsync();

            await Task.WhenAll(
                Task.Run(async () => buildstr = await update.GetBuildStringAsync()),
                Task.Run(async () => languages = await update.GetAvailableLanguagesAsync()));

            buildstr ??= "";

            //
            // Windows Phone Build Lab says hi
            //
            // Quirk with Nickel+ Windows NT builds where specific binaries
            // exempted from neutral build info gets the wrong build tags
            //
            if (buildstr.Contains("GitEnlistment(winpbld)"))
            {
                // We need to fallback to CompDB (less accurate but we have no choice, due to CUs etc...

                // Loop through all CompDBs to find the highest version reported
                CompDB selectedCompDB = null;
                Version currentHighest = null;
                foreach (CompDB compDB in compDBs)
                {
                    if (compDB.TargetOSVersion != null)
                    {
                        if (Version.TryParse(compDB.TargetOSVersion, out Version currentVer))
                        {
                            if (currentHighest == null || (currentVer != null && currentVer.GreaterThan(currentHighest)))
                            {
                                if (!string.IsNullOrEmpty(compDB.TargetBuildInfo) && !string.IsNullOrEmpty(compDB.TargetOSVersion))
                                {
                                    currentHighest = currentVer;
                                    selectedCompDB = compDB;
                                }
                            }
                        }
                    }
                }

                // We found a suitable CompDB is it is not null
                if (selectedCompDB != null)
                {
                    // Example format:
                    // TargetBuildInfo="rs_prerelease_flt.22509.1011.211120-1700"
                    // TargetOSVersion="10.0.22509.1011"

                    buildstr = $"{selectedCompDB.TargetOSVersion} ({selectedCompDB.TargetBuildInfo.Split(".")[0]}.{selectedCompDB.TargetBuildInfo.Split(".")[3]})";
                }
            }

            if (string.IsNullOrEmpty(buildstr) && update.Xml.LocalizedProperties.Title.Contains("(UUP-CTv2)"))
            {
                string unformattedBase = update.Xml.LocalizedProperties.Title.Split(" ")[0];
                buildstr = $"10.0.{unformattedBase.Split(".")[0]}.{unformattedBase.Split(".")[1]} ({unformattedBase.Split(".")[2]}.{unformattedBase.Split(".")[3]})";
            }
            else if (string.IsNullOrEmpty(buildstr))
            {
                buildstr = update.Xml.LocalizedProperties.Title;
            }

            Logging.Log("Build String: " + buildstr);
            Logging.Log("Languages: " + string.Join(", ", languages));

            /*Logging.Log("Parsing CompDBs...");

            if (compDBs != null)
            {
                CompDBXmlClass.Package editionPackPkg = compDBs.GetEditionPackFromCompDBs();
                if (editionPackPkg != null)
                {
                    string editionPkg = await update.DownloadFileFromDigestAsync(editionPackPkg.Payload.PayloadItem.First(x => !x.Path.EndsWith(".psf")).PayloadHash);
                    BuildTargets.EditionPlanningWithLanguage[] plans = await Task.WhenAll(languages.Select(x => update.GetTargetedPlanAsync(x, editionPkg)));

                    foreach (BuildTargets.EditionPlanningWithLanguage plan in plans)
                    {
                        Logging.Log("");
                        Logging.Log("Editions available for language: " + plan.LanguageCode);
                        plan.EditionTargets.PrintAvailablePlan();
                    }
                }
            }*/

            _ = await UnifiedUpdatePlatform.Services.WindowsUpdate.Downloads.UpdateUtils.ProcessUpdateAsync(update, pOutputFolder, MachineType, new ReportProgress(), Language, Edition, WriteMetadata, false);
        }
    }
}
