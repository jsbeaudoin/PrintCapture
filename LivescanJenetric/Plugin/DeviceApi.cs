using System;
using System.Collections.Generic;

using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using PrintsCapture.Device.Enum;
using PrintsCapture.Device.Interface;
using PrintsCapture.Device.LivescanJenetric.Sdk;
using PrintsCapture.Prints;
using PrintsCapture.Prints.Enum;
using TouchLab.NET;
using XL_ID.Utilities.Log;

namespace PrintsCapture.Device.LivescanJenetric.Plugin
{
    public class DeviceApi : ILivescanDevice
    {
        private TouchLabApi touchApi;

        private SdkApi jenetricSdk;

        private IEnumerable<PhysicalHandPart> handParts;
        private CapturePreviewHandler preview;
        private bool isFlat;

        private List<PrintCaptureQuality> printsToCapture;

        private bool deviceOpened;

        private HandScanKind currentScanKind;

        private bool isStopping;

        private PrintQualityLevel qualityIndicator = PrintQualityLevel.Good;


        internal DeviceApi(string internalName, string friendlyName, PrintResolution resolutions, DeviceScanKind scanKinds, string imageUri, SdkApi sdk)
        {
            this.jenetricSdk = sdk;
            //this.touchApi = TouchLabApi.Instance; //sdk.api;
            this.touchApi = sdk.api;
            this.InternalKey = internalName;

            this.HardwareMake = SdkApi.DeviceMake.ToUpper();
            this.DisplayName = friendlyName;

            this.SupportedResolutions = resolutions;
            this.SupportedScanKinds = scanKinds;

            this.Properties = new CustomPropertyList();
            this.ExternalTool = "Checkbox.exe";

            this.ImageUri = imageUri;

            this.Properties = new CustomPropertyList();            
        }

        #region Properties set by constructor

        public CustomPropertyList Properties { get; private set; }           

        public string ExternalTool { get; private set; }

        public string DisplayName { get; private set; }


        public string InternalKey { get; private set; }

        public string HardwareMake { get; private set; }

        public string ModelName { get; private set; }

        public string ImageUri { get; private set; }

        public PrintResolution SupportedResolutions { get; private set; }

        public DeviceScanKind SupportedScanKinds { get; private set; }

        public ICaptureSdk Sdk { get { return this.jenetricSdk; } }

        #endregion

        private Scanner scanner;
        private ScanMode scanMode;
        private FingerPositions fingerPositions;
        private ImageContentAnalysis disabledContentAnalysis = ImageContentAnalysis.NONE;

        public string SerialNumber { get; private set; }

        

        public PrintResolution FingerResolution { get; set; }
        public PrintResolution PalmResolution { get; set; }

        public SdkException LastException { get; private set; }

        public DeviceState State { get; private set; }

        public bool IsOpened { get; private set; }

        public event DeviceMessageHandler DeviceSendMessage;
        public event DeviceOpenedHandler DeviceOpened;
        public event CapturedPrintHandler PrintCaptured;
        public event CaptureQualityHandler CaptureQualityChanged;
        public event DeviceStateChangedHandler StateChanged;

        public bool CapturePrint(PrintResolution resolution, Hand printHand, HandPart handPart, HandScanKind scanKind)
        {
            LogDispatcher.DoLog($"CapturePrint {printHand} {handPart} - {scanKind} @{resolution}");
            this.OnQualityChanged(true);
            this.ChangeState(DeviceState.ScanInitialization);

            if(this.scanner == null || this.scanner.IsDisposed) this.Open();

            this.printsToCapture = new List<PrintCaptureQuality>();

            this.OnDeviceSendMessage("", DeviceMessageKind.Information, false);

            this.scanMode = getScanModeFromCapture(printHand, handPart, scanKind);
            this.fingerPositions = getFingerPositionFromCapture(printHand, handPart, scanKind);

            this.currentScanKind = scanKind;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            this.scanner.PreviewImageAvailable += Scanner_PreviewImageAvailable;
            this.scanner.ScanImageAvailable += Scanner_ScanImageAvailable;

            try
            {
                this.OnQualityChanged();
                if(this.scanner.Capabilities.SupportsBeep) this.scanner.Beep();
                this.scanner.StartAutoScan(this.scanMode, this.fingerPositions, this.disabledContentAnalysis);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LogDispatcher.DoLog("CapturePrint Error", LogEventLevel.Error, e);
            }

            return true;
        }

        public void Close()
        {
            this.ChangeState(DeviceState.Closing);
            this.StopCapture();
            this.ChangeState(DeviceState.Closed);
            this.scanner.Dispose();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }

        public bool InitializeCapture(IEnumerable<PhysicalHandPart> handParts, CapturePreviewHandler preview, int previewWindowHandle, bool isFlat)
        {
            this.handParts = handParts;
            this.preview = preview;
            this.isFlat = isFlat;
            return true;
        }

        public void Open()
        {
            this.deviceOpened = false;
            var currentScannerInfo = this.touchApi.GetScannerList().FirstOrDefault(x => x.ScannerTypeName == this.InternalKey);
            if (currentScannerInfo.ScannerTypeName != this.InternalKey)
            {
                this.LastException = new SdkException(this.DisplayName, "Open", SdkErrorKind.DeviceNotFound, "Not in sdk scanner list!");
                this.OnDeviceOpened(DeviceOpenStatus.ErrorOccured);
                LogDispatcher.DoLog($"Open error", LogEventLevel.Error, this.LastException);
                return;
            }

            this.SerialNumber = currentScannerInfo.Serial;
            this.ModelName = currentScannerInfo.ScannerTypeName;

            try
            {
                if (this.scanner == null)
                {
                    this.ChangeState(DeviceState.Opening);
                    this.scanner = touchApi.OpenScanner(this.SerialNumber);
                    this.scanner.CallbackFailed += Scanner_CallbackFailed;
                    this.scanner.ShowStartScreen(Workflow.WORKFLOW_442);
                    if(this.scanner.Capabilities.SupportsBeep) this.scanner.Beep();
                    this.IsOpened = true;
                }
            }
            catch (Exception ex)
            {
                this.LastException = new SdkException(this.DisplayName, "Open", SdkErrorKind.DeviceInitializationFailed, "Error from the Sdk while opening device !", false, ex);
                this.OnDeviceOpened(DeviceOpenStatus.ErrorOccured);
                LogDispatcher.DoLog($"Open error", LogEventLevel.Error, this.LastException);
                this.OnDeviceSendMessage(ex.Message, DeviceMessageKind.Error, true);
                return;
            }

            this.OnDeviceOpened(DeviceOpenStatus.Success);
            this.deviceOpened = true;
            this.ChangeState(DeviceState.Opened);
        }

        private void Scanner_CallbackFailed(object sender, CallbackFailedEventArgs e)
        {
            LogDispatcher.DoLog("Scanner Callback Failed", LogEventLevel.Error, e.Exception);
        }

        public bool StopCapture()
        {
            this.scanner.PreviewImageAvailable -= Scanner_PreviewImageAvailable;
            this.scanner.ScanImageAvailable -= Scanner_ScanImageAvailable;
            this.scanner.CallbackFailed -= Scanner_CallbackFailed;
            this.scanner.ShowEndScreen(Workflow.WORKFLOW_442);
            this.ChangeState(DeviceState.Closed);
            return true;
        }

        #region Interface event handler

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
            this.PrintCaptured?.BeginInvoke(resolution, img, null, null);
        }

        //  support quality feedback
        private void OnQualityChanged(bool reset = false)
        {
            this.CaptureQualityChanged?.BeginInvoke(reset ? null :  this.printsToCapture, null, null);
        }

        #endregion

        #region Sdk event handler

        private void Scanner_ScanImageAvailable(object sender, ImageAvailableEventArgs e)
        {
            try
            {
                this.OnDeviceSendMessage("Processing fingerprint, please wait ...", DeviceMessageKind.Information, false);
                this.scanner.Beep();
                this.scanner.PreviewImageAvailable -= Scanner_PreviewImageAvailable;
                this.scanner.ScanImageAvailable -= Scanner_ScanImageAvailable;

                //this.preview.BeginInvoke(new Bitmap(200, 200), null,null);
                this.preview.BeginInvoke(
                    XL_ID.Utilities.Image.ImageUtilities.ConvertToIndexedFormat(new Bitmap(200, 200),
                        XL_ID.Utilities.Image.ConvertBitmapFormat.Format8bppIndexed), null, null);
                this.OnPrintCaptured(this.FingerResolution, e.Image.ToBitmap());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                LogDispatcher.DoLog("Scanner_ScanImageAvailable : ", LogEventLevel.Error, exception);
            }
        }

        private void Scanner_PreviewImageAvailable(object sender, ImageAvailableEventArgs e)
        {
            if (e.Image != null)
            {
                this.ChangeState(DeviceState.Scanning);
                if (e.Feedback == ImageContentFeedback.CONTENT_VALID)
                {
                    this.qualityIndicator = PrintQualityLevel.Good;
                }
                else if (e.Feedback == ImageContentFeedback.NO_FINGERS_DETECTED || e.Feedback == ImageContentFeedback.FINGERTIPS || e.Feedback == ImageContentFeedback.NOT_ENOUGH_FINGERS)
                {
                    this.qualityIndicator = PrintQualityLevel.NotPresent;
                }
                else if (e.Feedback == ImageContentFeedback.HAND_ON_BORDER_BOTTOM ||
                         e.Feedback == ImageContentFeedback.HAND_ON_BORDER_LEFT ||
                         e.Feedback == ImageContentFeedback.HAND_ON_BORDER_RIGHT ||
                         e.Feedback == ImageContentFeedback.HAND_ON_BORDER_TOP)
                {
                    this.qualityIndicator = PrintQualityLevel.NotGoodEnough;
                }
                else
                {
                    this.qualityIndicator = PrintQualityLevel.Bad;
                }
                this.printsToCapture.ForEach(x=> x.Quality = this.qualityIndicator);
                this.OnQualityChanged();

                this.preview.BeginInvoke(e.Image.ToBitmap(), null, null);
            }
        }

        #endregion

        #region Sdk helper methods

        private ScanMode getScanModeFromCapture(Hand printHand, HandPart handPart, HandScanKind scanKind)
        {
            ScanMode value = 0;

            if (handPart == HandPart.TwoThumbs)
            {
                value = ScanMode.PLAIN_2_THUMBS;
                this.printsToCapture.AddRange(this.handParts.Where(x => x.HandPart == HandPart.Thumb && !x.IsMissing).Select(y => new PrintCaptureQuality(y, qualityIndicator)));
            } else if (handPart == HandPart.FourFlats)
            {
                value = printHand == Hand.Left ? ScanMode.PLAIN_4_FINGERS_LEFT : ScanMode.PLAIN_4_FINGERS_RIGHT;
                this.printsToCapture.AddRange(this.handParts.Where(x => x.Hand == printHand && x.HandPart != HandPart.Thumb && x.Kind == HandPartKind.Finger && !x.IsMissing).Select(y => new PrintCaptureQuality(y, qualityIndicator)));
            }
            else
            {
                if (printHand == Hand.Left)
                {
                    if (handPart == HandPart.Thumb) value = scanKind == HandScanKind.Flat ? ScanMode.PLAIN_1_THUMB_LEFT : ScanMode.ROLLED_1_THUMB_LEFT;
                    if (handPart == HandPart.Ring) value = scanKind == HandScanKind.Flat ? ScanMode.PLAIN_1_RING_FINGER_LEFT : ScanMode.ROLLED_1_RING_FINGER_LEFT;
                    if (handPart == HandPart.Middle) value = scanKind == HandScanKind.Flat ? ScanMode.PLAIN_1_MIDDLE_FINGER_LEFT : ScanMode.ROLLED_1_MIDDLE_FINGER_LEFT;
                    if (handPart == HandPart.Little) value = scanKind == HandScanKind.Flat ? ScanMode.PLAIN_1_LITTLE_FINGER_LEFT : ScanMode.ROLLED_1_LITTLE_FINGER_LEFT;
                    if (handPart == HandPart.Index) value = scanKind == HandScanKind.Flat ? ScanMode.PLAIN_1_INDEX_FINGER_LEFT : ScanMode.ROLLED_1_INDEX_FINGER_LEFT;
                }
                if (printHand == Hand.Right)
                {
                    if (handPart == HandPart.Thumb) value = scanKind == HandScanKind.Flat ? ScanMode.PLAIN_1_THUMB_RIGHT : ScanMode.ROLLED_1_THUMB_RIGHT;
                    if (handPart == HandPart.Ring) value = scanKind == HandScanKind.Flat ? ScanMode.PLAIN_1_RING_FINGER_RIGHT : ScanMode.ROLLED_1_RING_FINGER_RIGHT;
                    if (handPart == HandPart.Middle) value = scanKind == HandScanKind.Flat ? ScanMode.PLAIN_1_MIDDLE_FINGER_RIGHT : ScanMode.ROLLED_1_MIDDLE_FINGER_RIGHT;
                    if (handPart == HandPart.Little) value = scanKind == HandScanKind.Flat ? ScanMode.PLAIN_1_LITTLE_FINGER_RIGHT : ScanMode.ROLLED_1_LITTLE_FINGER_RIGHT;
                    if (handPart == HandPart.Index) value = scanKind == HandScanKind.Flat ? ScanMode.PLAIN_1_INDEX_FINGER_RIGHT : ScanMode.ROLLED_1_INDEX_FINGER_RIGHT;
                }
                this.printsToCapture.AddRange(this.handParts.Where(x => x.Hand == printHand && x.HandPart == handPart && !x.IsMissing).Select(y => new PrintCaptureQuality(y, qualityIndicator)));
            }

            return value;
        }

        // transform PrintCapture hand Part component into Jenetric FingerSet
        private FingerPositions getFingerPositionFromCapture(Hand printHand, HandPart handPart, HandScanKind scanKind)
        {
            int value = 0;
            // add all non missing fingers to the set
            if (handPart == HandPart.TwoThumbs)
            {
                value |= (this.handParts.Any(x => x.Hand == Hand.Left && x.HandPart == HandPart.Thumb && !x.IsMissing) ? (int)FingerPositions.LEFT_THUMB : 0);
                value |= (this.handParts.Any(x => x.Hand == Hand.Right && x.HandPart == HandPart.Thumb && !x.IsMissing) ? (int)FingerPositions.RIGHT_THUMB : 0);
            }
            else if (handPart == HandPart.FourFlats)
            {
                if (printHand == Hand.Left)
                {
                    value |= (this.handParts.Any(x => x.Hand == printHand && x.HandPart == HandPart.Index && !x.IsMissing) ? (int)FingerPositions.LEFT_INDEX_FINGER : 0);
                    value |= (this.handParts.Any(x => x.Hand == printHand && x.HandPart == HandPart.Middle && !x.IsMissing) ? (int)FingerPositions.LEFT_MIDDLE_FINGER : 0);
                    value |= (this.handParts.Any(x => x.Hand == printHand && x.HandPart == HandPart.Ring && !x.IsMissing) ? (int)FingerPositions.LEFT_RING_FINGER : 0);
                    value |= (this.handParts.Any(x => x.Hand == printHand && x.HandPart == HandPart.Little && !x.IsMissing) ? (int)FingerPositions.LEFT_LITTLE_FINGER : 0);
                }
                else
                {
                    value |= (this.handParts.Any(x => x.Hand == printHand && x.HandPart == HandPart.Index && !x.IsMissing) ? (int)FingerPositions.RIGHT_INDEX_FINGER : 0);
                    value |= (this.handParts.Any(x => x.Hand == printHand && x.HandPart == HandPart.Middle && !x.IsMissing) ? (int)FingerPositions.RIGHT_MIDDLE_FINGER : 0);
                    value |= (this.handParts.Any(x => x.Hand == printHand && x.HandPart == HandPart.Ring && !x.IsMissing) ? (int)FingerPositions.RIGHT_RING_FINGER : 0);
                    value |= (this.handParts.Any(x => x.Hand == printHand && x.HandPart == HandPart.Little && !x.IsMissing) ? (int)FingerPositions.RIGHT_LITTLE_FINGER : 0);
                }

            }
            else
            {
                if (printHand == Hand.Left)
                {
                    if (handPart == HandPart.Thumb) value = (int) FingerPositions.LEFT_THUMB;
                    if (handPart == HandPart.Ring) value = (int) FingerPositions.LEFT_RING_FINGER;
                    if (handPart == HandPart.Middle) value = (int) FingerPositions.LEFT_MIDDLE_FINGER;
                    if (handPart == HandPart.Little) value = (int) FingerPositions.LEFT_LITTLE_FINGER;
                    if (handPart == HandPart.Index) value = (int) FingerPositions.LEFT_INDEX_FINGER;
                }
                if (printHand == Hand.Right)
                {
                    if (handPart == HandPart.Thumb) value = (int) FingerPositions.RIGHT_THUMB;
                    if (handPart == HandPart.Ring) value = (int) FingerPositions.RIGHT_RING_FINGER;
                    if (handPart == HandPart.Middle) value = (int) FingerPositions.RIGHT_MIDDLE_FINGER;
                    if (handPart == HandPart.Little) value = (int) FingerPositions.RIGHT_LITTLE_FINGER;
                    if (handPart == HandPart.Index) value = (int) FingerPositions.RIGHT_INDEX_FINGER;
                }
            }
            return (FingerPositions)value;
        }

        #endregion

    }
}
