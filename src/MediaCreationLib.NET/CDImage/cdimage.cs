using DiscUtils.Iso9660;
using DiscUtils.Streams;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MediaCreationLib.CDImage
{
    public static class ArrayExtensions
    {
        public static T[] Concat<T>(this T[] x, T[] y)
        {
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");
            int oldLen = x.Length;
            Array.Resize<T>(ref x, x.Length + y.Length);
            Array.Copy(y, 0, x, oldLen, y.Length);
            return x;
        }
    }

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
                    CDBuilder builder = new CDBuilder();
                    builder.UseJoliet = true;
                    builder.VolumeIdentifier = volumelabel;

                    foreach (var directory in Directory.EnumerateDirectories(cdroot, "*", SearchOption.AllDirectories))
                    {
                        var strippedDirectory = directory.Replace(cdroot + Path.DirectorySeparatorChar, "").Replace(cdroot, "").Replace(Path.DirectorySeparatorChar, '\\');
                        var addedDirectory = builder.AddDirectory(strippedDirectory);
                        addedDirectory.CreationTime = creationtime;
                    }

                    foreach (var file in Directory.EnumerateFiles(cdroot, "*", SearchOption.AllDirectories))
                    {
                        var strippedFile = file.Replace(cdroot + Path.DirectorySeparatorChar, "").Replace(cdroot, "").Replace(Path.DirectorySeparatorChar, '\\');
                        var addedFile = builder.AddFile(strippedFile, file);
                        addedFile.CreationTime = creationtime;
                    }

                    using (var bootstrm = new MemoryStream(File.ReadAllBytes(Path.Combine(cdroot, "boot", "etfsboot.com")).Concat(File.ReadAllBytes(Path.Combine(cdroot, "efi", "microsoft", "boot", "efisys.bin")))))
                    {
                        builder.SetBootImage(bootstrm, BootDeviceEmulation.NoEmulation, 0);

                        using (var inDisc = builder.Build())
                        using (var outDisc = File.Open(isopath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            var pump = new StreamPump
                            {
                                InputStream = inDisc,
                                OutputStream = outDisc,
                                SparseCopy = true,
                                SparseChunkSize = 0x200,
                                BufferSize = 0x200 * 1024
                            };

                            var totalBytes = inDisc.Length;

                            pump.ProgressEvent += (o, e) =>
                            {
                                var percent = (int)Math.Round((double)e.BytesRead * 100d / (double)totalBytes);
                                progressCallback?.Invoke($"Building {isopath}", percent, false);
                            };

                            pump.Run();
                            return true;
                        }
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