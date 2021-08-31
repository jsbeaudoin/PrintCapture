namespace PrintsCapture.Ui.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using PrintsCapture.Prints.ViewModel;
    using PrintsCapture.Ui.Language;

    public class MainViewModel : INotifyPropertyChanged
    {        
        private string scannerName;

        private PrintGridViewModel printGridViewModel;

        private string idLine1;

        private string idLine2;

        private string scannerInfo;

        private bool inCaptureMode;

        private string userInstructionLabel;

        private string serviceMessage;                

        private bool serviceConnected;

        private BitmapImage serviceConnectedImage;

        private BitmapImage serviceDisconnectedImage;

        private BitmapImage serviceStatusImage;

        private List<EndorsableFingerViewModel> endorsableFingers;

        private bool isEndButtonAvailable;

        private ImageSource scannerImage;

        public MainViewModel()
        {            
            this.UserInstructionLabel = Text.CaptureStartInstruction;
            //                                                    pack://application:,,,/PrintsCapture.Ui;component/Images/
            this.serviceConnectedImage = new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/ServiceConnected16x16.png"));
            this.serviceDisconnectedImage = new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/ServiceDisconnected16x16.png"));
            this.ServiceStatusImage = this.serviceDisconnectedImage;
            this.ServiceMessage = Text.ServiceDisconnectedMessage;
        }

        public List<EndorsableFingerViewModel> EndorsableFingers
        {
            get
            {
                return this.endorsableFingers;
            }
            set
            {
                if (Equals(value, this.endorsableFingers))
                {
                    return;
                }
                this.endorsableFingers = value;
                this.OnPropertyChanged("EndorsableFingers");                
            }
        }        

        public PrintGridViewModel PrintGridViewModel
        {
            get
            {
                return this.printGridViewModel;
            }
            set
            {
                if (Equals(value, this.printGridViewModel))
                {
                    return;
                }

                this.printGridViewModel = value;
                this.OnPropertyChanged("PrintGridViewModel");
            }
        }

        public RulesViewModel Rules { get; set; }

        public string ScannerInfo
        {
            get
            {
                return this.scannerInfo;
            }
            set
            {
                if (Equals(value, this.scannerInfo))
                {
                    return;
                }
                this.scannerInfo = value;
                this.OnPropertyChanged("ScannerInfo");
            }
        }

        public string ScannerName
        {
            get
            {
                return this.scannerName;
            }
            set
            {
                if (Equals(this.scannerName, value))
                {
                    return;
                }

                this.scannerName = value;
                this.OnPropertyChanged("ScannerName");
            }
        }

        public ImageSource ScannerImage
        {
            get
            {
                return this.scannerImage;
            }
            set
            {
                if (value == this.scannerImage)
                {
                    return;
                }
                this.scannerImage = value;

                this.OnPropertyChanged("ScannerImage");
            }
        }

        public string IdLine1
        {
            get
            {
                return this.idLine1;
            }
            set
            {
                if (Equals(value, this.idLine1))
                {
                    return;
                }
                this.idLine1 = value;
                this.OnPropertyChanged("IdLine1");
            }
        }

        public string IdLine2
        {
            get
            {
                return this.idLine2;
            }
            set
            {
                if (Equals(value, this.idLine2))
                {
                    return;
                }
                this.idLine2 = value;
                this.OnPropertyChanged("IdLine2");
            }
        }        

        public bool InCaptureMode
        {
            get
            {
                return this.inCaptureMode;
            }
            set
            {
                if (Equals(this.inCaptureMode, value))
                {
                    return;
                }
                this.inCaptureMode = value;

                this.OnPropertyChanged("InCaptureMode");

                this.UserInstructionLabel = value ? Text.CaptureEndInstruction : Text.CaptureStartInstruction;

                this.PrintGridViewModel.IsCaptureInProgress = value;
            }
        }

        public string UserInstructionLabel
        {
            get
            {
                return this.userInstructionLabel;
            }
            set
            {
                if (Equals(value, this.userInstructionLabel))
                {
                    return;
                }
                this.userInstructionLabel = value;
                this.OnPropertyChanged("UserInstructionLabel");
            }
        }

        public bool ServiceConnected
        {
            get
            {
                return this.serviceConnected;
            }
            set
            {
                this.serviceConnected = value;
                this.ServiceStatusImage = value ? this.serviceConnectedImage : this.serviceDisconnectedImage;
                this.ServiceMessage = value ? Text.ServiceConnectedMessage : Text.ServiceDisconnectedMessage;
            }
        }

        public string ServiceMessage
        {
            get
            {
                return this.serviceMessage;
            }
            set
            {
                if (value == this.serviceMessage)
                {
                    return;
                }
                this.serviceMessage = value;
                this.OnPropertyChanged("ServiceMessage");
            }
        }

        

        public BitmapImage ServiceStatusImage
        {
            get
            {
                return this.serviceStatusImage;
            }
            private set
            {
                if (Equals(value,this.serviceStatusImage))
                {
                    return;
                }
                this.serviceStatusImage = value;
                this.OnPropertyChanged("ServiceStatusImage");
            }
        }

        public bool IsEndButtonAvailable
        {
            get
            {
                return this.isEndButtonAvailable;
            }
            set
            {
                if (value == this.isEndButtonAvailable)
                {
                    return;
                }
                this.isEndButtonAvailable = value;
                this.OnPropertyChanged("IsEndButtonAvailable");
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
       
    }
}
