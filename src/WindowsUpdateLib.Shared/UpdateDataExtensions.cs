using CompDB;
using Microsoft.Cabinet;
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
        public static async Task<string> DownloadFileFromDigestAsync(this UpdateData update, string Digest)
        {
            string metadataCabTemp = Path.GetTempFileName();
            await DownloadFileFromDigestAsync(update, Digest, metadataCabTemp);
            return metadataCabTemp;
        }

        public static async Task DownloadFileFromDigestAsync(this UpdateData update, string Digest, string Destination)
        {
            var editionPackUrl = await update.GetFileUrl(Digest);

            // Download the file
            WebClient client = new WebClient();
            await client.DownloadFileTaskAsync(new Uri(editionPackUrl), Destination);
        }

        public static async Task<string> GetFileUrl(this UpdateData update, string Digest)
        {
            return await FE3Handler.GetFileUrl(update, Digest, null, update.CTAC);
        }

        public async static Task<string> GetBuildStringAsync(this UpdateData update)
        {
            try
            {

                CExtendedUpdateInfoXml.File deploymentCab = null;

                foreach (var file in update.Xml.Files.File)
                {
                    if (file.FileName.EndsWith("desktopdeployment.cab", StringComparison.InvariantCultureIgnoreCase))
                    {
                        deploymentCab = file;
                        break;
                    }
                }

                if (deploymentCab == null)
                {
                    return null;
                }

                string deploymentUrl = await FE3Handler.GetFileUrl(update, deploymentCab.Digest, null, update.CTAC);
                string deploymentCabTemp = Path.GetTempFileName();
                WebClient client = new WebClient();
                await client.DownloadFileTaskAsync(new Uri(deploymentUrl), deploymentCabTemp);

                string result = null;

                try
                {
                    using (var cabinet = new CabinetHandler(File.OpenRead(deploymentCabTemp)))
                    {
                        foreach (var file in cabinet.Files)
                        {
                            if (file.Equals("UpdateAgent.dll", StringComparison.InvariantCultureIgnoreCase))
                            {
                                byte[] buffer;
                                using (var dllstream = cabinet.OpenFile(file))
                                {
                                    buffer = new byte[dllstream.Length];
                                    await dllstream.ReadAsync(buffer, 0, (int)dllstream.Length);
                                }
                                result = GetBuildStringFromUpdateAgent(buffer);
                                break;
                            }
                        }
                    }
                }
                catch { }

                var reportedBuildNumberFromService = update.Xml.ExtendedProperties.ReleaseVersion.Split('.')[2];
                if (!string.IsNullOrEmpty(result) && result.Count(x => x == '.') >= 2)
                {
                    var elements = result.Split('.');
                    elements[2] = reportedBuildNumberFromService;
                    result = string.Join(".", elements);
                }

                File.Delete(deploymentCabTemp);
                return result;
            }
            catch
            {
                return null;
            }
        }

        private static string GetBuildStringFromUpdateAgent(byte[] updateAgentFile)
        {
            byte[] sign = new byte[] {
                0x46, 0x00, 0x69, 0x00, 0x6c, 0x00, 0x65, 0x00, 0x56, 0x00, 0x65, 0x00, 0x72,
                0x00, 0x73, 0x00, 0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            var fIndex = IndexOf(updateAgentFile, sign) + sign.Length;
            var lIndex = IndexOf(updateAgentFile, new byte[] { 0x00, 0x00, 0x00 }, fIndex) + 1;

            var sliced = SliceByteArray(updateAgentFile, lIndex - fIndex, fIndex);

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
                if (searchFor.Length > searchIn.Length) return 0;
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
                        return startIndex - searchFor.Length;
                }
            }
            return -1;
        }

        public static async Task<IEnumerable<string>> GetAvailableLanguagesAsync(this UpdateData update)
        {
            return (await update.GetCompDBsAsync()).GetAvailableLanguages();
        }

        private static async Task<HashSet<CompDBXmlClass.CompDB>> GetCompDBs(UpdateData update)
        {
            HashSet<CompDBXmlClass.CompDB> neutralCompDB = new HashSet<CompDBXmlClass.CompDB>();
            List<CExtendedUpdateInfoXml.File> metadataCabs = new List<CExtendedUpdateInfoXml.File>();

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

                using (CabinetHandler cabinet = new CabinetHandler(File.OpenRead(update.CachedMetadata)))
                {
                    foreach (string file in cabinet.Files)
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
                foreach (CExtendedUpdateInfoXml.File file in metadataCabs)
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

            return neutralCompDB;
        }

        public static async Task<HashSet<CompDBXmlClass.CompDB>> GetCompDBsAsync(this UpdateData update)
        {
            return await GetCompDBs(update);
        }
    }
}
