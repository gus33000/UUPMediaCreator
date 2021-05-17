using CompDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WindowsUpdateLib;
using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace DownloadLib
{
    public static class UpdateUtils
    {
        public static string GetFilenameForCEUIFile(CExtendedUpdateInfoXml.File file2, IEnumerable<CompDBXmlClass.PayloadItem> payloadItems)
        {
            string filename = file2.FileName.Replace('\\', Path.DirectorySeparatorChar);
            if (payloadItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest))
            {
                CompDBXmlClass.PayloadItem payload = payloadItems.First(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest);
                filename = payload.Path.Replace('\\', Path.DirectorySeparatorChar);
            }
            else if (payloadItems.Count() == 0 && filename.Contains("_") && !filename.StartsWith("_") && (filename.IndexOf('-') == -1 || filename.IndexOf('-') > filename.IndexOf('_')))
            {
                filename = filename.Substring(0, filename.IndexOf('_')) + Path.DirectorySeparatorChar + filename.Substring(filename.IndexOf('_') + 1);
                filename = filename.TrimStart(Path.DirectorySeparatorChar);
            }
            return filename;
        }

        public static bool ShouldFileGetDownloaded(CExtendedUpdateInfoXml.File file2, IEnumerable<CompDBXmlClass.PayloadItem> payloadItems)
        {
            string filename = file2.FileName.Replace('\\', Path.DirectorySeparatorChar);
            if (payloadItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest))
            {
                CompDBXmlClass.PayloadItem payload = payloadItems.First(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest);
                filename = payload.Path.Replace('\\', Path.DirectorySeparatorChar);

                if (payload.PayloadType.Equals("ExpressCab", StringComparison.InvariantCultureIgnoreCase))
                {
                    // This is a diff cab, skip it
                    return false;
                }
            }

            if (filename.Contains("Diff", StringComparison.InvariantCultureIgnoreCase) ||
                filename.Contains("Baseless", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public static UpdateData TrimDeltasFromUpdateData(UpdateData update)
        {
            update.Xml.Files.File = update.Xml.Files.File.Where(x => !x.FileName.Replace('\\', Path.DirectorySeparatorChar).EndsWith(".psf", StringComparison.InvariantCultureIgnoreCase)
            && !x.FileName.Replace('\\', Path.DirectorySeparatorChar).StartsWith("Diff", StringComparison.InvariantCultureIgnoreCase)
             && !x.FileName.Replace('\\', Path.DirectorySeparatorChar).StartsWith("Baseless", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            return update;
        }


        private static bool IsFileBanned(CExtendedUpdateInfoXml.File file2, IEnumerable<CompDBXmlClass.PayloadItem> bannedItems)
        {
            if (bannedItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest))
            {
                return true;
            }

            return false;
        }

        public static async Task<string> ProcessUpdateAsync(UpdateData update, string pOutputFolder, MachineType MachineType, IProgress<GeneralDownloadProgress> generalDownloadProgress, string Language = "", string Edition = "", bool WriteMetadata = true)
        {
            HashSet<CompDBXmlClass.PayloadItem> payloadItems = new HashSet<CompDBXmlClass.PayloadItem>();
            HashSet<CompDBXmlClass.PayloadItem> bannedPayloadItems = new HashSet<CompDBXmlClass.PayloadItem>();
            HashSet<CompDBXmlClass.CompDB> specificCompDBs = new HashSet<CompDBXmlClass.CompDB>();

            string buildstr = "";
            IEnumerable<string> languages = null;

            int returnCode = 0;
            IEnumerable<CExtendedUpdateInfoXml.File> filesToDownload = null;

            bool getSpecific = !string.IsNullOrEmpty(Language) && !string.IsNullOrEmpty(Edition);
            bool getSpecificLanguageOnly = !string.IsNullOrEmpty(Language) && string.IsNullOrEmpty(Edition);

            var compDBs = await update.GetCompDBsAsync();

            await Task.WhenAll(
                Task.Run(async () => buildstr = await update.GetBuildStringAsync()),
                Task.Run(async () => languages = await update.GetAvailableLanguagesAsync()));

            if (buildstr == null)
                buildstr = "";

            if (string.IsNullOrEmpty(buildstr) && update.Xml.LocalizedProperties.Title.Contains("(UUP-CTv2)"))
            {
                string unformattedBase = update.Xml.LocalizedProperties.Title.Split(" ")[0];
                buildstr = $"10.0.{unformattedBase.Split(".")[0]}.{unformattedBase.Split(".")[1]} ({unformattedBase.Split(".")[2]}.{unformattedBase.Split(".")[3]})";
            }
            else if (string.IsNullOrEmpty(buildstr))
            {
                buildstr = update.Xml.LocalizedProperties.Title;
            }

            string name = $"{buildstr.Replace(" ", ".").Replace("(", "").Replace(")", "")}_{MachineType.ToString().ToLower()}fre_{update.Xml.UpdateIdentity.UpdateID.Split("-").Last()}";
            Regex illegalCharacters = new Regex(@"[\\/:*?""<>|]");
            name = illegalCharacters.Replace(name, "");
            string OutputFolder = Path.Combine(pOutputFolder, name);

            if (compDBs != null)
            {
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
                    throw new Exception("No update metadata matched the specified criteria");
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
                IEnumerable<FileExchangeV3FileDownloadInformation> fileUrls = await FE3Handler.GetFileUrls(update);

                if (fileUrls == null)
                {
                    throw new Exception("Getting file urls failed.");
                }

                if (!Directory.Exists(OutputFolder))
                {
                    Directory.CreateDirectory(OutputFolder);

                    string tmpname = update.Xml.LocalizedProperties.Title + " (" + MachineType.ToString() + ").uupmcreplay";
                    illegalCharacters = new Regex(@"[\\/:*?""<>|]");
                    tmpname = illegalCharacters.Replace(tmpname, "");
                    string filename = Path.Combine(OutputFolder, tmpname);
                    if (WriteMetadata && !File.Exists(filename))
                    {
                        File.WriteAllText(filename, JsonSerializer.Serialize(update, new JsonSerializerOptions() { WriteIndented = true }));
                    }
                }

                using (HttpDownloader helperDl = new HttpDownloader(OutputFolder))
                {
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

                    returnCode = await helperDl.DownloadAsync(fileList.ToList(), generalDownloadProgress) ? 0 : -1;

                    if (returnCode != 0)
                    {
                        continue;
                    }
                }
            }
            while (returnCode != 0);

            return OutputFolder;
        }
    }
}
