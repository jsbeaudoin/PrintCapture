using System.Collections.Generic;

namespace PrintsCapture.Device.DataLayer
{
    public class DeviceSetting
    {
        public DeviceSetting()
        {
            this.Settings = new List<SettingValue>();
        }

        public string Device { get; set; }

        public List<SettingValue> Settings { get; set; }
    }
}
