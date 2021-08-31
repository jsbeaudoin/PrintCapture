using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrintsCapture.Prints.ViewModel;

namespace PrintsCapture.Prints.Sequence
{
    using System.ServiceModel;
    using System.Windows;

    using NLog;

    using PrintsCapture.Prints.Delegates;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Language;

    using UniBIO = UniBIO.Services.Communication.BiometricService;

    public delegate void SequenceCheckExceptionHandler(object sender, SequenceCheckExceptionEventArgs e);

    public delegate void ServiceConnectionChangedHandler(object sender, ServiceConnectionStateEventArgs e);

    public abstract class BaseSeqCheckService
    {

        public abstract void Test();

        public event SequenceCheckExceptionHandler ServiceException;

        public event PrintModifiedHandler PrintModified;

        public EventHandler DeviceInitialized;

        private CaptureDeviceInfo captureDevice;

        public event EventHandler SessionStarted;

        public event EventHandler SessionEnded;

        public event ServiceConnectionChangedHandler ServiceConnected;

        public event ServiceConnectionChangedHandler ServiceDisconnected;

        public event EventHandler AllPrintsAdded;


        #region Properties

        public string CaptureMode { get; private set; }

        public CaptureDeviceInfo CaptureDevice
        {
            get
            {
                return this.captureDevice;
            }
            set
            {
                this.Logger.Trace("CaptureDevice Set");
                this.captureDevice = value;
                this.OnDeviceChanged();
            }
        }

        /// <summary>
        /// The minimum number of munutia if deltas are of poor quality
        /// </summary>
        public int MinimumMinutiaCount { get; set; }

        public bool IsDataCompressed { get; set; }           

        public CaptureKind CaptureProvenance { get; private set; }

        public PrintList Prints { get; private set; }

        public bool Connected { get; protected set; }

        public bool InSession { get; protected set; }

        protected Logger Logger { get; private set; }

        #endregion

        #region Ctor

        protected BaseSeqCheckService(CaptureKind captureProvenance, PrintList prints)
        {
                     
            this.CaptureProvenance = captureProvenance;
            this.Prints = prints;
            this.Logger = LogManager.GetCurrentClassLogger();
            this.MinimumMinutiaCount = 10;
        }

        #endregion

        #region public non abstract methods



        public List<PrintSetWarning> CheckSessionIntegrity(bool showMessage)
        {
            var warnings = this.CheckSessionIntegrity();

            if (warnings == null )
            {
                return null;
            }

            if (warnings.Count > 0 && showMessage)
            {
                var msg = CommonText.PrintsNotReady + Environment.NewLine;

                foreach (var warning in warnings)
                {
                    var printMsg = string.Empty;
                    var printName = PrintList.GetName(warning.Print);

                    if (warning.ServerMissingOrOverrideDifferent)
                    {
                        printMsg = string.Format(CommonText.MsgPrintSyncError, printName);
                    }

                    if (warning.WsqCreationError)
                    {
                        printMsg = string.Format(CommonText.MsgWsqCompressionFailed, printName);
                    }

                    if (warning.ServerSwapDifferent)
                    {
                        printMsg = string.Format(CommonText.MsgSwapDifferent, printName);
                    }

                    msg += printMsg + Environment.NewLine;
                }

                MessageBox.Show(msg, CommonText.ValidationTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            return warnings;
        }

        #endregion

        #region abstract methods

        public abstract bool Connect();

        public abstract bool StartSession();

        public abstract void AddPrint(PrintInfo print);

        public abstract void AddPrintRange(List<PrintInfo> printsToSequence);

        public abstract string EndSession(bool accept);

        public abstract bool ResetSession();

        public abstract void SetPrintOverride(PrintInfo print);

        public abstract void SetSegmentOverride(PrintInfo print, PrintSegment segment);

        public abstract void SequencePositionRuleChanged();

        public abstract void OnDeviceChanged();

        protected abstract List<PrintSetWarning> CheckSessionIntegrity();

        #endregion

        #region protected methods

        protected List<UniBIO.PrintSwap> GetPrintSwapList()
        {
                        
            try
            {
                var swapper = new PrintSwapper(this.Prints);
                var allSwaps = swapper.GetSwapList();
                return allSwaps;
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Can't swap prints");
                // throw;
            }

            return null;
        }

        protected void SetCaptureMode()
        {
            if (this.Prints.Rules.CaptureKind == CaptureKind.Cardscan)
            {
                this.CaptureMode = this.Prints.Rules.IsFlatCaptureMode ? "flatcard" : "card";
            }
            else
            {
                this.CaptureMode = this.Prints.Rules.IsFlatCaptureMode ? "flat" : "live";
            }
        }

        protected void ShowServiceError(string operation, Exception ex)
        {
            this.Logger.Trace("BaseSequence - ShowServiceError");
            var msg = "Service failed for operation '{0}'. Reason : {1}";
            var reason = "";

            if (ex is EndpointNotFoundException)
            {
                reason = "Cannot find Sequence Check Service : " + ex.Message;
            }else if (ex is FaultException)
            {
                reason = "Cannot communicate with Sequence Check service: " + ex.Message;
            }
            else
            {
                reason += ex.Message;
            }

            MessageBox.Show( string.Format(msg, operation, reason), CommonText.ValidationTitle, MessageBoxButton.OK, MessageBoxImage.Error);        
        }                

        protected void GetDataCompressed(byte[] bmpData, out byte[] compressedData, out UniBIO.ImageCompressionType compression)
        {
            this.Logger.Trace("BaseSequence - GetDataCompressed");
            if (!this.IsDataCompressed)
            {
                compressedData = bmpData;
                compression = UniBIO.ImageCompressionType.None;
                return;
            }

            try
            {
                var result = XL_ID.Utilities.SevenZip.SevenZipHelper.Compress(bmpData);
                compressedData = result;
                compression = UniBIO.ImageCompressionType.SevenZip;
            }
            catch (OutOfMemoryException)
            {
                compressedData = bmpData;
                compression = UniBIO.ImageCompressionType.None;
            }

        }

        protected void OnServiceException(string operation, Exception innerException)
        {
            this.Logger.Trace("BaseSequence - OnServiceException");
            var handler = this.ServiceException;
            if (handler == null)
            {
                return;
            }

            handler(this, new SequenceCheckExceptionEventArgs(operation, innerException));
        }

        protected void OnPrintModified(PrintInfo info)
        {
            this.Logger.Trace("BaseSequence - OnPrintModified");
            var handler = this.PrintModified;

            if (handler == null)
            {
                return;
            }
            handler(this, new PrintModifiedEventArgs(info));

            //var bg = new BackgroundWorker();
            //bg.DoWork += (sender, args) => handler(this, new PrintModifiedEventArgs(info));
            //bg.RunWorkerAsync();
        }

        protected void OnConnectionStateChanged(bool connected)
        {
            this.Logger.Trace("BaseSequence - OnConnectionStateChanged");
            var handler = connected ? this.ServiceConnected : this.ServiceDisconnected;

            if (handler == null)
            {
                return;
            }

            handler(this, new ServiceConnectionStateEventArgs(connected));
        }


        protected void OnSessionStartOrEnd(bool start)
        {
            this.Logger.Trace("BaseSequence - OnSessionStartOrEnd : {0}", start ? "start" : "End");
            var handler = start ? this.SessionStarted : this.SessionEnded;

            if (handler == null)
            {
                return;
            }

            handler(this, EventArgs.Empty);
        }

        protected void OnDeviceInitialized()
        {
            this.Logger.Trace("BaseSequence - OnDeviceInitialized");
            var handler = this.DeviceInitialized;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected void OnAllPrintsAdded()
        {
            this.Logger.Trace("BaseSequence - OnAllPrintsAdded");
            var handler = this.AllPrintsAdded;

            if (handler == null)
            {
                return;
            }

            handler(this, EventArgs.Empty);
        }

        #endregion

        #region private methods

        

        #endregion
    }
}
