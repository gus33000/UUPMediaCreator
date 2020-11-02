using CompDB;
using Microsoft.Cabinet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UUPMediaCreator.InterCommunication;
using static MediaCreationLib.MediaCreator;

namespace MediaCreationLib.NET
{
    internal static class FileLocator
    {
        internal static (bool Succeeded, string BaseESD) LocateFilesForSetupMediaCreation(
            string UUPPath,
            string LanguageCode,
            ProgressCallback progressCallback = null)
        {
            bool success = true;

            string BaseESD = null;
            HashSet<CompDBXmlClass.CompDB> compDBs = new HashSet<CompDBXmlClass.CompDB>();

            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            if (Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*").Count() > 0)
            {
                using (CabinetHandler cabinet = new CabinetHandler(File.OpenRead(Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*").First())))
                {
                    IEnumerable<string> potentialFiles = cabinet.Files.Where(x =>
                    x.ToLower().Contains($"desktoptargetcompdb_") &&
                    x.ToLower().Contains($"_{LanguageCode.ToLower()}") &&
                    !x.ToLower().Contains("lxp") &&
                    !x.ToLower().Contains($"desktoptargetcompdb_{LanguageCode.ToLower()}"));

                    if (potentialFiles.Count() == 0)
                        goto error;

                    foreach (var file in potentialFiles)
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
                }
            }
            else
            {
                IEnumerable<string> files = Directory.EnumerateFiles(UUPPath).Select(x => Path.GetFileName(x));
                IEnumerable<string> potentialFiles = files.Where(x =>
                    x.ToLower().Contains($"desktoptargetcompdb_") &&
                    x.ToLower().Contains($"_{LanguageCode.ToLower()}") &&
                    !x.ToLower().Contains("lxp") &&
                    !x.ToLower().Contains($"desktoptargetcompdb_{LanguageCode.ToLower()}"));

                if (potentialFiles.Count() == 0)
                    goto error;

                foreach (var file in potentialFiles)
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
            }

            if (compDBs.Count == 0)
                goto error;

            foreach (var firstCompDB in compDBs)
            {
                foreach (CompDBXmlClass.Package feature in firstCompDB.Features.Feature[0].Packages.Package)
                {
                    CompDBXmlClass.Package pkg = firstCompDB.Packages.Package.First(x => x.ID == feature.ID);

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

                        BaseESD = Path.Combine(UUPPath, file);
                        break;
                    }
                }

                if (BaseESD != null)
                    break;
            }

            if (BaseESD == null)
                goto error;

            goto exit;

            error:
            success = false;

            exit:
            return (success, BaseESD);
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
