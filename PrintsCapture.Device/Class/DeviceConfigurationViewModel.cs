namespace PrintsCapture.Device
{
    using System.Collections.Generic;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints.Enum;

    public class DeviceConfigurationViewModel
    {
        public CaptureKind CaptureKind { get; set; }

        public List<ICaptureSdk> Sdks { get; set; }

        public string SelectedDeviceKey { get; set; }
    }
}
