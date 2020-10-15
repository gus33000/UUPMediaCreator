using Neon.Downloader;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UUPDownload.Downloading
{
    public static class DownloadHelper
    {
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

        public static Task<int> GetDownloadFileTask(string OutputFolder, string filename, string url, SemaphoreSlim concurrencySemaphore)
        {
            return new Task<int>(() =>
            {
                string filenameonly = Path.GetFileName(filename);
                string filenameonlywithoutextension = Path.GetFileNameWithoutExtension(filename);
                string extension = filenameonly.Replace(filenameonlywithoutextension, "");
                string outputPath = filename.Replace(filenameonly, "");
                int returnCode = 0, countSame = 0;
                DownloaderClient dlclient = new DownloaderClient();
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

                        Logging.Log(ex.ToString(), Logging.LoggingLevel.Error);
                        if (ex.InnerException != null)
                            Logging.Log(ex.InnerException.ToString(), Logging.LoggingLevel.Error);
                        returnCode = -1;
                        Logging.Log("");
                        end = true;
                    };
                });
                thread.Start();

                dlclient.DownloadToFile(new Uri(url), filenameonly, Path.Combine(OutputFolder, outputPath));

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
                        Logging.Log("Download hanged");
                        countSame = 0;
                        thread.Abort();
                        return 0;
                    }
                    Thread.Sleep(200);
                }

                thread.Abort();
                concurrencySemaphore.Release();
                return returnCode;
            });
        }
    }
}