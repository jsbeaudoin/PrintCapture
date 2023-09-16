namespace PrintsCapture.Ui.View
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    using PrintsCapture.Ui.Language;
    using PrintsCapture.Ui.ViewModel;

    /// <summary>
    /// Interaction logic for FingerPresence.xaml
    /// </summary>
    public partial class FingerPresenceWindow : Window
    {                
        
        public FingerPresenceWindow(FingerPresenceViewModel viewModel)
        {
            this.ViewModel = viewModel;

            InitializeComponent();

            this.DisplaySelectedReason();
        }

        public FingerPresenceViewModel ViewModel { get; private set; }

        private void BtnAcceptClick(object sender, RoutedEventArgs e)
        {            
            // check for errors

            if (Validation.GetHasError(this.MissingDateTextBox))
            {
                MessageBox.Show(Text.CorrectErrorBeforeContinue);
                return;
            }

            // check for Parent and validate
            if (this.ViewModel.Parent != null && this.ViewModel.Parent.IsMissing && string.IsNullOrEmpty(this.ViewModel.MissingCode))
            {
                var parent = this.ViewModel.Parent;
                MessageBox.Show(Text.FingerNotPresentParentMissing);
                this.DialogResult = false;
            } else
            {
                this.DialogResult = true;
            }
            
            this.Close();
        }

        private void ReasonButtonClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleButton;
            if (btn == null)
            {
                return;
            }
            var reason = (string)btn.Tag;
            this.ViewModel.MissingCode = reason;

            this.DisplaySelectedReason();
        }

        private void DisplaySelectedReason()
        {
            this.PresentButton.IsChecked = string.IsNullOrEmpty(this.ViewModel.MissingCode);

            foreach (var model in this.ViewModel.Reasons)
            {
                model.IsSelected = model.Code == this.ViewModel.MissingCode;
            }
        }
        
    }
}
