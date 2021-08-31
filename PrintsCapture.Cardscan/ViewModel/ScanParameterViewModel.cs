using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using PrintsCapture.Prints.Enum;
using PrintsCapture.Prints.Language;

namespace PrintsCapture.Cardscan.ViewModel
{
    public class ScanParameterViewModel : INotifyPropertyChanged
    {        
        private int customSize;

        private int customOffset;

        private int presetCode;

        private int deviceResolution;

        private bool isCustom;                

        public ScanParameterViewModel(string deviceName, PrintResolution supportedResolutions, string serial, int presetCode)
        {
            this.Presets = ScanPreset.GetBaseList();

            // update and translate labels
            foreach (var preset in this.Presets)
            {
                var text = CommonText.ResourceManager.GetString(preset.Name, CommonText.Culture);
                preset.Label = String.IsNullOrEmpty(text) ? preset.Name : text;
            }

            this.DeviceName = deviceName;
            this.Resolutions = new List<int>();
            if (supportedResolutions.HasFlag(PrintResolution.Dpi500))
            {
                this.Resolutions.Add(500);
            }
            if (supportedResolutions.HasFlag(PrintResolution.Dpi1000))
            {
                this.Resolutions.Add(1000);
            }

            this.DeviceResolution = this.Resolutions.First();
            this.DeviceSerial = serial;
            this.PresetCode = presetCode;
            this.IsCustom = presetCode == 0;

            // Update custom values
            this.CustomOffset = (int)Math.Round(CardScanSettings.Default.PresetManualOffset * 100, 0);

            this.customSize = (int)Math.Round(CardScanSettings.Default.PresetManualSize * 100, 0);
        }

        public string DeviceName { get; set; }

        public string DeviceSerial { get; set; }

        public int DeviceResolution
        {
            get
            {
                return this.deviceResolution;
            }
            set
            {
                if (Equals(value, this.deviceResolution))
                {
                    return;
                }
                this.deviceResolution = value;
                this.OnPropertyChanged("DeviceResolution");
            }
        }

        public int PresetCode
        {
            get
            {
                return this.presetCode;
            }
            set
            {
                if (Equals(value, this.presetCode))
                {
                    return;
                }

                this.presetCode = value;
                this.OnPropertyChanged("PresetCode");

                if (value > 0 && this.IsCustom)
                {
                    this.IsCustom = false;
                }
            }
        }

        public List<int> Resolutions { get; private set; }

        public List<ScanPreset> Presets { get; private set; }

        public int CustomOffset
        {
            get
            {
                return this.customOffset;
            }
            set
            {
                if (Equals(value, this.customOffset))
                {
                    return;
                }
                this.customOffset = value;
                this.OnPropertyChanged("CustomOffset");
            }
        }

        public int CustomSize
        {
            get
            {
                return this.customSize;
            }
            set
            {
                if (Equals(value, this.customSize))
                {
                    return;
                }
                this.customSize = value;
                this.OnPropertyChanged("CustomSize");
            }
        }

        public bool IsCustom
        {
            get
            {
                return this.isCustom;
            }
            set
            {
                if (value == this.isCustom)
                {
                    return;
                }                

                this.isCustom = value;
                this.OnPropertyChanged("IsCustom");

                if (!value && this.PresetCode == 0)
                {
                    this.PresetCode = 1;
                }
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
