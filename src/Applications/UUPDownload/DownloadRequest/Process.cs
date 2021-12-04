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
using CompDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using UUPDownload.Downloading;
using WindowsUpdateLib;
using WindowsUpdateLib.Shared;

namespace UUPDownload.DownloadRequest
{
    public static class Process
    {
        internal static int ParseOptions(DownloadRequestOptions opts)
        {
            try
            {
                PerformOperation(opts).Wait();
            }
            catch (Exception ex)
            {
                Logging.Log("Something happened.", Logging.LoggingLevel.Error);
                while (ex != null)
                {
                    Logging.Log(ex.Message, Logging.LoggingLevel.Error);
                    Logging.Log(ex.StackTrace, Logging.LoggingLevel.Error);
                    ex = ex.InnerException;
                }
                if (Debugger.IsAttached)
                {
                    Console.ReadLine();
                }

                return 1;
            }

            return 0;
        }

        internal static int ParseReplayOptions(DownloadReplayOptions opts)
        {
            try
            {
                UpdateData update = JsonSerializer.Deserialize<UpdateData>(File.ReadAllText(opts.ReplayMetadata));

                Logging.Log("Title: " + update.Xml.LocalizedProperties.Title);
                Logging.Log("Description: " + update.Xml.LocalizedProperties.Description);

                ProcessUpdateAsync(update, opts.OutputFolder, opts.MachineType, opts.Language, opts.Edition, true).Wait();
            }
            catch (Exception ex)
            {
                Logging.Log("Something happened.", Logging.LoggingLevel.Error);
                while (ex != null)
                {
                    Logging.Log(ex.Message, Logging.LoggingLevel.Error);
                    Logging.Log(ex.StackTrace, Logging.LoggingLevel.Error);
                    ex = ex.InnerException;
                }
                if (Debugger.IsAttached)
                {
                    Console.ReadLine();
                }

                return 1;
            }

            return 0;
        }

        private static async Task PerformOperation(DownloadRequestOptions o)
        {
            Logging.Log("Checking for updates...");

            CTAC ctac = new(o.ReportingSku, o.ReportingVersion, o.MachineType, o.FlightRing, o.FlightingBranchName, o.BranchReadinessLevel, o.CurrentBranch, o.ReleaseType, o.SyncCurrentVersionOnly, ContentType: o.ContentType);
            string token = string.Empty;
            if (!string.IsNullOrEmpty(o.Mail) && !string.IsNullOrEmpty(o.Password))
            {
                token = await MBIHelper.GenerateMicrosoftAccountTokenAsync(o.Mail, o.Password).ConfigureAwait(false);
            }

            IEnumerable<UpdateData> data = await FE3Handler.GetUpdates(null, ctac, token, FileExchangeV3UpdateFilter.ProductRelease).ConfigureAwait(false);
            //data = data.Select(x => UpdateUtils.TrimDeltasFromUpdateData(x));

            if (!data.Any())
            {
                Logging.Log("No updates found that matched the specified criteria.", Logging.LoggingLevel.Error);
            }
            else
            {
                foreach (UpdateData update in data)
                {
                    Logging.Log("Title: " + update.Xml.LocalizedProperties.Title);
                    Logging.Log("Description: " + update.Xml.LocalizedProperties.Description);

                    await ProcessUpdateAsync(update, o.OutputFolder, o.MachineType, o.Language, o.Edition, true).ConfigureAwait(false);
                }
            }
            Logging.Log("Completed.");
            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }

        private static async Task ProcessUpdateAsync(UpdateData update, string pOutputFolder, MachineType MachineType, string Language = "", string Edition = "", bool WriteMetadata = true)
        {
            string buildstr = "";
            IEnumerable<string> languages = null;

            Logging.Log("Gathering update metadata...");

            HashSet<CompDBXmlClass.CompDB> compDBs = await update.GetCompDBsAsync().ConfigureAwait(false);

            await Task.WhenAll(
                Task.Run(async () => buildstr = await update.GetBuildStringAsync().ConfigureAwait(false)),
                Task.Run(async () => languages = await update.GetAvailableLanguagesAsync().ConfigureAwait(false))).ConfigureAwait(false);

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
                CompDBXmlClass.CompDB selectedCompDB = null;
                Version currentHighest = null;
                foreach (CompDBXmlClass.CompDB compDB in compDBs)
                {
                    if (compDB.TargetOSVersion != null)
                    {
                        Version currentVer = null;
                        if (Version.TryParse(compDB.TargetOSVersion, out currentVer))
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

            Logging.Log("Parsing CompDBs...");

            if (compDBs != null)
            {
                CompDBXmlClass.Package editionPackPkg = compDBs.GetEditionPackFromCompDBs();
                if (editionPackPkg != null)
                {
                    string editionPkg = await update.DownloadFileFromDigestAsync(editionPackPkg.Payload.PayloadItem.First(x => !x.Path.EndsWith(".psf")).PayloadHash).ConfigureAwait(false);
                    BuildTargets.EditionPlanningWithLanguage[] plans = await Task.WhenAll(languages.Select(x => update.GetTargetedPlanAsync(x, editionPkg))).ConfigureAwait(false);

                    foreach (BuildTargets.EditionPlanningWithLanguage plan in plans)
                    {
                        Logging.Log("");
                        Logging.Log("Editions available for language: " + plan.LanguageCode);
                        plan.EditionTargets.PrintAvailablePlan();
                    }
                }
            }

            await DownloadLib.UpdateUtils.ProcessUpdateAsync(update, pOutputFolder, MachineType, new ReportProgress(), Language, Edition, WriteMetadata).ConfigureAwait(false);
        }
    }
}
