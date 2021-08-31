namespace PrintsCapture.Device.CardscanAware.Plugin
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    using global::Aware.AccuScan;

    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints.Enum;
    using Microsoft.Win32;

    using XL_ID.Utilities.Image;

    public class DeviceApi : ICardscanDevice
    {
        public DeviceApi()
        {            
        }

        public DeviceApi(string displayName, string internalKey, string hardwareMake, string modelName, PrintResolution supportedResolutions, string imageUri, ICaptureSdk sdk)
        {
            this.DisplayName = displayName;
            this.InternalKey = internalKey;
            this.HardwareMake = hardwareMake;
            this.ModelName = modelName;
            this.SupportedResolutions = supportedResolutions;
            this.Sdk = sdk;
            this.SupportedScanKinds = DeviceScanKind.AllScanTypes;

            this.ImageUri = imageUri;
            this.scannerType = awAccuScan.AwareScannerType.AW_SCAN_TYPE_MEMORY;

            Enum.TryParse(this.InternalKey, out this.scannerType);            
        }

        private readonly awAccuScan.AwareScannerType scannerType;

        private awAccuScan scanObject;                

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

        public event DeviceStateChangedHandler StateChanged;

        public void Open()
        {
            this.ChangeState(DeviceState.Opening);
            this.IsOpened = true;
            try
            {
                this.scanObject = new awAccuScan(this.scannerType);
                this.ChangeState(DeviceState.Opened);
                this.ChangeState(DeviceState.Ready);
            }
            catch 
            {
                this.IsOpened = false;
                this.ChangeState(DeviceState.Closed);
            }
            

            //this.scanObject.ProgressCallbackFunction += ProgressCallbackFunction;
        }

        public void SetSerialNumber(string serial)
        {
            this.SerialNumber = serial;
        }

        /// <summary>
        /// Scan
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns>Scanned Bitmap</returns>
        /// <exception cref="ApplicationException">On error with scanner, can throw exceptions</exception>
        public Bitmap Scan(PrintResolution resolution, float offset, float size)
        {

            if (!this.IsOpened)
            {
                throw new ApplicationException("Device not opened");
            }
            this.ChangeState(DeviceState.ScanInitialization);
            //this.scanSize = size;
            var scanRes = resolution == PrintResolution.Dpi500 ? awAccuScan.AwareScanningResolution.AW_500PPI : awAccuScan.AwareScanningResolution.AW_1000PPI;
            
            awAccuScan.PageSize maxPageSize;
            if (this.scannerType == awAccuScan.AwareScannerType.AW_SCAN_TYPE_MEMORY)
            {
                maxPageSize = this.GetMemoryBitmap();                
            }
            else
            {
                maxPageSize = this.scanObject.GetMaxPageSize();                
            }

            var cropTop = (float)Math.Round(maxPageSize.Height * offset,2);
            var cropHeight = (float)Math.Round(maxPageSize.Height * size, 2);

            this.CheckError(this.scanObject.SetResolution(scanRes));

            this.CheckError(this.scanObject.SetUnits(awAccuScan.AwareMeasurementUnits.AW_UNITS_INCHES));

            this.CheckError(
                this.scanObject.SetPageArea(cropTop, 0, cropHeight, maxPageSize.Width, 0, resolution.ToDpi(), 1));

            this.CheckError(this.scanObject.SetPageCropRegion(1, cropTop, 0, cropHeight, maxPageSize.Width, 1));

            this.ChangeState(DeviceState.Scanning);

            this.CheckError(this.scanObject.AcquirePage(1));

            var cropSize = this.scanObject.GetCropRegionSize(1);
            var byteBuffer = this.scanObject.GetCropRegion(1, cropSize.Height, cropSize.Width);

            this.ChangeState(DeviceState.Ready);

            return ImageUtilities.ByteArrayToBitmap(
                byteBuffer,
                cropSize,
                PixelFormat.Format8bppIndexed,
                resolution.ToDpi());
            
        }

        private awAccuScan.PageSize GetMemoryBitmap()
        {
            try
            {
                var od = new OpenFileDialog();
                od.Filter = "Bitmap | *.bmp";
                od.CheckFileExists = true;

                if (od.ShowDialog() == true)
                {
                    var bmp = (Bitmap)Image.FromFile(od.FileName);                    
                    var cropRegionImage = this.GetImageRawData(bmp);
                    this.scanObject.LoadPageMem(cropRegionImage, bmp.Height, bmp.Width, awAccuScan.AwareScanningResolution.AW_500PPI, 1);
                    return new awAccuScan.PageSize { Height = bmp.Height / 500F, Width = bmp.Width / 500F };
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("GetMemoryBitmap Failed", ex);
            }

            return new awAccuScan.PageSize { Height = 0, Width = 0};
        }

        private byte[] GetImageRawData(Bitmap bmp)
        {
            

            // Need to set a lock to get access to bitmap data 
            BitmapData bmpDat1 = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            int pixelSize = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            var result = new byte[bmp.Width * bmp.Height * pixelSize];

            int startAdr = bmpDat1.Scan0.ToInt32();
            int adrIncrement = bmpDat1.Stride;

            if (bmpDat1.Stride < 1)
            {
                startAdr = (bmp.Height - 1) * adrIncrement;
            }
            int adr = startAdr;

            for (int origRow = 0, newRow = 0; origRow < bmp.Height; origRow++, newRow++)
            {
                Marshal.Copy(new IntPtr(adr), result, newRow * bmp.Width * pixelSize, bmp.Width);
                adr += adrIncrement;
            }
            
            // release the lock 
            bmp.UnlockBits(bmpDat1);

            return result;
        }

        private void CheckError(ErrorInfo.errorCode err)
        {
            if (err != ErrorInfo.errorCode.AWSCAN_NO_ERRORS)
            {
                throw new ApplicationException(string.Format("Aware Error # {0} : {1}", (int)err, err));
            }
        }               

        public void Close()
        {
            if (!this.IsOpened)
            {
                return;
            }
            this.ChangeState(DeviceState.Closing);

            try
            {                
                this.scanObject.Dispose();
            }
            catch (Exception)
            {
                // nothing                
            }

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
