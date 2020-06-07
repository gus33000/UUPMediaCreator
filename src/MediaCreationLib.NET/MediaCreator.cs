using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Imaging;
using MediaCreationLib.BaseEditions;
using MediaCreationLib.BootlegEditions;
using MediaCreationLib.Installer;
using UUPMediaCreator.InterCommunication;
using static MediaCreationLib.Planning.ConversionPlanBuilder;

namespace MediaCreationLib
{
    public class MediaCreator
    {
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

        public static void CreateISOMediaAdvanced(
            string ISOPath,
            string UUPPath,
            string LanguageCode,
            bool IntegrateUpdates,
            Common.CompressionType CompressionType,
            ProgressCallback progressCallback = null)
        {
            bool result = true;

            List<EditionTarget> editionTargets;
            result = GetTargetedPlan(UUPPath, LanguageCode, out editionTargets, progressCallback);
            if (!result)
                goto error;

            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            string WinREWIMFilePath = Path.Combine(UUPPath, "Winre.wim");
            string MediaRootPath = Path.Combine(UUPPath, "MediaRoot");
            string InstallWIMFilePath = CompressionType == Common.CompressionType.LZMS ?
                Path.Combine(MediaRootPath, "sources", "install.esd") :
                Path.Combine(MediaRootPath, "sources", "install.wim");

            //
            // Build installer
            //
            result = SetupMediaCreator.CreateSetupMedia(UUPPath, LanguageCode, MediaRootPath, WinREWIMFilePath, CompressionType, progressCallback);
            if (!result)
                goto error;

            //
            // Build Install.WIM/ESD
            //
            foreach (var ed in editionTargets)
            {
                result = HandleEditionPlan(ed, UUPPath, MediaRootPath, LanguageCode, InstallWIMFilePath, WinREWIMFilePath, CompressionType, progressCallback: progressCallback);
                if (!result)
                    goto error;
            }

            BootlegEditionCreator.CleanupLanguagePackFolderIfRequired();

            //
            // Build ISO
            //
            result = UUPMediaCreator.CreateISO(MediaRootPath, ISOPath, progressCallback);
            if (!result)
                goto error;

            progressCallback?.Invoke(Common.ProcessPhase.Done, true, 0, "");
            goto exit;

        error:
            progressCallback?.Invoke(Common.ProcessPhase.Error, true, 0, "");

        exit:
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
            progressCallback?.Invoke(Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            string WinREWIMFilePath = Path.Combine(UUPPath, "Winre.wim");
            string MediaRootPath = Path.Combine(UUPPath, "MediaRoot");
            string InstallWIMFilePath = CompressionType == Common.CompressionType.LZMS ?
                Path.Combine(MediaRootPath, "sources", "install.esd") :
                Path.Combine(MediaRootPath, "sources", "install.wim");

            //
            // Build installer
            //
            bool result = SetupMediaCreator.CreateSetupMedia(UUPPath, LanguageCode, MediaRootPath, WinREWIMFilePath, CompressionType, progressCallback);
            if (!result)
                goto error;

            //
            // Build Install.WIM/ESD
            //
            result = BaseEditionBuilder.CreateBaseEdition(UUPPath, LanguageCode, Edition, WinREWIMFilePath, InstallWIMFilePath, CompressionType, progressCallback);
            if (!result)
                goto error;

            //
            // Build ISO
            //
            result = UUPMediaCreator.CreateISO(MediaRootPath, ISOPath, progressCallback);
            if (!result)
                goto error;

            progressCallback?.Invoke(Common.ProcessPhase.Done, true, 0, "");
            goto exit;

        error:
            progressCallback?.Invoke(Common.ProcessPhase.Error, true, 0, "");

        exit:
            return;
        }
    }
}