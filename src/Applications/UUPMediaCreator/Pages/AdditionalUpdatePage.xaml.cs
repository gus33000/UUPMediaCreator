using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UUPMediaCreator.UWP.Pages
{
    public sealed partial class AdditionalUpdatePage : Page
    {
        public AdditionalUpdatePage()
        {
            this.InitializeComponent();
        }

        private void WizardPage_NextClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WIMTypePage));
        }

        private void WizardPage_BackClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}