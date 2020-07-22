using CommandLine;
using CompDB;
using Microsoft.Cabinet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WindowsUpdateLib;

namespace UUPDownload
{
    class Program
    {
        static void Main(string[] args)
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
            CTAC ctac;
            UpdateData[] data;
            Logging.Log("Checking for updates...");
            ctac = FE3Handler.BuildCTAC(o.ReportingSku, o.ReportingVersion, o.MachineType, o.FlightRing, o.FlightingBranchName, o.BranchReadinessLevel, o.CurrentBranch, o.SyncCurrentVersionOnly);
            data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            data = data.Select(x => TrimDeltasFromUpdateData(x)).ToArray();

            foreach (var update in data)
            {
                Logging.Log(update.Xml.LocalizedProperties.Title);
                Logging.Log(update.Xml.LocalizedProperties.Description);

                List<CompDBXmlClass.PayloadItem> payloadItems = new List<CompDBXmlClass.PayloadItem>();

                Logging.Log("Getting CompDBs...");
                var compDBs = await GetCompDBs(update);
                if (compDBs != null)
                {
                    foreach (var cdb in compDBs)
                    {
                        foreach (var pkg in cdb.Packages.Package)
                        {
                            payloadItems.Add(pkg.Payload.PayloadItem);
                        }
                    }
                }

                string name = update.Xml.LocalizedProperties.Title + " (" + o.MachineType.ToString() + ") (" + update.Xml.UpdateIdentity.UpdateID + "." + update.Xml.UpdateIdentity.RevisionNumber + ")";

                string OutputFolder = Path.Combine(o.OutputFolder, name);

                int returnCode = 0;

                do
                {
                    Logging.Log("Getting file urls...");
                    var urls = await FE3Handler.GetFileUrls(update, null, ctac);

                    Logging.Log("Generating aria2 script...");
                    string outputtext = "";

                    foreach (var file in update.Xml.Files.File.OrderBy(x => ulong.Parse(x.Size)))
                    {
                        string filename = file.FileName;

                        if (payloadItems.Any(x => x.PayloadHash == file.AdditionalDigest.Text))
                        {
                            filename = payloadItems.First(x => x.PayloadHash == file.AdditionalDigest.Text).Path;
                        }

                        if (filename.Contains("Diff", StringComparison.InvariantCultureIgnoreCase) ||
                            filename.Contains("Baseless", StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        Logging.Log("Adding file: " + filename);
                        var url = urls.First(x => x.FileDigest == file.Digest);
                        outputtext += url.Url + "\n";
                        outputtext += "  out=" + OutputFolder + "\\" + filename + "\n";
                        outputtext += "  checksum=" + file.DigestAlgorithm.ToLower().Replace("sha", "sha-") + "=" + Base64Decode(file.Digest).ToLower() + "\n";
                        outputtext += "\n";
                    }
                    File.WriteAllText(name + ".txt", outputtext);

                    string cmdline = "--no-conf --log-level=info --log=\"" + name + ".log\" --allow-overwrite=true --auto-file-renaming=false -i \"" + name + ".txt\"";
                    Process proc = new Process();
                    proc.StartInfo = new ProcessStartInfo("aria2c.exe", cmdline);
                    proc.StartInfo.UseShellExecute = false;
                    proc.Start();
                    proc.WaitForExit();
                    returnCode = proc.ExitCode;
                }
                while (returnCode != 0);
            }
            Logging.Log("Completed.");
            if (Debugger.IsAttached)
                Console.ReadLine();
        }

        private static async Task<CompDBXmlClass.CompDB> GetNeutralCompDB(UpdateData update)
        {
            CompDBXmlClass.CompDB neutralCompDB = null;
            List<CExtendedUpdateInfoXml.File> metadataCabs = new List<CExtendedUpdateInfoXml.File>();

            foreach (var file in update.Xml.Files.File)
            {
                if (file.PatchingType.Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                {
                    metadataCabs.Add(file);
                }
            }

            if (metadataCabs.Count == 0)
            {
                return neutralCompDB;
            }

            if (metadataCabs.Count == 1 && metadataCabs[0].FileName.Contains("metadata", StringComparison.InvariantCultureIgnoreCase))
            {
                // This is the new metadata format where all metadata is in a single cab

                if (string.IsNullOrEmpty(update.CachedMetadata))
                {
                    string metadataUrl = await FE3Handler.GetFileUrl(update, metadataCabs[0].Digest, null, update.CTAC);
                    string metadataCabTemp = Path.GetTempFileName();

                    // Download the file
                    WebClient client = new WebClient();
                    await client.DownloadFileTaskAsync(new Uri(metadataUrl), metadataCabTemp);

                    update.CachedMetadata = metadataCabTemp;
                }

                using (var cabinet = new CabinetHandler(File.OpenRead(update.CachedMetadata)))
                {
                    foreach (var file in cabinet.Files.Where(x =>
                        x.Contains("desktoptargetcompdb_", StringComparison.InvariantCultureIgnoreCase) &&
                        x.Count(y => y == '_') == 2 &&
                        !x.Contains("tools", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var lang = file.Split('_').Last().Replace(".xml.cab", "", StringComparison.InvariantCultureIgnoreCase);
                        if (!lang.Equals("neutral", StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        using (CabinetHandler cabinet2 = new CabinetHandler(cabinet.OpenFile(file)))
                        {
                            string xmlfile = cabinet2.Files.First();
                            using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                            {
                                neutralCompDB = CompDBXmlClass.DeserializeCompDB(xmlstream);
                            }
                        }
                    }
                }
            }
            else
            {
                // This is the old format, each cab is a file in WU
                foreach (var file in metadataCabs.Where(x =>
                    x.FileName.StartsWith("_desktoptargetcompdb_", StringComparison.InvariantCultureIgnoreCase) &&
                    x.FileName.Count(y => y == '_') == 3 &&
                    !x.FileName.Contains("tools", StringComparison.InvariantCultureIgnoreCase)))
                {
                    var lang = file.FileName.Split('_').Last().Replace(".xml.cab", "", StringComparison.InvariantCultureIgnoreCase);
                    if (!lang.Equals("neutral", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    if (string.IsNullOrEmpty(update.CachedMetadata))
                    {
                        string metadataUrl = await FE3Handler.GetFileUrl(update, file.Digest, null, update.CTAC);
                        string metadataCabTemp = Path.GetTempFileName();

                        // Download the file
                        WebClient client = new WebClient();
                        await client.DownloadFileTaskAsync(new Uri(metadataUrl), metadataCabTemp);

                        update.CachedMetadata = metadataCabTemp;
                    }

                    using (CabinetHandler cabinet2 = new CabinetHandler(File.OpenRead(update.CachedMetadata)))
                    {
                        string xmlfile = cabinet2.Files.First();
                        using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                        {
                            neutralCompDB = CompDBXmlClass.DeserializeCompDB(xmlstream);
                        }
                    }
                }
            }

            return neutralCompDB;
        }

        private static async Task<CompDBXmlClass.CompDB> GetToolsCompDB(UpdateData update)
        {
            CompDBXmlClass.CompDB neutralCompDB = null;
            List<CExtendedUpdateInfoXml.File> metadataCabs = new List<CExtendedUpdateInfoXml.File>();

            foreach (var file in update.Xml.Files.File)
            {
                if (file.PatchingType.Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                {
                    metadataCabs.Add(file);
                }
            }

            if (metadataCabs.Count == 0)
            {
                return neutralCompDB;
            }

            if (metadataCabs.Count == 1 && metadataCabs[0].FileName.Contains("metadata", StringComparison.InvariantCultureIgnoreCase))
            {
                // This is the new metadata format where all metadata is in a single cab

                if (string.IsNullOrEmpty(update.CachedMetadata))
                {
                    string metadataUrl = await FE3Handler.GetFileUrl(update, metadataCabs[0].Digest, null, update.CTAC);
                    string metadataCabTemp = Path.GetTempFileName();

                    // Download the file
                    WebClient client = new WebClient();
                    await client.DownloadFileTaskAsync(new Uri(metadataUrl), metadataCabTemp);

                    update.CachedMetadata = metadataCabTemp;
                }

                using (var cabinet = new CabinetHandler(File.OpenRead(update.CachedMetadata)))
                {
                    foreach (var file in cabinet.Files.Where(x =>
                        x.Contains("desktoptargetcompdb_", StringComparison.InvariantCultureIgnoreCase) &&
                        x.Count(y => y == '_') == 2 &&
                        x.Contains("tools", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var lang = file.Split('_').Last().Replace(".xml.cab", "", StringComparison.InvariantCultureIgnoreCase);
                        if (!lang.Equals("tools", StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        using (CabinetHandler cabinet2 = new CabinetHandler(cabinet.OpenFile(file)))
                        {
                            string xmlfile = cabinet2.Files.First();
                            using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                            {
                                neutralCompDB = CompDBXmlClass.DeserializeCompDB(xmlstream);
                            }
                        }
                    }
                }
            }
            else
            {
                // This is the old format, each cab is a file in WU
                foreach (var file in metadataCabs.Where(x =>
                    x.FileName.StartsWith("_desktoptargetcompdb_", StringComparison.InvariantCultureIgnoreCase) &&
                    x.FileName.Count(y => y == '_') == 3 &&
                    x.FileName.Contains("tools", StringComparison.InvariantCultureIgnoreCase)))
                {
                    var lang = file.FileName.Split('_').Last().Replace(".xml.cab", "", StringComparison.InvariantCultureIgnoreCase);
                    if (!lang.Equals("tools", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    if (string.IsNullOrEmpty(update.CachedMetadata))
                    {
                        string metadataUrl = await FE3Handler.GetFileUrl(update, file.Digest, null, update.CTAC);
                        string metadataCabTemp = Path.GetTempFileName();

                        // Download the file
                        WebClient client = new WebClient();
                        await client.DownloadFileTaskAsync(new Uri(metadataUrl), metadataCabTemp);

                        update.CachedMetadata = metadataCabTemp;
                    }

                    using (CabinetHandler cabinet2 = new CabinetHandler(File.OpenRead(update.CachedMetadata)))
                    {
                        string xmlfile = cabinet2.Files.First();
                        using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                        {
                            neutralCompDB = CompDBXmlClass.DeserializeCompDB(xmlstream);
                        }
                    }
                }
            }

            return neutralCompDB;
        }

        private static async Task<CompDBXmlClass.CompDB[]> GetLXPCompDBs(UpdateData update)
        {
            List<CompDBXmlClass.CompDB> neutralCompDB = new List<CompDBXmlClass.CompDB>();
            List<CExtendedUpdateInfoXml.File> metadataCabs = new List<CExtendedUpdateInfoXml.File>();

            foreach (var file in update.Xml.Files.File)
            {
                if (file.PatchingType.Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                {
                    metadataCabs.Add(file);
                }
            }

            if (metadataCabs.Count == 0)
            {
                return neutralCompDB.ToArray();
            }

            if (metadataCabs.Count == 1 && metadataCabs[0].FileName.Contains("metadata", StringComparison.InvariantCultureIgnoreCase))
            {
                // This is the new metadata format where all metadata is in a single cab

                if (string.IsNullOrEmpty(update.CachedMetadata))
                {
                    string metadataUrl = await FE3Handler.GetFileUrl(update, metadataCabs[0].Digest, null, update.CTAC);
                    string metadataCabTemp = Path.GetTempFileName();

                    // Download the file
                    WebClient client = new WebClient();
                    await client.DownloadFileTaskAsync(new Uri(metadataUrl), metadataCabTemp);

                    update.CachedMetadata = metadataCabTemp;
                }

                using (var cabinet = new CabinetHandler(File.OpenRead(update.CachedMetadata)))
                {
                    foreach (var file in cabinet.Files.Where(x =>
                        x.Contains("desktoptargetcompdb_", StringComparison.InvariantCultureIgnoreCase) &&
                        x.Count(y => y == '_') == 3 &&
                        !x.Contains("tools", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var lang = file.Split('_').Reverse().Skip(1).Reverse().Last().Replace(".xml.cab", "", StringComparison.InvariantCultureIgnoreCase);
                        if (!lang.Equals("lxp", StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        using (CabinetHandler cabinet2 = new CabinetHandler(cabinet.OpenFile(file)))
                        {
                            string xmlfile = cabinet2.Files.First();
                            using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                            {
                                neutralCompDB.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                            }
                        }
                    }
                }
            }
            else
            {
                // This is the old format, each cab is a file in WU
                foreach (var file in metadataCabs.Where(x =>
                    x.FileName.StartsWith("_desktoptargetcompdb_", StringComparison.InvariantCultureIgnoreCase) &&
                    x.FileName.Count(y => y == '_') == 4 &&
                    !x.FileName.Contains("tools", StringComparison.InvariantCultureIgnoreCase)))
                {
                    var lang = file.FileName.Split('_').Reverse().Skip(1).Reverse().Last().Replace(".xml.cab", "", StringComparison.InvariantCultureIgnoreCase);
                    if (!lang.Equals("lxp", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    if (string.IsNullOrEmpty(update.CachedMetadata))
                    {
                        string metadataUrl = await FE3Handler.GetFileUrl(update, file.Digest, null, update.CTAC);
                        string metadataCabTemp = Path.GetTempFileName();

                        // Download the file
                        WebClient client = new WebClient();
                        await client.DownloadFileTaskAsync(new Uri(metadataUrl), metadataCabTemp);

                        update.CachedMetadata = metadataCabTemp;
                    }

                    using (CabinetHandler cabinet2 = new CabinetHandler(File.OpenRead(update.CachedMetadata)))
                    {
                        string xmlfile = cabinet2.Files.First();
                        using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                        {
                            neutralCompDB.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                        }
                    }
                }
            }

            return neutralCompDB.ToArray();
        }

        private static async Task<CompDBXmlClass.CompDB[]> GetCompDBs(UpdateData update)
        {
            List<CompDBXmlClass.CompDB> neutralCompDB = new List<CompDBXmlClass.CompDB>();
            List<CExtendedUpdateInfoXml.File> metadataCabs = new List<CExtendedUpdateInfoXml.File>();

            foreach (var file in update.Xml.Files.File)
            {
                if (file.PatchingType.Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                {
                    metadataCabs.Add(file);
                }
            }

            if (metadataCabs.Count == 0)
            {
                return neutralCompDB.ToArray();
            }

            if (metadataCabs.Count == 1 && metadataCabs[0].FileName.Contains("metadata", StringComparison.InvariantCultureIgnoreCase))
            {
                // This is the new metadata format where all metadata is in a single cab

                if (string.IsNullOrEmpty(update.CachedMetadata))
                {
                    string metadataUrl = await FE3Handler.GetFileUrl(update, metadataCabs[0].Digest, null, update.CTAC);
                    string metadataCabTemp = Path.GetTempFileName();

                    // Download the file
                    WebClient client = new WebClient();
                    await client.DownloadFileTaskAsync(new Uri(metadataUrl), metadataCabTemp);

                    update.CachedMetadata = metadataCabTemp;
                }

                using (var cabinet = new CabinetHandler(File.OpenRead(update.CachedMetadata)))
                {
                    foreach (var file in cabinet.Files)
                    {
                        using (CabinetHandler cabinet2 = new CabinetHandler(cabinet.OpenFile(file)))
                        {
                            string xmlfile = cabinet2.Files.First();
                            using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                            {
                                neutralCompDB.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                            }
                        }
                    }
                }
            }
            else
            {
                // This is the old format, each cab is a file in WU
                foreach (var file in metadataCabs)
                {
                    string metadataUrl = await FE3Handler.GetFileUrl(update, file.Digest, null, update.CTAC);
                    string metadataCabTemp = Path.GetTempFileName();

                    // Download the file
                    WebClient client = new WebClient();
                    await client.DownloadFileTaskAsync(new Uri(metadataUrl), metadataCabTemp);

                    update.CachedMetadata = metadataCabTemp;

                    using (CabinetHandler cabinet2 = new CabinetHandler(File.OpenRead(update.CachedMetadata)))
                    {
                        string xmlfile = cabinet2.Files.First();
                        using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                        {
                            neutralCompDB.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                        }
                    }
                }
            }

            return neutralCompDB.ToArray();
        }

        private static UpdateData TrimDeltasFromUpdateData(UpdateData update)
        {
            update.Xml.Files.File = update.Xml.Files.File.Where(x => !x.FileName.EndsWith(".psf", StringComparison.InvariantCultureIgnoreCase) 
            && !x.FileName.StartsWith("Diff", StringComparison.InvariantCultureIgnoreCase)
             && !x.FileName.StartsWith("Baseless", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            return update;
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return BitConverter.ToString(base64EncodedBytes).Replace("-", "");
        }
    }
}
