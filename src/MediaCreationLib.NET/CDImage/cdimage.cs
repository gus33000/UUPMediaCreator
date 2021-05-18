using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MediaCreationLib.CDImage
{
    public class CDImage
    {
        public delegate void ProgressCallback(string Operation, int ProgressPercentage, bool IsIndeterminate);

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

        public static bool GenerateISOImage(string isopath, string cdroot, string volumelabel, ProgressCallback progressCallback)
        {
            var setupexe = Path.Combine(cdroot, "setup.exe");
            var creationtime = File.GetCreationTimeUtc(setupexe);

            if (GetOperatingSystem() == OSPlatform.Windows)
            {
                var runningDirectory = Process.GetCurrentProcess().MainModule.FileName.Contains(Path.DirectorySeparatorChar) ? string.Join(Path.DirectorySeparatorChar, Process.GetCurrentProcess().MainModule.FileName.Split(Path.DirectorySeparatorChar).Reverse().Skip(1).Reverse()) : "";

                string cdimagepath = Path.Combine(runningDirectory, "CDImage", "cdimage.exe");

                var timestamp = creationtime.ToString("MM/dd/yyyy,hh:mm:ss");

                ProcessStartInfo processStartInfo = new ProcessStartInfo(cdimagepath,
                    $"\"-bootdata:2#p0,e,b{cdroot}\\boot\\etfsboot.com#pEF,e,b{cdroot}\\efi\\Microsoft\\boot\\efisys.bin\" -o -h -m -u2 -udfver102 -t{timestamp} -l{volumelabel}  \"{cdroot}\" \"{isopath}\"");

                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.CreateNoWindow = true;

                Process process = new Process();
                process.StartInfo = processStartInfo;

                try
                {
                    process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data != null && e.Data.Contains("%"))
                        {
                            var percent = int.Parse(e.Data.Split(' ').First(x => x.Contains("%")).Replace("%", ""));
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
                    foreach (var entry in Directory.EnumerateFileSystemEntries(cdroot, "*", SearchOption.AllDirectories))
                    {
                        if (Directory.Exists(entry))
                        {
                            try
                            {
                                Directory.SetCreationTimeUtc(entry, creationtime);
                            }
                            catch { }
                            try
                            {
                                Directory.SetLastAccessTimeUtc(entry, creationtime);
                            }
                            catch { }
                            try
                            {
                                Directory.SetLastWriteTimeUtc(entry, creationtime);
                            }
                            catch { }
                        }
                        else
                        {
                            try
                            {
                                File.SetCreationTimeUtc(entry, creationtime);
                            }
                            catch { }
                            try
                            {
                                File.SetLastAccessTimeUtc(entry, creationtime);
                            }
                            catch { }
                            try
                            {
                                File.SetLastWriteTimeUtc(entry, creationtime);
                            }
                            catch { }
                        }
                    }

                    var cmdline = $"-b \"boot/etfsboot.com\" --no-emul-boot --eltorito-alt-boot -b \"efi/microsoft/boot/efisys.bin\" --no-emul-boot --udf --hide \"*\" -V \"{volumelabel}\" -o \"{isopath}\" {cdroot}";

                    ProcessStartInfo processStartInfo = new ProcessStartInfo("mkisofs",
                        cmdline);

                    processStartInfo.UseShellExecute = false;
                    processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processStartInfo.RedirectStandardError = true;
                    processStartInfo.CreateNoWindow = true;

                    Process process = new Process();
                    process.StartInfo = processStartInfo;

                    try
                    {
                        process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                        {
                            if (e.Data != null && e.Data.Contains("%"))
                            {
                                var percent = (int)Math.Round(double.Parse(e.Data.Split(' ').First(x => x.Contains("%")).Replace("%", "")));
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