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
                opts.Edition,
                opts.OutputFolder).Wait();
        }

        private static async Task<CompDB> GenerateChangelog(UpdateData update, string changelogOutput, int i, CompDB pevCompDB)
        {
            File.AppendAllLines(changelogOutput, ["", $"## {update.Xml.LocalizedProperties.Title} - 200.0.{i}.0"]);

            HashSet<CompDB> compDBs = await update.GetCompDBsAsync();
            CompDB curCompDB = compDBs.First();

            List<string> added = [];
            List<string> updated = [];
            List<string> removed = [];
            List<string> modified = [];

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
                    string Edition,
                    string OutputFolder)
        {
            DriverPlan[] plans = System.Text.Json.JsonSerializer.Deserialize<DriverPlan[]>(File.ReadAllText("DriverConfig.json"));

            foreach (DriverPlan plan in plans)
            {
                Logging.Log(plan.outputFolder);
                await ProcessDriverPlan(plan, ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, ContentType, Mail, Password, Language, Edition, OutputFolder);
            }

            if (Debugger.IsAttached)
            {
                _ = Console.ReadLine();
            }

            Logging.Log("Completed.");
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
                    string Edition,
                    string RepoLocation)
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
            if (string.IsNullOrEmpty(ProductGUID))
            {
                ProductGUID = CTAC.GenerateDeviceId(DriverPlan.Manufacturer, DriverPlan.Family, DriverPlan.Product, DriverPlan.Sku);
            }

            NewestDriverProductCTAC.Products += $"PN={ProductGUID}_{MachineType}&V=0.0.0.0&Source=SMBIOS;";

            IEnumerable<UpdateData> NewestDriverProductUpdateData = await FE3Handler.GetUpdates(null, NewestDriverProductCTAC, token, FileExchangeV3UpdateFilter.ProductRelease);

            string outputFolder = (RepoLocation + DriverPlan.outputFolder).Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

            if (!NewestDriverProductUpdateData.Any())
            {
                Logging.Log("No updates found that matched the specified criteria.", Logging.LoggingLevel.Error);
                return;
            }

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

            string newestDriverOutput = $"{outputFolder}{Path.DirectorySeparatorChar}{newestDriverVersion}";
            if (Directory.Exists(newestDriverOutput) && Directory.EnumerateFiles(newestDriverOutput).Any())
            {
                return;
            }

            string changelogOutput = $"{outputFolder}{Path.DirectorySeparatorChar}CHANGELOG.md";

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
                    foreach (UpdateData update in PreciseDriverProductVersionUpdateData)
                    {
                        if (update.Xml.LocalizedProperties.Title.Contains("Windows"))
                        {
                            continue;
                        }

                        Logging.Log("Title: " + update.Xml.LocalizedProperties.Title);
                        Logging.Log("Description: " + update.Xml.LocalizedProperties.Description);

                        string fwOutput = $"{outputFolder}{Path.DirectorySeparatorChar}200.0.{driverBuildNumber}.0";
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
