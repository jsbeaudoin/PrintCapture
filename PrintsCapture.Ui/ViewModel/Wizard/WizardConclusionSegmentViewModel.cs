using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using PrintsCapture.Prints;
using PrintsCapture.Prints.ViewModel;
using System.Text.RegularExpressions;

namespace PrintsCapture.Ui.ViewModel.Wizard
{
    public class WizardConclusionSegmentViewModel : INotifyPropertyChanged
    {
        private WizardActionItem selectedAction;
        private string otherReason;
        public SegmentInfoViewModel BaseVm { get; private set; }        

        public WizardActionItem SelectedAction
        {
            get { return selectedAction; }
            set
            {
                if (this.selectedAction == value) return;
                selectedAction = value;
                this.BaseVm.UserAction = this.selectedAction.Action;
                this.OnPropertyChanged(nameof(this.SelectedAction));

                var code = (int)(this.selectedAction.Action & WizardAction.OverrideCodeMask);

                if (code > 0)
                {
                    this.BaseVm.OverrideCode = code;

                    this.OtherReasonVisibility = this.selectedAction.Reason.HasUserText
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
                else
                {
                    this.BaseVm.OverrideCode = 0;
                    this.OtherReasonVisibility = Visibility.Collapsed;
                }
                this.OnPropertyChanged(nameof(this.OtherReasonVisibility));

            }
        }

        public Visibility OtherReasonVisibility { get; private set; }

        public Visibility OtherReasonInvalidVisibility { get; set; }

        public string OtherReason
        {
            get { return otherReason; }
            set
            {
                otherReason = value;
                this.OnPropertyChanged(nameof(this.OtherReason));

                if (string.IsNullOrEmpty(value))
                {
                    OtherReasonInvalidVisibility = this.OtherReasonVisibility == Visibility.Visible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
                else
                {
                    otherReason = otherReason.ToUpper();
                    var reg = new Regex("[^A-Z -]");
                    var match = reg.Match(otherReason).Success;
                    OtherReasonInvalidVisibility = match
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                    this.BaseVm.OverrideText = match ?
                        string.Empty :
                        value;
                }



                this.OnPropertyChanged(nameof(this.OtherReasonInvalidVisibility));
                this.OnPropertyChanged(nameof(this.OtherReason));
            }
        }

        public WizardConclusionSegmentViewModel(SegmentInfoViewModel baseVm, WizardActionItem selectedAction)
        {
            BaseVm = baseVm;

            this.SelectedAction = selectedAction;
            this.OtherReason = baseVm.OverrideText;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
