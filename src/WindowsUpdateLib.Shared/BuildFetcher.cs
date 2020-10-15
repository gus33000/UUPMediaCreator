using Microsoft.Cabinet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WindowsUpdateLib
{
    public static class StringExtensions
    {
        public static bool Contains(this String str, String substring,
                                    StringComparison comp)
        {
            if (substring == null)
                throw new ArgumentNullException("substring",
                                             "substring cannot be null.");
            else if (!Enum.IsDefined(typeof(StringComparison), comp))
                throw new ArgumentException("comp is not a member of StringComparison",
                                         "comp");

            return str.IndexOf(substring, comp) >= 0;
        }

        public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparisonType)
        {
            int startIndex = 0;
            while (true)
            {
                startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);
                if (startIndex == -1)
                    break;

                originalString = originalString.Substring(0, startIndex) + newValue + originalString.Substring(startIndex + oldValue.Length);

                startIndex += newValue.Length;
            }

            return originalString;
        }
    }

    public class BuildFetcher
    {
        public class AvailableBuild
        {
            public UpdateData UpdateData { get; set; }
            public string Title { get; set; }
            public string BuildString { get; set; }
            public string Description { get; set; }
            public string Created { get; set; }
        }

        public class AvailableBuildLanguages
        {
            public string Title { get; set; }
            public string LanguageCode { get; set; }
            public Uri FlagUri { get; set; }
        }

        public class AvailableEdition
        {
            public string Edition { get; set; }
        }

        public static async Task<AvailableBuild[]> GetAvailableBuildsAsync(MachineType machineType)
        {
            List<AvailableBuild> availableBuilds = new List<AvailableBuild>();

            var updates = await GetUpdates(machineType);

            foreach (var update in updates)
            {
                AvailableBuild availableBuild = new AvailableBuild()
                {
                    Title = update.Xml.LocalizedProperties.Title,
                    Description = update.Xml.LocalizedProperties.Description,
                    UpdateData = update,
                    Created = update.UpdateInfo.Deployment.LastChangeTime
                };

                string BuildStr = await GetBuildStringFromUpdate(update);
                if (!string.IsNullOrEmpty(BuildStr))
                {
                    availableBuild.Title = BuildStr;
                    availableBuild.BuildString = BuildStr;
                }

                availableBuilds.Add(availableBuild);
            }

            return availableBuilds.ToArray();
        }

        public static async Task<AvailableBuildLanguages[]> GetAvailableBuildLanguagesAsync(UpdateData UpdateData)
        {
            List<AvailableBuildLanguages> availableBuildLanguages = new List<AvailableBuildLanguages>();

            List<CExtendedUpdateInfoXml.File> metadataCabs = new List<CExtendedUpdateInfoXml.File>();

            foreach (var file in UpdateData.Xml.Files.File)
            {
                if (file.PatchingType.Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                {
                    metadataCabs.Add(file);
                }
            }

            if (metadataCabs.Count == 0)
            {
                goto exit;
            }

            if (metadataCabs.Count == 1 && metadataCabs[0].FileName.Contains("metadata", StringComparison.InvariantCultureIgnoreCase))
            {
                // This is the new metadata format where all metadata is in a single cab

                if (string.IsNullOrEmpty(UpdateData.CachedMetadata))
                {
                    string metadataUrl = await FE3Handler.GetFileUrl(UpdateData, metadataCabs[0].Digest, null, UpdateData.CTAC);
                    string metadataCabTemp = Path.GetTempFileName();

                    // Download the file
                    WebClient client = new WebClient();
                    await client.DownloadFileTaskAsync(new Uri(metadataUrl), metadataCabTemp);

                    UpdateData.CachedMetadata = metadataCabTemp;
                }

                using (var cabinet = new CabinetHandler(File.OpenRead(UpdateData.CachedMetadata)))
                {
                    foreach (var file in cabinet.Files.Where(x =>
                        x.Contains("desktoptargetcompdb_", StringComparison.InvariantCultureIgnoreCase) &&
                        x.Count(y => y == '_') == 2 &&
                        !x.Contains("tools", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var lang = file.Split('_').Last().Replace(".xml.cab", "", StringComparison.InvariantCultureIgnoreCase);
                        if (lang.Equals("neutral", StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        if (availableBuildLanguages.Any(x => x.LanguageCode == lang))
                            continue;

                        var boundlanguageobject = CultureInfo.GetCultureInfoByIetfLanguageTag(lang);

                        availableBuildLanguages.Add(new AvailableBuildLanguages() { LanguageCode = lang, Title = boundlanguageobject.DisplayName, FlagUri = new Uri($"ms-appx:///Assets/Flags/{lang.Split('-').Last()}.png") });
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
                    if (lang.Equals("neutral", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    if (availableBuildLanguages.Any(x => x.LanguageCode == lang))
                        continue;

                    var boundlanguageobject = CultureInfo.GetCultureInfoByIetfLanguageTag(lang);

                    availableBuildLanguages.Add(new AvailableBuildLanguages() { LanguageCode = lang, Title = boundlanguageobject.DisplayName, FlagUri = new Uri($"ms-appx:///Assets/Flags/{lang.Split('-').Last()}.png") });
                }
            }

            availableBuildLanguages.Sort((x, y) => x.Title.CompareTo(y.Title));

            exit:
            return availableBuildLanguages.ToArray();
        }

        public static async Task<AvailableEdition[]> GetAvailableEditions(UpdateData UpdateData, string languagecode)
        {
            List<AvailableEdition> availableEditions = new List<AvailableEdition>();

            List<CExtendedUpdateInfoXml.File> metadataCabs = new List<CExtendedUpdateInfoXml.File>();

            foreach (var file in UpdateData.Xml.Files.File)
            {
                if (file.PatchingType.Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                {
                    metadataCabs.Add(file);
                }
            }

            if (metadataCabs.Count == 0)
            {
                goto exit;
            }

            if (metadataCabs.Count == 1 && metadataCabs[0].FileName.Contains("metadata", StringComparison.InvariantCultureIgnoreCase))
            {
                // This is the new metadata format where all metadata is in a single cab

                if (string.IsNullOrEmpty(UpdateData.CachedMetadata))
                {
                    string metadataUrl = await FE3Handler.GetFileUrl(UpdateData, metadataCabs[0].Digest, null, UpdateData.CTAC);
                    string metadataCabTemp = Path.GetTempFileName();

                    // Download the file
                    WebClient client = new WebClient();
                    await client.DownloadFileTaskAsync(new Uri(metadataUrl), metadataCabTemp);

                    UpdateData.CachedMetadata = metadataCabTemp;
                }

                using (var cabinet = new CabinetHandler(File.OpenRead(UpdateData.CachedMetadata)))
                {
                    IEnumerable<string> potentialFiles = cabinet.Files.Where(x =>
                        x.ToLower().Contains($"desktoptargetcompdb_") &&
                        x.ToLower().Contains($"_{languagecode}") &&
                        !x.ToLower().Contains("lxp") &&
                        !x.ToLower().Contains($"desktoptargetcompdb_{languagecode}"));

                    foreach (var file in potentialFiles)
                    {
                        var edition = file.Split('_').Reverse().Skip(1).First();

                        if (availableEditions.Any(x => x.Edition == edition))
                            continue;

                        availableEditions.Add(new AvailableEdition() { Edition = edition });
                    }
                }
            }
            else
            {
                IEnumerable<string> potentialFiles = metadataCabs.Select(x => x.FileName).Where(x =>
                    x.ToLower().Contains($"desktoptargetcompdb_") &&
                    x.ToLower().Contains($"_{languagecode}") &&
                    !x.ToLower().Contains("lxp") &&
                    !x.ToLower().Contains($"desktoptargetcompdb_{languagecode}"));

                // This is the old format, each cab is a file in WU
                foreach (var file in potentialFiles)
                {
                    var edition = file.Split('_').Reverse().Skip(1).First();

                    if (availableEditions.Any(x => x.Edition == edition))
                        continue;

                    availableEditions.Add(new AvailableEdition() { Edition = edition });
                }
            }

            availableEditions.Sort((x, y) => x.Edition.CompareTo(y.Edition));

            exit:
            return availableEditions.ToArray();
        }

        private static UpdateData TrimDeltasFromUpdateData(UpdateData update)
        {
            update.Xml.Files.File = update.Xml.Files.File.Where(x => !x.FileName.EndsWith(".psf", StringComparison.InvariantCultureIgnoreCase)
            && !x.FileName.StartsWith("Diff", StringComparison.InvariantCultureIgnoreCase)
             && !x.FileName.StartsWith("Baseless", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            return update;
        }

        private static void AddUpdatesIfNotPresentAlready(List<UpdateData> updates, UpdateData[] uncleanedData)
        {
            var data = uncleanedData.Select(x => TrimDeltasFromUpdateData(x)).ToArray();

            foreach (var update in data)
            {
                if (updates.Any(x => x.Xml.Files.File.Count() == update.Xml.Files.File.Count()))
                {
                    var potentialDupes = updates.Where(x => x.Xml.Files.File.Count() == update.Xml.Files.File.Count());

                    CExtendedUpdateInfoXml.File metadataCab = null;

                    foreach (var file in update.Xml.Files.File)
                    {
                        if (file.PatchingType.Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                        {
                            metadataCab = file;
                            break;
                        }
                    }

                    if (metadataCab == null)
                    {
                        continue;
                    }

                    bool exists = false;

                    foreach (var potentialDupe in potentialDupes)
                    {
                        CExtendedUpdateInfoXml.File metadataCab2 = null;

                        foreach (var file in potentialDupe.Xml.Files.File)
                        {
                            if (file.PatchingType.Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                            {
                                metadataCab2 = file;
                                break;
                            }
                        }

                        if (metadataCab2 == null)
                        {
                            continue;
                        }

                        if (metadataCab.Digest == metadataCab2.Digest)
                        {
                            exists = true;
                            continue;
                        }
                    }

                    if (!exists)
                        updates.Add(update);
                }
                else
                {
                    updates.Add(update);
                }
            }
        }

        private static async Task<List<UpdateData>> GetUpdates(MachineType MachineType)
        {
            List<UpdateData> updates = new List<UpdateData>();
            CTAC ctac;
            UpdateData[] data;

            ctac = FE3Handler.BuildCTAC(OSSkuId.PPIPro, "10.0.15063.534", MachineType, "WIS", "", "CB", "rs2_release", false);
            data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            AddUpdatesIfNotPresentAlready(updates, data);

            ctac = FE3Handler.BuildCTAC(OSSkuId.PPIPro, "10.0.15063.534", MachineType, "WIF", "", "CB", "rs2_release", false);
            data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            AddUpdatesIfNotPresentAlready(updates, data);

            ctac = FE3Handler.BuildCTAC(OSSkuId.Professional, "10.0.16299.15", MachineType, "Retail", "", "CB", "rs3_release", true);
            data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            AddUpdatesIfNotPresentAlready(updates, data);

            ctac = FE3Handler.BuildCTAC(OSSkuId.Professional, "10.0.17134.1", MachineType, "Retail", "", "CB", "rs4_release", true);
            data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            AddUpdatesIfNotPresentAlready(updates, data);

            ctac = FE3Handler.BuildCTAC(OSSkuId.Professional, "10.0.17763.1217", MachineType, "Retail", "", "CB", "rs5_release", true);
            data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            AddUpdatesIfNotPresentAlready(updates, data);

            ctac = FE3Handler.BuildCTAC(OSSkuId.Professional, "10.0.18362.836", MachineType, "Retail", "", "CB", "19h1_release", true);
            data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            AddUpdatesIfNotPresentAlready(updates, data);

            ctac = FE3Handler.BuildCTAC(OSSkuId.Professional, "10.0.19041.200", MachineType, "Retail", "", "CB", "vb_release", true);
            data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            AddUpdatesIfNotPresentAlready(updates, data);

            ctac = FE3Handler.BuildCTAC(OSSkuId.Professional, "10.0.19041.200", MachineType, "External", "ReleasePreview", "CB", "19h1_release", false);
            data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            AddUpdatesIfNotPresentAlready(updates, data);

            ctac = FE3Handler.BuildCTAC(OSSkuId.Professional, "10.0.19041.200", MachineType, "External", "Beta", "CB", "19h1_release", false);
            data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            AddUpdatesIfNotPresentAlready(updates, data);

            ctac = FE3Handler.BuildCTAC(OSSkuId.Professional, "10.0.19041.200", MachineType, "External", "Dev", "CB", "19h1_release", false);
            data = await FE3Handler.GetUpdates(null, ctac, null, "ProductRelease");
            AddUpdatesIfNotPresentAlready(updates, data);

            return updates;
        }

        private async static Task<string> GetBuildStringFromUpdate(UpdateData update)
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

            File.Delete(deploymentCabTemp);
            return result;
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
    }
}