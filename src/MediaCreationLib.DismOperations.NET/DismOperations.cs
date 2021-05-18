using Microsoft.Dism;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MediaCreationLib.Dism
{
    public class DismOperations
    {
        public delegate void ProgressCallback(bool IsIndeterminate, int ProgressInPercentage, string SubOperation);

        private static string[] componentsNotInWinPE = new string[]
        {
            "Microsoft-Windows-ImageBasedSetup-Rejuvenation-Package-onecore~31bf3856ad364e35",
            "Microsoft-Windows-ImageBasedSetup-Rejuvenation-Package~31bf3856ad364e35",
            "Microsoft-Windows-OneCoreUAP-WCN-Package~31bf3856ad364e35",
            "Microsoft-Windows-WCN-net-Package~31bf3856ad364e35",
            "Microsoft-Windows-WCN-Package~31bf3856ad364e35",
            //"Microsoft-Windows-WinPE-AppxPackaging-Package~31bf3856ad364e35",
            "Microsoft-Windows-WinPE-Fonts-Legacy-onecoreuap-Package~31bf3856ad364e35",
            "Microsoft-Windows-WinPE-Fonts-Legacy-Package~31bf3856ad364e35",
            "Microsoft-Windows-WinPE-Fonts-Legacy-windows-Package~31bf3856ad364e35",
            "Microsoft-Windows-WinPE-FontSupport-WinRE-onecoreuap-Package~31bf3856ad364e35",
            "Microsoft-Windows-WinPE-FontSupport-WinRE-Package~31bf3856ad364e35",
            "Microsoft-Windows-WinPE-FontSupport-WinRE-windows-Package~31bf3856ad364e35",
            //"Microsoft-Windows-WinPE-OpcServices-Package~31bf3856ad364e35",
            "WinPE-AppxPackaging-Package~31bf3856ad364e35",
            "WinPE-FMAPI-Package~31bf3856ad364e35",
            "WinPE-HTA-Package-com~31bf3856ad364e35",
            "WinPE-HTA-Package-inetcore~31bf3856ad364e35",
            "WinPE-HTA-Package-onecoreuapwindows~31bf3856ad364e35",
            "WinPE-HTA-Package-onecoreuap~31bf3856ad364e35",
            "WinPE-HTA-Package-onecore~31bf3856ad364e35",
            "WinPE-HTA-Package-shell~31bf3856ad364e35",
            "WinPE-HTA-Package-windows~31bf3856ad364e35",
            "WinPE-HTA-Package~31bf3856ad364e35",
            "WinPE-OneCoreUAP-WiFi-Package~31bf3856ad364e35",
            "WinPE-OpcServices-Package~31bf3856ad364e35",
            "WinPE-Rejuv-Package~31bf3856ad364e35",
            "WinPE-Setup-Client-Package~31bf3856ad364e35",
            "WinPE-Setup-Package~31bf3856ad364e35",
            "WinPE-StorageWMI-Package-onecore~31bf3856ad364e35",
            "WinPE-StorageWMI-Package-servercommon~31bf3856ad364e35",
            "WinPE-StorageWMI-Package~31bf3856ad364e35",
            "WinPE-WiFi-Package~31bf3856ad364e35",
        };

        /// <summary>
        /// Uninstalls unneeded Windows Components for Windows Setup Preinstallation-Environment
        /// TODO: Get the list from session files instead 
        /// </summary>
        /// <param name="ospath">Path to the operating system</param>
        /// <param name="progressCallback">Callback to be notified of progress</param>
        public static void UninstallPEComponents(string ospath, ProgressCallback progressCallback)
        {
            //
            // Initialize DISM log
            //
            string tempLog = Path.GetTempFileName();
            DismApi.Initialize(DismLogLevel.LogErrorsWarningsInfo, tempLog);

            var session = DismApi.OpenOfflineSession(ospath);
            var packages = DismApi.GetPackages(session);
            List<DismPackage> componentsToRemove = new List<DismPackage>();

            //
            // Queue components we don't need according to our hardcoded list for removal
            //
            foreach (var package in packages)
            {
                if (componentsNotInWinPE.Any(x => package.PackageName.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                {
                    componentsToRemove.Add(package);
                }
            }

            //
            // Remove components
            //
            foreach (var pkg in componentsToRemove)
            {
                try
                {
                    void callback3(DismProgress progress)
                    {
                        progressCallback?.Invoke(false, (int)Math.Round((double)progress.Current / progress.Total * 100), "Removing " + pkg.PackageName);
                    }
                    DismApi.RemovePackageByName(session, pkg.PackageName, callback3);
                }
                catch //(Exception ex)
                {
                }
            }

            //
            // Clean DISM
            //
            try
            {
                DismApi.CloseSession(session);
            }
            catch { }

            try
            {
                DismApi.Shutdown();
            }
            catch { }

            try
            {
                File.Delete(tempLog);
            }
            catch { }
        }

        public static string GetCurrentEdition(string ospath)
        {
            //
            // Initialize DISM log
            //
            string tempLog = Path.GetTempFileName();
            DismApi.Initialize(DismLogLevel.LogErrorsWarningsInfo, tempLog);

            var session = DismApi.OpenOfflineSession(ospath);

            string edition = DismApi.GetCurrentEdition(session);

            //
            // Clean DISM
            //
            try
            {
                DismApi.CloseSession(session);
            }
            catch { }

            try
            {
                DismApi.Shutdown();
            }
            catch { }

            try
            {
                File.Delete(tempLog);
            }
            catch { }

            return edition;
        }

        public static void ApplyUnattend(string ospath, string unattendpath)
        {
            int counter = 0;
            //
            // Initialize DISM log
            //
            tryagain:
            string tempLog = Path.GetTempFileName();
            DismApi.Initialize(DismLogLevel.LogErrorsWarningsInfo, tempLog);
            DismSession session = DismApi.OpenOfflineSession(ospath);

            try
            {
                DismApi.ApplyUnattend(session, unattendpath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed applying unattend, retrying in one second...");
                Console.WriteLine("Failed attempt #" + counter);
                Console.WriteLine(ex.ToString());

                //
                // Clean DISM
                //
                try
                {
                    DismApi.CloseSession(session);
                }
                catch { }

                try
                {
                    DismApi.Shutdown();
                }
                catch { }

                if (counter < 3)
                {
                    Thread.Sleep(1000);
                    counter++;
                    goto tryagain;
                }
            }

            //
            // Clean DISM
            //
            try
            {
                DismApi.CloseSession(session);
            }
            catch { }

            try
            {
                DismApi.Shutdown();
            }
            catch { }

            try
            {
                File.Delete(tempLog);
            }
            catch { }
        }

        public static void SetProductKey(string ospath, string productkey)
        {
            //
            // Initialize DISM log
            //
            string tempLog = Path.GetTempFileName();
            DismApi.Initialize(DismLogLevel.LogErrorsWarningsInfo, tempLog);

            var session = DismApi.OpenOfflineSession(ospath);

            DismApi.SetProductKey(session, productkey);

            //
            // Clean DISM
            //
            try
            {
                DismApi.CloseSession(session);
            }
            catch { }

            try
            {
                DismApi.Shutdown();
            }
            catch { }

            try
            {
                File.Delete(tempLog);
            }
            catch { }
        }

        public static void SetTargetEdition(string ospath, string edition, ProgressCallback progressCallback)
        {
            int counter = 0;
            //
            // Initialize DISM log
            //
            tryagain:
            string tempLog = Path.GetTempFileName();
            DismApi.Initialize(DismLogLevel.LogErrorsWarningsInfo, tempLog);
            DismSession session = DismApi.OpenOfflineSession(ospath);

            try
            {
                void callback3(DismProgress progress)
                {
                    progressCallback?.Invoke(false, (int)Math.Round((double)progress.Current / progress.Total * 100), "Setting edition " + edition);
                }
                DismApi.SetEdition(session, edition, callback3);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed setting edition, retrying in one second...");
                Console.WriteLine("Failed attempt #" + counter);
                Console.WriteLine(ex.ToString());

                //
                // Clean DISM
                //
                try
                {
                    DismApi.CloseSession(session);
                }
                catch { }

                try
                {
                    DismApi.Shutdown();
                }
                catch { }

                if (counter < 3)
                {
                    Thread.Sleep(1000);
                    counter++;
                    goto tryagain;
                }
            }

            //
            // Clean DISM
            //
            try
            {
                DismApi.CloseSession(session);
            }
            catch { }

            try
            {
                DismApi.Shutdown();
            }
            catch { }

            try
            {
                File.Delete(tempLog);
            }
            catch { }
        }
    }
}