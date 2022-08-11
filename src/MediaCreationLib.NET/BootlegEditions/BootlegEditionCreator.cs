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
using Cabinet;
using Imaging.NET;
using IniParser;
using IniParser.Model;
using MediaCreationLib.NET.Settings;
using Microsoft.Wim.NET;
using System;
using System.IO;
using System.Linq;
using UUPMediaCreator.InterCommunication;

namespace MediaCreationLib.NET.BootlegEditions
{
    public static class BootlegEditionCreator
    {
        private static string LPFolder = null;

        public static void CleanupLanguagePackFolderIfRequired()
        {
            if (!string.IsNullOrEmpty(LPFolder) && Directory.Exists(LPFolder))
            {
                Directory.Delete(LPFolder, true);
            }
            LPFolder = null;
        }

        /*private static void ProvisionMissingApps()
        {
            string SourceEdition = "Professional";
            string TargetEdition = "PPIPro";
            string UUPPath = @"C:\Users\Gus\Downloads\19635.1_amd64_en-us_multi_6d892fb5\UUPs";

            CompDBXmlClass.CompDB neutralCompDB = null;

            if (Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*").Count() > 0)
            {
                using (CabinetHandler cabinet = new CabinetHandler(File.OpenRead(Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*").First())))
                {
                    IEnumerable<string> potentialNeutralFiles = cabinet.Files.Where(x =>
                    x.ToLower().Contains($"desktoptargetcompdb_neutral"));

                    if (potentialNeutralFiles.Count() == 0)
                        goto exit;

                    using (CabinetHandler cabinet2 = new CabinetHandler(cabinet.OpenFile(potentialNeutralFiles.First())))
                    {
                        string xmlfile = cabinet2.Files.First();
                        using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                        {
                            neutralCompDB = CompDBXmlClass.DeserializeCompDB(xmlstream);
                        }
                    }
                }
            }
            else
            {
                IEnumerable<string> files = Directory.EnumerateFiles(UUPPath).Select(x => Path.GetFileName(x));

                IEnumerable<string> potentialNeutralFiles = files.Where(x =>
                       x.ToLower().Contains($"desktoptargetcompdb_neutral"));

                if (potentialNeutralFiles.Count() == 0)
                    goto exit;

                using (CabinetHandler cabinet2 = new CabinetHandler(File.OpenRead(Path.Combine(UUPPath, potentialNeutralFiles.First()))))
                {
                    string xmlfile = cabinet2.Files.First();
                    using (Stream xmlstream = cabinet2.OpenFile(xmlfile))
                    {
                        neutralCompDB = CompDBXmlClass.DeserializeCompDB(xmlstream);
                    }
                }
            }

            var apppackages = neutralCompDB.Features.Feature.First(x => x.FeatureID == "BaseNeutral").Packages.Package.Select(x => x.ID.Split('_').Last()).Where(x => x.StartsWith("Microsoft.ModernApps.", StringComparison.InvariantCultureIgnoreCase));

            bool sourceneedsclientpack = !SourceEdition.StartsWith("enterpriseg", StringComparison.InvariantCultureIgnoreCase) && !SourceEdition.StartsWith("ppipro", StringComparison.InvariantCultureIgnoreCase);
            bool sourceneedsclientnpack = SourceEdition.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || SourceEdition.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase);

            bool targetneedsclientpack = !TargetEdition.StartsWith("enterpriseg", StringComparison.InvariantCultureIgnoreCase) && !SourceEdition.StartsWith("ppipro", StringComparison.InvariantCultureIgnoreCase);
            bool targetneedsclientnpack = TargetEdition.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || TargetEdition.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase);

            List<string> SourceEditionApps = new List<string>();
            List<string> TargetEditionApps = new List<string>();

            var potentialSourceEditionAppPackages = apppackages.Where(x => x.ToLower().Contains(SourceEdition.ToLower().TrimEnd('n')));
            if (potentialSourceEditionAppPackages.Count() > 0)
            {
                var apppackage = potentialSourceEditionAppPackages.First();
                string[] entries;
                Constants.imagingInterface.EnumerateFiles(Path.Combine(UUPPath, apppackage + ".esd"), 1, "", out entries);
                foreach (var entry in entries)
                {
                    if (entry.ToLower() != "$filehashes$.dat")
                    {
                        string[] entries2;
                        Constants.imagingInterface.EnumerateFiles(Path.Combine(UUPPath, apppackage + ".esd"), 1, entry, out entries2);
                        var pkg = entries2.Where(x => x.ToLower() != "appxsignature.p7x" && x.ToLower() != "appxblockmap.xml" && x.ToLower() != "appxmetadata").First();
                        var newpkg = pkg.Split('_')[0] + "_" + pkg.Split('_')[1] + "_neutral_~_" + pkg.Split('_')[4];
                        bool isbundle = entries2.Any(x => x.ToLower() == "appxmetadata");
                        SourceEditionApps.Add($"{entry}|{pkg}|{pkg.Split('_')[0]}|{newpkg}|{isbundle}");
                    }
                }
            }

            if (sourceneedsclientpack)
            {
                string package = "microsoft.modernapps.client.all.esd";
                if (sourceneedsclientnpack)
                {
                    package = "microsoft.modernapps.clientn.all.esd";
                }

                string[] entries;
                Constants.imagingInterface.EnumerateFiles(Path.Combine(UUPPath, package), 1, "", out entries);
                foreach (var entry in entries)
                {
                    if (entry.ToLower() != "$filehashes$.dat")
                    {
                        string[] entries2;
                        Constants.imagingInterface.EnumerateFiles(Path.Combine(UUPPath, package), 1, entry, out entries2);
                        var pkg = entries2.Where(x => x.ToLower() != "appxsignature.p7x" && x.ToLower() != "appxblockmap.xml" && x.ToLower() != "appxmetadata").First();
                        var newpkg = pkg.Split('_')[0] + "_" + pkg.Split('_')[1] + "_neutral_~_" + pkg.Split('_')[4];
                        bool isbundle = entries2.Any(x => x.ToLower() == "appxmetadata");
                        SourceEditionApps.Add($"{entry}|{pkg}|{pkg.Split('_')[0]}|{newpkg}|{isbundle}");
                    }
                }
            }

            var potentialTargetEditionAppPackages = apppackages.Where(x => x.ToLower().Contains(TargetEdition.ToLower().TrimEnd('n')));
            if (potentialTargetEditionAppPackages.Count() > 0)
            {
                var apppackage = potentialTargetEditionAppPackages.First();
                string[] entries;
                Constants.imagingInterface.EnumerateFiles(Path.Combine(UUPPath, apppackage + ".esd"), 1, "", out entries);
                foreach (var entry in entries)
                {
                    if (entry.ToLower() != "$filehashes$.dat")
                    {
                        string[] entries2;
                        Constants.imagingInterface.EnumerateFiles(Path.Combine(UUPPath, apppackage + ".esd"), 1, entry, out entries2);
                        var pkg = entries2.Where(x => x.ToLower() != "appxsignature.p7x" && x.ToLower() != "appxblockmap.xml" && x.ToLower() != "appxmetadata").First();
                        var newpkg = pkg.Split('_')[0] + "_" + pkg.Split('_')[1] + "_neutral_~_" + pkg.Split('_')[4];
                        bool isbundle = entries2.Any(x => x.ToLower() == "appxmetadata");
                        TargetEditionApps.Add($"{entry}|{pkg}|{pkg.Split('_')[0]}|{newpkg}|{isbundle}");
                    }
                }
            }

            if (targetneedsclientpack)
            {
                string package = "microsoft.modernapps.client.all.esd";
                if (targetneedsclientnpack)
                {
                    package = "microsoft.modernapps.clientn.all.esd";
                }

                string[] entries;
                Constants.imagingInterface.EnumerateFiles(Path.Combine(UUPPath, package), 1, "", out entries);
                foreach (var entry in entries)
                {
                    if (entry.ToLower() != "$filehashes$.dat")
                    {
                        string[] entries2;
                        Constants.imagingInterface.EnumerateFiles(Path.Combine(UUPPath, package), 1, entry, out entries2);
                        var pkg = entries2.Where(x => x.ToLower() != "appxsignature.p7x" && x.ToLower() != "appxblockmap.xml" && x.ToLower() != "appxmetadata").First();
                        var newpkg = pkg.Split('_')[0] + "_" + pkg.Split('_')[1] + "_neutral_~_" + pkg.Split('_')[4];
                        bool isbundle = entries2.Any(x => x.ToLower() == "appxmetadata");
                        TargetEditionApps.Add($"{entry}|{pkg}|{pkg.Split('_')[0]}|{newpkg}|{isbundle}");
                    }
                }
            }

            SourceEditionApps.Sort();
            TargetEditionApps.Sort();

            Console.WriteLine("Source apps (" + SourceEdition + ")");
            foreach (var app in SourceEditionApps)
            {
                Console.WriteLine(app);
            }
            Console.WriteLine();

            Console.WriteLine("Target apps (" + TargetEdition + ")");
            foreach (var app in TargetEditionApps)
            {
                Console.WriteLine(app);
            }
            Console.WriteLine();

            var common = TargetEditionApps.Intersect(SourceEditionApps);

            Console.WriteLine("The following apps must be uninstalled");
            foreach (var app in SourceEditionApps.Except(common))
            {
                Console.WriteLine(app);
            }
            Console.WriteLine();

            Console.WriteLine("The following apps must be installed");
            foreach (var app in TargetEditionApps.Except(common))
            {
                Console.WriteLine(app);
            }
            Console.WriteLine();

        exit:
            return;
        }*/

        public static bool CreateHackedEditionFromMountedImage(
            string UUPPath,
            string MediaPath,
            string MountedImagePath,
            string EditionID,
            string OutputInstallImage,
            Common.CompressionType CompressionType,
            TempManager.TempManager tempManager,
            ProgressCallback progressCallback = null)
        {
            bool result = true;
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Applying " + EditionID + " - Package Swap");

            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Getting serial for " + EditionID);
            string productinifilepath = Path.Combine(MediaPath, "sources", "product.ini");

            FileIniDataParser parser = new();
            IniData data = parser.ReadFile(productinifilepath);

            string serial = data["cmi"].First(x => string.Equals(x.KeyName, EditionID, StringComparison.CurrentCultureIgnoreCase)).Value;
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Serial: " + serial);

            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Getting current edition");
            string SourceEdition = DismOperations.NET.DismOperations.Instance.GetCurrentEdition(MountedImagePath);
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Current edition is: " + SourceEdition);

            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Getting wim info for: " + OutputInstallImage);
            result = Constants.imagingInterface.GetWIMInformation(OutputInstallImage, out WIMInformationXML.WIM wiminfo);
            if (!result)
            {
                goto exit;
            }

            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Searching index for : " + SourceEdition);
            WIMInformationXML.IMAGE srcimage = wiminfo.IMAGE.First(x =>
            {
                WIMInformationXML.IMAGE img = x;
                WIMInformationXML.WINDOWS win = img.WINDOWS;
                string ed = win.EDITIONID;
                return ed.Equals(SourceEdition, StringComparison.InvariantCultureIgnoreCase);
            });
            int index = int.Parse(srcimage.INDEX);
            string languagecode = srcimage.WINDOWS.LANGUAGES.DEFAULT;
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Source index: " + index);
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Source language: " + languagecode);

            WimCompressionType compression = WimCompressionType.None;
            switch (CompressionType)
            {
                case Common.CompressionType.LZMS:
                    compression = WimCompressionType.Lzms;
                    break;

                case Common.CompressionType.LZX:
                    compression = WimCompressionType.Lzx;
                    break;

                case Common.CompressionType.XPRESS:
                    compression = WimCompressionType.Xpress;
                    break;
            }

            //
            // Perform edition hack
            //
            string servicingPath = Path.Combine(MountedImagePath, "Windows", "servicing", "Packages");
            string manifest = $"Microsoft-Windows-{SourceEdition}Edition~31bf3856ad364e35~*~~10.0.*.*.mum";
            string catalog = $"Microsoft-Windows-{SourceEdition}Edition~31bf3856ad364e35~*~~10.0.*.*.cat";
            string manifestPath = Directory.EnumerateFiles(servicingPath, manifest, new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive }).First();
            string catalogPath = Directory.EnumerateFiles(servicingPath, catalog, new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive }).First();

            bool LTSB = false;
            if (EditionID.StartsWith("enterpriseg", StringComparison.CurrentCultureIgnoreCase) || EditionID.StartsWith("enterprises", StringComparison.CurrentCultureIgnoreCase) || EditionID.StartsWith("iotenterprises", StringComparison.CurrentCultureIgnoreCase))
            {
                LTSB = true;
            }

            bool HasEditionPack = Directory.EnumerateFiles(UUPPath, "*.esd", SearchOption.AllDirectories).Any(x =>
                Path.GetFileName(x).Equals($"microsoft-windows-editionpack-{EditionID}-package.esd", StringComparison.InvariantCultureIgnoreCase)
            );
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Has edition pack: " + HasEditionPack);

            string TemporaryFolder = tempManager.GetTempPath();
            _ = Directory.CreateDirectory(TemporaryFolder);

            string SxSFolder = Path.Combine(TemporaryFolder, "SxS");
            _ = Directory.CreateDirectory(SxSFolder);

            //
            // Build reconstructed edition xml
            //
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Generating edition manifest");
            string packageFilter = $"microsoft-windows-edition*{EditionID}-*.esd";
            System.Collections.Generic.IEnumerable<string> packages = Directory.EnumerateFiles(UUPPath, packageFilter, SearchOption.AllDirectories);

            string manifestFileName = Path.GetFileName(manifestPath).Replace(SourceEdition, EditionID);
            string catalogFileName = Path.GetFileName(catalogPath).Replace(SourceEdition, EditionID);

            string newManifestPath = Path.Combine(SxSFolder, manifestFileName);
            string newCatalogPath = Path.Combine(SxSFolder, catalogFileName);

            File.Copy(manifestPath, newManifestPath, true);
            File.Copy(catalogPath, newCatalogPath, true);

            string ManifestContent = File.ReadAllText(newManifestPath);
            ManifestContent = ManifestContent.Replace($"EditionSpecific-{SourceEdition}", $"EditionSpecific-{EditionID}").Replace($"Windows {SourceEdition} Edition", $"Windows {EditionID} Edition").Replace($"Microsoft-Windows-{SourceEdition}Edition", $"Microsoft-Windows-{EditionID}Edition");

            if (HasEditionPack)
            {
                ManifestContent = ManifestContent.Replace($"EditionPack-{SourceEdition}", $"EditionPack-{EditionID}");
            }

            File.WriteAllText(newManifestPath, ManifestContent);

            if (LTSB)
            {
                AssemblyManifestHandler.RemoveNonLTSBPackages(newManifestPath);
            }

            // Cleanup WOW64
            if (!packages.Any(x => x.Equals($"microsoft-windows-editionspecific-{EditionID}-wow64-package.esd", StringComparison.InvariantCultureIgnoreCase)))
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Cleaning up WOW64");
                AssemblyManifestHandler.RemoveWOW64Package(newManifestPath, $"microsoft-windows-editionspecific-{EditionID}-wow64-package");
            }

            //
            // Expand LP to folder
            //
            if (LPFolder == null)
            {
                LPFolder = tempManager.GetTempPath();
                _ = Directory.CreateDirectory(LPFolder);

                string lpfilter1 = $"*fre_client_{languagecode}_lp.cab";
                System.Collections.Generic.IEnumerable<string> paths = Directory.EnumerateFiles(UUPPath, lpfilter1, SearchOption.AllDirectories);
                if (paths.Any())
                {
                    string lppackage = paths.First();
                    void ProgressCallback(int percent, string file)
                    {
                        progressCallback?.Invoke(Common.ProcessPhase.PreparingFiles, false, percent, "Unpacking " + file + "...");
                    }

                    CabinetExtractor.ExtractCabinet(lppackage, LPFolder, ProgressCallback);
                }
                else
                {
                    string lppackage = "";

                    string lpfilter2 = $"*fre_client_{languagecode}_lp.esd";
                    if (!Directory.EnumerateFiles(UUPPath, lpfilter2, SearchOption.AllDirectories).Any())
                    {
                        lpfilter2 = $"microsoft-windows-client-languagepack-package_{languagecode}-*-{languagecode}.esd";
                        if (!Directory.EnumerateFiles(UUPPath, lpfilter2, SearchOption.AllDirectories).Any())
                        {
                            lpfilter2 = $"microsoft-windows-client-languagepack-package_{languagecode}~*~{languagecode}~.esd";
                            if (!Directory.EnumerateFiles(UUPPath, lpfilter2, SearchOption.AllDirectories).Any())
                            {
                                progressCallback?.Invoke(Common.ProcessPhase.Error, true, 0, "Unable to find LP package!");
                                goto exit;
                            }
                        }
                    }

                    lppackage = Directory.EnumerateFiles(UUPPath, lpfilter2, SearchOption.AllDirectories).First();

                    result = Constants.imagingInterface.ApplyImage(lppackage, 1, LPFolder, PreserveACL: false, progressCallback: callback2);
                    if (!result)
                    {
                        goto exit;
                    }
                }
            }

            //
            // Expand Edition related packages to SxS folder
            //
            foreach (string package in packages)
            {
                result = Constants.imagingInterface.ApplyImage(package, 1, SxSFolder, PreserveACL: false, progressCallback: callback2);
                if (!result)
                {
                    goto exit;
                }

                if (File.Exists(Path.Combine(SxSFolder, "update.mum")))
                {
                    Assembly assembly = AssemblyManifestHandler.Deserialize(File.ReadAllText(Path.Combine(SxSFolder, "update.mum")));
                    string cbsKey = assembly.AssemblyIdentity.Name + "~" + assembly.AssemblyIdentity.PublicKeyToken + "~" + assembly.AssemblyIdentity.ProcessorArchitecture + "~" + (string.Equals(assembly.AssemblyIdentity.Language, "neutral", StringComparison.CurrentCultureIgnoreCase) ? "" : assembly.AssemblyIdentity.Language) + "~" + assembly.AssemblyIdentity.Version;
                    if (!File.Exists(Path.Combine(SxSFolder, cbsKey + ".mum")))
                    {
                        File.Move(Path.Combine(SxSFolder, "update.mum"), Path.Combine(SxSFolder, cbsKey + ".mum"));
                    }
                    else
                    {
                        File.Delete(Path.Combine(SxSFolder, "update.mum"));
                    }

                    if (File.Exists(Path.Combine(SxSFolder, "update.cat")))
                    {
                        if (!File.Exists(Path.Combine(SxSFolder, cbsKey + ".cat")))
                        {
                            File.Move(Path.Combine(SxSFolder, "update.cat"), Path.Combine(SxSFolder, cbsKey + ".cat"));
                        }
                        else
                        {
                            File.Delete(Path.Combine(SxSFolder, "update.cat"));
                        }
                    }
                }
                File.Delete(Path.Combine(SxSFolder, "$filehashes$.dat"));
            }

            //
            // Generate unattend
            //
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Generating unattend");
            string arch = manifestFileName.Split('~')[2];
            string ver = manifestFileName.Split('~')[4].Replace(".mum", "");

            bool removeogedition = true;

            // TODO: get these from matrix
            //
            if (SourceEdition.Equals("core", StringComparison.InvariantCultureIgnoreCase))
            {
                if (EditionID.Equals("corecountryspecific", StringComparison.InvariantCultureIgnoreCase))
                {
                    removeogedition = false;
                }
            }
            if (SourceEdition.Equals("professional", StringComparison.InvariantCultureIgnoreCase))
            {
                if (EditionID.Equals("core", StringComparison.InvariantCultureIgnoreCase))
                {
                    removeogedition = false;
                }
            }
            if (SourceEdition.Equals("professionaln", StringComparison.InvariantCultureIgnoreCase))
            {
                if (EditionID.Equals("coren", StringComparison.InvariantCultureIgnoreCase))
                {
                    removeogedition = false;
                }
            }
            if (SourceEdition.Equals("core", StringComparison.InvariantCultureIgnoreCase))
            {
                if (EditionID.Equals("starter", StringComparison.InvariantCultureIgnoreCase))
                {
                    removeogedition = false;
                }
            }
            if (SourceEdition.Equals("coren", StringComparison.InvariantCultureIgnoreCase))
            {
                if (EditionID.Equals("startern", StringComparison.InvariantCultureIgnoreCase))
                {
                    removeogedition = false;
                }
            }

            string unattend = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                "<unattend xmlns=\"urn:schemas-microsoft-com:unattend\">\n" +
                                "    <servicing>\n";

            if (!removeogedition)
            {
                unattend +=
                               "        <package action=\"stage\">\n" +
                               $"            <assemblyIdentity name=\"Microsoft-Windows-{SourceEdition}Edition\" version=\"{ver}\" processorArchitecture=\"{arch}\" publicKeyToken=\"31bf3856ad364e35\" language=\"neutral\" />\n" +
                               "        </package>\n";
            }

            unattend +=
                                "        <package action=\"stage\">\n" +
                                $"            <assemblyIdentity name=\"Microsoft-Windows-{EditionID}Edition\" version=\"{ver}\" processorArchitecture=\"{arch}\" publicKeyToken=\"31bf3856ad364e35\" language=\"neutral\" />\n" +
                                $"            <source location=\"{newManifestPath}\" />\n" +
                                "        </package>\n";

            if (removeogedition)
            {
                unattend +=
                               "        <package action=\"remove\">\n" +
                               $"            <assemblyIdentity name=\"Microsoft-Windows-{SourceEdition}Edition\" version=\"{ver}\" processorArchitecture=\"{arch}\" publicKeyToken=\"31bf3856ad364e35\" language=\"neutral\" />\n" +
                               "        </package>\n";
            }

            unattend +=
                                "        <package action=\"install\">\n" +
                                $"            <assemblyIdentity name=\"Microsoft-Windows-{EditionID}Edition\" version=\"{ver}\" processorArchitecture=\"{arch}\" publicKeyToken=\"31bf3856ad364e35\" language=\"neutral\" />\n" +
                                "        </package>\n" +
                                "        <package action=\"install\">\n" +
                                $"            <assemblyIdentity name=\"Microsoft-Windows-Client-LanguagePack-Package\" version=\"{ver}\" processorArchitecture=\"{arch}\" publicKeyToken=\"31bf3856ad364e35\" language=\"{languagecode}\" />\n" +
                                $"            <source location=\"{LPFolder}\\update.mum\" />\n" +
                                "        </package>\n" +
                                "    </servicing>\n" +
                                "</unattend>";

            string unattendPath = Path.Combine(TemporaryFolder, "unattend.xml");
            File.WriteAllText(unattendPath, unattend);

            //
            // Backup OEMDefaultAssociations
            //
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Backing up OEMDefaultAssociations");

            bool HandleOEMDefaultAssociationsXml = File.Exists(Path.Combine(MountedImagePath, "Windows", "System32", "OEMDefaultAssociations.xml"));
            bool HandleOEMDefaultAssociationsDll = File.Exists(Path.Combine(MountedImagePath, "Windows", "System32", "OEMDefaultAssociations.dll"));

            string OEMDefaultAssociationsXml = tempManager.GetTempPath();
            string OEMDefaultAssociationsDll = tempManager.GetTempPath();

            string OEMDefaultAssociationsXmlInImage = Path.Combine(MountedImagePath, "Windows", "System32", "OEMDefaultAssociations.xml");
            string OEMDefaultAssociationsDllInImage = Path.Combine(MountedImagePath, "Windows", "System32", "OEMDefaultAssociations.dll");

            if (HandleOEMDefaultAssociationsXml)
            {
                File.Copy(OEMDefaultAssociationsXmlInImage, OEMDefaultAssociationsXml, true);
            }

            if (HandleOEMDefaultAssociationsDll)
            {
                File.Copy(OEMDefaultAssociationsDllInImage, OEMDefaultAssociationsDll, true);
            }

            //
            // Apply unattend
            //
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Applying unattend");
            DismOperations.NET.DismOperations.Instance.ApplyUnattend(MountedImagePath, unattendPath);

            //
            // Restore OEMDefaultAssociations
            //
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Restoring up OEMDefaultAssociations");

            if (HandleOEMDefaultAssociationsXml)
            {
                if (!File.Exists(OEMDefaultAssociationsXmlInImage))
                {
                    File.Move(OEMDefaultAssociationsXml, OEMDefaultAssociationsXmlInImage);
                }
            }

            if (HandleOEMDefaultAssociationsDll)
            {
                if (!File.Exists(OEMDefaultAssociationsDllInImage))
                {
                    File.Move(OEMDefaultAssociationsDll, OEMDefaultAssociationsDllInImage);
                }
            }

            //
            // Copy edition xml
            //
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Handling Edition Unattend XML");
            string editionXml = Path.Combine(MountedImagePath, "Windows", "servicing", "Editions", $"{EditionID}Edition.xml");
            string desintationEditionXml = Path.Combine(MountedImagePath, "Windows", $"{EditionID}.xml");

            File.Copy(editionXml, desintationEditionXml, true);

            //
            // Delete old edition xml
            //
            File.Delete(Path.Combine(MountedImagePath, "Windows", $"{SourceEdition}.xml"));

            //
            // Apply edition xml as unattend
            //
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Applying Edition Unattend XML");
            DismOperations.NET.DismOperations.Instance.ApplyUnattend(MountedImagePath, desintationEditionXml);

            //
            // Install correct product key
            //
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Installing product key");

            DismOperations.NET.DismOperations.Instance.SetProductKey(MountedImagePath, serial);

            //
            // Application handling
            //
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Installing apps");

            //////////////////////// TODO ////////////////////////

            //
            // Cleanup
            //
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Cleaning up");
            Directory.Delete(TemporaryFolder, true);
            CleanupLanguagePackFolderIfRequired();

            void callback2(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.CapturingImage, IsIndeterminate, ProgressPercentage, Operation);
            }

            string name = $"Windows 10 {EditionID}";
            if (IniReader.FriendlyEditionNames.Any(x => x.Key.Equals(EditionID, StringComparison.InvariantCultureIgnoreCase)))
            {
                name = IniReader.FriendlyEditionNames.First(x => x.Key.Equals(EditionID, StringComparison.InvariantCultureIgnoreCase)).Value;
            }

            result = Constants.imagingInterface.CaptureImage(
                OutputInstallImage,
                name,
                name,
                EditionID,
                MountedImagePath,
                tempManager,
                name,
                name,
                compression,
                progressCallback: callback2,
                UpdateFrom: index);

            if (!result)
            {
                goto exit;
            }

            result = Constants.imagingInterface.GetWIMImageInformation(OutputInstallImage, wiminfo.IMAGE.Count + 1, out WIMInformationXML.IMAGE tmpImageInfo);
            if (!result)
            {
                goto exit;
            }

            string sku = tmpImageInfo.WINDOWS.EDITIONID;

            tmpImageInfo.WINDOWS = srcimage.WINDOWS;
            tmpImageInfo.WINDOWS.EDITIONID = sku;
            tmpImageInfo.FLAGS = sku;
            tmpImageInfo.NAME = name;
            tmpImageInfo.DESCRIPTION = name;
            tmpImageInfo.DISPLAYNAME = name;
            tmpImageInfo.DISPLAYDESCRIPTION = name;

            result = Constants.imagingInterface.SetWIMImageInformation(OutputInstallImage, wiminfo.IMAGE.Count + 1, tmpImageInfo);
            if (!result)
            {
                goto exit;
            }

        exit:
            return result;
        }
    }
}