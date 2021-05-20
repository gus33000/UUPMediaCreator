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
using Cabinet;
using CompDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WindowsUpdateLib
{
    public static class UpdateDataExtensions
    {
        private static readonly WebClient client = new();

        public static async Task<string> DownloadFileFromDigestAsync(this UpdateData update, string Digest)
        {
            string metadataCabTemp = Path.GetTempFileName();
            await DownloadFileFromDigestAsync(update, Digest, metadataCabTemp).ConfigureAwait(false);
            return !File.Exists(metadataCabTemp) || new FileInfo(metadataCabTemp).Length == 0 ? null : metadataCabTemp;
        }

        public static async Task DownloadFileFromDigestAsync(this UpdateData update, string Digest, string Destination)
        {
            FileExchangeV3FileDownloadInformation fileDownloadInfo = await update.GetFileUrl(Digest).ConfigureAwait(false);
            if (fileDownloadInfo == null)
            {
                // TODO: notify of result
                return;
            }

            // Download the file
            await client.DownloadFileTaskAsync(new Uri(fileDownloadInfo.DownloadUrl), Destination).ConfigureAwait(false);

            if (fileDownloadInfo.IsEncrypted)
            {
                if (!await fileDownloadInfo.DecryptAsync(Destination, Destination + ".decrypted").ConfigureAwait(false))
                {
                    return;
                }

                File.Delete(Destination);
                File.Move(Destination + ".decrypted", Destination);
            }
        }

        public static async Task<FileExchangeV3FileDownloadInformation> GetFileUrl(this UpdateData update, string Digest)
        {
            return await FE3Handler.GetFileUrl(update, Digest).ConfigureAwait(false);
        }

        public async static Task<string> GetBuildStringAsync(this UpdateData update)
        {
            string result = null;

            try
            {
                CExtendedUpdateInfoXml.File deploymentCab = null;

                foreach (CExtendedUpdateInfoXml.File file in update.Xml.Files.File)
                {
                    if (file.FileName.Replace('\\', Path.DirectorySeparatorChar).EndsWith("deployment.cab", StringComparison.InvariantCultureIgnoreCase))
                    {
                        deploymentCab = file;
                        break;
                    }
                }

                if (deploymentCab == null)
                {
                    goto exit;
                }

                FileExchangeV3FileDownloadInformation fileDownloadInfo = await FE3Handler.GetFileUrl(update, deploymentCab.Digest).ConfigureAwait(false);
                if (fileDownloadInfo == null)
                {
                    goto exit;
                }

                string deploymentCabTemp = Path.GetTempFileName();
                await client.DownloadFileTaskAsync(new Uri(fileDownloadInfo.DownloadUrl), deploymentCabTemp).ConfigureAwait(false);

                if (fileDownloadInfo.IsEncrypted)
                {
                    if (!await fileDownloadInfo.DecryptAsync(deploymentCabTemp, deploymentCabTemp + ".decrypted").ConfigureAwait(false))
                    {
                        goto exit;
                    }

                    File.Delete(deploymentCabTemp);
                    File.Move(deploymentCabTemp + ".decrypted", deploymentCabTemp);
                }

                try
                {
                    byte[] buffer = CabinetExtractor.ExtractCabinetFile(deploymentCabTemp, "UpdateAgent.dll");
                    result = GetBuildStringFromUpdateAgent(buffer);
                }
                catch { }

                string reportedBuildNumberFromService = update.Xml.ExtendedProperties.ReleaseVersion.Split('.')[2];
                if (!string.IsNullOrEmpty(result) && result.Count(x => x == '.') >= 2)
                {
                    string[] elements = result.Split('.');
                    elements[2] = reportedBuildNumberFromService;
                    result = string.Join(".", elements);
                }

                File.Delete(deploymentCabTemp);
            }
            catch
            {
            }

        exit:

            // For some reason we couldn't get the build string, so attempt to get it from CompDB metadata instead
            // This is less reliable, but it is the best we can actually do.
            if (string.IsNullOrEmpty(result))
            {
                try
                {
                    HashSet<CompDBXmlClass.CompDB> compDBs = await update.GetCompDBsAsync().ConfigureAwait(false);
                    CompDBXmlClass.CompDB firstCompDB = compDBs.First();

                    // example:
                    // BuildInfo="co_release.21382.1.210511-1416" OSVersion="10.0.21382.1" TargetBuildInfo="co_release.21382.1.210511-1416" TargetOSVersion="10.0.21382.1"

                    string buildInfo = firstCompDB.TargetBuildInfo ?? firstCompDB.BuildInfo;
                    string osVersion = firstCompDB.TargetOSVersion ?? firstCompDB.OSVersion;

                    string[] splitBI = buildInfo.Split(".");

                    result = $"{osVersion} ({splitBI[0]}.{splitBI[3]})";
                }
                catch { }
            }

            return result;
        }

        private static string GetBuildStringFromUpdateAgent(byte[] updateAgentFile)
        {
            byte[] sign = new byte[] {
                0x46, 0x00, 0x69, 0x00, 0x6c, 0x00, 0x65, 0x00, 0x56, 0x00, 0x65, 0x00, 0x72,
                0x00, 0x73, 0x00, 0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            int fIndex = IndexOf(updateAgentFile, sign) + sign.Length;
            int lIndex = IndexOf(updateAgentFile, new byte[] { 0x00, 0x00, 0x00 }, fIndex) + 1;

            byte[] sliced = SliceByteArray(updateAgentFile, lIndex - fIndex, fIndex);

            return Encoding.Unicode.GetString(sliced);
        }

        private static byte[] SliceByteArray(byte[] source, int length, int offset)
        {
            byte[] destfoo = new byte[length];
            Array.Copy(source, offset, destfoo, 0, length);
            return destfoo;
        }

        private static int IndexOf(byte[] searchIn, byte[] searchFor, int offset = 0)
        {
            if ((searchIn != null) && (searchIn != null))
            {
                if (searchFor.Length > searchIn.Length)
                {
                    return 0;
                }

                for (int i = offset; i < searchIn.Length; i++)
                {
                    int startIndex = i;
                    bool match = true;
                    for (int j = 0; j < searchFor.Length; j++)
                    {
                        if (searchIn[startIndex] != searchFor[j])
                        {
                            match = false;
                            break;
                        }
                        else if (startIndex < searchIn.Length)
                        {
                            startIndex++;
                        }
                    }
                    if (match)
                    {
                        return startIndex - searchFor.Length;
                    }
                }
            }
            return -1;
        }

        public static async Task<IEnumerable<string>> GetAvailableLanguagesAsync(this UpdateData update)
        {
            return (await update.GetCompDBsAsync().ConfigureAwait(false)).GetAvailableLanguages();
        }

        private static async Task<HashSet<CompDBXmlClass.CompDB>> GetCompDBs(UpdateData update)
        {
            HashSet<CompDBXmlClass.CompDB> neutralCompDB = new();
            HashSet<CExtendedUpdateInfoXml.File> metadataCabs = new();

            foreach (CExtendedUpdateInfoXml.File file in update.Xml.Files.File)
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

            if (metadataCabs.Count == 1 && metadataCabs.First().FileName.Replace('\\', Path.DirectorySeparatorChar).Contains("metadata", StringComparison.InvariantCultureIgnoreCase))
            {
                // This is the new metadata format where all metadata is in a single cab

                if (string.IsNullOrEmpty(update.CachedMetadata))
                {
                    FileExchangeV3FileDownloadInformation fileDownloadInfo = await FE3Handler.GetFileUrl(update, metadataCabs.First().Digest).ConfigureAwait(false);
                    if (fileDownloadInfo == null)
                    {
                        return neutralCompDB;
                    }

                    string metadataCabTemp = Path.GetTempFileName();

                    try
                    {
                        // Download the file
                        await client.DownloadFileTaskAsync(new Uri(fileDownloadInfo.DownloadUrl), metadataCabTemp).ConfigureAwait(false);

                        if (fileDownloadInfo.IsEncrypted)
                        {
                            if (!await fileDownloadInfo.DecryptAsync(metadataCabTemp, metadataCabTemp + ".decrypted").ConfigureAwait(false))
                            {
                                return neutralCompDB;
                            }

                            metadataCabTemp += ".decrypted";
                        }

                        update.CachedMetadata = metadataCabTemp;
                    }
                    catch { }
                }

                try
                {
                    string tmp = Path.GetTempFileName();
                    File.Delete(tmp);
                    Directory.CreateDirectory(tmp);

                    CabinetExtractor.ExtractCabinet(update.CachedMetadata, tmp);

                    foreach (string file in Directory.EnumerateFiles(tmp, "*", SearchOption.AllDirectories))
                    {
                        byte[] xmlfile = CabinetExtractor.ExtractCabinetFile(file, CabinetExtractor.EnumCabinetFiles(file).First());
                        using Stream xmlstream = new MemoryStream(xmlfile);
                        neutralCompDB.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                    }
                }
                catch { }
            }
            else
            {
                // This is the old format, each cab is a file in WU
                foreach (CExtendedUpdateInfoXml.File file in metadataCabs)
                {
                    FileExchangeV3FileDownloadInformation fileDownloadInfo = await FE3Handler.GetFileUrl(update, file.Digest).ConfigureAwait(false);
                    if (fileDownloadInfo == null)
                    {
                        continue;
                    }

                    string metadataCabTemp = Path.GetTempFileName();

                    try
                    {
                        // Download the file
                        await client.DownloadFileTaskAsync(new Uri(fileDownloadInfo.DownloadUrl), metadataCabTemp).ConfigureAwait(false);

                        if (fileDownloadInfo.IsEncrypted)
                        {
                            if (!await fileDownloadInfo.DecryptAsync(metadataCabTemp, metadataCabTemp + ".decrypted").ConfigureAwait(false))
                            {
                                continue;
                            }

                            metadataCabTemp += ".decrypted";
                        }

                        update.CachedMetadata = metadataCabTemp;

                        byte[] xmlfile = CabinetExtractor.ExtractCabinetFile(update.CachedMetadata, CabinetExtractor.EnumCabinetFiles(update.CachedMetadata).First());
                        using Stream xmlstream = new MemoryStream(xmlfile);
                        neutralCompDB.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                    }
                    catch { }
                }
            }

            return neutralCompDB;
        }

        public static async Task<HashSet<CompDBXmlClass.CompDB>> GetCompDBsAsync(this UpdateData update)
        {
            return update.CompDBs ??= await GetCompDBs(update).ConfigureAwait(false);
        }
    }
}
