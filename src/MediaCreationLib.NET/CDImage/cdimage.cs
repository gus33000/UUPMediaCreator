using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MediaCreationLib.CDImage
{
    public class CDImage
    {
        public delegate void ProgressCallback(string Operation, int ProgressPercentage, bool IsIndeterminate);

        public static bool GenerateISOImage(string isopath, string cdroot, string volumelabel, ProgressCallback progressCallback)
        {
            var runningDirectory = Process.GetCurrentProcess().MainModule.FileName.Contains("\\") ? string.Join("\\", Process.GetCurrentProcess().MainModule.FileName.Split('\\').Reverse().Skip(1).Reverse()) : "";

            string cdimagepath = Path.Combine(runningDirectory, "CDImage", "cdimage.exe");

            var setupexe = Path.Combine(cdroot, "setup.exe");
            var creationtime = File.GetCreationTimeUtc(setupexe);
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
    }
}
