using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_AcquisitionProcessDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_DeviceCharacteristicsDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_ErrorCodesDefines;
using GBMSAPI_NET.GBMSAPI_NET_LibraryFunctions;
using PrintsCapture.Device.Enum;
using PrintsCapture.Device.Interface;
using PrintsCapture.Device.LivescanThales.Sdk;
using PrintsCapture.Prints;
using PrintsCapture.Prints.Enum;

namespace PrintsCapture.Device.LivescanThales.Plugin
{
    using System.Diagnostics.Eventing;
    using System.Linq;
    using System.Threading;

    using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_VisualInterfaceLCDDefines;

    using PrintsCapture.Device.AutoScan;
    using PrintsCapture.Prints.Extension;
    using PrintsCapture.Prints.Language;

    internal enum ScannerAction
    {
        Waiting,
        Initializing,
        Previewing,
        Acquiring,
        Stopping
    }

    public class DeviceApi : ILivescanDevice
    {
        private const string HardwareMakeName = "THALES";

        const string PropFlatAutoScanEnabled = "FLAT_AUTO_SCAN";
        const string PropDryEnhancementEnabled = "DRY_ENHANCE";
        const string PropMinPixelCount = "MIN_PIX_COUNT";
        const string PropErrorBeforeStopCount = "ERR_STOP_COUNT";


        private SdkApi sdk;

        private CapturePreviewHandler preview;        

        private uint AcquisitionOptionMask = 0;

        private const uint DisplayOptionMask = 0; //GBMSAPI_NET_DisplayOptions.GBMSAPI_NET_DO_FINAL_SCREEN;

        private uint optionalEquipmentCode = 0;

        private int correctImageMinPixel = 0;

        internal byte ThalesId { get; set; }

        private string lastErrorMessage;

        private int errorInARow = 3;
        private int maxErrorInArow = 5;
        private DiagnosticState currentDiag = null;

        private uint scannableTypesMask;

        private uint lcdFeatures;

        private PrintCaptureInformation currentCapture;

        private bool isFlatCapture;

        private ScannerAction currentAction = ScannerAction.Waiting;

        private IEnumerable<PhysicalHandPart> handParts;        

        public event DeviceStateChangedHandler StateChanged;

        public bool IsPlugged { get; set; }

        public string DisplayName { get; private set; }

        public string InternalKey { get; private set; }

        

        public string HardwareMake { get; private set; }

        public string ModelName { get; internal set; }

        public string SerialNumber { get; internal set; }

        public string ImageUri { get; private set; }

        // Defined by Sdk

        public PrintResolution SupportedResolutions { get; private set; }

        public DeviceScanKind SupportedScanKinds { get; private set; }

        public ICaptureSdk Sdk
        {
            get
            {
                return this.sdk;
            }
            private set
            {
                this.sdk = value as SdkApi;
            }
        }

        public DeviceState State { get; private set; }
       

        internal DeviceApi(
            byte id,
            string friendlyName,
            PrintResolution resolutions,
            DeviceScanKind scanKinds,
            SdkApi sdk,
            string imageUri,
            bool supportsLed = false)
        {
            this.Sdk = sdk;
            this.ThalesId = id;
            this.InternalKey = "Thales-" + id;

            this.HardwareMake = HardwareMakeName.ToUpper();
            this.DisplayName = friendlyName;

            this.SupportedResolutions = resolutions;
            this.SupportedScanKinds = scanKinds;
            this.ImageUri = imageUri;
            this.SupportsLed = supportsLed;

            this.CreatePropertyList();
        }

        private void CreatePropertyList()
        {
            this.Properties = new CustomPropertyList();
            this.Properties.AddBoolProperty(PropFlatAutoScanEnabled, CommonText.PropertyEnabled, true, CommonText.PropertyFlatAutoCapture);
            this.Properties.AddBoolProperty(PropDryEnhancementEnabled, CommonText.PropertyEnabled, false, CommonText.PropertyDryEnhance);            
            this.Properties.AddIntProperty(PropMinPixelCount, "Min pixel count", 500, "Image");
            this.Properties.AddIntProperty(PropErrorBeforeStopCount, "Error count to stop", 3, "Image");
            
        }

        public bool SupportsLed { get; set; }
        public bool SupportsPedal { get; }

        public void Close()
        {
            this.ChangeState(DeviceState.Closing);
            this.StopCapture();
            this.IsOpened = false;
            this.ChangeState(DeviceState.Closed);
        }

        public bool IsOpened { get; private set; }

        public CustomPropertyList Properties { get; private set; }

        public SdkException LastException { get; private set; }

        public PrintResolution FingerResolution { get; set; }

        public PrintResolution PalmResolution { get; set; }

        public string ExternalTool { get; private set; }

        public event DeviceMessageHandler DeviceSendMessage;

        public event DeviceOpenedHandler DeviceOpened;

        public event CapturedPrintHandler PrintCaptured;

        public event CaptureQualityHandler CaptureQualityChanged;

        public void Open()
        {
            this.ChangeState(DeviceState.Opening);
            this.currentAction = ScannerAction.Initializing;

            this.LastException = null;
            this.lastErrorMessage = null;
            this.currentDiag = null;
            this.errorInARow = 0;
            this.IsOpened = false;            

            if (this.CheckIfError(GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetOptionalExternalEquipment(
                    out this.optionalEquipmentCode)))
            {
                this.OnDeviceOpened(DeviceOpenStatus.ErrorOccured);
                this.ChangeState(DeviceState.Closed);
                return;
            }

            if (this.CheckIfError(GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetScannableTypes(
                    out this.scannableTypesMask)))
            {
                this.OnDeviceOpened(DeviceOpenStatus.ErrorOccured);
                this.ChangeState(DeviceState.Closed);
                return;
            }

            bool enhance = this.Properties.GetBoolValue(PropDryEnhancementEnabled);
            if (this.CheckIfError(GBMSAPI_NET_ScanSettingsRoutines.GBMSAPI_NET_EnableDrySkinImgEnhance(enhance))) {
                this.OnDeviceOpened(DeviceOpenStatus.ErrorOccured);
                this.ChangeState(DeviceState.Closed);
                return;
            }
            
            this.currentAction = ScannerAction.Waiting;
            this.IsOpened = true;
            this.ChangeState(DeviceState.Opened);

            this.OnDeviceOpened(DeviceOpenStatus.Success);
            this.ChangeState(DeviceState.Ready);
        }

        public bool StopCapture()
        {            
            try
            {
                this.WriteTrace("StopCapture - Start");
                this.currentAction = ScannerAction.Stopping;                
                GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_ROLL_StopPreview();
                var ret = GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                
                //Thread.Sleep(250);
                //if (this.State != DeviceState.Closing)
                //{
                //    this.ChangeState(DeviceState.Ready);
                //}
                this.WriteTrace("StopCapture - End");
                return true;
            }
            catch (Exception ex)
            {
                this.WriteTrace("StopCapture - Error encountered");
                this.LastException = new SdkException(
                    this.DisplayName,
                    "StopCapture",
                    SdkErrorKind.SpecificError,
                    ex.Message,
                    false,
                    ex);
                return false;
            }
        }

        public bool InitializeCapture(
            IEnumerable<PhysicalHandPart> handParts,
            CapturePreviewHandler preview,
            int previewWindowHandle,
            bool isFlat)
        {
            uint imgMaxSizeX, imgMaxSizeY;
            if (
                this.CheckIfError(
                    GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetMaxImageSize(
                        out imgMaxSizeX,
                        out imgMaxSizeY)))
            {
                return false;
            }

            this.handParts = handParts;
            this.isFlatCapture = isFlat;
            this.preview = preview;

            this.DisplayLcdLogoScreen();
                       
            return true;
        }

        public bool CapturePrint(PrintResolution resolution, Hand printHand, HandPart handPart, HandScanKind scanKind)
        {
            this.WriteTrace("CapturePrint - Start");
            this.currentAction = ScannerAction.Waiting;            
            this.correctImageMinPixel = this.Properties.GetIntValue(PropMinPixelCount);
            this.maxErrorInArow = this.Properties.GetIntValue(PropErrorBeforeStopCount);
            this.OnDeviceSendMessage("", DeviceMessageKind.Information, false);

            this.ChangeState(DeviceState.ScanInitialization);

            var fingerMapping = PrintMappings.GetMapping(printHand, handPart, scanKind);

            if (fingerMapping == null)
            {
                this.LastException = new SdkException(this.DisplayName, "Capture", SdkErrorKind.InvalidParameter, "Cannot find the specified print");
                return false;
            }

            var objectType = GBMSAPI_NET_ScanObjectsUtilities.GBMSAPI_NET_GetTypeFromObject(fingerMapping.ObjectToScanId);
            if (objectType == GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_NO_OBJECT_TYPE)
            {
                this.LastException = new SdkException(this.DisplayName, "Capture", SdkErrorKind.InvalidParameter, "Sdk does not know the scan object type");
                return false;
            }

            this.currentCapture = new PrintCaptureInformation(fingerMapping, objectType, resolution, this.isFlatCapture);

            if ((scannableTypesMask & fingerMapping.ObjectToScanId) == 0)
            {
                this.LastException = new SdkException(this.DisplayName, "Capture", SdkErrorKind.InvalidParameter, "Scanner cannot scan given print type");
                return false;
            }

            uint acquisitionOptions = AcquisitionOptionMask;
            uint scanOptions;
            if (this.currentCapture.MappedPrint.ScanKind == HandScanKind.Rolled && 
                GBMSAPI_NET_DeviceCharacteristicsRoutines.GBMSAPI_NET_GetSupportedScanOptions(out scanOptions) == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR && 
                (scanOptions > 0))
            {
                acquisitionOptions |= GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_ADAPT_ROLL_AREA_POSITION;
            }

            // if no pedal Or PropFlatScan is enabled, then set auto flat capture
            if ((this.optionalEquipmentCode & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_PEDAL) == 0 || this.Properties.GetBoolValue(PropFlatAutoScanEnabled))
            {
                acquisitionOptions |= GBMSAPI_NET_AcquisitionOptions.GBMSAPI_NET_AO_AUTOCAPTURE;
            }

                this.DoQualityCallback();

            this.WriteTrace("CapturePrint - called GBMSAPI_NET_StartAcquisition2");
            this.currentAction = ScannerAction.Previewing;
            if (
                this.CheckIfError(
                    GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StartAcquisition2(
                        fingerMapping.ObjectToScanId,
                        acquisitionOptions,
                        this.GetScanAreaFromObjectType(this.currentCapture.ScanObjectType, false, false),
                        this.AcquisitionCallback,
                        IntPtr.Zero,
                        DisplayOptionMask,
                        0,
                        0)))

            {
                this.currentAction = ScannerAction.Waiting;
                return false;
            }

            this.WriteTrace("CapturePrint - GBMSAPI_NET_StartAcquisition2 - end");
            return true;
        }

        void DoQualityCallback(PrintQualityLevel quality = PrintQualityLevel.NotPresent)
        {
            // Quality callback is not supported, but we return all non missing prints that need to be scanned.
            var handler = this.CaptureQualityChanged;
            if (handler == null)
            {
                return;
            }

            // create the expected list
            var list = new List<PrintCaptureQuality>();

            var prn = this.currentCapture.MappedPrint;
                                   
            if (prn.HandPart == HandPart.UpperPalm || prn.HandPart == HandPart.FourFlats)
            {
                var fingers = this.handParts.Where(x => !x.IsMissing && x.Hand == prn.PrintHand && x.HandPart != HandPart.Thumb && x.Kind == HandPartKind.Finger);
                foreach (var finger in fingers)
                {
                    list.Add(new PrintCaptureQuality(finger, quality));
                }

                if (prn.HandPart == HandPart.UpperPalm)
                {
                    list.Add(new PrintCaptureQuality(this.handParts.Single(x => x.Hand == prn.PrintHand && x.HandPart == HandPart.UpperPalm), PrintQualityLevel.NotPresent));
                }
            } 
            else if (prn.HandPart == HandPart.TwoThumbs)
            {
                var fingers = this.handParts.Where(x => !x.IsMissing && x.HandPart == HandPart.Thumb && x.Kind == HandPartKind.Finger);
                foreach (var finger in fingers)
                {
                    list.Add(new PrintCaptureQuality(finger, quality));
                }
            }
            else
            {
                var part = this.handParts.SingleOrDefault(x => x.Hand == prn.PrintHand && x.HandPart == prn.HandPart);
                list.Add(new PrintCaptureQuality(part, quality));            
            }

            handler(list);
        }

        void OnDeviceOpened(DeviceOpenStatus status)
        {
            var handler = this.DeviceOpened;
            if (handler == null)
            {
                return;
            }

            handler(status);
        }

        /// <summary>
        /// Check if an error occured
        /// </summary>
        /// <param name="errorCode">Operation return code</param>
        /// <returns>True when there are no errors</returns>
        private bool CheckIfError(int errorCode)
        {
            if (errorCode != GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
            {
                var errorKind = SdkErrorKind.SpecificError;
                var errorText = ErrorMessages.GBMSAPI_Example_GetErrorStringFromCode(errorCode);

                switch (errorCode)
                {
                    case GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_DEVICE_NOT_FOUND:
                        errorKind = SdkErrorKind.DeviceNotFound;
                        break;

                    case GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_DEVICE_NOT_RESPONDING:
                    case GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_SCANNER_COMMUNICATION:
                        errorKind = SdkErrorKind.CommunicationError;
                        break;

                    case GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_MEMORY_ALLOCATION:
                        errorKind = SdkErrorKind.OutOfMemory;
                        break;
                    
                    case GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_USB_DRIVER:
                        errorKind = SdkErrorKind.DeviceInitializationFailed;
                        errorText = "USB Driver Error";
                        break;

                    case GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_USB_FULLSPEED_NOT_SUPPORTED:
                        errorKind = SdkErrorKind.DeviceInitializationFailed;
                        errorText = "USB Port speed is not enough";
                        break;
                }
                                
                this.LastException = new SdkException(this.DisplayName, "CheckSdkError", errorKind, errorText);
                this.WriteTrace("Error : " + errorText);
                return true;
            }

            return false;
        }

        private void OnDeviceSendMessage(string text, DeviceMessageKind kind, bool showResumeButton)
        {
            var handler = this.DeviceSendMessage;
            if (handler != null)
            {
                handler(text, kind, showResumeButton);
            }
        }

        private void DisplayLcdLogoScreen()
        {
            // Is lcd supported ?
            if ((this.optionalEquipmentCode & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_VUI_LCD)
                != GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_VUI_LCD)
            {
                return;
            }

            GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_VUI_LCD_SetLogoScreen();
        }

        /// <summary>
        /// This Sdks works with events
        /// </summary>
        /// <param name="occurredEventCode"></param>
        /// <param name="frameErrorCode"></param>
        /// <param name="eventInfo"></param>
        /// <param name="framePtr"></param>
        /// <param name="frameSizeX"></param>
        /// <param name="frameSizeY"></param>
        /// <param name="currentFrameRate"></param>
        /// <param name="nominalFrameRate"></param>
        /// <param name="diagnosticCode"></param>
        /// <param name="userDefinedParameters"></param>
        /// <returns></returns>        
        public int AcquisitionCallback(
            uint occurredEventCode,
            int frameErrorCode,
            uint eventInfo,
            byte[] framePtr,
            int frameSizeX,
            int frameSizeY,
            double currentFrameRate,
            double nominalFrameRate,
            uint diagnosticCode,
            System.IntPtr userDefinedParameters)
        {
            int ret = 0;
            // call form method appropriate for EventCode
            switch (occurredEventCode)
            {
                case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_ACQUISITION_ERROR:
                    this.WriteTrace("Event: GBMSAPI_NET_AE_ACQUISITION_ERROR");
                    ret = OnAcquisitionError(frameErrorCode);
                    break;

                case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_SCANNER_STARTED:
                    this.WriteTrace("Event: GBMSAPI_NET_AE_SCANNER_STARTED");
                    ret = OnScannerStarted();
                    break;

                case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_PREVIEW_PHASE_END:
                    this.WriteTrace("Event: GBMSAPI_NET_AE_PREVIEW_PHASE_END");
                    ret = OnPreviewPhaseEnd();
                    break;

                case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_VALID_FRAME_ACQUIRED:
                    //this.WriteTrace("Event: GBMSAPI_NET_AE_VALID_FRAME_ACQUIRED");
                    //ret = OnValidFrameAcquired(framePtr, frameSizeX, frameSizeY, diagnosticCode, false);
                    DoWorkPreviewFrame(framePtr, frameSizeX, frameSizeY, diagnosticCode);
                    break;                

                case GBMSAPI_NET_AcquisitionEvents.GBMSAPI_NET_AE_ACQUISITION_END:
                    this.WriteTrace("Event: GBMSAPI_NET_AE_ACQUISITION_END");
                    
                    if ((this.optionalEquipmentCode & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_SOUND) != 0)
                    {
                        this.WriteTrace("Acquisition END");
                        GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_Sound(12, 1, 1); // 12, 4, 2
                    }
                // contains also a valid frame
                    //V1.2 - FramePtr can be null
                    PrintImageAcquired(framePtr, frameSizeX, frameSizeY, diagnosticCode);


                    ret = OnAcquisitionEnd();
                    break;

                default:
                    this.WriteTrace("Event: #" + occurredEventCode);
                    ret = 0;
                    break;
            }

            return 1;
        }

        private int OnAcquisitionEnd()
        {
            this.WriteTrace("OnAcquisitionEnd");
            
            this.currentAction = ScannerAction.Waiting;

            if (this.State != DeviceState.Closing)
            {
                this.ChangeState(DeviceState.Ready);
            }

            return 1;
        }

        private int OnPreviewPhaseEnd()
        {
            if (currentAction == ScannerAction.Stopping)
            {
                return 1;
            }

            this.WriteTrace("OnPreviewPhaseEnd");
            // Sound to advise operator that the preview has ended
            // Only for rolled prints !
            if ((this.optionalEquipmentCode & GBMSAPI_NET_OptionalExternalEquipment.GBMSAPI_NET_OED_SOUND) != 0 && this.currentCapture.MappedPrint.ScanKind == HandScanKind.Rolled)
            {
                this.WriteTrace("END PREVIEW");
                GBMSAPI_NET_ExternalDevicesControlRoutines.GBMSAPI_NET_Sound(10, 2, 1);
            }
            this.DoQualityCallback(PrintQualityLevel.Good);

            if (this.currentCapture.MappedPrint.ScanKind != HandScanKind.Rolled)
            {
                this.currentAction = ScannerAction.Acquiring;
            }
            

            return 1;
        }

        private void PrintImageAcquired(byte[] framePtr,
            int frameSizeX,
            int frameSizeY,
            uint diagnosticCode)
        {
            this.WriteTrace(string.Format("Action: {0}", this.currentAction));
            // When stopping the capture, if device is closing, do not put the Ready status
            if (this.currentAction == ScannerAction.Stopping)
            {
                return;                                                
            }

            var diag = this.CheckDiagnosticCode(diagnosticCode);

            if (diag.IsError)
            {
                GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
                this.errorInARow += 1;
                if (this.errorInARow >= this.maxErrorInArow)
                {
                    this.errorInARow = 0;
                    this.OnDeviceSendMessage(string.Join("\n", diag.Messages), DeviceMessageKind.Error, true);
                } else
                {
                    var prn = this.currentCapture.MappedPrint;
                    currentDiag = diag;
                    this.BeginInvoke(() =>
                    {
                        this.CapturePrint(this.currentCapture.Resolution, prn.PrintHand, prn.HandPart, prn.ScanKind);
                    });
                }
                
                return;
            }

            if (framePtr == null)
            {
                this.OnDeviceSendMessage(CommonText.EmptyImage, DeviceMessageKind.Error, true);
                return;
            }
                        
            GBMSAPI_NET_EndAcquisitionRoutines.GBMSAPI_NET_ImageFinalization(framePtr);

            var bmp = XL_ID.Utilities.Image.ImageUtilities.ByteArrayToBitmap(
                framePtr,
                new Size(frameSizeX, frameSizeY),
                PixelFormat.Format8bppIndexed,
                this.currentCapture.Resolution.ToDpi());

            var clip = this.GetClipInfo();
            if (clip.Enabled)
            {
                var old = bmp;
                bmp = clip.ApplyClip(bmp);
                if (bmp != old)
                {
                    old.Dispose();
                }
            }

            var editor = new IndexedImageEditor(bmp);

            // Verify image is not empty !!                
            var rect = editor.GetAutoCropRectangle();

            if (rect.Width < 50 || rect.Height < 50)
            {
                // Empty image !!
                this.OnDeviceSendMessage(CommonText.EmptyImage, DeviceMessageKind.Error,  true);
                return;
            }

            this.currentDiag = null;
            this.errorInARow = 0;

            this.BeginInvoke(() =>
            {
                this.SyncFrameAquired(bmp);
                this.ChangeState(DeviceState.Ready);
            });
        }

       

        private void DoWorkPreviewFrame(byte[] framePtr,
            int frameSizeX,
            int frameSizeY,
            uint diagnosticCode)
        {
            if (this.currentAction != ScannerAction.Previewing)
            {
                return;
            }

            var clip = this.GetClipInfo();
            
            var diag = this.CheckDiagnosticCode(diagnosticCode);

            var bmp = XL_ID.Utilities.Image.ImageUtilities.ByteArrayToBitmap(
                    framePtr,
                    new Size(frameSizeX, frameSizeY),
                    PixelFormat.Format8bppIndexed,
                    this.currentCapture.Resolution.ToDpi());

            var editor = new IndexedImageEditor(bmp);
            
            editor.Begin();            
            
            var pixelCount = editor.GetAutoCropPixelCount();
            //var autoCropRect = editor.GetAutoCropRectangle(); // WITHOUT FINALIZE, DOESNT WORKWELL            

            // draw the clipping rectangle on the image
            if (clip.Enabled)
            {
                editor.DrawRectangle(new Rectangle(clip.X, clip.Y, clip.Width, clip.Height), Color.Black);
            }

            // * MESSAGES *******************
            var y = 0;
            var fontSize = this.currentCapture.MappedPrint.ScanKind == HandScanKind.Rolled ? 16 : 8;
            var ySpace = fontSize + 2;

            // draw the messages
            if (diag.Messages.Count > 0)
            {                                
                foreach (var message in diag.Messages)
                {
                    editor.DrawText(message, fontSize, 0, y);
                    y += ySpace;
                }
            }
            // draw error messages !
            if (this.currentDiag != null && this.currentDiag.Messages.Count > 0)
            {
                editor.DrawText(CommonText.RescanCauses, fontSize, bmp.Width / 2, 0);
                y = 10;
                foreach (var message in this.currentDiag.Messages)
                {
                    editor.DrawText(message, fontSize, bmp.Width / 2, y);
                    y += ySpace;
                }
            }



            editor.End();

            this.BeginInvoke(() =>
            {
                this.preview(bmp);
                this.ChangeState(DeviceState.Scanning);
            });

            //this.WriteTrace("Not empty pixel count : {0}", pixelCount);
            //this.WriteTrace("Autocrop rect : {0}x{1}", autoCropRect.Width, autoCropRect.Height);
            if (pixelCount > this.correctImageMinPixel)
            {
                // image is present !                
                this.DoQualityCallback(PrintQualityLevel.NotGoodEnough);
            }
            else
            {
                this.DoQualityCallback(PrintQualityLevel.NotPresent);
            }
            
            //if (this.currentCapture.MappedPrint.HandPart == HandPart.LowerPalm && isImagePresent)
            //{
            //    // trigger capture after a delay !
            //    GBMSAPI_NET_ScanningRoutines.GBMSAPI_NET_StopAcquisition();
            //}

        }

        private void BeginInvoke(Action action)
        {
            var bg = new BackgroundWorker();
            bg.DoWork += (sender, args) => action();
            bg.RunWorkerAsync();
        }

        private int SyncFrameAquired(Bitmap bmp)
        {

            var handler = this.PrintCaptured;
            if (handler != null)
            {
                handler(this.currentCapture.Resolution, bmp);
                return 1;
            }

            return 0;
        }

        private int OnScannerStarted()
        {
            this.OnDeviceSendMessage(null, DeviceMessageKind.Information, false);

            // set clipping area
            var size = this.currentCapture.CaptureSize;

            if (this.currentCapture.MappedPrint.ScanKind != HandScanKind.Rolled)
            {
                if (
                    this.CheckIfError(
                        GBMSAPI_NET_ScannerStartedRoutines.GBMSAPI_NET_SetClippingRegionSize(
                            (uint)size.Width,
                            (uint)size.Height)))
                {
                    this.WriteTrace(@"Could not set clipping area !!!");

                    //this.SendDeviceMessage("Cannot set clipping area");
                    //this.StopCapture();
                    //this.ChangeState(DeviceState.Ready);
                    //return 0;
                }
            }

            return 1;
        }

        private int OnAcquisitionError(int frameErrorCode)
        {
            //turn off acquisition flag
            var msg = ErrorMessages.GBMSAPI_Example_GetErrorStringFromCode(frameErrorCode);
            this.OnDeviceSendMessage(msg, DeviceMessageKind.Error, true);

            return 0;
        }        

        public UInt32 GetScanAreaFromObjectType(UInt32 objType, bool flatFingerOnRollArea, bool rollAreaGa)
        {
            switch (objType)
            {
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_INDEXES_2: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_LOWER_HALF_PALM: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SINGLE_FINGER:
                    {
                        if (flatFingerOnRollArea)
                        {
                            if (rollAreaGa) return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA;
                            else return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
                        }
                        else
                        {
                            return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                        }
                    }
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SLAP_2:
                    return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_SLAP_4:
                    return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_THUMBS_2: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_UPPER_HALF_PALM:
                    return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_FLAT_WRITER_PALM: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_FULL_FRAME;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PHOTO: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_PHOTO;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PLAIN_JOINT_LEFT_SIDE:
                    return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_JOINT;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_PLAIN_JOINT_RIGHT_SIDE:
                    return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_JOINT;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLL_SINGLE_FINGER:
                    {
                        if (rollAreaGa) return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA;
                        else return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
                    }
                // ver 3.2.0.0
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_UP:
                    {
                        if (rollAreaGa) return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA;
                        else return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
                    }
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_DOWN:
                    {
                        if (rollAreaGa) return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_GA;
                        else return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
                    }
                // end ver 3.2.0.0
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_JOINT: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_JOINT;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_JOINT_CENTER: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_JOINT;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_THENAR: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_THENAR;
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_TIP: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_IQS;
                // Ver 3.3.0.0
                case GBMSAPI_NET_ScannableBiometricTypes.GBMSAPI_NET_SBT_ROLLED_HYPOTHENAR: return GBMSAPI_NET_ScanAreas.GBMSAPI_NET_SA_ROLL_THENAR;
                // end Ver 3.3.0.0
                default: return 0;
            }
        }

        private void ChangeState(DeviceState newState)
        {
            var oldState = this.State;
            this.State = newState;

            var handler = this.StateChanged;
            if (handler == null || oldState == newState)
            {
                return;
            }

            this.WriteTrace(string.Format("State : {0}", newState));
            handler(this, newState);
            
        }

        private void WriteTrace(string msg)
        {
            Console.WriteLine(@"{0:yyyy-MM-dd HH:mm:ss.fff} - {1}", DateTime.Now, msg);            
        }

        private DiagnosticState CheckDiagnosticCode(uint diagnostic)
        {
            var result = new DiagnosticState();            

            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_SCANNER_SURFACE_NOT_NORMA) != 0)
            {
                result.Messages.Add(CommonText.ThalesSurfaceNotNormal);
                result.IsError = true;
            }
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_SCANNER_FAILURE) != 0)
            {
                result.Messages.Add(CommonText.ThalesScannerFailure);
                result.IsError = true;
            }
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_COMPOSITION_SLOW) != 0)
            {
                result.Messages.Add(CommonText.ThalesCompositionSlow);
            }
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_SLIDING) != 0)
            {
                result.Messages.Add(CommonText.ThalesFlatFingerSlide);
                result.IsError = true;
            }
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_EXT_LIGHT_TOO_STRONG) != 0)
            {
                result.Messages.Add(CommonText.ThalesTooMuchLight);
                result.IsError = true;
            }
            if (((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_LEFT) != 0) ||
                ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_OUTSIDE_BORDER_LEFT) != 0))
            {
                result.Messages.Add(CommonText.ThalesImageOutLeft);
                if (this.currentCapture != null && this.currentCapture.MappedPrint.ScanKind != HandScanKind.Rolled)
                {
                    result.IsError = true;
                }
            }
            if (((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_RIGHT) != 0) ||
                ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_OUTSIDE_BORDER_RIGHT) != 0))
            {
                result.Messages.Add(CommonText.ThalesImageOutRight);
                if (this.currentCapture != null && this.currentCapture.MappedPrint.ScanKind != HandScanKind.Rolled)
                {
                    result.IsError = true;
                }
                
            }
            if (((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_OUT_OF_REGION_TOP) != 0) ||
                ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_OUTSIDE_BORDER_TOP)) != 0)
            {
                // Only a warning for rolled prints !
                result.Messages.Add(CommonText.ThalesImageOutTop);
                
                result.IsError = true;
                
            }
            if (((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_DISPLACED_DOWN) != 0) ||
                ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_FLAT_FINGER_DISPLACED_DOWN) != 0))
            {
                // Only a warning for rolled prints !
                result.Messages.Add(CommonText.ThalesImageOutBottom);
                if (this.currentCapture != null && this.currentCapture.MappedPrint.ScanKind != HandScanKind.Rolled)                
                {
                    result.IsError = true;
                }
                
            }
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_IMPROPER_ROLL) != 0)
            {
                result.Messages.Add(CommonText.ThalesImproperRoll);
                result.IsError = true;
            }
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_TOO_FAST_ROLL) != 0)
            {
                result.Messages.Add(CommonText.ThalesRollTooFast);
                result.IsError = true;
            }
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_TOO_NARROW_ROLL) != 0)
            {
                result.Messages.Add(CommonText.ThalesRollTooNarrow);
                result.IsError = true;
            }
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_RIGHT) != 0)
            {
                result.Messages.Add(CommonText.ThalesRollToTheRight);
            }
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_LEFT) != 0)
            {
                result.Messages.Add(CommonText.ThalesRollToTheLeft);
            }
            // VER 2.9.0.0
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_DRY_FINGER) != 0)
            {
                result.Messages.Add(CommonText.ThalesDryFinger);
            }
            // VER 2.9.0.0
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_WET_FINGER) != 0)
            {
                result.Messages.Add(CommonText.ThalesWetFinger);
            }
            // VER 3.1.0.0
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_TOO_SHORT_VERTICAL_ROLL) != 0)
            {
                result.Messages.Add(CommonText.ThalesRollVerticalTooShort);
            }
            // end VER 3.1.0.0
            // VER 3.2.0.0
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_UP) != 0)
            {
                result.Messages.Add(CommonText.ThalesRollTowardTop);
            }
            if ((diagnostic & GBMSAPI_NET_DiagnosticMessages.GBMSAPI_NET_DM_VSROLL_ROLL_DIRECTION_DOWN) != 0)
            {
                result.Messages.Add(CommonText.ThalesRollTowardBottom);
            }
            // end VER 3.2.0.0

            return result;
        }

        private ClipInformation GetClipInfo()
        {
            var result = new ClipInformation();
            if (this.currentCapture.MappedPrint.ScanKind == HandScanKind.Rolled)
            {
                result.Enabled = false;
                return result;
            }
            
            int clipX, clipY;
            uint clipSizeX, clipSizeY;

            if (GBMSAPI_NET_ValidFrameAcquiredRoutines.GBMSAPI_NET_GetClippingRegionPosition(
                out clipX,
                out clipY,
                out clipSizeX,
                out clipSizeY) == GBMSAPI_NET_ErrorCodes.GBMSAPI_NET_ERROR_CODE_NO_ERROR)
            {
                result.X = clipX;
                result.Y = clipY;
                result.Width = (int)clipSizeX;
                result.Height = (int)clipSizeY;

                result.Enabled = clipSizeX > 0 && clipSizeY > 0;

            }
            else
            {
                result.Enabled = false;
            }

            return result;            
        }

        private class DiagnosticState
        {
            public DiagnosticState()
            {
                this.Messages = new List<string>();
            }

            public List<string> Messages { get; set; }

            public bool IsError { get; set; }            
        }

        private class ClipInformation
        {
            public int X { get; set; }
            public int Y { get; set; }

            public int Width { get; set; }
            public int Height { get; set; }

            public bool Enabled { get; set; }

            public Bitmap ApplyClip(Bitmap bmp)
            {
                if (this.X + this.Width > bmp.Width)
                {
                    this.Width = bmp.Width - this.X;
                }

                if (this.Y + this.Height > bmp.Height)
                {
                    this.Height = bmp.Height - this.Y;
                }

                if (this.Width == bmp.Width && this.Height == bmp.Height)
                {
                    return bmp;
                }

                return XL_ID.Utilities.Image.ImageUtilities.Crop(bmp, this.X, this.Y, new Size(this.Width, this.Height));
            }
        }
    }
}
