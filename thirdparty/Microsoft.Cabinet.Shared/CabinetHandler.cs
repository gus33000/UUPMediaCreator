using Microsoft.PackageManagement.Archivers.Internal.Compression;
using Microsoft.PackageManagement.Archivers.Internal.Compression.Cab;
using System;
using System.IO;
using System.Linq;

namespace Microsoft.Cabinet
{
    public class CabinetHandler : IDisposable
    {
        private Stream CabinetStream;
        private CabEngine engine = new CabEngine();
        private bool DisposeStream;

        public delegate void ProgressCallback(int ProgressPercent, string CurrentFile);

        public CabinetHandler(Stream CabinetStream, bool DisposeStream = true)
        {
            this.CabinetStream = CabinetStream;
            this.Files = engine.GetFiles(this.CabinetStream).ToArray();
            this.DisposeStream = DisposeStream;
        }

        public string[] Files { get; private set; }

        public Stream OpenFile(string File) => engine.Unpack(CabinetStream, File);

        /*public void ExpandFiles(string OutputPath, ProgressCallback progressCallback)
        {
            int buffersize = 0x20000;

            int fileCount = Files.Count();

            int fileCounter = 0;
            foreach (var file in Files)
            {
                int progressOffset = (int)Math.Round((double)fileCounter / fileCount * 100);
                int progressScale = (int)Math.Round((double)1 / fileCount * 100);

                var filedirectoryonly = file.Contains("\\") ? string.Join("\\", file.Split('\\').Reverse().Skip(1).Reverse()) : "";
                var targetDirectory = Path.Combine(OutputPath, filedirectoryonly);
                var targetFile = Path.Combine(OutputPath, file);
                if (!Directory.Exists(targetDirectory))
                    Directory.CreateDirectory(targetDirectory);

                using (var inStream = OpenFile(file))
                using (var outStream = File.Create(targetFile))
                {
                    byte[] buffer = new byte[buffersize];
                    for (int i = 0; i < inStream.Length; i += buffersize)
                    {
                        int read = inStream.Read(buffer, 0, buffersize);
                        outStream.Write(buffer, 0, read);
                        progressCallback.Invoke(progressOffset + (int)Math.Round((double)i / inStream.Length * progressScale), file);
                    }
                }
                fileCounter++;
            }
        }*/

        public static void ExpandFiles(string CabinetPath, string OutputPath, ProgressCallback progressCallback)
        {
            CabInfo info = new CabInfo(CabinetPath);

            info.Unpack(OutputPath, (a, b) =>
            {
                progressCallback.Invoke((int)Math.Round((double)b.CurrentArchiveBytesProcessed / b.CurrentArchiveTotalBytes * 100), b.CurrentFileName);
            });
        }

        public void Dispose()
        {
            engine.Dispose();
            if (DisposeStream)
                CabinetStream.Dispose();
        }
    }
}
