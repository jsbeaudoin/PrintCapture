using PrintsCapture.Device.Class;

namespace PrintsCapture.Device
{
    using System;
    using System.Collections.Generic;

    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;

    public delegate void PrintCapturedHandler(object sender, PrintCapturedEventArgs e);

    public abstract class DeviceCaptureDriver
    {
        protected DeviceCaptureDriver(ICaptureDevice captureDevice, PrintList printList)
        {
            this.CaptureDevice = captureDevice;
            this.PrintList = printList;
        }

        public const string GenericDeviceKey = "*";

        public ICaptureDevice CaptureDevice { get; protected set; }

        public PrintList PrintList { get; protected set; }

        public abstract string GetDeviceSecondaryInfo();

        public abstract ScanResult CaptureAuto();

        public abstract ScanResult CaptureSingle(PrintInfo print);

        public abstract ScanResult CapturePositions();

        public abstract void ConfigureDefaultValue(List<ICaptureSdk> sdkList, string selectedKey);

        public abstract bool? Configure(DeviceConfigurationViewModel viewModel);

        public abstract bool? DoOperation(PrintInfo print, PrintZoomAction op);

        public event PrintCapturedHandler PrintCaptured;

        public event EventHandler SettingsCorrupted;

        public event EventHandler CaptureCompleted;

        public event EventHandler DeviceInitialized;

        public void SetDevice(ICaptureDevice captureDevice)
        {
            this.CaptureDevice = captureDevice;
            if (!this.CaptureDevice.Sdk.IsOpened)
            {
                this.CaptureDevice.Sdk.Open();
            }
        }

        protected void TriggerPrintCaptured(PrintInfo print, bool keepOriginal, bool? sendBatch = null)
        {
            var handler = this.PrintCaptured;
            if (handler == null)
            {
                return;
            }
            var e = new PrintCapturedEventArgs(print);
            e.SendBatch = sendBatch;
            e.KeepOriginalImage = keepOriginal;
            handler(this, e);
            PrintModificationDispatcher.PrintModified(print);
        }

        protected void TriggerSettingsCorrupted()
        {
            this.SettingsCorrupted?.Invoke(this, EventArgs.Empty);
        }

        protected void TriggerCaptureCompleted(bool hasError)
        {
            this.CaptureCompleted?.Invoke(this, new CaptureCompletedEventArgs(hasError));
        }

        protected void TriggerDeviceInitialized()
        {
            this.DeviceInitialized?.Invoke(this, EventArgs.Empty);
            
        }
    }
}
