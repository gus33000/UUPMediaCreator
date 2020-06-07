using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UUPMediaCreator.UWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            this.InitializeComponent();
        }

        public void WizardPage_NextClicked(object sender, RoutedEventArgs e)
        {
            //Frame.Navigate(typeof(BuildingISOPage));
            Frame.Navigate(typeof(ArchitecturePage));
        }
    }
}
