using CompDB;
using Imaging;
using MediaCreationLib.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UUPMediaCreator.InterCommunication;

namespace MediaCreationLib.Planning
{
    public class ConversionPlanBuilder
    {
        private static WIMImaging imagingInterface = new WIMImaging();

        public enum AvailabilityType
        {
            Canonical,
            EditionUpgrade,
            VirtualEdition,
            EditionPackageSwap
        }

        public class PlannedEdition
        {
            public string EditionName { get; set; }
            public AvailabilityType AvailabilityType { get; set; }
        }

        public class EditionTarget
        {
            public PlannedEdition PlannedEdition { get; set; }
            public List<EditionTarget> NonDestructiveTargets = new List<EditionTarget>();
            public List<EditionTarget> DestructiveTargets = new List<EditionTarget>();
        }

        private static List<string> editionsAdded = new List<string>();

        private static EditionTarget BuildTarget(
            PlannedEdition edition,
            List<PlannedEdition> hackEditions,
            List<EditionMappingXML.Edition> virtualWindowsEditions,
            Dictionary<string, string> editionMatrixItems,
            List<PlannedEdition> availableBasisHackedEditions)
        {
            EditionTarget target = new EditionTarget() { PlannedEdition = edition };
            editionsAdded.Add(edition.EditionName);

            //
            // Handle edition downgrades
            //
            if (availableBasisHackedEditions != null)
            {
                foreach (var element in Constants.EditionDowngradeDict)
                {
                    string source = element.Key;
                    foreach (var destination in element.Value)
                    {
                        if (availableBasisHackedEditions.Any(x => x.EditionName.Equals(destination, StringComparison.InvariantCultureIgnoreCase)) &&
                        edition.EditionName.Equals(source, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (!editionsAdded.Any(x => x.Equals(destination, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                var planedition = availableBasisHackedEditions.First(x => x.EditionName.Equals(destination, StringComparison.InvariantCultureIgnoreCase));
                                availableBasisHackedEditions.Remove(planedition);
                                target.DestructiveTargets.Add(BuildTarget(planedition, hackEditions, virtualWindowsEditions, editionMatrixItems, availableBasisHackedEditions));
                            }
                        }
                    }
                }
            }

            if (virtualWindowsEditions != null)
            {
                foreach (var ed in virtualWindowsEditions.Where(x => x.ParentEdition.Equals(edition.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (editionsAdded.Any(x => x.Equals(ed.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }
                    editionsAdded.Add(ed.Name);

                    PlannedEdition plannedEdition = new PlannedEdition() { AvailabilityType = AvailabilityType.VirtualEdition, EditionName = ed.Name };
                    target.NonDestructiveTargets.Add(BuildTarget(plannedEdition, hackEditions, virtualWindowsEditions, editionMatrixItems, null));
                }
            }

            target.NonDestructiveTargets = target.NonDestructiveTargets.OrderBy(x => x.PlannedEdition.EditionName).ToList();

            foreach (var ed in editionMatrixItems.Where(x => x.Key.Equals(edition.EditionName, StringComparison.InvariantCultureIgnoreCase)))
            {
                if (editionsAdded.Any(x => x.Equals(ed.Value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }
                editionsAdded.Add(ed.Value);

                PlannedEdition plannedEdition = new PlannedEdition() { AvailabilityType = AvailabilityType.EditionUpgrade, EditionName = ed.Value };
                target.DestructiveTargets.Add(BuildTarget(plannedEdition, hackEditions, virtualWindowsEditions, editionMatrixItems, null));
            }

            var corehackeditions = hackEditions.Where(x =>
            x.EditionName.StartsWith("core", StringComparison.InvariantCultureIgnoreCase) &&
            !(x.EditionName.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || x.EditionName.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase)));

            var corehackneditions = hackEditions.Where(x =>
            x.EditionName.StartsWith("core", StringComparison.InvariantCultureIgnoreCase) &&
            (x.EditionName.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || x.EditionName.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase)));

            var professionalhackeditions = hackEditions.Where(x =>
            !x.EditionName.StartsWith("core", StringComparison.InvariantCultureIgnoreCase) &&
            !(x.EditionName.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || x.EditionName.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase)));

            var professionalhackneditions = hackEditions.Where(x =>
            !x.EditionName.StartsWith("core", StringComparison.InvariantCultureIgnoreCase) &&
            (x.EditionName.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || x.EditionName.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase)));

            if (edition.EditionName.Equals("professional", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var ed in professionalhackeditions)
                {
                    if (editionsAdded.Any(x => x.Equals(ed.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }
                    editionsAdded.Add(ed.EditionName);

                    target.DestructiveTargets.Add(BuildTarget(ed, hackEditions, virtualWindowsEditions, editionMatrixItems, null));
                }
            }
            else if (edition.EditionName.Equals("professionaln", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var ed in professionalhackneditions)
                {
                    if (editionsAdded.Any(x => x.Equals(ed.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }
                    editionsAdded.Add(ed.EditionName);

                    target.DestructiveTargets.Add(BuildTarget(ed, hackEditions, virtualWindowsEditions, editionMatrixItems, null));
                }
            }
            else if (edition.EditionName.Equals("core", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var ed in corehackeditions)
                {
                    if (editionsAdded.Any(x => x.Equals(ed.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }
                    editionsAdded.Add(ed.EditionName);

                    target.DestructiveTargets.Add(BuildTarget(ed, hackEditions, virtualWindowsEditions, editionMatrixItems, null));
                }
            }
            else if (edition.EditionName.Equals("coren", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var ed in corehackneditions)
                {
                    if (editionsAdded.Any(x => x.Equals(ed.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }
                    editionsAdded.Add(ed.EditionName);

                    target.DestructiveTargets.Add(BuildTarget(ed, hackEditions, virtualWindowsEditions, editionMatrixItems, null));
                }
            }

            target.DestructiveTargets = target.DestructiveTargets.OrderBy(x => x.PlannedEdition.EditionName).ToList();

            return target;
        }

        public static bool GetTargetedPlan(
            string UUPPath,
            string LanguageCode,
            out List<EditionTarget> EditionTargets,
            MediaCreator.ProgressCallback progressCallback = null)
        {
            EditionTargets = new List<EditionTarget>();

            bool result = true;
            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            HashSet<CompDBXmlClass.CompDB> compDBs = FileLocator.GetCompDBsFromUUPFiles(UUPPath);
            HashSet<CompDBXmlClass.CompDB> filteredCompDBs = FileLocator.GetEditionCompDBsForLanguage(compDBs, LanguageCode);
            CompDBXmlClass.CompDB neutralCompDB = FileLocator.GetNeutralCompDB(compDBs);

            if (filteredCompDBs.Count == 0)
                goto error;

            List<PlannedEdition> availableCanonicalEditions = new List<PlannedEdition>();
            List<PlannedEdition> availableBasisHackedEditions = new List<PlannedEdition>();
            List<PlannedEdition> hackEditions = new List<PlannedEdition>();

            List<EditionMappingXML.Edition> virtualWindowsEditions = new List<EditionMappingXML.Edition>();
            Dictionary<string, string> editionTargetMapping = new Dictionary<string, string>();

            foreach (var compDB in filteredCompDBs)
            {
                PlannedEdition edition = new PlannedEdition();
                if (compDB.Tags != null)
                {
                    edition.EditionName = compDB.Tags.Tag.First(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)).Value;
                }
                else
                {
                    edition.EditionName = compDB.Features.Feature[0].FeatureID.Split('_')[0];
                }
                edition.AvailabilityType = AvailabilityType.Canonical;
                availableCanonicalEditions.Add(edition);
            }

            availableCanonicalEditions = availableCanonicalEditions.OrderBy(x => x.EditionName).ToList();

            var firstCompDB = filteredCompDBs.First();

            //
            // Attempt to get virtual editions from one compdb
            //
            foreach (CompDBXmlClass.Package feature in firstCompDB.Features.Feature[0].Packages.Package)
            {
                CompDBXmlClass.Package pkg = firstCompDB.Packages.Package.First(x => x.ID == feature.ID);

                //
                // Some download utilities that start with the letter U and finish with UPDump or start with the letter U and finish with UP.rg-adguard download files without respecting Microsoft filenames
                // We attempt to locate files based on what we think they use first.
                //
                string file = pkg.Payload.PayloadItem.Path.Split('\\').Last().Replace("~31bf3856ad364e35", "").Replace("~.", ".").Replace("~", "-").Replace("-.", ".");

                if (!File.Exists(Path.Combine(UUPPath, file)))
                {
                    //
                    // Wow, someone actually downloaded UUP files using a tool that respects Microsoft paths, that's exceptional
                    //
                    file = pkg.Payload.PayloadItem.Path;
                    if (!File.Exists(Path.Combine(UUPPath, file)))
                    {
                        //
                        // What a disapointment, they simply didn't download everything.. Oops.
                        // TODO: generate missing files out of thin air
                        //
                        goto error;
                    }
                }

                //
                // Attempt to locate an edition specific image, this image contains the edition matrix we want
                //
                if (virtualWindowsEditions.Count <= 0 && file.EndsWith(".esd", StringComparison.InvariantCultureIgnoreCase) && file.ToLower().Contains("microsoft-windows-editionspecific"))
                {
                    try
                    {
                        var edSpecific = Path.Combine(UUPPath, file);
                        var tempHashMap = Path.GetTempFileName();
                        File.Delete(tempHashMap);

                        result = imagingInterface.ExtractFileFromImage(edSpecific, 1, "$filehashes$.dat", tempHashMap);
                        if (!result)
                            goto error;

                        string[] hashmapcontent = File.ReadAllLines(tempHashMap);
                        File.Delete(tempHashMap);

                        try
                        {
                            string pathEditionMapping = hashmapcontent.First(x => x.ToLower().Contains("editionmappings.xml")).Split('=').First();

                            result = imagingInterface.ExtractFileFromImage(edSpecific, 1, pathEditionMapping, tempHashMap);
                            if (!result)
                                goto error;

                            string editionmappingcontent = File.ReadAllText(tempHashMap);
                            File.Delete(tempHashMap);

                            var mapping = EditionMappingXML.Deserialize(editionmappingcontent);
                            var virtualeditions = mapping.Edition.Where(x =>
                                !string.IsNullOrEmpty(x.Virtual) &&
                                x.Virtual.Equals("true", StringComparison.InvariantCultureIgnoreCase)).OrderBy(x => x.ParentEdition);

                            virtualWindowsEditions = virtualeditions.ToList();
                        }
                        catch { };
                    }
                    catch { };
                }
                else if (virtualWindowsEditions.Count > 0)
                {
                    break;
                }

                //
                // Loop again
                //
            }

            //
            // Attempt to get upgradable editions from one compdb
            //
            foreach (CompDBXmlClass.Package feature in firstCompDB.Features.Feature[0].Packages.Package)
            {
                CompDBXmlClass.Package pkg = firstCompDB.Packages.Package.First(x => x.ID == feature.ID);

                //
                // Some download utilities that start with the letter U and finish with UPDump or start with the letter U and finish with UP.rg-adguard download files without respecting Microsoft filenames
                // We attempt to locate files based on what we think they use first.
                //
                string file = pkg.Payload.PayloadItem.Path.Split('\\').Last().Replace("~31bf3856ad364e35", "").Replace("~.", ".").Replace("~", "-").Replace("-.", ".");

                if (!File.Exists(Path.Combine(UUPPath, file)))
                {
                    //
                    // Wow, someone actually downloaded UUP files using a tool that respects Microsoft paths, that's exceptional
                    //
                    file = pkg.Payload.PayloadItem.Path;
                    if (!File.Exists(Path.Combine(UUPPath, file)))
                    {
                        //
                        // What a disapointment, they simply didn't download everything.. Oops.
                        // TODO: generate missing files out of thin air
                        //
                        goto error;
                    }
                }

                //
                // Attempt to locate an edition specific image, this image contains the edition matrix we want
                //
                if (editionTargetMapping.Count <= 0 && file.EndsWith(".esd", StringComparison.InvariantCultureIgnoreCase) && file.ToLower().Contains("microsoft-windows-editionspecific"))
                {
                    try
                    {
                        var edSpecific = Path.Combine(UUPPath, file);
                        var tempHashMap = Path.GetTempFileName();
                        File.Delete(tempHashMap);

                        result = imagingInterface.ExtractFileFromImage(edSpecific, 1, "$filehashes$.dat", tempHashMap);
                        if (!result)
                            goto error;

                        string[] hashmapcontent = File.ReadAllLines(tempHashMap);
                        File.Delete(tempHashMap);

                        try
                        {
                            string pathEditionMatrix = hashmapcontent.First(x => x.ToLower().Contains("editionmatrix.xml")).Split('=').First();

                            result = imagingInterface.ExtractFileFromImage(edSpecific, 1, pathEditionMatrix, tempHashMap);
                            if (!result)
                                goto error;

                            string editionmatrixcontent = File.ReadAllText(tempHashMap);
                            File.Delete(tempHashMap);

                            var mapping = EditionMatrixXML.Deserialize(editionmatrixcontent);
                            foreach (var edition in mapping.Edition)
                            {
                                if (edition.Target != null && edition.Target.Count != 0)
                                {
                                    foreach (var target in edition.Target)
                                    {
                                        if (!availableCanonicalEditions.Any(x => x.EditionName.Equals(target.ID, StringComparison.InvariantCultureIgnoreCase)))
                                        {
                                            editionTargetMapping.Add(edition.ID, target.ID);
                                        }
                                    }
                                }
                            }
                        }
                        catch { };
                    }
                    catch { };
                }
                else if (editionTargetMapping.Count > 0)
                {
                    break;
                }

                //
                // Loop again
                //
            }

            var _files = neutralCompDB.Features.Feature.First(x => x.FeatureID == "BaseNeutral").Packages.Package.Select(x => x.ID);

            var editionSpecificPackages = _files.Where(x => x.Count(y => y == '-') == 4).Where(x => x.ToLower().Contains("microsoft-windows-editionspecific"));
            var highPriorityPackages = editionSpecificPackages.Where(x => x.ToLower().Contains("starter") || x.ToLower().Contains("professional") || x.ToLower().Contains("core"));
            editionSpecificPackages = editionSpecificPackages.Except(highPriorityPackages);

            foreach (var file in highPriorityPackages)
            {
                var sku = file.Split('-')[3];
                if (!availableCanonicalEditions.Any(x => x.EditionName.Equals(sku, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (editionTargetMapping != null && !editionTargetMapping
                        .Where(x => availableCanonicalEditions.Any(y => y.EditionName.Equals(x.Key, StringComparison.InvariantCultureIgnoreCase)) ||
                                    hackEditions.Any(y => y.EditionName.Equals(x.Key, StringComparison.InvariantCultureIgnoreCase)))
                        .Any(x => x.Value.Equals(sku, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        PlannedEdition edition = new PlannedEdition();
                        edition.EditionName = sku;
                        edition.AvailabilityType = AvailabilityType.EditionPackageSwap;

                        availableBasisHackedEditions.Add(edition);
                    }
                }
            }

            foreach (var file in editionSpecificPackages)
            {
                var sku = file.Split('-')[3];
                if (!availableCanonicalEditions.Any(x => x.EditionName.Equals(sku, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (!editionTargetMapping
                        .Where(x => availableCanonicalEditions.Any(y => y.EditionName.Equals(x.Key, StringComparison.InvariantCultureIgnoreCase)) ||
                                    hackEditions.Any(y => y.EditionName.Equals(x.Key, StringComparison.InvariantCultureIgnoreCase)))
                        .Any(x => x.Value.Equals(sku, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        PlannedEdition edition = new PlannedEdition();
                        edition.EditionName = sku;
                        edition.AvailabilityType = AvailabilityType.EditionPackageSwap;

                        hackEditions.Add(edition);
                    }
                }
            }

            foreach (var ed in availableCanonicalEditions)
            {
                EditionTargets.Add(BuildTarget(ed, hackEditions, virtualWindowsEditions, editionTargetMapping, availableBasisHackedEditions));
            }

            error:
            return result;
        }
    }
}