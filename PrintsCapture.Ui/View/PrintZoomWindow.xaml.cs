using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PrintsCapture.Prints;
using PrintsCapture.Prints.Enum;
using PrintsCapture.Prints.Extension;
using PrintsCapture.Prints.ViewModel;
using PrintsCapture.Ui.Language;
using PrintsCapture.Ui.ViewModel;
using XL_ID.Utilities.Wpf.Extension;

namespace PrintsCapture.Ui.View
{
    /// <summary>
    /// Interaction logic for PrintZoomWindow.xaml
    /// </summary>
    public partial class PrintZoomWindow : INotifyPropertyChanged
    {
        private PrintZoomViewModel viewModel;

        private Brush optionNoChosenBrush;

        public PrintZoomWindow()
        {
            InitializeComponent();
            PrintModificationDispatcher.AddWatch(this.PrintModified);

            this.Closed += (s, e) =>
            {
                PrintModificationDispatcher.RemoveWatch(this.PrintModified);
                this.PrintImage.Source = null;
                this.PrintImage.UpdateLayout();
            };
        }

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        public PrintZoomViewModel ViewModel
        {
            get
            {
                return this.viewModel;
            }
            set
            {
                if (Equals(value, this.viewModel))
                {
                    return;
                }
                this.viewModel = value;

                this.EndorsementBypassButton.Visibility = (this.viewModel !=null && this.viewModel.Print.HandPart == HandPart.Endorsement)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                this.OnPropertyChanged("ViewModel");
                this.DisplayPrint();
            }
        }

        private void DisplayPrint()
        {
            if (this.viewModel?.Print?.ImageForProcessing != null)
            {
                var imageDisplay = WriteableBitmapExtension.FromBitmap(this.viewModel.Print.ImageForProcessing);
                imageDisplay.WriteBitmap(this.viewModel.Print.ImageForProcessing);
                this.PrintImage.Source = imageDisplay;
                this.PrintImage.UpdateLayout();
            } else
            {
                this.PrintImage.Source = null;
                this.PrintImage.UpdateLayout();
            }
            
            
        }

        private void PrintModified(object sender, PrintModifiedEventArgs e)
        {
            if (e.Info == this.viewModel.Print)
            {
                this.ViewModel = new PrintZoomViewModel(e.Info);
            }
        }

        

        public event PropertyChangedEventHandler PropertyChanged;
       
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void PrintActionClick(object sender, RoutedEventArgs e)
        {
            var selectedKey = (PrintZoomAction)((Button)sender).Tag;

            this.viewModel.ActionSelected = selectedKey;

            this.DialogResult = true;
            this.Close();
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.viewModel.ActionSelected = PrintZoomAction.SavePrintChanges;
            this.DialogResult = true;
            this.Close();
        }

        private void ResolutionDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void SequenceTextBlockMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
            {
                return;
            }

            var msg = string.Empty;
            var prints = this.viewModel.Print.PrintList.Prints;

            foreach (var result in this.ViewModel.Print.AllScores.OrderByDescending(x => x.Score).ThenBy(x => x.Position))
            {
                var matchedPrint = prints.SingleOrDefault(x => x.NistPosition == result.Position);
                if (matchedPrint != null)
                {
                    msg += string.Format("{0} = {1} \n", PrintList.GetName(matchedPrint), result.Score);
                }                
            }

            this.DebugText.Text = msg;

            this.DebugPopup.IsOpen = !this.DebugPopup.IsOpen;
        }

        private void PopupLabelMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DebugPopup.IsOpen = false;
        }             

        private void ImageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ViewModel.PrintSegments != null)
            {
                foreach (var segment in this.ViewModel.PrintSegments)
                {
                    segment.Scale(this.ImageBorder);
                }
            }
        }

        private void SegmentMouseDown(object sender, MouseButtonEventArgs e)
        {
            var currentBorder = sender as Border;
            SegmentInfoViewModel segment = null;
           
            if (currentBorder != null)
            {
                segment = currentBorder.Tag as SegmentInfoViewModel;
            }

            this.SelectSegment(segment);

            e.Handled = true;
        }

        private void SelectSegment(SegmentInfoViewModel segment)
        {
            if (this.ViewModel.SelectedSegment == segment)
            {
                return;
            }            

            var segmentBorders = this.SegmentsItemsControl.FindVisualChildren<Border>().ToList();            
            var titleLabels = this.SegmentListItemsControl.FindVisualChildren<Label>().ToList();
            

            var previousSegment = this.ViewModel.SelectedSegment;
            if (previousSegment != null)
            {                
                var rect = segmentBorders.Single(x => x.Tag == previousSegment).FindName("PART_Rect") as Rectangle; 
                rect.Fill = this.ViewModel.BrushForNotSelected;
                titleLabels.Single(x => x.Tag == previousSegment).Background = this.viewModel.BrushForNotSelected;  
            }

            if (segment != null)
            {
                var rect = segmentBorders.Single(x => x.Tag == segment).FindName("PART_Rect") as Rectangle;
                rect.Fill = this.ViewModel.BrushForSelected;
                titleLabels.Single(x => x.Tag == segment).Background = this.viewModel.BrushForSelected;  
            }            

            this.ViewModel.SelectedSegment = segment;
        }

        private void DisplayPopup(int code, string text)
        {
            this.OverrideListPopup.Visibility = Visibility.Visible;
            this.OverrideListPopup.IsOpen = true;

            if (this.optionNoChosenBrush == null)
            {
                this.optionNoChosenBrush = this.PopupNoOverrideButton.Background;
            }

            var overrideButtons = this.OverrideItemsControlPopup.FindVisualChildren<Button>();
            foreach (var button in overrideButtons)
            {
                var buttonReason = (OverrideReason) button.Tag;
                button.Background = buttonReason.Code == code ? this.ViewModel.BrushForSelected : this.optionNoChosenBrush;
            }

            this.PopupNoOverrideButton.Background = code == 0 ? this.ViewModel.BrushForSelected : this.optionNoChosenBrush;

            this.viewModel.OverridePopupText = text;

            this.OverrideListPopup.StaysOpen = false;
            
        }

        

        private void OverrideButtonClick(object sender, RoutedEventArgs e)
        {
            var el = sender as FrameworkElement;
            OverrideReason reason = null;
            string text = string.Empty;
            int code = 0;

            if (el.Tag is OverrideReason)
            {
                reason = (OverrideReason)el.Tag;
                text = reason.HasUserText ? this.ViewModel.OverridePopupText : string.Empty;
                code = reason.Code;
                if (reason.HasUserText && string.IsNullOrEmpty(text))
                {
                    this.ViewModel.OverrideErrorMessage = Text.OverrideReasonCannotBeBlank;
                    // if reason has a text field, it cannot be empty !
                    return;
                }
            }

            var overrideLabel = this.ViewModel.GetOverrideText(reason, text);

            if (this.ViewModel.PopupForSegment)
            {
                this.ViewModel.SelectedSegment.OverrideCode = code;
                this.viewModel.SelectedSegment.OverrideText = text;
                this.viewModel.SelectedSegment.OverrideLabel = overrideLabel;
            }
            else
            {
                this.viewModel.OverrideCode = code;
                this.viewModel.OverrideText = text;
                this.viewModel.OverrideLabel = overrideLabel;
            }

            this.ViewModel.OverridePopupText = string.Empty;
            this.ViewModel.OverrideErrorMessage = string.Empty;
            this.OverrideListPopup.IsOpen = false;            
        }

        private void PrintOverrideClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.PopupForSegment = false;
            this.OverrideListPopup.PlacementTarget = sender as UIElement;
            this.DisplayPopup(this.ViewModel.OverrideCode, this.ViewModel.OverrideText);
        }

        private void SegmentOverrideClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.PopupForSegment = true;
            this.OverrideListPopup.PlacementTarget = sender as UIElement;
            this.DisplayPopup(this.ViewModel.SelectedSegment.OverrideCode, this.ViewModel.SelectedSegment.OverrideText);
        }

        private void SegmentNameLabelClick(object sender, MouseButtonEventArgs e)
        {
            var label = sender as Label;
            var segment = label.Tag as SegmentInfoViewModel;

            this.SelectSegment(segment);
        }

        private void EndorsementBypassOverrideClick(object sender, RoutedEventArgs e)
        {
            var prn = this.viewModel.Print;
            prn.SequenceSelfScore = prn.PrintList.Rules.SequenceThreshold;
            prn.SequenceScore = prn.SequenceSelfScore;
            prn.SequenceBestScore = 0;

            this.viewModel.ActionSelected = PrintZoomAction.SavePrintChanges;
            this.DialogResult = true;
            this.Close();
        }

    }
}
