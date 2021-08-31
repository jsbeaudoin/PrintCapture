namespace PrintsCapture.Device.Extension
{
    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints.Enum;

    public static class CaptureDeviceExtension
    {
        public static string GetSecondaryInfo(this ICaptureDevice device)
        {
            if (device == null)
            {
                return string.Empty;
            }

            if (device.Sdk.DeviceKind == CaptureKind.Cardscan)
            {
                return device.SerialNumber;
            }

            return null;
        }

        
        public static bool Supports(this ICaptureDevice device, PrintResolution resolution)
        {
            return (device.SupportedResolutions & resolution) == resolution;
        }

        public static bool Supports(this ICaptureDevice device, DeviceScanKind scanKind)
        {
            return (device.SupportedScanKinds & scanKind) == scanKind;
        }
    }
}
