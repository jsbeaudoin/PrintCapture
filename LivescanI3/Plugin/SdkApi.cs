namespace PrintsCapture.Device.LivescanI3.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;

    using Idintl.LiveScan;

    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints.Enum;

    [Export(typeof(ICaptureSdk))]
    public class SdkApi : ICaptureSdk
    {
        public const string DeviceMake = "I3";

        public CaptureKind DeviceKind { get; private set; }

        internal LiveScanManager Manager { get; private set; }

        public SdkApi()
        {
            this.IsProvidingSerial = true;
            this.DeviceKind = CaptureKind.Livescan;
            this.SetDeviceList();
        }

        public bool Open()
        {
            this.LastException = null;
            this.IsOpened = true;
            this.Manager = new LiveScanManager();
            this.Manager.Initialize();
            
            return true;
        }

        public bool IsOpened { get; private set; }

        public IEnumerable<ICaptureDevice> SupportedDeviceList { get; private set; }

        public IEnumerable<ICaptureDevice> GetPluggedDevices()
        {
            if (!this.IsOpened)
            {
                return null;
            }

            var dev = this.Manager.FirstDevice();
            if (dev != null)
            {
                var plugged = this.SupportedDeviceList.FirstOrDefault(x => x.InternalKey == dev.Identification.Model);
                return new List<ICaptureDevice> {plugged};
            }

            return null;
        }

        public void Close()
        {
            this.IsOpened = false;
            this.Manager.Dispose();
            this.Manager = null;
        }

        public bool IsProvidingSerial { get; private set; }

        public bool IsVirtual { get { return false; } }

        public Exception LastException { get; private set; }

        private void SetDeviceList()
        {
            var list = new List<ICaptureDevice>();

            list.Add(new DeviceApi("digID Mini", "I3 digID Mini", PrintResolution.Dpi500, DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatSingleFinger | DeviceScanKind.FlatTwoFinger | DeviceScanKind.RolledSingleFinger,
                "pack://application:,,,/PrintsCapture.Device.LivescanI3;component/Images/LivescanDigIdMini.png",
                this));
            list.Add(new DeviceApi("digID mini plus", "I3 digID mini plus", PrintResolution.Dpi500, DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatSingleFinger | DeviceScanKind.FlatTwoFinger | DeviceScanKind.RolledSingleFinger,
                "pack://application:,,,/PrintsCapture.Device.LivescanI3;component/Images/LivescanDigIdMini.png",
                this));
            list.Add(new DeviceApi("digID mini+", "I3 digID mini+", PrintResolution.Dpi500, DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatSingleFinger | DeviceScanKind.FlatTwoFinger | DeviceScanKind.RolledSingleFinger,
                "pack://application:,,,/PrintsCapture.Device.LivescanI3;component/Images/LivescanDigIdMini.png",
                this));
            this.SupportedDeviceList = list;
        }

    }
}
