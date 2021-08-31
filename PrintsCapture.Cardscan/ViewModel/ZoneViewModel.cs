namespace PrintsCapture.Cardscan.ViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Media.Imaging;
    using System.Xml.Serialization;

    public class ZoneViewModel : INotifyPropertyChanged
    {
        private BitmapSource displayedImage;

        private int rotation;

        public ZoneViewModel(ScanPrintZone scanPrintZone)
        {
            this.LinkedZone = scanPrintZone;
            this.Id = this.LinkedZone.PrintZone.Id;
            this.Name = this.LinkedZone.PrintZone.Label;
            this.rotation = this.LinkedZone.Rotation;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public int Rotation
        {
            get
            {
                return this.rotation;
            }            
            set
            {
                var pivot = value;
                if (pivot < 0)
                {
                    pivot = 360 + pivot;
                }

                if (pivot >= 360)
                {
                    pivot -= 360;
                }
                this.rotation = pivot;

                this.LinkedZone.Rotation = this.rotation;
                this.OnPropertyChanged("Rotation");
                PageScan.CaptureCurrentZone();
            }            
        }

        [XmlIgnore]
        public ScanPrintZone LinkedZone { get; private set; }

        [XmlIgnore]
        public BitmapSource DisplayedImage
        {
            get
            {
                return this.displayedImage;
            }
            set
            {
                this.displayedImage = value;
                this.OnPropertyChanged("DisplayedImage");
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
