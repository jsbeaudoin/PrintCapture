using System.Collections.Generic;

namespace PrintsCapture.Device.DataLayer
{
    public class DeviceSettingList
    {

        public DeviceSettingList()
        {
            this.Devices = new List<DeviceSetting>();
        }

        public List<DeviceSetting> Devices { get; set; }
    }
}
