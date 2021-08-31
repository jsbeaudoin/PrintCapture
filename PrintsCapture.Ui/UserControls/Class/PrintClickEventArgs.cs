namespace PrintsCapture.Ui.UserControls
{
    using System.Windows;

    using PrintsCapture.Prints.ViewModel;    

    public class PrintClickEventArgs : RoutedEventArgs
    {
        public PrintClickEventArgs(PrintElementViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public PrintElementViewModel ViewModel { get; set; }
    }
}
