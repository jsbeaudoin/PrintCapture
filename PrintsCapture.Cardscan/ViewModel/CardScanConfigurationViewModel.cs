namespace PrintsCapture.Cardscan.ViewModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using PrintsCapture.Device;

    public class CardScanConfigurationViewModel : INotifyPropertyChanged
    {
        private DeviceViewModel selectedDevice;

        public List<DeviceViewModel> DeviceList { get; set; }

        public DeviceViewModel SelectedDevice
        {
            get
            {
                return this.selectedDevice;
            }
            set
            {
                if (Equals(value, this.selectedDevice))
                {
                    return;
                }
                
                this.selectedDevice = value;
                this.OnPropertyChanged("SelectedDevice");
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
