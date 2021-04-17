using Microsoft.Cabinet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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

                string BuildStr = await update.GetBuildStringAsync();
                if (!string.IsNullOrEmpty(BuildStr))
                {
                    availableBuild.Title = BuildStr;
                    availableBuild.BuildString = BuildStr;
                }

                availableBuilds.Add(availableBuild);
            }

            return availableBuilds.OrderBy(x => x.Title).ToArray();
        }

        public static async Task<AvailableBuildLanguages[]> GetAvailableBuildLanguagesAsync(UpdateData UpdateData)
        {
            List<AvailableBuildLanguages> availableBuildLanguages = (await UpdateData.GetAvailableLanguagesAsync()).Select(lang =>
            {
                var boundlanguageobject = CultureInfo.GetCultureInfoByIetfLanguageTag(lang);

                return new AvailableBuildLanguages() { LanguageCode = lang, Title = boundlanguageobject.DisplayName, FlagUri = new Uri($"ms-appx:///Assets/Flags/{lang.Split('-').Last()}.png") };
            }).ToList();

            availableBuildLanguages.Sort((x, y) => x.Title.CompareTo(y.Title));

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
                    var fileDownloadInfo = await FE3Handler.GetFileUrl(UpdateData, metadataCabs[0].Digest);
                    if (fileDownloadInfo == null)
                    {
                        goto exit;
                    }

                    string metadataCabTemp = Path.GetTempFileName();

                    // Download the file
                    WebClient client = new WebClient();
                    await client.DownloadFileTaskAsync(new Uri(fileDownloadInfo.DownloadUrl), metadataCabTemp);

                    if (fileDownloadInfo.IsEncrypted)
                    {
                        if (!await fileDownloadInfo.DecryptAsync(metadataCabTemp, metadataCabTemp + ".decrypted"))
                            goto exit;
                        metadataCabTemp += ".decrypted";
                    }

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

        private static void AddUpdatesIfNotPresentAlready(ICollection<UpdateData> updates, IEnumerable<UpdateData> uncleanedData)
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

        private static async Task<IEnumerable<UpdateData>> GetUpdates(MachineType MachineType)
        {
            HashSet<UpdateData> updates = new HashSet<UpdateData>();

            CTAC[] ctacs = new CTAC[]
            {
                new CTAC(OSSkuId.PPIPro, "10.0.15063.534", MachineType, "WIS", "", "CB", "rs2_release", "Production", false),
                new CTAC(OSSkuId.PPIPro, "10.0.15063.534", MachineType, "WIF", "", "CB", "rs2_release", "Production", false),
                new CTAC(OSSkuId.PPIPro, "10.0.19041.84", MachineType, "Retail", "", "CB", "vb_release", "Production", false),
                new CTAC(OSSkuId.Professional, "10.0.16299.15", MachineType, "Retail", "", "CB", "rs3_release", "Production", true),
                new CTAC(OSSkuId.Professional, "10.0.17134.1", MachineType, "Retail", "", "CB", "rs4_release", "Production", true),
                new CTAC(OSSkuId.Professional, "10.0.17763.1217", MachineType, "Retail", "", "CB", "rs5_release", "Production", true),
                new CTAC(OSSkuId.Professional, "10.0.18362.836", MachineType, "Retail", "", "CB", "19h1_release", "Production", true),
                new CTAC(OSSkuId.Professional, "10.0.19041.84", MachineType, "Retail", "", "CB", "vb_release", "Production", false),
                new CTAC(OSSkuId.Professional, "10.0.19041.84", MachineType, "External", "ReleasePreview", "CB", "vb_release", "Production", false),
                new CTAC(OSSkuId.Professional, "10.0.19041.84", MachineType, "External", "FeaturePreview", "CB", "vb_release", "Production", false),
                new CTAC(OSSkuId.Professional, "10.0.19041.84", MachineType, "External", "Beta", "CB", "vb_release", "Production", false),
                new CTAC(OSSkuId.Professional, "10.0.19041.84", MachineType, "External", "Dev", "CB", "vb_release", "Production", false)
            };

            List<Task<IEnumerable<UpdateData>>> tasks = new List<Task<IEnumerable<UpdateData>>>();
            foreach (var ctac in ctacs)
            {
                tasks.Add(FE3Handler.GetUpdates(null, ctac, null, FileExchangeV3UpdateFilter.ProductRelease));
            }

            IEnumerable<UpdateData>[] datas = await Task.WhenAll(tasks);
            foreach (var data in datas)
            {
                AddUpdatesIfNotPresentAlready(updates, data);
            }

            return updates;
        }
    }
}