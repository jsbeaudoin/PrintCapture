namespace PrintsCapture.Device.LivescanVirtual.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;    

    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints.Enum;

    [Export(typeof(ICaptureSdk))]
    public class SdkApi : ICaptureSdk
    {
        public SdkApi()
        {
            var devices = new List<ILivescanDevice>();
            devices.Add(new DeviceApi("VIRTUAL-ALL", "Virtual Scanner-All Mode", PrintResolution.Dpi500 | PrintResolution.Dpi1000, 
                DeviceScanKind.RolledSingleFinger | DeviceScanKind.FlatSingleFinger | DeviceScanKind.FlatFourFinger  | DeviceScanKind.FlatPartialPalm | DeviceScanKind.FlatTwoFinger,
                "pack://application:,,,/PrintsCapture.Device.LivescanVirtual;component/Images/LivescanVirtual.png",
                this ));

            //devices.Add(new DeviceApi("VIRTUAL-MONO", "Virtual Scanner-mono finger", PrintResolution.Dpi500, DeviceScanKind.FlatSingleFinger,
            //    "pack://application:,,,/PrintsCapture.Device.LivescanVirtual;component/Images/LivescanVirtual.png",
            //    this));

            devices.Add(new DeviceApi("VIRTUAL-PATROL", "Virtual Scanner-Patrol capabilities", PrintResolution.Dpi500, DeviceScanKind.RolledSingleFinger | DeviceScanKind.FlatSingleFinger | DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatTwoFinger,
                "pack://application:,,,/PrintsCapture.Device.LivescanVirtual;component/Images/LivescanVirtual.png",
                this));
            this.DeviceKind = CaptureKind.Livescan;
            this.IsProvidingSerial = true;
            this.SupportedDeviceList = devices;
        }        

        public CaptureKind DeviceKind { get; private set; }

        public bool Open()
        {
            this.LastException = null;
            this.IsOpened = true;
            return true;
        }

        public bool IsOpened { get; private set; }

        public IEnumerable<ICaptureDevice> SupportedDeviceList { get; private set; }

        public IEnumerable<ICaptureDevice> GetPluggedDevices()
        {
            return null;
        }

        public void Close()
        {
            this.IsOpened = false;            
        }

        public bool IsProvidingSerial { get; private set; }

        public bool IsVirtual { get { return true; } }

        public Exception LastException { get; private set; }
    }
}