namespace PrintsCapture.Device.Enum
{
    public enum DeviceState
    {
        /// <summary>
        /// Device has not been initialized or opened yet
        /// </summary>
        Undefined,
        /// <summary>
        /// Device is opening
        /// </summary>
        Opening,
        /// <summary>
        /// Device has been opened. Should not happen again until device is closed
        /// </summary>
        Opened,
        /// <summary>
        /// Device is ready to scan
        /// </summary>
        Ready,
        /// <summary>
        /// Device is preparing to scan
        /// </summary>
        ScanInitialization,

        /// <summary>
        /// Device is scanning
        /// </summary>
        Scanning,
        /// <summary>
        /// Device is preparing to close
        /// </summary>
        Closing,
        /// <summary>
        /// Device is closed
        /// </summary>
        Closed
    }

    public static class DeviceStateExtension
    {
        /// <summary>
        /// Tells if a device is busy and cannot process command at this time.
        /// </summary>
        /// <param name="state">Current device state</param>
        /// <returns>true if device cannot process command, false otherwise</returns>
        public static bool IsDeviceBusy(this DeviceState state)
        {
            return state == DeviceState.Opening || state == DeviceState.Closing ||
                   state == DeviceState.ScanInitialization;
        }

        
    }
}