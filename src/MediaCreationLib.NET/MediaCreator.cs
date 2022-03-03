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
using Imaging;
using MediaCreationLib.BaseEditions;
using MediaCreationLib.BootlegEditions;
using MediaCreationLib.Installer;
using MediaCreationLib.Planning.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using UUPMediaCreator.InterCommunication;

namespace MediaCreationLib
{
    public static class MediaCreator
    {
        private static readonly bool RunsAsAdministrator = IsAdministrator();

        private static bool IsAdministrator()
        {
            if (GetOperatingSystem() == OSPlatform.Windows)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
#pragma warning restore CA1416 // Validate platform compatibility
            }
            else
            {
                return false;
            }
        }

        public static OSPlatform GetOperatingSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows;
            }

            return RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)
                ? OSPlatform.FreeBSD
                : throw new Exception("Cannot determine operating system!");
        }

        private static readonly WIMImaging imagingInterface = new();

        public delegate void ProgressCallback(Common.ProcessPhase phase, bool IsIndeterminate, int ProgressInPercentage, string SubOperation);

        private static bool HandleEditionPlan(
            EditionTarget targetEdition,
            string UUPPath,
            string MediaPath,
            string LanguageCode,
            string InstallWIMFilePath,
            string WinREWIMFilePath,
            Common.CompressionType CompressionType,
            TempManager.TempManager tempManager,
            string VHDMountPath = null,
            string CurrentBackupVHD = null,
            ProgressCallback progressCallback = null,
            string edition = null)
        {
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, $"Applying {targetEdition.PlannedEdition.EditionName} - {targetEdition.PlannedEdition.AvailabilityType}");

            bool result = true;
            switch (targetEdition.PlannedEdition.AvailabilityType)
            {
                case AvailabilityType.Canonical:
                    {
                        if (RunsAsAdministrator && targetEdition.PlannedEdition.AppXInstallWorkloads?.Length > 0)
                        {
                            // Allow AppX Slipstreaming
                            result = BaseEditionBuilder.CreateBaseEditionWithAppXs(
                                    UUPPath,
                                    LanguageCode,
                                    targetEdition.PlannedEdition.EditionName,
                                    WinREWIMFilePath,
                                    InstallWIMFilePath,
                                    CompressionType,
                                    targetEdition.PlannedEdition.AppXInstallWorkloads,
                                    tempManager,
                                    progressCallback);
                        }
                        else
                        {
                            // Otherwise not
                            result = BaseEditionBuilder.CreateBaseEdition(
                                    UUPPath,
                                    LanguageCode,
                                    targetEdition.PlannedEdition.EditionName,
                                    WinREWIMFilePath,
                                    InstallWIMFilePath,
                                    CompressionType,
                                    tempManager,
                                    progressCallback);
                        }

                        if (!result)
                        {
                            goto exit;
                        }

                        break;
                    }
                case AvailabilityType.VirtualEdition:
                    {
                        result = UUPMediaCreator.CreateUpgradedEditionFromMountedImage(
                                    VHDMountPath,
                                    targetEdition.PlannedEdition.EditionName,
                                    InstallWIMFilePath,
                                    true,
                                    CompressionType,
                                    progressCallback);

                        if (!result)
                        {
                            goto exit;
                        }

                        break;
                    }
                case AvailabilityType.EditionUpgrade:
                    {
                        string newvhd = VirtualHardDiskLib.VHDUtilities.CreateDiffDisk(CurrentBackupVHD);

                        progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Mounting VHD");
                        using VirtualHardDiskLib.VirtualDiskSession vhdSession = new(existingVHD: newvhd);
                        VHDMountPath = vhdSession.GetMountedPath();

                        result = UUPMediaCreator.CreateUpgradedEditionFromMountedImage(
                            VHDMountPath,
                            targetEdition.PlannedEdition.EditionName,
                            InstallWIMFilePath,
                            false,
                            CompressionType,
                            progressCallback);

                        if (!result)
                        {
                            goto exit;
                        }

                        break;
                    }
                case AvailabilityType.EditionPackageSwap:
                    {
                        if (targetEdition.PlannedEdition.EditionName.StartsWith("starter", StringComparison.CurrentCultureIgnoreCase))
                        {
                            // TODO
                            // (Downgrade from core/coren to starter/startern)
                        }
                        else if (targetEdition.PlannedEdition.EditionName.StartsWith("professionaln", StringComparison.CurrentCultureIgnoreCase))
                        {
                            // TODO
                            // (Downgrade from ppipro to pron)
                        }
                        else if (targetEdition.PlannedEdition.EditionName.StartsWith("professional", StringComparison.CurrentCultureIgnoreCase))
                        {
                            // TODO
                            // (Downgrade from ppipro to pro)
                        }
                        else
                        {
                            string newvhd = VirtualHardDiskLib.VHDUtilities.CreateDiffDisk(CurrentBackupVHD);

                            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Mounting VHD");
                            using VirtualHardDiskLib.VirtualDiskSession vhdSession = new(existingVHD: newvhd);
                            VHDMountPath = vhdSession.GetMountedPath();

                            result = BootlegEditionCreator.CreateHackedEditionFromMountedImage(
                                UUPPath,
                                MediaPath,
                                VHDMountPath,
                                targetEdition.PlannedEdition.EditionName,
                                InstallWIMFilePath,
                                CompressionType,
                                tempManager,
                                progressCallback);
                            if (!result)
                            {
                                goto exit;
                            }
                        }
                        break;
                    }
            }

            if (targetEdition.DestructiveTargets.Count > 0 || targetEdition.NonDestructiveTargets.Count > 0)
            {
                string vhdpath = null;

                using (VirtualHardDiskLib.VirtualDiskSession vhdSession = new(delete: false))
                {
                    // Apply WIM
                    WIMImaging.GetWIMInformation(InstallWIMFilePath, out WIMInformationXML.WIM wiminfo);

                    int index = int.Parse(wiminfo.IMAGE.First(x => x.WINDOWS.EDITIONID.Equals(targetEdition.PlannedEdition.EditionName, StringComparison.InvariantCultureIgnoreCase)).INDEX);

                    void callback(string Operation, int ProgressPercentage, bool IsIndeterminate)
                    {
                        progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, IsIndeterminate, ProgressPercentage, Operation);
                    }
                    result = imagingInterface.ApplyImage(InstallWIMFilePath, index, vhdSession.GetMountedPath(), progressCallback: callback);
                    if (!result)
                    {
                        goto exit;
                    }

                    vhdpath = vhdSession.VirtualDiskPath;
                }

                if (targetEdition.NonDestructiveTargets.Count > 0 && (string.IsNullOrEmpty(edition) || (!string.IsNullOrEmpty(edition) && targetEdition.NonDestructiveTargets.Any(x => IsRightPath(x, edition)))))
                {
                    string newvhd = VirtualHardDiskLib.VHDUtilities.CreateDiffDisk(vhdpath);

                    progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, "Mounting VHD");

                    using VirtualHardDiskLib.VirtualDiskSession vhdSession = new(existingVHD: newvhd);
                    foreach (EditionTarget ed in targetEdition.NonDestructiveTargets)
                    {
                        if (!string.IsNullOrEmpty(edition) && !IsRightPath(ed, edition))
                        {
                            continue;
                        }

                        result = HandleEditionPlan(
                            ed,
                            UUPPath,
                            MediaPath,
                            LanguageCode,
                            InstallWIMFilePath,
                            WinREWIMFilePath,
                            CompressionType,
                            tempManager,
                            VHDMountPath: vhdSession.GetMountedPath(),
                            CurrentBackupVHD: vhdpath,
                            progressCallback: progressCallback);

                        if (!result)
                        {
                            goto exit;
                        }
                    }
                }

                if (targetEdition.DestructiveTargets.Count > 0 && (string.IsNullOrEmpty(edition) || (!string.IsNullOrEmpty(edition) && targetEdition.DestructiveTargets.Any(x => IsRightPath(x, edition)))))
                {
                    foreach (EditionTarget ed in targetEdition.DestructiveTargets)
                    {
                        if (!string.IsNullOrEmpty(edition) && !IsRightPath(ed, edition))
                        {
                            continue;
                        }

                        result = HandleEditionPlan(
                            ed,
                            UUPPath,
                            MediaPath,
                            LanguageCode,
                            InstallWIMFilePath,
                            WinREWIMFilePath,
                            CompressionType,
                            tempManager,
                            CurrentBackupVHD: vhdpath,
                            progressCallback: progressCallback);

                        if (!result)
                        {
                            goto exit;
                        }
                    }
                }

                File.Delete(vhdpath);
            }

        exit:
            return result;
        }

        public static bool GetTargetedPlan(
            string UUPPath,
            string LanguageCode,
            out List<EditionTarget> EditionTargets,
            ProgressCallback progressCallback = null)
        {
            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Acquiring Composition Databases");

            HashSet<CompDBXmlClass.CompDB> compDBs = FileLocator.GetCompDBsFromUUPFiles(UUPPath);

            string EditionPack = "";

            //
            // Get base editions that are available with all their files
            //
            IEnumerable<CompDBXmlClass.CompDB> filteredCompDBs = compDBs.GetEditionCompDBsForLanguage(LanguageCode).Where(x =>
            {
                (bool success, HashSet<string> missingfiles) = FileLocator.VerifyFilesAreAvailableForCompDB(x, UUPPath);
                return success;
            });

            if (filteredCompDBs.Any())
            {
                foreach (CompDBXmlClass.Package feature in filteredCompDBs.First().Features.Feature[0].Packages.Package)
                {
                    CompDBXmlClass.Package pkg = filteredCompDBs.First().Packages.Package.First(x => x.ID == feature.ID);

                    string file = pkg.GetCommonlyUsedIncorrectFileName();

                    //
                    // We know already that all files exist, so it's just a matter of knowing which path format is used
                    //
                    file = !File.Exists(Path.Combine(UUPPath, file)) ? pkg.Payload.PayloadItem[0].Path.Replace('\\', Path.DirectorySeparatorChar) : file;

                    if (!file.EndsWith(".esd", StringComparison.InvariantCultureIgnoreCase) ||
                        !file.Contains("microsoft-windows-editionspecific", StringComparison.InvariantCultureIgnoreCase) ||
                        file.Contains("WOW64", StringComparison.InvariantCultureIgnoreCase) ||
                        file.Contains("arm64.arm", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // We do not care about this file
                        continue;
                    }

                    EditionPack = Path.Combine(UUPPath, file);
                }
            }

            if (string.IsNullOrEmpty(EditionPack))
            {
                bool result = true;
                string BaseESD = null;

                (result, BaseESD) = NET.FileLocator.LocateFilesForSetupMediaCreation(UUPPath, LanguageCode, progressCallback);
                if (result)
                {
                    EditionPack = BaseESD;
                }
            }

            return ConversionPlanBuilder.GetTargetedPlan(UUPPath, compDBs, EditionPack, LanguageCode, RunsAsAdministrator, out EditionTargets, (string msg) => progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, msg));
        }

        public static bool IsRightPath(EditionTarget editionTarget, string edition)
        {
            if (editionTarget.PlannedEdition.EditionName.Equals(edition, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            foreach (EditionTarget ed in editionTarget.DestructiveTargets)
            {
                if (IsRightPath(ed, edition))
                {
                    return true;
                }
            }

            foreach (EditionTarget ed in editionTarget.NonDestructiveTargets)
            {
                if (IsRightPath(ed, edition))
                {
                    return true;
                }
            }

            return false;
        }

        public static void CreateISOMedia(
            string ISOPath,
            string UUPPath,
            string Edition,
            string LanguageCode,
            bool IntegrateUpdates,
            Common.CompressionType CompressionType,
            ProgressCallback progressCallback = null,
            string Temp = null)
        {
            bool result = true;
            string error = "";

            TempManager.TempManager tempManager = new(Temp);

            try
            {
                result = GetTargetedPlan(UUPPath, LanguageCode, out List<EditionTarget> editionTargets, progressCallback);
                if (!result)
                {
                    error = "An error occurred while getting target plans for the conversion.";
                    goto error;
                }

                foreach (EditionTarget ed in editionTargets)
                {
                    foreach (string line in ConversionPlanBuilder.PrintEditionTarget(ed))
                    {
                        progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, line);
                    }
                }

                progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

                string temp = tempManager.GetTempPath();
                Directory.CreateDirectory(temp);

                string WinREWIMFilePath = Path.Combine(temp, "Winre.wim");
                string MediaRootPath = Path.Combine(temp, "MediaRoot");
                string InstallWIMFilePath = CompressionType == Common.CompressionType.LZMS ?
                    Path.Combine(MediaRootPath, "sources", "install.esd") :
                    Path.Combine(MediaRootPath, "sources", "install.wim");

                //
                // Build installer
                //
                result = SetupMediaCreator.CreateSetupMedia(UUPPath, LanguageCode, MediaRootPath, WinREWIMFilePath, CompressionType, tempManager, progressCallback);
                if (!result)
                {
                    error = "An error occurred while creating setup media.";
                    goto error;
                }

                //
                // Build Install.WIM/ESD
                //
                foreach (EditionTarget ed in editionTargets)
                {
                    if (!string.IsNullOrEmpty(Edition) && !IsRightPath(ed, Edition))
                    {
                        continue;
                    }

                    result = HandleEditionPlan(ed, UUPPath, MediaRootPath, LanguageCode, InstallWIMFilePath, WinREWIMFilePath, CompressionType, tempManager, progressCallback: progressCallback, edition: Edition);
                    if (!result)
                    {
                        error = "An error occurred while handling edition plan for the following edition: " + ed.PlannedEdition.EditionName + " available as: " + ed.PlannedEdition.AvailabilityType;
                        goto error;
                    }
                }

                BootlegEditionCreator.CleanupLanguagePackFolderIfRequired();

                //
                // Build ISO
                //
                result = UUPMediaCreator.CreateISO(MediaRootPath, ISOPath, progressCallback);
                if (!result)
                {
                    error = "An error occurred while creating the ISO.";
                    goto error;
                }

                progressCallback?.Invoke(Common.ProcessPhase.Done, true, 0, "");
                goto exit;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }

        error:
            progressCallback?.Invoke(Common.ProcessPhase.Error, true, 0, error);

        exit:
            tempManager.Dispose();
            return;
        }
    }
}