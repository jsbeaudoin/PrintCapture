// -----------------------------------------------------------------------
// <copyright file="LivePreviewViewModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace PrintsCapture.Livescan.ViewModel
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media.Imaging;

    using PrintsCapture.Prints.ViewModel;

    /// <summary>
    /// Live Privew ViewModel
    /// </summary>
    public class LivePreviewViewModel : INotifyPropertyChanged
    {
        // private PrintElement currentPrint;

        private PrintElementViewModel previousPrint;

        private int thresholdSequenceCheck;

        private int thresholdQuality;

        private BitmapSource leftHandImage;

        private BitmapSource scanKindImage;

        private BitmapSource rightHandImage;

        private string scanInstructionText;

        private string deviceMessage;

        private bool captureStopped;

        private Visibility deviceMessageVisibility;

        private Visibility resumeButtonVisibility;

        private Visibility printCaptureVisibility;

        private string deviceStateLabel;

        private bool topMostWindow;

        public LivePreviewViewModel()
        {
            this.deviceMessageVisibility = Visibility.Collapsed;
            this.ResumeButtonVisibility = Visibility.Collapsed;
            this.PrintCaptureVisibility = Visibility.Visible;            
        }

        public event EventHandler Resume;

        public event EventHandler ScanPreviousPrint;

        public event EventHandler Win32HandleCreated;

        public int ThresholdSequenceCheck
        {
            get
            {
                return this.thresholdSequenceCheck;
            }
            set
            {
                if (value == this.thresholdSequenceCheck)
                {
                    return;
                }
                this.thresholdSequenceCheck = value;
                this.OnPropertyChanged(@"ThresholdSequenceCheck");
            }
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
                this.OnPropertyChanged(@"TopMostWindow");
            }
        }

        public int ThresholdQuality
        {
            get
            {
                return this.thresholdQuality;
            }
            set
            {
                if (value == this.thresholdQuality)
                {
                    return;
                }
                this.thresholdQuality = value;
                this.OnPropertyChanged(@"ThresholdQuality");
            }
        }

        public string DeviceMessage
        {
            get
            {
                return this.deviceMessage;
            }
            set
            {
                if (value == this.deviceMessage)
                {
                    return;
                }
                this.deviceMessage = value;
                this.OnPropertyChanged(@"DeviceMessage");

                this.DeviceMessageVisibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
                var newValue = string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Hidden;
                this.PrintCaptureVisibility = newValue;
                Console.WriteLine(@"{2} - MSG: {0}, Cap:{1}/{3}", this.DeviceMessageVisibility, this.PrintCaptureVisibility, DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss"), newValue);
            }
        }

        public Visibility DeviceMessageVisibility
        {
            get
            {
                return this.deviceMessageVisibility;
            }
            private set
            {
                if (value == this.deviceMessageVisibility)
                {
                    return;
                }
                this.deviceMessageVisibility = value;
                this.OnPropertyChanged(@"DeviceMessageVisibility");                
            }
        }        

        public Visibility ResumeButtonVisibility
        {
            get
            {
                return this.resumeButtonVisibility;
            }
            set
            {
                if (value == this.resumeButtonVisibility)
                {
                    return;
                }
                this.resumeButtonVisibility = value;
                this.OnPropertyChanged(@"ResumeButtonVisibility");
            }
        }

        public Visibility PrintCaptureVisibility
        {
            get
            {
                return this.printCaptureVisibility;
            }
            private set
            {
                if (value == this.printCaptureVisibility)
                {
                    return;
                }
                this.printCaptureVisibility = value;
                this.OnPropertyChanged(@"PrintCaptureVisibility");
            }
        }

        public string ScanInstructionText
        {
            get
            {
                return this.scanInstructionText;
            }
            set
            {
                if (value == this.scanInstructionText)
                {
                    return;
                }
                this.scanInstructionText = value;
                this.OnPropertyChanged(@"ScanInstructionText");
            }
        }

        public string DeviceStateLabel
        {
            get
            {
                return this.deviceStateLabel;
            }
            set
            {
                this.deviceStateLabel = value;
                this.OnPropertyChanged(@"DeviceStateLabel");
            }
        }

        public BitmapSource LeftHandImage
        {
            get
            {
                return this.leftHandImage;
            }
            set
            {
                if (Equals(value, this.leftHandImage))
                {
                    return;
                }
                this.leftHandImage = value;
                this.OnPropertyChanged(@"LeftHandImage");
            }
        }

        public BitmapSource RightHandImage
        {
            get
            {
                return this.rightHandImage;
            }
            set
            {
                if (Equals(value, this.rightHandImage))
                {
                    return;
                }
                this.rightHandImage = value;
                this.OnPropertyChanged(@"RightHandImage");
            }
        }

        public BitmapSource ScanKindImage
        {
            get
            {
                return this.scanKindImage;
            }
            set
            {
                if (Equals(value, this.scanKindImage))
                {
                    return;
                }
                this.scanKindImage = value;
                this.OnPropertyChanged(@"ScanKindImage");
            }
        }
        

        public PrintElementViewModel PreviousPrint
        {
            get
            {
                return this.previousPrint;
            }
            set
            {
                if (Equals(value, this.previousPrint))
                {
                    return;
                }
                this.previousPrint = value;
                this.OnPropertyChanged(@"PreviousPrint");
            }
        }

        public bool CaptureStopped
        {
            get
            {
                return this.captureStopped;
            }
            set
            {
                if (value.Equals(this.captureStopped))
                {
                    return;
                }
                this.captureStopped = value;
                this.OnPropertyChanged(@"CaptureStopped");

                //this.ResumeButtonVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                this.PrintCaptureVisibility = value ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool KeepWindowOpen { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnScanPreviousPrint()
        {
            var handler = this.ScanPreviousPrint;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void OnResume()
        {
            var handler = this.Resume;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void OnWin32HandleCreated()
        {
            var handler = this.Win32HandleCreated;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

      
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }        
    }
}
