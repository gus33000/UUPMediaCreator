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
using System.Threading.Tasks;
using UnifiedUpdatePlatform.Common.Messaging;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UUPMediaCreator.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        public async Task<bool> IsBrokerElevated()
        {
            Common.UnifiedUpdatePlatform.Common.Messaging comm = new()
            {
                UnifiedUpdatePlatform.Common.MessagingType = Common.UnifiedUpdatePlatform.Common.MessagingType.IsElevated
            };

            ValueSet val = new()
            {
                { "UnifiedUpdatePlatform.Common.Messaging", JsonSerializer.Serialize(comm) }
            };

            AppServiceResponse response = await App.Connection.SendMessageAsync(val);
            if (response.Message.ContainsKey("UnifiedUpdatePlatform.Common.Messaging"))
            {
                if (response.Message["UnifiedUpdatePlatform.Common.Messaging"] is bool adminStatus)
                {
                    return adminStatus;
                }
            }

            return false;
        }

        public void WizardPage_NextClicked(object sender, RoutedEventArgs e)
        {
            //Frame.Navigate(typeof(BuildingISOPage));
            _ = Frame.Navigate(typeof(ArchitecturePage));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AdminWarningBar.IsOpen = !await IsBrokerElevated();
        }
    }
}