using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UUPMediaCreator.UWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MediumTypePage : Page
    {
        public MediumTypePage()
        {
            this.InitializeComponent();
        }

        private void WizardPage_NextClicked(object sender, RoutedEventArgs e)
        {
            if (InstallMediumRadioButton.IsChecked.Value)
            {
                App.ConversionPlan.MediumType = MediumType.WindowsInstallationMedium;
                Frame.Navigate(typeof(LanguagePage));
            }
            /*else if (LanguageMediumRadioButton.IsChecked.Value)
            {
                App.ConversionPlan.MediumType = MediumType.WindowsLanguagePackMedium;
            }
            else if (FODMediumRadioButton.IsChecked.Value)
            {
                App.ConversionPlan.MediumType = MediumType.WindowsFeatureOnDemandMedium;
            }*/
        }

        private void WizardPage_BackClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}