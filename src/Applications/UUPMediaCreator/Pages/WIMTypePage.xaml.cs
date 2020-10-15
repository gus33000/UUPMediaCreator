using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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