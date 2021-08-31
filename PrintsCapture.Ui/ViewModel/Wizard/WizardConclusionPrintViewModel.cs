using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using PrintsCapture.Prints;
using PrintsCapture.Prints.Enum;
using PrintsCapture.Prints.Language;
using PrintsCapture.Prints.ViewModel;
using PrintsCapture.Ui.Language;

namespace PrintsCapture.Ui.ViewModel.Wizard
{
    /// <summary>
    /// Selected print viewmodel for conclusion Window
    /// </summary>
    public class WizardConclusionPrintViewModel : INotifyPropertyChanged
    {
        private readonly List<OverrideReason> allOverrides;

        private WizardActionItem selectedAction;
        private string otherReason;

        public bool AlwaysCanOverride { get; private set; }

        public WizardPrintViewModel BaseVm { get; private set; }

        public WizardConclusionPrintViewModel(WizardPrintViewModel baseVm,List<OverrideReason> allOverrides, bool alwaysCanOverride = false)
        {
            this.allOverrides = allOverrides;
            this.AlwaysCanOverride = alwaysCanOverride;

            this.EditButtonVisibility = baseVm.Print.LinkedPrint.PrintList.Rules.CaptureKind == CaptureKind.Cardscan
                ? Visibility.Visible
                : Visibility.Collapsed;

            this.ScanMode = baseVm.Print.LinkedPrint.ScanKind == HandScanKind.Flat ? Language.Text.ScanKindFlat : Text.ScanKindRolled;
            this.BaseVm = baseVm;
            this.Update();
        }

        public void Update()
        {
                        
            var pr = this.BaseVm.Print.LinkedPrint;
            
            bool canOverride = true;
            bool canAccept = true;

            if (pr.ProcessStatus == PrintProcessStatus.TemplateError)
            {
                canOverride = false;
                canAccept = false;
                this.WizardError = Text.WizardPrintInvalid;
            }
            else if (pr.Status == PrintStatus.InError || pr.Status == PrintStatus.InErrorSwapped)
            {
                canAccept = false;
                this.WizardError = Text.WizardPrintWarning;
            }
            else
            {
                canOverride = false;
                this.WizardError = Text.WizardPrintOk;
            }

            if (pr.IsEndorsement || pr.TemplateErrors.Any( x => x == TemplateError.WrongTemplateCount))
            {
                canOverride = false;
            }

            if (pr.Status == PrintStatus.Empty)
            {
                canAccept = false;
            }



            var actionList = new List<WizardActionItem>();
            this.SegmentActionList = new List<WizardActionItem>();
            // TODO : Cardscan does not have this option
            actionList.Add(new WizardActionItem() { Action = WizardAction.ScanAgain, Label = Text.WizardActionScan});

            if (canAccept)
            {
                actionList.Add( new WizardActionItem() { Label = CommonText.Accept, Action = WizardAction.Accept});
            }
            this.SegmentActionList.Add(new WizardActionItem() { Label = CommonText.Accept, Action = WizardAction.Accept });

            if (canOverride || this.AlwaysCanOverride)
            {
                foreach (var overrideReason in this.allOverrides)
                {
                    var wai = new WizardActionItem();
                    wai.Label = CommonText.Accept + " - " + overrideReason.Text;
                    wai.Action = (WizardAction)overrideReason.Code;
                    wai.Reason = overrideReason;
                    actionList.Add(wai);
                    this.SegmentActionList.Add(wai);
                }
            }
            // TODO : Cardscan action ONLY
            //actionList.Add(new WizardActionItem() { Action = WizardAction.ValidateAgain, Label = "Validate again"});

            this.ActionList = actionList;

            var currentOverride = this.allOverrides.FirstOrDefault(x => x.Code == pr.OverrideCode);
            this.OtherReasonVisibility = currentOverride?.HasUserText == true
                ? Visibility.Visible
                : Visibility.Collapsed;

            this.OtherReason = pr.OverrideUserReason;


            this.PrintSegments =
                pr.Segments?.Select(x => new SegmentInfoViewModel(pr, x)).ToList();

            this.OnPropertyChanged(nameof(this.BaseVm));
            this.OnPropertyChanged(nameof(this.ActionList));

            this.SelectedAction = this.ActionList.FirstOrDefault(x => x.Action == pr.UserAction);
        }

        public List<WizardActionItem> SegmentActionList { get; set; }


        public string WizardError { get; private set; }

        public string ScanMode { get; private set; }

        public List<WizardActionItem> ActionList { get; private set; }

        public WizardActionItem SelectedAction
        {
            get { return selectedAction; }
            set
            {
                if (this.selectedAction == value) return;
                selectedAction = value;
                this.BaseVm.Print.LinkedPrint.UserAction = this.selectedAction.Action;
                this.OnPropertyChanged(nameof(this.SelectedAction));

                var code = (int)(this.selectedAction.Action & WizardAction.OverrideCodeMask);

                if (code > 0)
                {
                    this.BaseVm.Print.LinkedPrint.OverrideCode = code;
                    this.OtherReasonVisibility = this.selectedAction.Reason.HasUserText
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    if (!this.selectedAction.Reason.HasUserText)
                    {
                        this.BaseVm.Print.LinkedPrint.OverrideUserReason = null;
                    }
                    //Set segment overrides
                }
                else
                {
                    this.BaseVm.Print.LinkedPrint.OverrideCode = 0;
                    this.OtherReasonVisibility = Visibility.Collapsed;
                }

                this.BaseVm.SetStyle();
                this.OnPropertyChanged(nameof(this.OtherReasonVisibility));

            }
        }


        public Visibility OtherReasonVisibility { get; private set; }

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
                    this.BaseVm.Print.LinkedPrint.OverrideUserReason = string.Empty;
                }
                else
                {
                    otherReason = value.ToUpper();
                    var reg = new Regex("[^A-Z -]");
                    var match = reg.Match(otherReason).Success;
                    OtherReasonInvalidVisibility = match
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                    this.BaseVm.Print.LinkedPrint.OverrideUserReason = match ?
                        string.Empty :
                        value;
                }


                this.OnPropertyChanged(nameof(this.OtherReason));
                this.OnPropertyChanged(nameof(this.OtherReasonInvalidVisibility));
            }
        }

        public Visibility OtherReasonInvalidVisibility { get; set; }

        public Visibility EditButtonVisibility { get; private set; }


        public List<SegmentInfoViewModel> PrintSegments { get; private set; }

        public WizardConclusionSegmentViewModel CurrentSegment { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetCurrentSegment(SegmentInfoViewModel vm)
        {
            this.CurrentSegment = new WizardConclusionSegmentViewModel(vm,
                this.SegmentActionList.FirstOrDefault(x => x.Action == vm.UserAction)
                );
            this.OnPropertyChanged(nameof(this.CurrentSegment));
        }
    }
}
