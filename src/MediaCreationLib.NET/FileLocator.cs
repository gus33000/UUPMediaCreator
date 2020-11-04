using CompDB;
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
        internal static (bool, HashSet<string>) VerifyFilesAreAvailableForCompDBs(HashSet<CompDBXmlClass.CompDB> compDBs, string UUPPath)
        {
            HashSet<string> missingPackages = new HashSet<string>();

            foreach (CompDBXmlClass.CompDB compDB in compDBs)
            {
                (bool succeeded, HashSet<string> missingFiles) = Planning.NET.FileLocator.VerifyFilesAreAvailableForCompDB(compDB, UUPPath);
                foreach (var missingFile in missingFiles)
                {
                    if (!missingPackages.Contains(missingFile))
                    {
                        missingPackages.Add(missingFile);
                    }
                }
            }

            return (missingPackages.Count <= 0, missingPackages);
        }

        internal static CompDBXmlClass.CompDB? GetEditionCompDBForLanguage(
            HashSet<CompDBXmlClass.CompDB> compDBs,
            string Edition,
            string LanguageCode)
        {
            foreach (var compDB in compDBs)
            {
                //
                // Newer style compdbs have a tag attribute, make use of it.
                //
                if (compDB.Tags != null)
                {
                    if (compDB.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) &&
                       compDB.Tags.Tag != null &&
                       compDB.Tags.Tag.Count == 3 &&
                       compDB.Tags.Tag.FirstOrDefault(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true &&
                       compDB.Tags.Tag.FirstOrDefault(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals(LanguageCode, StringComparison.InvariantCultureIgnoreCase) == true &&
                       compDB.Tags.Tag.FirstOrDefault(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals(Edition, StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        return compDB;
                    }
                }
                //
                // Older style compdbs have no tag elements, we need to find out if it's an edition compdb using another way
                // TODO: Do not do contains
                //
                else if (compDB.Features?.Feature?.FirstOrDefault(x =>
                        x.Type?.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase) == true &&
                        x.FeatureID?.Contains(LanguageCode, StringComparison.InvariantCultureIgnoreCase) == true &&
                        x.FeatureID?.Contains(Edition, StringComparison.InvariantCultureIgnoreCase) == true) != null)
                {
                    return compDB;
                }
            }

            return null;
        }

        internal static (bool Succeeded, string BaseESD) LocateFilesForSetupMediaCreation(
            string UUPPath,
            string LanguageCode,
            ProgressCallback? progressCallback = null)
        {
            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Looking up Composition Database in order to find a Base ESD image appropriate for building windows setup files.");

            if (Planning.NET.FileLocator.GetCompDBsFromUUPFiles(UUPPath) is HashSet<CompDBXmlClass.CompDB> compDBs)
            {
                HashSet<CompDBXmlClass.CompDB> filteredCompDBs = compDBs.GetEditionCompDBsForLanguage(LanguageCode);
                if (filteredCompDBs.Count > 0)
                {
                    foreach (var currentCompDB in filteredCompDBs)
                    {
                        foreach (CompDBXmlClass.Package feature in currentCompDB.Features.Feature[0].Packages.Package)
                        {
                            CompDBXmlClass.Package pkg = currentCompDB.Packages.Package.First(x => x.ID == feature.ID);

                            string file = pkg.GetCommonlyUsedIncorrectFileName();

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
            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            CompDBXmlClass.CompDB? compDB = GetEditionCompDBForLanguage(Planning.NET.FileLocator.GetCompDBsFromUUPFiles(UUPPath), EditionID, LanguageCode);

            if (compDB == null)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "No compDB found");
                goto error;
            }

            foreach (CompDBXmlClass.Package feature in compDB.Features.Feature[0].Packages.Package)
            {
                CompDBXmlClass.Package pkg = compDB.Packages.Package.First(x => x.ID == feature.ID);

                string file = pkg.GetCommonlyUsedIncorrectFileName();

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
