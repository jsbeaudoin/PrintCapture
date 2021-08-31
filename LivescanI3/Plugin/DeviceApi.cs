namespace PrintsCapture.Device.LivescanI3.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading;    

    using Idintl.LiveScan;

    using PrintsCapture.Device;
    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Extension;
    using PrintsCapture.Prints.Language;

    using XL_ID.Utilities.Log;

    public class DeviceApi : ILivescanDevice
    {
        private const string PropSystemSoundMuted = "PROP_SOUND_MUTED";

        #region Fields

        const int WaitMaxDelay = 10000;
        private bool waitForPlaten;

        private readonly SdkApi i3Sdk;

        private LiveScanDevice i3Device;

        private CapturePreviewHandler preview;

        private List<PrintCaptureQuality> printsToCapture;

        private IEnumerable<PhysicalHandPart> handParts;

        private bool deviceOpened;

        private HandScanKind currentScanKind;

        private bool isFlat;

        private bool isStopping;

        #endregion

        #region Events

        public event DeviceMessageHandler DeviceSendMessage;

        public event DeviceOpenedHandler DeviceOpened;

        public event CapturedPrintHandler PrintCaptured;

        public event CaptureQualityHandler CaptureQualityChanged;

        public event DeviceStateChangedHandler StateChanged;

        #endregion

        #region Properties

        public string DisplayName { get; private set; }

        public string InternalKey { get; private set; }

        public string HardwareMake { get; private set; }

        public string ModelName { get; private set; } // filled by sdk

        public string SerialNumber { get; private set; }

        public string ImageUri { get; private set; }

        // filled by sdk

        public PrintResolution SupportedResolutions { get; private set; }

        public DeviceScanKind SupportedScanKinds { get; private set; }

        public ICaptureSdk Sdk
        {
            get
            {
                return this.i3Sdk;
            }
        }

        public DeviceState State { get; private set; }        

        public bool IsOpened { get; private set; }

        public CustomPropertyList Properties { get; private set; }

        public SdkException LastException { get; private set; }

        public PrintResolution FingerResolution { get; set; }

        public PrintResolution PalmResolution { get; set; }

        public string ExternalTool { get; private set; }

        #endregion

        #region Constructor

        internal DeviceApi(string internalName, string friendlyName, PrintResolution resolutions, DeviceScanKind scanKinds, string imageUri, SdkApi sdk)
        {
            this.i3Sdk = sdk;            
            this.InternalKey = internalName;

            this.HardwareMake = SdkApi.DeviceMake.ToUpper();
            this.DisplayName = friendlyName;

            this.SupportedResolutions = resolutions;
            this.SupportedScanKinds = scanKinds;            

            this.Properties = new CustomPropertyList();
            this.ExternalTool = "LivescanSupport.exe";

            this.ImageUri = imageUri;

            this.Properties = new CustomPropertyList();
            this.Properties.AddBoolProperty(PropSystemSoundMuted, "Muted", false, "System Sound");
        }

        #endregion

        #region public methods

        public void Open()
        {
            this.deviceOpened = false;
            var device = this.i3Sdk.Manager.Devices.FirstOrDefault(x => x.Identification.Model == this.InternalKey);
            if (device == null)
            {                
                this.OnDeviceOpened(DeviceOpenStatus.DeviceNotFound);
                return;                
            }

            this.ChangeState(DeviceState.Opening);
            this.waitForPlaten = false;
            this.IsOpened = true;
            this.i3Device = device;
            
            this.i3Device.AsyncOpenComplete += this.I3DeviceOnAsyncOpenComplete;
            this.i3Device.AsyncAcquireComplete += this.I3DeviceOnAsyncAcquireComplete;
            this.i3Device.CaptureBegin += this.I3DeviceOnCaptureBegin;
            this.i3Device.CaptureEnd += this.I3DeviceOnCaptureEnd;
            this.i3Device.FrameAvailable += this.I3DeviceOnFrameAvailable;
            this.i3Device.StatusChange += this.I3DeviceOnStatusChange;
            this.i3Device.ClearPlaten += this.I3DeviceOnClearPlaten;
            this.i3Device.RollBegin += (sender, args) =>  DeviceSoundPlayer.Play(DeviceSound.Beep);
            this.i3Device.RollRestart += (sender, args) => DeviceSoundPlayer.Play(DeviceSound.Error);

            var devId = device.Identification;
            this.ModelName = devId.Model.ToUpperInvariant();
            this.SerialNumber = devId.SerialNumber.ToUpperInvariant();

            DeviceSoundPlayer.IsMuted = this.Properties.GetBoolValue(PropSystemSoundMuted);            

            LogDispatcher.DoLog("OpenAsync called");
            this.i3Device.OpenAsync(true, false);

        }        

        public void Close()
        {
            
            this.ChangeState(DeviceState.Closing);
            
            if (this.i3Device != null)
            {                
                this.i3Device.AsyncOpenComplete -= this.I3DeviceOnAsyncOpenComplete;
                this.i3Device.AsyncAcquireComplete -= this.I3DeviceOnAsyncAcquireComplete;
                this.i3Device.CaptureBegin -= this.I3DeviceOnCaptureBegin;
                this.i3Device.CaptureEnd -= this.I3DeviceOnCaptureEnd;
                this.i3Device.FrameAvailable -= this.I3DeviceOnFrameAvailable;
                this.i3Device.StatusChange -= this.I3DeviceOnStatusChange;
                this.i3Device.ClearPlaten -= this.I3DeviceOnClearPlaten;

                this.StopCapture();

                try
                {
                    this.i3Device.Close();
                    this.i3Device.Dispose();
                }
                catch (Exception ex)
                {
                    LogDispatcher.DoLog("Error closing I3 device", LogEventLevel.Warning, ex);
                }
                
                this.i3Device = null;
            }
            this.IsOpened = false;

            this.ChangeState(DeviceState.Closed);
        }

        public bool InitializeCapture(IEnumerable<PhysicalHandPart> handParts, CapturePreviewHandler preview, int previewWindowHandle, bool isFlat)
        {
            this.handParts = handParts;
            this.preview = preview;
            this.isFlat = isFlat;
            return true;
        }

        public bool CapturePrint(PrintResolution resolution, Hand printHand, HandPart handPart, HandScanKind scanKind)
        {
            LogDispatcher.DoLog($"CapturePrint {printHand} {handPart} - {scanKind} @{resolution}");
            this.ChangeState(DeviceState.ScanInitialization);
            this.printsToCapture = new List<PrintCaptureQuality>();

            var qualityIndicator = PrintQualityLevel.Good;
            var printType = PrintType.FbiRoll;
            this.OnDeviceSendMessage("", DeviceMessageKind.Information, false);

            if (handPart == HandPart.TwoThumbs)
            {
                printType = PrintType.Type14Slap;
                this.printsToCapture.AddRange(this.handParts.Where(x => x.HandPart == HandPart.Thumb && !x.IsMissing).Select(y => new PrintCaptureQuality(y, qualityIndicator)));
            }
            else if (handPart == HandPart.FourFlats)
            {
                printType = this.isFlat ? PrintType.Type14Slap : PrintType.Fbi4Slap;
                this.printsToCapture.AddRange(this.handParts.Where(x => x.Hand == printHand && x.HandPart != HandPart.Thumb && x.Kind == HandPartKind.Finger && !x.IsMissing).Select(y => new PrintCaptureQuality(y, qualityIndicator)));
            }
            else if (scanKind == HandScanKind.Flat)
            {
                printType = PrintType.Fbi1Slap;
                this.printsToCapture.AddRange(this.handParts.Where(x => x.Hand == printHand && x.HandPart == handPart && !x.IsMissing).Select(y => new PrintCaptureQuality(y, qualityIndicator)));
            }
            else
            {
                this.printsToCapture.AddRange(this.handParts.Where(x => x.Hand == printHand && x.HandPart == handPart && !x.IsMissing).Select(y => new PrintCaptureQuality(y, qualityIndicator)));
            }

            this.currentScanKind = scanKind;

            this.WaitForDeviceReady(
                () =>
                {
                    var logMsg = string.Format($"Acquire called. Status : {this.i3Device.Status}");
                    LogDispatcher.DoLog(logMsg);
                    this.i3Device.AcquireAsync(printType, false);
                    this.OnQualityChanged();

                }); // wait for the previous capture end, else capture fails without warning ...            
            return true;
        }

        public bool StopCapture()
        {
            if (this.isStopping)
            {
                return true;
            }

            LogDispatcher.DoLog("StopCapture called");
            this.ChangeState(DeviceState.Closing);
            this.isStopping = true;

            if (this.IsOpened)
            {
                try
                {
                    Thread.Sleep(250);
                    this.i3Device.CancelOperation();
                    Thread.Sleep(500);
                    //this.Close();
                }
                catch (Exception ex)
                {
                    LogDispatcher.DoLog("StopCapture Error occured", LogEventLevel.Warning, ex);                    
                }
                
                this.OnQualityChanged(true);
                //this.i3Device.SetLedState(Led.SingleGreen, LedState.Off);
                //this.i3Device.SetLedState(Led.SingleRed, LedState.Off);
            }

            this.isStopping = false;
            return true;
        }

        #endregion

        #region sdk methods

        private void I3DeviceOnCaptureBegin(object sender, EventArgs eventArgs)
        {
            LogDispatcher.DoLog("I3DeviceOnCaptureBegin");
            this.OnDeviceSendMessage(string.Empty, DeviceMessageKind.Information, false);            
        }

        private void I3DeviceOnClearPlaten(object sender, ClearPlatenEventArgs e)
        {
            if (this.waitForPlaten)
            {
                return;
            }
            LogDispatcher.DoLog("I3DeviceOnClearPlaten");

            this.OnDeviceSendMessage(CommonText.DeviceClearPlaten, DeviceMessageKind.Information, false);
            this.waitForPlaten = true;
            //DeviceSoundPlayer.Play(DeviceSound.Error);
        }

        private void WaitForDeviceReady(Action actionToExecute)
        {
            LogDispatcher.DoLog("WaitForDeviceReadyCalled");
            var wait = new BackgroundWorker();
            wait.DoWork += (sender, args) =>
                {
                    Console.WriteLine("{0} Waiting. {1}", DateTime.Now, this.i3Device.Status);
                    var start = DateTime.Now;
                    while (this.waitForPlaten)
                    {
                        this.waitForPlaten = false;
                        Thread.Sleep(200);
                    }

                    while (this.i3Device.Status != DeviceStatus.OpenFull)
                    {
                        Thread.Sleep(200);
                        var elapsed = DateTime.Now.Subtract(start);
                        if (elapsed.TotalMilliseconds > WaitMaxDelay)
                        {
                            LogDispatcher.DoLog("Wait timeout", LogEventLevel.Error);
                            this.OnDeviceSendMessage("Wait timeout !", DeviceMessageKind.Error, true);
                            return;
                        }
                    }
                    Console.WriteLine("{0} Waiting done. {1}", DateTime.Now, this.i3Device.Status);
                };
            wait.RunWorkerCompleted += (sender, args) => actionToExecute();
            wait.RunWorkerAsync();


            //var waitTask = new Func<Task>(
            //    async () =>
            //    {
            //        if (this.i3Device == null)
            //        {
            //            return;
            //        }
                    
            //        Console.WriteLine("{0} Waiting. {1}", DateTime.Now, this.i3Device.Status);
            //        var start = DateTime.Now;
            //        while (this.i3Device.Status != DeviceStatus.OpenFull)
            //        {
            //            await Task.Delay(200);
            //            var elapsed = DateTime.Now.Subtract(start);
            //            if (elapsed.TotalMilliseconds > WaitMaxDelay)
            //            {
            //                this.OnDeviceSendMessage("Wait timeout !", true);
            //                return;
            //            }
            //        }
            //        Console.WriteLine("{0} Waiting done. {1}", DateTime.Now, this.i3Device.Status);
            //        //var start = DateTime.Now;
            //        //while (this.acquisitionInProgress)
            //        //{
            //        //    await Task.Delay(200);
            //        //    var elapsed = DateTime.Now.Subtract(start);
            //        //    if (elapsed.TotalMilliseconds > WaitMaxDelay)
            //        //    {
            //        //        this.OnDeviceSendMessage("Wait timeout !", true);
            //        //        return;
            //        //    }
            //        //}
            //    });
            //waitTask.Invoke();
        }

        private void I3DeviceOnStatusChange(object sender, StatusChangeEventArgs e)
        {
            LogDispatcher.DoLog(string.Format("I3DeviceOnStatusChange. Status changed from {1} to {0}", e.Status, e.PreviousStatus));

            var msg = string.Empty;
            bool endCapture = false;
            switch (e.Status)
            {
                case DeviceStatus.Calibrating:
                case DeviceStatus.Initializing:
                    msg = CommonText.DeviceInitialization;
                    break;

                case DeviceStatus.PreparingCapture:
                    msg = CommonText.CaptureStepCapturePreparation;
                    break;

                case DeviceStatus.Capturing:
                case DeviceStatus.Closed:
                case DeviceStatus.Closing:
                case DeviceStatus.Disconnected:
                case DeviceStatus.OpenFull:
                case DeviceStatus.OpenPartial:
                case DeviceStatus.Testing:
                case DeviceStatus.Unknown:
                    msg = string.Empty;
                    break;

                case DeviceStatus.Error:                    
                    msg = this.i3Device.OpenException?.Message ?? CommonText.DeviceInternalError;

                    if (msg.Contains("platen"))
                    {
                        msg = CommonText.DeviceClearPlaten;
                    }
                    else if (!this.deviceOpened)
                    {
                        msg += CommonText.DeviceCouldRequireUsb2;
                    }
                    DeviceSoundPlayer.Play(DeviceSound.Error);
                    endCapture = true;
                    break;
            }

            if (!string.IsNullOrEmpty(msg))
            {
                this.OnDeviceSendMessage(msg, endCapture ? DeviceMessageKind.Error : DeviceMessageKind.Information, endCapture);
            }                                             
        }

        private void I3DeviceOnFrameAvailable(object sender, FrameAvailableEventArgs e)
        {
            if (e.DataPresent != DataPresent.No && e.Image != null)
            {
                this.ChangeState(DeviceState.Scanning);
                e.DisposeImage = false;

                this.preview((Bitmap)e.Image);

            }
        }

        private void I3DeviceOnCaptureEnd(object sender, CaptureEndEventArgs e)
        {
            LogDispatcher.DoLog("I3DeviceOnCaptureEnd"); 
            // image will be freed by calling thread, so a copy is needed !           
            Bitmap img = null;
            if (e.Image != null)
            {
                img = (Bitmap)e.Image.Clone();                
                //img.SetResolution((float)this.FingerResolution, (float)this.FingerResolution);
                //img.Save("C:\\" + DateTime.Now.ToString("HHmmss") + ".bmp" );
            }

            // calling thread must not be blocked
            var work = new BackgroundWorker();
            work.DoWork += (o, args) =>                
                {
                    this.ChangeState(DeviceState.Opened);

                    this.preview(null);
                    this.OnQualityChanged(true);

                    this.CloseLeds();

                    DeviceSoundPlayer.Play(DeviceSound.Beep);

                    if (img != null)
                    {
                        this.OnPrintCaptured(this.FingerResolution, img);
                    }
                };
            work.RunWorkerAsync();
        }

        private void CloseLeds()
        {
            if (this.i3Device != null)
            {
                this.i3Device.SetLedState(Led.SingleGreen, LedState.Off);
                this.i3Device.SetLedState(Led.SingleRed, LedState.Off);
            }
        }

        private void I3DeviceOnAsyncAcquireComplete(object sender, AsyncAcquireCompleteEventArgs e)
        {
            //this.i3Device.PerformTrigger();   
            LogDispatcher.DoLog("I3DeviceOnAsyncAcquireComplete");
        }

        private void I3DeviceOnAsyncOpenComplete(object sender, AsyncOpenCompleteEventArgs e)
        {
            LogDispatcher.DoLog("I3DeviceOnAsyncOpenComplete");
            string msg = CommonText.DeviceInternalError;
            if (e.Exception != null)
            {                
                LogDispatcher.DoLog("I3DeviceOnAsyncOpenComplete Error", LogEventLevel.Error, e.Exception);                
                this.ChangeState(DeviceState.Closed);
                if (e.Exception.Message.Contains("platen"))
                {
                    msg = CommonText.DeviceClearPlaten;
                    this.LastException = new SdkException( this.DisplayName, CommonText.CaptureStepDeviceOpen, SdkErrorKind.CaptureSurfaceDirty, e.Exception.Message, false, e.Exception);
                }
                else
                {                    
                    this.LastException = new SdkException(this.DisplayName, CommonText.CaptureStepDeviceOpen, SdkErrorKind.SpecificError, e.Exception.Message + CommonText.DeviceCouldRequireUsb2, false, e.Exception);
                }
                
                this.OnDeviceSendMessage(msg, DeviceMessageKind.Error, true);
                this.OnDeviceOpened(DeviceOpenStatus.ErrorOccured);
                this.Close();
                return;
            }

            if (e.Canceled)
            {
                LogDispatcher.DoLog("I3DeviceOnAsyncOpenComplete Canceled", LogEventLevel.Warning);    
                this.ChangeState(DeviceState.Closed);
                this.OnDeviceOpened(DeviceOpenStatus.ErrorOccured);
                return;
            }

            if (this.i3Device.CanCalibrate())
            {
                // Note : Due to some bug in I3 calibration, it can no longer be done. So we must assume everything is ok.
                LogDispatcher.DoLog("I3DeviceOnAsyncOpenComplete GetCalibrationStatus");
                var calibStatus = this.i3Device.GetCalibrationStatus();
                if (calibStatus.CalibrationRequired)
                {

            //        this.i3Device.AsyncCalibrateComplete += (o, args) =>
            //        {
            //            if (args.Exception == null && !args.Canceled)
            //            {
            //                this.OnDeviceOpened(DeviceOpenStatus.Success);
            //            }
            //            else
            //            {
            //                this.OnDeviceOpened(DeviceOpenStatus.ErrorOccured);
            //            }
            //        };

                    

                    LogDispatcher.DoLog("I3DeviceOnAsyncOpenComplete Calibration required", LogEventLevel.Warning);
            //        //this.OnDeviceOpened(DeviceOpenStatus.ErrorOccured);
            //        this.OnDeviceSendMessage(CommonText.CalibratingDevice, DeviceMessageKind.Information, false);
            //        this.i3Device.CalibrateAsync();
                    
            //        return;
                }
            }            

            this.OnDeviceOpened(DeviceOpenStatus.Success);
            this.deviceOpened = true;
            this.ChangeState(DeviceState.Opened);
        }

        #endregion

        #region event handlers

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

        private void OnDeviceOpened(DeviceOpenStatus status)
        {
            this.DeviceOpened?.Invoke(status);
        }

        private void OnDeviceSendMessage(string text, DeviceMessageKind kind, bool showResumeButton)
        {
            this.DeviceSendMessage?.Invoke(text, kind, showResumeButton);
        }

        private void OnPrintCaptured(PrintResolution resolution, Bitmap img)
        {
            this.PrintCaptured?.Invoke(resolution, img);
        }

        // I3 does not support quality feedback
        private void OnQualityChanged(bool reset = false)
        {
            this.CaptureQualityChanged?.Invoke(!reset ? this.printsToCapture : null);
        }

        #endregion        

    }
}
