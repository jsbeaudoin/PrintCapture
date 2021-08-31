using System.ComponentModel;
using System.Windows;
using PrintsCapture.Livescan.Properties;
using PrintsCapture.Prints;
using PrintsCapture.Prints.Enum;

namespace PrintsCapture.Ui.ViewModel.Wizard
{
    public class WizardOptionViewModel : INotifyPropertyChanged
    {              
        private bool isFirstStep;

        private bool topMostWindow;

        public WizardOptionViewModel(PrintList printList)
        {
            this.CardScanOptionVisibility = Visibility.Collapsed;
            this.PrintList = printList;
            var availableCaptureMode = PrintList.Rules.CaptureGroupAllowed;            

            this.IsFlatAvailable = availableCaptureMode.HasFlag(PrintCaptureGroup.FlatOnly);
            this.IsRolledAvailable = availableCaptureMode.HasFlag(PrintCaptureGroup.Standard14);
            this.IsPalmAvailable = availableCaptureMode.HasFlag(PrintCaptureGroup.StandardAndPalm);
            this.IsOneFingerAvailable = availableCaptureMode.HasFlag(PrintCaptureGroup.OneFingerOnly);

            var availableCount = (this.IsFlatAvailable ? 1 : 0) + (this.IsRolledAvailable ? 1 : 0) +
                                 (this.IsPalmAvailable ? 1 : 0) + (this.IsOneFingerAvailable ? 1 : 0);

            this.CaptureModeVisibility = availableCount > 1 ? Visibility.Visible : Visibility.Collapsed;


            this.CaptureGroup = this.IsFlatAvailable ? PrintCaptureGroup.FlatOnly : PrintCaptureGroup.StandardAndPalm;
            this.IsFirstStep = true;
        }

        public PrintList PrintList { get; }

        public PrintCaptureGroup CaptureGroup { get; private set; }

        public bool IsFlatAvailable { get; }

        public bool IsRolledAvailable { get; }

        public bool IsPalmAvailable { get; }

        public bool IsOneFingerAvailable { get; }

        public Visibility CaptureModeVisibility { get; }

        public Visibility CardScanOptionVisibility { get; }

        public bool IsCapture14
        {
            get { return this.CaptureGroup == PrintCaptureGroup.Standard14; }
            set { if (value) this.ChangeCaptureMode(PrintCaptureGroup.Standard14); }
        }

        public bool IsCapturePalm
        {            
            get { return this.CaptureGroup == PrintCaptureGroup.StandardAndPalm; }
            set { if (value) this.ChangeCaptureMode(PrintCaptureGroup.StandardAndPalm); }
        }

        public bool IsCaptureFlat
        {
            get { return this.CaptureGroup == PrintCaptureGroup.FlatOnly; }
            set { if (value) this.ChangeCaptureMode(PrintCaptureGroup.FlatOnly); }
        }

        public bool IsCaptureOneFinger
        {
            get { return this.CaptureGroup == PrintCaptureGroup.OneFingerOnly; }
            set { if (value) this.ChangeCaptureMode(PrintCaptureGroup.OneFingerOnly); }
        }

        public bool TopMostWindow
        {
            get
            {
                return this.topMostWindow;
            } 
            set
            {
                if (value == this.topMostWindow)
                {
                    return;
                }
                this.topMostWindow = value;
                this.OnPropertyChanged(nameof(this.TopMostWindow));
            }
        }

        public Visibility InstructionVisibility { get; private set; }

        public Visibility MissingFingersVisibility { get; private set; }        
        
        public bool IsFirstStep
        {
            get { return this.isFirstStep; }
            set
            {
                this.isFirstStep = value;
                this.InstructionVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                this.MissingFingersVisibility = value ? Visibility.Collapsed : Visibility.Visible;                

                this.OnPropertyChanged(nameof(this.InstructionVisibility));
                this.OnPropertyChanged(nameof(this.MissingFingersVisibility));                
            }
        }        


        public event PropertyChangedEventHandler PropertyChanged;



        private void ChangeCaptureMode(PrintCaptureGroup newGroup)
        {
            this.CaptureGroup = newGroup;
            this.OnPropertyChanged(nameof(this.IsCapture14));
            this.OnPropertyChanged(nameof(this.IsCapturePalm));
            this.OnPropertyChanged(nameof(this.IsCaptureFlat));
            this.OnPropertyChanged(nameof(this.IsCaptureOneFinger));
        }                


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
