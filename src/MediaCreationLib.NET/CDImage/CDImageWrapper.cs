﻿/*
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
using MediaCreationLib.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MediaCreationLib.CDImage
{
    internal static class CDImageWrapper
    {
        internal delegate void ProgressCallback(string Operation, int ProgressPercentage, bool IsIndeterminate);

        internal static bool GenerateISOImage(string isopath, string cdroot, string volumelabel, ProgressCallback progressCallback)
        {
            string setupexe = Path.Combine(cdroot, "setup.exe");
            DateTime creationtime = File.GetCreationTimeUtc(setupexe);

            if (PlatformUtilities.OperatingSystem == OSPlatform.Windows)
            {
                string cdimagepath = Path.Combine(PlatformUtilities.CurrentRunningDirectory, "CDImage", "cdimage.exe");

                string timestamp = creationtime.ToString("MM/dd/yyyy,hh:mm:ss");

                ProcessStartInfo processStartInfo = new(cdimagepath,
                    $"\"-bootdata:2#p0,e,b{cdroot}\\boot\\etfsboot.com#pEF,e,b{cdroot}\\efi\\Microsoft\\boot\\efisys.bin\" -o -h -m -u2 -udfver102 -t{timestamp} -l{volumelabel}  \"{cdroot}\" \"{isopath}\"");

                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.CreateNoWindow = true;

                Process process = new();
                process.StartInfo = processStartInfo;

                try
                {
                    process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data?.Contains("%") == true)
                        {
                            int percent = int.Parse(e.Data.Split(' ').First(x => x.Contains("%")).Replace("%", ""));
                            progressCallback?.Invoke($"Building {isopath}", percent, false);
                        }
                    };
                    process.Start();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    FolderUtilities.TrySetTimestampsRecursive(cdroot, creationtime);

                    string cmdline = $"-b \"boot/etfsboot.com\" --no-emul-boot --eltorito-alt-boot -b \"efi/microsoft/boot/efisys.bin\" --no-emul-boot --udf --hide \"*\" -V \"{volumelabel}\" -o \"{isopath}\" {cdroot}";

                    ProcessStartInfo processStartInfo = new("mkisofs",
                        cmdline);

                    processStartInfo.UseShellExecute = false;
                    processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processStartInfo.RedirectStandardError = true;
                    processStartInfo.CreateNoWindow = true;

                    Process process = new();
                    process.StartInfo = processStartInfo;

                    try
                    {
                        process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                        {
                            if (e.Data?.Contains("%") == true)
                            {
                                int percent = (int)Math.Round(double.Parse(e.Data.Split(' ').First(x => x.Contains("%")).Replace("%", "")));
                                progressCallback?.Invoke($"Building {isopath}", percent, false);
                            }
                        };
                        process.Start();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                        return process.ExitCode == 0;
                    }
                    catch
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}