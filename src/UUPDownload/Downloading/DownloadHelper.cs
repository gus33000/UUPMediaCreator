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
        private static readonly DownloadConfiguration downloadOpt = new DownloadConfiguration()
        {
            AllowedHeadRequest = false,
            MaxTryAgainOnFailover = 10,
            ParallelDownload = true,
            ChunkCount = 8,
            Timeout = 5000,
            OnTheFlyDownload = false,
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
                }
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

        private static string GetDismLikeProgBar(int perc)
        {
            int eqsLength = (int)((double)perc / 100 * 55);
            string bases = new string('=', eqsLength) + new string(' ', 55 - eqsLength);
            bases = bases.Insert(28, perc + "%");
            if (perc == 100)
                bases = bases.Substring(1);
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

            DownloadService downloader = new DownloadService(downloadOpt);

            Logging.Log("Downloading " + Path.Combine(outputPath, filenameonly) + "...");

            int maxlength = 0;

            downloader.DownloadProgressChanged += (object sender, Downloader.DownloadProgressChangedEventArgs e) =>
            {
                TimeSpan timeellapsed = DateTime.Now - startTime;
                double BytesPerSecondSpeed = (timeellapsed.Milliseconds > 0 ? e.BytesReceived / timeellapsed.Milliseconds : 0) * 60;
                long remainingBytes = e.TotalBytesToReceive - e.BytesReceived;
                double remainingTime = BytesPerSecondSpeed > 0 ? remainingBytes / BytesPerSecondSpeed : 0;
                TimeSpan timeRemaining = TimeSpan.FromSeconds(remainingTime);

                string speed = FormatBytes(BytesPerSecondSpeed) + "/s";
                if (speed.Length > maxlength)
                    maxlength = speed.Length;
                else if (speed.Length < maxlength)
                    speed = speed + new string(' ', maxlength - speed.Length);

                Logging.Log($"{GetDismLikeProgBar((int)e.ProgressPercentage)} {timeRemaining:hh\\:mm\\:ss\\.f} {speed}", Logging.LoggingLevel.Information, false);
            };

            try
            {
                await downloader.DownloadFileAsync(fileDownloadInfo.DownloadUrl, Path.Combine(OutputFolder, outputPath, filenameonly));
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
                fileDownloadInfo.Decrypt(Path.Combine(OutputFolder, outputPath, filenameonly), Path.Combine(OutputFolder, outputPath, filenameonly) + ".decrypted");
            }

            goto OnExit;

        OnError:
            returnCode = -1;

        OnExit:
            return returnCode;
        }

        /*public static async Task<int> GetDownloadFileTask(string OutputFolder, string filename, FileExchangeV3FileDownloadInformation fileDownloadInfo, SemaphoreSlim concurrencySemaphore)
        {
            int returnCode = 0;

            if (!fileDownloadInfo.IsDownloadable)
            {
                Logging.Log($"Skipping {filename} as the download link expired. This file will get downloaded shortly once the tool loops back again.", Logging.LoggingLevel.Warning);
                goto OnError;
            }

            try
            {
                string filenameonly = Path.GetFileName(filename);
                string filenameonlywithoutextension = Path.GetFileNameWithoutExtension(filename);
                string extension = filenameonly.Replace(filenameonlywithoutextension, "");
                string outputPath = filename.Replace(filenameonly, "");
                int countSame = 0;
                DownloaderClient dlclient = new DownloaderClient(1000000000 * 2); // 2GB max download
                DateTime startTime = DateTime.Now;
                bool end = false;
                long dled = 0;

                Logging.Log("Downloading " + Path.Combine(outputPath, filenameonly) + "...");

                Thread thread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    dlclient.OnDownloading += (DownloadMetric metric) =>
                    {
                        dled = metric.DownloadedBytes;
                        Logging.Log($"{GetDismLikeProgBar((int)metric.Progress)} {metric.TimeRemaining:hh\\:mm\\:ss\\.f} {FormatBytes(metric.Speed)}/s", Logging.LoggingLevel.Information, false);
                        if (metric.IsComplete)
                        {
                            Logging.Log("");
                            end = true;
                        }
                    };
                    dlclient.OnError += (Exception ex) =>
                    {
                        if (ex.InnerException != null && ex.InnerException.GetType() == typeof(NullReferenceException))
                        {
                            // ignore
                            return;
                        }

                        Logging.Log("");
                        Logging.Log(ex.ToString(), Logging.LoggingLevel.Error);
                        if (ex.InnerException != null)
                            Logging.Log(ex.InnerException.ToString(), Logging.LoggingLevel.Error);
                        returnCode = -1;
                        Logging.Log("");
                        end = true;
                    };
                });
                thread.Start();

                dlclient.DownloadToFile(new Uri(fileDownloadInfo.DownloadUrl), filenameonly, Path.Combine(OutputFolder, outputPath));

                long prev = dled;
                while (!end)
                {
                    if (prev == dled)
                    {
                        countSame++;
                    }
                    else
                    {
                        countSame = 0;
                        prev = dled;
                    }

                    if (countSame == 60 * 5) // One minute of hang
                    {
                        Logging.Log("");
                        Logging.Log("Download hung", Logging.LoggingLevel.Error);
                        goto OnError;
                    }
                    Thread.Sleep(200);
                }

                if (returnCode == 0 && fileDownloadInfo.IsEncrypted)
                {
                    Logging.Log("Decrypting file...");
                    fileDownloadInfo.Decrypt(Path.Combine(OutputFolder, outputPath, filenameonly), Path.Combine(OutputFolder, outputPath, filenameonly) + ".decrypted");
                }

                goto OnExit;
            }
            catch
            {
                Logging.Log("");
                Logging.Log("Unknown error occured while downloading", Logging.LoggingLevel.Error);
                Logging.Log("");
                goto OnError;
            }

            OnError:
            returnCode = -1;

            OnExit:
            concurrencySemaphore.Release();
            return returnCode;
        }*/
    }
}