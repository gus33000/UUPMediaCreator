using CompDB;
using Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable enable

namespace MediaCreationLib.Planning.NET
{
    public class EditionTarget
    {
        public PlannedEdition PlannedEdition { get; set; } = new PlannedEdition();
        public List<EditionTarget> NonDestructiveTargets = new List<EditionTarget>();
        public List<EditionTarget> DestructiveTargets = new List<EditionTarget>();
    }

    public class PlannedEdition
    {
        public string EditionName { get; set; } = "";
        public AvailabilityType AvailabilityType { get; set; }
    }

    public enum AvailabilityType
    {
        Canonical,
        EditionUpgrade,
        VirtualEdition,
        EditionPackageSwap
    }

    public class ConversionPlanBuilder
    {
        public delegate void ProgressCallback(string SubOperation);

        private static WIMImaging imagingInterface = new WIMImaging();

        private static EditionTarget BuildTarget(
            PlannedEdition edition,
            List<PlannedEdition> availableEditionsByDowngrading,
            List<EditionMappingXML.Edition> virtualWindowsEditions,
            List<EditionMappingXML.Edition> possibleEditionUpgrades,
            List<PlannedEdition>? availableEditionsByDowngradingInPriority,
            ref List<string> editionsAdded
        )
        {
            EditionTarget target = new EditionTarget() { PlannedEdition = edition };
            editionsAdded.Add(edition.EditionName);

            //
            // Handle edition downgrades that can be done using the current edition
            // We do these first because they potentially can be used for other downgrades, so they should be done first
            //
            if (availableEditionsByDowngradingInPriority != null)
            {
                foreach (var element in Constants.EditionDowngradeDict)
                {
                    string editionThatCanBePackageSwapped = element.Key;

                    foreach (var destinationEditionAfterPackageSwap in element.Value)
                    {
                        //
                        // Is the currently examined edition from the dictionary is possible to get with the current UUP set?
                        //
                        if (availableEditionsByDowngradingInPriority.Any(x => x.EditionName.Equals(destinationEditionAfterPackageSwap, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            //
                            // If the edition we want to target is one edition that can be used for package swapping
                            //
                            if (edition.EditionName.Equals(editionThatCanBePackageSwapped, StringComparison.InvariantCultureIgnoreCase))
                            {
                                //
                                // If we have not added this edition already
                                //
                                if (!editionsAdded.Any(x => x.Equals(destinationEditionAfterPackageSwap, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    //
                                    // Get the edition that can be targeted using package swap
                                    //
                                    var planedition = availableEditionsByDowngradingInPriority.First(x => x.EditionName.Equals(destinationEditionAfterPackageSwap, StringComparison.InvariantCultureIgnoreCase));

                                    //
                                    // Remove it from the list of editions that can be targeted, as we are targeting it here so it doesn't get picked up again
                                    //
                                    availableEditionsByDowngradingInPriority.Remove(planedition);

                                    //
                                    // Add the edition
                                    //
                                    target.DestructiveTargets.Add(BuildTarget(planedition, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, availableEditionsByDowngradingInPriority, ref editionsAdded));
                                }
                            }
                        }
                    }
                }
            }

            //
            // Handle editions that can be obtained by upgrading to a virtual edition
            // Loop through all editions we can upgrade to with the current examined edition
            //
            foreach (var ed in virtualWindowsEditions.Where(x => x.ParentEdition.Equals(edition.EditionName, StringComparison.InvariantCultureIgnoreCase)))
            {
                //
                // Verify that we have not added this edition already
                //
                if (editionsAdded.Any(x => x.Equals(ed.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                editionsAdded.Add(ed.Name);

                PlannedEdition plannedEdition = new PlannedEdition()
                {
                    AvailabilityType = AvailabilityType.VirtualEdition,
                    EditionName = ed.Name
                };

                target.NonDestructiveTargets.Add(BuildTarget(plannedEdition, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, null, ref editionsAdded));
            }

            //
            // Sort editions by name
            //
            target.NonDestructiveTargets = target.NonDestructiveTargets.OrderBy(x => x.PlannedEdition.EditionName).ToList();

            //
            // Handle editions that we can upgrade to using a full upgrade process
            //
            foreach (var ed in possibleEditionUpgrades.Where(x => x.ParentEdition.Equals(edition.EditionName, StringComparison.InvariantCultureIgnoreCase)))
            {
                //
                // Verify that we have not added this edition already
                //
                if (editionsAdded.Any(x => x.Equals(ed.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                editionsAdded.Add(ed.Name);

                PlannedEdition plannedEdition = new PlannedEdition()
                {
                    AvailabilityType = AvailabilityType.EditionUpgrade,
                    EditionName = ed.Name
                };

                target.DestructiveTargets.Add(BuildTarget(plannedEdition, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, null, ref editionsAdded));
            }

            //
            // Handle editions that can be obtained by doing a package swap
            //
            var CoreEditionsToBeHacked = availableEditionsByDowngrading.Where(x =>
            x.EditionName.StartsWith("core", StringComparison.InvariantCultureIgnoreCase) &&
            !(x.EditionName.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || x.EditionName.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase)));

            var CoreEditionsWithNoMediaTechnologiesToBeHacked = availableEditionsByDowngrading.Where(x =>
            x.EditionName.StartsWith("core", StringComparison.InvariantCultureIgnoreCase) &&
            (x.EditionName.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || x.EditionName.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase)));

            var ProfessionalEditionsToBeHacked = availableEditionsByDowngrading.Where(x =>
            !x.EditionName.StartsWith("core", StringComparison.InvariantCultureIgnoreCase) &&
            !(x.EditionName.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || x.EditionName.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase)));

            var ProfessionalEditionsWithNoMediaTechnologiesToBeHacked = availableEditionsByDowngrading.Where(x =>
            !x.EditionName.StartsWith("core", StringComparison.InvariantCultureIgnoreCase) &&
            (x.EditionName.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || x.EditionName.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase)));

            if (edition.EditionName.Equals("professional", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var ed in ProfessionalEditionsToBeHacked)
                {
                    //
                    // Verify that we have not added this edition already
                    //
                    if (editionsAdded.Any(x => x.Equals(ed.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }

                    editionsAdded.Add(ed.EditionName);

                    target.DestructiveTargets.Add(BuildTarget(ed, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, null, ref editionsAdded));
                }
            }
            else if (edition.EditionName.Equals("professionaln", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var ed in ProfessionalEditionsWithNoMediaTechnologiesToBeHacked)
                {
                    //
                    // Verify that we have not added this edition already
                    //
                    if (editionsAdded.Any(x => x.Equals(ed.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }

                    editionsAdded.Add(ed.EditionName);

                    target.DestructiveTargets.Add(BuildTarget(ed, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, null, ref editionsAdded));
                }
            }
            else if (edition.EditionName.Equals("core", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var ed in CoreEditionsToBeHacked)
                {
                    //
                    // Verify that we have not added this edition already
                    //
                    if (editionsAdded.Any(x => x.Equals(ed.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }

                    editionsAdded.Add(ed.EditionName);

                    target.DestructiveTargets.Add(BuildTarget(ed, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, null, ref editionsAdded));
                }
            }
            else if (edition.EditionName.Equals("coren", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var ed in CoreEditionsWithNoMediaTechnologiesToBeHacked)
                {
                    //
                    // Verify that we have not added this edition already
                    //
                    if (editionsAdded.Any(x => x.Equals(ed.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }

                    editionsAdded.Add(ed.EditionName);

                    target.DestructiveTargets.Add(BuildTarget(ed, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, null, ref editionsAdded));
                }
            }

            target.DestructiveTargets = target.DestructiveTargets.OrderBy(x => x.PlannedEdition.EditionName).ToList();

            return target;
        }

        private static (List<PlannedEdition>, List<PlannedEdition>) GetEditionsThatCanBeTargetedUsingPackageDowngrade(
            string UUPPath,
            HashSet<CompDBXmlClass.CompDB> compDBs,
            IEnumerable<PlannedEdition> availableCanonicalEditions,
            List<EditionMappingXML.Edition> possibleEditionUpgrades,
            ProgressCallback? progressCallback = null)
        {
            List<PlannedEdition> availableEditionsByDowngradingInPriority = new List<PlannedEdition>();
            List<PlannedEdition> availableEditionsByDowngrading = new List<PlannedEdition>();

            //
            // Attempt to get the neutral Composition Database listing all available files
            //
            CompDBXmlClass.CompDB? neutralCompDB = compDBs.GetNeutralCompDB();

            if (neutralCompDB != null)
            {
                var packages = neutralCompDB.Features.Feature.First(x => x.FeatureID == "BaseNeutral").Packages.Package;

                var editionSpecificPackages = packages.Where(x => x.ID.Count(y => y == '-') == 4).Where(x => x.ID.Contains("microsoft-windows-editionspecific", StringComparison.InvariantCultureIgnoreCase));
                var highPriorityPackages = editionSpecificPackages.Where(x => x.ID.Contains("starter", StringComparison.InvariantCultureIgnoreCase) ||
                                                                            x.ID.Contains("professional", StringComparison.InvariantCultureIgnoreCase) ||
                                                                            x.ID.Contains("core", StringComparison.InvariantCultureIgnoreCase));
                editionSpecificPackages = editionSpecificPackages.Except(highPriorityPackages);

                foreach (var file in highPriorityPackages)
                {
                    CompDBXmlClass.Package pkg = neutralCompDB.Packages.Package.First(x => x.ID == file.ID);

                    //
                    // First check if the file exists
                    //
                    if (!string.IsNullOrEmpty(UUPPath))
                    {
                        (bool Success, string MissingFile) = FileLocator.VerifyFileIsAvailableForPackage(pkg, UUPPath);
                        if (!Success)
                        {
                            progressCallback?.Invoke("One edition Composition Database failed file validation, below is highlighted the files that could not be found in the UUP path. This means that you will not get all possible editions.");
                            progressCallback?.Invoke($"Missing: {MissingFile}");
                            continue;
                        }
                    }

                    var sku = file.ID.Split('-')[3];

                    //
                    // If the edition not available as canonical
                    //
                    if (!availableCanonicalEditions.Any(x => x.EditionName.Equals(sku, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (possibleEditionUpgrades != null && !possibleEditionUpgrades
                            .Where(x => availableCanonicalEditions.Any(y => y.EditionName.Equals(x.ParentEdition, StringComparison.InvariantCultureIgnoreCase)) ||
                                        availableEditionsByDowngrading.Any(y => y.EditionName.Equals(x.ParentEdition, StringComparison.InvariantCultureIgnoreCase)))
                            .Any(x => x.Name.Equals(sku, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            PlannedEdition edition = new PlannedEdition
                            {
                                EditionName = sku,
                                AvailabilityType = AvailabilityType.EditionPackageSwap
                            };

                            availableEditionsByDowngradingInPriority.Add(edition);
                        }
                    }
                }

                foreach (var file in editionSpecificPackages)
                {
                    CompDBXmlClass.Package pkg = neutralCompDB.Packages.Package.First(x => x.ID == file.ID);

                    //
                    // First check if the file exists
                    //
                    if (!string.IsNullOrEmpty(UUPPath))
                    {
                        (bool Success, string MissingFile) = FileLocator.VerifyFileIsAvailableForPackage(pkg, UUPPath);
                        if (!Success)
                        {
                            progressCallback?.Invoke("One edition Composition Database failed file validation, below is highlighted the files that could not be found in the UUP path. This means that you will not get all possible editions.");
                            progressCallback?.Invoke($"Missing: {MissingFile}");
                            continue;
                        }
                    }

                    var sku = file.ID.Split('-')[3];

                    //
                    // If the edition not available as canonical
                    //
                    if (!availableCanonicalEditions.Any(x => x.EditionName.Equals(sku, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (possibleEditionUpgrades != null && !possibleEditionUpgrades
                            .Where(x => availableCanonicalEditions.Any(y => y.EditionName.Equals(x.ParentEdition, StringComparison.InvariantCultureIgnoreCase)) ||
                                        availableEditionsByDowngrading.Any(y => y.EditionName.Equals(x.ParentEdition, StringComparison.InvariantCultureIgnoreCase)))
                            .Any(x => x.Name.Equals(sku, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            PlannedEdition edition = new PlannedEdition
                            {
                                EditionName = sku,
                                AvailabilityType = AvailabilityType.EditionPackageSwap
                            };

                            availableEditionsByDowngrading.Add(edition);
                        }
                    }
                }
            }

            return (availableEditionsByDowngradingInPriority, availableEditionsByDowngrading);
        }

        public static bool GetTargetedPlan(
            HashSet<CompDBXmlClass.CompDB> compDBs,
            string EditionPack,
            string LanguageCode,
            bool IncludeServicingCapableOnlyTargets,
            out List<EditionTarget> EditionTargets,
            ProgressCallback? progressCallback = null)
        {
            return GetTargetedPlan("", compDBs, EditionPack, LanguageCode, IncludeServicingCapableOnlyTargets, out EditionTargets, progressCallback);
        }

        public static List<string> PrintEditionTarget(EditionTarget editionTarget, int padding = 0)
        {
            List<string> lines = new List<string>();
            lines.Add($"-> Name: {editionTarget.PlannedEdition.EditionName}, Availability: {editionTarget.PlannedEdition.AvailabilityType}");
            if (editionTarget.NonDestructiveTargets.Count > 0)
            {
                lines.Add("   Non Destructive Edition Upgrade Targets:");
                foreach (var ed in editionTarget.NonDestructiveTargets)
                {
                    lines.AddRange(PrintEditionTarget(ed, padding + 1));
                }
            }
            if (editionTarget.DestructiveTargets.Count > 0)
            {
                lines.Add("   Destructive Edition Upgrade Targets:");
                foreach (var ed in editionTarget.DestructiveTargets)
                {
                    lines.AddRange(PrintEditionTarget(ed, padding + 1));
                }
            }

            for (int j = 0; j < lines.Count; j++)
            {
                for (int i = 0; i < padding; i++)
                {
                    lines[j] = "   " + lines[j];
                }
            }

            return lines;
        }

        public static bool GetTargetedPlan(
            string UUPPath,
            HashSet<CompDBXmlClass.CompDB> compDBs,
            string EditionPack,
            string LanguageCode,
            bool IncludeServicingCapableOnlyTargets,
            out List<EditionTarget> EditionTargets,
            ProgressCallback? progressCallback = null)
        {
            bool VerifyFiles = !string.IsNullOrEmpty(UUPPath);

            EditionTargets = new List<EditionTarget>();

            bool result = true;

            progressCallback?.Invoke("Acquiring Base Editions");

            //
            // Get base editions that are available with all their files
            //
            IEnumerable<CompDBXmlClass.CompDB> filteredCompDBs = compDBs.GetEditionCompDBsForLanguage(LanguageCode).Where(x =>
            {
                bool success = !VerifyFiles;
                if (!success)
                {
                    (bool success2, HashSet<string> missingfiles) = FileLocator.VerifyFilesAreAvailableForCompDB(x, UUPPath);
                    success = success2;

                    if (!success)
                    {
                        progressCallback?.Invoke("One edition Composition Database failed file validation, below is highlighted the files that could not be found in the UUP path. This means that you will not get all possible editions.");
                        foreach (var file in missingfiles)
                        {
                            progressCallback?.Invoke($"Missing: {file}");
                        }
                    }
                }

                return success;
            });

            if (filteredCompDBs.Count() == 0)
            {
                progressCallback?.Invoke("No edition CompDB validated or were found, this is a fatal error, as one edition CompDB is needed at the very least for creating an ISO.");
                goto error;
            }

            progressCallback?.Invoke("Adding Base Editions to the conversion plan");

            //
            // Add available canonical editions
            //
            IEnumerable<PlannedEdition> availableCanonicalEditions = filteredCompDBs.Select(compDB =>
            {
                PlannedEdition edition = new PlannedEdition()
                {
                    AvailabilityType = AvailabilityType.Canonical
                };

                if (compDB.Tags != null)
                {
                    edition.EditionName = compDB.Tags.Tag.First(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)).Value;
                }
                else
                {
                    edition.EditionName = compDB.Features.Feature[0].FeatureID.Split('_')[0];
                }

                return edition;
            }).OrderBy(x => x.EditionName);

            if (IncludeServicingCapableOnlyTargets)
            {
                progressCallback?.Invoke("Acquiring Edition Upgrades");

                //
                // This dictionary holds the possible virtual edition upgrades
                // Example: Professional -> ProfessionalEducation
                //
                List<EditionMappingXML.Edition> virtualWindowsEditions = new List<EditionMappingXML.Edition>();

                //
                // This dictionary holds the possible edition upgrades
                // Example: Core -> Professional
                //
                List<EditionMappingXML.Edition> possibleEditionUpgrades = new List<EditionMappingXML.Edition>();

                if (!string.IsNullOrEmpty(EditionPack) && File.Exists(EditionPack))
                {
                    //
                    // Attempt to get virtual editions from one compdb
                    //
                    if (virtualWindowsEditions.Count <= 0)
                    {
                        try
                        {
                            var tempHashMap = Path.GetTempFileName();
                            File.Delete(tempHashMap);

                            string pathEditionMapping = "";
                            int index = 0;

                            result = imagingInterface.ExtractFileFromImage(EditionPack, 1, "$filehashes$.dat", tempHashMap);
                            if (result)
                            {
                                string[] hashmapcontent = File.ReadAllLines(tempHashMap);
                                File.Delete(tempHashMap);
                                pathEditionMapping = hashmapcontent.First(x => x.ToLower().Contains("editionmappings.xml")).Split('=').First();
                                index = 1;
                            }
                            else
                            {
                                result = true;
                                pathEditionMapping = "Windows\\Servicing\\Editions\\EditionMappings.xml";
                                index = 3;
                            }

                            try
                            {
                                result = imagingInterface.ExtractFileFromImage(EditionPack, index, pathEditionMapping, tempHashMap);
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

                    //
                    // Attempt to get upgradable editions from one compdb
                    //
                    if (possibleEditionUpgrades.Count <= 0)
                    {
                        try
                        {
                            var tempHashMap = Path.GetTempFileName();
                            File.Delete(tempHashMap);

                            string pathEditionMatrix = "";
                            int index = 0;

                            result = imagingInterface.ExtractFileFromImage(EditionPack, 1, "$filehashes$.dat", tempHashMap);
                            if (result)
                            {
                                string[] hashmapcontent = File.ReadAllLines(tempHashMap);
                                File.Delete(tempHashMap);
                                pathEditionMatrix = hashmapcontent.First(x => x.ToLower().Contains("editionmatrix.xml")).Split('=').First();
                                index = 1;
                            }
                            else
                            {
                                result = true;
                                pathEditionMatrix = "Windows\\Servicing\\Editions\\EditionMatrix.xml";
                                index = 3;
                            }

                            try
                            {
                                result = imagingInterface.ExtractFileFromImage(EditionPack, index, pathEditionMatrix, tempHashMap);
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
                                                possibleEditionUpgrades.Add(new EditionMappingXML.Edition() { ParentEdition = edition.ID, Name = target.ID, Virtual = "false" });
                                            }
                                        }
                                    }
                                }
                            }
                            catch { };
                        }
                        catch { };
                    }
                }

                progressCallback?.Invoke("Acquiring Edition Downgrades");

                (List<PlannedEdition> availableEditionsByDowngradingInPriority, List<PlannedEdition> availableEditionsByDowngrading) = GetEditionsThatCanBeTargetedUsingPackageDowngrade(UUPPath, compDBs, availableCanonicalEditions, possibleEditionUpgrades);

                progressCallback?.Invoke("Building Targets");


                List<string> editionsAdded = new List<string>();

                foreach (var ed in availableCanonicalEditions)
                {
                    EditionTargets.Add(BuildTarget(ed, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, availableEditionsByDowngradingInPriority, ref editionsAdded));
                }
            }
            else
            {
                progressCallback?.Invoke("Building Targets");

                foreach (var ed in availableCanonicalEditions)
                {
                    EditionTargets.Add(new EditionTarget() { PlannedEdition = ed });
                }
            }

        error:
            return result;
        }
    }
}