namespace PrintsCapture.Device
{
    using System.ComponentModel;

    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints.Enum;

    public class DeviceViewModel : INotifyPropertyChanged
    {
        private string internalKey;

        private string label;

        private string serialNumber;

        private string resolutions;

        private string imageUri;

        public DeviceViewModel(ICaptureDevice device)
        {
            this.internalKey = device.InternalKey;
            this.Label = device.DisplayName;
            this.Resolutions = device.SupportedResolutions.ToDescription();
            this.SerialNumber = device.SerialNumber;
            this.ImageUri = device.ImageUri;

            var cd = device as ICardscanDevice;
            if (cd != null)
            {
                this.DetectKey = cd.ModelName.ToLowerInvariant();
            }
        }

        public string DetectKey { get; private set; }

        public string InternalKey
        {
            get
            {
                return this.internalKey;
            }
            set
            {
                if (Equals(value, this.internalKey))
                {
                    return;
                }

                this.internalKey = value;
                this.OnPropertyChanged("InternalKey");
            }
        }

        public string Label
        {
            get
            {
                return this.label;
            }
            set
            {
                if (Equals(value, this.label))
                {
                    return;
                }

                this.label = value;
                this.OnPropertyChanged("Label");
            }
        }

        public string SerialNumber
        {
            get
            {
                return this.serialNumber;
            }
            set
            {
                if (Equals(value, this.serialNumber))
                {
                    return;
                }

                this.serialNumber = value;
                this.OnPropertyChanged("SerialNumber");
            }
        }

        public string Resolutions
        {
            get
            {
                return this.resolutions;
            }
            set
            {
                if (Equals(value, this.resolutions))
                {
                    return;
                }

                this.resolutions = value;
                this.OnPropertyChanged("Resolutions");
            }
        }

        public string ImageUri
        {
            get
            {
                return this.imageUri;
            }
            set
            {
                if (this.imageUri == value)
                {
                    return;
                }
                this.imageUri = value;

                this.OnPropertyChanged("ImageUri");
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
