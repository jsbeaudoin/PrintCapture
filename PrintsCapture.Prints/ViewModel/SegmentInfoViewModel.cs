using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PrintsCapture.Prints.Enum;
using PrintsCapture.Prints.Language;

namespace PrintsCapture.Prints.ViewModel
{
    public class SegmentInfoViewModel :INotifyPropertyChanged
    {
        private string overrideText;
        private int overrideCode;
        private ImageSource qualityIndicator;
        private bool isOverriden;
        private int referenceHeight;
        private Thickness margin;
        private double height;
        private double width;
        private string overrideLabel;

        private static ImageSource QualityOkImage;
        private static ImageSource QualityErrorImage;
        private static ImageSource QualityOverrideImage;
        private static ImageSource QualityMissingImage;
        private PrintSegment segment;
        private string statusText;
        private bool isExpected;
        private bool isRectangleShown;
        private WizardAction userAction;
        private Visibility otherReasonVisibility;

        private static void LoadImages()
        {
            if (QualityOkImage != null)
            {
                return;
            }

            QualityOkImage = new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/GreenButton.png"));
            QualityOverrideImage = new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/StatusModified.png"));
            QualityErrorImage = new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/StatusError.png"));
            QualityMissingImage = new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/StatusMIssing.png"));
        }

        public SegmentInfoViewModel(PrintInfo masterPrint, PrintSegment segment)
        {            

            LoadImages();
            Print = masterPrint;
            if (masterPrint.Image != null)
            {
                referenceHeight = masterPrint.Image.Height;
            }
            
            this.segment = segment;
            this.Name = PrintList.GetName(segment.Part);
            this.Part = segment.Part;

            this.Rect = this.segment.Position;
            this.OverrideCode = this.segment.OverrideCode;
            this.OverrideText = this.segment.OverrideText;
            this.Quality = this.segment.QualityScore;
            this.MinutiaCount = this.segment.MinutiaCount;
            this.Sequence = this.segment.SelfScore.Score;

            this.IsMissing = !string.IsNullOrEmpty(this.segment.MissingCode);
            this.IsExpected = segment.IsExpected;

            this.UserAction = !this.isOverriden ?
                WizardAction.Accept :
                (WizardAction)this.overrideCode;           

            this.OtherReasonVisibility = Visibility.Collapsed;

            this.SetStatus();
        }

        private PrintInfo Print { get; set; }

        public string Name { get; private set; }
        public PhysicalHandPart Part { get; set; }

        public int Index { get; private set; }

        public WizardAction UserAction
        {
            get { return userAction; }
            set
            {
                if (value == this.userAction) return;
                userAction = value;               
                
                this.OnPropertyChanged(nameof(this.userAction));                
            }
        }

        public Visibility OtherReasonVisibility
        {
            get { return otherReasonVisibility; }
            set
            {
                if (this.otherReasonVisibility == value) return;
                otherReasonVisibility = value; 
                this.OnPropertyChanged(nameof(this.OtherReasonVisibility));
            }
        }

        public Rectangle Rect { get; private set; }

        public Thickness Margin
        {
            get { return margin; }
            private set
            {
                if (this.margin == value)
                {
                    return;
                }
                margin = value;
                this.OnPropertyChanged("Margin");
            }
        }

        public double Height
        {
            get { return height; }
            private set
            {
                if (Math.Abs(this.height - value) < 0.01)
                {
                    return;
                }
                height = value;
                this.OnPropertyChanged("Height");
            }
        }

        public double Width
        {
            get { return width; }
            private set
            {
                if (Math.Abs(this.width - value) < 0.01)
                {
                    return;
                }
                width = value;
                this.OnPropertyChanged("Width");
            }
        }

        public bool IsOverriden
        {
            get { return isOverriden; }
            set
            {
                if (this.isOverriden == value)
                {
                    return;
                }
                isOverriden = value;
                this.OnPropertyChanged("IsOverriden");
            }
        }

        public string OverrideLabel
        {
            get { return overrideLabel; }
            set
            {
                if (value == this.overrideLabel)
                {
                    return;
                }
                overrideLabel = value;
                this.OnPropertyChanged("OverrideLabel");
            }
        }

        public int OverrideCode
        {
            get { return overrideCode; }
            set
            {
                if (value == this.overrideCode)
                {
                    return;
                }
                overrideCode = value;
                this.IsOverriden = value > 0;
                this.Print.Segments.First(x => x.Part == this.Part).OverrideCode = this.IsOverriden ? value : 0;
                this.OnPropertyChanged("OverrideCode");
                this.SetStatus();
            }
        }

        public string OverrideText
        {
            get { return overrideText; }
            set
            {
                if (value == this.OverrideText)
                {
                    return;
                }
                overrideText = value;
                this.Print.Segments.First(x => x.Part == this.Part).OverrideText = value;
                this.OnPropertyChanged("OverrideText");
            }
        }

        public string StatusText
        {
            get { return statusText; }
            set
            {
                if (value == statusText)
                {
                    return;
                }
                statusText = value;
                this.OnPropertyChanged("StatusText");
            }
        }

        public int Quality { get; set; }

        public int MinutiaCount { get; set; }

        public int Sequence { get; set; }

        public ImageSource QualityIndicator
        {
            get { return qualityIndicator; }
            set
            {
                if (value == this.qualityIndicator)
                {
                    return;
                }
                qualityIndicator = value;
                this.OnPropertyChanged("QualityIndicator");
            }
        }

        public bool IsMissing { get; private set; }

        public bool IsExpected
        {
            get { return isExpected; }
            set
            {
                if (value == isExpected)
                {
                    return;
                }
                isExpected = value;
                this.OnPropertyChanged("IsExpected");

                this.IsRectangleShown = this.IsExpected && !this.IsMissing && !this.Rect.IsEmpty;
                this.SetStatus();
            }
        }

        public bool IsRectangleShown
        {
            get { return isRectangleShown; }
            private set
            {
                if (value == this.isRectangleShown)
                {
                    return;
                }
                isRectangleShown = value;
                this.OnPropertyChanged("IsRectangleShown");
            }
        }

        public void Scale(Border container)
        {
            var ratio = container.ActualHeight/this.referenceHeight;
            var top = this.Rect.Y*ratio;
            var left = this.Rect.X*ratio;

            this.Margin = new Thickness(left, top, 0 ,0);
            this.Height = this.Rect.Height*ratio;
            this.Width = this.Rect.Width*ratio;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }       

        private void SetStatus()
        {            
            var rules = this.Print.PrintList.Rules;

            if (this.IsMissing)
            {
                this.QualityIndicator = QualityMissingImage;
                this.StatusText = CommonText.PartQualityMissing;
            }
            else if (!this.IsExpected)
            {
                this.QualityIndicator = QualityMissingImage;
                this.StatusText = CommonText.PartQualityNotExpected;
            }            
            else if (this.Rect.IsEmpty)
            {
                this.QualityIndicator = QualityErrorImage;
                this.StatusText = CommonText.PartQualityNotFound;
                
            }
            else if (this.overrideCode != 0)
            {
                this.QualityIndicator = QualityOverrideImage;
                this.StatusText = CommonText.PartQualityOverriden;
            }
            else if (this.Sequence < rules.SequenceThreshold)
            {
                this.QualityIndicator = rules.IsSequenceEnabled ? QualityErrorImage : QualityOkImage;
                this.StatusText = CommonText.SequenceBad;
            }
            else if (this.Quality > rules.QualityThreshold || this.Quality == 0)
            {                
                this.QualityIndicator = rules.IsQualityEnabled ? QualityErrorImage : QualityOkImage;
                this.StatusText = CommonText.PartQualityLow;
            }
            else
            {
                this.QualityIndicator = QualityOkImage;
                this.StatusText = CommonText.PartQualityOk;
            }
        }
    }
}
