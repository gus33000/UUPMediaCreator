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
using UnifiedUpdatePlatform.Services.WindowsUpdate;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UUPMediaCreator.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditionPage : Page
    {
        public EditionPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            WizardPage.Glyph = "\uE128";
            WizardPage.Title = "Loading editions";
            WizardPage.Subtitle = "This process might take a few minutes to complete.";
            WizardPage.BackEnabled = false;
            WizardPage.NextEnabled = false;
            LoadingRing.Visibility = Visibility.Visible;
            SelectionGrid.Visibility = Visibility.Collapsed;
            _ = ThreadPool.RunAsync(async (IAsyncAction operation) =>
            {
                BuildFetcher.AvailableEdition[] editions = await BuildFetcher.GetAvailableEditions(App.ConversionPlan.UpdateData, App.ConversionPlan.Language);

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
            App.ConversionPlan.Edition = (dataGrid.SelectedItem as BuildFetcher.AvailableEdition)?.Edition;
            //Frame.Navigate(typeof(AdditionalUpdatePage));
            _ = Frame.Navigate(typeof(WIMTypePage));
        }

        private void WizardPage_BackClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}