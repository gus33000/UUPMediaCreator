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
using System;
using System.IO;
using System.Text.Json;
using UUPMediaCreator.InterCommunication;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static UUPMediaCreator.InterCommunication.Common;

namespace UUPMediaCreator.UWP.Pages
{
    public sealed partial class BuildingISOPage : Page
    {
        private BinaryWriter bw;
        private readonly CoreWindow coreWindow;

        public BuildingISOPage()
        {
            InitializeComponent();
            coreWindow = CoreWindow.GetForCurrentThread();

            App.Connection.RequestReceived += Connection_RequestReceived;

            Loaded += BuildingVHDPage_Loaded;
            Unloaded += BuildingISOPage_Unloaded;
        }

        private void BuildingISOPage_Unloaded(object sender, RoutedEventArgs e)
        {
            bw.Dispose();
        }

        private async void BuildingVHDPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationData data = ApplicationData.Current;
            StorageFile file = await data.LocalFolder.CreateFileAsync("log.txt", CreationCollisionOption.OpenIfExists);
            Windows.Storage.Streams.IRandomAccessStream strm = await file.OpenAsync(FileAccessMode.ReadWrite);
            bw = new(strm.AsStream());
            strm.Seek(strm.Size);

            ISOConversion job = new()
            {
                UUPPath = App.ConversionPlan.TmpOutputFolder,
                ISOPath = App.ConversionPlan.ISOPath,
                Edition = App.ConversionPlan.Edition,
                LanguageCode = App.ConversionPlan.Language,
                CompressionType = (CompressionType)App.ConversionPlan.InstallationWIMMediumType,
                IntegrateUpdates = false
            };

            Common.InterCommunication comm = new() { InterCommunicationType = Common.InterCommunicationType.StartISOConversionProcess, ISOConversion = job };

            ValueSet val = new()
            {
                { "InterCommunication", JsonSerializer.Serialize(comm) }
            };

            _ = await App.Connection.SendMessageAsync(val);
        }

        private ProcessPhase lastPhase;

        private int prevperc = -1;
        private Common.ProcessPhase prevphase = Common.ProcessPhase.ReadingMetadata;
        private string prevop = "";

        private void Log(string msg)
        {
            bw.Write(System.Text.Encoding.UTF8.GetBytes(msg + "\r\n"));
            bw.Flush();
        }

        private void LogInterComm(Common.InterCommunication interCommunication)
        {
            if (interCommunication.ISOConversionProgress.Phase == prevphase && prevperc == interCommunication.ISOConversionProgress.ProgressInPercentage && interCommunication.ISOConversionProgress.SubOperation == prevop)
            {
                return;
            }

            prevphase = interCommunication.ISOConversionProgress.Phase;
            prevop = interCommunication.ISOConversionProgress.SubOperation;
            prevperc = interCommunication.ISOConversionProgress.ProgressInPercentage;

            if (interCommunication.ISOConversionProgress.Phase == Common.ProcessPhase.Error)
            {
                Log("An error occured!");
                Log(interCommunication.ISOConversionProgress.SubOperation);
                return;
            }
            string progress = interCommunication.ISOConversionProgress.IsIndeterminate ? "" : $" [Progress: {interCommunication.ISOConversionProgress.ProgressInPercentage}%]";
            Log($"[{interCommunication.ISOConversionProgress.Phase}]{progress} {interCommunication.ISOConversionProgress.SubOperation}");
        }

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
                            LogInterComm(interCommunication);

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

                                            // cleanup
                                            if (App.ConversionPlan.TmpOutputFolder != null)
                                            {
                                                Directory.Delete(App.ConversionPlan.TmpOutputFolder, true);
                                            }

                                            break;
                                        }
                                }

                                if (interCommunication.ISOConversionProgress.Phase == ProcessPhase.Done)
                                {
                                    // cleanup
                                    //Directory.Delete(App.ConversionPlan.TmpOutputFolder, true);

                                    // Move to finish page when done, for now, welcome page
                                    _ = Frame.Navigate(typeof(EndPage));
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