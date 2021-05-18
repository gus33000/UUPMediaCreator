using System;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WindowsUpdateLib;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UUPMediaCreator.UWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BuildPage : Page
    {
        public BuildPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            WizardPage.Glyph = "\uE128";
            WizardPage.Title = "Loading builds";
            WizardPage.Subtitle = "This process might take a few minutes to complete.";
            WizardPage.BackEnabled = false;
            WizardPage.NextEnabled = false;
            _ = ThreadPool.RunAsync(async (IAsyncAction operation) =>
            {
                var updates = await BuildFetcher.GetAvailableBuildsAsync(App.ConversionPlan.MachineType);

                await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    WizardPage.Glyph = "\uF785";
                    WizardPage.Title = "What version of windows do you want to build media for?";
                    WizardPage.Subtitle = "The selected build will be used for the final medium";
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
            App.ConversionPlan.UpdateData = (dataGrid.SelectedItem as BuildFetcher.AvailableBuild).UpdateData;
            App.ConversionPlan.BuildString = (dataGrid.SelectedItem as BuildFetcher.AvailableBuild).BuildString;
            Frame.Navigate(typeof(LanguagePage));
        }

        private void WizardPage_BackClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemDescription.Text = (e.AddedItems[0] as BuildFetcher.AvailableBuild).Description;
        }
    }
}