using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UUPMediaCreator.UWP.Pages
{
    public sealed partial class WIMTypePage : Page
    {
        public WIMTypePage()
        {
            this.InitializeComponent();
        }

        private void WizardPage_NextClicked(object sender, RoutedEventArgs e)
        {
            if (LZXRadioButton.IsChecked.Value)
            {
                App.ConversionPlan.InstallationWIMMediumType = InstallationWIMMediumType.LZX;
            }
            else if (LZMSRadioButton.IsChecked.Value)
            {
                App.ConversionPlan.InstallationWIMMediumType = InstallationWIMMediumType.LZMS;
            }
            else if (XPRESSRadioButton.IsChecked.Value)
            {
                App.ConversionPlan.InstallationWIMMediumType = InstallationWIMMediumType.XPRESS;
            }
            Frame.Navigate(typeof(RecapPage));
        }

        private void WizardPage_BackClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
