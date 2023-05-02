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
using CompDB;
using Imaging;
using IniParser;
using IniParser.Model;
using MediaCreationLib.DismOperations;
using MediaCreationLib.Planning.Applications;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UUPMediaCreator.InterCommunication;
using VirtualHardDiskLib;

namespace MediaCreationLib.BaseEditions
{
    public static class BaseEditionBuilder
    {
        public static bool CreateBaseEdition(
            string UUPPath,
            string LanguageCode,
            string EditionID,
            string InputWindowsREPath,
            string OutputInstallImage,
            Common.CompressionType CompressionType,
            IEnumerable<CompDBXmlClass.CompDB> CompositionDatabases,
            TempManager.TempManager tempManager,
            ProgressCallback progressCallback = null)
        {
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

            (bool result, string BaseESD, HashSet<string> ReferencePackages) = BuildReferenceImageList(UUPPath, LanguageCode, EditionID, CompositionDatabases, tempManager, progressCallback);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> BuildReferenceImageList failed");
                goto exit;
            }

            //
            // Gather information to transplant later into DisplayName and DisplayDescription
            //
            result = Constants.imagingInterface.GetWIMImageInformation(BaseESD, 3, out WIMInformationXML.IMAGE image);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> GetWIMImageInformation failed");
                goto exit;
            }

            //
            // Export the install image
            //
            void callback(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, IsIndeterminate, ProgressPercentage, Operation);
            }

            result = Constants.imagingInterface.ExportImage(
                BaseESD,
                OutputInstallImage,
                3,
                referenceWIMs: ReferencePackages,
                compressionType: compression,
                progressCallback: callback);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> ExportImage failed");
                goto exit;
            }

            result = Constants.imagingInterface.GetWIMInformation(OutputInstallImage, out WIMInformationXML.WIM wim);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> GetWIMInformation failed");
                goto exit;
            }

            //
            // Set the correct metadata on the image
            //
            image.DISPLAYNAME = image.NAME;
            image.DISPLAYDESCRIPTION = image.DESCRIPTION;
            image.NAME = image.NAME;
            image.DESCRIPTION = image.NAME;
            image.FLAGS = image.WINDOWS.EDITIONID;
            if (image.WINDOWS.INSTALLATIONTYPE.EndsWith(" Core", StringComparison.InvariantCultureIgnoreCase) && !image.FLAGS.EndsWith("Core", StringComparison.InvariantCultureIgnoreCase))
            {
                image.FLAGS += "Core";
            }

            if (image.WINDOWS.LANGUAGES == null)
            {
                image.WINDOWS.LANGUAGES = new WIMInformationXML.LANGUAGES()
                {
                    LANGUAGE = LanguageCode,
                    FALLBACK = new WIMInformationXML.FALLBACK()
                    {
                        LANGUAGE = LanguageCode,
                        Text = "en-US"
                    },
                    DEFAULT = LanguageCode
                };
            }
            result = Constants.imagingInterface.SetWIMImageInformation(OutputInstallImage, wim.IMAGE.Count, image);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> SetWIMImageInformation failed");
                goto exit;
            }

            void callback2(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.IntegratingWinRE, IsIndeterminate, ProgressPercentage, Operation);
            }

            //
            // Integrate the WinRE image into the installation image
            //
            progressCallback?.Invoke(Common.ProcessPhase.IntegratingWinRE, true, 0, "");

            result = Constants.imagingInterface.AddFileToImage(
                OutputInstallImage,
                wim.IMAGE.Count,
                InputWindowsREPath,
                Path.Combine("Windows", "System32", "Recovery", "Winre.wim"),
                callback2);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> AddFileToImage failed");
                goto exit;
            }

        exit:
            return result;
        }

        public static bool CreateBaseEditionWithAppXs(
            string UUPPath,
            string LanguageCode,
            string EditionID,
            string InputWindowsREPath,
            string OutputInstallImage,
            Common.CompressionType CompressionType,
            AppxInstallWorkload[] appxWorkloads,
            IEnumerable<CompDBXmlClass.CompDB> CompositionDatabases,
            TempManager.TempManager tempManager,
            bool keepVhd,
            out string vhdPath,
            ProgressCallback progressCallback = null)
        {
            vhdPath = null;
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

            (bool result, string BaseESD, HashSet<string> ReferencePackages) = BuildReferenceImageList(UUPPath, LanguageCode, EditionID, CompositionDatabases, tempManager, progressCallback);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEditionWithAppXs -> BuildReferenceImageList failed");
                goto exit;
            }

            //
            // Gather information to transplant later into DisplayName and DisplayDescription
            //
            result = Constants.imagingInterface.GetWIMImageInformation(BaseESD, 3, out WIMInformationXML.IMAGE image);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEditionWithAppXs -> GetWIMImageInformation failed");
                goto exit;
            }

            string licenseFolder = tempManager.GetTempPath();
            _ = Directory.CreateDirectory(licenseFolder);

            //
            // Export License files
            //
            result = FileLocator.GenerateAppXLicenseFiles(licenseFolder, LanguageCode, EditionID, CompositionDatabases, progressCallback);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEditionWithAppXs -> GenerateAppXLicenseFiles failed");
                goto exit;
            }

            //
            // Export the install image
            //
            void callback(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, IsIndeterminate, ProgressPercentage, Operation);
            }

            using (VirtualDiskSession vhdSession = new(tempManager, delete: !keepVhd))
            {
                vhdPath = vhdSession.VirtualDiskPath;

                result = Constants.imagingInterface.ApplyImage(BaseESD, 3, vhdSession.GetMountedPath(), referenceWIMs: ReferencePackages, progressCallback: callback);
                if (!result)
                {
                    progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEditionWithAppXs -> ApplyImage failed");
                    goto exit;
                }

                void customCallback(bool IsIndeterminate, int ProgressInPercentage, string SubOperation)
                {
                    progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, IsIndeterminate, ProgressInPercentage, SubOperation);
                }

                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, $"Installing Applications");
                result = RemoteDismOperations.Instance.PerformAppxWorkloadsInstallation(vhdSession.GetMountedPath(), UUPPath, licenseFolder, appxWorkloads, customCallback);
                if (!result)
                {
                    result = DismOperations.DismOperations.Instance.PerformAppxWorkloadsInstallation(vhdSession.GetMountedPath(), UUPPath, licenseFolder, appxWorkloads, customCallback);
                    if (!result)
                    {
                        foreach (AppxInstallWorkload appx in appxWorkloads)
                        {
                            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, $"Installing {appx.AppXPath}");
                            result = RemoteDismOperations.Instance.PerformAppxWorkloadInstallation(vhdSession.GetMountedPath(), UUPPath, licenseFolder, appx);
                            if (!result)
                            {
                                result = DismOperations.DismOperations.Instance.PerformAppxWorkloadInstallation(vhdSession.GetMountedPath(), UUPPath, licenseFolder, appx);
                            }
                        }
                    }
                }

                if (!result)
                {
                    progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "An error occured while running the external tool for appx installation.");
                    goto exit;
                }

                progressCallback?.Invoke(Common.ProcessPhase.IntegratingWinRE, true, 0, "Integrating WinRE");
                File.Copy(InputWindowsREPath, Path.Combine(vhdSession.GetMountedPath(), "Windows", "System32", "Recovery", "Winre.wim"), true);

                result = Constants.imagingInterface.CaptureImage(
                    OutputInstallImage,
                    image.NAME,
                    image.DESCRIPTION,
                    image.FLAGS,
                    vhdSession.GetMountedPath(),
                    tempManager,
                    image.DISPLAYNAME,
                    image.DISPLAYDESCRIPTION,
                    compressionType: compression,
                    progressCallback: callback);
                if (!result)
                {
                    progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEditionWithAppXs -> CaptureImage failed");
                    goto exit;
                }
            }

            result = Constants.imagingInterface.GetWIMInformation(OutputInstallImage, out WIMInformationXML.WIM wim);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> GetWIMInformation failed");
                goto exit;
            }

            //
            // Set the correct metadata on the image
            //
            image.DISPLAYNAME = image.NAME;
            image.DISPLAYDESCRIPTION = image.DESCRIPTION;
            image.NAME = image.NAME;
            image.DESCRIPTION = image.NAME;
            image.FLAGS = image.WINDOWS.EDITIONID;
            if (image.WINDOWS.INSTALLATIONTYPE.EndsWith(" Core", StringComparison.InvariantCultureIgnoreCase) && !image.FLAGS.EndsWith("Core", StringComparison.InvariantCultureIgnoreCase))
            {
                image.FLAGS += "Core";
            }

            if (image.WINDOWS.LANGUAGES == null)
            {
                image.WINDOWS.LANGUAGES = new WIMInformationXML.LANGUAGES()
                {
                    LANGUAGE = LanguageCode,
                    FALLBACK = new WIMInformationXML.FALLBACK()
                    {
                        LANGUAGE = LanguageCode,
                        Text = "en-US"
                    },
                    DEFAULT = LanguageCode
                };
            }
            result = Constants.imagingInterface.SetWIMImageInformation(OutputInstallImage, wim.IMAGE.Count, image);
            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "CreateBaseEdition -> SetWIMImageInformation failed");
                goto exit;
            }

        exit:
            return result;
        }

        private static (bool result, string BaseESD, HashSet<string> ReferencePackages) BuildReferenceImageList(
            string UUPPath,
            string LanguageCode,
            string EditionID,
            IEnumerable<CompDBXmlClass.CompDB> CompositionDatabases,
            TempManager.TempManager tempManager,
            ProgressCallback progressCallback = null)
        {
            HashSet<string> ReferencePackages, referencePackagesToConvert;
            string BaseESD = null;

            (bool result, BaseESD, ReferencePackages, referencePackagesToConvert) = FileLocator.LocateFilesForBaseEditionCreation(UUPPath, LanguageCode, EditionID, CompositionDatabases, progressCallback);
            if (!result)
            {
                goto exit;
            }

            string unpackedFODEsd = tempManager.GetTempPath();
            _ = Directory.CreateDirectory(unpackedFODEsd);

            int progressPercentage = 0;
            int total = referencePackagesToConvert.Count;
            ParallelLoopResult parallelResult = Parallel.ForEach(referencePackagesToConvert, (file, parallel) =>
            {
                int progressScale = (int)Math.Round((double)1 / total * 100);
                string fileName = Path.GetFileName(file);
                progressCallback?.Invoke(Common.ProcessPhase.PreparingFiles, false, progressPercentage, $"Unpacking {fileName}");
                try
                {
                    CabinetExtractor.ExtractCabinet(file, Path.Combine(unpackedFODEsd, fileName));
                }
                catch { result = false; }
                progressPercentage += progressScale;

                if (!result)
                {
                    parallel.Break();
                }
            });

            if (!parallelResult.IsCompleted)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "CreateBaseEdition -> Reference ESD creation from Cabinet files failed - Cab extraction");
                result = false;
                goto exit;
            }

            Dictionary<string, List<string>> iniSections = new();

            foreach (string referencePackageToConvert in referencePackagesToConvert)
            {
                string fileName = Path.GetFileName(referencePackageToConvert);
                string extractionPath = Path.Combine(unpackedFODEsd, fileName);
                string metadata = Path.Combine(extractionPath, "$filehashes$.dat");

                FileIniDataParser parser = new();
                IniData data = parser.ReadFile(metadata);

                foreach (SectionData section in data.Sections)
                {
                    if (!iniSections.ContainsKey(section.SectionName))
                    {
                        iniSections.Add(section.SectionName, new List<string>());
                    }

                    foreach (KeyData element in section.Keys)
                    {
                        iniSections[section.SectionName].Add($"{fileName}\\{element.KeyName}={element.Value}");
                    }
                }
            }

            string iniContent = "";
            foreach (KeyValuePair<string, List<string>> element in iniSections)
            {
                iniContent += $"[{element.Key}]\n\n";
                foreach (string el in element.Value)
                {
                    iniContent += $"{el}\n";
                }
                iniContent += "\n";
            }

            File.WriteAllText(Path.Combine(unpackedFODEsd, "$filehashes$.dat"), iniContent);

            string esdFilePath = tempManager.GetTempPath();

            progressCallback?.Invoke(Common.ProcessPhase.PreparingFiles, false, progressPercentage, $"Creating {esdFilePath}");

            result = Constants.imagingInterface.CaptureImage(esdFilePath, "Metadata ESD", null, null, unpackedFODEsd, compressionType: WimCompressionType.None, PreserveACL: false, tempManager: tempManager);

            Directory.Delete(unpackedFODEsd, true);

            if (!result)
            {
                progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "CreateBaseEdition -> Reference ESD creation from Cabinet files failed - WIM creation");
                goto exit;
            }

            _ = ReferencePackages.Add(esdFilePath);

        exit:
            return (result, BaseESD, ReferencePackages);
        }
    }
}