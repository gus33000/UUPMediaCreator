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

        int previous = -1;

        public async void Report(GeneralDownloadProgress e)
        {
            foreach (FileDownloadStatus status in e.DownloadedStatus)
            {
                if (status == null)
                {
                    continue;
                }

                mutex.WaitOne();

                files[status.File.FileName] = status.FileStatus;

                if (e.NumFilesDownloadedSuccessfully == previous)
                {
                    mutex.ReleaseMutex();
                    return;
                }

                previous = e.NumFilesDownloadedSuccessfully;

                mutex.ReleaseMutex();

                uint progress = (uint)Math.Round((double)status.DownloadedBytes / status.File.FileSize * 100);

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    if (ProgressBar.Value != e.NumFilesDownloadedSuccessfully || ProgressBar.IsIndeterminate == true)
                    {
                        ProgressBar.IsIndeterminate = false;
                        ProgressBar.Value = e.NumFilesDownloadedSuccessfully;
                        StatusText.Text = $"{progress}%";
                        ProgressBar.Maximum = e.NumFiles;
                    }
                });
            }
        }
    }
}
