using System;
using System.Text.Json;
using System.Threading.Tasks;
using UUPMediaCreator.InterCommunication;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static UUPMediaCreator.InterCommunication.Common;

namespace UUPMediaCreator.UWP.Pages
{
    public sealed partial class BuildingISOPage : Page
    {
        private CoreWindow coreWindow;

        public BuildingISOPage()
        {
            this.InitializeComponent();
            coreWindow = CoreWindow.GetForCurrentThread();

            App.Connection.RequestReceived += Connection_RequestReceived;

            Loaded += BuildingVHDPage_Loaded;
        }

        private async Task<string> InputTextDialogAsync(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return "";
        }

        private async void BuildingVHDPage_Loaded(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null)
            {
                Application.Current.Exit();
            }

            var sku = await InputTextDialogAsync("Please enter the Sku you would like");
            var lang = await InputTextDialogAsync("Please enter the Language you would like");

            ISOConversion job = new ISOConversion()
            {
                UUPPath = folder.Path,
                ISOPath = $@"{folder.Path}\uup.iso",
                Edition = sku,
                LanguageCode = lang,
                CompressionType = CompressionType.LZX,
                IntegrateUpdates = false
            };

            var comm = new Common.InterCommunication() { InterCommunicationType = Common.InterCommunicationType.StartISOConversionProcess, ISOConversion = job };

            var val = new ValueSet();
            val.Add("InterCommunication", JsonSerializer.Serialize(comm));

            await App.Connection.SendMessageAsync(val);
        }

        private ProcessPhase lastPhase;

        private async void Connection_RequestReceived(Windows.ApplicationModel.AppService.AppServiceConnection sender, Windows.ApplicationModel.AppService.AppServiceRequestReceivedEventArgs args)
        {
            ValueSet message = args.Request.Message;
            if (message.ContainsKey("InterCommunication"))
            {
                Common.InterCommunication interCommunication = JsonSerializer.Deserialize<Common.InterCommunication>(message["InterCommunication"] as string);

                switch (interCommunication.InterCommunicationType)
                {
                    case Common.InterCommunicationType.ReportISOConversionProgress:
                        {
                            await coreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                switch (interCommunication.ISOConversionProgress.Phase)
                                {
                                    case ProcessPhase.ReadingMetadata:
                                        {
                                            ReadingMetadataGlyph.Glyph = "\uF167";
                                            break;
                                        }
                                    case ProcessPhase.PreparingFiles:
                                        {
                                            ReadingMetadataGlyph.Glyph = "\uEC61";
                                            PreparingFilesGlyph.Glyph = "\uF167";
                                            break;
                                        }
                                    case ProcessPhase.CreatingWindowsInstaller:
                                        {
                                            ReadingMetadataGlyph.Glyph = "\uEC61";
                                            PreparingFilesGlyph.Glyph = "\uEC61";
                                            CreatingWindowsInstallerGlyph.Glyph = "\uF167";
                                            break;
                                        }
                                    case ProcessPhase.ApplyingImage:
                                        {
                                            ReadingMetadataGlyph.Glyph = "\uEC61";
                                            PreparingFilesGlyph.Glyph = "\uEC61";
                                            CreatingWindowsInstallerGlyph.Glyph = "\uEC61";
                                            ApplyingImageGlyph.Glyph = "\uF167";
                                            break;
                                        }
                                    case ProcessPhase.IntegratingWinRE:
                                        {
                                            ReadingMetadataGlyph.Glyph = "\uEC61";
                                            PreparingFilesGlyph.Glyph = "\uEC61";
                                            CreatingWindowsInstallerGlyph.Glyph = "\uEC61";
                                            ApplyingImageGlyph.Glyph = "\uEC61";
                                            IntegratingWinREGlyph.Glyph = "\uF167";
                                            break;
                                        }
                                    case ProcessPhase.CapturingImage:
                                        {
                                            ReadingMetadataGlyph.Glyph = "\uEC61";
                                            PreparingFilesGlyph.Glyph = "\uEC61";
                                            CreatingWindowsInstallerGlyph.Glyph = "\uEC61";
                                            ApplyingImageGlyph.Glyph = "\uEC61";
                                            IntegratingWinREGlyph.Glyph = "\uEC61";
                                            CapturingImageGlyph.Glyph = "\uF167";
                                            break;
                                        }
                                    case ProcessPhase.CreatingISO:
                                        {
                                            ReadingMetadataGlyph.Glyph = "\uEC61";
                                            PreparingFilesGlyph.Glyph = "\uEC61";
                                            CreatingWindowsInstallerGlyph.Glyph = "\uEC61";
                                            ApplyingImageGlyph.Glyph = "\uEC61";
                                            IntegratingWinREGlyph.Glyph = "\uEC61";
                                            CapturingImageGlyph.Glyph = "\uEC61";
                                            CreatingISOGlyph.Glyph = "\uF167";
                                            break;
                                        }
                                    case ProcessPhase.Error:
                                        {
                                            switch (lastPhase)
                                            {
                                                case ProcessPhase.ReadingMetadata:
                                                    {
                                                        ReadingMetadataGlyph.Glyph = "\uEB90";
                                                        break;
                                                    }
                                                case ProcessPhase.PreparingFiles:
                                                    {
                                                        PreparingFilesGlyph.Glyph = "\uEB90";
                                                        break;
                                                    }
                                                case ProcessPhase.CreatingWindowsInstaller:
                                                    {
                                                        CreatingWindowsInstallerGlyph.Glyph = "\uEB90";
                                                        break;
                                                    }
                                                case ProcessPhase.ApplyingImage:
                                                    {
                                                        ApplyingImageGlyph.Glyph = "\uEB90";
                                                        break;
                                                    }
                                                case ProcessPhase.IntegratingWinRE:
                                                    {
                                                        IntegratingWinREGlyph.Glyph = "\uEB90";
                                                        break;
                                                    }
                                                case ProcessPhase.CapturingImage:
                                                    {
                                                        CapturingImageGlyph.Glyph = "\uEB90";
                                                        break;
                                                    }
                                                case ProcessPhase.CreatingISO:
                                                    {
                                                        CreatingISOGlyph.Glyph = "\uEB90";
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                }

                                if (interCommunication.ISOConversionProgress.Phase == ProcessPhase.Done)
                                {
                                    // Move to finish page when done, for now, welcome page
                                    Frame.Navigate(typeof(WelcomePage));
                                    return;
                                }

                                lastPhase = interCommunication.ISOConversionProgress.Phase;
                                ProgressBar.Value = interCommunication.ISOConversionProgress.ProgressInPercentage;
                                ProgressBar.IsIndeterminate = interCommunication.ISOConversionProgress.IsIndeterminate;
                                StatusText.Text = interCommunication.ISOConversionProgress.SubOperation;
                            });
                            break;
                        }
                }
            }
        }
    }
}
