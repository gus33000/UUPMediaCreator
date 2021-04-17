// Copyright (c) Gustave Monce and Contributors
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

using Downloader;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WindowsUpdateLib;

namespace UUPDownload.Downloading
{
    public static class DownloadHelper
    {
        private static readonly DownloadConfiguration downloadOpt = new()
        {
            MaxTryAgainOnFailover = 10,
            ParallelDownload = true,
            ChunkCount = 2,
            Timeout = 1000,
            OnTheFlyDownload = true,
            BufferBlockSize = 10240,
            MaximumBytesPerSecond = 0,
            TempDirectory = Path.GetTempPath(),
            RequestConfiguration =
                {
                    Accept = "*/*",
                    UserAgent = Constants.UserAgent,
                    ProtocolVersion = HttpVersion.Version11,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    KeepAlive = true,
                    UseDefaultCredentials = false
                },
             CheckDiskSizeBeforeDownload = true
        };

        private static string FormatBytes(double bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return $"{dblSByte:0.##}{suffix[i]}";
        }

        private static string GetProgressBarString(int perc)
        {
            int eqsLength = (int)((double)perc / 100 * 55);
            string bases = new string('=', eqsLength) + new string(' ', 55 - eqsLength);
            bases = bases.Insert(28, perc + "%");
            if (perc == 100)
                bases = bases[1..];
            else if (perc < 10)
                bases = bases.Insert(28, " ");
            return "[" + bases + "]";
        }

        public static async Task<int> GetDownloadFileTask(string OutputFolder, string filename, FileExchangeV3FileDownloadInformation fileDownloadInfo)
        {
            int returnCode = 0;

            if (!fileDownloadInfo.IsDownloadable)
            {
                Logging.Log($"Skipping {filename} as the download link expired. This file will get downloaded shortly once the tool loops back again.", Logging.LoggingLevel.Warning);
                goto OnError;
            }

            string filenameonly = Path.GetFileName(filename);
            string filenameonlywithoutextension = Path.GetFileNameWithoutExtension(filename);
            string extension = filenameonly.Replace(filenameonlywithoutextension, "");
            string outputPath = filename.Replace(filenameonly, "");

            // Download starts here
            
            DateTime startTime = DateTime.Now;

            DownloadService downloader = new(downloadOpt);

            Logging.Log("Downloading " + Path.Combine(outputPath, filenameonly) + "...");

            int maxlength = 0;

            downloader.DownloadProgressChanged += (object sender, Downloader.DownloadProgressChangedEventArgs e) =>
            {
                TimeSpan timeellapsed = DateTime.Now - startTime;
                double BytesPerSecondSpeed = (timeellapsed.Milliseconds > 0 ? e.ReceivedBytesSize / timeellapsed.Milliseconds : 0) * 60;
                long remainingBytes = e.TotalBytesToReceive - e.ReceivedBytesSize;
                double remainingTime = BytesPerSecondSpeed > 0 ? remainingBytes / BytesPerSecondSpeed : 0;
                TimeSpan timeRemaining = TimeSpan.FromSeconds(remainingTime);

                string speed = FormatBytes(BytesPerSecondSpeed) + "/s";
                if (speed.Length > maxlength)
                    maxlength = speed.Length;
                else if (speed.Length < maxlength)
                    speed += new string(' ', maxlength - speed.Length);

                Logging.Log($"{GetProgressBarString((int)e.ProgressPercentage)} {timeRemaining:hh\\:mm\\:ss\\.f}", Logging.LoggingLevel.Information, false);
            };

            try
            {
                await downloader.DownloadFileTaskAsync(fileDownloadInfo.DownloadUrl, Path.Combine(OutputFolder, outputPath, filenameonly));
                Logging.Log("");
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null || ex.InnerException.GetType() != typeof(NullReferenceException))
                {
                    Logging.Log("");
                    Logging.Log(ex.ToString(), Logging.LoggingLevel.Error);
                    if (ex.InnerException != null)
                        Logging.Log(ex.InnerException.ToString(), Logging.LoggingLevel.Error);
                    returnCode = -1;
                    Logging.Log("");
                }
            }

            // Download ends here

            if (returnCode == 0 && fileDownloadInfo.IsEncrypted)
            {
                Logging.Log("Decrypting file...");
                await fileDownloadInfo.DecryptAsync(Path.Combine(OutputFolder, outputPath, filenameonly), Path.Combine(OutputFolder, outputPath, filenameonly) + ".decrypted");
            }

            goto OnExit;

        OnError:
            returnCode = -1;

        OnExit:
            return returnCode;
        }
    }
}