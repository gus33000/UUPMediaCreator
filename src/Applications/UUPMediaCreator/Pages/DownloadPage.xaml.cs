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
using DownloadLib;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UUPMediaCreator.UWP.Pages
{
    public sealed partial class DownloadPage : Page, IProgress<GeneralDownloadProgress>
    {
        public DownloadPage()
        {
            this.InitializeComponent();
        }

        private void DownloadPage_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressBar.IsIndeterminate = true;

            _ = Windows.System.Threading.ThreadPool.RunAsync(async (o) =>
            {
                StorageFolder tmp = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync($"{DateTime.Now.Ticks}");

                App.ConversionPlan.TmpOutputFolder = await UpdateUtils.ProcessUpdateAsync(App.ConversionPlan.UpdateData, tmp.Path, App.ConversionPlan.MachineType, this, App.ConversionPlan.Language, App.ConversionPlan.Edition, UseAutomaticDownloadFolder: false).ConfigureAwait(false);

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => Frame.Navigate(typeof(BuildingISOPage)));
            });
        }

        private readonly Dictionary<string, FileStatus> files = new();

        private readonly Mutex mutex = new();

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

        public async void Report(GeneralDownloadProgress e)
        {
            foreach (FileDownloadStatus status in e.DownloadedStatus)
            {
                if (status == null)
                {
                    continue;
                }

                /*const bool shouldReport = true;//!files.ContainsKey(status.File.FileName) || files[status.File.FileName] != status.FileStatus;

                if (!shouldReport)
                {
                    continue;
                }*/

                mutex.WaitOne();

                files[status.File.FileName] = status.FileStatus;

                mutex.ReleaseMutex();

                string msg = "Unknown: ";

                switch (status.FileStatus)
                {
                    case FileStatus.Completed:
                        msg = "Completed: ";
                        break;
                    case FileStatus.Downloading:
                        msg = "Downloading ";
                        break;
                    case FileStatus.Expired:
                        msg = "Expired: ";
                        break;
                    case FileStatus.Failed:
                        msg = "Failed: ";
                        break;
                    case FileStatus.Verifying:
                        msg = "Verifying: ";
                        break;
                }

                uint progress = (uint)Math.Round((double)status.DownloadedBytes / status.File.FileSize * 100);

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ProgressBar.IsIndeterminate = false;
                    StatusText.Text = $"{msg} {status.File.FileName} ({FormatBytes(status.File.FileSize)}) ({progress}%)";
                    ProgressBar.Maximum = e.NumFiles;
                    ProgressBar.Value = e.NumFilesDownloadedSuccessfully;
                });
            }
        }
    }
}
