using System.ComponentModel;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using XL_ID.Utilities.Image;

namespace PrintsCapture.Device.LivescanVirtual.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using PrintsCapture.Device.DataLayer;
    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Device.LivescanVirtual.Properties;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;

    using XL_ID.Utilities.XML;

    public class DeviceApi : ILivescanDevice
    {
        private const string PropUseFileDialog = "PROP_FILE_DIALOG";

        private CapturePreviewHandler preview;        

        private IEnumerable<PhysicalHandPart> handParts;

        private List<PrintCaptureInformation> supportedPrints;

        private string lastFolder;

        private int openCount=0;
        private bool stopping;


        internal DeviceApi(string internalName, string friendlyName, PrintResolution supportedResolutions, DeviceScanKind supportedScanKinds, string imageUri, ICaptureSdk sdk)
        {
            this.Sdk = sdk;
            this.InternalKey = internalName;

            this.ModelName = internalName;
            this.HardwareMake = "XL-ID";
            this.DisplayName = friendlyName;
            this.SerialNumber = "VIRTUAL DEVICE";

            this.SupportedResolutions = supportedResolutions;
            this.SupportedScanKinds = supportedScanKinds;

            this.Properties = new CustomPropertyList();

            this.CreateSupportedprintList();

            this.ExternalTool = string.Empty;

            this.ImageUri = imageUri;

            this.Properties = new CustomPropertyList();
            this.Properties.AddBoolProperty(PropUseFileDialog, "Enabled", true, "File Dialog");
        }

        public string DisplayName { get; private set; }

        public string InternalKey { get; private set; }

        public string HardwareMake { get; private set; }

        public string ModelName { get; private set; }

        public string SerialNumber { get; private set; }

        public string ImageUri { get; private set; }

        public string ExternalTool { get; private set; }

        public PrintResolution SupportedResolutions { get; private set; }

        public DeviceScanKind SupportedScanKinds { get; private set; }

        public CustomPropertyList Properties { get; private set; }        

        public SdkException LastException { get; private set; }

        public ICaptureSdk Sdk { get; private set; }        

        public DeviceState State { get; private set; }

        public event DeviceStateChangedHandler StateChanged;

        public PrintResolution FingerResolution { get; set; }

        public PrintResolution PalmResolution { get; set; }

        public void Open()
        {
            openCount++;
            this.ChangeState(DeviceState.Opening);
            this.OnDeviceSendMessage("Connection Successful", DeviceMessageKind.Information, false);
            var doNotCleanError = false;
            

            if (openCount == 1 && doNotCleanError)
            {
                // Test, provoquer une erreur !!
                this.LastException = new SdkException("Test", "Open", SdkErrorKind.CaptureSurfaceDirty, "Pas Clean",
                    true);

                //this.OnDeviceSendMessage("Pas clean", DeviceMessageKind.Error, true);
                this.OnDeviceOpened(DeviceOpenStatus.ErrorOccured);
            }
            else
            {
                this.IsOpened = true;
                this.OnDeviceOpened(DeviceOpenStatus.Success);
                this.ChangeState(DeviceState.Opened);
            }
        }

        public void Close()
        {

            this.StopCapture();

            this.ChangeState(DeviceState.Closing);
            this.IsOpened = false;
            this.ChangeState(DeviceState.Closed);
            return;
        }

        public bool InitializeCapture(IEnumerable<PhysicalHandPart> handParts, CapturePreviewHandler preview, int previewWindowHandle, bool isFlat)
        {
            this.preview = preview;            
            this.handParts = handParts;
            this.lastFolder = @"D:\Job\Tests\__Set-Tests\Flats\";

            if (!Directory.Exists(this.lastFolder))
            {
                this.lastFolder = null;
            }

            return true;
        }

        public bool CapturePrint(PrintResolution resolution, Hand printHand, HandPart handPart, HandScanKind scanKind)
        {
            if (!this.IsOpened)
            {
                this.Open();
            }            

            this.ChangeState(DeviceState.ScanInitialization);
            this.OnDeviceSendMessage("", DeviceMessageKind.Information, false);

            var print =
                this.supportedPrints.SingleOrDefault(
                    x => x.Hand == printHand && x.Part == handPart && x.Kind == scanKind && x.Resolution == resolution);

            if (print == null)
            {
                throw new ApplicationException("Fingerprint type not supported");
            }

            try
            {
                this.ChangeState(DeviceState.Scanning);
                this.CaptureSimulator(print);
                
            }
            catch (Exception ex)
            {
                this.LastException = new SdkException(this.ModelName, "CapturePrint", SdkErrorKind.InvalidParameter, ex.Message, false, ex);
                this.ChangeState(DeviceState.Opened);
                return false;
            }

            this.ChangeState(DeviceState.Opened);
            return true;
        }

        public bool IsOpened { get; private set; }

        public bool StopCapture()
        {
            this.stopping = true;

            // wait for not busy ! 
            this.WaitForState();

            stopping = false;

            this.ChangeState(DeviceState.Ready);

            return true;
        }

        void WaitForState(DeviceState targetState = DeviceState.Undefined)
        {
            var cpt = 1;
            while (cpt < 10 &&
                (this.State != targetState || 
                (targetState == DeviceState.Undefined && this.State.IsDeviceBusy())))
            {
                Thread.Sleep(200);
                cpt++;
            }

        }

        #region Sdk methods

        private void CaptureSimulator(PrintCaptureInformation print)
        {
            var captureTask = new Task(() =>
            {

                Bitmap img;
                if (print.Part == HandPart.FourFlats)
                {
                    img = Resources.Virtual4slaps;
                }
                else if (print.Part == HandPart.TwoThumbs)
                {
                    img = Resources.VirtualTwoSlap;
                }
                else
                {
                    img = Resources.VirtualPrint;
                }

                this.preview(img);

                // resize print
                int offsetx, offsety;

                offsetx = (print.ScanSize.Width / 2) - (img.Width / 2);
                offsety = (print.ScanSize.Height / 2) - (img.Height / 2);

                var newImg = new Bitmap(50, 50);
                var g = Graphics.FromImage(newImg);
                g.DrawImageUnscaled(img, offsetx, offsety);

                this.CaptureImage(print.Resolution, newImg);
            });

            DeviceSoundPlayer.Play(DeviceSound.Beep);

            var handler = this.CaptureQualityChanged;
            if (handler != null)
            {
                handler(new List<PrintCaptureQuality>());
            }

            // dialog cannot be called on another thread.
            if (this.Properties.GetBoolValue(PropUseFileDialog))
            {

                if (!string.IsNullOrEmpty(lastFolder))
                {
                    var key = print.NistIndex;
                    var filePath = lastFolder + "\\" + key.ToString() + ".bmp";
                    if (!File.Exists(filePath))
                    {
                        var files = Directory.GetFiles(lastFolder, string.Format("*_{0}.bmp", key));
                        if (files.Length == 1)
                        {
                            filePath = files[0];
                        }
                    }

                    if (File.Exists(filePath))
                    {
                        var newImage = (Bitmap)Image.FromFile(filePath);
                        this.CaptureImage(print.Resolution, newImage);

                        return;
                    }
                }

                var dlg = new OpenFileDialog { Filter = "Bitmap|*.bmp" };
                dlg.Title = string.Format("Print {0} {2} ({1} hand). Index:{3}", print.Part, print.Hand, print.Kind, print.NistIndex);
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    throw new ApplicationException("Dialog canceled");
                }

                this.lastFolder = Path.GetDirectoryName(dlg.FileName);

                Bitmap dlgImage = (Bitmap)Image.FromFile(dlg.FileName);

                this.CaptureImage(print.Resolution, dlgImage);

                return;
            }


            captureTask.Start();
        }

        private void CaptureImage(PrintResolution res, Bitmap img)
        {
            var blank = new Bitmap(50, 50);            

            using (var g = Graphics.FromImage(blank))
            {
                g.Clear(Color.White);
                g.Flush();
            }
            blank = ImageUtilities.ConvertToIndexedFormat(blank, ConvertBitmapFormat.Format8bppIndexed);
            this.preview(blank);

            var bw = new BackgroundWorker();
            bw.DoWork += (sender, args) =>
            {                
                Thread.Sleep(500);
                if (stopping)
                {                   
                    this.ChangeState(DeviceState.Ready);
                    return;                 
                }
                this.preview(img);
                Thread.Sleep(500);
                if (stopping)
                {
                    this.ChangeState(DeviceState.Ready);
                    return;
                }
                this.OnDevicePrintCaptured(res, img);
            };

            bw.RunWorkerAsync();
        }

        private void CreateSupportedprintList()
        {
            // get the right size
            var TwoThumbs500 = new Size(1600,1500);
            var FourSlap500 = new Size(1600, 1000);
            var SingleSlap500 = new Size(500, 1000);
            var PartialPalm500 = new Size(2750, 2750);
            var Hypothenar500 = new Size(900, 2500);
            var SingleRoll500 = new Size(800, 750);

            var FourSlap1000 = new Size(3200, 2000);
            var SingleSlap1000 = new Size(1000, 2000);
            var PartialPalm1000 = new Size(5500, 5500);
            var Hypothenar1000 = new Size(1800, 5000);
            var SingleRoll1000 = new Size(1600, 1500);


            this.supportedPrints = new List<PrintCaptureInformation>();

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.FourFlats, HandScanKind.Flat, PrintResolution.Dpi500, FourSlap500, 14));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.FourFlats, HandScanKind.Flat, PrintResolution.Dpi500, FourSlap500,13));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.None, HandPart.TwoThumbs, HandScanKind.Flat, PrintResolution.Dpi500, TwoThumbs500, 15));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.None, HandPart.TwoThumbs, HandScanKind.Flat, PrintResolution.Dpi1000, TwoThumbs500, 15));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Thumb, HandScanKind.Flat, PrintResolution.Dpi500, SingleSlap500, 12));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Index, HandScanKind.Flat, PrintResolution.Dpi500, SingleSlap500, 0));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Middle, HandScanKind.Flat, PrintResolution.Dpi500, SingleSlap500,0));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Ring, HandScanKind.Flat, PrintResolution.Dpi500, SingleSlap500, 0));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Little, HandScanKind.Flat, PrintResolution.Dpi500, SingleSlap500, 0));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Thumb, HandScanKind.Flat, PrintResolution.Dpi500, SingleSlap500, 11));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Index, HandScanKind.Flat, PrintResolution.Dpi500, SingleSlap500, 0));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Middle, HandScanKind.Flat, PrintResolution.Dpi500, SingleSlap500, 0));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Ring, HandScanKind.Flat, PrintResolution.Dpi500, SingleSlap500, 0));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Little, HandScanKind.Flat, PrintResolution.Dpi500, SingleSlap500, 0));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Thumb, HandScanKind.Rolled, PrintResolution.Dpi500, SingleRoll500, 6));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Index, HandScanKind.Rolled, PrintResolution.Dpi500, SingleRoll500, 7));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Middle, HandScanKind.Rolled, PrintResolution.Dpi500, SingleRoll500, 8));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Ring, HandScanKind.Rolled, PrintResolution.Dpi500, SingleRoll500, 9));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Little, HandScanKind.Rolled, PrintResolution.Dpi500, SingleRoll500, 10));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Thumb, HandScanKind.Rolled, PrintResolution.Dpi500, SingleRoll500, 1));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Index, HandScanKind.Rolled, PrintResolution.Dpi500, SingleRoll500, 2));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Middle, HandScanKind.Rolled, PrintResolution.Dpi500, SingleRoll500, 3));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Ring, HandScanKind.Rolled, PrintResolution.Dpi500, SingleRoll500, 4));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Little, HandScanKind.Rolled, PrintResolution.Dpi500, SingleRoll500, 5));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.UpperPalm, HandScanKind.Flat, PrintResolution.Dpi500, PartialPalm500, 28));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.LowerPalm, HandScanKind.Flat, PrintResolution.Dpi500, PartialPalm500, 27));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Hypothenar, HandScanKind.Flat, PrintResolution.Dpi500, Hypothenar500, 24));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.UpperPalm, HandScanKind.Flat, PrintResolution.Dpi500, PartialPalm500, 26));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.LowerPalm, HandScanKind.Flat, PrintResolution.Dpi500, PartialPalm500, 25));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Hypothenar, HandScanKind.Flat, PrintResolution.Dpi500, Hypothenar500, 22));


            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.FourFlats, HandScanKind.Flat, PrintResolution.Dpi1000, FourSlap1000, 14));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.FourFlats, HandScanKind.Flat, PrintResolution.Dpi1000, FourSlap1000, 13));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Thumb, HandScanKind.Flat, PrintResolution.Dpi1000, SingleSlap1000, 12));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Index, HandScanKind.Flat, PrintResolution.Dpi1000, SingleSlap1000, 0));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Middle, HandScanKind.Flat, PrintResolution.Dpi1000, SingleSlap1000, 0));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Ring, HandScanKind.Flat, PrintResolution.Dpi1000, SingleSlap1000, 0));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Little, HandScanKind.Flat, PrintResolution.Dpi1000, SingleSlap1000, 0));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Thumb, HandScanKind.Flat, PrintResolution.Dpi1000, SingleSlap1000, 11));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Index, HandScanKind.Flat, PrintResolution.Dpi1000, SingleSlap1000, 0));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Middle, HandScanKind.Flat, PrintResolution.Dpi1000, SingleSlap1000, 0));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Ring, HandScanKind.Flat, PrintResolution.Dpi1000, SingleSlap1000, 0));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Little, HandScanKind.Flat, PrintResolution.Dpi1000, SingleSlap1000, 0));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Thumb, HandScanKind.Rolled, PrintResolution.Dpi1000, SingleRoll1000, 6));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Index, HandScanKind.Rolled, PrintResolution.Dpi1000, SingleRoll1000, 7));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Middle, HandScanKind.Rolled, PrintResolution.Dpi1000, SingleRoll1000, 8));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Ring, HandScanKind.Rolled, PrintResolution.Dpi1000, SingleRoll1000, 9));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Little, HandScanKind.Rolled, PrintResolution.Dpi1000, SingleRoll1000, 10));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Thumb, HandScanKind.Rolled, PrintResolution.Dpi1000, SingleRoll1000, 1));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Index, HandScanKind.Rolled, PrintResolution.Dpi1000, SingleRoll1000, 2));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Middle, HandScanKind.Rolled, PrintResolution.Dpi1000, SingleRoll1000, 3));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Ring, HandScanKind.Rolled, PrintResolution.Dpi1000, SingleRoll1000, 4));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Little, HandScanKind.Rolled, PrintResolution.Dpi1000, SingleRoll1000, 5));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.UpperPalm, HandScanKind.Flat, PrintResolution.Dpi1000, PartialPalm1000, 28));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.LowerPalm, HandScanKind.Flat, PrintResolution.Dpi1000, PartialPalm1000, 27));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Left, HandPart.Hypothenar, HandScanKind.Flat, PrintResolution.Dpi1000, Hypothenar1000, 24));

            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.UpperPalm, HandScanKind.Flat, PrintResolution.Dpi1000, PartialPalm1000, 26));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.LowerPalm, HandScanKind.Flat, PrintResolution.Dpi1000, PartialPalm1000, 25));
            this.supportedPrints.Add(new PrintCaptureInformation(Hand.Right, HandPart.Hypothenar, HandScanKind.Flat, PrintResolution.Dpi1000, Hypothenar1000, 22));

        }

        #endregion

        /// <summary>
        /// This class store information about print capture
        /// Each sdk should use a similar class, with added properties depending on sdk values
        /// </summary>
        class PrintCaptureInformation
        {
            public PrintCaptureInformation(Hand hand, HandPart part, HandScanKind kind, PrintResolution resolution, Size scanSize, int nistIndex)
            {
                // set key fiedls
                this.Hand = hand;
                this.Part = part;
                this.Kind = kind;
                this.Resolution = resolution;
                this.NistIndex = nistIndex;
                // set sdk fields value
                this.ScanSize = scanSize;
            }

            public int NistIndex { get; set; }

            public Size ScanSize { get; private set; }

            public Hand Hand { get; private set; }
            public HandPart Part { get; private set; }
            public HandScanKind Kind { get; private set; }
            public PrintResolution Resolution { get; private set; }

        }


        public event DeviceMessageHandler DeviceSendMessage;

        public event DeviceOpenedHandler DeviceOpened;

        public event CapturedPrintHandler PrintCaptured;

        public event CaptureQualityHandler CaptureQualityChanged;


        #region Event handlers

        private void OnDeviceSendMessage(string text, DeviceMessageKind kind, bool showResumeButton)
        {
            var handler = this.DeviceSendMessage;
            if (handler != null)
            {
                handler(text, kind, showResumeButton);
            }
        }

        private void OnDeviceOpened(DeviceOpenStatus status)
        {
            var handler = this.DeviceOpened;
            if (handler != null)
            {
                handler(status);
            }
        }

        private void OnDevicePrintCaptured(PrintResolution position, Bitmap image)
        {
            var handler = this.PrintCaptured;
            if (handler != null)
            {
                handler(position, image);
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

            handler(this, newState);
        }

        #endregion
    }


}