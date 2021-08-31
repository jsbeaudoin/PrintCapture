using PrintsCapture.Prints.Language;
using PrintsCapture.Ui.ViewModel.Wizard;

namespace PrintsCapture.Ui.View
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls.Primitives;

    using PrintsCapture.Ui.ViewModel;

    using XL_ID.Utilities.Log;
    

    /// <summary>
    /// Interaction logic for EndorsementSelectWindow.xaml
    /// </summary>
    public partial class WizardOptionWindow : Window
    {
        public WizardOptionViewModel ViewModel { get; private set; }

        public WizardOptionWindow(WizardOptionViewModel viewModel)
        {
            this.ViewModel = viewModel;
            
            this.InitializeComponent();

        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            return;
            //Calculate half of the offset to move the form
            try
            {
                if (sizeInfo.HeightChanged)
                    this.Top += (sizeInfo.PreviousSize.Height - sizeInfo.NewSize.Height) / 2;

                if (sizeInfo.WidthChanged)
                    this.Left += (sizeInfo.PreviousSize.Width - sizeInfo.NewSize.Width) / 2;
            }
            catch (Exception ex)
            {
                LogDispatcher.DoLog("WizardOptionWindow - OnRenderSizeChanged " + ex.Message, LogEventLevel.Error, ex);
            }
        }

        private void AcceptButtonClick(object sender, RoutedEventArgs e)
        {
            // First set all reasons if fingers are missing           

            this.DialogResult = true;
            this.Close();
        }

        //private void MissingStatusClick(object sender, RoutedEventArgs e)
        //{
        //    var reason = (string)((sender as ToggleButton)?.Tag);

        //    this.ViewModel.MissingReason = reason;

        //    this.SetToggleState(this.MissingToggleButton, reason);
        //    this.SetToggleState(this.InjuredToggleButton, reason);
        //    this.SetToggleState(this.BandagedToggleButton, reason);
        //}        

        //private void SetToggleState(ToggleButton ctrl, object tag)
        //{            
        //    bool match = Equals(ctrl.Tag, tag);

        //    if (match)
        //    {
        //        ctrl.IsHitTestVisible = false;
        //        ctrl.IsChecked = true;
        //    }
        //    else
        //    {
        //        ctrl.IsHitTestVisible = true;
        //        ctrl.IsChecked = false;
        //    }            
        //}

        private void MissingFingersButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.IsFirstStep = false;

            this.HandPartsControl.Initialize(this.ViewModel.PrintList);
            this.HandPartsControl.AllowConditionEdition = true;
            this.NextButtonText.Text = CommonText.WizardNext;
        }

        private void WizardOptionWindow_OnClosing(object sender, CancelEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
