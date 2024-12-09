namespace PrintsCapture.Prints.ViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using PrintsCapture.Prints.Enum;

    public class PrintElementViewModel : INotifyPropertyChanged
    {
        private PrintInfo linkedPrint;

        private ImageSource image;

        private string name;

        private BitmapImage statusImage;

        private string missingText;

        private bool isMissing;

        private bool isReady;

        private string statusMessage;

        private int sequenceScore;

        private int qualityScore;

        private bool sequenceAnalyzed;

        private string endorsementFingerName;

        private int minutiaCount;

        private string wizardStatus;

        public PrintElementViewModel(PrintInfo printInfo)
        {
            this.LinkedPrint = printInfo;
        }

        public PrintInfo LinkedPrint
        {
            private set
            {
                this.linkedPrint = value;
                this.OnPropertyChanged("LinkedPrint");
            }
            get
            {
                return this.linkedPrint;
                
            }
        }

        public ImageSource Image
        {
            get
            {
                return this.image;
            }
            set
            {
                if (value != null)
                {
                    if (!value.IsFrozen)
                    {
                        value.Freeze();
                    }
                }
                this.image = value;

                this.OnPropertyChanged("Image");
            }
        }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string WizardStatus
        {
            get
            {
                return this.wizardStatus;
            }
            set
            {
                if (value == this.wizardStatus)
                {
                    return;
                }
                this.wizardStatus = value;
                this.OnPropertyChanged(nameof(this.WizardStatus));
            }
        }

        public BitmapImage StatusImage
        {
            get
            {
                return this.statusImage;
            }
            set
            {
                this.statusImage = value;
                this.OnPropertyChanged(nameof(this.StatusImage));
            }
        }

        public string StatusMessage
        {
            get
            {
                return this.statusMessage;
            }
            set
            {
                if (this.statusMessage == value)
                {
                    return;
                }
                this.statusMessage = value;
                this.OnPropertyChanged(nameof(this.StatusMessage));
            }
        }

        public string MissingText
        {
            get
            {
                return this.missingText;
            }
            set
            {
                this.missingText = value;
                this.OnPropertyChanged(nameof(this.MissingText));

                this.IsMissing = !string.IsNullOrEmpty(value);
            }
        }

        public bool IsMissing
        {
            get
            {
                return this.isMissing;
            }
            private set
            {
                this.isMissing = value;
                this.OnPropertyChanged(nameof(this.IsMissing));
            }
        }

        public bool IsReady
        {
            get
            {
                return this.isReady;
            }
            set
            {
                this.isReady = value;
                this.OnPropertyChanged(nameof(this.IsReady));
            }
        }

        /// <summary>
        /// Determine if underlying print will be captured and validated.
        /// When false, it means the print is not in the capture group.
        /// </summary>
        public bool IsUsed { get; set; }

        public int SequenceScore
        {
            get
            {
                return this.sequenceScore;
            }
            set
            {
                if (Equals(value, this.sequenceScore))
                {
                    return;
                }
                this.sequenceScore = value;
                this.OnPropertyChanged(nameof(this.SequenceScore));
            }
        }

        public int MinutiaCount
        {
            get
            {
                return this.minutiaCount;
            }
            set
            {
                if (this.minutiaCount == value)
                {
                    return;
                }
                this.minutiaCount = value;
                this.OnPropertyChanged(nameof(this.MinutiaCount));
            }
        }

        public bool SequenceAnalyzed
        {
            get
            {
                return this.sequenceAnalyzed;
            }
            set
            {
                if (Equals(value, this.sequenceAnalyzed))
                {
                    return;
                }
                this.sequenceAnalyzed = value;
                this.OnPropertyChanged(nameof(this.SequenceAnalyzed));
            }
        }

        public int QualityScore
        {
            get
            {
                return this.qualityScore;
            }
            set
            {
                if (Equals(value, this.qualityScore))
                {
                    return;
                }
                this.qualityScore = value;
                this.OnPropertyChanged(nameof(this.QualityScore));
            }
        }

        public int Resolution
        {
            get
            {
                return this.linkedPrint.Resolution.ToDpi();
            }
        }

        public string EndorsementFingerName
        {
            get
            {
                return this.endorsementFingerName;
            }
            set
            {
                if (value == this.endorsementFingerName)
                {
                    return;
                }
                this.endorsementFingerName = value;
                this.OnPropertyChanged(nameof(this.EndorsementFingerName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
