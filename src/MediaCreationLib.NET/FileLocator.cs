using CompDB;
using Microsoft.Cabinet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UUPMediaCreator.InterCommunication;
using static MediaCreationLib.MediaCreator;

#nullable enable

namespace MediaCreationLib.NET
{
    internal static class FileLocator
    {
        internal static HashSet<CompDBXmlClass.CompDB> GetCompDBsFromUUPFiles(string UUPPath)
        {
            HashSet<CompDBXmlClass.CompDB> compDBs = new HashSet<CompDBXmlClass.CompDB>();

            try
            {
                if (Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*").Count() > 0)
                {
                    using (CabinetHandler cabinet = new CabinetHandler(File.OpenRead(Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*").First())))
                    {
                        foreach (var file in cabinet.Files.Where(x => x.EndsWith(".xml.cab", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            try
                            {
                                using (CabinetHandler cabinet2 = new CabinetHandler(cabinet.OpenFile(file)))
                                {
                                    string xmlfile = cabinet2.Files.First();
                                    using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                                    {
                                        compDBs.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                else
                {
                    IEnumerable<string> files = Directory.EnumerateFiles(UUPPath).Select(x => Path.GetFileName(x)).Where(x => x.EndsWith(".xml.cab", StringComparison.InvariantCultureIgnoreCase));

                    foreach (var file in files)
                    {
                        try
                        {
                            using (CabinetHandler cabinet2 = new CabinetHandler(File.OpenRead(Path.Combine(UUPPath, file))))
                            {
                                string xmlfile = cabinet2.Files.First();
                                using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                                {
                                    compDBs.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            return compDBs;
        }

        internal static CompDBXmlClass.CompDB? GetNeutralCompDB(HashSet<CompDBXmlClass.CompDB> compDBs)
        {
            foreach (var compDB in compDBs)
            {
                //
                // Newer style compdbs have a tag attribute, make use of it.
                //
                if (compDB.Tags != null &&
                    compDB.Tags.Type.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase) &&
                    compDB.Tags.Tag != null && 
                    compDB.Tags.Tag.FirstOrDefault(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    return compDB;
                }
                //
                // Older style compdbs have no tag elements, we need to find out if it's a neutral compdb using another way
                //
                else if (compDB.Features?.Feature?.FirstOrDefault(x => 
                    x.Type?.Contains("BaseNeutral", StringComparison.InvariantCultureIgnoreCase) == true) != null)
                {
                    return compDB;
                }
            }

            return null;
        }

        internal static HashSet<CompDBXmlClass.CompDB> GetEditionCompDBsForLanguage(HashSet<CompDBXmlClass.CompDB> compDBs, string LanguageCode)
        {
            HashSet<CompDBXmlClass.CompDB> filteredCompDBs = new HashSet<CompDBXmlClass.CompDB>();

            foreach (var compDB in compDBs)
            {
                //
                // Newer style compdbs have a tag attribute, make use of it.
                //
                if (compDB.Tags != null &&
                    compDB.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) &&
                    compDB.Tags.Tag != null &&
                    compDB.Tags.Tag.Count == 3 &&
                    compDB.Tags.Tag.FirstOrDefault(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true &&
                    compDB.Tags.Tag.FirstOrDefault(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals(LanguageCode, StringComparison.InvariantCultureIgnoreCase) == true &&
                    compDB.Tags.Tag.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)))
                {
                    filteredCompDBs.Add(compDB);
                }
                //
                // Older style compdbs have no tag elements, we need to find out if it's an edition compdb using another way
                //
                else if (compDB.Features?.Feature?.FirstOrDefault(x => 
                    x.Type?.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase) == true && 
                    x.FeatureID?.Contains(LanguageCode, StringComparison.InvariantCultureIgnoreCase) == true) != null)
                {
                    filteredCompDBs.Add(compDB);
                }
            }

            return filteredCompDBs;
        }

        internal static (bool Succeeded, string BaseESD) LocateFilesForSetupMediaCreation(
            string UUPPath,
            string LanguageCode,
            ProgressCallback? progressCallback = null)
        {
            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Looking up Composition Database in order to find a Base ESD image appropriate for building windows setup files.");

            if (GetCompDBsFromUUPFiles(UUPPath) is HashSet<CompDBXmlClass.CompDB> compDBs)
            {
                HashSet<CompDBXmlClass.CompDB> filteredCompDBs = GetEditionCompDBsForLanguage(compDBs, LanguageCode);
                if (filteredCompDBs.Count > 0)
                {
                    foreach (var currentCompDB in filteredCompDBs)
                    {
                        foreach (CompDBXmlClass.Package feature in currentCompDB.Features.Feature[0].Packages.Package)
                        {
                            CompDBXmlClass.Package pkg = currentCompDB.Packages.Package.First(x => x.ID == feature.ID);

                            string file = pkg.Payload.PayloadItem.Path.Split('\\').Last().Replace("~31bf3856ad364e35", "").Replace("~.", ".").Replace("~", "-").Replace("-.", ".");

                            if (feature.PackageType == "MetadataESD")
                            {
                                if (!File.Exists(Path.Combine(UUPPath, file)))
                                {
                                    file = pkg.Payload.PayloadItem.Path;
                                    if (!File.Exists(Path.Combine(UUPPath, file)))
                                    {
                                        break;
                                    }
                                }

                                return (true, Path.Combine(UUPPath, file));
                            }
                        }
                    }
                }

                progressCallback?.Invoke(Common.ProcessPhase.Error, true, 0, "While looking up the Composition Database, we couldn't find an edition composition for the specified language. This error is fatal.");
                return (false, null);
            }
            else
            {
                progressCallback?.Invoke(Common.ProcessPhase.Error, true, 0, "We couldn't find the Composition Database. Please make sure you have downloaded the <aggregatedmetadata> cabinet file, or the <CompDB> cabinet files (if the build is lower than RS3 RTM). This error is fatal.");
                return (false, null);
            }
        }

        internal static (bool Succeeded, string BaseESD, HashSet<string> ReferencePackages, HashSet<string> ReferencePackagesToConvert) LocateFilesForBaseEditionCreation(
            string UUPPath,
            string LanguageCode,
            string EditionID,
            ProgressCallback progressCallback = null)
        {
            bool success = true;

            HashSet<string> ReferencePackages = new HashSet<string>();
            HashSet<string> referencePackagesToConvert = new HashSet<string>();
            string BaseESD = null;
            CompDBXmlClass.CompDB compDB = null;

            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            if (Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*").Count() > 0)
            {
                using (CabinetHandler cabinet = new CabinetHandler(File.OpenRead(Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*").First())))
                {
                    IEnumerable<string> potentialFiles = cabinet.Files.Where(x => x.ToLower().Contains($"desktoptargetcompdb_{EditionID.ToLower()}_{LanguageCode.ToLower()}"));

                    if (potentialFiles.Count() == 0)
                    {
                        progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "CompDB not found from aggregated metadata");
                        goto error;
                    }

                    string file = potentialFiles.First();

                    using (CabinetHandler cabinet2 = new CabinetHandler(cabinet.OpenFile(file)))
                    {
                        string xmlfile = cabinet2.Files.First();
                        using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                        {
                            compDB = CompDBXmlClass.DeserializeCompDB(xmlstream);
                        }
                    }
                }
            }
            else
            {
                IEnumerable<string> files = Directory.EnumerateFiles(UUPPath).Select(x => Path.GetFileName(x));
                IEnumerable<string> potentialFiles = files.Where(x => x.ToLower().Contains($"desktoptargetcompdb_{EditionID.ToLower()}_{LanguageCode.ToLower()}"));

                if (potentialFiles.Count() == 0)
                {
                    progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "CompDB not found from metadata cab list");
                    goto error;
                }

                string file = potentialFiles.First();

                using (CabinetHandler cabinet2 = new CabinetHandler(File.OpenRead(Path.Combine(UUPPath, file))))
                {
                    string xmlfile = cabinet2.Files.First();
                    using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                    {
                        compDB = CompDBXmlClass.DeserializeCompDB(xmlstream);
                    }
                }
            }

            if (compDB == null)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "No compDB found");
                goto error;
            }

            foreach (CompDBXmlClass.Package feature in compDB.Features.Feature[0].Packages.Package)
            {
                CompDBXmlClass.Package pkg = compDB.Packages.Package.First(x => x.ID == feature.ID);

                string file = pkg.Payload.PayloadItem.Path.Split('\\').Last().Replace("~31bf3856ad364e35", "").Replace("~.", ".").Replace("~", "-").Replace("-.", ".");

                if (!File.Exists(Path.Combine(UUPPath, file)))
                {
                    file = pkg.Payload.PayloadItem.Path;
                    if (!File.Exists(Path.Combine(UUPPath, file)))
                    {
                        progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, $"File {file} is missing");
                        goto error;
                    }
                }

                if (feature.PackageType == "MetadataESD")
                {
                    BaseESD = Path.Combine(UUPPath, file);
                    continue;
                }

                if (file.EndsWith(".esd", StringComparison.InvariantCultureIgnoreCase))
                {
                    ReferencePackages.Add(Path.Combine(UUPPath, file));
                }
                else if (file.EndsWith(".cab", StringComparison.InvariantCultureIgnoreCase))
                {
                    referencePackagesToConvert.Add(Path.Combine(UUPPath, file));
                }
            }

            if (BaseESD == null)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Base ESD not found");
                goto error;
            }

            goto exit;

            error:
            success = false;

            exit:
            return (success, BaseESD, ReferencePackages, referencePackagesToConvert);
        }
    }
}
