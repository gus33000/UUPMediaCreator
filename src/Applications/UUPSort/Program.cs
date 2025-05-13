using Cabinet;
using System.Security.Cryptography;
using System.Xml.Serialization;
using UnifiedUpdatePlatform.Services.Composition.Database;

namespace UUPSort
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string directoryWithCabs = args[0];

            Console.WriteLine("Parsing CompDB Packages...");

            List<CompDB> compDBs = GetCompDBsFromUUPFiles(directoryWithCabs);

            if (!compDBs.Any(db => db.AppX != null))
            {
                Console.WriteLine("No Composition Database references Appx files, nothing to do.");
                return;
            }

            Console.WriteLine("Building appx license map from cabs...");

            IDictionary<string, string> appxLicenseFileMap = GetAppFilePairs(Path.GetFullPath(directoryWithCabs));

            Console.WriteLine("Completed Building appx license map from cabs.");

            string[] appxFiles = Directory.GetFiles(Path.GetFullPath(directoryWithCabs), "appx_*", SearchOption.TopDirectoryOnly);

            CompDB? canonicalCompDB = compDBs
                .Where(compDB => compDB.Tags.Tag
                .Find(x => x.Name
                .Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?
                .Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true)
                .FirstOrDefault(x => x.AppX != null);

            if (canonicalCompDB != null)
            {
                foreach (string appxFile in appxFiles)
                {
                    string payloadHash;
                    using (FileStream fileStream = File.OpenRead(appxFile))
                    {
                        using SHA256 sha = SHA256.Create();
                        payloadHash = Convert.ToBase64String(sha.ComputeHash(fileStream));
                    }

                    AppxPackage? package = canonicalCompDB.AppX.AppXPackages.Package.Where(p => p.Payload.PayloadItem.FirstOrDefault()?.PayloadHash == payloadHash).FirstOrDefault();
                    if (package == null)
                    {
                        Console.WriteLine($"Could not locate package with payload hash {payloadHash}. Skipping.");
                    }
                    else
                    {
                        string appxFolder = Path.Combine(directoryWithCabs, Path.GetDirectoryName(package.Payload.PayloadItem.FirstOrDefault().Path));

                        if (!Directory.Exists(appxFolder))
                        {
                            Console.WriteLine($"Creating {appxFolder}");
                            _ = Directory.CreateDirectory(appxFolder);
                        }

                        string appxPath = Path.Combine(directoryWithCabs, package.Payload.PayloadItem.FirstOrDefault().Path);
                        Console.WriteLine($"Moving {appxFile} to {appxPath}");
                        File.Move(appxFile, appxPath, true);
                    }
                }

                foreach (AppxPackage package in canonicalCompDB.AppX.AppXPackages.Package)
                {
                    if (package.LicenseData != null)
                    {
                        string appxPath = package.Payload.PayloadItem.FirstOrDefault().Path;
                        string appxLicensePath = Path.Combine(directoryWithCabs, $"{appxLicenseFileMap[appxPath]}");

                        string appxFolder = Path.GetDirectoryName(appxLicensePath);
                        if (!Directory.Exists(appxFolder))
                        {
                            Console.WriteLine($"Creating {appxFolder}");
                            _ = Directory.CreateDirectory(appxFolder);
                        }

                        Console.WriteLine($"Writing license to {appxLicensePath}");
                        File.WriteAllText(appxLicensePath, package.LicenseData);
                    }
                }
            }
        }

        private static List<CompDB> GetCompDBsFromUUPFiles(string UUPPath)
        {
            List<CompDB> compDBs = new();
            List<string> tempFiles = new();

            try
            {
                IEnumerable<string>? enumeratedFiles = Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*", new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive });
                if (enumeratedFiles.Any())
                {
                    string? cabFile = enumeratedFiles.First();

                    foreach (string? file in CabinetExtractor.EnumCabinetFiles(cabFile).Where(x => x.FileName.EndsWith(".xml.cab", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.FileName))
                    {
                        try
                        {
                            string tmp = Path.GetTempFileName();
                            tempFiles.Add(tmp);
                            File.Delete(tmp);
                            File.WriteAllBytes(tmp, CabinetExtractor.ExtractCabinetFile(cabFile, file));

                            string filename = CabinetExtractor.EnumCabinetFiles(tmp).First().FileName;
                            byte[] xmlfile = CabinetExtractor.ExtractCabinetFile(tmp, filename);

                            using Stream xmlstream = new MemoryStream(xmlfile);
                            compDBs.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    IEnumerable<string> files = Directory.EnumerateFiles(UUPPath).Select(x => Path.GetFileName(x)).Where(x => x.EndsWith(".xml.cab", StringComparison.InvariantCultureIgnoreCase));

                    foreach (string? file in files)
                    {
                        try
                        {
                            string? cabFile = Path.Combine(UUPPath, file);

                            byte[] xmlfile = CabinetExtractor.ExtractCabinetFile(cabFile, CabinetExtractor.EnumCabinetFiles(cabFile).First().FileName);

                            using Stream xmlstream = new MemoryStream(xmlfile);
                            compDBs.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                        }
                        catch { }
                    }
                }
            }
            catch { }

            foreach (string tmp in tempFiles)
            {
                if (File.Exists(tmp))
                {
                    File.Delete(tmp);
                }
            }

            return compDBs;
        }

        private static Dictionary<string, string> GetAppFilePairs(string directoryWithCabs)
        {
            Dictionary<string, string> appFilePairs = new();

            IEnumerable<string> cabFiles = Directory.EnumerateFiles(directoryWithCabs, "*.cab", SearchOption.AllDirectories);
            IEnumerable<string> orderedCabFiles = cabFiles.OrderBy(cabFile => new FileInfo(cabFile).Length);

            foreach (string cabFile in orderedCabFiles)
            {
                using FileStream cabFileStream = File.OpenRead(cabFile);

                Cabinet.Cabinet cab = new(cabFileStream);

                bool extracted = TryGetMicrosoftUpdateManifestFromCabinet(cab, out XmlMum.Assembly? assembly);

                if (extracted && assembly != null)
                {
                    bool succeeded = TryGetFeatureManifestsFromCabinet(cab, assembly, out ImageUpdate.FeatureManifest[]? featureManifests);

                    if (succeeded && featureManifests != null && featureManifests.Any())
                    {
                        foreach (ImageUpdate.FeatureManifest fm in featureManifests)
                        {
                            if (fm.AppX?.AppXPackages?.PackageFile is List<ImageUpdate.PackageFile> appxPackageFiles)
                            {
                                foreach (ImageUpdate.PackageFile appxPackageFile in appxPackageFiles)
                                {
                                    if (!string.IsNullOrEmpty(appxPackageFile.LicenseFile))
                                    {
                                        string destinationPath = appxPackageFile.Path.Replace("$(mspackageroot)", "").TrimStart('\\');

                                        string destinationFileName = $"{destinationPath}\\{appxPackageFile.Name}";
                                        string destinationLicenseFileName = $"{destinationPath}\\{appxPackageFile.LicenseFile}";

                                        appFilePairs.Add(destinationFileName, destinationLicenseFileName);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return appFilePairs;
        }

        private static bool TryGetFeatureManifestsFromCabinet(Cabinet.Cabinet cab, XmlMum.Assembly assembly, out ImageUpdate.FeatureManifest[]? featureManifests)
        {
            try
            {
                XmlSerializer xmlSerializer = new(typeof(ImageUpdate.FeatureManifest));

                List<ImageUpdate.FeatureManifest> featureManifestsList = new();
                List<XmlMum.File> assemblyComponentFiles = assembly.Package.CustomInformation.File;

                foreach (XmlMum.File assemblyComponentFile in assemblyComponentFiles)
                {
                    string componentFileName = assemblyComponentFile.Name;

                    if (componentFileName.Contains("Windows\\ImageUpdate\\FeatureManifest", StringComparison.InvariantCultureIgnoreCase) && componentFileName.EndsWith(".xml"))
                    {
                        byte[] featureManifestBuffer = cab.ReadFile(assemblyComponentFile.Cabpath);

                        using MemoryStream stream = new(featureManifestBuffer);

                        ImageUpdate.FeatureManifest? fm = xmlSerializer.Deserialize(stream) as ImageUpdate.FeatureManifest;

                        if (fm != null)
                        {
                            featureManifestsList.Add(fm);
                        }
                    }
                }

                if (featureManifestsList.Any())
                {
                    featureManifests = featureManifestsList.ToArray();
                    return true;
                }
            }
            catch { }

            featureManifests = null;
            return false;
        }

        private static bool TryGetMicrosoftUpdateManifestFromCabinet(Cabinet.Cabinet cab, out XmlMum.Assembly? assembly)
        {
            try
            {
                if (cab.Files.Any(x => x.FileName.Equals("update.mum")))
                {
                    byte[] updateMumBuffer = cab.ReadFile("update.mum");

                    XmlSerializer xmlSerializer = new(typeof(XmlMum.Assembly));

                    using MemoryStream stream = new(updateMumBuffer);

                    assembly = xmlSerializer.Deserialize(stream) as XmlMum.Assembly;
                    return true;
                }
            }
            catch { }

            assembly = null;
            return false;
        }
    }
}