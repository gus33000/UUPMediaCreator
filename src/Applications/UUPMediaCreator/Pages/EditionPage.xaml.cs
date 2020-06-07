using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WindowsUpdateLib;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UUPMediaCreator.UWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditionPage : Page
    {
        public EditionPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            WizardPage.Glyph = "\uE128";
            WizardPage.Title = "Loading editions";
            WizardPage.Subtitle = "🥁 Drumroll...";
            WizardPage.BackEnabled = false;
            WizardPage.NextEnabled = false;
            LoadingRing.Visibility = Visibility.Visible;
            SelectionGrid.Visibility = Visibility.Collapsed;
            _ = ThreadPool.RunAsync(async (IAsyncAction operation) =>
            {
                var editions = await BuildFetcher.GetAvailableEditions(App.ConversionPlan.UpdateData, App.ConversionPlan.Language);

                await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    WizardPage.Glyph = "\uE7B8";
                    WizardPage.Title = "Select the Windows edition";
                    WizardPage.Subtitle = "";
                    WizardPage.BackEnabled = true;
                    WizardPage.NextEnabled = true;
                    dataGrid.ItemsSource = editions;
                    dataGrid.SelectedIndex = 0;
                    LoadingRing.Visibility = Visibility.Collapsed;
                    SelectionGrid.Visibility = Visibility.Visible;
                });
            });
        }

        private void WizardPage_NextClicked(object sender, RoutedEventArgs e)
        {
            App.ConversionPlan.Edition = (dataGrid.SelectedItem as BuildFetcher.AvailableEdition).Edition;
            Frame.Navigate(typeof(AdditionalUpdatePage));
        }

        private void WizardPage_BackClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
