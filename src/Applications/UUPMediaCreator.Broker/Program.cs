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
using MediaCreationLib.NET;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UUPMediaCreator.InterCommunication;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace UUPMediaCreator.Broker
{
    internal static class Program
    {
        private static AppServiceConnection connection;
        private static ManualResetEvent appServiceExit;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            if (!ElevateIfPossibleToAdministrator())
            {
                return;
            }

            try
            {
                appServiceExit = new(false);
                InitializeAppServiceConnection();

                _ = appServiceExit.WaitOne();
            }
            finally
            {
                connection?.Dispose();
                appServiceExit?.Dispose();
                appServiceExit = null;
            }
        }

        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static bool ElevateIfPossibleToAdministrator()
        {
            if (!IsAdministrator())
            {
                try
                {
                    using (Process elevatedProcess = new())
                    {
                        elevatedProcess.StartInfo.Verb = "runas";
                        elevatedProcess.StartInfo.UseShellExecute = true;
                        elevatedProcess.StartInfo.FileName = Environment.ProcessPath;
                        elevatedProcess.StartInfo.Arguments = "elevate";
                        _ = elevatedProcess.Start();
                    }
                    return false;
                }
                catch (Win32Exception)
                {
                    return true;
                }
            }
            return true;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _ = e.ExceptionObject as Exception;
            // Log error
        }

        private static async void InitializeAppServiceConnection()
        {
            string packageFamilyName = Package.Current.Id.FamilyName;

            try
            {
                using CancellationTokenSource cts = new();
                cts.CancelAfter(TimeSpan.FromSeconds(15));

                connection = new()
                {
                    PackageFamilyName = packageFamilyName,
                    AppServiceName = "UUPMediaCreatorService"
                };

                connection.ServiceClosed += OnServiceClosed;
                connection.RequestReceived += OnConnectionRequestReceived;

                AppServiceConnectionStatus connectionStatus = await connection.OpenAsync();
                if (connectionStatus != AppServiceConnectionStatus.Success)
                {
                    throw new Exception("Could not connect to the main application!");
                }
            }
            catch (Exception)
            {

            }
        }

        private static void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            connection.ServiceClosed -= OnServiceClosed;
            connection.RequestReceived -= OnConnectionRequestReceived;
            connection = null;
            _ = (appServiceExit?.Set());
        }

        private static async void OnConnectionRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs arguments)
        {
            ValueSet message = arguments.Request.Message;
            AppServiceDeferral deferral = arguments.GetDeferral();

            if (message == null)
            {
                return;
            }

            if (message.ContainsKey("InterCommunication"))
            {
                try
                {
                    await Task.Run(() => ParseInterCommunicationMessage(message, arguments, deferral));
                }
                catch (Exception) { }
            }
            else
            {
                deferral.Complete();
            }
        }

        private static void ParseInterCommunicationMessage(ValueSet message, AppServiceRequestReceivedEventArgs arguments, AppServiceDeferral deferral)
        {
            Common.InterCommunication interCommunication = JsonSerializer.Deserialize<Common.InterCommunication>((string)message["InterCommunication"]);

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
                            _ = await arguments.Request.SendResponseAsync(val);
                        });

                        thread.Start();
                        thread.Join();

                        deferral.Complete();

                        _ = (appServiceExit?.Set());
                        break;
                    }

                case Common.InterCommunicationType.IsElevated:
                    {
                        Thread thread = new(async () =>
                        {
                            ValueSet val = new()
                            {
                                { "InterCommunication", IsAdministrator() }
                            };
                            _ = await arguments.Request.SendResponseAsync(val);
                        });

                        thread.Start();
                        thread.Join();

                        deferral.Complete();
                        break;
                    }

                case Common.InterCommunicationType.StartISOConversionProcess:
                    {
                        static async void callback(Common.ProcessPhase phase, bool IsIndeterminate, int ProgressInPercentage, string SubOperation)
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

                            _ = await connection.SendMessageAsync(val);
                        }

                        Thread thread = new(async () =>
                        {
                            try
                            {
                                MediaCreator.CreateISOMedia(
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

                                _ = await connection.SendMessageAsync(val);
                            }
                        });

                        thread.Start();

                        deferral.Complete();
                        break;
                    }

                default:

                    deferral.Complete();
                    break;
            }
        }
    }
}