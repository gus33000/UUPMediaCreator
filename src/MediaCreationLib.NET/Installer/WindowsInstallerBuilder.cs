using Imaging;
using Microsoft.Wim;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UUPMediaCreator.InterCommunication;
using VirtualHardDiskLib;
using static MediaCreationLib.MediaCreator;

namespace MediaCreationLib.Installer
{
    public class WindowsInstallerBuilder
    {
        private static WIMImaging imagingInterface = new WIMImaging();

        // 6 progress bars
        public static bool BuildSetupMedia(
            string BaseESD,
            string OutputWinREPath,
            string MediaPath,
            WimCompressionType compressionType,
            bool RunsAsAdministrator,
            string LanguageCode,
            ProgressCallback progressCallback = null
            )
        {
            //
            // First create the setup media base, minus install.wim and boot.wim
            //
            bool result = CreateSetupMediaRoot(BaseESD, MediaPath, progressCallback);
            if (!result)
                goto exit;

            //
            // Gather information about the Windows Recovery Environment image so we can transplant it later
            // into our new images
            //
            WIMInformationXML.IMAGE image;
            result = imagingInterface.GetWIMImageInformation(BaseESD, 2, out image);
            if (!result)
                goto exit;

            //
            // Gather the architecture string under parenthesis for the new images we are creating
            //
            string ArchitectureInNameAndDescription = image.NAME.Split('(')[1].Replace(")", "");

            string BootFirstImageName = $"Microsoft Windows PE ({ArchitectureInNameAndDescription})";
            string BootSecondImageName = $"Microsoft Windows Setup ({ArchitectureInNameAndDescription})";
            string BootFirstImageFlag = "9";
            string BootSecondImageFlag = "2";

            //
            // Bootable wim files must not be lzms
            //
            if (compressionType == WimCompressionType.Lzms)
                compressionType = WimCompressionType.Lzx;

            void callback(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, IsIndeterminate, ProgressPercentage, Operation);
            }

            //
            // Prepare our base PE image which will serve as a basis for all subsequent operations
            // This function also generates WinRE
            //
            result = PreparePEImage(BaseESD, OutputWinREPath, MediaPath, compressionType, LanguageCode, progressCallback);
            if (!result)
                goto exit;

            //
            // If we are running as administrator, perform additional component cleanup
            //
            if (RunsAsAdministrator)
            {
                result = PerformComponentCleanupOnPEImage(MediaPath, compressionType, image, progressCallback);
                if (!result)
                    goto exit;
            }

            string bootwim = Path.Combine(MediaPath, "sources", "boot.wim");

            string tmpwimcopy = Path.GetTempFileName();
            File.Delete(tmpwimcopy);
            File.Copy(bootwim, tmpwimcopy);

            //
            // Duplicate the boot image so we have two of them
            //
            result = imagingInterface.ExportImage(tmpwimcopy, bootwim, 1, compressionType: compressionType, progressCallback: callback);
            if (!result)
                goto exit;

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
            result = imagingInterface.SetWIMImageInformation(bootwim, 1, image);
            if (!result)
                goto exit;

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
            result = imagingInterface.SetWIMImageInformation(bootwim, 2, image);
            if (!result)
                goto exit;

            //
            // Mark image as bootable
            //
            result = imagingInterface.MarkImageAsBootable(bootwim, 2);
            if (!result)
                goto exit;

            //
            // Modifying registry for each index
            //
            string tempSoftwareHiveBackup = Path.GetTempFileName();
            string tempSystemHiveBackup = Path.GetTempFileName();

            result = imagingInterface.ExtractFileFromImage(bootwim, 1, Constants.SYSTEM_Hive_Location, tempSystemHiveBackup);
            if (!result)
                goto exit;

            result = imagingInterface.ExtractFileFromImage(bootwim, 1, Constants.SOFTWARE_Hive_Location, tempSoftwareHiveBackup);
            if (!result)
                goto exit;

            File.Copy(tempSoftwareHiveBackup, $"{tempSoftwareHiveBackup}.2");

            result = RegistryOperations.ModifyBootIndex2Registry($"{tempSoftwareHiveBackup}.2");
            if (!result)
                goto exit;

            result = imagingInterface.AddFileToImage(bootwim, 2, $"{tempSoftwareHiveBackup}.2", Constants.SOFTWARE_Hive_Location, progressCallback: callback);
            if (!result)
                goto exit;

            File.Delete($"{tempSoftwareHiveBackup}.2");

            result = RegistryOperations.ModifyBootIndex1Registry(tempSystemHiveBackup, tempSoftwareHiveBackup);
            if (!result)
                goto exit;

            result = imagingInterface.AddFileToImage(bootwim, 1, tempSystemHiveBackup, Constants.SYSTEM_Hive_Location, progressCallback: callback);
            if (!result)
                goto exit;

            result = imagingInterface.AddFileToImage(bootwim, 1, tempSoftwareHiveBackup, Constants.SOFTWARE_Hive_Location, progressCallback: callback);
            if (!result)
                goto exit;

            File.Delete(tempSoftwareHiveBackup);
            File.Delete(tempSystemHiveBackup);

            //
            // Adding missing files in index 2
            //
            progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, true, 0, "Modifying assets for Setup PE (1)");
            result = imagingInterface.RenameFileInImage(bootwim, 2, Path.Combine("Windows", "System32", "winpe.jpg"), Path.Combine("Windows", "System32", "setup.bmp"), progressCallback: callback);
            if (!result)
                goto exit;

            progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, true, 0, "Modifying assets for Setup PE (2)");
            var winpejpgtmp = Path.GetTempFileName();
            File.WriteAllBytes(winpejpgtmp, Constants.winpejpg);
            result = imagingInterface.AddFileToImage(bootwim, 2, winpejpgtmp, Path.Combine("Windows", "System32", "winpe.jpg"), progressCallback: callback);
            File.Delete(winpejpgtmp);
            if (!result)
                goto exit;

            progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, true, 0, "Backporting missing files");

            var dirs = Directory.EnumerateDirectories(Path.Combine(MediaPath, "sources"), "??-??");
            if (dirs.Count() == 0)
            {
                dirs = Directory.EnumerateDirectories(Path.Combine(MediaPath, "sources"), "*-*");
            }
            string langcode = dirs.First().Replace(Path.Combine(MediaPath, "sources") + "\\", "");

            foreach (string file in Constants.SetupFilesToBackport)
            {
                string matchingfile = Path.Combine(MediaPath, file).Replace("??-??", langcode);
                string normalizedPath = file.Replace("??-??", langcode);
                string normalizedPathWithoutFile = normalizedPath.Contains("\\") ? string.Join("\\", normalizedPath.Split('\\').Reverse().Skip(1).Reverse()) : "";

                if (file == "sources\\background.bmp")
                {
                    string matchingfile1 = Path.Combine(MediaPath, "sources", "background_cli.bmp");
                    string matchingfile2 = Path.Combine(MediaPath, "sources", "background_srv.bmp");
                    if (File.Exists(matchingfile1))
                    {
                        result = imagingInterface.AddFileToImage(bootwim, 2, matchingfile1, normalizedPath, progressCallback: callback);
                        if (!result)
                            goto exit;
                    }
                    else if (File.Exists(matchingfile2))
                    {
                        result = imagingInterface.AddFileToImage(bootwim, 2, matchingfile2, normalizedPath, progressCallback: callback);
                        if (!result)
                            goto exit;
                    }
                }
                else if (File.Exists(matchingfile))
                {
                    result = imagingInterface.AddFileToImage(bootwim, 2, matchingfile, normalizedPath, progressCallback: callback);
                    if (!result)
                        goto exit;
                }
            }

            if (ulong.Parse(image.WINDOWS.VERSION.BUILD) >= 20231)
            {
                foreach (string file in Constants.SetupFilesToBackportStartingWith20231)
                {
                    string matchingfile = Path.Combine(MediaPath, file).Replace("??-??", langcode);
                    string normalizedPath = file.Replace("??-??", langcode);
                    string normalizedPathWithoutFile = normalizedPath.Contains("\\") ? string.Join("\\", normalizedPath.Split('\\').Reverse().Skip(1).Reverse()) : "";

                    if (file == "sources\\background.bmp")
                    {
                        string matchingfile1 = Path.Combine(MediaPath, "sources", "background_cli.bmp");
                        string matchingfile2 = Path.Combine(MediaPath, "sources", "background_srv.bmp");
                        if (File.Exists(matchingfile1))
                        {
                            result = imagingInterface.AddFileToImage(bootwim, 2, matchingfile1, normalizedPath, progressCallback: callback);
                            if (!result)
                                goto exit;
                        }
                        else if (File.Exists(matchingfile2))
                        {
                            result = imagingInterface.AddFileToImage(bootwim, 2, matchingfile2, normalizedPath, progressCallback: callback);
                            if (!result)
                                goto exit;
                        }
                    }
                    else if (File.Exists(matchingfile))
                    {
                        result = imagingInterface.AddFileToImage(bootwim, 2, matchingfile, normalizedPath, progressCallback: callback);
                        if (!result)
                            goto exit;
                    }
                }
            }

            //
            // We're done
            //

        exit:
            return result;
        }

        private static bool PreparePEImage(
            string BaseESD,
            string OutputWinREPath,
            string MediaPath,
            WimCompressionType compressionType,
            string LanguageCode,
            ProgressCallback progressCallback = null
            )
        {
            //
            // Export the RE image to our re path, in this case a WIM
            //
            void callback(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, IsIndeterminate, ProgressPercentage, Operation);
            }

            bool result = imagingInterface.ExportImage(BaseESD, OutputWinREPath, 2, compressionType: compressionType, progressCallback: callback);
            if (!result)
                goto exit;

            WIMInformationXML.IMAGE image;
            result = imagingInterface.GetWIMImageInformation(OutputWinREPath, 1, out image);
            if (!result)
                goto exit;

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

                result = imagingInterface.SetWIMImageInformation(OutputWinREPath, 1, image);
                if (!result)
                    goto exit;
            }

            progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, true, 0, "Marking image as bootable");
            result = imagingInterface.MarkImageAsBootable(OutputWinREPath, 1);
            if (!result)
                goto exit;

            string bootwim = Path.Combine(MediaPath, "sources", "boot.wim");
            File.Copy(OutputWinREPath, bootwim);

            //
            // Cleanup WinPE Shell directive
            //
            string sys32 = Path.Combine("Windows", "System32");
            string peshellini = Path.Combine(sys32, "winpeshl.ini");

            // Ignore return result
            imagingInterface.DeleteFileFromImage(bootwim, 1, peshellini, progressCallback: callback);

            //
            // Cleanup log file from RE conversion phase mentions
            //
            try
            {
                progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, true, 0, "Cleaning log files");
                string logfile = Path.GetTempFileName();
                string pathinimage = Path.Combine("Windows", "INF", "setupapi.offline.log");

                bool cresult = imagingInterface.ExtractFileFromImage(bootwim, 1, pathinimage, logfile);

                if (cresult)
                {
                    string[] lines = File.ReadAllLines(logfile);

                    int bootsessioncount = 0;
                    List<string> finallines = new List<string>();
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
                            imagingInterface.AddFileToImage(bootwim, 1, logfile, pathinimage, progressCallback: callback);
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
            progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, true, 0, "Disabling UMCI");
            string tempSystemHiveBackup = Path.GetTempFileName();

            result = imagingInterface.ExtractFileFromImage(bootwim, 1, Constants.SYSTEM_Hive_Location, tempSystemHiveBackup);
            if (!result)
                goto cleanup;

            result = RegistryOperations.ModifyBootGlobalRegistry(tempSystemHiveBackup);
            if (!result)
                goto cleanup;

            result = imagingInterface.AddFileToImage(bootwim, 1, tempSystemHiveBackup, Constants.SYSTEM_Hive_Location, progressCallback: callback);
            if (!result)
                goto cleanup;

            cleanup:
            File.Delete(tempSystemHiveBackup);

        exit:
            return result;
        }

        private static bool PerformComponentCleanupOnPEImage(
            string MediaPath,
            WimCompressionType compressionType,
            WIMInformationXML.IMAGE image,
            ProgressCallback progressCallback = null
            )
        {
            using (VirtualDiskSession vhdsession = new VirtualDiskSession())
            {
                string ospath = vhdsession.GetMountedPath();

                //
                // Apply the RE image to our ospath, in this case our VHD
                //
                void callback(string Operation, int ProgressPercentage, bool IsIndeterminate)
                {
                    progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, IsIndeterminate, ProgressPercentage, Operation);
                }

                bool result = imagingInterface.ApplyImage(Path.Combine(MediaPath, "sources", "boot.wim"), 1, ospath, progressCallback: callback);
                if (!result)
                    goto exit;

                File.Delete(Path.Combine(MediaPath, "sources", "boot.wim"));

                result = RunDismComponentRemovalOperation(ospath, progressCallback);
                if (!result)
                    goto exit;

                //
                // Cleanup leftovers for WLAN
                //
                progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, true, 0, "Cleaning up leftovers");

                string winsxsfolder = Path.Combine(ospath, "Windows", "WinSxS");
                string winsxsManFolder = Path.Combine(winsxsfolder, "Manifests");

                IEnumerable<string> directoriesToCleanOut = Directory.EnumerateDirectories(winsxsfolder, "*_dual_netnwifi.inf_31bf3856ad364e35_*", SearchOption.TopDirectoryOnly);
                IEnumerable<string> manifestsToCleanOut = Directory.EnumerateFiles(winsxsManFolder, "*_dual_netnwifi.inf_31bf3856ad364e35_*", SearchOption.TopDirectoryOnly);

                foreach (string dir in directoriesToCleanOut)
                {
                    try
                    {
                        progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, true, 0, "Deleting " + dir);
                        TakeOwn.TakeOwnDirectory(dir);
                        Directory.Delete(dir, true);
                    }
                    catch { }
                }

                foreach (string file in manifestsToCleanOut)
                {
                    try
                    {
                        progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, true, 0, "Deleting " + file);
                        TakeOwn.TakeOwnFile(file);
                        File.Delete(file);
                    }
                    catch { }
                }

                //
                // Add missing files from the setup media root
                //
                progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, true, 0, "Adding missing files");

                if (!File.Exists(Path.Combine(ospath, "Windows", "System32", "ReAgent.dll")))
                    File.Copy(Path.Combine(MediaPath, "sources", "ReAgent.dll"), Path.Combine(ospath, "Windows", "System32", "ReAgent.dll"));
                if (!File.Exists(Path.Combine(ospath, "Windows", "System32", "unattend.dll")))
                    File.Copy(Path.Combine(MediaPath, "sources", "unattend.dll"), Path.Combine(ospath, "Windows", "System32", "unattend.dll"));
                if (!File.Exists(Path.Combine(ospath, "Windows", "System32", "wpx.dll")))
                    File.Copy(Path.Combine(MediaPath, "sources", "wpx.dll"), Path.Combine(ospath, "Windows", "System32", "wpx.dll"));

                result = imagingInterface.CaptureImage(
                    Path.Combine(MediaPath, "sources", "boot.wim"),
                    image.NAME,
                    image.DESCRIPTION,
                    image.FLAGS,
                    ospath,
                    progressCallback: callback,
                    compressionType: compressionType);
                if (!result)
                    goto exit;

                exit:
                return result;
            }
        }

        private static bool RunDismComponentRemovalOperation(
            string OSPath,
            ProgressCallback progressCallback = null
            )
        {
            string parentDirectory = PathUtils.GetParentExecutableDirectory();
            string toolpath = Path.Combine(parentDirectory, "UUPMediaCreator.DismBroker", "UUPMediaCreator.DismBroker.exe");
            if (!File.Exists(toolpath))
            {
                parentDirectory = PathUtils.GetExecutableDirectory();
                toolpath = Path.Combine(parentDirectory, "UUPMediaCreator.DismBroker", "UUPMediaCreator.DismBroker.exe");
            }

            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo("cmd.exe", $"/c \"\"{toolpath}\" /PECompUninst \"{OSPath}\"\"");

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;

            proc.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (e.Data != null && e.Data.Contains(","))
                {
                    var percent = int.Parse(e.Data.Split(',')[0]);
                    progressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, false, percent, e.Data.Split(',')[1]);
                }
            };
            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();
            return proc.ExitCode == 0;
        }

        private static bool CreateSetupMediaRoot(
            string BaseESD,
            string OutputPath,
            ProgressCallback ProgressCallback = null
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

            void callback(string Operation, int ProgressPercentage, bool IsIndeterminate)
            {
                ProgressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, IsIndeterminate, ProgressPercentage, Operation);
            }

            //
            // Apply the first index of the base ESD containing the setup files we need
            //
            result = imagingInterface.ApplyImage(
                BaseESD,
                1,
                OutputPath,
                progressCallback: callback,
                PreserveACL: false);
            if (!result)
                goto exit;

            //
            // The setup files from the first index are missing a single component (wtf?) so extract it from index 3 and place it in sources
            // Note: the file in question isn't in a wim that needs to be referenced, so we don't need to mention reference images.
            //
            ProgressCallback?.Invoke(Common.ProcessPhase.CreatingWindowsInstaller, true, 0, "Extracting XML Lite");
            result = imagingInterface.ExtractFileFromImage(BaseESD, 3, Path.Combine("Windows", "System32", "xmllite.dll"), Path.Combine(OutputPath, "sources", "xmllite.dll"));
            if (!result)
                goto exit;

            exit:
            return result;
        }
    }
}