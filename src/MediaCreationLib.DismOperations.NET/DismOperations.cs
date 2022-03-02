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
using Microsoft.Dism;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace MediaCreationLib.Dism
{
    public static class DismOperations
    {
        public delegate void ProgressCallback(bool IsIndeterminate, int ProgressInPercentage, string SubOperation);

        private static List<string> GetRemovableREPackages(string mountDir)
        {
            XDocument sessDoc = XDocument.Load(Path.Combine(mountDir, @"Windows\servicing\Sessions\Sessions.xml"));
            IEnumerable<XElement> sessions = sessDoc.Element("Sessions").Elements("Session");
            bool dupeFound = false;
            List<string> pkgsToRemove = new List<string>();
            foreach (XElement test in sessions)
            {
                bool phasesEmpty = !test.Element("Actions").Elements("Phase").First().Elements().Any();
                if (phasesEmpty && !dupeFound)
                    dupeFound = true;
                if (dupeFound && !phasesEmpty)
                {
                    string pkgName = test.Element("Tasks").Elements("Phase").First().Element("package").Attribute("id").Value;
                    if (pkgName.Contains("~~"))
                        pkgsToRemove.Add(pkgName);
                }
            }
            return pkgsToRemove;
        }

        public static bool PerformAppxWorkloadInstallation(string ospath, string repositoryPath, AppxInstallWorkload workload)
        {
            bool result = true;

            //
            // Initialize DISM log
            //
            string tempLog = Path.GetTempFileName();
            DismApi.Initialize(DismLogLevel.LogErrorsWarningsInfo, tempLog);

            DismSession session = DismApi.OpenOfflineSession(ospath);

            try
            {
                DismApi.AddProvisionedAppxPackage(
                    session,
                    Path.Combine(repositoryPath, workload.AppXPath),
                    workload.DependenciesPath?.Select(x => Path.Combine(repositoryPath, x)).ToList() ?? new List<string>(),
                    string.IsNullOrEmpty(workload.LicensePath) ? null : Path.Combine(repositoryPath, workload.LicensePath),
                    null,
                    string.IsNullOrEmpty(workload.StubPackageOption) ? Microsoft.Dism.StubPackageOption.None : Microsoft.Dism.StubPackageOption.InstallStub); // TODO: proper handling
            }
            catch { result = false; }

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

            return result;
        }

        /// <summary>
        /// Uninstalls unneeded Windows Components for Windows Setup Preinstallation-Environment
        /// TODO: Get the list from session files instead
        /// </summary>
        /// <param name="ospath">Path to the operating system</param>
        /// <param name="progressCallback">Callback to be notified of progress</param>
        public static void UninstallPEComponents(string ospath, ProgressCallback progressCallback)
        {
            List<string> componentsNotInWinPE = GetRemovableREPackages(ospath);

            //
            // Initialize DISM log
            //
            string tempLog = Path.GetTempFileName();
            DismApi.Initialize(DismLogLevel.LogErrorsWarningsInfo, tempLog);

            DismSession session = DismApi.OpenOfflineSession(ospath);
            DismPackageCollection packages = DismApi.GetPackages(session);
            List<DismPackage> componentsToRemove = new();

            //
            // Queue components we don't need according to our hardcoded list for removal
            //
            foreach (DismPackage package in packages)
            {
                if (componentsNotInWinPE.Any(x => package.PackageName.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                {
                    componentsToRemove.Add(package);
                }
            }

            //
            // Remove components
            //
            foreach (DismPackage pkg in componentsToRemove)
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

            DismSession session = DismApi.OpenOfflineSession(ospath);

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

            DismSession session = DismApi.OpenOfflineSession(ospath);

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