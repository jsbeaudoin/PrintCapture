// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuickFlatCapture.cs" company="XL-ID">
//   update text
// </copyright>
// <summary>
//   This drives capture, given prints type to capture and device interface. manages missing prints and quality display
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using XL_ID.Utilities.Log;

namespace PrintsCapture.LivescanWinForm
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using PrintsCapture.Device;
    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Extension;
    using PrintsCapture.Device.Interface;        
    using PrintsCapture.Livescan.ViewModel;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Extension;
    using PrintsCapture.Prints.Language;

    using XL_ID.Utilities.Image;

    using Color = System.Drawing.Color;

    //public delegate void CaptureCompletedHandler();

    public enum AsyncCommands
    {
        None,
        StopScan,
        StartScan,
        ResumeScan,
        CloseDevice,
        CaptureAfterChangeCommand
    }

    /// <summary>
    /// This drives capture order, given prints type to capture and device interface. manages missing prints and quality display
    /// </summary>
    public class WizardCapture : DeviceCaptureDriver
    {
        private BackgroundWorker commandRunner;

        private Queue<AsyncCommands> commandQueue = new Queue<AsyncCommands>();

        private object commandQueueLocker = new object();

        private readonly WizardProcessViewModel viewModel;        

        private List<PrintInfo> captureList;        

        private int captureIndex;        

        private ILivescanDevice livescanDevice;
        private bool endingCapture;

        private bool callbackSet;

        private bool callbackOpenSet;        

        private bool captureStopped;

        public WizardCapture(ICaptureDevice device, PrintList printList, WizardProcessViewModel viewModel)
            : base(device, printList)
        {
            this.viewModel = viewModel;            
            this.livescanDevice = device as ILivescanDevice;
            if (this.livescanDevice != null)
            {
                this.livescanDevice.StateChanged += (sender, state) => 
                    this.viewModel.DeviceStateLabel = state.ToString();
            }
            

            viewModel.CaptureResumed += this.LivePreviewResumeDemanded;
            this.commandRunner = new BackgroundWorker();
            this.commandRunner.DoWork += (sender, args) => this.AsyncCommandRunnerLoop();
        }

        /// <summary>
        /// Capture a single print
        /// </summary>        
        /// <param name="print"></param>
        public override ScanResult CaptureSingle(PrintInfo print)
        {
            this.captureStopped = false;
            if (print.IsEndorsement)
            {
                
                var endorsementFinger = print.PrintList.GetEndorsableFingers().First();
                
                print.EndorsementFinger =
                    print.PrintList.PhysicalParts.SingleOrDefault(x => x.EndorsementIndex == endorsementFinger.Index);
            }                        

            this.captureList = new List<PrintInfo> {print}; // new CaptureOrder(this.PrintList, this.livescanDevice).GetQuickFlatCaptureList(print.PrintList.Rules.IsEndorsementAllowed);
            this.viewModel.RestartButtonVisibility = Visibility.Collapsed;

            this.InitCapture();

            return ScanResult.InProgress;
        }

        public override string GetDeviceSecondaryInfo()
        {
            if (this.CaptureDevice == null)
            {
                return string.Empty;
            }

            var desc = this.livescanDevice.FingerResolution.ToDescription();

            if (this.livescanDevice.Supports(DeviceScanKind.FlatPartialPalm)
                || this.livescanDevice.Supports(DeviceScanKind.FlatCompletePalm))
            {
                desc += @" / " + this.livescanDevice.PalmResolution.ToDescription();
            }

            return desc;
        }

        /// <summary>
        /// Capture all required prints. Async, Does not Start immediately !
        /// </summary>        
        public override ScanResult CaptureAuto()
        {
            this.viewModel.RestartButtonVisibility = Visibility.Collapsed;

            this.SendAsyncCommand(AsyncCommands.StartScan);
            
            // wait for window to be ready !            
            return ScanResult.InProgress;
        }

        public override ScanResult CapturePositions()
        {
            this.viewModel.RestartButtonVisibility = Visibility.Collapsed;

            this.SendAsyncCommand(AsyncCommands.CaptureAfterChangeCommand);

            // wait for window to be ready !            
            return ScanResult.InProgress;
        }

        /// <summary>
        /// Stop the current capture. Async, Does not Stop immediately !
        /// </summary>
        public void StopCapture()
        {
            //this.captureStopped = true;
            //if (this.livescanDevice != null)
            //{
            //    this.livescanDevice.StopCapture();
            //}
            this.SendAsyncCommand(AsyncCommands.StopScan);
        }

        /// <summary>
        /// Close the device. Async, Does not Close immediately !
        /// </summary>
        public void CloseDevice()
        {
            this.SendAsyncCommand(AsyncCommands.CloseDevice);
        }
        

        void SendAsyncCommand(AsyncCommands command)
        {
            lock (commandQueueLocker)
            {
                this.commandQueue.Enqueue(command);
            }

            if (!this.commandRunner.IsBusy)
            {
                this.commandRunner.RunWorkerAsync();
            }
        }

        void AsyncCommandRunnerLoop()
        {            
            while (true)
            {
                AsyncCommands current;
                lock (commandQueueLocker)
                {
                    if (this.commandQueue.Count == 0)
                    {
                        return;
                    }
                    current = this.commandQueue.Dequeue();
                    if (current == AsyncCommands.None)
                    {
                        return;
                    }
                }

                // Process command when device is not busy !
                this.WaitForDeviceState();                

                var targetState = DeviceState.Undefined;
                Action actionToExecute = null;

                switch (current)
                {
                    case AsyncCommands.StartScan:
                        targetState = DeviceState.Scanning;
                        actionToExecute = this.CaptureCommand;                        
                        break;

                    case AsyncCommands.CaptureAfterChangeCommand:
                        targetState = DeviceState.Scanning;
                        actionToExecute = this.CaptureAfterChangeCommand;
                        break;

                    case AsyncCommands.ResumeScan:
                        targetState = DeviceState.Scanning;
                        actionToExecute = this.CaptureResume;
                        break;

                    case AsyncCommands.StopScan:
                        targetState = DeviceState.Ready;
                        actionToExecute = this.StopDeviceCaptureCommand;
                        break;

                    case AsyncCommands.CloseDevice:
                        targetState = DeviceState.Closed;
                        actionToExecute = this.CloseDeviceCommand;
                        break;
                }

                if (actionToExecute == null)
                {
                    LogDispatcher.DoLog($"Command returned null action : {current}. Wrong dll version ??", LogEventLevel.Error);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(actionToExecute);

                    this.WaitForDeviceState(targetState);
                }
                

                // Always sleep a tenth of second between commands
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Wait for a given state or when the device won't be busy. max wait : 2 seconds
        /// </summary>
        /// <param name="targetState"></param>
        private void WaitForDeviceState(DeviceState targetState = DeviceState.Undefined)
        {
            int cpt = 0;

            while (cpt < 10 && (
                this.livescanDevice.State != targetState ||
                ( targetState == DeviceState.Undefined && this.livescanDevice.State.IsDeviceBusy()))
                )
            {
                Thread.Sleep(200);
                cpt++;
            }



            if (targetState == DeviceState.Undefined)
            {
                if (this.livescanDevice.State.IsDeviceBusy())
                {
                    LogDispatcher.DoLog($"Command wanted available device state. Could not obtain it. Current state is {this.livescanDevice.State}.", LogEventLevel.Warning);
                }
            }
            else
            {
                if (this.livescanDevice.State != targetState && targetState != DeviceState.Undefined)
                {
                    LogDispatcher.DoLog($"Command wanted device state of {targetState}. Could not obtain it. Current state is {this.livescanDevice.State}.", LogEventLevel.Warning);
                }
            }

        }

        private void CloseDeviceCommand()
        {
            this.captureStopped = true;
            this.livescanDevice?.Close();
        }

        private void StopDeviceCaptureCommand()
        {
            this.captureStopped = true;
            this.livescanDevice?.StopCapture();
        }

        private void CaptureAfterChangeCommand()
        {
            this.DoCapture(true);
        }

        private void CaptureCommand()
        {
            this.DoCapture(false);            
        }

        private void CaptureResume()
        {
            this.DoCapture(true);            
        }

        private void DoCapture(bool resumeMode)
        {
            this.captureStopped = false;
            // 1 --> Get Order            
            this.captureList = new CaptureOrder(this.PrintList, this.livescanDevice, resumeMode).GetCaptureList();

            // no order means that print have been edited without error, or overridden.
            if (this.captureList.Count == 0)
            {
                this.captureStopped = true;
                this.TriggerCaptureCompleted(false);
                return;
            }

            this.InitCapture();
        }

        private void InitCapture()
        {            
            this.captureIndex = -1;

            // set capture count to 0 for each print !
            foreach (var printInfo in this.captureList)
            {
                printInfo.CaptureCount = 0;
            }

            // Prepare Sdk and Device            
            this.viewModel.Instructions = CommonText.DeviceInitialization;
            // CommonText.InstructionInitialization;            
            //this.livePreviewData.Resume += this.LivePreviewResumeDemanded;
            this.viewModel.ResetQuality();

            // Based on Standard capture, But in quick capture, Window is already ready !
            this.PreviewWindowReady();

            // Closed += (sender, e) => this.CaptureEnded();            
        }        

        void LivePreviewResumeDemanded(object sender, EventArgs e)
        {
            if (this.captureList.Count < 1)
            {
                return;
            }

            this.viewModel.Instructions = string.Empty;
            
            if (this.captureIndex < 0 || this.captureIndex > this.captureList.Count)
            {
                // Start process from scratch !
                this.PreviewWindowReady();
                return;
            }

            var currentPrint = this.captureList[this.captureIndex];
            this.CapturePrint(currentPrint);
        }        

        private void PreviewWindowReady()
        {
            string msg = string.Empty;

            try
            {
                msg = CommonText.CaptureStepSdk;
                var sdk = this.livescanDevice.Sdk;
                if (!sdk.IsOpened)
                {
                    if (!sdk.Open())
                    {
                        if (sdk.LastException != null)
                        {
                            msg += Environment.NewLine + sdk.LastException.Message;
                        }

                        this.SendMessage(string.Format(CommonText.FailCommand, msg), true);
                        return;
                    }
                }                                                                             

                if (this.livescanDevice.IsOpened)
                {
                    // already opened, proceed
                    this.DeviceOpened(DeviceOpenStatus.Success);                    
                    return;
                }

                // set device events
                if (!this.callbackOpenSet)
                {
                    this.livescanDevice.DeviceOpened += this.DeviceOpenedFromThread;
                    this.livescanDevice.DeviceSendMessage += this.DeviceMessage;
                    this.callbackOpenSet = true;
                }

                msg = CommonText.CaptureStepDeviceOpen;
                Task.Factory.StartNew(this.OpenDevice);
            }
            catch (Exception ex)
            {
                this.viewModel.Instructions = string.Format(CommonText.FailCommand, msg + @" / " + ex.Message);
            }
        }        

        private void OpenDevice()
        {
            try
            {
                this.livescanDevice.Open();
                //PrintModificationDispatcher.AddWatch(PrintModified);
            }
            catch (Exception ex)
            {
                LogDispatcher.DoLog("Livescan-OpenDevice error", LogEventLevel.Error, ex);                
                var msg = string.Empty;
                if (this.livescanDevice.LastException != null)
                {
                    msg = Environment.NewLine + this.livescanDevice.LastException.Message;
                }
                this.SendMessage(string.Format(CommonText.FailCommand, CommonText.CaptureStepDeviceOpen) + msg, true);
            }
        }

        private void DeviceOpenedFromThread(DeviceOpenStatus openStatus)
        {
            var invoked = new Action(() => this.DeviceOpened(openStatus));
            Application.Current.Dispatcher.Invoke(invoked);
        }

        private void DeviceOpened(DeviceOpenStatus openStatus)
        {                        
            string msg = CommonText.CaptureStepDeviceStatus;
            if (openStatus != DeviceOpenStatus.Success)
            {                
                var detail = openStatus == DeviceOpenStatus.DeviceNotFound
                    ? CommonText.DeviceNotFound
                    : CommonText.WizardDeviceInternalError;                

                LogDispatcher.DoLog(string.Format(CommonText.FailCommand, msg) + Environment.NewLine + detail, LogEventLevel.Warning);
                
                this.SendMessage(detail, true);
                return;

            }

            if (!this.callbackSet)
            {                
                this.livescanDevice.PrintCaptured += this.DeviceCaptureDone;
                this.livescanDevice.CaptureQualityChanged += this.DeviceQualityCallback;
                this.callbackSet = true;
            }

            msg = CommonText.CaptureStepCapturePreparation;
            // this.viewModel.PreviewHandle
            if (!this.livescanDevice.InitializeCapture(this.PrintList.Prints.Select(x => x.PhysicalPart).Distinct().ToList(), this.DisplayHandler, 0, true))
            {
                this.SendMessage(string.Format(CommonText.FailCommand, msg), true);
                return;
            }

            this.TriggerDeviceInitialized();

            if (this.captureStopped)
            {
                return;
            }

            // Start Capture
            this.CaptureNextPrint();
        }

        private void DisplayHandler(Bitmap img)
        {
            var action = new Action(
                () =>
                {
                    // write to Writeable Bitmap !
                    var im = this.viewModel.DisplayImage;
                    WriteableBitmap wbmp;

                    if (img == null)
                    {
                        im.Source = null;
                        return;
                    }

                    if (im.Source == null || Math.Abs(im.Source.Width - img.Width) > 0.1
                        || Math.Abs(im.Source.Height - img.Height) > 0.1)
                    {
                        this.viewModel.Win32Visibility = Visibility.Hidden;
                        
                        var pal =
                            new BitmapPalette(
                                img.Palette.Entries.Select(x => System.Windows.Media.Color.FromArgb(255, x.R, x.G, x.B))
                                    .ToList());
                        wbmp = new WriteableBitmap(
                            img.Width,
                            img.Height,
                            img.HorizontalResolution,
                            img.VerticalResolution,
                            PixelFormats.Indexed8,
                            pal);
                    }
                    else
                    {
                        wbmp = (WriteableBitmap)im.Source;
                        im.Source = null;
                    }

                    this.WriteIntoWpfBitmap(wbmp, img);

                    im.Source = wbmp;
                });

            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(action);  
            }
            
        }

        private void WriteIntoWpfBitmap(WriteableBitmap wbmp, Bitmap img)
        {

            if (img == null)
            {
                img = new Bitmap(wbmp.PixelWidth, wbmp.PixelHeight);
                using (var g = Graphics.FromImage(img))
                {
                    g.Clear(Color.White);
                }
            }

            BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadOnly, img.PixelFormat);

            try
            {
                var ptr0 = data.Scan0;
                var stride = data.Stride;
                if (stride < 0)
                {
                    stride = Math.Abs(stride);
                    ptr0 = new IntPtr(ptr0.ToInt32() - (stride * (img.Height - 1)));

                    var arraySize = stride * img.Height;
                    var pix = new byte[arraySize];
                    var height1 = img.Height - 1;

                    for(var row=0; row <= height1; row++)
                    {
                        var rowAdr = new IntPtr(ptr0.ToInt32() + (height1 - row) * stride);
                        Marshal.Copy(rowAdr, pix, row * stride, stride);
                    }

                    // Marshal.Copy(ptr0, pix, 0, arraySize);
                    // Array.Reverse(pix);
                    wbmp.WritePixels(new Int32Rect(0, 0, img.Width, img.Height), pix, stride, 0);

                }
                else
                {
                    wbmp.WritePixels(
                    new Int32Rect(0, 0, img.Width, img.Height),
                    ptr0,
                    stride * img.Height,
                    stride);
                }
                
            }
            finally
            {
                img.UnlockBits(data);
            }                   
        }

        private void CapturePrint(PrintInfo print)
        {            
            //this.livePreviewData.CaptureStopped = false;

            // 3 --> Capture   
            this.viewModel.ResetQuality();

            this.viewModel.Instructions = PrintList.GetInstruction(print); // GetName(print); // + Environment.NewLine + CommonText.ResolutionLabel + print.Resolution.ToDpi(); // this.livePreviewData.CurrentPrint.PrintName;
            this.viewModel.InstructionHandImage = this.viewModel.GetHandImage(print);
            if (this.viewModel.IsLoginMode) this.viewModel.InstructionHandImage = this.viewModel.GetHandImageEmpty();

            this.SendMessage(string.Empty, false);
            print.Reset();
            PrintModificationDispatcher.PrintModified(print);

            try
            {
                bool scanSuccess;
                if (print.HandPart == HandPart.Endorsement)
                {
                    scanSuccess = this.livescanDevice.CapturePrint(print.Resolution, print.EndorsementFinger.Hand, print.EndorsementFinger.HandPart, print.ScanKind);
                }
                else
                {
                    scanSuccess = this.livescanDevice.CapturePrint(print.Resolution, print.Hand, print.HandPart, print.ScanKind);
                }

                if (!scanSuccess)
                {
                    var msg = livescanDevice.LastException?.Message ?? CommonText.UnknownError;
                    throw new ApplicationException(msg, this.livescanDevice.LastException);
                }
            }
            catch (Exception ex)
            {
                this.DeviceMessage(CommonText.CaptureFailure + Environment.NewLine + ex.Message, DeviceMessageKind.Error,  true);                
            }
        }

        private void SyncDeviceCaptureDone(PrintResolution resolution, Bitmap printImage)
        {
            // remove last preview image
            // this.livePreview.LivePreviewImage.Source = null;

            if (printImage != null)
            {                
                var print = this.captureList[this.captureIndex];
                print.CaptureCount += 1;
                
                try
                {
                    //printImage.Save("C:\\TestI3.bmp", ImageFormat.Bmp);
                    var cropped = ImageUtilities.AutoCropAndCenter(printImage, Color.White, print.PrintList.Rules.CropTolerance,
                            Color.White, print.CaptureSize(this.PrintList.Rules.IsFlatCaptureMode));
                    cropped.SetResolution(printImage.HorizontalResolution, printImage.VerticalResolution);
                    print.Image = cropped;
                    //var saveAction =
                    //    new Action(
                    //        () =>
                    //            print.ImageForProcessing.Save(
                    //                @"D:\tmp\" + this.captureIndex.ToString() + "-A.bmp",
                    //                ImageFormat.Bmp));
                    //Application.Current.Dispatcher.Invoke(saveAction);

                }
                catch (Exception ex)
                {
                    LogDispatcher.DoLog("Could not process image", LogEventLevel.Error, ex);                    
                    this.DeviceMessage(CommonText.CannotProcessImage, DeviceMessageKind.Error,  true);
                    return;
                }
                
                //print.Image.Save(@"D:\tmp\" + this.captureIndex.ToString() + "-B.bmp");                

                print.Resolution = resolution;
                print.ProcessStatus = PrintProcessStatus.InProcess;

                PrintModificationDispatcher.PrintModified(print);
                this.TriggerPrintCaptured(print, false);                 

                this.CaptureNextPrint();
            }
            else
            {
                var msg = CommonText.CaptureFailure;
               
                msg += Environment.NewLine + CommonText.NoImageReturned;
                
                if (this.livescanDevice.LastException != null)
                {
                    msg += @": " + this.livescanDevice.LastException.Message;
                }

                this.DeviceMessage(msg, DeviceMessageKind.Error,  true);
            }
        }        

        private void SendMessage(string text, bool stopCapture)
        {
            if (this.livescanDevice.LastException != null)
            {
                text += @"\n" + this.livescanDevice.LastException.Message;
            }

            this.DeviceMessage(text, stopCapture ? DeviceMessageKind.Error : DeviceMessageKind.Information, stopCapture);
        }

        private void DeviceMessage(string text, DeviceMessageKind kind, bool showResumeButton)
        {
            LogDispatcher.DoLog($"DeviceMessage - {kind}. ShowResume ? {showResumeButton}. \nText: {text}", kind == DeviceMessageKind.Error ? LogEventLevel.Warning : LogEventLevel.Info);
            var ex = this.livescanDevice.LastException;
            if (ex != null)
            {
                LogDispatcher.DoLog($"DeviceMessage - Device-LastException: {ex.GetType()} - {ex.Message}", LogEventLevel.Warning);
            }            
            
            if (ex != null && ex.ErrorKind == SdkErrorKind.CaptureSurfaceDirty)
            {
                text = CommonText.WizardCleanPlaten;
            }
            else if (ex != null)
            {
                text = CommonText.WizardDeviceInternalError;
            }
            else if (kind == DeviceMessageKind.Error && String.IsNullOrEmpty(text))
            {
                text = CommonText.WizardDeviceInternalError;
            }

            var displayMessage = new Action(
                () =>
                    {
                        this.viewModel.DeviceMessage = text;
                        this.viewModel.ResumeButtonVisibility = showResumeButton ? Visibility.Visible : Visibility.Hidden;                                                
                    });

            Application.Current.Dispatcher.Invoke(displayMessage);
        }

        private void DeviceQualityCallback(IEnumerable<PrintCaptureQuality> qualityList)
        {                        
            var invoked = new Action(() => this.viewModel.QualityChanged(qualityList));
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(invoked);
            }
            
        }        

        private void DeviceCaptureDone(PrintResolution resolution, Bitmap printImage)
        {
            var invoked =
                new Action(() => this.SyncDeviceCaptureDone(resolution, printImage));
            Application.Current.Dispatcher.Invoke(invoked);
        }                               

        /// <summary>
        /// Stops the device capture. Check set status.
        /// Called when all prints of the sets have been captured.
        /// </summary>
        private void CaptureEnded()
        {            
            if (this.endingCapture)
            {
                return;
            }

            this.endingCapture = true;
            
            this.viewModel.ResetQuality();

            if (this.livescanDevice.IsOpened)
            {
                this.livescanDevice.StopCapture();
            }

            this.viewModel.RestartButtonVisibility = Visibility.Visible;

            var hasError = !this.PrintList.CheckAllStatuses(false);
            if (!hasError)
            {                
                if (this.PrintList.IsEndorsementEmpty() && this.PrintList.Rules.IsEndorsementAllowed)
                {
                    this.viewModel.Instructions = CommonText.EndorsementFingerRequired;
                }
                else
                {
                    this.viewModel.Instructions = CommonText.CaptureCompleted;                                        
                }
                
            }
            else
            {
                this.viewModel.Instructions = CommonText.VerifyPrintsSomeNotReady;
            }
            
            this.viewModel.ResetQuality();
            //this.viewModel.Instructions = string.Empty;
            this.DisplayHandler(null);
            

            //this.SendMessage(CommonText.CaptureCompleted, false);
            this.endingCapture = false;


            this.TriggerCaptureCompleted(hasError);            

                        
        }                

        private void CaptureNextPrint(int fixedIndex = -1)
        {
            var newIndex = fixedIndex == -1 ? this.captureIndex + 1 : fixedIndex;

            if (newIndex >= this.captureList.Count)
            {
                this.CaptureEnded();
                return;
            }

            if (newIndex < 0)
            {
                throw new ApplicationException(CommonText.NegativePrintIndexError);
            }

            this.captureIndex = newIndex;
            var currentPrint = this.captureList[this.captureIndex];

            this.CapturePrint(currentPrint);
        }        

        public override void ConfigureDefaultValue(List<ICaptureSdk> sdkList, string selectedKey)
        {
            var deviceList = new List<ILivescanDevice>();

            sdkList.ForEach(
                x => x.SupportedDeviceList.ToList().ForEach(dev => deviceList.Add(dev as ILivescanDevice)));

            // build viewModel            
            var selected = deviceList.FirstOrDefault(x => x.InternalKey == selectedKey);

            selected.FingerResolution = PrintResolution.Dpi500;            
        }

        public override bool? Configure(DeviceConfigurationViewModel vm)
        {
            throw new NotImplementedException();
        }

        public override bool? DoOperation(PrintInfo print, PrintZoomAction op)
        {
            if (op != PrintZoomAction.Scan)
            {
                return false;
            }

            this.CaptureSingle(print);
            return true;
        }         
    }
}
