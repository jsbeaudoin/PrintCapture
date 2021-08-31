namespace PrintsCapture.Device.Interface
{
    using PrintsCapture.Device.Enum;
    using PrintsCapture.Prints.Enum;

    public delegate void DeviceStateChangedHandler(ICaptureDevice sender, DeviceState newState);

    public interface ICaptureDevice
    {
        string DisplayName { get; }

        string InternalKey { get; }

        string HardwareMake { get; }

        string ModelName { get; }

        string SerialNumber { get; }

        string ImageUri { get; }

        PrintResolution SupportedResolutions { get; }

        DeviceScanKind SupportedScanKinds { get; }

        ICaptureSdk Sdk { get; }

        DeviceState State { get; }

        event DeviceStateChangedHandler StateChanged;

        /// <summary>
        /// Opens the device.
        /// </summary>            
        void Open();

        /// <summary>
        /// Closes the device and unload it's resources.
        /// </summary>
        void Close();

        /// <summary>
        /// Is this device already opened
        /// </summary>
        /// <returns></returns>
        bool IsOpened { get; }        
    }    
}