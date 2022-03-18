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
using MediaCreationLib.Planning.Applications;

namespace MediaCreationLib.Dism
{
    public delegate void ProgressCallback(bool IsIndeterminate, int ProgressInPercentage, string SubOperation);

    public interface IDismOperations
    {
        public bool PerformAppxWorkloadInstallation(string ospath, string repositoryPath, string licenseFolder, AppxInstallWorkload workload);

        public bool PerformAppxWorkloadsInstallation(string ospath, string repositoryPath, string licenseFolder, AppxInstallWorkload[] workloads, ProgressCallback progressCallback);

        /// <summary>
        /// Uninstalls unneeded Windows Components for Windows Setup Preinstallation-Environment
        /// </summary>
        /// <param name="ospath">Path to the operating system</param>
        /// <param name="progressCallback">Callback to be notified of progress</param>
        public bool UninstallPEComponents(string ospath, ProgressCallback progressCallback);

        public string GetCurrentEdition(string ospath);

        public void ApplyUnattend(string ospath, string unattendpath);

        public void SetProductKey(string ospath, string productkey);

        public void SetTargetEdition(string ospath, string edition, ProgressCallback progressCallback);
    }
}
