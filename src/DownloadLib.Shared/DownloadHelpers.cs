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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WindowsUpdateLib;

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
                return payload.Path.Replace('\\', Path.DirectorySeparatorChar);
            }
            else if (!payloadItems.Any() && filename.Contains("_") && !filename.StartsWith("_") && (!filename.Contains('-') || filename.IndexOf('-') > filename.IndexOf('_')))
            {
                filename = filename.Substring(0, filename.IndexOf('_')) + Path.DirectorySeparatorChar + filename[(filename.IndexOf('_') + 1)..];
                return filename.TrimStart(Path.DirectorySeparatorChar);
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

            return !filename.Contains("Diff", StringComparison.InvariantCultureIgnoreCase) &&
!filename.Contains("Baseless", StringComparison.InvariantCultureIgnoreCase);
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
            return bannedItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest);
        }

        public static async Task<string> ProcessUpdateAsync(UpdateData update, string pOutputFolder, MachineType MachineType, IProgress<GeneralDownloadProgress> generalDownloadProgress, string Language = "", string Edition = "", bool WriteMetadata = true, bool UseAutomaticDownloadFolder = true)
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

            HashSet<CompDBXmlClass.CompDB> compDBs = await update.GetCompDBsAsync().ConfigureAwait(false);

            await Task.WhenAll(
                Task.Run(async () => buildstr = await update.GetBuildStringAsync().ConfigureAwait(false)),
                Task.Run(async () => languages = await update.GetAvailableLanguagesAsync().ConfigureAwait(false))).ConfigureAwait(false);

            if (buildstr == null)
            {
                buildstr = "";
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

            string name = $"{buildstr.Replace(" ", ".").Replace("(", "").Replace(")", "")}_{MachineType.ToString().ToLower()}fre_{update.Xml.UpdateIdentity.UpdateID.Split("-").Last()}";
            Regex illegalCharacters = new(@"[\\/:*?""<>|]");
            name = illegalCharacters.Replace(name, "");
            string OutputFolder = pOutputFolder;
            if (UseAutomaticDownloadFolder)
            {
                OutputFolder = Path.Combine(pOutputFolder, name);
            }

            if (compDBs != null)
            {
                foreach (CompDBXmlClass.CompDB cdb in compDBs)
                {
                    bool IsDiff = cdb.Tags?.Tag?.Any(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase) && (x.Value.Equals("Diff", StringComparison.InvariantCultureIgnoreCase) || x.Value.Equals("Baseless", StringComparison.InvariantCultureIgnoreCase))) == true;

                    if (IsDiff)
                    {
                        foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                        {
                            foreach (CompDBXmlClass.PayloadItem item in pkg.Payload.PayloadItem)
                            {
                                bannedPayloadItems.Add(item);
                            }
                        }
                    }
                    else
                    {
                        if (getSpecific || getSpecificLanguageOnly)
                        {
                            bool hasLang = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase)) == true;
                            bool hasEdition = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)) == true;

                            bool editionMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Edition, StringComparison.InvariantCultureIgnoreCase)) == true;
                            bool langMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Language, StringComparison.InvariantCultureIgnoreCase)) == true;

                            if (cdb.Tags != null && ((getSpecificLanguageOnly && langMatching) || (getSpecific && editionMatching && langMatching) || (!hasLang && !hasEdition && cdb.Tags?.Type.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase) == false)))
                            {
                                specificCompDBs.Add(cdb);

                                foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                                {
                                    foreach (CompDBXmlClass.PayloadItem item in pkg.Payload.PayloadItem)
                                    {
                                        if (item.PayloadType.Equals("Diff", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            bannedPayloadItems.Add(item);
                                        }
                                        else
                                        {
                                            payloadItems.Add(item);
                                        }
                                    }
                                }
                            }
                            else if (cdb.Tags != null && 
                                (cdb.Tags.Type.Equals("Language", StringComparison.InvariantCultureIgnoreCase) ||
                                cdb.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) ||
                                (!getSpecificLanguageOnly && cdb.Tags.Type.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase)) ||
                                (!getSpecificLanguageOnly && hasLang) || (getSpecific && hasLang && hasEdition)))
                            {
                                foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                                {
                                    foreach (CompDBXmlClass.PayloadItem item in pkg.Payload.PayloadItem)
                                    {
                                        bannedPayloadItems.Add(item);
                                    }
                                }
                            }
                            else
                            {
                                foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                                {
                                    if (pkg.Payload == null)
                                        continue;

                                    foreach (CompDBXmlClass.PayloadItem item in pkg.Payload.PayloadItem)
                                    {
                                        if (item.PayloadType.Equals("Diff", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            bannedPayloadItems.Add(item);
                                        }
                                        else
                                        {
                                            payloadItems.Add(item);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (CompDBXmlClass.Package pkg in cdb.Packages.Package)
                            {
                                if (pkg.Payload == null)
                                    continue;

                                foreach (CompDBXmlClass.PayloadItem item in pkg.Payload.PayloadItem)
                                {
                                    if (item.PayloadType.Equals("Diff", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        bannedPayloadItems.Add(item);
                                    }
                                    else
                                    {
                                        payloadItems.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (getSpecific || getSpecificLanguageOnly)
            {
                if (specificCompDBs.Count == 0)
                {
                    throw new Exception("No update metadata matched the specified criteria");
                }
                else
                {
                    foreach (CompDBXmlClass.CompDB specificCompDB in specificCompDBs)
                    {
                        foreach (CompDBXmlClass.Package pkg in specificCompDB.Packages.Package)
                        {
                            foreach (CompDBXmlClass.PayloadItem item in pkg.Payload.PayloadItem)
                            {
                                bannedPayloadItems.RemoveWhere(x => x.PayloadHash == item.PayloadHash);
                            }
                        }
                    }
                }
            }

            do
            {
                IEnumerable<FileExchangeV3FileDownloadInformation> fileUrls = await FE3Handler.GetFileUrls(update).ConfigureAwait(false);

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

                using HttpDownloader helperDl = new(OutputFolder);
                filesToDownload = update.Xml.Files.File.AsParallel().Where(x => !IsFileBanned(x, bannedPayloadItems));

                returnCode = 0;

                IEnumerable<(CExtendedUpdateInfoXml.File, FileExchangeV3FileDownloadInformation)> boundList = filesToDownload
                    .AsParallel()
                    .Select(x => (x, fileUrls.First(y => y.Digest == x.Digest)))
                    //.Where(x => UpdateUtils.ShouldFileGetDownloaded(x.x, payloadItems))
                    .OrderBy(x => x.Item2.ExpirationDate);

                IEnumerable<UUPFile> fileList = boundList.Select(boundFile =>
                {
                    return new UUPFile(
                        boundFile.Item2,
                        UpdateUtils.GetFilenameForCEUIFile(boundFile.Item1, payloadItems),
                        long.Parse(boundFile.Item1.Size),
                        boundFile.Item1.AdditionalDigest.Text,
                        boundFile.Item1.AdditionalDigest.Algorithm);
                });

                returnCode = await helperDl.DownloadAsync(fileList.ToList(), generalDownloadProgress).ConfigureAwait(false) ? 0 : -1;

                if (returnCode != 0)
                {
                    continue;
                }
            }
            while (returnCode != 0);

            return OutputFolder;
        }
    }
}
