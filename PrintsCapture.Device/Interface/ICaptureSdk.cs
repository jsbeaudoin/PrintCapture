namespace PrintsCapture.Device.Interface
{
    using System;
    using System.Collections.Generic;    
    using PrintsCapture.Prints.Enum;

    public interface ICaptureSdk
    {
        /// <summary>
        /// Gets the device kind returned by this sdk
        /// </summary>
        CaptureKind DeviceKind { get; }

        /// <summary>
        /// Opens the sdk. Check if it can run or not. Returns false in case of error / missing dlls.
        /// </summary>           
        /// <returns></returns>
        bool Open();

        /// <summary>
        /// Sdk has been opened
        /// </summary>
        bool IsOpened { get; }

        /// <summary>
        /// Returns the list of all supported non initialized device. For list puspose.
        /// </summary>
        IEnumerable<ICaptureDevice> SupportedDeviceList { get; }

        /// <summary>
        /// Return the list of plugged devices
        /// </summary>
        /// <returns></returns>
        IEnumerable<ICaptureDevice> GetPluggedDevices();

        /// <summary>
        /// Stop the usage of the sdk
        /// </summary>
        void Close();

        bool IsProvidingSerial { get; }

        bool IsVirtual { get; }

        Exception LastException { get; }
    }

}