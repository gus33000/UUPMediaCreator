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
    public sealed partial class LanguagePage : Page
    {
        public LanguagePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            WizardPage.Glyph = "\uE128";
            WizardPage.Title = "Loading languages";
            WizardPage.Subtitle = "🥁 Drumroll...";
            WizardPage.BackEnabled = false;
            WizardPage.NextEnabled = false;
            LoadingRing.Visibility = Visibility.Visible;
            SelectionGrid.Visibility = Visibility.Collapsed;
            _ = ThreadPool.RunAsync(async (IAsyncAction operation) =>
            {
                var updates = await BuildFetcher.GetAvailableBuildLanguagesAsync(App.ConversionPlan.UpdateData);

                await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    WizardPage.Glyph = "\uE775";
                    WizardPage.Title = "Ciao, Hola, Bonjour, Hello oder Hallo?";
                    WizardPage.Subtitle = "What language do you speak?";
                    WizardPage.BackEnabled = true;
                    WizardPage.NextEnabled = true;
                    dataGrid.ItemsSource = updates;
                    dataGrid.SelectedIndex = 0;
                    LoadingRing.Visibility = Visibility.Collapsed;
                    SelectionGrid.Visibility = Visibility.Visible;
                });
            });
        }

        private void WizardPage_NextClicked(object sender, RoutedEventArgs e)
        {
            App.ConversionPlan.Language = (dataGrid.SelectedItem as BuildFetcher.AvailableBuildLanguages).LanguageCode;
            App.ConversionPlan.LanguageTitle = (dataGrid.SelectedItem as BuildFetcher.AvailableBuildLanguages).Title;
            App.ConversionPlan.FlagUri = (dataGrid.SelectedItem as BuildFetcher.AvailableBuildLanguages).FlagUri;
            Frame.Navigate(typeof(EditionPage));
        }

        private void WizardPage_BackClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
