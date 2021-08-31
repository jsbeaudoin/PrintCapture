using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media;
using PrintsCapture.Livescan.Properties;
using PrintsCapture.Prints.Enum;
using PrintsCapture.Prints.Extension;
using PrintsCapture.Prints.Language;

namespace PrintsCapture.Livescan.ViewModel
{
    using System.ComponentModel;
    using System.Windows;

    using Device;
    using Device.Enum;
    using Prints.ViewModel;

    using Image = System.Windows.Controls.Image;

    public class WizardProcessViewModel : INotifyPropertyChanged
    {                
        public enum HandImage
        {
            None = 0,
            LeftHand = 1,
            LeftThumb = 2,
            RightHand = 3,
            RightThumb = 4,
            TwoThumbs = 5
        }

        private Dictionary<HandImage, Bitmap> HandImagesDictonary { get; set; }

        private string instructions;

        private Visibility endorsementVisibility;       

        private Visibility win32Visibility;

        private string deviceMessage;

        private string deviceInformation;

        private Visibility deviceMessageVisibility;

        private Visibility printCaptureVisibility;

        private Visibility resumeButtonVisibility;

        private string deviceStateLabel;

        private Visibility restartButtonVisibility;
        private Visibility printAndMessageVisibility;
        private Visibility fingerStatusVisibility;        
        private bool hasEnded;
        private string deviceName;
        private ImageSource _instructionHandImage;


        public WizardProcessViewModel()
        {            
            this.deviceMessageVisibility = Visibility.Hidden;
            this.restartButtonVisibility = Visibility.Collapsed;
            // IMPORTANT : Before app starts, it waits for the handle.
            // No handle is created if the visibility is set to Hidden !!
            this.printCaptureVisibility = Visibility.Visible;

            this.FingerStatusVisibility = Visibility.Hidden;
            this.PrintAndMessageVisibility = Visibility.Visible;

            this.Messages = new ObservableCollection<string>();     

            //Loading HandImageDictionay
            this.HandImagesDictonary = new Dictionary<HandImage, Bitmap>
            {
                {HandImage.None, new Bitmap(CommonText.HandModel)},
                {HandImage.LeftHand, new Bitmap(CommonText.CaptureLeftFlatHand)},
                {HandImage.LeftThumb, new Bitmap(CommonText.CaptureLeftFlatThumb)},
                {HandImage.RightHand, new Bitmap(CommonText.CaptureRightFlatHand)},
                {HandImage.RightThumb, new Bitmap(CommonText.CaptureRightFlatThumb)},
                {HandImage.TwoThumbs, new Bitmap(CommonText.CaptureFlatTwoThumb)}
            };
        }

        public ImageSource GetHandImage(Prints.PrintInfo printInfo)
        {
            Bitmap bmp;
            HandImage key = HandImage.None;

            if (printInfo.Hand == Hand.Left)
            {
                if (printInfo.HandPart == HandPart.FourFlats)
                {
                    key = HandImage.LeftHand;
                }
                else if (printInfo.HandPart == HandPart.Thumb)
                {
                    key = HandImage.LeftThumb;
                }
                else
                {
                    key = HandImage.None;
                }
            }
            else if (printInfo.Hand == Hand.Right)
            {
                if (printInfo.HandPart == HandPart.FourFlats)
                {
                    key = HandImage.RightHand;
                }
                else if (printInfo.HandPart == HandPart.Thumb)
                {
                    key = HandImage.RightThumb;
                }
                else
                {
                    key = HandImage.None;
                }
            }
            else if (printInfo.Hand == Hand.None)
            {
                if (printInfo.HandPart == HandPart.TwoThumbs)
                {
                    key = HandImage.TwoThumbs;
                }
                else if(printInfo.IsEndorsement)
                {
                    if (printInfo.EndorsementFinger.Hand == Hand.Left)
                    {
                        key = HandImage.LeftThumb;
                    }
                    else if (printInfo.EndorsementFinger.Hand == Hand.Right)
                    {
                        key = HandImage.RightThumb;
                    }
                    else
                    {
                        key = HandImage.None;
                    }
                }
                else
                {
                    key = HandImage.None;
                }
            }

            if (this.HandImagesDictonary.TryGetValue(key, out bmp))
            {
                return bmp.ToImageSource();
            }

            return null;
        }

        public ImageSource GetHandImageEmpty()
        {
            var key = HandImage.None;
            Bitmap bmp;

            if (this.HandImagesDictonary.TryGetValue(key, out bmp))
            {
                return bmp.ToImageSource();
            }

            return null;
        }


        public ImageSource InstructionHandImage
        {
            get => _instructionHandImage;
            set
            {
                _instructionHandImage = value;
                this.OnPropertyChanged(@"InstructionHandImage");
            }
        }

        public EventHandler ShowWindow;

        public EventHandler HideWindow;

        private bool topMostWindow;

        public bool? AcceptPrint { get; set; }        

        public ObservableCollection<string> Messages { get; private set; }

        public bool IsLoginMode { get; set; }

        public string DeviceName
        {
            get
            {
                return this.deviceName;
            }
            set
            {
                if (value == this.deviceName)
                {
                    return;
                }
                this.deviceName = value;
                this.OnPropertyChanged(@"DeviceName");
            }
        }

        public string DeviceInformation
        {
            get
            {
                return this.deviceInformation;
            }
            set
            {
                if (value == this.deviceInformation)
                {
                    return;
                }
                this.deviceInformation = value;
                this.OnPropertyChanged(@"DeviceInformation");
            }
        }

        public string DeviceMessage
        {
            get { return this.deviceMessage; }
            set
            {
                if (value == this.deviceMessage)
                {
                    return;
                }
                this.deviceMessage = value;
                this.OnPropertyChanged(@"DeviceMessage");

                this.Messages.Add( DateTime.Now.ToString("HH:mm:ss") + " " + value  );

                
                this.DeviceMessageVisibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
                
            }
        }

        private bool alwaysCanOverride;

        public bool AlwaysCanOverride
        {
            get { return this.alwaysCanOverride; }
            set
            {
                if(value == this.alwaysCanOverride) return;
                this.alwaysCanOverride = value;
                this.OnPropertyChanged(@"AlwaysCanOverride");
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
                if(value == this.topMostWindow)return;

                this.topMostWindow = value;
                this.OnPropertyChanged(@"TopMost");
            }
        }

        public string Instructions
        {
            get
            {
                return this.instructions;
            }
            set
            {
                if (value == this.instructions)
                {
                    return;
                }
                this.instructions = value;
                this.OnPropertyChanged(@"Instructions");
            }
        }

        
        public Image DisplayImage { get; set; }

        public Visibility Win32Visibility
        {
            get
            {
                return this.win32Visibility;
            }
            set
            {
                if (value == this.win32Visibility)
                {
                    return;
                }
                this.win32Visibility = value;
                this.OnPropertyChanged(@"Win32Visibility");
            }
        }

        public Visibility EndorsementVisibility
        {
            get
            {
                return this.endorsementVisibility;
            }
            set
            {
                if (value == this.endorsementVisibility)
                {
                    return;
                }
                this.endorsementVisibility = value;
                this.OnPropertyChanged(@"EndorsementVisibility");
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

        public Visibility PrintCaptureVisibility
        {
            get
            {
                return this.printCaptureVisibility;
            }
            set
            {
                if (value == this.printCaptureVisibility)
                {
                    return;
                }
                this.printCaptureVisibility = value;
                this.OnPropertyChanged(@"PrintCaptureVisibility");
            }
        }

        public Visibility PrintAndMessageVisibility
        {
            get { return printAndMessageVisibility; }
            set
            {
                if (Equals(printAndMessageVisibility, value))
                {
                    return;
                }
                printAndMessageVisibility = value;
                this.OnPropertyChanged(@"PrintAndMessageVisibility");
            }
        }

        public Visibility FingerStatusVisibility
        {
            get { return fingerStatusVisibility; }
            set
            {
                if (Equals(fingerStatusVisibility, value))
                {
                    return;
                }

                fingerStatusVisibility = value;
                this.OnPropertyChanged(@"FingerStatusVisibility");
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

                if (value == Visibility.Visible)
                {
                    this.DeviceMessageVisibility = Visibility.Visible;
                }
            }
        }

        public Visibility RestartButtonVisibility
        {
            get
            {
                return this.restartButtonVisibility;
            }
            set
            {
                if (this.restartButtonVisibility == value)
                {
                    return;
                }
                this.restartButtonVisibility = value;
                this.OnPropertyChanged(@"RestartButtonVisibility");
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
                if (value == this.deviceStateLabel)
                {
                    return;
                }
                this.deviceStateLabel = value;
                this.OnPropertyChanged(@"DeviceStateLabel");
            }
        }

        public string AppVersionLabel { get; set; }        

        public void ResetQuality(PrintQualityLevel quality = PrintQualityLevel.Unknown)
        {
            
        }

        public void QualityChanged(IEnumerable<PrintCaptureQuality> qualityList)
        {
            
        }

        public void UpdateVm(PrintElementViewModel vm)
        {            

            // Vm are no more displayed, do nothing !
        }

        public void TriggerResumeCapture()
        {
            this.DeviceMessage = string.Empty;
            this.DeviceMessageVisibility = Visibility.Collapsed;

            if (hasEnded)
            {
                this.hasEnded = false;
                this.CaptureRequired?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                this.CaptureResumed?.Invoke(this, EventArgs.Empty);
            }
            
        }

        
        public void TriggerCaptureStart()
        {
            this.CaptureRequired?.Invoke(this, EventArgs.Empty);
        }       

        public void TriggerCloseWindowRequired()
        {
            this.CloseWindowRequired?.Invoke(this, EventArgs.Empty);
        }       

        public event EventHandler CaptureRequired;        

        public event EventHandler CaptureResumed;

        public event EventHandler CloseWindowRequired;

        public event EventHandler ConfigurationRequired;

        public event EventHandler ConfigurationCompleted;


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void CaptureCompleted()
        {            
            this.hasEnded = true;            
            this.AcceptPrint = true;
            this.TriggerCloseWindowRequired();                        
        }        

        public void TriggerHideWindow()
        {
            this.HideWindow?.Invoke(this, EventArgs.Empty);
        }

        public void TriggerShowWindow()
        {
            this.ShowWindow?.Invoke(this, EventArgs.Empty);
        }

        public void TriggerConfigureDevice()
        {
            this.ConfigurationRequired?.Invoke(this, EventArgs.Empty);
        }

        public void TriggerConfigurationCompleted()
        {
            this.ConfigurationCompleted?.Invoke(this, EventArgs.Empty);
        }        
    }
}
