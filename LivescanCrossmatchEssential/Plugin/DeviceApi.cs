namespace PrintsCapture.Device.LivescanCrossmatchEssential.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Livescan.Scanners.DriverEssential;
    using Livescan.Scanners.DriverEssential.Plugin;
    using Livescan.Scanners.DriverEssential.Sdk;

    using PrintsCapture.Device;
    using PrintsCapture.Device.AutoScan;
    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Language;

    using XL_ID.Utilities.Image;
    using XL_ID.Utilities.Log;

    public class DeviceApi : ILivescanDevice
    {        
        const string PropAutoScanEnabled = "AUTO_SCAN";
        const string PropBeebEnabled = "BEEP";
        const string PropAutoScanDelay = "AUTO_SCAN_DELAY";
        const string PropAutoScanCountDownStart = "AUTO_COUNTDOWN";
        const string PropUseSystemSound = "SOUND_FROM_SYSTEM";
        const string PropSystemSoundMuted = "SYSTEM_SOUND_MUTED";
        const string PropCaptureRectBottom = "CAPTURE_RECT_BOTTOM";

        const string PropIgnoreWarning = "IGNORE_WARNING";

        const string DeviceMake = "Crossmatch Technologies";

        int deviceHandle = -1;
        uint timeoutOverlayHandle = uint.MaxValue;
        uint msgOverlayHandle = uint.MaxValue;

        List<PhysicalHandPart> leftHandFingers;
        List<PhysicalHandPart> rightHandFingers;

        List<PrintCaptureInformation> supportedPrints;

        IEnumerable<PhysicalHandPart> handParts;

        bool beeperDefined;

        enumLScanBeeperType beeperType;

        CaptureAutoScan autoCaptureTrigger;

        PrintCaptureInformation currentScan;

               

        private List<PhysicalHandPart> qualityParts;

        private CapturePreviewHandler capturePreviewHandler;

        private Rectangle captureRect = new Rectangle(-1,-1,0,0);

        private LedManager ledManage;

        private DisplayManager displayManage;

        #region Sdk callbacks

        private LSCAN_CallbackResultImage sdkCallbackResultImage;

        private LSCAN_CallbackClearObjectsFromPlaten sdkCallbackClearObjectFromPlaten;

        private LSCAN_CallbackObjectQuality sdkCallbackQualityObject;

        private LSCAN_Callback sdkCallbackTakingResult;

        private LSCAN_Callback sdkCallbackAquisitionComplete;        

        private LSCAN_CallbackPreviewImage sdkCallbackPreviewImage;

        private LSCAN_CallbackProgress sdkCallbackProgress;                

        private int capturePreviewWindowHandle;

        private bool callbackSet;

        private LSCAN_Callback sdkCallbackCommunicationBreak;

        private TftManager tftManage;

        #endregion

        #region Constructor

        public DeviceApi()
        {
            this.CreatePropertyList();
            this.CreateSupportedPrintList();            
        }        

        internal DeviceApi(string internalName, string friendlyName, PrintResolution resolutions, DeviceScanKind scanKinds, string imageUri, ICaptureSdk sdk, bool supportsLed = false) : this()
        {
            this.Sdk = sdk;
            this.InternalKey = internalName;
            
            this.HardwareMake = DeviceMake.ToUpper();
            this.DisplayName = friendlyName;

            this.SupportedResolutions = resolutions;
            this.SupportedScanKinds = scanKinds;

            this.SupportsLed = supportsLed;
            this.ExternalTool = "LS_TestWizard.exe";

            this.ImageUri = imageUri;
        }

        #endregion

        public PrintResolution PalmResolution { get; set; }

        public string ExternalTool { get; private set; }

        public event DeviceMessageHandler DeviceSendMessage;

        public event DeviceOpenedHandler DeviceOpened;

        public event CapturedPrintHandler PrintCaptured;

        public event CaptureQualityHandler CaptureQualityChanged;

        public event DeviceStateChangedHandler StateChanged;

        [XmlIgnore]
        public ICaptureSdk Sdk { get; private set; }

        public DeviceState State { get; private set; }
        

        public PrintResolution FingerResolution { get; set; }

        public bool SupportsLed { get; set; }

        public void Open()
        {
            this.LastException = null;
            this.ChangeState(DeviceState.Opening);

            var hasErrorOccured = false;
            if (this.DeviceSendMessage == null)
            {
                this.ChangeState(DeviceState.Closed);
                throw new ApplicationException("DeviceSendMessage event must be handled");
            }            

            this.sdkCallbackProgress = this.CallbackDeviceInitProgress;
            if (this.CheckSdkError(LSE_SDK.LSCAN_Main_RegisterCallbackProgress(this.sdkCallbackProgress, IntPtr.Zero)))
            {
                this.ChangeState(DeviceState.Closed);
                throw new ApplicationException("Cannot register callback to device progress");
            }            

            var devices = SdkApi.GetConnectedDevices();

            var essentialDevice = devices.SingleOrDefault(x => x.TypeName == this.InternalKey);

            if (this.InternalKey == DeviceCaptureDriver.GenericDeviceKey && essentialDevice == null)
            {
                essentialDevice = devices.FirstOrDefault();
                
            }

            if (essentialDevice == null)
            {
                this.DeviceOpened?.Invoke(DeviceOpenStatus.DeviceNotFound);
                return;
            }

            this.ModelName = essentialDevice.TypeName;
            this.SerialNumber = essentialDevice.SerialNumber;
            //this.InternalKey = essentialDevice.TypeName;

            // get device handle with device INDEX
            if (this.CheckSdkError(LSE_SDK.LSCAN_Main_Initialize(essentialDevice.Index, 0, out this.deviceHandle)))
            {                
                hasErrorOccured = true;
                this.IsOpened = this.deviceHandle > -1;
                this.DeviceOpened?.Invoke(DeviceOpenStatus.ErrorOccured);
                if (this.LastException != null && !this.LastException.IsWarningOnly)
                {
                    this.ChangeState(DeviceState.Closed);
                    return;
                }                
            }

            if (this.InternalKey == DeviceCaptureDriver.GenericDeviceKey)
            {
                this.ConfigureAsAny(essentialDevice);
            }

            

            this.IsOpened = true;

            DeviceSoundPlayer.IsMuted = this.Properties.GetBoolValue(PropSystemSoundMuted);

            if (!hasErrorOccured)
            {
                this.DeviceOpened?.Invoke(DeviceOpenStatus.Success);
            }
            this.ChangeState(DeviceState.Opened);
            this.ChangeState(DeviceState.Ready);

           
        }

        public string SerialNumber { get; set; }

        public string ImageUri { get; private set; }

        public bool InitializeCapture(IEnumerable<PhysicalHandPart> physHandParts, CapturePreviewHandler preview, int previewWindowHandle, bool isFlat)
        {
            this.handParts = physHandParts;
            this.SetHandFingers();
            this.AdaptSupportedPrints(isFlat);

            this.ledManage = new LedManager(this.deviceHandle, this.handParts);
            this.displayManage = new DisplayManager(this.deviceHandle, this.handParts);
            this.tftManage = new TftManager(this.deviceHandle, this.handParts.ToList());

            this.displayManage.DisplayStandby();
            this.capturePreviewHandler = preview;

            this.capturePreviewWindowHandle = previewWindowHandle;                        

            if (this.PrintCaptured == null)
            {
                this.LastException = new SdkException(this.DisplayName, "InitializeCapture", SdkErrorKind.SdkInitFailed, "PrintCaptured event is null");
                return false;
            }

            if (isFlat)
            {
                //captureRect = new Rectangle(0,0,400,200);
            }

            return true;
        }

        public bool CapturePrint(PrintResolution resolution, Hand printHand, HandPart handPart, HandScanKind scanKind)
        {
            this.LastException = null;

            if (!this.IsOpened)
            {
                this.Log("CapturePrint called but Device was not opened");
                this.Open();
                return this.IsOpened;  
                // capture next print is called when a device is opened
            }

            this.Log("CapturePrint called");

            this.ChangeState(DeviceState.ScanInitialization);                           

            if (scanKind == HandScanKind.Rolled)
            {
                LSE_SDK.LSCAN_Main_SetProperty(this.deviceHandle, enumLScanPropertyId.LSCAN_PROPERTY_ROLL_FLEXIBLE, "TRUE");
            }

            this.qualityParts = new List<PhysicalHandPart>();
            this.OnDeviceSendMessage("", DeviceMessageKind.Information, false);

            this.currentScan = this.supportedPrints.Single(x => x.Hand == printHand && x.Kind == scanKind && x.Part == handPart && x.Resolution == resolution);

            this.ledManage.InitForCapture(handPart, printHand);
            this.displayManage.InitForCapture(this.currentScan.Part, this.currentScan.Hand, this.currentScan.Kind);
            this.tftManage.InitForCapture(this.currentScan.Part, this.currentScan.Hand, this.currentScan.Kind);

            // this.autoCaptureTrigger = new CaptureAutoScan(true, 5);
            // Enable autoscan after delay for flats
            if (this.Properties.GetBoolValue(PropAutoScanEnabled) && scanKind == HandScanKind.Flat)
            {
                int delay = this.Properties.GetIntValue(PropAutoScanDelay);
                this.autoCaptureTrigger = new CaptureAutoScan(true, delay);
            }
            else
            {
                this.autoCaptureTrigger = new CaptureAutoScan(false, 5);
            }

            this.autoCaptureTrigger.OngoingTimeOut += this.autoCaptureTrigger_OngoingTimeOut;

            if (!this.SetCallback())
            {
                this.OnDeviceSendMessage("Could not set callbacks", DeviceMessageKind.Error, true);
                return false;
            }

            if (
                this.CheckSdkError(
                    LSE_SDK.LSCAN_Visualization_SetWindow(
                        this.deviceHandle, (uint)this.capturePreviewWindowHandle, this.captureRect)))
            {
                this.OnDeviceSendMessage("Could not set Window",  DeviceMessageKind.Error,  true);
                return false;
            }

            if (!this.SetCaptureMode())
            {
                this.OnDeviceSendMessage("Could not set capture mode", DeviceMessageKind.Error,  true);
                return false;
            }
            
            int captureObjectCount = this.SetPrintsToCapture(this.currentScan);
            this.autoCaptureTrigger.InitializeCapture(captureObjectCount);

            this.ResetOverlays();
            this.ShowPrintsToCapture();

            if (this.CheckSdkError(LSE_SDK.LSCAN_Capture_Start(this.deviceHandle, captureObjectCount))) 
            {
                this.OnDeviceSendMessage("Could not start capture", DeviceMessageKind.Error,  true);
                return false;
            }

            return true;
        }        

        public CustomPropertyList Properties { get; private set; }   

        public bool StopCapture()
        {
            this.Log("StopCapture called");
            this.displayManage.DisplayStandby();
            this.ledManage?.CloseLeds();

            if (this.deviceHandle > -1)
            {                
                LSE_SDK.LSCAN_Controls_DisplayShowLogoScreen(this.deviceHandle, enumLScanDisplayLogoOption.LSCAN_DISPLAY_LOGO_OPTION_ERASE, 0);

                int captureIsActive;
                if (LSE_SDK.LSCAN_Capture_IsActive(this.deviceHandle, out captureIsActive) == LSE_ErrorCode.LSCAN_STATUS_OK)
                {
                    if (captureIsActive == LSE_Constants.BOOL_TRUE)
                    {
                        LSE_SDK.LSCAN_Capture_Abort(this.deviceHandle);
                        
                        //return true;
                    }
                }

                this.ClearCallback();            
                this.ChangeState(DeviceState.Ready);
            }

            return true;
        }        

        public void Close()
        {
            this.Log("Close called");
            
            this.ChangeState(DeviceState.Closing);

            if (this.deviceHandle == -1 || LSE_SDK.LSCAN_Main_IsInitialized(this.deviceHandle) != LSE_ErrorCode.LSCAN_STATUS_OK) 
            {
                this.Log("Close - Device is not initialized");
                return;
            }

            try
            {
                this.ClearCallback();
            }
            catch (Exception)
            {
                // nothing
            }

            try
            {
                LSE_SDK.LSCAN_Controls_DisplayShowLogoScreen(this.deviceHandle, enumLScanDisplayLogoOption.LSCAN_DISPLAY_LOGO_OPTION_ERASE, 0);
                LSE_SDK.LSCAN_Main_Release(this.deviceHandle, 0);
            }
            catch (Exception ex)
            {
                LogDispatcher.DoLog("Crossmatch DeviceApi Close error", LogEventLevel.Warning, ex);
            }

            this.deviceHandle = -1;            

            this.CaptureQualityChanged = null;
            this.PrintCaptured = null;
            this.DeviceOpened = null;
            this.DeviceSendMessage = null;
            this.capturePreviewHandler = null;            

            this.handParts = null;

            this.IsOpened = false;

            this.ChangeState(DeviceState.Closed);
            this.Log("Close - Device was closed");
        }

        public void Dispose()
        {
            this.Close();
        }

        public string DisplayName { get; private set; }

        public string InternalKey { get; private set; }

        public string HardwareMake { get; private set; }

        public string ModelName { get; private set; }

        public PrintResolution SupportedResolutions { get;  private set; }

        public DeviceScanKind SupportedScanKinds { get; private set; }

        public SdkException LastException { get; private set; }

        //public bool LastExceptionIsWarning { get; set; }

        public bool IsOpened { get; private set; }

        public void LoadConfiguration()
        {
            //var values
        }

        public void SaveConfiguration()
        {
            throw new NotImplementedException();
        }

        #region Event handler

        private void OnDeviceSendMessage(string text, DeviceMessageKind kind, bool showResumeButton)
        {
            var handler = this.DeviceSendMessage;

            if (this.LastException != null)
            {
                text += Environment.NewLine + this.LastException.Message;
                if (this.LastException.IsWarningOnly)
                {
                    showResumeButton = true;
                }
            }

            handler?.Invoke(text, kind, showResumeButton);
        }

        

        private void OnDevicePrintCaptured(PrintResolution resolution, Bitmap image)
        {
            this.displayManage.DisplayStandby();
            this.ChangeState(DeviceState.Ready);
            this.PrintCaptured?.Invoke(resolution, image);                        
        }        

        #endregion

        #region private sdk method

        private void ChangeState(DeviceState newState)
        {            
            var oldState = this.State;
            this.State = newState;

            var handler = this.StateChanged;
            if (handler == null || oldState == newState)
            {
                return;
            }
            this.Log("ChangeState - Device State changed to : " + newState.ToString());
            handler(this, newState);
        }

        private void ConfigureAsAny(DeviceInfo devInfo)
        {
            if (devInfo == null)
            {
                return;
            }

            this.SupportsLed = false;
            this.ModelName = devInfo.TypeName;

            if (this.IsAvailable(devInfo.Index, enumLScanImageType.LSCAN_PALM_LEFT_FULL, enumLScanImageResolution.LSCAN_IMAGE_RESOLUTION_500))
            {
                this.SupportedScanKinds |= DeviceScanKind.FlatCompletePalm;
            }

            if (this.IsAvailable(devInfo.Index,enumLScanImageType.LSCAN_FLAT_TWO_THUMBS,enumLScanImageResolution.LSCAN_IMAGE_RESOLUTION_500))
            {
                this.SupportedScanKinds |= DeviceScanKind.FlatTwoFinger;                    
            }

            if (this.IsAvailable(devInfo.Index,enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS,enumLScanImageResolution.LSCAN_IMAGE_RESOLUTION_500))
            {
                this.SupportedScanKinds |= DeviceScanKind.FlatFourFinger;                    
            }

            if (this.IsAvailable(devInfo.Index,enumLScanImageType.LSCAN_PALM_LEFT_LOWER,enumLScanImageResolution.LSCAN_IMAGE_RESOLUTION_500))
            {
                this.SupportedScanKinds |= DeviceScanKind.FlatPartialPalm;                    
            }

            if (this.IsAvailable(devInfo.Index,enumLScanImageType.LSCAN_FLAT_SINGLE_FINGER,enumLScanImageResolution.LSCAN_IMAGE_RESOLUTION_1000))
            {
                this.SupportedResolutions |= PrintResolution.Dpi1000;
            }            
        }
        

        bool IsAvailable(int index, enumLScanImageType img, enumLScanImageResolution res)
        {
            try
            {
                int isAvailable = 0;
                this.CheckSdkError(LSE_SDK.LSCAN_Capture_IsModeAvailable(index, img, res, ref isAvailable));
                
                return isAvailable > 0;                
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        /// <summary>
        /// Chekc if there was a sdk error. If so, fills the LastException
        /// </summary>
        /// <param name="errorCode">int with the sdk error code</param>
        /// <returns>True if there was an error, false otherwise</returns>
        private bool CheckSdkError(int errorCode)
        {
            if (errorCode != LSE_ErrorCode.LSCAN_STATUS_OK)
            {
                bool isWarn = false;
                var errorKind = SdkErrorKind.SpecificError;
                var errorText = LSE_ErrorCode.returnErrorText(errorCode);
                isWarn = errorCode > 0;
                if (errorCode == LSE_ErrorCode.LSCAN_WRN_OPTICS_SURFACE_DIRTY)
                {
                    errorKind = SdkErrorKind.CaptureSurfaceDirty;
                    errorText = null;
                    isWarn = true;
                }
                else if (errorCode == LSE_ErrorCode.LSCAN_ERR_DEVICE_INSUFFICIENT_MEMORY)
                {
                    errorKind = SdkErrorKind.OutOfMemory;
                    errorText = null;
                }

                if (this.Properties.GetBoolValue(PropIgnoreWarning) && isWarn)
                {
                    this.OnDeviceSendMessage(errorText, DeviceMessageKind.Information, false);
                    Thread.Sleep(250);
                    return false;
                }

                this.LastException = new SdkException(this.DisplayName, "CheckSdkError", errorKind, errorText, isWarn, null);
                LogDispatcher.DoLog("Crossmatch DeviceApi CheckSdkError error", LogEventLevel.Warning, LastException);
                return true;
            }

            return false;
        }

        private bool SetCaptureMode()
        {
            this.Log("SetCaptureMode called");
            // set capture mode !
            int width, height, resX, resY;
            uint m_defaultCapOption = LSE_CaptureOptions.LSCAN_OPTION_AUTO_CAPTURE;
            var capturePrint  = this.currentScan;            

            if (this.CheckSdkError( LSE_SDK.LSCAN_Capture_SetMode(this.deviceHandle, capturePrint.ScanImageType, capturePrint.ScanResolution, enumLScanImageOrientation.LSCAN_IMAGE_TOP_DOWN, m_defaultCapOption, out width, out height, out resX, out resY))) {
                return false;
            }

            if (capturePrint.Kind == HandScanKind.Rolled)
            {
                return true;
            }

            //get Capture Area and object count to retrieve (4 fingers slap - missing finger(s) )
            Rectangle captureArea = new Rectangle(-1, -1, Math.Min(width, capturePrint.ScanSize.Width), Math.Min(capturePrint.ScanSize.Height, height));
            
            // to take picture of right size, ensure that capture area is clipped for 4 slaps and palms, If Supported !
            if (!captureArea.IsEmpty)
            {                
                var result = LSE_SDK.LSCAN_Capture_SetActiveArea(this.deviceHandle, captureArea.Left, captureArea.Y, captureArea.Width, captureArea.Height);

                if ((result == LSE_ErrorCode.LSCAN_ERR_INVALID_PARAM_VALUE) && (captureArea.X < 0) )
                {
                    // Do not support automatic clipping.. put manual clipping values !
                    // take bottom-centered images or top-centered
                    
                    var isBottom = this.Properties.GetBoolValue(PropCaptureRectBottom);

                    var clipArea = new Rectangle(
                            (int)((width / 2F) - (captureArea.Width / 2F)),
                             isBottom ? 
                                (height - captureArea.Height) : 
                                0,
                            captureArea.Width,
                            captureArea.Height);
                                        
                    result = LSE_SDK.LSCAN_Capture_SetActiveArea(this.deviceHandle, clipArea.X, clipArea.Y, clipArea.Width, clipArea.Height);
                }

                if (result != LSE_ErrorCode.LSCAN_STATUS_OK)
                {
                    this.LastException = new SdkException(this.DisplayName, "LSCAN_Capture_SetActiveArea", SdkErrorKind.SpecificError, "Cannot set capture Area : " + LSE_ErrorCode.returnErrorText(result));
                    return false;                    
                }
            }

            return true;
        }

        /// <summary>
        /// Define the fingers in order in which they are taken by the quality event
        /// </summary>
        private void SetHandFingers()
        {
            this.leftHandFingers = new List<PhysicalHandPart>();
            this.AddToListNonMissing(this.leftHandFingers, this.handParts.Single(x => x.Kind == HandPartKind.Finger && x.HandPart == HandPart.Little && x.Hand == Hand.Left));
            this.AddToListNonMissing(this.leftHandFingers, this.handParts.Single(x => x.Kind == HandPartKind.Finger && x.HandPart == HandPart.Ring && x.Hand == Hand.Left));
            this.AddToListNonMissing(this.leftHandFingers, this.handParts.Single(x => x.Kind == HandPartKind.Finger && x.HandPart == HandPart.Middle && x.Hand == Hand.Left));
            this.AddToListNonMissing(this.leftHandFingers, this.handParts.Single(x => x.Kind == HandPartKind.Finger && x.HandPart == HandPart.Index && x.Hand == Hand.Left));

            this.rightHandFingers = new List<PhysicalHandPart>();
            this.AddToListNonMissing(this.rightHandFingers, this.handParts.Single(x => x.Kind == HandPartKind.Finger && x.HandPart == HandPart.Index && x.Hand == Hand.Right));
            this.AddToListNonMissing(this.rightHandFingers, this.handParts.Single(x => x.Kind == HandPartKind.Finger && x.HandPart == HandPart.Middle && x.Hand == Hand.Right));
            this.AddToListNonMissing(this.rightHandFingers, this.handParts.Single(x => x.Kind == HandPartKind.Finger && x.HandPart == HandPart.Ring && x.Hand == Hand.Right));
            this.AddToListNonMissing(this.rightHandFingers, this.handParts.Single(x => x.Kind == HandPartKind.Finger && x.HandPart == HandPart.Little && x.Hand == Hand.Right));                                  
        }

        private void AddToListNonMissing(List<PhysicalHandPart> list, PhysicalHandPart part)
        {
            if (part.IsMissing)
            {
                return;
            }

            list.Add(part);
        }

        private int SetPrintsToCapture(PrintCaptureInformation capturePrint)
        {           
            var selfPart = this.handParts.SingleOrDefault(x => x.HandPart == capturePrint.Part && x.Hand == capturePrint.Hand);
            if (selfPart != null && selfPart.IsMissing)
            {
                return 0;
            }                        

            var prints = new List<PhysicalHandPart>();

            switch (capturePrint.Part)
            {
                case HandPart.TwoThumbs:
                    var thumbs = this.handParts.Where(x => x.Kind == HandPartKind.Finger && x.HandPart == HandPart.Thumb);
                    prints.AddRange(thumbs.Where(thumb => !thumb.IsMissing));

                    break;

                case HandPart.FourFlats:
                    prints.AddRange(capturePrint.Hand == Hand.Left ? this.leftHandFingers : this.rightHandFingers);                                        
                    break;

                case HandPart.UpperPalm:
                    prints.AddRange(capturePrint.Hand == Hand.Left ? this.leftHandFingers : this.rightHandFingers);                    
                    prints.Add(selfPart);
                    
                    break;

                default:                    
                    prints = new List<PhysicalHandPart> { selfPart };
                    break;
            }                        

            // define objects
            this.qualityParts = prints;

            return prints.Count();
        }

        private void ClearCallback()
        {
            this.sdkCallbackResultImage = null;
            LSE_SDK.LSCAN_Capture_RegisterCallbackResultImage(this.deviceHandle, null, IntPtr.Zero);

            this.sdkCallbackClearObjectFromPlaten = null;
            LSE_SDK.LSCAN_Capture_RegisterCallbackClearObjectsFromPlaten(this.deviceHandle, null, IntPtr.Zero);

            this.sdkCallbackQualityObject = null;
            LSE_SDK.LSCAN_Capture_RegisterCallbackObjectQuality(this.deviceHandle, null, IntPtr.Zero);

            this.sdkCallbackTakingResult = null;
            LSE_SDK.LSCAN_Capture_RegisterCallbackTakingResultImage(this.deviceHandle, null, IntPtr.Zero);

            this.sdkCallbackAquisitionComplete = null;
            LSE_SDK.LSCAN_Capture_RegisterCallbackAcquisitionComplete(this.deviceHandle, null, IntPtr.Zero);
            this.callbackSet = false;        
        }

        private bool SetCallback()
        {
            if (this.callbackSet)
            {
                return true;
            }

            this.sdkCallbackResultImage = this.DeviceCapturedPrint;
            if (this.CheckSdkError(LSE_SDK.LSCAN_Capture_RegisterCallbackResultImage(this.deviceHandle, this.sdkCallbackResultImage, IntPtr.Zero)))
            {
                return false;
            }

            this.sdkCallbackClearObjectFromPlaten = this.DeviceRemoveObjectFromPlatenCallback;
            if (this.CheckSdkError(LSE_SDK.LSCAN_Capture_RegisterCallbackClearObjectsFromPlaten(this.deviceHandle, this.sdkCallbackClearObjectFromPlaten, IntPtr.Zero)))
            {
                return false;
            }

            this.sdkCallbackQualityObject = this.DeviceObjectQualityCallback;
            if (this.CheckSdkError(LSE_SDK.LSCAN_Capture_RegisterCallbackObjectQuality(this.deviceHandle,this.sdkCallbackQualityObject, IntPtr.Zero)))
            {
                return false;
            }

            this.sdkCallbackTakingResult = this.DeviceImageAcquisitionStart;
            if (this.CheckSdkError(LSE_SDK.LSCAN_Capture_RegisterCallbackTakingResultImage(this.deviceHandle, this.sdkCallbackTakingResult , IntPtr.Zero)))
            {
                return false;
            }

            this.sdkCallbackAquisitionComplete = this.DeviceImageAcquisitionStop;
            if (this.CheckSdkError(LSE_SDK.LSCAN_Capture_RegisterCallbackAcquisitionComplete(this.deviceHandle, this.sdkCallbackAquisitionComplete, IntPtr.Zero)))
            {
                return false;
            }            
            
            
            this.sdkCallbackPreviewImage = this.DevicePreviewImage;
            if (this.CheckSdkError(LSE_SDK.LSCAN_Capture_RegisterCallbackPreviewImage(this.deviceHandle, this.sdkCallbackPreviewImage, IntPtr.Zero)))
            {
                return false;
            }

            this.sdkCallbackCommunicationBreak = this.DeviceCommunicationBreak;
            if (this.CheckSdkError(LSE_SDK.LSCAN_Main_RegisterCallbackCommunicationBreak(this.deviceHandle, this.sdkCallbackCommunicationBreak, IntPtr.Zero)))

            this.callbackSet = true;

            return true;
        }

        private void DeviceCommunicationBreak(int errorDeviceHandle, IntPtr pContext)
        {
            this.Log("DeviceCommunicationBreak called");
            this.OnDeviceSendMessage(CommonText.CommunicationBreakDetected, DeviceMessageKind.Error, false);
            try
            {
                LSE_SDK.LSCAN_Capture_Abort(this.deviceHandle);
                LSE_SDK.LSCAN_Controls_DisplayShowLogoScreen(this.deviceHandle, enumLScanDisplayLogoOption.LSCAN_DISPLAY_LOGO_OPTION_ERASE, 0);
                LSE_SDK.LSCAN_Main_Release(this.deviceHandle, 1);
            }
            catch (Exception ex)
            {
                LogDispatcher.DoLog("Crossmatch DeviceApi DeviceCommunicationBreak error", LogEventLevel.Warning, ex);
            }

            this.deviceHandle = -1;
            this.IsOpened = false;

            this.OnDeviceSendMessage(CommonText.CommunicationBreakReconnect, DeviceMessageKind.Error, true);

        }

        private void CallbackDeviceInitProgress(int deviceIndex, IntPtr pContext, enumLScanOperationType operationType, float progressValue)
        {
            int progressInt = (int)(progressValue * 100);
            LSE_SDK.LSCAN_Controls_DisplayShowLogoScreen(this.deviceHandle, enumLScanDisplayLogoOption.LSCAN_DISPLAY_LOGO_OPTION_SHOW_FW_VERSION, progressInt);

            if (operationType == enumLScanOperationType.LSCAN_OPERATION_INITIALIZATION)
            {
                if (progressInt < 99)
                {
                    var msg = string.Format("Device Initialization : {0} %", progressInt);
                    this.OnDeviceSendMessage(msg, DeviceMessageKind.Information, false);
                }
                else
                {                    
                    this.OnDeviceSendMessage(null, DeviceMessageKind.Information, false);
                }
                
            }
            
        }

        /// <summary>
        /// Callback for incoming image captured by scanner
        /// </summary>
        /// <param name="deviceHandle">
        /// The device Handle.
        /// </param>
        /// <param name="pContext">
        /// The p Context.
        /// </param>
        /// <param name="imageStatus">
        /// The image Status.
        /// </param>
        /// <param name="image">
        /// The image.
        /// </param>
        /// <param name="imageType">
        /// The image Type.
        /// </param>
        /// <param name="detectedObjects">
        /// The detected Objects.
        /// </param>
        /// <returns>
        /// </returns>
        public void DeviceCapturedPrint(int deviceHandle, IntPtr pContext, int imageStatus, ImageData image, enumLScanImageType imageType, int detectedObjects)
        {
            this.Log("DeviceCapturedPrint called");
            this.tftManage.DisplayWait();            

            if (this.SupportsLed)
            {
                LSE_SDK.LSCAN_Controls_SetActiveLEDs(this.deviceHandle, 0);
            }
            

            this.autoCaptureTrigger.StopTimeout();
                        
            // Create the bitmap for saving
            Bitmap imageBmp = ImageUtilities.BytesPointerToBitmap(image.Buffer, (int)image.Width, (int)image.Height, PixelFormat.Format8bppIndexed, this.currentScan.Resolution == PrintResolution.Dpi500 ? 500 : 1000);

            // now we call the thread ! The state is changed there too.
            Task.Factory.StartNew(() => this.OnDevicePrintCaptured(this.currentScan.Resolution, imageBmp));
        }        

        /// <summary>
        /// Writes down a countdown timer to show how many seconds before autocapture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void autoCaptureTrigger_OngoingTimeOut(object sender, OngoingTimeoutArgs e)
        {
            if (e.NoTimeLeft)
            {
                //this.DoBeep(3);
                var result = LSE_SDK.LSCAN_Capture_TakeResultImage(this.deviceHandle);
                //this.autoCaptureTrigger.InitializeCapture(4);
                Console.WriteLine(result);
                return;
            }

            if (e.Interrupted)
            {
                // erase overlay !
                LSE_SDK.LSCAN_Visualization_RemoveOverlay(this.deviceHandle, this.timeoutOverlayHandle);
                this.timeoutOverlayHandle = uint.MaxValue;

            }

            // only display timeout lesser thant the countdown start
            if (e.SecondRemaining > this.Properties.GetIntValue(PropAutoScanCountDownStart))
            {
                return;
            }

            const int posX = 30;
            const int posY = 50;
            var timeoutText = string.Format("Autocapture : {0}", e.SecondRemaining);
            System.Diagnostics.Debug.WriteLine("captureStateInfo_OngoingTimeOut " + DateTime.Now);

            if (this.timeoutOverlayHandle == uint.MaxValue)
            {
                COLORREF timeColor = new COLORREF();

                LSE_SDK.LSCAN_Visualization_AddOverlayText(this.deviceHandle,
                                                           timeoutText,
                                                           posX, posY, timeColor, "Arial", 12, 0, out this.timeoutOverlayHandle);
            }
            else
            {
                LSE_SDK.LSCAN_Visualization_ModifyOverlayText(this.deviceHandle, this.timeoutOverlayHandle, timeoutText, posX, posY);
            }
        }

        void DoBeep(int pattern)
        {
            if (this.Properties.GetBoolValue(PropUseSystemSound))
            {                
                DeviceSoundPlayer.Play(DeviceSound.Beep);             
            }

            if (!this.Properties.GetBoolValue(PropBeebEnabled))
            {
                return;
            }

            //#if DEBUG
            //            // no beep in debug
            //            return;
            //#endif

            if (!this.beeperDefined)
            {
                this.beeperDefined = true;
                if (LSE_SDK.LSCAN_Controls_GetAvailableBeeper(this.deviceHandle, out this.beeperType) != LSE_ErrorCode.LSCAN_STATUS_OK)
                {
                    this.beeperType = enumLScanBeeperType.LSCAN_BEEPER_NONE;
                }
            }

            if ((pattern > 7) || (pattern < 0))
            {
                throw new ArgumentOutOfRangeException("pattern");
            }

            if (this.beeperType == enumLScanBeeperType.LSCAN_BEEPER_NONE) { return; }

            LSE_SDK.LSCAN_Controls_Beeper(this.deviceHandle, pattern, 50);
        }

        void ResetOverlays()
        {
            this.msgOverlayHandle = uint.MaxValue;
            this.timeoutOverlayHandle = uint.MaxValue;
            LSE_SDK.LSCAN_Visualization_RemoveAllOverlays(this.deviceHandle);
        }

        void ShowPrintsToCapture()
        {
            var qualityInformation = new List<PrintCaptureQuality>();
            var partCount = this.qualityParts.Count;

            for (var index = 0; index < partCount; index++)
            {
                qualityInformation.Add(new PrintCaptureQuality(this.qualityParts[index], PrintQualityLevel.NotPresent));
            }

            this.CaptureQualityChanged?.Invoke(qualityInformation);
        }

        void DeviceObjectQualityCallback(int deviceHandle, IntPtr pContext, IntPtr pQualityArray, int qualityCount)
        {
            if (deviceHandle != this.deviceHandle) return;

            var qualityInformation = new List<PrintCaptureQuality>();

            // 1 --> Create managed array
            //enumLScanObjectQualityState[] objectQualities = new enumLScanObjectQualityState[qualityCount];
            var tempQuality = new int[qualityCount];

            // 2 --> copy to managed array [copy to type enum array doesn't work.. we have to get through a int[] array... dumb..]
            Marshal.Copy(pQualityArray, tempQuality, 0, qualityCount);

            var partCount = this.qualityParts.Count;
            var partIndex = 0;
            var presentCount = 0;
            
            for (var deviceQualIndex = 0; deviceQualIndex < qualityCount; deviceQualIndex++)
            {
                PrintQualityLevel qualLevel;

                switch ((enumLScanObjectQualityState)tempQuality[deviceQualIndex])
                {
                    case enumLScanObjectQualityState.LSCAN_OBJECT_POSITION_NOT_OK:
                    case enumLScanObjectQualityState.LSCAN_OBJECT_POSITION_TOO_HIGH:
                    case enumLScanObjectQualityState.LSCAN_OBJECT_POSITION_TOO_LEFT:
                    case enumLScanObjectQualityState.LSCAN_OBJECT_POSITION_TOO_RIGHT:
                    case enumLScanObjectQualityState.LSCAN_OBJECT_CORE_NOT_PRESENT:
                    case enumLScanObjectQualityState.LSCAN_OBJECT_FLEX_POSITION_TOO_HIGH:
                    case enumLScanObjectQualityState.LSCAN_OBJECT_FLEX_POSITION_TOO_LEFT:
                    case enumLScanObjectQualityState.LSCAN_OBJECT_FLEX_POSITION_TOO_RIGHT:
                    case enumLScanObjectQualityState.LSCAN_OBJECT_FLEX_POSITION_TOO_LOW:
                        qualLevel = PrintQualityLevel.Bad;
                        break;

                    case enumLScanObjectQualityState.LSCAN_OBJECT_TOO_DARK:
                    case enumLScanObjectQualityState.LSCAN_OBJECT_TOO_LIGHT:
                    case enumLScanObjectQualityState.LSCAN_OBJECT_BAD_SHAPE:
                        qualLevel = PrintQualityLevel.Bad;
                        break;

                    case enumLScanObjectQualityState.LSCAN_OBJECT_TRACKING_NOT_OK:
                        qualLevel = PrintQualityLevel.NotGoodEnough;
                        break;

                    case enumLScanObjectQualityState.LSCAN_OBJECT_GOOD:
                        qualLevel = PrintQualityLevel.Good;
                        break;
                    case enumLScanObjectQualityState.LSCAN_OBJECT_NOT_PRESENT:
                        qualLevel = PrintQualityLevel.NotPresent;
                        break;
                    default:
                        qualLevel = PrintQualityLevel.NotPresent;
                        break;
                } //  switch( tempQuality )
                                
                if (qualLevel != PrintQualityLevel.NotPresent)
                {
                    var key = (deviceQualIndex + 1).ToString() + qualLevel.ToString();                    

                    var qualObject = 
                    partIndex < partCount
                        ? new PrintCaptureQuality(this.qualityParts[partIndex], qualLevel)
                        : new PrintCaptureQuality(
                              new PhysicalHandPart(HandPart.Unknown, Hand.None, HandPartKind.Other), qualLevel);

                    this.tftManage.DefineQuality(qualObject.HandPart.HandPart, qualObject.HandPart.Hand, qualLevel);

                    qualityInformation.Add(qualObject);
                    presentCount += 1;
                    partIndex += 1;
                }
            }

            // if parts are missing, display it
            for (var index = partIndex; index < partCount; index++)
            {
                qualityInformation.Add(
                    new PrintCaptureQuality(this.qualityParts[partIndex], PrintQualityLevel.NotPresent));
            }

            this.ledManage.DisplayQuality(qualityInformation);
            this.tftManage.DisplayQuality();

            this.autoCaptureTrigger.StartTimeout(presentCount);

            this.CaptureQualityChanged?.Invoke(qualityInformation);                        
        }

        
        

        void DeviceRemoveObjectFromPlatenCallback(int deviceHandle, IntPtr pContext, enumLScanPlatenState state)
        {
            if (deviceHandle != this.deviceHandle) return;

            if (state == enumLScanPlatenState.LSCAN_CLEAR_OBJECT_FROM_PLATEN)
            {
                var color = new COLORREF { B = 255 };

                int error = LSE_ErrorCode.LSCAN_STATUS_OK;

                var msg = "Remove Object From Platen";

                if (this.msgOverlayHandle == uint.MaxValue)
                {

                    error = LSE_SDK.LSCAN_Visualization_AddOverlayText(this.deviceHandle, msg, 10, 10, color, "Arial", 10, LSE_Constants.BOOL_TRUE, out this.msgOverlayHandle);
                }
                else
                {
                    error = LSE_SDK.LSCAN_Visualization_ModifyOverlayText(this.deviceHandle, this.msgOverlayHandle, msg, 10, 10);
                }
            }
            else
            {
                if (this.msgOverlayHandle != uint.MaxValue)
                {
                    LSE_SDK.LSCAN_Visualization_RemoveOverlay(this.deviceHandle, this.msgOverlayHandle);
                }
            }
        }

        void DevicePreviewImage(int deviceHandle, IntPtr pContext, ImageData image)
        {
            this.ChangeState(DeviceState.Scanning);
            if (this.capturePreviewWindowHandle != 0)
            {
                return;
            }

            Bitmap imageBmp = ImageUtilities.BytesPointerToBitmap(image.Buffer, (int)image.Width, (int)image.Height, PixelFormat.Format8bppIndexed, this.currentScan.Resolution.ToDpi());            

            // now we call the thread !
            Task.Factory.StartNew(() => this.capturePreviewHandler(imageBmp));
        }

        void DeviceImageAcquisitionStart(int deviceHandle, IntPtr context)
        {
            this.autoCaptureTrigger.StopTimeout();
            this.DoBeep(3);
        }

        void DeviceImageAcquisitionStop(int deviceHandle, IntPtr context)
        {
            this.DoBeep(3);
        }

        void Log(string text)
        {
            Console.WriteLine(@"{0:yyyy-MM-dd HH:mm:ss} - DeveiApiCrossmatch - {1}", DateTime.Now, text);
        }

        #endregion

        #region Values definitions

        private void AdaptSupportedPrints(bool isFlat)
        {
            var FourSlap500 = new Size(1600, isFlat ? 1500 : 1000);
            var FourSlap1000 = new Size(3200, isFlat ? 3000 : 2000);

            foreach (var source in this.supportedPrints.Where(x => x.Kind == HandScanKind.Flat && x.Part == HandPart.FourFlats && x.Resolution == PrintResolution.Dpi500))
            {
                source.ScanSize = FourSlap500;
            }

            foreach (var source in this.supportedPrints.Where(x => x.Kind == HandScanKind.Flat && x.Part == HandPart.FourFlats && x.Resolution == PrintResolution.Dpi1000))
            {
                source.ScanSize = FourSlap1000;
            }
        }

        private void CreateSupportedPrintList()
        {
            var FourSlap500 = new Size(1600, 1500); // for type 4, height = 1000
            var TwoThumbs500 = new Size(1600,1500);
            
            var SingleSlap500 = new Size(500, 1000);
            var PartialPalm500 = new Size(2750, 2750);
            var Hypothenar500 = new Size(900, 2500);
            var SingleRoll500 = new Size(800, 750);

            var FourSlap1000 = new Size(3200, 3000); // for type 4, Height = 2000 !
            var TwoThumbs1000 = new Size(3200, 3000);
            var SingleSlap1000 = new Size(1000, 2000);
            var PartialPalm1000 = new Size(5500, 5500);
            var Hypothenar1000 = new Size(1800, 5000);
            var SingleRoll1000 = new Size(1600, 1500);

            this.supportedPrints = new List<PrintCaptureInformation>();

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.FourFlats, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS, PrintResolution.Dpi500, FourSlap500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.FourFlats, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_RIGHT_FINGERS, PrintResolution.Dpi500, FourSlap500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.None, HandPart.TwoThumbs, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_TWO_THUMBS, PrintResolution.Dpi500, TwoThumbs500));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Thumb, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS, PrintResolution.Dpi500, SingleSlap500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Index, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS, PrintResolution.Dpi500, SingleSlap500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Middle, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS, PrintResolution.Dpi500, SingleSlap500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Ring, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS, PrintResolution.Dpi500, SingleSlap500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Little, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS, PrintResolution.Dpi500, SingleSlap500));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Thumb, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_RIGHT_FINGERS, PrintResolution.Dpi500, SingleSlap500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Index, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_RIGHT_FINGERS, PrintResolution.Dpi500, SingleSlap500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Middle, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_RIGHT_FINGERS, PrintResolution.Dpi500, SingleSlap500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Ring, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_RIGHT_FINGERS, PrintResolution.Dpi500, SingleSlap500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Little, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_RIGHT_FINGERS, PrintResolution.Dpi500, SingleSlap500));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Thumb, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi500, SingleRoll500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Index, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi500, SingleRoll500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Middle, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi500, SingleRoll500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Ring, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi500, SingleRoll500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Little, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi500, SingleRoll500));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Thumb, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi500, SingleRoll500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Index, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi500, SingleRoll500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Middle, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi500, SingleRoll500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Ring, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi500, SingleRoll500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Little, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi500, SingleRoll500));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.UpperPalm, HandScanKind.Flat, enumLScanImageType.LSCAN_PALM_LEFT_UPPER, PrintResolution.Dpi500, PartialPalm500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.LowerPalm, HandScanKind.Flat, enumLScanImageType.LSCAN_PALM_LEFT_LOWER, PrintResolution.Dpi500, PartialPalm500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Hypothenar, HandScanKind.Flat, enumLScanImageType.LSCAN_PALM_LEFT_WRITERS, PrintResolution.Dpi500, Hypothenar500));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.UpperPalm, HandScanKind.Flat, enumLScanImageType.LSCAN_PALM_RIGHT_UPPER, PrintResolution.Dpi500, PartialPalm500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.LowerPalm, HandScanKind.Flat, enumLScanImageType.LSCAN_PALM_RIGHT_LOWER, PrintResolution.Dpi500, PartialPalm500));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Hypothenar, HandScanKind.Flat, enumLScanImageType.LSCAN_PALM_RIGHT_WRITERS, PrintResolution.Dpi500, Hypothenar500));


            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.FourFlats, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS, PrintResolution.Dpi1000, FourSlap1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.FourFlats, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_RIGHT_FINGERS, PrintResolution.Dpi1000, FourSlap1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.None, HandPart.TwoThumbs, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_TWO_THUMBS, PrintResolution.Dpi1000, TwoThumbs1000));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Thumb, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS, PrintResolution.Dpi1000, SingleSlap1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Index, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS, PrintResolution.Dpi1000, SingleSlap1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Middle, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS, PrintResolution.Dpi1000, SingleSlap1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Ring, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS, PrintResolution.Dpi1000, SingleSlap1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Little, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_LEFT_FINGERS, PrintResolution.Dpi1000, SingleSlap1000));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Thumb, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_RIGHT_FINGERS, PrintResolution.Dpi1000, SingleSlap1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Index, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_RIGHT_FINGERS, PrintResolution.Dpi1000, SingleSlap1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Middle, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_RIGHT_FINGERS, PrintResolution.Dpi1000, SingleSlap1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Ring, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_RIGHT_FINGERS, PrintResolution.Dpi1000, SingleSlap1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Little, HandScanKind.Flat, enumLScanImageType.LSCAN_FLAT_RIGHT_FINGERS, PrintResolution.Dpi1000, SingleSlap1000));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Thumb, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi1000, SingleRoll1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Index, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi1000, SingleRoll1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Middle, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi1000, SingleRoll1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Ring, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi1000, SingleRoll1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Little, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi1000, SingleRoll1000));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Thumb, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi1000, SingleRoll1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Index, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi1000, SingleRoll1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Middle, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi1000, SingleRoll1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Ring, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi1000, SingleRoll1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Little, HandScanKind.Rolled, enumLScanImageType.LSCAN_ROLL_SINGLE_FINGER, PrintResolution.Dpi1000, SingleRoll1000));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.UpperPalm, HandScanKind.Flat, enumLScanImageType.LSCAN_PALM_LEFT_UPPER, PrintResolution.Dpi1000, PartialPalm1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.LowerPalm, HandScanKind.Flat, enumLScanImageType.LSCAN_PALM_LEFT_LOWER, PrintResolution.Dpi1000, PartialPalm1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Hypothenar, HandScanKind.Flat, enumLScanImageType.LSCAN_PALM_LEFT_WRITERS, PrintResolution.Dpi1000, Hypothenar1000));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.UpperPalm, HandScanKind.Flat, enumLScanImageType.LSCAN_PALM_RIGHT_UPPER, PrintResolution.Dpi1000, PartialPalm1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.LowerPalm, HandScanKind.Flat, enumLScanImageType.LSCAN_PALM_RIGHT_LOWER, PrintResolution.Dpi1000, PartialPalm1000));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Hypothenar, HandScanKind.Flat, enumLScanImageType.LSCAN_PALM_RIGHT_WRITERS, PrintResolution.Dpi1000, Hypothenar1000));
        }

        private void CreatePropertyList()
        {
            this.Properties = new CustomPropertyList();

            this.Properties.AddBoolProperty(PropBeebEnabled, CommonText.PropertyEnabled, true, CommonText.PropertyDeviceSound);
            this.Properties.AddBoolProperty(PropIgnoreWarning,CommonText.PropertyEnabled, false, "Warning - Ignore");
            this.Properties.AddBoolProperty(PropAutoScanEnabled, CommonText.PropertyEnabled, true, CommonText.PropertyAutoCapture);
            this.Properties.AddRangeProperty(PropAutoScanDelay, CommonText.PropertyDelay, 10, 3, 60, CommonText.PropertyAutoCapture);
            this.Properties.AddRangeProperty(PropAutoScanCountDownStart, CommonText.PropertyCountDown, 3, 3, 10, CommonText.PropertyAutoCapture);
            this.Properties.AddBoolProperty(PropUseSystemSound, CommonText.PropertyEnabled, false, CommonText.PropertyComputerSound);
            this.Properties.AddBoolProperty(PropSystemSoundMuted, CommonText.PropertyMuted, false, CommonText.PropertyComputerSound);
            this.Properties.AddBoolProperty(PropCaptureRectBottom, CommonText.PropertyAtBottom, true, CommonText.PropertyCaptureRectangle);
        }

        

        #endregion

        class PrintCaptureInformation
        {
            public PrintCaptureInformation(Hand hand, HandPart part, HandScanKind kind, enumLScanImageType scanImageType, PrintResolution resolution, Size scanSize)
            {
                // set key fiedls
                this.Hand = hand;
                this.Part = part;
                this.Kind = kind;                
                this.Resolution = resolution;

                // set sdk fields value
                this.ScanImageType = scanImageType;
                this.ScanResolution = resolution == PrintResolution.Dpi500 ? enumLScanImageResolution.LSCAN_IMAGE_RESOLUTION_500 : enumLScanImageResolution.LSCAN_IMAGE_RESOLUTION_1000;
                this.ScanSize = scanSize;
            }

            public enumLScanImageType ScanImageType { get; private set; }
            public enumLScanImageResolution ScanResolution { get; private set; }
            public Size ScanSize { get; set; }

            public Hand Hand { get; private set; }
            public HandPart Part { get; private set; }
            public HandScanKind Kind { get; private set; }
            public PrintResolution Resolution { get;private set; }
                        
        }


          
    }
}
