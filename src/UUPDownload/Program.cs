using CommandLine;
using Common;
using CompDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UUPDownload.Downloading;
using WindowsUpdateLib;

namespace UUPDownload
{
    public class UpdateScan
    {
        public UpdateData UpdateData { get; set; }
        public BuildTargets.EditionPlanningWithLanguage[] Targets { get; set; }
        public MachineType Architecture { get; set; }
        public string BuildString { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string Serialize()
        {
            var xmlSerializer = new XmlSerializer(typeof(UpdateScan));

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("s", "http://www.w3.org/2003/05/soap-envelope");
            ns.Add("a", "http://www.w3.org/2005/08/addressing");

            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
                {
                    xmlSerializer.Serialize(xmlWriter, this, ns);
                    return stringWriter.ToString().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
                }
            }
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            Parser.Default.ParseArguments<DownloadRequestOptions, DownloadReplayOptions>(args).MapResult(
              (DownloadRequestOptions opts) =>
              {
                  Logging.Log("UUPDownload - Downloads complete updates from Microsoft's Unified Update Platform");
                  Logging.Log("Copyright (c) 2020, Gustave Monce - gus33000.me - @gus33000");
                  Logging.Log("Released under the MIT license at github.com/gus33000/UUPMediaCreator");
                  Logging.Log("");

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
              },
              (DownloadReplayOptions opts) =>
              {
                  Logging.Log("UUPDownload - Downloads complete updates from Microsoft's Unified Update Platform");
                  Logging.Log("Copyright (c) 2020, Gustave Monce - gus33000.me - @gus33000");
                  Logging.Log("Released under the MIT license at github.com/gus33000/UUPMediaCreator");
                  Logging.Log("");

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
              },
              errs => 1);
        }

        private static async Task PerformOperation(DownloadRequestOptions o)
        {
            Logging.Log("Checking for updates...");

            CTAC ctac = new CTAC(o.ReportingSku, o.ReportingVersion, o.MachineType, o.FlightRing, o.FlightingBranchName, o.BranchReadinessLevel, o.CurrentBranch, o.ReleaseType, o.SyncCurrentVersionOnly);
            IEnumerable<UpdateData> data = await FE3Handler.GetUpdates(null, ctac, null, FileExchangeV3UpdateFilter.ProductRelease);
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

        private static async Task BuildUpdateXml(UpdateData update, MachineType MachineType)
        {
            string buildstr = "";
            IEnumerable<string> languages = null;

            var compDBs = await update.GetCompDBsAsync();

            await Task.WhenAll(
                Task.Run(async () => buildstr = await update.GetBuildStringAsync()),
                Task.Run(async () => languages = await update.GetAvailableLanguagesAsync()));

            buildstr = buildstr ?? "";

            CompDBXmlClass.Package editionPackPkg = compDBs.GetEditionPackFromCompDBs();
            string editionPkg = await update.DownloadFileFromDigestAsync(editionPackPkg.Payload.PayloadItem.PayloadHash);
            var plans = await Task.WhenAll(languages.Select(x => update.GetTargetedPlanAsync(x, editionPkg)));

            UpdateScan scan = new UpdateScan()
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
            HashSet<CompDBXmlClass.PayloadItem> payloadItems = new HashSet<CompDBXmlClass.PayloadItem>();
            HashSet<CompDBXmlClass.PayloadItem> bannedPayloadItems = new HashSet<CompDBXmlClass.PayloadItem>();
            HashSet<CompDBXmlClass.CompDB> specificCompDBs = new HashSet<CompDBXmlClass.CompDB>();

            string buildstr = "";
            IEnumerable<string> languages = null;

            int returnCode = 0;
            IEnumerable<CExtendedUpdateInfoXml.File> filesToDownload = null;

            bool getSpecific = !string.IsNullOrEmpty(Language) && !string.IsNullOrEmpty(Edition);
            bool getSpecificLanguage = !string.IsNullOrEmpty(Language) && string.IsNullOrEmpty(Edition);

            Logging.Log("Gathering update metadata...");

            var compDBs = await update.GetCompDBsAsync();

            await Task.WhenAll(
                Task.Run(async () => buildstr = await update.GetBuildStringAsync()),
                Task.Run(async () => languages = await update.GetAvailableLanguagesAsync()));

            buildstr = buildstr ?? "";

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

            string name = $"{buildstr.Replace(" ", ".").Replace("(", "").Replace(")", "")}_{MachineType.ToString().ToLower()}fre_{update.Xml.UpdateIdentity.UpdateID.Split("-")[^1]}";
            string OutputFolder = Path.Combine(pOutputFolder, name);

            Logging.Log("Title: " + update.Xml.LocalizedProperties.Title);
            Logging.Log("Description: " + update.Xml.LocalizedProperties.Description);
            Logging.Log("Build String: " + buildstr);
            Logging.Log("Languages: " + string.Join(", ", languages));

            Logging.Log("Parsing CompDBs...");

            if (compDBs != null)
            {
                foreach (CompDBXmlClass.CompDB cdb in compDBs)
                {
                    if (getSpecific || getSpecificLanguage)
                    {
                        bool editionMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Edition, StringComparison.InvariantCultureIgnoreCase)) == true;
                        bool langMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Language, StringComparison.InvariantCultureIgnoreCase)) == true;
                        bool typeMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase)) == true;

                        if (typeMatching)
                        {
                            if ((getSpecificLanguage && langMatching) || (getSpecific && editionMatching && langMatching))
                            {
                                specificCompDBs.Add(cdb);
                            }
                            else if (cdb.Tags.Type.Equals("Language", StringComparison.InvariantCultureIgnoreCase) ||
                                cdb.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) ||
                                (!getSpecificLanguage && cdb.Tags.Type.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
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
                }
            }

            if (getSpecific || getSpecificLanguage)
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
                        string filename = Path.Combine(OutputFolder, update.Xml.LocalizedProperties.Title + " (" + MachineType.ToString() + ").uupmcreplay");
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

                if (filesToDownload != null)
                {
                    filesToDownload = filesToDownload.AsParallel().Where(x => UpdateUtils.ShouldFileGetDownloaded(x, OutputFolder, payloadItems));
                }
                else
                {
                    filesToDownload = update.Xml.Files.File.AsParallel().Where(x => !IsFileBanned(x, bannedPayloadItems) && UpdateUtils.ShouldFileGetDownloaded(x, OutputFolder, payloadItems)).OrderBy(x => ulong.Parse(x.Size));
                }

                returnCode = 0;

                IEnumerable<(CExtendedUpdateInfoXml.File, FileExchangeV3FileDownloadInformation)> boundList = filesToDownload.AsParallel().Select(x => (x, fileUrls.First(y => y.Digest == x.Digest))).OrderBy(x => x.Item2.ExpirationDate);

                foreach ((CExtendedUpdateInfoXml.File, FileExchangeV3FileDownloadInformation) boundFile in boundList)
                {
                    if (await DownloadHelper.GetDownloadFileTask(OutputFolder, UpdateUtils.GetFilenameForCEUIFile(boundFile.Item1, payloadItems), boundFile.Item2) != 0)
                        returnCode = -1;
                }

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