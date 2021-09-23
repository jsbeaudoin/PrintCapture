// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LivescanCapture.cs" company="Solutions XL-ID inc">
//   update text
// </copyright>
// <summary>
//   This drives capture, given prints type to capture and device interface. manages missing prints and quality display
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.ComponentModel;
using System.Threading;
using PrintsCapture.Livescan.Class;
using XL_ID.Utilities.Log;
using XL_ID.Utilities.Wpf.ViewModel;

namespace PrintsCapture.Livescan
{
    using System;
    using System.Collections.Generic;    
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using PrintsCapture.Device;
    using PrintsCapture.Device.DataLayer;
    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Extension;
    using PrintsCapture.Device.Interface;        
    using PrintsCapture.Livescan.ViewModel;
    using PrintsCapture.Livescan.View;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Extension;
    using PrintsCapture.Prints.Language;
    using PrintsCapture.Prints.ViewModel;

    using XL_ID.Utilities.Image;
    using XL_ID.Utilities.XML;    

    using Color = System.Drawing.Color;

    public delegate void CaptureCompletedHandler();

    /// <summary>
    /// This drives capture order, given prints type to capture and device interface. manages missing prints and quality display
    /// </summary>
    public class LivescanCapture : DeviceCaptureDriver
    {             
        private LivePreviewViewModel livePreviewData;

        private PreviewWindow livePreviewWindow;

        private List<PrintInfo> captureList;        

        private int captureIndex;        

        private ILivescanDevice livescanDevice;
        private bool endingCapture;

        //private bool aquisitionInProgress = 0;

        private bool callbackSet;

        private bool callbackOpenSet;       

        private bool rescanPending;

        public LivescanCapture(ICaptureDevice device, PrintList printList) : base(device, printList)
        {            
            this.livescanDevice = device as ILivescanDevice;
            if (this.livescanDevice != null && this.livePreviewData != null)
            {
                this.livescanDevice.StateChanged +=
                (sender, state) => this.livePreviewData.DeviceStateLabel = state.ToString();
            }
            
        }

        /// <summary>
        /// Capture a single print
        /// </summary>        
        /// <param name="print"></param>
        public override ScanResult CaptureSingle(PrintInfo print)
        {
            if (print.IsEndorsement)
            {
                var vm = new EndorsementSelectViewModel();
                vm.FingerList = print.PrintList.GetEndorsableFingers();
                vm.SelectedFinger = vm.FingerList.First();

                var win = new EndorsementSelectWindow(vm);
                if (win.ShowDialog() != true || vm.SelectedFinger == null)
                {
                    return ScanResult.Canceled;
                }

                print.EndorsementFinger =
                    print.PrintList.PhysicalParts.SingleOrDefault(x => x.EndorsementIndex == vm.SelectedFinger.Index);
            }

            PrintResolution resolution = print.PhysicalPart.Kind == HandPartKind.Palm ? this.livescanDevice.PalmResolution : this.livescanDevice.FingerResolution;

            this.captureList = new CaptureOrder(this.PrintList, this.livescanDevice, false).GetCaptureList(print, resolution);                        

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
        /// Capture all required prints
        /// </summary>        
        public override ScanResult CaptureAuto()
        {                                               
            // 1 --> Get Order            
            this.captureList = new CaptureOrder(this.PrintList, this.livescanDevice, false).GetCaptureList();            

            this.InitCapture();
            
            // wait for window to be ready !            
            return ScanResult.InProgress;
        }

        /// <summary>
        /// Captures all prints with no image
        /// </summary>
        /// <returns></returns>
        public override ScanResult CapturePositions()
        {
            // 1 --> Get Order            
            this.captureList = new CaptureOrder(this.PrintList, this.livescanDevice, true).GetCaptureList();

            this.InitCapture();

            // wait for window to be ready !            
            return ScanResult.InProgress;
        }

        public void CloseDevice()
        {
            if (this.livescanDevice != null)
            {
                this.livescanDevice.Close();
                this.livescanDevice = null;
            }
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
            this.livePreviewData = new LivePreviewViewModel { KeepWindowOpen = this.captureList.Count < 2 };
            this.livePreviewData.DeviceMessage = CommonText.DeviceInitialization;
            this.livePreviewData.ScanInstructionText = CommonText.InstructionInitialization;            
            this.livePreviewData.Resume += this.LivePreviewResumeDemanded;
            this.livePreviewData.ScanPreviousPrint += this.LivePreviewOnRescanDemanded;
            this.livePreviewData.Win32HandleCreated += this.PreviewWindowReady;

            this.livePreviewWindow = new PreviewWindow(this.livePreviewData);
            this.livePreviewWindow.Closed += (sender, e) => this.CaptureEnded(true);
            this.livescanDevice.StateChanged += LivescanDeviceOnStateChanged;
            // Show LivePreview.  Flows to PreviewWindowReady after

            var bw = new BackgroundWorker();
            bw.DoWork += (sender, args) =>
            {
                Thread.Sleep(200);
                Application.Current.Dispatcher.Invoke((Action) (() => this.livePreviewWindow.ShowDialog()));
            };

            bw.RunWorkerAsync();
        }

        private void LivescanDeviceOnStateChanged(ICaptureDevice sender, DeviceState newState)
        {
            if (newState == DeviceState.Ready && this.rescanPending)
            {
                this.rescanPending = false;
                if (Application.Current != null && Application.Current.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(this.StartRescan));
                }                
            }
        }

        private void LivePreviewOnRescanDemanded(object sender, EventArgs eventArgs)
        {
            this.livePreviewWindow.ResetQuality();

            if (this.livescanDevice.State == DeviceState.Ready)
            {
                this.rescanPending = false;
                Application.Current.Dispatcher.Invoke(new Action(this.StartRescan));
            }
            else
            {
                this.rescanPending = true;
                this.livePreviewData.ScanInstructionText = CommonText.StoppingCapture;
                this.livescanDevice.StopCapture(); 
            }
               
                        
            // when device is ready, the rescan will start
        }

        void StartRescan()
        {
            var newCaptureIndex = this.captureIndex - 1;

            if (newCaptureIndex < 0 || newCaptureIndex > this.captureList.Count)
            {
                newCaptureIndex = 0;
            }
            var printRescan = this.captureList[this.captureIndex];

            printRescan.Reset();

            PrintModificationDispatcher.PrintModified(printRescan);

            //var currentPrint = this.captureList[newCaptureIndex];
            this.CaptureNextPrint(newCaptureIndex); 
        }

        void LivePreviewResumeDemanded(object sender, EventArgs e)
        {
            if (this.captureList.Count < 1)
            {
                return;
            }

            this.livePreviewData.CaptureStopped = false;
            this.livePreviewData.DeviceMessage = string.Empty;
            this.livePreviewData.ScanInstructionText = string.Empty;

            if (this.captureIndex < 0 || this.captureIndex > this.captureList.Count)
            {
                // Start process from scratch !
                this.PreviewWindowReady(this, EventArgs.Empty);
                return;
            }

            var currentPrint = this.captureList[this.captureIndex];
            this.CapturePrint(currentPrint);
        }        

        private void PreviewWindowReady(object sender, EventArgs e)
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
                        //MessageBox.Show(sdk.LastException.Message + Environment.NewLine + sdk.LastException.StackTrace, "PreviewWindowReady");
                        //MessageBox.Show($"Device Status : {this.livescanDevice.IsOpened}", "PreviewWindowReady");

                        this.DeviceMessage(string.Format(CommonText.FailCommand, msg), DeviceMessageKind.Error,  true);
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
                this.livePreviewData.DeviceMessage = string.Format(CommonText.FailCommand, msg + @" / " + ex.Message);
            }
        }        

        private void OpenDevice()
        {
            try
            {
                this.livescanDevice.Open();
                PrintModificationDispatcher.AddWatch(PrintModified);
            }
            catch (Exception ex)
            {
                LogDispatcher.DoLog("Livescan-OpenDeviceError", LogEventLevel.Error, ex);                
                var msg = string.Empty;
                if (this.livescanDevice.LastException != null)
                {
                    msg = Environment.NewLine + this.livescanDevice.LastException.Message;
                }
                this.DeviceMessage(string.Format(CommonText.FailCommand, CommonText.CaptureStepDeviceOpen) + msg, DeviceMessageKind.Error,  true);
            }
        }

        private void DeviceOpenedFromThread(DeviceOpenStatus openStatus)
        {
            var invoked = new Action(() => this.DeviceOpened(openStatus));
            Application.Current.Dispatcher.Invoke(invoked);
        }

        private void DeviceOpened(DeviceOpenStatus openStatus)
        {            

            if (this.livePreviewWindow == null)
            {
                // form was closed before opening was done !
                return;
            }

            string msg = CommonText.CaptureStepDeviceStatus;
            if (openStatus != DeviceOpenStatus.Success)
            {
                var detail = openStatus == DeviceOpenStatus.DeviceNotFound
                    ? CommonText.DeviceNotFound
                    : CommonText.DeviceInternalError;
                this.DeviceMessage(string.Format(CommonText.FailCommand, msg) + Environment.NewLine + detail, DeviceMessageKind.Error,  true);
                return;
            }

            if (!this.callbackSet)
            {                
                this.livescanDevice.PrintCaptured += this.DeviceCaptureDone;
                this.livescanDevice.CaptureQualityChanged += this.DeviceQualityCallback;
                this.callbackSet = true;
            }

            msg = CommonText.CaptureStepCapturePreparation;
            var isFlat = this.PrintList.Rules.IsFlatCaptureMode;
            if (!this.livescanDevice.InitializeCapture(this.PrintList.Prints.Select(x => x.PhysicalPart).Distinct().ToList(), this.DisplayHandler, this.livePreviewWindow.GetPreviewHandle(), isFlat))
            {
                this.DeviceMessage(string.Format(CommonText.FailCommand, msg), DeviceMessageKind.Error,  true);
                return;
            }

            this.TriggerDeviceInitialized();

            // Start Capture
            this.CaptureNextPrint();
        }

        private void DisplayHandler(Bitmap img)
        {
            var action = new Action(
                () =>
                {
                    // write to Writeable Bitmap !
                    var im = this.livePreviewWindow.DisplayImage;
                    WriteableBitmap wbmp;

                    if (img == null)
                    {
                        im.Source = null;
                        return;
                    }

                    if (im.Source == null || Math.Abs(im.Source.Width - img.Width) > 0.1
                        || Math.Abs(im.Source.Height - img.Height) > 0.1)
                    {
                        this.livePreviewWindow.Win32Window.Visibility = Visibility.Hidden;
                        
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

            if (Application.Current == null || Application.Current.Dispatcher == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(action);
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
            this.livePreviewData.CaptureStopped = false;

            // 3 --> Capture   
            this.livePreviewWindow.ResetQuality();
            this.DeviceMessage(string.Empty, DeviceMessageKind.Information, false);
            
            //this.livePreviewData.CurrentPrint = new PrintElement(print.Print) { Tag = "Current" };
            this.livePreviewData.PreviousPrint = this.GetPreviousPrint();
            var printName = PrintList.GetName(print);

            if (print.EndorsementFinger != null)
            {
                this.livePreviewData.ScanInstructionText = printName + Environment.NewLine
                                                           + PrintList.GetName(print.EndorsementFinger);
            }
            else if (this.PrintList.Rules.CaptureGroup == PrintCaptureGroup.OneFingerOnly && ! string.IsNullOrEmpty(this.PrintList.Rules.Labels.SingleFingerCapturePrompt))
            {
                this.livePreviewData.ScanInstructionText = this.PrintList.Rules.Labels.SingleFingerCapturePrompt;
            }
            else {
                
                this.livePreviewData.ScanInstructionText = printName;
            }
            
            
            // this.livePreviewData.CurrentPrint.PrintName;
            // + Environment.NewLine + CommonText.ResolutionLabel + print.Resolution.ToDpi()
            print.Reset();
            PrintModificationDispatcher.PrintModified(print);

            try
            {
                if (!this.livescanDevice.IsOpened)
                {
                    // set next print to capture as previous print, since opening will trigger a capture of the next print !
                    this.captureIndex -= 1;
                    this.OpenDevice();
                    return;
                }

                bool scanSuccess;
                if (print.HandPart == HandPart.Endorsement)
                {
                    scanSuccess = this.livescanDevice.CapturePrint(print.Resolution, print.EndorsementFinger.Hand, print.EndorsementFinger.HandPart, print.ScanKind);
                }
                else
                {
                    scanSuccess = this.livescanDevice.CapturePrint(print.Resolution, print.Hand, print.HandPart, print.ScanKind);
                }

                // If the livescan is not opened, a message is already displayed. Only display unmanaged errors
                if (!scanSuccess)
                {
                    var msg = this.livescanDevice.LastException != null
                        ? this.livescanDevice.LastException.Message
                        : CommonText.UnknownError;
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
                    LogDispatcher.DoLog("Livescan-Could not process image", LogEventLevel.Error, ex);                    
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

        private void PrintModified(object sender, PrintModifiedEventArgs e)
        {
            if (this.livePreviewData.PreviousPrint == null || this.livePreviewData.PreviousPrint.LinkedPrint != e.Info)
            {
                return;                
            }

            this.livePreviewData.PreviousPrint = this.GetPreviousPrint();
        }

        //private void SendMessage(string text, bool stopCapture)
        //{
        //    if (this.livescanDevice.LastException != null)
        //    {
        //        text += Environment.NewLine + this.livescanDevice.LastException.Message;
        //    }

        //    this.DeviceMessage(text, stopCapture ? DeviceMessageKind.Error : DeviceMessageKind.Information, stopCapture);
        //}

        private void DeviceMessage(string text, DeviceMessageKind kind, bool showResumeButton)
        {
            if (kind == DeviceMessageKind.Error)
            {
                Console.WriteLine(text);
            }

            var displayMessage = new Action(
                () =>
                    {
                        this.livePreviewData.DeviceMessage = text;
                        this.livePreviewData.CaptureStopped = kind == DeviceMessageKind.Error;
                        this.livePreviewData.ResumeButtonVisibility = showResumeButton
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                        if (kind == DeviceMessageKind.Error)
                        {
                            this.livePreviewData.ScanInstructionText = CommonText.PrintCaptureHalted;                            
                            // this.livePreview.LivePreviewImage.Source = null;
                        }
                    });

            Application.Current.Dispatcher.Invoke(displayMessage);
        }

        private void DeviceQualityCallback(IEnumerable<PrintCaptureQuality> qualityList)
        {
            if (this.livePreviewWindow == null)
            {
                return;
            }

            var invoked = new Action(() => this.livePreviewWindow.RefreshQualityInformation(qualityList));
            Application.Current.Dispatcher.Invoke(invoked);
        }        

        private void DeviceCaptureDone(PrintResolution resolution, Bitmap printImage)
        {
            var invoked =
                new Action(() => this.SyncDeviceCaptureDone(resolution, printImage));
            Application.Current.Dispatcher.Invoke(invoked);
        }                               

        private void CaptureEnded(bool triggeredByWinClosed = false)
        {            
            if (this.endingCapture)
            {
                return;
            }

            this.endingCapture = true;
            PrintModificationDispatcher.RemoveWatch(PrintModified);            

            if (this.livescanDevice.IsOpened)
            {
                this.livescanDevice.StopCapture();
            }                        

            if (this.livePreviewWindow != null)
            {
                //if (this.livePreviewData.KeepWindowOpen)
                //{
                //    this.DeviceMessage(CommonText.SingleCaptureCompleted, DeviceMessageKind.Information, false);                    
                //}
                //else
                //{
                    this.livePreviewWindow.Close();
                    this.TriggerCaptureCompleted(false);
                //}

                
                // this.livePreview = null;
            }

            if (triggeredByWinClosed && this.livePreviewData.KeepWindowOpen)
            {
                this.TriggerCaptureCompleted(false);
                
            }
                        
            this.endingCapture = false;
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

        private PrintElementViewModel GetPreviousPrint()
        {
            var previousIndex = this.captureIndex - 1;

            if (previousIndex < 0)
            {
                previousIndex = 0;
                //return null;
            }

            var vm = this.PrintList.GetViewModel(this.captureList[previousIndex]);

            return vm;
        }        

        public override void ConfigureDefaultValue(List<ICaptureSdk> sdkList, string selectedKey)
        {
            var deviceList = new List<ILivescanDevice>();

            sdkList.ForEach(
                x => x.SupportedDeviceList.ToList().ForEach(dev => deviceList.Add(dev as ILivescanDevice)));

            // build viewModel            
            var selected = deviceList.FirstOrDefault(x => x.InternalKey == selectedKey);

            if (selected != null)
            {
                selected.FingerResolution = LivescanSettings.Default.ScannerFingerRes.ToResolution();
                selected.PalmResolution = LivescanSettings.Default.ScannerPalmRes.ToResolution();
                this.LoadConfiguration(selected);
            }
        }

        public override bool? Configure(DeviceConfigurationViewModel viewModel)
        {
            
            // build device list
            LiveScanLog.Logger.Debug($"Building device list. SDKs count : {viewModel.Sdks.Count}");
            var deviceList = new List<ListElementViewModel<ILivescanDevice>>();

            viewModel.Sdks.ForEach(
                x => x.SupportedDeviceList.ToList().ForEach(dev =>
                {
                    var live = (ILivescanDevice) dev;
                    this.LoadConfiguration(live);
                    deviceList.Add(new ListElementViewModel<ILivescanDevice>(live, live.DisplayName));
                } ));
            LiveScanLog.Logger.Debug($"Devie List Count : {deviceList.Count}");

            // build viewModel            
            var selected = deviceList.FirstOrDefault(x => x.Key.InternalKey == viewModel.SelectedDeviceKey);

            var vm = new LiveScanConfigurationViewModel { DeviceList = deviceList, SelectedDevice = selected };

            var confWindow = new ConfigurationWindow(vm);

            var result = confWindow.ShowDialog();

            if (result.HasValue && result.Value)
            {
                if (vm.SelectedDevice != null && vm.SelectedDevice.Key != null)
                {
                    var device = vm.SelectedDevice.Key;
                    device.FingerResolution = vm.FingerResolution.Key;
                    device.PalmResolution = vm.PalmResolution.Key;

                    LivescanSettings.Default.ScannerFingerRes = device.FingerResolution.ToDpi();
                    LivescanSettings.Default.ScannerPalmRes = device.PalmResolution.ToDpi();
                    this.SaveConfiguration(device);
                    
                    viewModel.SelectedDeviceKey = device.InternalKey;
                }
                else
                {
                    viewModel.SelectedDeviceKey = string.Empty;
                }
            }            

            return result;

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

        private void LoadConfiguration(ILivescanDevice device)
        {
            var conf = LivescanSettings.Default.DeviceConfiguration;

            if (conf == null || conf.Devices == null)
            {
                return;
            }

            var currentConf = conf.Devices.SingleOrDefault(x => x.Device == device.InternalKey);
            if (currentConf == null)
            {
                return;
            }

            foreach (var setting in currentConf.Settings)
            {
                device.Properties.SetValue(setting.Name, setting.Value);
            }
        }

        private void SaveConfiguration(ILivescanDevice device)
        {
            var conf = LivescanSettings.Default.DeviceConfiguration;

            if (conf == null || conf.Devices == null)
            {
                conf = new DeviceSettingList();
                LivescanSettings.Default.DeviceConfiguration = conf;
            }

            var currentConf = conf.Devices.SingleOrDefault(x => x.Device == device.InternalKey);
            if (currentConf == null)
            {
                currentConf = new DeviceSetting { Device = device.InternalKey };
                conf.Devices.Add(currentConf);
            }
            else
            {
                currentConf.Settings.Clear();
            }

            if (device != null && device.Properties != null)
            {
                var paramList = device.Properties.GetPropertyList().Select(x => new KeyAndValue<string, string>(x.InternalKey, x.GetValue())).ToList();
                foreach (var keyAndValue in paramList)
                {
                    currentConf.Settings.Add(new SettingValue { Name = keyAndValue.Key, Value = keyAndValue.Value });
                }
            }

            LivescanSettings.Save();
        }        
    }
}
