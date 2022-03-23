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
using System.IO;
using System.Diagnostics;
using MediaCreationLib.Planning.Applications;

namespace MediaCreationLib.Dism
{
    public class RemoteDismOperations : IDismOperations
    {
        public static readonly RemoteDismOperations Instance = new();

        private static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        private static string DismBrokerInstalledLocation;

        private static void SetupDismBroker()
        {
            if (DismBrokerInstalledLocation != null)
            {
                return;
            }

            bool shouldCopyDirectory = true;
            string parentDirectory = PathUtils.GetParentExecutableDirectory();
            string toolpath = Path.Combine(parentDirectory, "UUPMediaConverterDismBroker", "UUPMediaConverterDismBroker.exe");

            if (!File.Exists(toolpath))
            {
                parentDirectory = PathUtils.GetExecutableDirectory();
                toolpath = Path.Combine(parentDirectory, "UUPMediaConverterDismBroker", "UUPMediaConverterDismBroker.exe");
            }

            if (!File.Exists(toolpath))
            {
                parentDirectory = PathUtils.GetExecutableDirectory();
                toolpath = Path.Combine(parentDirectory, "UUPMediaConverterDismBroker.exe");
                shouldCopyDirectory = false;
            }

            if (!File.Exists(toolpath))
            {
                return;
            }

            string dst = Path.Combine(Path.GetTempPath(), "UUPMediaConverterDismBroker");
            Directory.CreateDirectory(dst);
            if (shouldCopyDirectory)
            {
                CopyFolder(toolpath.Replace(@"\UUPMediaConverterDismBroker.exe", ""), dst);
            }
            else
            {
                File.Copy(toolpath, Path.Combine(dst, "UUPMediaConverterDismBroker.exe"), true);
            }
            toolpath = Path.Combine(dst, "UUPMediaConverterDismBroker.exe");

            DismBrokerInstalledLocation = toolpath;
        }

        public bool PerformAppxWorkloadInstallation(string ospath, string repositoryPath, string licenseFolder, AppxInstallWorkload workload)
        {
            SetupDismBroker();

            if (DismBrokerInstalledLocation == null || !File.Exists(DismBrokerInstalledLocation))
            {
                return false;
            }

            Process proc = new();
            proc.StartInfo = new ProcessStartInfo("cmd.exe", $"/c \"\"{DismBrokerInstalledLocation}\" /InstallAppXWorkload \"{ospath}\" \"{repositoryPath}\" \"{licenseFolder}\" \"{System.Text.Json.JsonSerializer.Serialize(workload).Replace("\"", "\"\"")}\"\"")
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();
            return proc.ExitCode == 0;
        }

        public bool PerformAppxWorkloadsInstallation(string ospath, string repositoryPath, string licenseFolder, AppxInstallWorkload[] workloads, ProgressCallback progressCallback)
        {
            SetupDismBroker();

            if (DismBrokerInstalledLocation == null || !File.Exists(DismBrokerInstalledLocation))
            {
                progressCallback?.Invoke(true, 0, "Cannot find the external tool for appx installation.");
                return false;
            }

            Process proc = new();
            string workloadsArgument = $"\"{System.Text.Json.JsonSerializer.Serialize(workloads).Replace("\"", "\"\"")}\"";
            proc.StartInfo = new ProcessStartInfo("cmd.exe", $"/c \"\"{DismBrokerInstalledLocation}\" /InstallAppXWorkloads \"{ospath}\" \"{repositoryPath}\" \"{licenseFolder}\" {workloadsArgument}\"")
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            proc.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (e.Data?.Contains(",") == true)
                {
                    int percent = int.Parse(e.Data.Split(',')[0]);
                    progressCallback?.Invoke(false, percent, e.Data.Split(',')[1]);
                }
            };
            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                progressCallback?.Invoke(true, 0, "An error occured while running the external tool for appx installation. Error code: " + proc.ExitCode);
            }
            return proc.ExitCode == 0;
        }

        /// <summary>
        /// Uninstalls unneeded Windows Components for Windows Setup Preinstallation-Environment
        /// </summary>
        /// <param name="ospath">Path to the operating system</param>
        /// <param name="progressCallback">Callback to be notified of progress</param>
        public bool UninstallPEComponents(string ospath, ProgressCallback progressCallback)
        {
            SetupDismBroker();

            if (DismBrokerInstalledLocation == null || !File.Exists(DismBrokerInstalledLocation))
            {
                progressCallback?.Invoke(true, 0, "Cannot find the external tool for component cleanup.");
                return false;
            }

            Process proc = new();
            proc.StartInfo = new ProcessStartInfo("cmd.exe", $"/c \"\"{DismBrokerInstalledLocation}\" /PECompUninst \"{ospath}\"\"")
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            proc.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (e.Data?.Contains(",") == true)
                {
                    int percent = int.Parse(e.Data.Split(',')[0]);
                    progressCallback?.Invoke(false, percent, e.Data.Split(',')[1]);
                }
            };
            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                progressCallback?.Invoke(true, 0, "An error occured while running the external tool for component cleanup. Error code: " + proc.ExitCode);
            }
            return proc.ExitCode == 0;
        }

        public string GetCurrentEdition(string ospath)
        {
            return "";
        }

        public void ApplyUnattend(string ospath, string unattendpath)
        {

        }

        public void SetProductKey(string ospath, string productkey)
        {

        }

        public void SetTargetEdition(string ospath, string edition, ProgressCallback progressCallback)
        {

        }
    }
}
