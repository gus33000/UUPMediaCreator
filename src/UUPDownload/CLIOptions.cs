// Copyright (c) 2020, Gustave M. - gus33000.me - @gus33000
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using CommandLine;
using WindowsUpdateLib;

namespace UUPDownload
{
    internal class Options
    {
        [Option('s', "reporting-sku", HelpText = @"The sku to report to the Windows Update servers. Example: Professional", Required = true)]
        public OSSkuId ReportingSku { get; set; }

        [Option('v', "reporting-version", HelpText = @"The version to report to the Windows Update servers. Example: 10.0.20152.1000", Required = true)]
        public string ReportingVersion { get; set; }

        [Option('t', "machine-type", HelpText = @"The architecture to report to the Windows Update servers. Example: amd64", Required = true)]
        public MachineType MachineType { get; set; }

        [Option('r', "flight-ring", HelpText = @"The ring to report to the Windows Update servers. Example: External, other example: Retail or External", Required = true)]
        public string FlightRing { get; set; }

        [Option('b', "flighting-branch-name", HelpText = @"The flighting branch name to report to the Windows Update servers. Example: External, other example: Dev, Beta or ReleasePreview", Required = false, Default = "")]
        public string FlightingBranchName { get; set; }

        [Option('l', "branch-readiness-level", HelpText = @"The branch readiness level to report to the Windows Update servers. Example: CB", Required = false, Default = "CB")]
        public string BranchReadinessLevel { get; set; }

        [Option('c', "current-branch", HelpText = @"The branch to report to the Windows Update servers. Example: 19h1_release", Required = true)]
        public string CurrentBranch { get; set; }

        [Option('y', "sync-current-version-only", HelpText = @"Only get updates for the current version, enables getting cumulative updates.", Required = false, Default = false)]
        public bool SyncCurrentVersionOnly { get; set; }

        [Option('o', "output-folder", HelpText = @"The folder to use for downloading the update files.", Required = false, Default = ".")]
        public string OutputFolder { get; set; }
    }
}