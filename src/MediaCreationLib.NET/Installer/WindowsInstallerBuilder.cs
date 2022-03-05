﻿/*
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
using Imaging;
using MediaCreationLib.Settings;
using MediaCreationLib.Utils;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UUPMediaCreator.InterCommunication;
using VirtualHardDiskLib;

namespace MediaCreationLib.Installer
{
    public static class WindowsInstallerBuilder
    {
        // 6 progress bars
        public static bool BuildSetupMedia(
            string BaseESD,
            string OutputWinREPath,
            string MediaPath,
            WimCompressionType compressionType,
            bool RunsAsAdministrator,
            string LanguageCode,
            TempManager.TempManager tempManager,
            ProgressCallback progressCallback = null
            )
        {
            //
            // First create the setup media base, minus install.wim and boot.wim
            //
            bool result = CreateSetupMediaRoot(BaseESD, MediaPath, progressCallback);
            if (!result)
            {
                goto exit;
            }

            //
            // Gather information about the Windows Recovery Environment image so we can transplant it later
            // into our new images
            //
            result = WIMImaging.GetWIMImageInformation(BaseESD, 2, out WIMInformationXML.IMAGE image);
            if (!result)
            {
                progressCallback?.Log("An error occured while getting WIM image information.");
                goto exit;
            }

            //
            // Gather the architecture string under parenthesis for the new images we are creating
            //
            string ArchitectureInNameAndDescription = image.NAME.Split('(')[1].Replace(")", "");

            string BootFirstImageName = $"Microsoft Windows PE ({ArchitectureInNameAndDescription})";
            string BootSecondImageName = $"Microsoft Windows Setup ({ArchitectureInNameAndDescription})";
            const string BootFirstImageFlag = "9";
            const string BootSecondImageFlag = "2";

            //
            // Bootable wim files must not be lzms
            //
            if (compressionType == WimCompressionType.Lzms)
            {
                compressionType = WimCompressionType.Lzx;
            }

            //
            // Prepare our base PE image which will serve as a basis for all subsequent operations
            // This function also generates WinRE
            //
            result = PreparePEImage(BaseESD, OutputWinREPath, MediaPath, compressionType, LanguageCode, tempManager, progressCallback);
            if (!result)
            {
                progressCallback?.Log("An error occured while preparing the PE image.");
                goto exit;
            }

            //
            // If we are running as administrator, perform additional component cleanup
            //
            if (RunsAsAdministrator)
            {
                result = PerformComponentCleanupOnPEImage(MediaPath, compressionType, image, tempManager, progressCallback);
                if (!result)
                {
                    progressCallback?.Log("An error occured while performing component cleanup on pe image.");
                    goto exit;
                }
            }

            string bootwim = Path.Combine(MediaPath, "sources", "boot.wim");

            string tmpwimcopy = tempManager.GetTempPath();
            File.Copy(bootwim, tmpwimcopy);

            //
            // Duplicate the boot image so we have two of them
            //
            result = Constants.imagingInterface.ExportImage(tmpwimcopy, bootwim, 1, compressionType: compressionType, progressCallback: progressCallback?.GetImagingCallback());
            if (!result)
            {
                progressCallback?.Log("An error occured while exporting the main boot image.");
                goto exit;
            }

            File.Delete(tmpwimcopy);

            //
            // Set the correct metadata on both images
            //
            image.NAME = BootFirstImageName;
            image.DESCRIPTION = BootFirstImageName;
            image.FLAGS = BootFirstImageFlag;
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
            result = WIMImaging.SetWIMImageInformation(bootwim, 1, image);
            if (!result)
            {
                progressCallback?.Log("An error occured while setting image information for index 1.");
                goto exit;
            }

            image.NAME = BootSecondImageName;
            image.DESCRIPTION = BootSecondImageName;
            image.FLAGS = BootSecondImageFlag;
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
            result = WIMImaging.SetWIMImageInformation(bootwim, 2, image);
            if (!result)
            {
                progressCallback?.Log("An error occured while setting image information for index 2.");
                goto exit;
            }

            //
            // Mark image as bootable
            //
            result = WIMImaging.MarkImageAsBootable(bootwim, 2);
            if (!result)
            {
                goto exit;
            }

            result = ModifyImageRegistry(MediaPath, tempManager, progressCallback);
            if (!result)
            {
                goto exit;
            }

            result = IntegrateSetupFilesIntoImage(MediaPath, tempManager, progressCallback);
            if (!result)
            {
                goto exit;
            }

        //
        // We're done
        //

        exit:
            return result;
        }

        private static bool ModifyImageRegistry(
            string MediaPath,
            TempManager.TempManager tempManager,
            ProgressCallback progressCallback = null
            )
        {
            string bootwim = Path.Combine(MediaPath, "sources", "boot.wim");

            //
            // Modifying registry for each index
            //
            string tempSoftwareHiveBackup = tempManager.GetTempPath();
            string tempSystemHiveBackup = tempManager.GetTempPath();

            bool result = WIMImaging.ExtractFileFromImage(bootwim, 1, Constants.SYSTEM_Hive_Location, tempSystemHiveBackup);
            if (!result)
            {
                progressCallback?.Log("An error occured while extracting the SYSTEM hive from index 1.");
                goto exit;
            }

            result = WIMImaging.ExtractFileFromImage(bootwim, 1, Constants.SOFTWARE_Hive_Location, tempSoftwareHiveBackup);
            if (!result)
            {
                progressCallback?.Log("An error occured while extracting the SOFTWARE hive from index 1.");
                goto exit;
            }

            File.Copy(tempSoftwareHiveBackup, $"{tempSoftwareHiveBackup}.2");

            result = PreinstallationEnvironmentRegistryService.ModifyBootIndex2Registry($"{tempSoftwareHiveBackup}.2");
            if (!result)
            {
                progressCallback?.Log("An error occured while modifying the SOFTWARE hive for index 2.");
                goto exit;
            }

            result = Constants.imagingInterface.AddFileToImage(bootwim, 2, $"{tempSoftwareHiveBackup}.2", Constants.SOFTWARE_Hive_Location, progressCallback: progressCallback?.GetImagingCallback());
            if (!result)
            {
                progressCallback?.Log("An error occured while modifying adding back the SOFTWARE hive for index 2.");
                goto exit;
            }

            File.Delete($"{tempSoftwareHiveBackup}.2");

            result = PreinstallationEnvironmentRegistryService.ModifyBootIndex1Registry(tempSystemHiveBackup, tempSoftwareHiveBackup);
            if (!result)
            {
                progressCallback?.Log("An error occured while modifying the SOFTWARE/SYSTEM hives for index 1.");
                goto exit;
            }

            result = Constants.imagingInterface.AddFileToImage(bootwim, 1, tempSystemHiveBackup, Constants.SYSTEM_Hive_Location, progressCallback: progressCallback?.GetImagingCallback());
            if (!result)
            {
                progressCallback?.Log("An error occured while modifying adding back the SYSTEM hive for index 1.");
                goto exit;
            }

            result = Constants.imagingInterface.AddFileToImage(bootwim, 1, tempSoftwareHiveBackup, Constants.SOFTWARE_Hive_Location, progressCallback: progressCallback?.GetImagingCallback());
            if (!result)
            {
                progressCallback?.Log("An error occured while modifying adding back the SOFTWARE hive for index 1.");
                goto exit;
            }

            File.Delete(tempSoftwareHiveBackup);
            File.Delete(tempSystemHiveBackup);

        exit:
            return result;
        }

        private static bool IntegrateSetupFilesIntoImage(
            string MediaPath,
            TempManager.TempManager tempManager,
            ProgressCallback progressCallback = null
            )
        {
            string bootwim = Path.Combine(MediaPath, "sources", "boot.wim");

            //
            // Adding missing files in index 2
            //
            progressCallback?.Log("Modifying assets for Setup PE (1)");

            bool result = Constants.imagingInterface.DeleteFileFromImage(bootwim, 2, Path.Combine("Windows", "System32", "winpe.jpg"), progressCallback: progressCallback?.GetImagingCallback());
            if (!result)
            {
                progressCallback?.Log("An error occured while modifying deleting the background file for index 2.");
                goto exit;
            }

            string bgfile = new[] { "background_cli.bmp", "background_svr.bmp", "background_cli.png", "background_svr.png" }
                .Select(asset => Path.Combine(MediaPath, $@"sources\{asset}"))
                .FirstOrDefault(assetPath => File.Exists(assetPath));

            result = Constants.imagingInterface.AddFileToImage(bootwim, 2, bgfile, Path.Combine("Windows", "System32", "setup.bmp"), progressCallback: progressCallback?.GetImagingCallback());
            if (!result)
            {
                progressCallback?.Log("An error occured while modifying adding the background file for index 2.");
                goto exit;
            }

            progressCallback?.Log("Modifying assets for Setup PE (2)");
            string winpejpgtmp = tempManager.GetTempPath();
            File.WriteAllBytes(winpejpgtmp, Constants.winpejpg);
            result = Constants.imagingInterface.AddFileToImage(bootwim, 2, winpejpgtmp, Path.Combine("Windows", "System32", "winpe.jpg"), progressCallback: progressCallback?.GetImagingCallback());
            File.Delete(winpejpgtmp);
            if (!result)
            {
                progressCallback?.Log("An error occured while adding back the background file for index 2.");
                goto exit;
            }

            progressCallback?.Log("Backporting missing files");

            IEnumerable<string> dirs = Directory.EnumerateDirectories(Path.Combine(MediaPath, "sources"), "??-??");
            if (!dirs.Any())
            {
                dirs = Directory.EnumerateDirectories(Path.Combine(MediaPath, "sources"), "*-*");
            }
            string langcode = dirs.First().Replace(Path.Combine(MediaPath, "sources") + Path.DirectorySeparatorChar, "");

            foreach (string file in IniReader.SetupFilesToBackport)
            {
                string matchingfile = Path.Combine(MediaPath, file).Replace("??-??", langcode);
                string normalizedPath = file.Replace("??-??", langcode);
                string normalizedPathWithoutFile = FolderUtilities.GetParentPath(normalizedPath);

                if (file == $"sources{Path.DirectorySeparatorChar}background.bmp")
                {
                    result = Constants.imagingInterface.AddFileToImage(bootwim, 2, bgfile, normalizedPath, progressCallback: progressCallback?.GetImagingCallback());
                    if (!result)
                    {
                        goto exit;
                    }
                }
                else if (File.Exists(matchingfile))
                {
                    result = Constants.imagingInterface.AddFileToImage(bootwim, 2, matchingfile, normalizedPath, progressCallback: progressCallback?.GetImagingCallback());
                    if (!result)
                    {
                        goto exit;
                    }
                }
            }

        exit:
            return result;
        }

        private static bool PreparePEImage(
            string BaseESD,
            string OutputWinREPath,
            string MediaPath,
            WimCompressionType compressionType,
            string LanguageCode,
            TempManager.TempManager tempManager,
            ProgressCallback progressCallback = null
            )
        {
            //
            // Export the RE image to our re path, in this case a WIM
            //
            bool result = Constants.imagingInterface.ExportImage(BaseESD, OutputWinREPath, 2, compressionType: compressionType, progressCallback: progressCallback.GetImagingCallback());
            if (!result)
            {
                goto exit;
            }

            result = WIMImaging.GetWIMImageInformation(OutputWinREPath, 1, out WIMInformationXML.IMAGE image);
            if (!result)
            {
                goto exit;
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

                result = WIMImaging.SetWIMImageInformation(OutputWinREPath, 1, image);
                if (!result)
                {
                    goto exit;
                }
            }

            progressCallback?.Log("Marking image as bootable");
            result = WIMImaging.MarkImageAsBootable(OutputWinREPath, 1);
            if (!result)
            {
                goto exit;
            }

            string bootwim = Path.Combine(MediaPath, "sources", "boot.wim");
            File.Copy(OutputWinREPath, bootwim);

            //
            // Cleanup WinPE Shell directive
            //
            string sys32 = Path.Combine("Windows", "System32");
            string peshellini = Path.Combine(sys32, "winpeshl.ini");

            // Ignore return result
            Constants.imagingInterface.DeleteFileFromImage(bootwim, 1, peshellini, progressCallback: progressCallback?.GetImagingCallback());

            //
            // Cleanup log file from RE conversion phase mentions
            //
            try
            {
                progressCallback?.Log("Cleaning log files");
                string logfile = tempManager.GetTempPath();
                string pathinimage = Path.Combine("Windows", "INF", "setupapi.offline.log");

                bool cresult = WIMImaging.ExtractFileFromImage(bootwim, 1, pathinimage, logfile);

                if (cresult)
                {
                    string[] lines = File.ReadAllLines(logfile);

                    int bootsessioncount = 0;
                    List<string> finallines = new();
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("[Boot Session: ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            bootsessioncount++;
                        }
                        if (bootsessioncount == 2)
                        {
                            finallines.RemoveAt(finallines.Count - 1);
                            File.WriteAllLines(logfile, finallines);
                            // Ignore return result
                            Constants.imagingInterface.AddFileToImage(bootwim, 1, logfile, pathinimage, progressCallback: progressCallback?.GetImagingCallback());
                            break;
                        }
                        finallines.Add(line);
                    }
                }
            }
            catch { }

            //
            // Disable UMCI
            //
            progressCallback?.Log("Disabling UMCI");
            string tempSystemHiveBackup = tempManager.GetTempPath();

            result = WIMImaging.ExtractFileFromImage(bootwim, 1, Constants.SYSTEM_Hive_Location, tempSystemHiveBackup);
            if (!result)
            {
                goto cleanup;
            }

            result = PreinstallationEnvironmentRegistryService.ModifyBootGlobalRegistry(tempSystemHiveBackup);
            if (!result)
            {
                goto cleanup;
            }

            result = Constants.imagingInterface.AddFileToImage(bootwim, 1, tempSystemHiveBackup, Constants.SYSTEM_Hive_Location, progressCallback: progressCallback?.GetImagingCallback());
            if (!result)
            {
                goto cleanup;
            }

        cleanup:
            File.Delete(tempSystemHiveBackup);

        exit:
            return result;
        }

        private static bool PerformComponentCleanupOnPEImage(
            string MediaPath,
            WimCompressionType compressionType,
            WIMInformationXML.IMAGE image,
            TempManager.TempManager tempManager,
            ProgressCallback progressCallback = null
            )
        {
            using (VirtualDiskSession vhdsession = new(tempManager))
            {
                string ospath = vhdsession.GetMountedPath();

                //
                // Apply the RE image to our ospath, in this case our VHD
                //
                bool result = Constants.imagingInterface.ApplyImage(Path.Combine(MediaPath, "sources", "boot.wim"), 1, ospath, progressCallback: progressCallback?.GetImagingCallback());
                if (!result)
                {
                    progressCallback?.Log("An error occured while applying the first boot image for component cleanup.");
                    goto exit;
                }

                File.Delete(Path.Combine(MediaPath, "sources", "boot.wim"));

                void customCallback(bool IsIndeterminate, int ProgressInPercentage, string SubOperation)
                {
                    progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, IsIndeterminate, ProgressInPercentage, SubOperation);
                }

                result = Dism.RemoteDismOperations.Instance.UninstallPEComponents(ospath, customCallback);
                if (!result)
                {
                    progressCallback?.Log("An error occured while performing component cleanup with external tool.");
                    goto exit;
                }

                //
                // Cleanup leftovers for WLAN
                //
                progressCallback?.Log("Cleaning up leftovers");

                string winsxsfolder = Path.Combine(ospath, "Windows", "WinSxS");
                string winsxsManFolder = Path.Combine(winsxsfolder, "Manifests");

                IEnumerable<string> directoriesToCleanOut = Directory.EnumerateDirectories(winsxsfolder, "*_dual_netnwifi.inf_31bf3856ad364e35_*", SearchOption.TopDirectoryOnly);
                IEnumerable<string> manifestsToCleanOut = Directory.EnumerateFiles(winsxsManFolder, "*_dual_netnwifi.inf_31bf3856ad364e35_*", SearchOption.TopDirectoryOnly);

                foreach (string dir in directoriesToCleanOut)
                {
                    try
                    {
                        progressCallback?.Log("Deleting " + dir);
                        TakeOwn.TakeOwnDirectory(dir);
                        Directory.Delete(dir, true);
                    }
                    catch { }
                }

                foreach (string file in manifestsToCleanOut)
                {
                    try
                    {
                        progressCallback?.Log("Deleting " + file);
                        TakeOwn.TakeOwnFile(file);
                        File.Delete(file);
                    }
                    catch { }
                }

                //
                // Add missing files from the setup media root
                //
                progressCallback?.Log("Adding missing files");

                if (!File.Exists(Path.Combine(ospath, "Windows", "System32", "ReAgent.dll")))
                {
                    File.Copy(Path.Combine(MediaPath, "sources", "ReAgent.dll"), Path.Combine(ospath, "Windows", "System32", "ReAgent.dll"));
                }

                if (!File.Exists(Path.Combine(ospath, "Windows", "System32", "unattend.dll")))
                {
                    File.Copy(Path.Combine(MediaPath, "sources", "unattend.dll"), Path.Combine(ospath, "Windows", "System32", "unattend.dll"));
                }

                if (!File.Exists(Path.Combine(ospath, "Windows", "System32", "wpx.dll")))
                {
                    File.Copy(Path.Combine(MediaPath, "sources", "wpx.dll"), Path.Combine(ospath, "Windows", "System32", "wpx.dll"));
                }

                result = Constants.imagingInterface.CaptureImage(
                    Path.Combine(MediaPath, "sources", "boot.wim"),
                    image.NAME,
                    image.DESCRIPTION,
                    image.FLAGS,
                    ospath,
                    tempManager,
                    progressCallback: progressCallback?.GetImagingCallback(),
                    compressionType: compressionType);
                if (!result)
                {
                    progressCallback?.Log("An error occured while capturing the modified boot image for component cleanup.");
                    goto exit;
                }

            exit:
                return result;
            }
        }

        private static bool CreateSetupMediaRoot(
            string BaseESD,
            string OutputPath,
            ProgressCallback progressCallback = null
            )
        {
            bool result = true;

            //
            // Verify that the folder exists, if it doesn't, simply create it
            //
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }

            //
            // Apply the first index of the base ESD containing the setup files we need
            //
            result = Constants.imagingInterface.ApplyImage(
                BaseESD,
                1,
                OutputPath,
                progressCallback: progressCallback?.GetImagingCallback(),
                PreserveACL: false);
            if (!result)
            {
                progressCallback?.Log("An error occured while applying the image for the setup files.");
                goto exit;
            }

            //
            // The setup files from the first index are missing a single component (wtf?) so extract it from index 3 and place it in sources
            // Note: the file in question isn't in a wim that needs to be referenced, so we don't need to mention reference images.
            //
            progressCallback?.Log("Extracting XML Lite");
            result = WIMImaging.ExtractFileFromImage(BaseESD, 3, Path.Combine("Windows", "System32", "xmllite.dll"), Path.Combine(OutputPath, "sources", "xmllite.dll"));
            if (!result)
            {
                progressCallback?.Log("An error occured while extracting XML Lite.");
                goto exit;
            }

        exit:
            return result;
        }
    }
}