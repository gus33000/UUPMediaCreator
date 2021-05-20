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
            Windows.Storage.Pickers.FileSavePicker savePicker = new()
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
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