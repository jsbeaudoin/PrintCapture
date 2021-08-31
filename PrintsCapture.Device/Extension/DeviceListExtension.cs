using System.Collections.Generic;
using System.Linq;

namespace PrintsCapture.Device.Extension
{
    using PrintsCapture.Device.Interface;

    public static class DeviceListExtension
    {
        public static ICaptureDevice GetDeviceFromKey(this List<ICaptureSdk> list, string key)
        {
            var deviceList = new List<ICaptureDevice>();

            list.ForEach(x => x.SupportedDeviceList.ToList().ForEach(deviceList.Add));

            // build viewModel            
            var selected = deviceList.FirstOrDefault(x => x.InternalKey == key);

            return selected;
        }
    }
}
