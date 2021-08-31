namespace PrintsCapture.Ui.View
{
    using System.Windows;

    using PrintsCapture.Ui.ViewModel;

    /// <summary>
    /// Interaction logic for EndorsementConsentWindow.xaml
    /// </summary>
    public partial class EndorsementConsentWindow : Window
    {
        public EndorsementConsentViewModel ViewModel { get; set; }

        public EndorsementConsentWindow(EndorsementConsentViewModel viewModel)
        {
            this.ViewModel = viewModel;
            InitializeComponent();
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
