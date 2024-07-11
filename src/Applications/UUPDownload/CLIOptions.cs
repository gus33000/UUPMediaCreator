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
using CommandLine;
using UnifiedUpdatePlatform.Services.WindowsUpdate;

namespace UUPDownload
{
    [Verb("request-download", isDefault: true, HelpText = "Request a download from zero using a different parameters. Use 'help' to see more options.")]
    internal class DownloadRequestOptions
    {
        [Option('s', "reporting-sku", HelpText = "The sku to report to the Windows Update servers." +
            
  Example: Professional, EnterpriseG. Enter 'type editions.txt' to see a full list of possibilities.' ", Required = true, Default = "Professional")]
        public OSSkuId ReportingSku
        {
            get; set;
        }

        [Option('v', "reporting-version", HelpText = "The version to report to the Windows Update servers.\n  Example: 10.0.21262.1000.rs_prerelease.201113-1441, 10.0.25832.1", Required = true)]
        public string ReportingVersion
        {
            get; set;
        }

        [Option('t', "machine-type", HelpText = "The architecture to report to the Windows Update servers.\n  Example: amd64, arm64, unknown, x86, wcemipsv2, axp, sh3, sh3dsp, sh4, sh5, arm, thumb, woa, am33, powerpc, powerpcfp, ia64, mips16, mipsfpu, mipsfpu16, ebc, m32r", Required = true)]
        public MachineType MachineType
        {
            get; set;
        }

        [Option('r', "flight-ring", HelpText = "The ring to report to the Windows Update servers.\n  Example: Retail, External, Internal", Required = true)]
        public string FlightRing
        {
            get; set;
        }

        [Option('c', "current-branch", HelpText = "The branch to report to the Windows Update servers.\n  Example: rs3_release, 19h1_release (TI), co_refresh, ni_release, ge_prerelease, ge_release", Required = true)]
        public string CurrentBranch
        {
            get; set;
        }

        [Option('b', "flighting-branch-name", HelpText = "The flighting branch name to report to the Windows Update servers.\n  Example: Retail, CanaryChannel, Dev, Beta, ReleasePreview (RP), WindowsInsiderSlow/Fast (WIS/WIF)", Required = false, Default = "")]
        public string FlightingBranchName
        {
            get; set;
        }

        [Option('y', "sync-current-version-only", HelpText = "Only get updates for the current version, enables getting cumulative updates.", Required = false, Default = false)]
        public bool SyncCurrentVersionOnly
        {
            get; set;
        }

        [Option('a', "branch-readiness-level", HelpText = "The branch readiness level to report to the Windows Update servers.\n  Example: CB, CBB, LTSB, LTSC", Required = false, Default = "CB")]
        public string BranchReadinessLevel
        {
            get; set;
        }

        [Option('o', "output-folder", HelpText = "The folder to use for downloading the update files.", Required = false, Default = ".")]
        public string OutputFolder
        {
            get; set;
        }

        [Option('e', "edition", HelpText = "The edition to get. Must be used with the language parameter. Omit either of these to download everything.\n  Example: Professional, EnterpriseG. Enter 'type editions.txt' to see a full list of possibilities.'", Required = false, Default = "")]
        public string Edition
        {
            get; set;
        }

        [Option('l', "language", HelpText = "The language to get. Must be used with the edition parameter. Omit either of these to download everything.\n  Example: en-US, de-DE, zh-CN", Required = false, Default = "")]
        public string Language
        {
            get; set;
        }

        [Option('z', "releasetype", HelpText = "The release type to report to the Windows Update servers." +
            
  Example: Production", Required = false, Default = "Production")]
        public string ReleaseType
        {
            get; set;
        }

        [Option('n', "contenttype", HelpText = "The content type to report to the Windows Update servers.\n  Example: Mainline, Custom", Required = false, Default = "Mainline")]
        public string ContentType
        {
            get; set;
        }

        [Option('m', "mail", HelpText = "Email for the Windows Insider account to use to generate authorization tokens. (Optional)", Required = false, Default = "")]
        public string Mail
        {
            get; set;
        }

        [Option('p', "password", HelpText = "Password for the Windows Insider account to use to generate authorization tokens.\n  (If 2FA, must be generated app password) (Optional)", Required = false, Default = "")]
        public string Password
        {
            get; set;
        }
    }

    [Verb("replay-download", isDefault: false, HelpText = "Replay a download from zero using a *.uupmcreplay file.")]
    internal class DownloadReplayOptions
    {
        [Option('r', "replay-metadata", HelpText = "The path to a *.uupmcreplay file to replay an older update and resume the download process.\n  Example: D:\\20236.1005.uupmcreplay", Required = true)]
        public string ReplayMetadata
        {
            get; set;
        }

        [Option('t', "machine-type", HelpText = "The architecture to report to the Windows Update servers.\n  Example: amd64, arm64, unknown, x86, wcemipsv2, axp, sh3, sh3dsp, sh4, sh5, arm, thumb,\n woa, am33, powerpc, powerpcfp, ia64, mips16, mipsfpu, mipsfpu16, ebc, m32r", Required = true)]
        public MachineType MachineType
        {
            get; set;
        }

        [Option('o', "output-folder", HelpText = "The folder to use for downloading the update files.", Required = false, Default = ".")]
        public string OutputFolder
        {
            get; set;
        }

        [Option('e', "edition", HelpText = "The edition to get. Must be used with the language parameter. Omit either of these to download everything.\n  Example: Professional, EnterpriseG. Enter 'type editions.txt' to see a full list of possibilities.' ", Required = false, Default = "")]
        public string Edition
        {
            get; set;
        }

        [Option('l', "language", HelpText = "The language to get. Must be used with the edition parameter. Omit either of these to download everything.\n  Example: en-US, de-DE, zh-CN", Required = false, Default = "")]
        public string Language
        {
            get; set;
        }

        [Option('f', "fixup", HelpText = "Applies a fixup to files in output folder.\n  Example: Appx", Required = false)]
        public Fixup? Fixup
        {
            get; set;
        }

        [Option('g', "appxroot", HelpText = "The folder containing the appx files for use with the Appx fixup", Required = false)]
        public string AppxRoot
        {
            get; set;
        }

        [Option('h', "cabsroot", HelpText = "The folder containing the cab files for use with the Appx fixup", Required = false)]
        public string CabsRoot
        {
            get; set;
        }
    }

    [Verb("get-builds", isDefault: false, HelpText = "Get builds in all rings matching the request type")]
    internal class GetBuildsOptions
    {
        [Option('s', "reporting-sku", HelpText = "The sku to report to the Windows Update servers.\n  Example: Professional, EnterpriseG. Enter 'type editions.txt' to see a full list of possibilities.' ", Required = true, Default = "")]
        public OSSkuId ReportingSku
        {
            get; set;
        }

        [Option('t', "machine-type", HelpText = "The architecture to report to the Windows Update servers." +
            
  Example: amd64, arm64, unknown, x86, wcemipsv2, axp, sh3, sh3dsp, sh4, sh5, arm, thumb,\n woa, am33, powerpc, powerpcfp, ia64, mips16, mipsfpu, mipsfpu16, ebc, m32r", Required = true)]
        public MachineType MachineType
        {
            get; set;
        }

        [Option('m', "mail", HelpText = "Email for the Windows Insider account to use to generate authorization tokens. (Optional)", Required = false, Default = "")]
        public string Mail
        {
            get; set;
        }

        [Option('p', "password", HelpText = "Password for the Windows Insider account to use to generate authorization tokens.\n (If 2FA, must be generated app password) (Optional)", Required = false, Default = "")]
        public string Password
        {
            get; set;
        }

        [Option('q', "preview-targeting-attribute", HelpText = "The name of the set of targeting attributes to use.", Required = false, Default = "")]
        public string TargetingAttribute
        {
            get; set;
        }
    }
}
