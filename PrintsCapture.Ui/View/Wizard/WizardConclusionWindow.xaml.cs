using PrintsCapture.Prints;
using PrintsCapture.Ui.Language;
using PrintsCapture.Ui.ViewModel.Wizard;

namespace PrintsCapture.Ui.View
{
    using System.Windows;
    using System.Windows.Controls;

    using PrintsCapture.Prints.ViewModel;
    using PrintsCapture.Ui.Class;
    using PrintsCapture.Ui.ViewModel;

    /// <summary>
    /// Interaction logic for WizardConclusionWindow.xaml
    /// </summary>
    public partial class WizardConclusionWindow : Window
    {
        public WizardConclusionViewModel ViewModel { get; private set; }

        public WizardConclusionWindow(WizardConclusionViewModel viewModel)
        {
            this.ViewModel = viewModel;
            InitializeComponent();
        }
        

        private void AdvancedInfoCheckedChange(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;

            var newVisibility = (cb?.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;

            this.ViewModel.AdvancedInformationVisibility = newVisibility;

        }

        private void ApplyActionButtonClick(object sender, RoutedEventArgs e)
        {

            if (!this.ViewModel.IsValid())
            {
                MessageBox.Show(Text.WizardReasonInvalid, Text.WizardPrintWarning, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            this.ViewModel.ApplyActions();
            this.DialogResult = true;
            this.Close();
            PrintCaptureDriver.Instance.CapturePositions();
        }

        private void PreviousButtonClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void NextButtonClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void SegmentMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var vm = (sender as FrameworkElement)?.Tag as SegmentInfoViewModel;

            if (this.SegmentPopup.IsOpen)
            {
                this.SegmentPopup.IsOpen = false;
            }

            this.ViewModel.SetCurrentSegment(vm);

            this.SegmentPopup.IsOpen = true;            
        }

        private void ScaleSegments()
        {
            var ps = this.ViewModel.DisplayedPrint.PrintSegments;
            if (ps != null)
            {
                foreach (var segment in ps)
                {
                    segment.Scale(this.ImageBorder);
                }
            }
        }

        private void ImageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ScaleSegments();
        }        

        private void SegmentPopupClosed(object sender, System.EventArgs e)
        {
            
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void StatusLabelMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.ViewModel.IsStatusDisplayed = true;
        }        

        private void ReviewPrintClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.IsStatusDisplayed = false;
        }

        private void PrintStatusMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var vm = (sender as FrameworkElement)?.Tag as WizardPrintViewModel;
            if (vm == null)
            {
                return;
            }

            this.ViewModel.IsStatusDisplayed = false;
            this.ViewModel.SetDisplayedPrint(vm);
            this.ScaleSegments();
        }

        private void LeftArrowMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.ViewModel.DisplayPrevious();
            this.ScaleSegments();
        }

        private void RightArrowMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.ViewModel.DisplayNext();
            this.ScaleSegments();
        }
    }
}
