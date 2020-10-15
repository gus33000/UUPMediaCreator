using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UUPMediaCreator.UWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InstallMediumTypePage : Page
    {
        public InstallMediumTypePage()
        {
            this.InitializeComponent();
        }

        private void WizardPage_NextClicked(object sender, RoutedEventArgs e)
        {
            if (ISO.IsChecked.Value)
            {
                App.ConversionPlan.InstallationMediumType = InstallationMediumType.ISO;
                Frame.Navigate(typeof(AdditionalUpdatePage));
            }
            /*else if (InstallWindowsImage.IsChecked.Value)
            {
                App.ConversionPlan.InstallationMediumType = InstallationMediumType.InstallWIM;
                Frame.Navigate(typeof(FODPage));
            }
            else if (BootWindowsImage.IsChecked.Value)
            {
                App.ConversionPlan.InstallationMediumType = InstallationMediumType.BootWIM;
                Frame.Navigate(typeof(AdditionalUpdatePage));
            }
            else if (VirtualHardDisk.IsChecked.Value)
            {
                App.ConversionPlan.InstallationMediumType = InstallationMediumType.VHD;
                Frame.Navigate(typeof(FODPage));
            }*/
        }

        private void WizardPage_BackClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}