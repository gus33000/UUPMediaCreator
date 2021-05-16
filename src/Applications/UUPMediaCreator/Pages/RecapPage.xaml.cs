using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace UUPMediaCreator.UWP.Pages
{
    public sealed partial class RecapPage : Page
    {
        public RecapPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            BuildStringTextBlock.Text = App.ConversionPlan.BuildString ?? App.ConversionPlan.UpdateData.Xml.LocalizedProperties.Title ?? "";
            ArchitectureTextBlock.Text = App.ConversionPlan.MachineType.ToString();
            LanguageTextBlock.Text = App.ConversionPlan.LanguageTitle;
            FlagBitmap.UriSource = App.ConversionPlan.FlagUri;
            WIMTypeTextBlock.Text = App.ConversionPlan.InstallationWIMMediumType.ToString();
            MediumTypeTextBlock.Text = App.ConversionPlan.MediumType.ToString();
            EditionTextBlock.Text = App.ConversionPlan.Edition;
        }

        private void WizardPage_NextClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(DownloadPage));
        }

        private void WizardPage_BackClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            WizardPage.NextEnabled = !string.IsNullOrEmpty(PathTextBox.Text);
            App.ConversionPlan.ISOPath = PathTextBox.Text;
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Disc Image File", new List<string>() { ".iso" });
            savePicker.SuggestedFileName = "Windows.iso";

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                PathTextBox.Text = file.Path;
            }
        }
    }
}