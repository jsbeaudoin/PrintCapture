namespace PrintsCapture.Device.Enum
{
    public enum SdkErrorKind
    {
        SpecificError, // No translation. Rare error only for one kind of sdk

        SdkInitFailed,
        DeviceNotFound,
        CommunicationError, // Error with the Usb/Firewire device communication
        CaptureSurfaceDirty,
        OutOfMemory,
        InvalidParameter, // function not supported for device,
        DeviceInitializationFailed

    }
}
