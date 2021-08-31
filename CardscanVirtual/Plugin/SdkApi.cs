namespace PrintsCapture.Device.CardscanVirtual.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;

    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints.Enum;

    [Export(typeof(ICaptureSdk))]
    public class SdkApi : ICaptureSdk
    {
        public SdkApi()
        {
            this.SetList();
            this.IsProvidingSerial = false;
            this.DeviceKind = CaptureKind.Cardscan;
        }

        public bool Open()
        {
            this.LastException = null;
            this.IsOpened = true;
            return true;
        }

        public CaptureKind DeviceKind { get; private set; }        

        public bool IsOpened { get; private set; }

        public IEnumerable<ICaptureDevice> SupportedDeviceList { get; private set; }

        public IEnumerable<ICaptureDevice> GetPluggedDevices()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            this.IsOpened = false;
        }

        public bool IsProvidingSerial { get; private set; }

        public bool IsVirtual { get { return true; } }
        public Exception LastException { get; private set; }

        private void SetList()
        {
            var list = new List<ICaptureDevice>
                       {
                           new DeviceApi(
                               "VIRTUAL",
                               "VIRTUAL",
                               "NONE",
                               "VIRTUAL IMAGE SCANNER",
                               PrintResolution.Dpi500 | PrintResolution.Dpi1000, 
                               "pack://application:,,,/PrintsCapture.Device.CardscanVirtual;component/Images/CardscanVirtual.png",
                               this),
                          new DeviceApi
                               (
                                "FOLDER IMPORTER",
                                "FOLDER",
                                "FOLDER",
                                "FOLDER",
                                PrintResolution.Dpi500 | PrintResolution.Dpi1000, 
                                "pack://application:,,,/PrintsCapture.Device.CardscanVirtual;component/Images/CardscanVirtual.png",
                                this
                               )
                       };

            this.SupportedDeviceList = list;
        }
    }
}
