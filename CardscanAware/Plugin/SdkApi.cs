namespace PrintsCapture.Device.CardscanAware.Plugin
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Aware.AccuScan;

    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints.Enum;

    using XL_ID.Utilities.XML;

    [Export(typeof(ICaptureSdk))]
    public class SdkApi : ICaptureSdk
    {
        public SdkApi()
        {
            this.SetList();
            this.DeviceKind = CaptureKind.Cardscan;
            this.IsProvidingSerial = false;
        }

        public bool Open()
        {                        
            this.IsOpened = true;
            this.LastException = null;
            return true;
        }

        public CaptureKind DeviceKind { get; private set; }        

        public bool IsOpened { get; private set; }

        public IEnumerable<ICaptureDevice> SupportedDeviceList { get; private set; }

        public IEnumerable<ICaptureDevice> GetPluggedDevices()
        {
            throw new System.NotImplementedException();
        }

        public void Close()
        {
            this.IsOpened = false;
        }

        public bool IsProvidingSerial { get; private set; }

        public bool IsVirtual { get { return false; } }

        private void SetList()
        {            
            var awareDevices =                
                    new List<AwareScanner>()
                    {
                        new AwareScanner()
                        {
                            DisplayName = "Epson V500",
                            HardwareMake = "EPSON",
                            ModelName = "V500",
                            InternalKey = "AW_SCAN_TYPE_PERFV500",
                            ImageUri = "pack://application:,,,/PrintsCapture.Device.CardscanAware;component/Images/CardscanV500.png",
                            SupportedResolutions = PrintResolution.Dpi500
                        },
                        new AwareScanner()
                        {
                            DisplayName = "Epson V550",
                            HardwareMake = "EPSON",
                            ModelName = "V550",
                            InternalKey = "AW_SCAN_TYPE_PERFV550",
                            ImageUri = "pack://application:,,,/PrintsCapture.Device.CardscanAware;component/Images/CardscanV550.png",
                            SupportedResolutions = PrintResolution.Dpi500
                        },
                        new AwareScanner
                        {
                            DisplayName = "Epson V700",
                            HardwareMake = "EPSON",
                            ModelName = "V700",
                            InternalKey = "AW_SCAN_TYPE_PERFV700",
                            ImageUri = "pack://application:,,,/PrintsCapture.Device.CardscanAware;component/Images/CardscanV700.png",
                            SupportedResolutions = PrintResolution.Dpi500
                        },
                        new AwareScanner
                        {
                            DisplayName = "Epson Memory",
                            HardwareMake = "EPSON",
                            ModelName = "MEMORY",
                            InternalKey = "AW_SCAN_TYPE_MEMORY",
                            ImageUri = "pack://application:,,,/PrintsCapture.Device.CardscanAware;component/Images/CardscanVirtual.png",
                            SupportedResolutions =
                                PrintResolution.Dpi500 | PrintResolution.Dpi1000
                        }
                    };


            var list = new List<ICardscanDevice>();

            foreach (var device in awareDevices)
            {
                list.Add(new DeviceApi(device.DisplayName, device.InternalKey, device.HardwareMake, device.ModelName, device.SupportedResolutions, device.ImageUri, this));
            }

            // MM20160822 : Not adding uncertified scanners anymore

            //foreach (awAccuScan.AwareScannerType scanType in System.Enum.GetValues(typeof(awAccuScan.AwareScannerType)))
            //{
            //    var key = scanType.ToString();
            //    var deviceName = key.Substring(13).ToUpperInvariant();

            //    if (!deviceName.StartsWith("PERF") || awareDevices.FirstOrDefault(x => x.InternalKey == key) != null)
            //    {
            //        continue;
            //    }
                
            //    list.Add(
            //        new DeviceApi(
            //            "Aware " + deviceName,
            //            key,                        
            //            "EPSON",
            //            deviceName,
            //            PrintResolution.Dpi500,
            //            "pack://application:,,,/PrintsCapture.Device.CardscanAware;component/Images/CardscanV500.png",
            //            this));

            //}

            
            

            this.SupportedDeviceList = list;            
        }


        public System.Exception LastException { get; private set; }
    }
}
