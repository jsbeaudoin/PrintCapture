using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using PrintsCapture.Prints;
using PrintsCapture.Prints.Enum;
using PrintsCapture.Prints.ViewModel;
using PrintsCapture.Ui.Class;
using PrintsCapture.Ui.Language;

namespace PrintsCapture.Ui.ViewModel.Wizard
{
    public class WizardConclusionViewModel : INotifyPropertyChanged
    {
        private readonly PrintList printList;

        private List<WizardPrintViewModel> printViewModels;        

        private Dictionary<string, Action<WizardPrintViewModel, PrintElementViewModel>> vmAssignations = new Dictionary<string, Action<WizardPrintViewModel, PrintElementViewModel>>();

        private readonly List<OverrideReason> overrideReasonList;

        private Visibility advancedInformationVisibility;

        private List<int> orderNist = new List<int>();
        private bool isStatusDisplayed;
        private string statusText;
        private string statusActionText;
        private bool AlwaysCanOverride { get; set; }

        public WizardConclusionViewModel(PrintList pl, bool alwaysCanOverride)
        {
            this.AlwaysCanOverride = alwaysCanOverride;
            this.printList = pl;
            this.printViewModels = new List<WizardPrintViewModel>(); //pl.GetAllViewModels(); //.ToDictionary( x => x.LinkedPrint.Key, y => y);
            orderNist.AddRange(new []{14, 13, 15, 12, 11, 2,3,4,5,7,8,9,10, 6, 1, 24, 22,25,26,27,28, 16});            

            foreach (var printInfo in pl.GetPrintListToVerify())
            {
                // all rescanned prints should be reevaluated
                if (printInfo.UserAction == WizardAction.ScanAgain)
                {
                    printInfo.UserAction = WizardAction.Undefined; 
                }
                var vm = pl.GetViewModel(printInfo);
                
                this.printViewModels.Add( new WizardPrintViewModel(vm));
            }
            this.printViewModels = this.printViewModels.OrderBy(x => orderNist.IndexOf(x.Print.LinkedPrint.NistPosition)).ToList();
            
            this.DefineAssignements();            


            PrintModificationDispatcher.AddWatch(this.PrintModifiedCallback);
            

            var lang = PrintCaptureApp.ApplicationCulture.TwoLetterISOLanguageName;
            this.overrideReasonList = OverrideReason.GetBaseList().Where(x => x.Language == lang).ToList();

            
            this.DefineVisibility();            

            this.ComputeWizardStatus();

            this.DisplayedPrint = new WizardConclusionPrintViewModel(this.Left4, this.overrideReasonList, alwaysCanOverride);
        }

        public WizardConclusionPrintViewModel DisplayedPrint { get; private set; }

        public Visibility TwoThumbsVisibility { get; private set; }

        public Visibility SingleThumbsVisibility { get; private set; }

        public Visibility RolledVisibility { get; private set; }

        public Visibility PalmVisibility { get; private set; }

        public Visibility EndorsementVisibility { get; private set; }

        public WizardPrintViewModel Endorsement => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.IsEndorsement);

        public WizardPrintViewModel Left4 => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.FourFlats && x.Print.LinkedPrint.Hand == Hand.Left );

        public WizardPrintViewModel Right4 => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.FourFlats && x.Print.LinkedPrint.Hand == Hand.Right);

        public WizardPrintViewModel TwoThumbs => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.TwoThumbs);

        public WizardPrintViewModel LeftThumb => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Thumb && x.Print.LinkedPrint.Hand == Hand.Left && x.Print.LinkedPrint.ScanKind == HandScanKind.Flat);

        public WizardPrintViewModel RightThumb => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Thumb && x.Print.LinkedPrint.Hand == Hand.Right && x.Print.LinkedPrint.ScanKind == HandScanKind.Flat);

        public WizardPrintViewModel RolledLeftThumb => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Thumb && x.Print.LinkedPrint.Hand == Hand.Left && x.Print.LinkedPrint.ScanKind == HandScanKind.Rolled);

        public WizardPrintViewModel RolledRightThumb => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Thumb && x.Print.LinkedPrint.Hand == Hand.Right && x.Print.LinkedPrint.ScanKind == HandScanKind.Rolled);

        public WizardPrintViewModel RolledLeftIndex => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Index && x.Print.LinkedPrint.Hand == Hand.Left && x.Print.LinkedPrint.ScanKind == HandScanKind.Rolled);

        public WizardPrintViewModel RolledRightIndex => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Index && x.Print.LinkedPrint.Hand == Hand.Right && x.Print.LinkedPrint.ScanKind == HandScanKind.Rolled);

        public WizardPrintViewModel RolledLeftMiddle => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Middle && x.Print.LinkedPrint.Hand == Hand.Left && x.Print.LinkedPrint.ScanKind == HandScanKind.Rolled);

        public WizardPrintViewModel RolledRightMiddle => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Middle && x.Print.LinkedPrint.Hand == Hand.Right && x.Print.LinkedPrint.ScanKind == HandScanKind.Rolled);

        public WizardPrintViewModel RolledLeftRing => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Ring && x.Print.LinkedPrint.Hand == Hand.Left && x.Print.LinkedPrint.ScanKind == HandScanKind.Rolled);

        public WizardPrintViewModel RolledRightRing => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Ring && x.Print.LinkedPrint.Hand == Hand.Right && x.Print.LinkedPrint.ScanKind == HandScanKind.Rolled);

        public WizardPrintViewModel RolledLeftLittle => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Little && x.Print.LinkedPrint.Hand == Hand.Left && x.Print.LinkedPrint.ScanKind == HandScanKind.Rolled);

        public WizardPrintViewModel RolledRightLittle => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Little && x.Print.LinkedPrint.Hand == Hand.Right && x.Print.LinkedPrint.ScanKind == HandScanKind.Rolled);

        public WizardPrintViewModel PalmLeftUpper => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.UpperPalm && x.Print.LinkedPrint.Hand == Hand.Left);

        public WizardPrintViewModel PalmRightUpper => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.UpperPalm && x.Print.LinkedPrint.Hand == Hand.Right);

        public WizardPrintViewModel PalmLeftLower => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.LowerPalm && x.Print.LinkedPrint.Hand == Hand.Left);

        public WizardPrintViewModel PalmRightLower => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.LowerPalm && x.Print.LinkedPrint.Hand == Hand.Right);

        public WizardPrintViewModel PalmLeftHypothenar => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Hypothenar && x.Print.LinkedPrint.Hand == Hand.Left);

        public WizardPrintViewModel PalmRightHypothenar => this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.HandPart == HandPart.Hypothenar && x.Print.LinkedPrint.Hand == Hand.Right);


        public Visibility AdvancedInformationVisibility
        {
            get
            {
                return this.advancedInformationVisibility;
            }
            set
            {
                if (value == this.advancedInformationVisibility)
                {
                    return;
                }
                this.advancedInformationVisibility = value;
                this.OnPropertyChanged(nameof(this.AdvancedInformationVisibility));
            }
        }

        public string StatusText
        {
            get { return statusText; }
            set
            {
                if (statusText == value) return;
                statusText = value;
                this.OnPropertyChanged(nameof(this.StatusText));
            }
        }

        public string StatusActionText
        {
            get { return statusActionText; }
            set
            {
                if (statusActionText == value) return;
                statusActionText = value;
                this.OnPropertyChanged(nameof(this.StatusActionText));
            }
        }

        public bool IsStatusDisplayed
        {
            get { return isStatusDisplayed; }
            set
            {
                if (isStatusDisplayed == value)
                {
                    return;
                }
                isStatusDisplayed = value;
                this.OnPropertyChanged(nameof(this.IsStatusDisplayed));
            }
        }


        public void SetDisplayedPrint(WizardPrintViewModel vm)
        {
            if (this.DisplayedPrint != null)
            {
                ComputeWizardStatus();
            }

            this.DisplayedPrint = new WizardConclusionPrintViewModel(vm, this.overrideReasonList, this.AlwaysCanOverride);
            this.OnPropertyChanged(nameof(this.DisplayedPrint));
        }


        public void DisplayNext()
        {
            this.DisplayOtherPrint(1);
        }

        public void DisplayPrevious()
        {
            this.DisplayOtherPrint(-1);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        

        public void ApplyActions()
        {
            foreach (var model in this.printViewModels)
            {
                var print = model.Print.LinkedPrint;
                print.OverrideCode = (int)(print.UserAction & WizardAction.OverrideCodeMask);                
            }
        }


       

        public void SetCurrentSegment(SegmentInfoViewModel vm)
        {
            this.DisplayedPrint.SetCurrentSegment(vm);

            // Console.WriteLine("POUET");
            //throw new NotImplementedException();
        }

        public bool IsValid()
        {
            // validate that no user reason is left blank !!
            foreach (var model in this.printViewModels)
            {                
                if (model.Print.LinkedPrint.OverrideCode > 0)
                {
                    var over = this.overrideReasonList.FirstOrDefault(x => x.Code == model.Print.LinkedPrint.OverrideCode);
                    if (over != null && over.HasUserText && string.IsNullOrEmpty(model.Print.LinkedPrint.OverrideUserReason))
                    {
                        return false;
                    }
                }

                if (model.Print.LinkedPrint.HasSegments)
                {
                    foreach (var segment in model.Print.LinkedPrint.Segments)
                    {
                        
                        if (segment.OverrideCode > 0)
                        {
                            var over = this.overrideReasonList.FirstOrDefault(x => x.Code == segment.OverrideCode);
                            if (over != null && over.HasUserText && string.IsNullOrEmpty(segment.OverrideText))
                            {
                                return false;
                            }
                        }
                    }
                }

            }

            return true;


        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DisplayOtherPrint(int direction)
        {
            int currentIndex = this.printViewModels.IndexOf(this.DisplayedPrint.BaseVm);

            var newIndex = currentIndex + direction;

            if (newIndex < 0)
            {
                newIndex = 0;
            }
            else if (newIndex >= this.printViewModels.Count)
            {
                newIndex = this.printViewModels.Count - 1;
            }

            var vm = this.printViewModels[newIndex];

            this.SetDisplayedPrint(vm);

        }

        private void DefineVisibility()
        {
            var captureGroup = this.printList.Rules.CaptureGroup;
            var showEndorsement = this.printList.Rules.IsEndorsementAllowed;

            this.AdvancedInformationVisibility = Visibility.Collapsed;
            this.TwoThumbsVisibility = captureGroup == PrintCaptureGroup.FlatOnly
                ? Visibility.Visible
                : Visibility.Collapsed;
            this.SingleThumbsVisibility = captureGroup != PrintCaptureGroup.FlatOnly
                ? Visibility.Visible
                : Visibility.Collapsed;

            this.RolledVisibility = (captureGroup == PrintCaptureGroup.Standard14 || captureGroup == PrintCaptureGroup.StandardAndPalm)
                ? Visibility.Visible
                : Visibility.Collapsed;

            this.PalmVisibility = captureGroup == PrintCaptureGroup.StandardAndPalm
                ? Visibility.Visible
                : Visibility.Collapsed;

            this.EndorsementVisibility = showEndorsement
                ? Visibility.Visible
                : Visibility.Collapsed;

            this.IsStatusDisplayed = true;
        }

        private void DefineAssignements()
        {
            // For use with cardscan

            this.SetAssignment(this.Endorsement, nameof(this.Endorsement));
            this.SetAssignment(this.Left4, nameof(this.Left4));
            this.SetAssignment(this.Right4, nameof(this.Right4));
            this.SetAssignment(this.TwoThumbs, nameof(this.TwoThumbs));
            this.SetAssignment(this.LeftThumb, nameof(this.LeftThumb));
            this.SetAssignment(this.RightThumb, nameof(this.LeftThumb));
            this.SetAssignment(this.RolledLeftThumb, nameof(this.RolledLeftThumb));
            this.SetAssignment(this.RolledRightThumb, nameof(this.RolledRightThumb));

            this.SetAssignment(this.RolledLeftIndex, nameof(this.RolledLeftIndex));
            this.SetAssignment(this.RolledRightIndex, nameof(this.RolledRightIndex));

            this.SetAssignment(this.RolledLeftMiddle, nameof(this.RolledLeftMiddle));
            this.SetAssignment(this.RolledRightMiddle, nameof(this.RolledRightMiddle));

            this.SetAssignment(this.RolledLeftRing, nameof(this.RolledLeftRing));
            this.SetAssignment(this.RolledRightRing, nameof(this.RolledRightRing));

            this.SetAssignment(this.RolledLeftLittle, nameof(this.RolledLeftLittle));
            this.SetAssignment(this.RolledRightLittle, nameof(this.RolledRightLittle));

            this.SetAssignment(this.PalmLeftUpper, nameof(this.PalmLeftUpper));
            this.SetAssignment(this.PalmRightUpper, nameof(this.PalmRightUpper));

            this.SetAssignment(this.PalmLeftLower, nameof(this.PalmLeftLower));
            this.SetAssignment(this.PalmRightLower, nameof(this.PalmRightLower));

            this.SetAssignment(this.PalmLeftHypothenar, nameof(this.PalmLeftHypothenar));
            this.SetAssignment(this.PalmRightHypothenar, nameof(this.PalmRightHypothenar));
        }

        private void ComputeWizardStatus()
        {
            var toScanList = new List<PrintInfo>();
            foreach (var model in this.printViewModels)
            {
                var print = model.Print.LinkedPrint;
                if (print.UserAction == WizardAction.Undefined)
                {
                    print.UserAction = print.IsInError ? WizardAction.ScanAgain : WizardAction.Accept;
                    if (print.Status == PrintStatus.Empty)
                    {
                        print.UserAction = WizardAction.ScanAgain;
                    }
                }
                if (print.UserAction == WizardAction.ScanAgain)
                {
                    toScanList.Add(print);
                }
                this.ComputeWizardStatus(model);
            }

            if (toScanList.Count > 0)
            {
                var separator = Environment.NewLine + " - ";
                this.StatusText = string.Empty; //Language.Text.WizardStatusErrorList;
                this.StatusActionText = Language.Text.WizardStatusCaptureAgain + separator +
                                        string.Join(separator,
                                            toScanList.Select(PrintList.GetShortName));

            }
            else
            {
                this.StatusText = Language.Text.WizardStatusNoError;
                this.StatusActionText = Language.Text.WizardStatusEndCapture;
            }

        }

        private void ComputeWizardStatus(WizardPrintViewModel model)
        {
            var print = model.Print.LinkedPrint;

            if (print.UserAction == WizardAction.ScanAgain)
            {
                model.Print.WizardStatus = Text.ScanAgain;
            }

            else if (!print.IsInError)
            {
                model.Print.WizardStatus = Text.Accept;
            }

            else
            {
                if ((print.UserAction & WizardAction.OverrideCodeMask) > 0)
                {
                    model.Print.WizardStatus = Text.AcceptStar;
                }
            }

            model.SetStyle();
        }

        private void PrintModifiedCallback(object sender, PrintModifiedEventArgs e)
        {
            var key = e.Info.Key;            
            
            if (this.vmAssignations.ContainsKey(key))
            {
                var pr = this.printViewModels.FirstOrDefault(x => x.Print.LinkedPrint.Key == key);                

                var vm = this.printList.GetViewModel(e.Info);                               

                this.vmAssignations[key](pr, vm);
            }
        }

        private void SetAssignment(WizardPrintViewModel vm, string propertyName)
        {
            if (vm == null) return;
            this.vmAssignations.Add(vm.Print.LinkedPrint.Key, (wizModel, printModel) => this.SetVm(wizModel, printModel, propertyName));
        }

        private void SetVm(WizardPrintViewModel wizModel, PrintElementViewModel model, string propertyName)
        {
            var key = model.LinkedPrint.Key;
            if (!this.vmAssignations.ContainsKey(key))
            {
                return;
            }

            wizModel.Update(model);            

            this.OnPropertyChanged(propertyName);
        }

    }
}
