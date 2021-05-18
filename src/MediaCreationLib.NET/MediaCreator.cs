using Imaging;
using MediaCreationLib.BaseEditions;
using MediaCreationLib.BootlegEditions;
using MediaCreationLib.Installer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UUPMediaCreator.InterCommunication;
using MediaCreationLib.Planning.NET;
using CompDB;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace MediaCreationLib
{
    public class MediaCreator
    {
        private static bool RunsAsAdministrator = IsAdministrator();

        private static bool IsAdministrator()
        {
            if (GetOperatingSystem() == OSPlatform.Windows)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                return OSPlatform.FreeBSD;
            }

            throw new Exception("Cannot determine operating system!");
        }

        private static WIMImaging imagingInterface = new WIMImaging();

        public delegate void ProgressCallback(Common.ProcessPhase phase, bool IsIndeterminate, int ProgressInPercentage, string SubOperation);

        private static bool HandleEditionPlan(
            EditionTarget targetEdition,
            string UUPPath,
            string MediaPath,
            string LanguageCode,
            string InstallWIMFilePath,
            string WinREWIMFilePath,
            Common.CompressionType CompressionType,
            string VHDMountPath = null,
            string CurrentBackupVHD = null,
            ProgressCallback progressCallback = null)
        {
            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, $"Applying {targetEdition.PlannedEdition.EditionName} - {targetEdition.PlannedEdition.AvailabilityType}");

            bool result = true;
            switch (targetEdition.PlannedEdition.AvailabilityType)
            {
                case AvailabilityType.Canonical:
                    {
                        result = BaseEditionBuilder.CreateBaseEdition(
                                UUPPath,
                                LanguageCode,
                                targetEdition.PlannedEdition.EditionName,
                                WinREWIMFilePath,
                                InstallWIMFilePath,
                                CompressionType,
                                progressCallback);

                        if (!result)
                            goto exit;

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
                            goto exit;

                        break;
                    }
                case AvailabilityType.EditionUpgrade:
                    {
                        var newvhd = VirtualHardDiskLib.VHDUtilities.CreateDiffDisk(CurrentBackupVHD);

                        progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, $"Mounting VHD");
                        using (var vhdSession = new VirtualHardDiskLib.VirtualDiskSession(existingVHD: newvhd))
                        {
                            VHDMountPath = vhdSession.GetMountedPath();

                            result = UUPMediaCreator.CreateUpgradedEditionFromMountedImage(
                                VHDMountPath,
                                targetEdition.PlannedEdition.EditionName,
                                InstallWIMFilePath,
                                false,
                                CompressionType,
                                progressCallback);

                            if (!result)
                                goto exit;
                        }
                        break;
                    }
                case AvailabilityType.EditionPackageSwap:
                    {
                        if (targetEdition.PlannedEdition.EditionName.ToLower().StartsWith("starter"))
                        {
                            // TODO
                            // (Downgrade from core/coren to starter/startern)
                        }
                        else if (targetEdition.PlannedEdition.EditionName.ToLower().StartsWith("professionaln"))
                        {
                            // TODO
                            // (Downgrade from ppipro to pron)
                        }
                        else if (targetEdition.PlannedEdition.EditionName.ToLower().StartsWith("professional"))
                        {
                            // TODO
                            // (Downgrade from ppipro to pro)
                        }
                        else
                        {
                            var newvhd = VirtualHardDiskLib.VHDUtilities.CreateDiffDisk(CurrentBackupVHD);

                            progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, $"Mounting VHD");
                            using (var vhdSession = new VirtualHardDiskLib.VirtualDiskSession(existingVHD: newvhd))
                            {
                                VHDMountPath = vhdSession.GetMountedPath();

                                result = BootlegEditionCreator.CreateHackedEditionFromMountedImage(
                                    UUPPath,
                                    MediaPath,
                                    VHDMountPath,
                                    targetEdition.PlannedEdition.EditionName,
                                    InstallWIMFilePath,
                                    CompressionType,
                                    progressCallback);
                                if (!result)
                                    goto exit;
                            }
                        }
                        break;
                    }
            }

            if (targetEdition.DestructiveTargets.Count > 0 || targetEdition.NonDestructiveTargets.Count > 0)
            {
                string vhdpath = null;

                using (var vhdSession = new VirtualHardDiskLib.VirtualDiskSession(delete: false))
                {
                    // Apply WIM
                    WIMInformationXML.WIM wiminfo;
                    imagingInterface.GetWIMInformation(InstallWIMFilePath, out wiminfo);

                    var index = int.Parse(wiminfo.IMAGE.First(x => x.WINDOWS.EDITIONID.Equals(targetEdition.PlannedEdition.EditionName, StringComparison.InvariantCultureIgnoreCase)).INDEX);

                    void callback(string Operation, int ProgressPercentage, bool IsIndeterminate)
                    {
                        progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, IsIndeterminate, ProgressPercentage, Operation);
                    };
                    result = imagingInterface.ApplyImage(InstallWIMFilePath, index, vhdSession.GetMountedPath(), progressCallback: callback);
                    if (!result)
                        goto exit;

                    vhdpath = vhdSession.VirtualDiskPath;
                }

                if (targetEdition.NonDestructiveTargets.Count > 0)
                {
                    var newvhd = VirtualHardDiskLib.VHDUtilities.CreateDiffDisk(vhdpath);

                    progressCallback?.Invoke(Common.ProcessPhase.ApplyingImage, true, 0, $"Mounting VHD");

                    using (var vhdSession = new VirtualHardDiskLib.VirtualDiskSession(existingVHD: newvhd))
                    {
                        foreach (var ed in targetEdition.NonDestructiveTargets)
                        {
                            result = HandleEditionPlan(
                                ed,
                                UUPPath,
                                MediaPath,
                                LanguageCode,
                                InstallWIMFilePath,
                                WinREWIMFilePath,
                                CompressionType,
                                VHDMountPath: vhdSession.GetMountedPath(),
                                CurrentBackupVHD: vhdpath,
                                progressCallback: progressCallback);

                            if (!result)
                                goto exit;
                        }
                    }
                }

                if (targetEdition.DestructiveTargets.Count > 0)
                {
                    foreach (var ed in targetEdition.DestructiveTargets)
                    {
                        result = HandleEditionPlan(
                            ed,
                            UUPPath,
                            MediaPath,
                            LanguageCode,
                            InstallWIMFilePath,
                            WinREWIMFilePath,
                            CompressionType,
                            CurrentBackupVHD: vhdpath,
                            progressCallback: progressCallback);

                        if (!result)
                            goto exit;
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

            if (filteredCompDBs.Count() > 0)
            {
                foreach (CompDBXmlClass.Package feature in filteredCompDBs.First().Features.Feature[0].Packages.Package)
                {
                    CompDBXmlClass.Package pkg = filteredCompDBs.First().Packages.Package.First(x => x.ID == feature.ID);

                    string file = pkg.GetCommonlyUsedIncorrectFileName();

                    //
                    // We know already that all files exist, so it's just a matter of knowing which path format is used
                    //
                    file = !File.Exists(Path.Combine(UUPPath, file)) ? pkg.Payload.PayloadItem.Path.Replace('\\', Path.DirectorySeparatorChar) : file;

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

        public static void CreateISOMediaAdvanced(
            string ISOPath,
            string UUPPath,
            string LanguageCode,
            bool IntegrateUpdates,
            Common.CompressionType CompressionType,
            ProgressCallback progressCallback = null)
        {
            bool result = true;
            string error = "";

            List<EditionTarget> editionTargets;
            result = GetTargetedPlan(UUPPath, LanguageCode, out editionTargets, progressCallback);
            if (!result)
            {
                error = "An error occurred while getting target plans for the conversion.";
                goto error;
            }

            foreach (var ed in editionTargets)
            {
                foreach (var line in ConversionPlanBuilder.PrintEditionTarget(ed))
                {
                    progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, line);
                }
            }

            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            var temp = TempManager.TempManager.Instance.GetTempPath();
            Directory.CreateDirectory(temp);

            string WinREWIMFilePath = Path.Combine(temp, "Winre.wim");
            string MediaRootPath = Path.Combine(temp, "MediaRoot");
            string InstallWIMFilePath = CompressionType == Common.CompressionType.LZMS ?
                Path.Combine(MediaRootPath, "sources", "install.esd") :
                Path.Combine(MediaRootPath, "sources", "install.wim");

            //
            // Build installer
            //
            result = SetupMediaCreator.CreateSetupMedia(UUPPath, LanguageCode, MediaRootPath, WinREWIMFilePath, CompressionType, progressCallback);
            if (!result)
            {
                error = "An error occurred while creating setup media.";
                goto error;
            }

            //
            // Build Install.WIM/ESD
            //
            foreach (var ed in editionTargets)
            {
                result = HandleEditionPlan(ed, UUPPath, MediaRootPath, LanguageCode, InstallWIMFilePath, WinREWIMFilePath, CompressionType, progressCallback: progressCallback);
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

            error:
            progressCallback?.Invoke(Common.ProcessPhase.Error, true, 0, error);

            exit:
            TempManager.TempManager.Instance.Dispose();
            return;
        }

        public static void CreateISOMedia(
            string ISOPath,
            string UUPPath,
            string Edition,
            string LanguageCode,
            bool IntegrateUpdates,
            Common.CompressionType CompressionType,
            ProgressCallback progressCallback = null)
        {
            string error = "";
            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            var temp = TempManager.TempManager.Instance.GetTempPath();
            Directory.CreateDirectory(temp);

            string WinREWIMFilePath = Path.Combine(temp, "Winre.wim");
            string MediaRootPath = Path.Combine(temp, "MediaRoot");
            string InstallWIMFilePath = CompressionType == Common.CompressionType.LZMS ?
                Path.Combine(MediaRootPath, "sources", "install.esd") :
                Path.Combine(MediaRootPath, "sources", "install.wim");

            //
            // Build installer
            //
            bool result = SetupMediaCreator.CreateSetupMedia(UUPPath, LanguageCode, MediaRootPath, WinREWIMFilePath, CompressionType, progressCallback);
            if (!result)
            {
                error = "An error occurred while creating setup media.";
                goto error;
            }

            //
            // Build Install.WIM/ESD
            //
            result = BaseEditionBuilder.CreateBaseEdition(UUPPath, LanguageCode, Edition, WinREWIMFilePath, InstallWIMFilePath, CompressionType, progressCallback);
            if (!result)
            {
                error = "An error occurred while handling edition plan for the following edition: " + Edition;
                goto error;
            }

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

        error:
            progressCallback?.Invoke(Common.ProcessPhase.Error, true, 0, error);

        exit:
            TempManager.TempManager.Instance.Dispose();
            return;
        }
    }
}