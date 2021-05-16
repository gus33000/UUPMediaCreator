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
            Loaded += DownloadPage_Loaded;
        }

        private async void DownloadPage_Loaded(object sender, RoutedEventArgs e)
        {
            var tmp = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync($"tmpDl_{DateTime.Now.Ticks}");
            ProgressBar.IsIndeterminate = true;

            App.ConversionPlan.TmpOutputFolder = await UpdateUtils.ProcessUpdateAsync(App.ConversionPlan.UpdateData, tmp.Path, App.ConversionPlan.MachineType, this, App.ConversionPlan.Language, App.ConversionPlan.Edition);

            Frame.Navigate(typeof(BuildingISOPage));
        }

        private void WizardPage_NextClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BuildingISOPage));
        }

        private Dictionary<string, FileStatus> files = new Dictionary<string, FileStatus>();

        private Mutex mutex = new Mutex();

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

        public void Report(GeneralDownloadProgress e)
        {
            mutex.WaitOne();

            foreach (FileDownloadStatus status in e.DownloadedStatus)
            {
                if (status == null)
                    continue;

                bool shouldReport = true;//!files.ContainsKey(status.File.FileName) || files[status.File.FileName] != status.FileStatus;

                if (!shouldReport)
                    continue;

                files[status.File.FileName] = status.FileStatus;

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

                ProgressBar.IsIndeterminate = false;
                StatusText.Text = $"{msg} {status.File.FileName} ({FormatBytes(status.File.FileSize)}) ({progress}%)";
                ProgressBar.Maximum = e.NumFiles;
                ProgressBar.Value = e.NumFilesDownloadedSuccessfully;
            }

            mutex.ReleaseMutex();
        }
    }
}