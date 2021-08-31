using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using PrintsCapture.Device.Enum;
using PrintsCapture.Device.Interface;
using PrintsCapture.Prints.Enum;
using TouchLab.NET;

namespace PrintsCapture.Device.LivescanJenetric.Plugin
{
    [Export(typeof(ICaptureSdk))]
    public class SdkApi : ICaptureSdk
    {
        public const string DeviceMake = "Jenetric";

        public CaptureKind DeviceKind { get; private set; }

        public bool IsOpened { get; private set; }

        public IEnumerable<ICaptureDevice> SupportedDeviceList { get; private set; }

        public bool IsProvidingSerial { get; private set; }

        public bool IsVirtual { get { return false; } }

        public Exception LastException { get; private set; }

        public TouchLabApi api;

        public SdkApi()
        {
            this.IsProvidingSerial = true;
            this.DeviceKind = CaptureKind.Livescan;
            this.api = TouchLabApi.Instance;
            this.SetDeviceList();
        }

        public void Close()
        {
            this.IsOpened = false;
            this.api.Dispose();
            this.api = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }

        public IEnumerable<ICaptureDevice> GetPluggedDevices()
        {
            var connectedDevices = this.api.GetScannerList().Select(x => x.ScannerTypeName).ToList();
            return this.SupportedDeviceList.Where(x => connectedDevices.Any(conn => conn == x.InternalKey));
        }

        public bool Open()
        {
            this.LastException = null;
            this.IsOpened = false;

            try
            {
                api = TouchLabApi.Instance;
            }
            catch (Exception ex)
            {
                this.LastException = ex;                
                return false;
            }
            
            this.IsOpened = true;
            
            return true;

        }

        private void SetDeviceList()
        {
            var list = new List<ICaptureDevice>();

            list.Add(new DeviceApi("LIVETOUCH quattro", "TouchLab Quattro", PrintResolution.Dpi500, DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatSingleFinger | DeviceScanKind.FlatTwoFinger | DeviceScanKind.RolledSingleFinger,
                "pack://application:,,,/PrintsCapture.Device.LivescanJenetric;component/Images/LivetouchQuattro.png",
                this));
            list.Add(new DeviceApi("LIVETOUCH quattro Compact", "TouchLab Quattro Compact", PrintResolution.Dpi500, DeviceScanKind.FlatFourFinger | DeviceScanKind.FlatSingleFinger | DeviceScanKind.FlatTwoFinger | DeviceScanKind.RolledSingleFinger,
                "pack://application:,,,/PrintsCapture.Device.LivescanJenetric;component/Images/LivetouchQuattroCompact.png",
                this));
            this.SupportedDeviceList = list;
        }
    }
}
