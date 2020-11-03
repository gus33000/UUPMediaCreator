using CommandLine;
using CompDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UUPDownload.Downloading;
using WindowsUpdateLib;

namespace UUPDownload
{
    internal class Program
    {
        private static void Main(string[] args)
        {
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

            CTAC ctac = FE3Handler.BuildCTAC(o.ReportingSku, o.ReportingVersion, o.MachineType, o.FlightRing, o.FlightingBranchName, o.BranchReadinessLevel, o.CurrentBranch, o.SyncCurrentVersionOnly);
            UpdateData[] data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            data = data.Select(x => UpdateUtils.TrimDeltasFromUpdateData(x)).ToArray();

            foreach (UpdateData update in data)
            {
                await ProcessUpdateAsync(update, o.OutputFolder, o.MachineType, o.Language, o.Edition, true);
            }
            Logging.Log("Completed.");
            if (Debugger.IsAttached)
                Console.ReadLine();
        }

        private static DateTime GetFileExpirationDateTime(CGetExtendedUpdateInfo2Response.FileLocation fileLocation)
        {
            DateTime dateTime = DateTime.MaxValue;
            try
            {
                long value = long.Parse(fileLocation.Url.Split("P1=")[1].Split("&")[0]);
                dateTime = DateTimeOffset.FromUnixTimeSeconds(value).ToLocalTime().DateTime;
            }
            catch { }
            return dateTime;
        }

        private static async Task ProcessUpdateAsync(UpdateData update, string pOutputFolder, MachineType MachineType, string Language = "", string Edition = "", bool WriteMetadata = true)
        {
            bool getSpecific = !string.IsNullOrEmpty(Language) && !string.IsNullOrEmpty(Edition);

            Logging.Log("Title: " + update.Xml.LocalizedProperties.Title);
            Logging.Log("Description: " + update.Xml.LocalizedProperties.Description);

            HashSet<CompDBXmlClass.PayloadItem> payloadItems = new HashSet<CompDBXmlClass.PayloadItem>();
            HashSet<CompDBXmlClass.PayloadItem> bannedPayloadItems = new HashSet<CompDBXmlClass.PayloadItem>();
            string name = update.Xml.UpdateIdentity.UpdateID + "." + update.Xml.UpdateIdentity.RevisionNumber;
            string OutputFolder = Path.Combine(pOutputFolder, name);

            Logging.Log("Getting CompDBs...");
            CompDBXmlClass.CompDB[] compDBs = await UpdateUtils.GetCompDBs(update);
            Logging.Log("Parsing CompDBs...");
            CompDBXmlClass.CompDB specificCompDB = null;
            if (compDBs != null)
            {
                foreach (CompDBXmlClass.CompDB cdb in compDBs)
                {
                    if (getSpecific)
                    {
                        bool editionMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Edition, StringComparison.InvariantCultureIgnoreCase)) == true;
                        bool langMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Language, StringComparison.InvariantCultureIgnoreCase)) == true;
                        bool typeMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase)) == true;

                        if (typeMatching)
                        {
                            if (editionMatching && langMatching)
                            {
                                specificCompDB = cdb;
                            }
                            else if (cdb.Tags.Type.Equals("Language", StringComparison.InvariantCultureIgnoreCase) ||
                                cdb.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) ||
                                cdb.Tags.Type.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase))
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

            if (getSpecific)
            {
                if (specificCompDB == null)
                {
                    Logging.Log("No update metadata matched the specified criteria", Logging.LoggingLevel.Warning);
                    return;
                }
                else
                {
                    foreach (CompDBXmlClass.Package pkg in specificCompDB.Packages.Package)
                    {
                        bannedPayloadItems.RemoveWhere(x => x.PayloadHash == pkg.Payload.PayloadItem.PayloadHash);
                    }
                }
            }

            int returnCode = 0;
            IEnumerable<CExtendedUpdateInfoXml.File> filesToDownload = null;
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            do
            {
                Logging.Log("Getting file urls...");
                CGetExtendedUpdateInfo2Response.FileLocation[] fileUrls = await FE3Handler.GetFileUrls(update, null, update.CTAC);

                if (!Directory.Exists(OutputFolder))
                {
                    Directory.CreateDirectory(OutputFolder);
                    try
                    {
                        string filename = Path.Combine(OutputFolder, update.Xml.LocalizedProperties.Title + " (" + MachineType.ToString() + ").uupmcreplay");
                        if (WriteMetadata && !File.Exists(filename))
                        {
                            File.WriteAllText(filename, JsonConvert.SerializeObject(update, Formatting.Indented));
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
                    filesToDownload = filesToDownload.Where(x => UpdateUtils.ShouldFileGetDownloaded(x, OutputFolder, payloadItems));
                }
                else
                {
                    filesToDownload = update.Xml.Files.File.Where(x => !IsFileBanned(x, bannedPayloadItems)).Where(x => UpdateUtils.ShouldFileGetDownloaded(x, OutputFolder, payloadItems)).OrderBy(x => ulong.Parse(x.Size));
                }

                returnCode = 0;

                HashSet<Task<int>> tasks = new HashSet<Task<int>>();
                IEnumerable<(CExtendedUpdateInfoXml.File, CGetExtendedUpdateInfo2Response.FileLocation)> boundList = filesToDownload.Select(x => (x, fileUrls.First(y => y.FileDigest == x.Digest))).OrderBy(x => GetFileExpirationDateTime(x.Item2));

                int maxConcurrency = 20;
                using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(maxConcurrency))
                {
                    ServicePointManager.DefaultConnectionLimit = int.MaxValue;

                    foreach ((CExtendedUpdateInfoXml.File, CGetExtendedUpdateInfo2Response.FileLocation) boundFile in boundList)
                    {
                        concurrencySemaphore.Wait();
                        tasks.Add(DownloadHelper.GetDownloadFileTask(OutputFolder, UpdateUtils.GetFilenameForCEUIFile(boundFile.Item1, payloadItems), boundFile.Item2.Url, concurrencySemaphore));
                    }

                    int[] res = await Task.WhenAll(tasks.ToArray());
                    if (res.Any(x => x != 0))
                    {
                        returnCode = -1;
                        Logging.Log("Previous download did not fully succeed, resuming past downloads...");
                        continue;
                    }
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