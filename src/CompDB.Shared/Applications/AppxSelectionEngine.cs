/*
 * Copyright (c) The UUP Media Creator authors and Contributors
 * 
 * Written by and used with permission from
 * @thebookisclosed (https://github.com/thebookisclosed)
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MediaCreationLib.Planning.Applications
{
    public static class AppxSelectionEngine
    {
        /// <summary>
        /// Internal Variable setup function
        /// </summary>
        /// <param name="editionCdb"></param>
        /// <param name="appsCdb"></param>
        /// <param name="repositoryPath"></param>
        /// <returns></returns>
        private static (Dictionary<string, DeploymentProperties> preinstalledApps, CompDB.CompDBXmlClass.Feature[] appsFeatures) SetupVariables(CompDB.CompDBXmlClass.CompDB editionCdb, CompDB.CompDBXmlClass.CompDB appsCdb)
        {
            Dictionary<string, DeploymentProperties> preinstalledApps = editionCdb.Features.Feature
                .First(x => x.Type == "DesktopMedia")
                .Dependencies
                .Feature
                .Where(x => x.Group == "PreinstalledApps")
                .Select(x => x.FeatureID)
                .Distinct()
                .ToDictionary(x => x, _ => new DeploymentProperties());

            CompDB.CompDBXmlClass.Feature[] appsFeatures = appsCdb.Features.Feature;

            // Load dependencies & intents
            foreach (CompDB.CompDBXmlClass.Feature ftr in appsFeatures)
            {
                string appFeatureId = ftr.FeatureID;
                if (!preinstalledApps.ContainsKey(appFeatureId))
                {
                    continue;
                }

                DeploymentProperties deployProps = preinstalledApps[appFeatureId];

                bool preferStub = ftr.InitialIntents?.InitialIntent
                    .Any(x => x.Value == "PREFERSTUB") ?? false;

                if (preferStub)
                {
                    deployProps.PreferStub = true;
                }

                List<CompDB.CompDBXmlClass.Feature> dependencies = ftr.Dependencies?.Feature;
                if (dependencies == null)
                {
                    continue;
                }

                HashSet<string> depsForApp = new();
                foreach (CompDB.CompDBXmlClass.Feature dep in dependencies)
                {
                    string depAppId = dep.FeatureID;
                    if (!preferStub)
                    {
                        preinstalledApps[depAppId] = new DeploymentProperties();
                        depsForApp.Add(depAppId);
                    }
                    else if (depAppId.StartsWith("Microsoft.VCLibs.140.00_"))
                    {
                        preinstalledApps[depAppId] = new DeploymentProperties();
                        depsForApp.Add(depAppId);
                        break;
                    }
                }
                preinstalledApps[appFeatureId].Dependencies = depsForApp;
            }

            return (preinstalledApps, appsFeatures);
        }

        /// <summary>
        /// Generates License XML files for AppX packages
        /// </summary>
        /// <param name="editionCdb">The edition Composition Database to generate licenses for</param>
        /// <param name="appsCdb">The application Composition Database</param>
        /// <param name="repositoryPath">The path to the repository file set</param>
        public static void GenerateLicenseXmlFiles(CompDB.CompDBXmlClass.CompDB editionCdb, CompDB.CompDBXmlClass.CompDB appsCdb, string repositoryPath)
        {
            (Dictionary<string, DeploymentProperties> preinstalledApps, CompDB.CompDBXmlClass.Feature[] appsFeatures) = SetupVariables(editionCdb, appsCdb);

            // Pick packages and dump licenses
            foreach (CompDB.CompDBXmlClass.Feature ftr in appsFeatures)
            {
                string appFeatureId = ftr.FeatureID;
                if (!preinstalledApps.ContainsKey(appFeatureId))
                {
                    continue;
                }

                string licenseData = ftr.CustomInformation?.CustomInfo
                    .FirstOrDefault(x => x.Key == "licensedata")?.Value;
                if (licenseData != null)
                {
                    string LicenseDirectory = Path.Combine(repositoryPath, "Licenses");
                    if (!Directory.Exists(LicenseDirectory))
                    {
                        Directory.CreateDirectory(LicenseDirectory);
                    }
                    File.WriteAllText(Path.Combine(LicenseDirectory, appFeatureId + "_License.xml"), licenseData);
                }
            }
        }

        /// <summary>
        /// Gathers a list of AppX files to install for a target edition
        /// </summary>
        /// <param name="editionCdb">The edition Composition Database to generate workloads for</param>
        /// <param name="appsCdb">The application Composition Database</param>
        /// <param name="repositoryPath">The path to the repository file set</param>
        /// <returns></returns>
        public static AppxInstallWorkload[] GetAppxInstallationWorkloads(CompDB.CompDBXmlClass.CompDB editionCdb, CompDB.CompDBXmlClass.CompDB appsCdb)
        {
            List<AppxInstallWorkload> workloads = new();

            string editionLanguage = editionCdb.Tags.Tag
                .First(x => x.Name == "Language").Value;

            IEnumerable<string> applicableLanguageTags = editionLanguage.Split('-').Combinations().Select(x => string.Join("-", x));

            (Dictionary<string, DeploymentProperties> preinstalledApps, CompDB.CompDBXmlClass.Feature[] appsFeatures) = SetupVariables(editionCdb, appsCdb);

            HashSet<string> allPackageIDs = new();
            // Pick packages and dump licenses
            foreach (CompDB.CompDBXmlClass.Feature ftr in appsFeatures)
            {
                string appFeatureId = ftr.FeatureID;
                if (!preinstalledApps.ContainsKey(appFeatureId))
                {
                    continue;
                }

                DeploymentProperties deployProps = preinstalledApps[appFeatureId];

                bool isFramework = ftr.Type == "MSIXFramework";
                if (isFramework)
                {
                    deployProps.IsFramework = true;
                }

                string licenseData = ftr.CustomInformation?.CustomInfo
                    .FirstOrDefault(x => x.Key == "licensedata")?.Value;
                if (licenseData != null)
                {
                    deployProps.HasLicense = true;
                }

                List<CompDB.CompDBXmlClass.Package> appPackages = ftr.Packages.Package;
                deployProps.AddApplicablePackages(appPackages, applicableLanguageTags);

                foreach (string pid in deployProps.PackageIDs)
                {
                    allPackageIDs.Add(pid);
                }
            }

            Dictionary<string, PackageProperties> packageHashDict = new();
            foreach (CompDB.CompDBXmlClass.Package p in appsCdb.Packages.Package)
            {
                string packageId = p.ID;
                if (!allPackageIDs.Contains(packageId))
                {
                    continue;
                }

                CompDB.CompDBXmlClass.PayloadItem pCanonical = p
                    .Payload
                    .PayloadItem
                    .First(x => x.PayloadType == "Canonical");
                packageHashDict[packageId] = new PackageProperties()
                {
                    Path = pCanonical.Path,
                    SHA256 = pCanonical.PayloadHash
                };
            }

            foreach (KeyValuePair<string, DeploymentProperties> deployKvp in preinstalledApps.Where(x => !x.Value.IsFramework))
            {
                DeploymentProperties deployProps = deployKvp.Value;
                AppxInstallWorkload workload = new();
                workload.AppXPath = packageHashDict[deployProps.MainPackageID].Path;

                if (deployProps.Dependencies != null)
                {
                    List<string> dependencies = new();
                    foreach (string dependency in deployProps.Dependencies)
                    {
                        DeploymentProperties dependProps = preinstalledApps[dependency];
                        dependencies.Add(packageHashDict[dependProps.MainPackageID].Path);
                    }
                    workload.DependenciesPath = dependencies.ToArray();
                }
                if (deployProps.HasLicense)
                {
                    workload.LicensePath = Path.Combine("Licenses", deployKvp.Key + "_License.xml");
                }

                if (deployProps.PreferStub)
                {
                    workload.StubPackageOption = "installstub";
                }

                workloads.Add(workload);
            }

            return workloads.ToArray();
        }
    }
}
