using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using UUPMediaCreator.InterCommunication;
using System.Threading;

namespace UUPMediaCreator.Broker
{
    class UUPMediaCreatorApplicationContext : ApplicationContext
    {
        private AppServiceConnection connection = null;

        public UUPMediaCreatorApplicationContext()
        {
            OpenConnection();
        }

        private async void OpenConnection()
        {
            if (connection == null)
            {
                Process.Start("uupmediacreator:");

                connection = new AppServiceConnection();
                connection.PackageFamilyName = Package.Current.Id.FamilyName;
                connection.AppServiceName = "UUPMediaCreatorService";
                connection.ServiceClosed += Connection_ServiceClosed;
                AppServiceConnectionStatus connectionStatus = await connection.OpenAsync();
                if (connectionStatus != AppServiceConnectionStatus.Success)
                {
                    return;
                }

                connection.RequestReceived += Connection_RequestReceived;
            }
        }

        private void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            ValueSet message = args.Request.Message;
            if (message.ContainsKey("InterCommunication"))
            {
                Common.InterCommunication interCommunication = JsonSerializer.Deserialize<Common.InterCommunication>(message["InterCommunication"] as string);

                switch (interCommunication.InterCommunicationType)
                {
                    case Common.InterCommunicationType.Exit:
                        {
                            connection.Dispose();
                            Application.Exit();
                            break;
                        }
                    case Common.InterCommunicationType.StartISOConversionProcess:
                        {
                            async void callback(Common.ProcessPhase phase, bool IsIndeterminate, int ProgressInPercentage, string SubOperation)
                            {
                                var prog = new Common.ISOConversionProgress()
                                {
                                    Phase = phase,
                                    IsIndeterminate = IsIndeterminate,
                                    ProgressInPercentage = ProgressInPercentage,
                                    SubOperation = SubOperation
                                };

                                var comm = new Common.InterCommunication() { InterCommunicationType = Common.InterCommunicationType.ReportISOConversionProgress, ISOConversionProgress = prog };

                                var val = new ValueSet();
                                val.Add("InterCommunication", JsonSerializer.Serialize(comm));

                                await SendToUWP(val);
                            }

                            Thread thread = new Thread(async () =>
                            {
                                try
                                {
                                    MediaCreationLib.MediaCreator.CreateISOMedia(
                                            interCommunication.ISOConversion.ISOPath,
                                            interCommunication.ISOConversion.UUPPath,
                                            interCommunication.ISOConversion.Edition,
                                            interCommunication.ISOConversion.LanguageCode,
                                            interCommunication.ISOConversion.IntegrateUpdates,
                                            interCommunication.ISOConversion.CompressionType,
                                            callback);
                                }
                                catch (Exception ex)
                                {
                                    var prog = new Common.ISOConversionProgress()
                                    {
                                        Phase = Common.ProcessPhase.Error,
                                        IsIndeterminate = true,
                                        ProgressInPercentage = 0,
                                        SubOperation = ex.ToString()
                                    };

                                    var comm = new Common.InterCommunication() { InterCommunicationType = Common.InterCommunicationType.ReportISOConversionProgress, ISOConversionProgress = prog };

                                    var val = new ValueSet();
                                    val.Add("InterCommunication", JsonSerializer.Serialize(comm));

                                    await SendToUWP(val);
                                }
                            });

                            thread.Start();
                            break;
                        }
                }
            }
        }

        private async Task SendToUWP(ValueSet message)
        {
            await connection.SendMessageAsync(message);
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            connection.ServiceClosed -= Connection_ServiceClosed;
            connection = null;
        }
    }
}
