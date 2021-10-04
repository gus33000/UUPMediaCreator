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
using UUPMediaCreator.InterCommunication;
using UUPMediaCreator.UWP.Pages;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WindowsUpdateLib;
using static UUPMediaCreator.InterCommunication.Common;

namespace UUPMediaCreator
{
    public enum MediumType
    {
        WindowsInstallationMedium,
        WindowsFeatureOnDemandMedium,
        WindowsLanguagePackMedium
    }

    public enum InstallationMediumType
    {
        ISO,
        InstallWIM,
        BootWIM,
        VHD
    }

    public enum InstallationWIMMediumType
    {
        XPRESS,
        LZX,
        LZMS
    }

    public class ConversionPlan
    {
        public UpdateData UpdateData { get; set; }
        public string Language { get; set; }
        public string LanguageTitle { get; set; }
        public Uri FlagUri { get; set; }
        public string Edition { get; set; }
        public string BuildString { get; set; }

        //public string FakeEdition { get; set; }
        //public string VirtualEdition { get; set; }
        public MachineType MachineType { get; set; }

        public MediumType MediumType { get; set; }
        public InstallationMediumType InstallationMediumType { get; set; }
        public InstallationWIMMediumType InstallationWIMMediumType { get; set; }
        //public string[] FODs { get; set; }
        //public bool IntegrateUpdates { get; set; }

        public string TmpOutputFolder { get; set; }

        public string ISOPath { get; set; }
    }

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        public static BackgroundTaskDeferral AppServiceDeferral = null;
        public static AppServiceConnection Connection = null;
        public static ConversionPlan ConversionPlan = new();

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                ShowMainPage();
            }
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            // connection established from the fulltrust process
            base.OnBackgroundActivated(args);
            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
            {
                AppServiceDeferral = args.TaskInstance.GetDeferral();
                args.TaskInstance.Canceled += OnTaskCanceled;
                Connection = details.AppServiceConnection;
                Connection.RequestReceived += OnRequestReceived;
                ShowMainPage();
            }
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            AppServiceDeferral?.Complete();
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            ApplicationView.PreferredLaunchViewSize = new Size(800, 600);
            ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private async void ShowMainPage()
        {
            await Windows.ApplicationModel.FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (Window.Current.Content is not Frame rootFrame)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (Connection != null)
                {
                    rootFrame.Navigate(typeof(WelcomePage));

                    SystemNavigationManagerPreview mgr = SystemNavigationManagerPreview.GetForCurrentView();
                    mgr.CloseRequested += SystemNavigationManager_CloseRequested;
                }
                else
                {
                    rootFrame.Navigate(typeof(AdministratorAccessRequiredPage));
                }
            }
            else if (rootFrame.Content.GetType() == typeof(AdministratorAccessRequiredPage) && Connection != null)
            {
                rootFrame.Navigate(typeof(WelcomePage));

                SystemNavigationManagerPreview mgr = SystemNavigationManagerPreview.GetForCurrentView();
                mgr.CloseRequested += SystemNavigationManager_CloseRequested;
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private async void SystemNavigationManager_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            Deferral deferral = e.GetDeferral();
            ContentDialog dlg = new() { Title = "Do you really want to exit the application?", PrimaryButtonText = "Yes", SecondaryButtonText = "No" };
            ContentDialogResult result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Secondary)
            {
                // user cancelled the close operation
                e.Handled = true;
                deferral.Complete();
            }
            else
            {
                if (Connection != null)
                {
                    ValueSet message = new();
                    message.Add("InterCommunication", JsonSerializer.Serialize(new Common.InterCommunication() { InterCommunicationType = InterCommunicationType.Exit }));
                    await Connection.SendMessageAsync(message);
                }
                e.Handled = false;
                deferral.Complete();
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            ShowMainPage();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}