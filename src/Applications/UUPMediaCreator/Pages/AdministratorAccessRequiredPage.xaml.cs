using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UUPMediaCreator.UWP.Pages
{
    public sealed partial class AdministratorAccessRequiredPage : Page
    {
        public AdministratorAccessRequiredPage()
        {
            this.InitializeComponent();
        }

        private void WizardPage_NextClicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}