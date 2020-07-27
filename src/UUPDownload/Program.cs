using CommandLine;
using CompDB;
using Microsoft.Cabinet;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
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

                string name = update.Xml.UpdateIdentity.UpdateID + "." + update.Xml.UpdateIdentity.RevisionNumber;

                string OutputFolder = Path.Combine(o.OutputFolder, name);

                Logging.Log("Getting CompDBs...");
                var compDBs = await GetCompDBs(update);
                //Logging.Log("Done CompDBs...");
                if (compDBs != null)
                {
                    //Logging.Log("CompDBs is not null...");
                    foreach (var cdb in compDBs)
                    {
                        foreach (var pkg in cdb.Packages.Package)
                        {
                            //Logging.Log(pkg.Payload.PayloadItem.Path);
                            payloadItems.Add(pkg.Payload.PayloadItem);
                        }
                    }
                }

                int returnCode = 0;

                List<string> downloadedFiles = new List<string>();

                do
                {
                    Logging.Log("Getting file urls...");
                    var urls = await FE3Handler.GetFileUrls(update, null, ctac);

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

                    /*Logging.Log("Generating aria2 script...");
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

                    string cmdline = "--no-conf --log-level=info --log=\"" + name + ".log\" --allow-overwrite=true --auto-file-renaming=false --max-concurrent-downloads=4 -i \"" + name + ".txt\"";
                    Process proc = new Process();
                    proc.StartInfo = new ProcessStartInfo("aria2c.exe", cmdline);
                    proc.StartInfo.UseShellExecute = false;
                    proc.Start();
                    proc.WaitForExit();
                    returnCode = proc.ExitCode;*/

                    /*foreach (var file2 in update.Xml.Files.File.OrderBy(x => ulong.Parse(x.Size)))
                    {
                        bool regen = false;

                        string filename = file2.FileName;

                        if (payloadItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text))
                        {
                            filename = payloadItems.First(x => x.PayloadHash == file2.AdditionalDigest.Text).Path;
                        }

                        if (filename.Contains("Diff", StringComparison.InvariantCultureIgnoreCase) ||
                            filename.Contains("Baseless", StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        string filenameonly = Path.GetFileName(filename);
                        string filenameonlywithoutextension = Path.GetFileNameWithoutExtension(filename);
                        string extension = filenameonly.Replace(filenameonlywithoutextension, "");
                        string outputPath = filename.Replace(filenameonly, "");

                        // Download the file
                        using (var client = new WebClient())
                        {
                            // Prevents WebClient from hanging for like 5 seconds to get IE proxy info
                            client.Proxy = null;

                            // Block that detects if our file already exists and appends (int) next to the file name
                            string padding = "";
                            int counter = 1;
                            bool fexists = true;

                            bool skip = false;

                            while (fexists)
                            {
                                fexists = File.Exists(OutputFolder + @"\" + outputPath + filenameonlywithoutextension + padding + extension);
                                if (fexists)
                                {
                                    if (new FileInfo(OutputFolder + @"\" + outputPath + filenameonlywithoutextension + padding + extension).Length == long.Parse(file2.Size))
                                    {
                                        skip = true;
                                        break;
                                    }

                                    counter++;
                                    padding = " (" + counter + ")";
                                }
                            }

                            if (skip)
                                continue;

                            string newfilename = outputPath + filenameonlywithoutextension + padding + extension;

                            var dir = OutputFolder + @"\" + outputPath;
                            if (!Directory.Exists(dir))
                                Directory.CreateDirectory(dir);

                            bool done = false;

                            while (!done)
                            {
                                try
                                {
                                    done = true;

                                    Logging.Log("Downloading " + newfilename + "...");

                                    new Thread(() =>
                                    {
                                        Thread.CurrentThread.IsBackground = true;
                                        client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs args) =>
                                        {
                                            Console.Write($"\rProgres: {args.ProgressPercentage}%...");
                                        };
                                    }).Start();

                                    client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs args) =>
                                    {
                                        lock (args.UserState)
                                        {
                                            if (new FileInfo(OutputFolder + @"\" + newfilename).Length != long.Parse(file2.Size))
                                            {
                                                File.Delete(OutputFolder + @"\" + newfilename);
                                                returnCode = 1;
                                                regen = true;
                                            }
                                            else
                                            {
                                                File.SetLastWriteTimeUtc(OutputFolder + @"\" + newfilename, DateTime.Parse(file2.Modified));
                                            }

                                            //releases blocked thread
                                            Monitor.Pulse(args.UserState);
                                            Console.Write("\rProgres: 100%...");
                                            Console.WriteLine();
                                        }
                                    };

                                    var syncObject = new Object();
                                    lock (syncObject)
                                    {
                                        var url = urls.First(x => x.FileDigest == file2.Digest);
                                        client.DownloadFileAsync(new Uri(url.Url), OutputFolder + @"\" + newfilename, syncObject);
                                        //This would block the thread until download completes
                                        Monitor.Wait(syncObject);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                    done = false;
                                }
                            }
                        }

                        if (regen)
                            break;
                    }*/

                    returnCode = 0;

                    foreach (var file2 in update.Xml.Files.File.OrderBy(x => ulong.Parse(x.Size)))
                    {
                        string filename = file2.FileName;
                        //Logging.Log(file2.AdditionalDigest.Algorithm + "=" + file2.AdditionalDigest.Text + " - " + file2.DigestAlgorithm + "=" + file2.Digest);
                        if (payloadItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest))
                        {
                            var payload = payloadItems.First(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest);
                            filename = payload.Path;

                            if (payload.PayloadType.Equals("ExpressCab", StringComparison.InvariantCultureIgnoreCase))
                            {
                                // This is a diff cab, skip it
                                continue;
                            }
                        }

                        if (filename.Contains("Diff", StringComparison.InvariantCultureIgnoreCase) ||
                            filename.Contains("Baseless", StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        if (downloadedFiles.Any(x => x.Equals(filename, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            continue;
                        }

                        string filenameonly = Path.GetFileName(filename);
                        string filenameonlywithoutextension = Path.GetFileNameWithoutExtension(filename);
                        string extension = filenameonly.Replace(filenameonlywithoutextension, "");
                        string outputPath = filename.Replace(filenameonly, "");

                        Logging.Log("Checking " + Path.Combine(outputPath, filenameonly) + "...");

                        if (File.Exists(Path.Combine(OutputFolder, outputPath, filenameonly)))
                        {
                            Logging.Log("File " + Path.Combine(outputPath, filenameonly) + " already exists. Verifying if it's matching expectations.");
                            var expectedHash = Convert.FromBase64String(file2.Digest);

                            if (file2.DigestAlgorithm.Equals("sha1", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Logging.Log("Computing SHA1 hash...");
                                using (SHA1 SHA1 = SHA1Managed.Create())
                                {
                                    byte[] hash;
                                    using (FileStream fileStream = File.OpenRead(Path.Combine(OutputFolder, outputPath, filenameonly)))
                                        hash = SHA1.ComputeHash(fileStream);
                                    if (StructuralComparisons.StructuralEqualityComparer.Equals(expectedHash, hash))
                                    {
                                        Logging.Log("Hash matches! Skipping file");
                                        continue;
                                    }
                                    else
                                    {
                                        Logging.Log("Hash does not match! Deleting and redownloading the file.");
                                        File.Delete(Path.Combine(OutputFolder, outputPath, filenameonly));
                                        Logging.Log("File deleted");
                                    }
                                }
                            }
                            else if (file2.DigestAlgorithm.Equals("sha256", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Logging.Log("Computing SHA256 hash...");

                                using (SHA256 SHA256 = SHA256Managed.Create())
                                {
                                    byte[] hash;
                                    using (FileStream fileStream = File.OpenRead(Path.Combine(OutputFolder, outputPath, filenameonly)))
                                        hash = SHA256.ComputeHash(fileStream);
                                    if (StructuralComparisons.StructuralEqualityComparer.Equals(expectedHash, hash))
                                    {
                                        Logging.Log("Hash matches! Skipping file");
                                        continue;
                                    }
                                    else
                                    {
                                        Logging.Log("Hash does not match! Deleting and redownloading the file.");
                                        File.Delete(Path.Combine(OutputFolder, outputPath, filenameonly));
                                        Logging.Log("File deleted");
                                    }
                                }
                            }
                        }

                        var dlclient = new Neon.Downloader.DownloaderClient();

                        DateTime startTime = DateTime.Now;

                        bool end = false;

                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;

                            dlclient.OnDownloading += (Neon.Downloader.DownloadMetric metric) =>
                            {
                                Logging.Log($"{GetDismLikeProgBar((int)metric.Progress)} {metric.TimeRemaining:hh\\:mm\\:ss\\.f} {metric.Speed}B/s", Logging.LoggingLevel.Information, false);
                                if (metric.IsComplete)
                                {
                                    Logging.Log("");
                                    end = true;
                                }
                            };
                            dlclient.DownloadCompleted += (Neon.Downloader.DownloadMetric metric, Stream stream) =>
                            {
                                
                            };
                            dlclient.OnError += (Exception ex) =>
                            {
                                if (ex.InnerException != null && ex.InnerException.GetType() == typeof(NullReferenceException))
                                {
                                    // ignore
                                    return;
                                }

                                Logging.Log(ex.ToString(), Logging.LoggingLevel.Error);
                                if (ex.InnerException != null)
                                    Logging.Log(ex.InnerException.ToString(), Logging.LoggingLevel.Error);
                                returnCode = -1;
                                Logging.Log("");
                                end = true;
                            };
                        }).Start();

                        var url = urls.First(x => x.FileDigest == file2.Digest);

                        //CancellationToken ct = new CancellationToken();
                        //await dlclient.DownloadToFileAsync(url.Url, Path.Combine(OutputFolder, outputPath), filenameonly, ct);
                        //dlclient.DownloadToFile(new Uri(url.Url), filenameonly, Path.Combine(OutputFolder, outputPath));

                        dlclient.DownloadToFile(new Uri(url.Url), filenameonly, Path.Combine(OutputFolder, outputPath));

                        while (!end) { Thread.Sleep(200); }

                        if (returnCode != 0)
                            break;
                    }
                }
                while (returnCode != 0);
            }
            Logging.Log("Completed.");
            if (Debugger.IsAttached)
                Console.ReadLine();
        }

        private static string GetDismLikeProgBar(Int32 perc)
        {
            Int32 eqsLength = (Int32)((Double)perc / 100 * 55);
            string bases = new string('=', eqsLength) + new string(' ', 55 - eqsLength);
            bases = bases.Insert(28, perc + "%");
            if (perc == 100)
                bases = bases.Substring(1);
            else if (perc < 10)
                bases = bases.Insert(28, " ");
            return "[" + bases + "]";
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
                //Logging.Log("New Metadata");
                // This is the new metadata format where all metadata is in a single cab

                if (string.IsNullOrEmpty(update.CachedMetadata))
                {
                    string metadataUrl = await FE3Handler.GetFileUrl(update, metadataCabs[0].Digest, null, update.CTAC);
                    string metadataCabTemp = Path.GetTempFileName();

                    // Download the file
                    WebClient client = new WebClient();
                    await client.DownloadFileTaskAsync(new Uri(metadataUrl), metadataCabTemp);

                    //Logging.Log("Metadata temp: " + metadataCabTemp);

                    update.CachedMetadata = metadataCabTemp;
                }

                using (var cabinet = new CabinetHandler(File.OpenRead(update.CachedMetadata)))
                {
                    foreach (var file in cabinet.Files)
                    {
                        //Logging.Log("File in metadatacabinet: " + file);

                        using (CabinetHandler cabinet2 = new CabinetHandler(cabinet.OpenFile(file)))
                        {
                            string xmlfile = cabinet2.Files.First();
                            //Logging.Log("Xml file: " + xmlfile);

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
                //Logging.Log("Old Metadata");
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

            /*var xmlSerializer = new XmlSerializer(typeof(List<CompDBXmlClass.CompDB>));

            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
                {
                    Indent = true,
                    OmitXmlDeclaration = false,
                    NewLineOnAttributes = false,
                    Encoding = Encoding.UTF8
                }))
                {
                    xmlSerializer.Serialize(xmlWriter, neutralCompDB);

                    File.WriteAllText("D:\\testdl\\compdbs.xml", stringWriter.ToString());

                }
            }*/

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
