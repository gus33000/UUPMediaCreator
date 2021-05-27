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
using System.Text.Json;
using System.Threading;
using UUPMediaCreator.InterCommunication;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using System.Security.Principal;

namespace UUPMediaCreator.Broker
{
    internal class UUPMediaCreatorApplicationContext : IDisposable
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
                if (!await Windows.System.Launcher.LaunchUriAsync(new Uri("uupmediacreator:")))
                {
                    Environment.Exit(0);
                    return;
                }

                connection = new AppServiceConnection
                {
                    PackageFamilyName = Package.Current.Id.FamilyName,
                    AppServiceName = "UUPMediaCreatorService"
                };
                connection.ServiceClosed += Connection_ServiceClosed;
                AppServiceConnectionStatus connectionStatus = await connection.OpenAsync();
                if (connectionStatus != AppServiceConnectionStatus.Success)
                {
                    return;
                }

                connection.RequestReceived += Connection_RequestReceived;
            }
        }

        private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            ValueSet message = args.Request.Message;
            if (message.ContainsKey("InterCommunication"))
            {
                Common.InterCommunication interCommunication = JsonSerializer.Deserialize<Common.InterCommunication>(message["InterCommunication"] as string);

                switch (interCommunication.InterCommunicationType)
                {
                    case Common.InterCommunicationType.Exit:
                        {
                            Thread thread = new(async () =>
                            {
                                ValueSet val = new()
                                {
                                    { "InterCommunication", "" }
                                };
                                await args.Request.SendResponseAsync(val);
                            });

                            thread.Start();
                            thread.Join();

                            Environment.Exit(0);
                            break;
                        }
                    case Common.InterCommunicationType.StartISOConversionProcess:
                        {
                            async void callback(Common.ProcessPhase phase, bool IsIndeterminate, int ProgressInPercentage, string SubOperation)
                            {
                                Common.ISOConversionProgress prog = new()
                                {
                                    Phase = phase,
                                    IsIndeterminate = IsIndeterminate,
                                    ProgressInPercentage = ProgressInPercentage,
                                    SubOperation = SubOperation
                                };

                                Common.InterCommunication comm = new() { InterCommunicationType = Common.InterCommunicationType.ReportISOConversionProgress, ISOConversionProgress = prog };

                                ValueSet val = new()
                                {
                                    { "InterCommunication", JsonSerializer.Serialize(comm) }
                                };

                                await connection.SendMessageAsync(val);
                            }

                            Thread thread = new(async () =>
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
                                    Common.ISOConversionProgress prog = new()
                                    {
                                        Phase = Common.ProcessPhase.Error,
                                        IsIndeterminate = true,
                                        ProgressInPercentage = 0,
                                        SubOperation = ex.ToString()
                                    };

                                    Common.InterCommunication comm = new() { InterCommunicationType = Common.InterCommunicationType.ReportISOConversionProgress, ISOConversionProgress = prog };

                                    ValueSet val = new()
                                    {
                                        { "InterCommunication", JsonSerializer.Serialize(comm) }
                                    };

                                    await connection.SendMessageAsync(val);
                                }
                            });

                            thread.Start();
                            break;
                        }
                    case Common.InterCommunicationType.ReportPrivilege:
                        ValueSet val = new()
                        {
                            { "Privileged", new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) }
                        };
                        await args.Request.SendResponseAsync(val);
                        break;
                }
            }
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            connection.ServiceClosed -= Connection_ServiceClosed;
            connection = null;
        }

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}
