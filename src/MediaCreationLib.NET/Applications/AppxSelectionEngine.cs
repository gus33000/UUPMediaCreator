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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MediaCreationLib.Applications
{
    public static class AppxSelectionEngine
    {
        public static void GetAppxDismCommands(CompDB.CompDBXmlClass.CompDB editionCdb, CompDB.CompDBXmlClass.CompDB appsCdb, string repositoryPath)
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

            string editionLanguage = editionCdb.Tags.Tag
                .First(x => x.Name == "Language").Value;

            IEnumerable<string> applicableLanguageTags = editionLanguage.Split('-').Combinations().Select(x => string.Join("-", x));

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
                    File.WriteAllText(Path.Combine(repositoryPath, "Licenses", appFeatureId + "_License.xml"), licenseData);
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
                    Path = pCanonical.Path.Replace(@"UUP\Desktop\Apps\IPA\", ""),
                    SHA256 = pCanonical.PayloadHash
                };
            }

            foreach (KeyValuePair<string, PackageProperties> x in packageHashDict)
            {
                Console.WriteLine("{ \"" + x.Value.SHA256 + "\", @\"" + x.Value.Path + "\" },");
            }

            foreach (KeyValuePair<string, DeploymentProperties> deployKvp in preinstalledApps.Where(x => !x.Value.IsFramework))
            {
                DeploymentProperties deployProps = deployKvp.Value;
                string deployCommand = "dism /image:mount /add-provisionedappxpackage /packagepath:\"";
                deployCommand += packageHashDict[deployProps.MainPackageID].Path + "\"";
                if (deployProps.Dependencies != null)
                {
                    foreach (string dependency in deployProps.Dependencies)
                    {
                        DeploymentProperties dependProps = preinstalledApps[dependency];
                        deployCommand += " /dependencypackagepath:\"";
                        deployCommand += packageHashDict[dependProps.MainPackageID].Path + "\"";
                    }
                }
                if (deployProps.HasLicense)
                {
                    deployCommand += " /licensepath:\"Licenses\\" + deployKvp.Key + "_License.xml\"";
                }

                if (deployProps.PreferStub)
                {
                    deployCommand += " /stubpackageoption:installstub";
                }

                Console.WriteLine(deployCommand);
            }
        }
    }
}
