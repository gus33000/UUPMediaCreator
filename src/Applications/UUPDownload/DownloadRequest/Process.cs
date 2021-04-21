// Copyright (c) Gustave Monce and Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using CompDB;
using Download.Downloading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                Logging.Log(ex.Message, Logging.LoggingLevel.Error);
                Logging.Log(ex.StackTrace, Logging.LoggingLevel.Error);
                if (Debugger.IsAttached)
                    Console.ReadLine();
                return 1;
            }

            return 0;
        }

        internal static int ParseReplayOptions(DownloadReplayOptions opts)
        {
            try
            {
                UpdateData update = JsonConvert.DeserializeObject<UpdateData>(File.ReadAllText(opts.ReplayMetadata));
                ProcessUpdateAsync(update, opts.OutputFolder, opts.MachineType, opts.Language, opts.Edition, true).Wait();
            }
            catch (Exception ex)
            {
                Logging.Log("Something happened.", Logging.LoggingLevel.Error);
                Logging.Log(ex.Message, Logging.LoggingLevel.Error);
                Logging.Log(ex.StackTrace, Logging.LoggingLevel.Error);
                if (ex.InnerException != null)
                {
                    Logging.Log("\tSomething happened.", Logging.LoggingLevel.Error);
                    Logging.Log("\t" + ex.InnerException.Message, Logging.LoggingLevel.Error);
                    Logging.Log("\t" + ex.InnerException.StackTrace, Logging.LoggingLevel.Error);
                }
                if (Debugger.IsAttached)
                    Console.ReadLine();
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
                token = await MBIHelper.GenerateMicrosoftAccountTokenAsync(o.Mail, o.Password);

            IEnumerable<UpdateData> data = await FE3Handler.GetUpdates(null, ctac, token, FileExchangeV3UpdateFilter.ProductRelease);
            data = data.Select(x => UpdateUtils.TrimDeltasFromUpdateData(x));

            foreach (UpdateData update in data)
            {
                await ProcessUpdateAsync(update, o.OutputFolder, o.MachineType, o.Language, o.Edition, true);
                //await BuildUpdateXml(update, o.MachineType);
            }
            Logging.Log("Completed.");
            if (Debugger.IsAttached)
                Console.ReadLine();
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static async Task BuildUpdateXml(UpdateData update, MachineType MachineType)
#pragma warning restore IDE0051 // Remove unused private members
        {
            string buildstr = "";
            IEnumerable<string> languages = null;

            var compDBs = await update.GetCompDBsAsync();

            await Task.WhenAll(
                Task.Run(async () => buildstr = await update.GetBuildStringAsync()),
                Task.Run(async () => languages = await update.GetAvailableLanguagesAsync()));

            buildstr ??= "";

            CompDBXmlClass.Package editionPackPkg = compDBs.GetEditionPackFromCompDBs();
            string editionPkg = await update.DownloadFileFromDigestAsync(editionPackPkg.Payload.PayloadItem.PayloadHash);
            var plans = await Task.WhenAll(languages.Select(x => update.GetTargetedPlanAsync(x, editionPkg)));

            UpdateScan scan = new()
            {
                Architecture = MachineType,
                BuildString = buildstr,
                Description = update.Xml.LocalizedProperties.Description,
                Title = update.Xml.LocalizedProperties.Title,
                Targets = plans,
                UpdateData = update
            };

            File.WriteAllText("updatetest.xml", scan.Serialize());
        }

        private static async Task ProcessUpdateAsync(UpdateData update, string pOutputFolder, MachineType MachineType, string Language = "", string Edition = "", bool WriteMetadata = true)
        {
            HashSet<CompDBXmlClass.PayloadItem> payloadItems = new();
            HashSet<CompDBXmlClass.PayloadItem> bannedPayloadItems = new();
            HashSet<CompDBXmlClass.CompDB> specificCompDBs = new();

            string buildstr = "";
            IEnumerable<string> languages = null;

            int returnCode = 0;
            IEnumerable<CExtendedUpdateInfoXml.File> filesToDownload = null;

            bool getSpecific = !string.IsNullOrEmpty(Language) && !string.IsNullOrEmpty(Edition);
            bool getSpecificLanguageOnly = !string.IsNullOrEmpty(Language) && string.IsNullOrEmpty(Edition);

            Logging.Log("Title: " + update.Xml.LocalizedProperties.Title);
            Logging.Log("Description: " + update.Xml.LocalizedProperties.Description);

#if DEBUG
            foreach (var file in update.Xml.Files.File.Select(x => UpdateUtils.GetFilenameForCEUIFile(x, payloadItems)).OrderBy(x => x))
            {
                Console.WriteLine(file);
            }
#endif

            Logging.Log("Gathering update metadata...");

            var compDBs = await update.GetCompDBsAsync();

            await Task.WhenAll(
                Task.Run(async () => buildstr = await update.GetBuildStringAsync()),
                Task.Run(async () => languages = await update.GetAvailableLanguagesAsync()));

            buildstr ??= "";

            if (string.IsNullOrEmpty(buildstr) && update.Xml.LocalizedProperties.Title.Contains("(UUP-CTv2)"))
            {
                string unformattedBase = update.Xml.LocalizedProperties.Title.Split(" ")[0];
                buildstr = $"10.0.{unformattedBase.Split(".")[0]}.{unformattedBase.Split(".")[1]} ({unformattedBase.Split(".")[2]}.{unformattedBase.Split(".")[3]})";
            }
            else if (string.IsNullOrEmpty(buildstr))
            {
                buildstr = update.Xml.LocalizedProperties.Title;
            }

            string name = $"{buildstr.Replace(" ", ".").Replace("(", "").Replace(")", "")}_{MachineType.ToString().ToLower()}fre_{update.Xml.UpdateIdentity.UpdateID.Split("-")[^1]}";
            Regex illegalCharacters = new(@"[\\/:*?""<>|]");
            name = illegalCharacters.Replace(name, "");
            string OutputFolder = Path.Combine(pOutputFolder, name);

            Logging.Log("Build String: " + buildstr);
            Logging.Log("Languages: " + string.Join(", ", languages));

            Logging.Log("Parsing CompDBs...");

            if (compDBs != null)
            {
                CompDBXmlClass.Package editionPackPkg = compDBs.GetEditionPackFromCompDBs();
                if (editionPackPkg != null)
                {
                    string editionPkg = await update.DownloadFileFromDigestAsync(editionPackPkg.Payload.PayloadItem.PayloadHash);
                    var plans = await Task.WhenAll(languages.Select(x => update.GetTargetedPlanAsync(x, editionPkg)));

                    foreach (var plan in plans)
                    {
                        Logging.Log("");
                        Logging.Log("Editions available for language: " + plan.LanguageCode);
                        plan.EditionTargets.PrintAvailablePlan();
                    }
                }

                foreach (CompDBXmlClass.CompDB cdb in compDBs)
                {
                    if (getSpecific || getSpecificLanguageOnly)
                    {
                        bool hasLang = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase)) == true;
                        bool hasEdition = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)) == true;

                        bool editionMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Edition, StringComparison.InvariantCultureIgnoreCase)) == true;
                        bool langMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Language, StringComparison.InvariantCultureIgnoreCase)) == true;
                        bool typeMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase) && (x.Value.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase))) == true;

                        bool isDiff = cdb.Tags?.Tag?.Any(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase) && (x.Value.Equals("Diff", StringComparison.InvariantCultureIgnoreCase) || x.Value.Equals("Baseless", StringComparison.InvariantCultureIgnoreCase))) == true;

                        if (typeMatching)
                        {
                            if ((getSpecificLanguageOnly && langMatching) || (getSpecific && editionMatching && langMatching))
                            {
                                specificCompDBs.Add(cdb);
                            }
                            else if (cdb.Tags.Type.Equals("Language", StringComparison.InvariantCultureIgnoreCase) ||
                                cdb.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) ||
                                (!getSpecificLanguageOnly && cdb.Tags.Type.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase)) ||
                                (!getSpecificLanguageOnly && hasLang) || (getSpecific && hasLang && hasEdition))
                            {
                                foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                                {
                                    bannedPayloadItems.Add(pkg.Payload.PayloadItem);
                                }
                            }
                            else
                            {
                                foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                                {
                                    if (pkg.Payload.PayloadItem.PayloadType.Equals("Diff", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        bannedPayloadItems.Add(pkg.Payload.PayloadItem);
                                    }
                                }
                            }
                        }
                        else if (isDiff)
                        {
                            foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                            {
                                bannedPayloadItems.Add(pkg.Payload.PayloadItem);
                            }
                        }
                        else
                        {
                            foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                            {
                                if (pkg.Payload.PayloadItem.PayloadType.Equals("Diff", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    bannedPayloadItems.Add(pkg.Payload.PayloadItem);
                                }
                            }
                        }
                    }
                    else
                    {
                        bool isDiff = cdb.Tags?.Tag?.Any(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase) && (x.Value.Equals("Diff", StringComparison.InvariantCultureIgnoreCase) || x.Value.Equals("Baseless", StringComparison.InvariantCultureIgnoreCase))) == true;
                        
                        if (isDiff)
                        {
                            foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                            {
                                bannedPayloadItems.Add(pkg.Payload.PayloadItem);
                            }
                        }
                        else
                        {
                            foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                            {
                                if (pkg.Payload.PayloadItem.PayloadType.Equals("Diff", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    bannedPayloadItems.Add(pkg.Payload.PayloadItem);
                                }
                            }
                        }
                    }

                    foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                    {
                        payloadItems.Add(pkg.Payload.PayloadItem);
                    }

                    if (cdb.AppX != null)
                        foreach (CompDBXmlClass.Package pkg in cdb.AppX.AppXPackages.Package)
                            payloadItems.Add(pkg.Payload.PayloadItem);
                }
            }

            if (getSpecific || getSpecificLanguageOnly)
            {
                if (specificCompDBs.Count <= 0)
                {
                    Logging.Log("No update metadata matched the specified criteria", Logging.LoggingLevel.Warning);
                    return;
                }
                else
                {
                    foreach (var specificCompDB in specificCompDBs)
                    {
                        foreach (CompDBXmlClass.Package pkg in specificCompDB.Packages.Package)
                        {
                            bannedPayloadItems.RemoveWhere(x => x.PayloadHash == pkg.Payload.PayloadItem.PayloadHash);
                        }
                    }
                }
            }

            do
            {
                Logging.Log("Getting file urls...");
                IEnumerable<FileExchangeV3FileDownloadInformation> fileUrls = await FE3Handler.GetFileUrls(update);

                if (fileUrls == null)
                {
                    Logging.Log("Getting file urls failed.");
                    return;
                }

                if (!Directory.Exists(OutputFolder))
                {
                    Directory.CreateDirectory(OutputFolder);
                    try
                    {
                        string tmpname = update.Xml.LocalizedProperties.Title + " (" + MachineType.ToString() + ").uupmcreplay";
                        illegalCharacters = new Regex(@"[\\/:*?""<>|]");
                        tmpname = illegalCharacters.Replace(tmpname, "");
                        string filename = Path.Combine(OutputFolder, tmpname);
                        if (WriteMetadata && !File.Exists(filename))
                        {
                            File.WriteAllText(filename, JsonConvert.SerializeObject(update, Newtonsoft.Json.Formatting.Indented));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log(ex.ToString(), Logging.LoggingLevel.Error);
                        throw;
                    }
                }

                using HttpDownloader helperDl = new(OutputFolder);

                filesToDownload = update.Xml.Files.File.AsParallel().Where(x => !IsFileBanned(x, bannedPayloadItems));

                returnCode = 0;

                IEnumerable<(CExtendedUpdateInfoXml.File, FileExchangeV3FileDownloadInformation)> boundList = filesToDownload
                    .AsParallel()
                    .Select(x => (x, fileUrls.First(y => y.Digest == x.Digest)))
                    .Where(x => UpdateUtils.ShouldFileGetDownloaded(x.x, payloadItems))
                    .OrderBy(x => x.Item2.ExpirationDate);

                IEnumerable<UUPFile> fileList = boundList.Select(boundFile =>
                {
                    return new UUPFile(
                        boundFile.Item2, 
                        UpdateUtils.GetFilenameForCEUIFile(boundFile.Item1, payloadItems), 
                        long.Parse(boundFile.Item1.Size), 
                        boundFile.Item1.AdditionalDigest.Text);
                });

                returnCode = await helperDl.DownloadAsync(fileList.ToList(), new ReportProgress()) ? 0 : -1;

                if (returnCode != 0)
                {
                    Logging.Log("Previous download did not fully succeed, resuming past downloads...");
                    continue;
                }
            }
            while (returnCode != 0);
        }

        private static bool IsFileBanned(CExtendedUpdateInfoXml.File file2, IEnumerable<CompDBXmlClass.PayloadItem> bannedItems)
        {
            if (bannedItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest))
            {
                return true;
            }

            return false;
        }
    }
}
