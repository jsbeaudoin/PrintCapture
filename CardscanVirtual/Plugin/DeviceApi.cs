namespace PrintsCapture.Device.CardscanVirtual.Plugin
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    using Microsoft.Win32;

    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints.Enum;

    using XL_ID.Utilities.Image;

    public class DeviceApi : ICardscanDevice
    {
        public DeviceApi(string displayName, string internalKey, string hardwareMake, string modelName, PrintResolution supportedResolutions, string imageUri, ICaptureSdk sdk)
        {
            this.DisplayName = displayName;
            this.InternalKey = internalKey;
            this.HardwareMake = hardwareMake;
            this.ModelName = modelName;
            this.SupportedResolutions = supportedResolutions;
            this.Sdk = sdk;
            this.ImageUri = imageUri;
            this.SupportedScanKinds = DeviceScanKind.AllScanTypes;
        }

        public event DeviceStateChangedHandler StateChanged;

        public string DisplayName { get; private set; }

        public string InternalKey { get; private set; }

        public string HardwareMake { get; private set; }

        public string ModelName { get; private set; }

        public string SerialNumber { get; set; }

        public string ImageUri { get; private set; }

        public PrintResolution SupportedResolutions { get; private set; }

        public DeviceScanKind SupportedScanKinds { get; private set; }

        public ICaptureSdk Sdk { get; private set; }
        public DeviceState State { get; private set; }
        

        public void Open()
        {
            // nothing
            this.ChangeState(DeviceState.Opening);
            this.IsOpened = true;
            this.ChangeState(DeviceState.Opened);
            this.ChangeState(DeviceState.Ready);
        }

        public void SetSerialNumber(string serial)
        {
            this.SerialNumber = serial;
        }

        public Bitmap Scan(PrintResolution resolution, float offset, float size)
        {
            this.ChangeState(DeviceState.ScanInitialization);
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            OpenFileDialog od;
            try
            {
                od = new OpenFileDialog { CheckFileExists = true, DefaultExt = "bmp", InitialDirectory = folder };
            }
            catch (Exception)
            {
                try
                {
                    od = new OpenFileDialog { CheckFileExists = true, DefaultExt = "bmp" };
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
                
            }
            

            var dlgResult = od.ShowDialog();

            if (!(dlgResult.HasValue && dlgResult.Value))
            {
                return null;
            }

            var bmp = (Bitmap)Image.FromFile(od.FileName);

            this.ChangeState(DeviceState.Scanning);

            var indexed = ImageUtilities.ConvertToIndexedFormat(bmp,ConvertBitmapFormat.Format8bppIndexed);

            if (bmp.HorizontalResolution < 400 || bmp.VerticalResolution < 400)
            {
                indexed.SetResolution(500, 500);
            }
            else
            {
                indexed.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
            }

            this.ChangeState(DeviceState.Ready);

            return indexed;
        }        




        public void Close()
        {
            // nothing
            this.ChangeState(DeviceState.Closing);
            this.IsOpened = false;
            this.ChangeState(DeviceState.Closed);
        }

        public bool IsOpened { get; private set; }

        private void ChangeState(DeviceState newState)
        {
            var oldState = this.State;
            this.State = newState;

            var handler = this.StateChanged;
            if (handler == null || oldState == newState)
            {
                return;
            }

            handler(this, newState);
        }
    }
}
