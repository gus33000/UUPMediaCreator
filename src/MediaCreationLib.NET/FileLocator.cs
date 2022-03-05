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
using MediaCreationLib.Planning.Applications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UUPMediaCreator.InterCommunication;

#nullable enable

namespace MediaCreationLib.NET
{
    internal static class FileLocator
    {
        internal static (bool, HashSet<string>) VerifyFilesAreAvailableForCompDBs(HashSet<CompDBXmlClass.CompDB> compDBs, string UUPPath)
        {
            HashSet<string> missingPackages = new();

            foreach (CompDBXmlClass.CompDB compDB in compDBs)
            {
                (bool succeeded, HashSet<string> missingFiles) = Planning.NET.FileLocator.VerifyFilesAreAvailableForCompDB(compDB, UUPPath);
                foreach (string? missingFile in missingFiles)
                {
                    if (!missingPackages.Contains(missingFile))
                    {
                        missingPackages.Add(missingFile);
                    }
                }
            }

            return (missingPackages.Count == 0, missingPackages);
        }

        internal static CompDBXmlClass.CompDB? GetEditionCompDBForLanguage(
            HashSet<CompDBXmlClass.CompDB> compDBs,
            string Edition,
            string LanguageCode)
        {
            foreach (CompDBXmlClass.CompDB? compDB in compDBs)
            {
                //
                // Newer style compdbs have a tag attribute, make use of it.
                //
                if (compDB.Tags != null)
                {
                    if (compDB.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) &&
                       compDB.Tags.Tag?.Count == 3 &&
                       compDB.Tags.Tag.Find(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true &&
                       compDB.Tags.Tag.Find(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals(LanguageCode, StringComparison.InvariantCultureIgnoreCase) == true &&
                       compDB.Tags.Tag.Find(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals(Edition, StringComparison.InvariantCultureIgnoreCase) == true)
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
            TempManager.TempManager tempManager,
            ProgressCallback? progressCallback = null)
        {
            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Looking up Composition Database in order to find a Base ESD image appropriate for building windows setup files.");

            if (Planning.NET.FileLocator.GetCompDBsFromUUPFiles(UUPPath, tempManager) is HashSet<CompDBXmlClass.CompDB> compDBs)
            {
                HashSet<CompDBXmlClass.CompDB> filteredCompDBs = compDBs.GetEditionCompDBsForLanguage(LanguageCode);
                if (filteredCompDBs.Count > 0)
                {
                    foreach (CompDBXmlClass.CompDB? currentCompDB in filteredCompDBs)
                    {
                        foreach (CompDBXmlClass.Package feature in currentCompDB.Features.Feature[0].Packages.Package)
                        {
                            CompDBXmlClass.Package pkg = currentCompDB.Packages.Package.First(x => x.ID == feature.ID);

                            string file = pkg.GetCommonlyUsedIncorrectFileName();

                            if (feature.PackageType == "MetadataESD")
                            {
                                if (!File.Exists(Path.Combine(UUPPath, file)))
                                {
                                    file = pkg.Payload.PayloadItem[0].Path.Replace('\\', Path.DirectorySeparatorChar);
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

        internal static bool GenerateAppXLicenseFiles(
            string UUPPath,
            string LanguageCode,
            string EditionID,
            TempManager.TempManager tempManager,
            ProgressCallback? progressCallback = null)
        {
            bool success = true;

            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            HashSet<CompDBXmlClass.CompDB> compDBs = Planning.NET.FileLocator.GetCompDBsFromUUPFiles(UUPPath, tempManager);

            CompDBXmlClass.CompDB? compDB = GetEditionCompDBForLanguage(compDBs, EditionID, LanguageCode);

            if (compDB == null)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "No compDB found");
                goto error;
            }

            if (compDBs.Any(x => x.Name?.StartsWith("Build~") == true && x.Name?.EndsWith("~Desktop_Apps~~") == true))
            {
                CompDBXmlClass.CompDB AppCompDB = compDBs.First(x => x.Name?.StartsWith("Build~") == true && x.Name?.EndsWith("~Desktop_Apps~~") == true);
                AppxSelectionEngine.GenerateLicenseXmlFiles(compDB, AppCompDB, UUPPath);
            }

            goto exit;

        error:
            success = false;

        exit:
            return success;
        }

        internal static (bool Succeeded, string BaseESD, HashSet<string> ReferencePackages, HashSet<string> ReferencePackagesToConvert) LocateFilesForBaseEditionCreation(
            string UUPPath,
            string LanguageCode,
            string EditionID,
            TempManager.TempManager tempManager,
            ProgressCallback? progressCallback = null)
        {
            bool success = true;

            HashSet<string> ReferencePackages = new();
            HashSet<string> referencePackagesToConvert = new();
            string? BaseESD = null;
            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            CompDBXmlClass.CompDB? compDB = GetEditionCompDBForLanguage(Planning.NET.FileLocator.GetCompDBsFromUUPFiles(UUPPath, tempManager), EditionID, LanguageCode);

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
                    file = pkg.Payload.PayloadItem[0].Path.Replace('\\', Path.DirectorySeparatorChar);
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
