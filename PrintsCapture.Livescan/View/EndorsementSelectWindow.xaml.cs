namespace PrintsCapture.Livescan.View
{
    using System.Windows;

    using PrintsCapture.Livescan.ViewModel;

    /// <summary>
    /// Interaction logic for EndorsementSelectWindow.xaml
    /// </summary>
    public partial class EndorsementSelectWindow : Window
    {
        public EndorsementSelectViewModel ViewModel { get; set; }

        public EndorsementSelectWindow(EndorsementSelectViewModel viewModel)
        {
            this.ViewModel = viewModel;
            InitializeComponent();
        }

        private void AcceptButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
