// -----------------------------------------------------------------------
// <copyright file="DeviceConfiguration.cs" company="Solutions XL-ID inc.">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PrintsCapture.Device.Enum;
using PrintsCapture.Device.Extension;
using PrintsCapture.Device.Interface;
using PrintsCapture.Prints.Enum;
using PrintsCapture.Prints.Language;
using XL_ID.Utilities.Wpf.ViewModel;

namespace PrintsCapture.Livescan.ViewModel
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class LiveScanConfigurationViewModel : INotifyPropertyChanged
    {
        private ListElementViewModel<PrintResolution> fingerResolution;

        private ListElementViewModel<PrintResolution> palmResolution;

        private ListElementViewModel<ILivescanDevice> selectedDevice;

        private List<ListElementViewModel<ILivescanDevice>> deviceList;

        private List<ListElementViewModel<PrintResolution>> fingerResolutions;                

        private bool supportPalms;        

        private bool captureTwoFlatThumbs;

        private List<DeviceProperty> deviceProperties;        

        public LiveScanConfigurationViewModel()
        {
            this.Resolutions = new List<ListElementViewModel<PrintResolution>>
                
                               {
                                   new ListElementViewModel<PrintResolution>(PrintResolution.Dpi500, @"500"),
                                   new ListElementViewModel<PrintResolution>(PrintResolution.Dpi1000, @"1000")
                               };
        }

        /// <summary>
        /// Gets or sets the finger resolution.
        /// </summary>
        public ListElementViewModel<PrintResolution> FingerResolution
        {
            get
            {
                return this.fingerResolution;
            }
            set
            {
                if (value == this.fingerResolution)
                {
                    return;
                }

                this.fingerResolution = value;
                this.OnPropertyChanged(@"FingerResolution");
                this.OnPropertyChanged(@"ResolutionDescription");
            }
        }        

        /// <summary>
        /// Gets or sets the palm resolution.
        /// </summary>
        public ListElementViewModel<PrintResolution> PalmResolution
        {
            get
            {
                return this.palmResolution;
            }
            set
            {
                if (value == this.palmResolution)
                {
                    return;
                }

                this.palmResolution = value;
                this.OnPropertyChanged(@"PalmResolution");
                this.OnPropertyChanged(@"ResolutionDescription");
            }
        }

        public bool CaptureTwoFlatThumbs
        {
            get
            {
                return this.captureTwoFlatThumbs;
            }
            set
            {
                if (value.Equals(this.captureTwoFlatThumbs))
                {
                    return;
                }
                this.captureTwoFlatThumbs = value;
                this.OnPropertyChanged(@"CaptureTwoFlatThumbs");
            }
        }

        
        public string ResolutionDescription
        {
            get
            {
                var desc = string.Format(@"{0} : {1}", CommonText.Fingers, this.FingerResolution.Label);

                if (this.SupportPalms)
                {
                    desc = string.Format(@"{0}  /  {1} : {2}", desc, CommonText.Palms, this.PalmResolution.Label);
                }

                return desc;
            }            
        }

        public ListElementViewModel<ILivescanDevice> SelectedDevice
        {
            get
            {
                return this.selectedDevice;
            }

            set
            {
                if (value == this.selectedDevice)
                {
                    return;
                }
                
                this.selectedDevice = value;
                ILivescanDevice scanner = null;
                if (value == null)
                {
                    this.SupportPalms = false;
                }
                else
                {
                    scanner = this.selectedDevice.Key;
                    this.SupportPalms = scanner.Supports(DeviceScanKind.FlatPartialPalm)
                                    | scanner.Supports(DeviceScanKind.FlatCompletePalm);                                        
                }

                this.OnPropertyChanged(@"SelectedDevice");

                this.CreateProperties();

                this.AdjustResolutions(scanner);
            }
        }

        public void GenericScannerAdaptation()
        {
            if (this.selectedDevice != null)
            {
                var scanner = this.selectedDevice.Key;
                this.AdjustResolutions(scanner);
                this.SupportPalms = scanner.Supports(DeviceScanKind.FlatPartialPalm)
                                    | scanner.Supports(DeviceScanKind.FlatCompletePalm); 
            }            
        }

        private void AdjustResolutions(ILivescanDevice scanner)
        {
            if (scanner == null)
            {
                this.Resolutions = null;
                return;
            }

            ListElementViewModel<PrintResolution> defaultResolution = null;

            var resolutions = new List<ListElementViewModel<PrintResolution>>();

            if (scanner.SupportedResolutions.IsSupported(PrintResolution.Dpi500))
            {
                var res = new ListElementViewModel<PrintResolution>(PrintResolution.Dpi500, @"500");
                resolutions.Add(res);
                defaultResolution = res;                
            }

            if (scanner.SupportedResolutions.IsSupported(PrintResolution.Dpi1000))
            {
                var res = new ListElementViewModel<PrintResolution>(PrintResolution.Dpi1000, @"1000");
                resolutions.Add(res);
                if (defaultResolution == null)
                {
                    defaultResolution = res;
                }
            }

            this.Resolutions = resolutions;
            this.FingerResolution = defaultResolution;
            this.PalmResolution = defaultResolution;
        }        

        public bool SupportPalms
        {
            get
            {
                return this.supportPalms;
            }
            private set
            {
                if (value.Equals(this.supportPalms))
                {
                    return;
                }
                this.supportPalms = value;
                this.OnPropertyChanged(@"SupportPalms");
            }
        }       

        /// <summary>
        /// Gets or sets the resolutions.
        /// </summary>
        public List<ListElementViewModel<PrintResolution>> Resolutions
        {
            get
            {
                return this.fingerResolutions;
            }
            private set
            {
                if (Equals(value, this.fingerResolutions))
                {
                    return;
                }
                this.fingerResolutions = value;
                this.OnPropertyChanged(@"Resolutions");
            }
        }        

        /// <summary>
        /// Gets or sets the scanners.
        /// </summary>
        public List<ListElementViewModel<ILivescanDevice>> DeviceList
        {
            get
            {
                return this.deviceList;
            }
            set
            {
                if (Equals(value, this.deviceList))
                {
                    return;
                }
                this.deviceList = value;
                this.OnPropertyChanged(@"DeviceList");
            }
        }

        public List<DeviceProperty> DeviceProperties
        {
            get
            {
                return this.deviceProperties;
            }
            private set
            {
                if (Equals(value, this.deviceProperties))
                {
                    return;
                }
                this.deviceProperties = value;
                this.OnPropertyChanged(@"DeviceProperties");
            }
        }
        
        private void CreateProperties()
        {            
            if ( this.selectedDevice == null ||  this.selectedDevice.Key == null || this.selectedDevice.Key.Properties == null)
            {
                this.DeviceProperties = null;
                return;
            }

            var list = this.selectedDevice.Key.Properties.GetPropertyList().Select(property => new DeviceProperty(property)).OrderBy(x => x.Label).ToList();
             
            this.DeviceProperties = list;
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
