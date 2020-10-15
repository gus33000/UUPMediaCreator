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
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                Logging.Log("UUPDownload - Downloads complete updates from Microsoft's Unified Update Platform");
                Logging.Log("Copyright (c) 2020, Gustave Monce - gus33000.me - @gus33000");
                Logging.Log("Released under the MIT license at github.com/gus33000/UUPMediaCreator");
                Logging.Log("");

                try
                {
                    PerformOperation(o).Wait();
                }
                catch (Exception ex)
                {
                    Logging.Log("Something happened.", Logging.LoggingLevel.Error);
                    Logging.Log(ex.Message, Logging.LoggingLevel.Error);
                    Logging.Log(ex.StackTrace, Logging.LoggingLevel.Error);
                    if (Debugger.IsAttached)
                        Console.ReadLine();
                    Environment.Exit(1);
                }
            });
        }

        private static async Task PerformOperation(Options o)
        {
            Logging.Log("Checking for updates...");

            CTAC ctac = FE3Handler.BuildCTAC(o.ReportingSku, o.ReportingVersion, o.MachineType, o.FlightRing, o.FlightingBranchName, o.BranchReadinessLevel, o.CurrentBranch, o.SyncCurrentVersionOnly);
            UpdateData[] data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            data = data.Select(x => UpdateUtils.TrimDeltasFromUpdateData(x)).ToArray();

            foreach (UpdateData update in data)
            {
                Logging.Log(update.Xml.LocalizedProperties.Title);
                Logging.Log(update.Xml.LocalizedProperties.Description);

                HashSet<CompDBXmlClass.PayloadItem> payloadItems = new HashSet<CompDBXmlClass.PayloadItem>();
                string name = update.Xml.UpdateIdentity.UpdateID + "." + update.Xml.UpdateIdentity.RevisionNumber;
                string OutputFolder = Path.Combine(o.OutputFolder, name);

                Logging.Log("Getting CompDBs...");
                CompDBXmlClass.CompDB[] compDBs = await UpdateUtils.GetCompDBs(update);
                if (compDBs != null)
                {
                    foreach (CompDBXmlClass.CompDB cdb in compDBs)
                    {
                        foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                        {
                            payloadItems.Add(pkg.Payload.PayloadItem);
                        }
                    }
                }

                int returnCode = 0;
                HashSet<string> downloadedFiles = new HashSet<string>();
                IEnumerable<CExtendedUpdateInfoXml.File> files = null;

                do
                {
                    Logging.Log("Getting file urls...");
                    CGetExtendedUpdateInfo2Response.FileLocation[] urls = await FE3Handler.GetFileUrls(update, null, ctac);

                    if (!Directory.Exists(OutputFolder))
                    {
                        Directory.CreateDirectory(OutputFolder);
                        try
                        {
                            File.WriteAllText(Path.Combine(OutputFolder, update.Xml.LocalizedProperties.Title + " (" + o.MachineType.ToString() + ").txt"), JsonConvert.SerializeObject(update, Newtonsoft.Json.Formatting.Indented));
                        }
                        catch (Exception ex)
                        {
                            Logging.Log(ex.ToString(), Logging.LoggingLevel.Error);
                            throw ex;
                        }
                    }

                    if (files != null)
                    {
                        files = update.Xml.Files.File.Where(x => files.Any(y => y.Digest == x.Digest)).Where(x => UpdateUtils.ShouldFileGetDownloaded(x, OutputFolder, payloadItems, downloadedFiles)).OrderBy(x => ulong.Parse(x.Size));
                    }
                    else
                    {
                        files = update.Xml.Files.File.Where(x => UpdateUtils.ShouldFileGetDownloaded(x, OutputFolder, payloadItems, downloadedFiles)).OrderBy(x => ulong.Parse(x.Size));
                    }

                    returnCode = 0;

                    ServicePointManager.DefaultConnectionLimit = int.MaxValue;
                    HashSet<Task<int>> tasks = new HashSet<Task<int>>();
                    DateTime startTime = DateTime.Now;

                    int maxConcurrency = 20;
                    using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(maxConcurrency))
                    {
                        ServicePointManager.DefaultConnectionLimit = int.MaxValue;

                        foreach (CExtendedUpdateInfoXml.File file2 in files)
                        {
                            concurrencySemaphore.Wait();
                            tasks.Add(DownloadHelper.GetDownloadFileTask(OutputFolder, UpdateUtils.GetFilenameForCEUIFile(file2, payloadItems), urls.First(x => x.FileDigest == file2.Digest).Url, concurrencySemaphore));
                        }

                        int[] res = await Task.WhenAll(tasks.ToArray());
                        if (res.Any(x => x != 0))
                        {
                            returnCode = -1;
                            break;
                        }
                    }
                }
                while (returnCode != 0);
            }
            Logging.Log("Completed.");
            if (Debugger.IsAttached)
                Console.ReadLine();
        }
    }
}