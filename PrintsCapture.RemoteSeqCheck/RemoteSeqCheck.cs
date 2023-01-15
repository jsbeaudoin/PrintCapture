using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using NLog;
using PrintsCapture.Prints;
using PrintsCapture.Prints.Enum;
using PrintsCapture.Prints.Extension;
using PrintsCapture.Prints.Sequence;
using RemoteModules.Communication;
using XL_ID.Utilities.Image;

namespace PrintsCapture.RemoteSeqCheck
{
    using bs = UniBIO.Services.Communication.BiometricService;

    internal delegate void AsyncServiceOperationHandler(bool success, Exception ex);

    public class PrintSetService : BaseSeqCheckService, IDisposable
    {

        /// <summary>
        /// Wrapper class to avoid Exposing interface from Communication dll
        /// </summary>
        private class ServiceCallback : bs.IPrintSetServiceCallback
        {
            private readonly PrintSetService parent;

            public ServiceCallback(PrintSetService parent)
            {
                this.parent = parent;
            }

            public void OnPrintSequenceCheckCompleted(bs.PrintDetail printDetail)
            {
                this.parent.OnPrintSequenceCheckCompleted(printDetail);
            }
        }

        /// <summary>
        /// This class verifies that service stays connected until a error occured
        /// </summary>
        private class ServiceConnectionChecker : ServiceChecker.ICheckable, IDisposable
        {
            private readonly bs.IPrintSetService service;

            private readonly ServiceChecker serviceChecker;

            public event EventHandler Disconnected;

            public ServiceConnectionChecker(bs.IPrintSetService service)
            {
                this.service = service;
                this.serviceChecker = new ServiceChecker(this);
                var handler = this.Connected;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            #region ICheckable

            public bool IsAlive()
            {
                try
                {
                    this.service.GetPrintSet();
                    return true;
                }
                catch
                {
                    var handler = this.Disconnected;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                    return false;
                }
            }

            public void Reconnect()
            {
                // Service is meant to stay alive, but cannot reconnect due to it's nature                
            }

            public event EventHandler Connected;

            #endregion

            public void Dispose()
            {
                this.serviceChecker.Dispose();
            }
        }        

        private ServiceConnectionChecker serviceConnectionChecker;        

        private readonly bool offlineMode;

        private List<bs.PrintSwap> swapped;

        private bool connected;        

        private bs.IPrintSetService service;        

        private ServiceCallback callbackClass;

        private readonly string busConnectString;

        private int printInProcessCount;

        public PrintSetService(string tokenId, string connectString, CaptureKind captureProvenance, PrintList prints) 
            : base(captureProvenance, prints)
        {

            

            this.offlineMode = string.IsNullOrEmpty(connectString);
            this.Token = tokenId;
            this.busConnectString = connectString;
            //this.ServiceBusAddress = servicebusAddress;

            //this.printSwapCaller.OperationCalled += this.SwapPrints;
        }        

        public string Token { get; private set; }

        /// <summary>
        /// Used with the result printset id to allow access to saved data
        /// </summary>
        public string ExternalReference { private get; set; }        

        public string PrintSetId { get; private set; }


        public override void Test()
        {
            throw new NotImplementedException();
        }

        public override bool Connect()
        {
            this.Logger.Info("Connect()");
            if (this.service != null || this.connected || this.offlineMode)
            {
                return true;
            }

            try
            {
                this.connected = false;
                //var serverAddress = string.Format("net.tcp://{0}/{1}", this.serviceAddress, this.serviceName);
                this.callbackClass = new ServiceCallback(this);

                var channel = CreateRelayedDuplexWebService<bs.IPrintSetServiceChannel>(
                    this.callbackClass, 
                    this.busConnectString);

                this.service = channel;
                channel.Faulted += ChannelOnFaulted;

                this.service.Initialize(this.Token);
                this.serviceConnectionChecker = new ServiceConnectionChecker(this.service);
                this.serviceConnectionChecker.Disconnected += (s, e) => this.OnConnectionStateChanged(false);

                this.connected = true;
                this.InSession = false;

                this.OnConnectionStateChanged(true);

                return true;
            }            
            catch (Exception ex)
            {
                this.OnServiceException("Connect", ex);                
            }

            return false;
        }

        

        // Only start a session is none has already been started
        public override bool StartSession()
        {
            this.Logger.Info("StartSession()");
            if (this.offlineMode || this.InSession)
            {
                return true;
            }

            this.SetCaptureMode();

            return this.Connect() && this.StartServiceSession();
        }

        public override void AddPrint(PrintInfo print)
        {
            this.printInProcessCount++;
            this.Logger.Info("AddPrint()");
            if (this.offlineMode) return;
            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (o, e) => this.AddPrintToService(print);
            bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Sends a list of prints waiting for previous prints to be completed before sending others
        /// </summary>
        /// <param name="printsToSequence"></param>
        public override void AddPrintRange(List<PrintInfo> printsToSequence)
        {
            this.printInProcessCount += printsToSequence.Count;
            this.Logger.Info("AddPrintRange()");
            printsToSequence = new List<PrintInfo>(printsToSequence);

            // don't freeze the UI !
            var bgWorker = new BackgroundWorker();


            bgWorker.DoWork += (o, e) =>
            {
                // send to the sequence check Flats then the others
                // send flats first ! 
                var flatPrints =
                    printsToSequence.Where(
                        x =>
                            x.ScanKind == HandScanKind.Flat && x.Kind != HandPartKind.Palm
                            && x.HandPart != HandPart.Endorsement).OrderByDescending(x => x.NistPosition).ToList();

                foreach (var sortedPrint in flatPrints)
                {
                    this.AddPrintToService(sortedPrint);
                }

                var otherPrints = printsToSequence.Where(x => flatPrints.All(y => y.Key != x.Key)).OrderBy(y => y.NistPosition).ToList();

                foreach (var sortedPrint in otherPrints)
                {
                    var bgPrintWorker = new BackgroundWorker();
                    PrintInfo print = sortedPrint;
                    bgPrintWorker.DoWork += (x, y) => this.AddPrintToService(print);
                    bgPrintWorker.RunWorkerAsync();
                }
            };

            bgWorker.RunWorkerAsync();
        }

        public override string EndSession(bool accept)
        {
            this.Logger.Info("EndSession(" + accept + ")");
            if (this.offlineMode) return string.Empty;

            if (this.CheckSession(accept))
            {
                this.DoServiceCommand(s => s.EndSession(accept), "EndSession", accept ? (bool?)true : null);
            }

            this.OnSessionStartOrEnd(false);

            if (this.serviceConnectionChecker != null)
            {
                this.serviceConnectionChecker.Dispose();
                this.serviceConnectionChecker = null;
            }
            var id = this.PrintSetId;
            this.InSession = false;
            this.PrintSetId = null;
            this.service = null;
            this.connected = false;

            return id;
        }

        public override bool ResetSession()
        {
            this.Logger.Info("ResetSession()");
            if (this.offlineMode) return true;

            if (this.InSession)
            {
                this.EndSession(false);
            }

            return this.Connect();
        }

        public override void SetPrintOverride(PrintInfo print)
        {
            this.Logger.Info("SetPrintOverride()");
            var position = (bs.PrintPosition)print.NistPosition;
            if (position == bs.PrintPosition.Unknown)
            {
                return;
            }

            if (print.IsOverriden)
            {
                var over = new bs.PrintOverride();
                over.NistReasonCode = print.OverrideCode;
                over.OtherReasonText = print.OverrideUserReason;

                this.DoServiceCommandAsync(x => x.SetOverride(position, over), "SetPrintOverride");
            }
            else
            {
                this.DoServiceCommandAsync(x => x.RemoveOverride(position), "SetPrintOverride");
            }
        }

        public override void SetSegmentOverride(PrintInfo print, PrintSegment segment)
        {
            this.Logger.Info("SetSegmentOverride()");
            var position = (bs.PrintPosition)print.NistPosition;
            if (position == bs.PrintPosition.Unknown)
            {
                return;
            }

            var segmentPosition = (bs.PrintPosition)segment.Part.EndorsementIndex;

            if (segment.IsOverriden)
            {
                var over = new bs.PrintOverride();
                over.NistReasonCode = segment.OverrideCode;
                over.OtherReasonText = segment.OverrideText;

                this.DoServiceCommandAsync(x => x.SetSegmentOverride(position, segmentPosition, over), "SetPrintOverride");
            }
            else
            {
                this.DoServiceCommandAsync(x => x.RemoveOverride(position), "SetPrintOverride");
            }
        }

        public override void SequencePositionRuleChanged()
        {
            if (printInProcessCount < 1)
            {
                this.ChangeMatchedIndexes(this.GetPrintSwapList());
            }            
        }

        public override void OnDeviceChanged()
        {
            this.SetDeviceInformation();
        }

        private void SetDeviceInformation()
        {
            if (this.CaptureDevice == null || !this.InSession)
            {
                return;
            }

            var deviceInfo = new bs.DeviceInformation
            {
                Manufacturer = this.CaptureDevice.Make,
                ModelName = this.CaptureDevice.ModelName,
                SerialNumber = this.CaptureDevice.SerialNumber,
                Kind =
                    this.CaptureDevice.Kind == CaptureKind.Livescan
                        ? bs.DeviceKind.LiveScan
                        : bs.DeviceKind.CardScan
            };

            this.DoServiceCommandAsync(x => x.SetDeviceInformation(deviceInfo), "SetDeviceInfo");
        }

        protected override List<PrintSetWarning> CheckSessionIntegrity()
        {
            var result = new List<PrintSetWarning>();
            bs.PrintSet setInfo = null;
            this.SetDeviceInformation();
            this.DoServiceCommand(x => setInfo = x.GetPrintSet(), "GetPrintSet");
            if (setInfo == null)
            {
                return null;
            }

            foreach (var printDetail in setInfo.Prints)
            {
                var warning = new PrintSetWarning();
                var print = printDetail.IsEndorsement
                    ? this.Prints.Prints.SingleOrDefault(x => printDetail.IsEndorsement && x.IsEndorsement)
                    : this.Prints.Prints.SingleOrDefault(x => (int)printDetail.ScannedPosition == x.NistPosition);

                if (print == null)
                {
                    continue;
                }

                var hasError = false;
                warning.Print = print;

                if (print.IsMissing != printDetail.IsMissing || print.IsOverriden != printDetail.IsOverridden)
                {
                    hasError = true;
                    warning.ServerMissingOrOverrideDifferent = true;
                }

                if (!hasError && !print.IsMissing)
                {
                    if (!(printDetail.WsqCreated.HasValue && printDetail.WsqCreated.Value))
                    {
                        hasError = true;
                        warning.WsqCreationError = true;
                    }

                    if (printDetail.Position != printDetail.ScannedPosition && (int)printDetail.Position != print.MatchedNistPosition)
                    {
                        hasError = true;
                        warning.ServerSwapDifferent = true;
                    }
                }

                if (hasError)
                {
                    result.Add(warning);
                }
            }



            return result;
        }

        private void AddPrintToService(PrintInfo print)
        {
            this.Logger.Info("AddPrintToService()");
            var tryCount = 0;

            while (print.ImageForProcessing == null && tryCount < 5)
            {
                Thread.Sleep(100);
                tryCount++;
            }

            if (print.ImageForProcessing == null)
            {
                this.Logger.Warn("ImageForProcessing For print " + print.Key + " was null");
            }

            byte[] dataToSend;
            bs.ImageCompressionType compression;

            try
            {
                var copy2 = print.Image.DeepClone();
                var bmpData = ImageUtilities.ConvertToBinary(copy2);

                this.GetDataCompressed(bmpData, out dataToSend, out compression);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "AddPrintToServiceAsync Exception.");
                print.ProcessStatus = PrintProcessStatus.ServiceError;
                this.OnPrintModified(print);
                this.printInProcessCount--;
                
                return;
            }

            if (this.service == null || this.offlineMode)
            {
                return;
            }

            //if (print.ImageForProcessing != null)
            //{
            //    print.ImageForProcessing.Save("D:\\temp\\Prints\\2SetMath_" + this.PrintSetId + "-" + print.NistPosition + ".bmp", ImageFormat.Bmp);
            //}                        

            var segments = new List<bs.PrintSegment>();
            if (print.HasSegments)
            {
                foreach (var printSegment in print.Segments)
                {
                    segments.Add(new bs.PrintSegment
                    {
                        Position = (bs.PrintPosition) printSegment.Part.EndorsementIndex,
                        IsExpected = printSegment.IsExpected
                    });
                }
            }

            var printImage = new bs.PrintImage
            {
                CompressionType = compression,
                Position = (bs.PrintPosition)print.NistPosition,
                Data = dataToSend,
                Segments = segments
            };

            if (print.HandPart == HandPart.Endorsement)
            {
                // can't process an empty endorsement
                if (print.EndorsementFinger == null)
                {
                    print.ProcessStatus = PrintProcessStatus.TemplateError;
                    this.OnPrintModified(print);
                    this.printInProcessCount--;
                    
                    return;
                }

                printImage.Position = (bs.PrintPosition)print.EndorsementFinger.EndorsementIndex;
                printImage.IsEndorsement = true;
            }

            try
            {
                this.CheckSession(true);                

                this.DoServiceCommandAsync(
                    x =>
                    {
                        var detail = x.AddOrUpdatePrint(printImage);
                        print.QualityScore = detail.Quality;
                        this.OnPrintModified(print);
                    }, "Add Or update print", false);                            
            }
            catch (Exception ex)
            {
                print.QualityScore = 0;
                print.ProcessStatus = PrintProcessStatus.ServiceError;
                this.OnPrintModified(print);
                this.Logger.Error(ex, "AddPrintToServiceAsync Exception. Finger=" + PrintList.GetName(print));
            }

            this.printInProcessCount--;
            this.OnPrintModified(print);
        }       

        private bool CheckSession(bool throwExceptionIfInactive)
        {
            if (this.InSession)
            {
                return true;
            }

            if (throwExceptionIfInactive)
            {
                this.OnServiceException("Check Session", null);
            }

            return false;
        }

        /// <summary>
        /// Do service operations and trap errors
        /// </summary>
        /// <param name="serviceAction">The service actions to perform</param>
        /// <param name="operation">The name of the operation. Will be logged in case of error.</param>
        /// <param name="handleError">True -> Error is trapped and displayed. False : Error is logged and raised. Null: Error is logged and ignored.</param>
        /// <returns></returns>
        private void DoServiceCommandAsync(Action<bs.IPrintSetService> serviceAction, string operation, bool? handleError = true, AsyncServiceOperationHandler asyncOp = null)
        {
            try
            {
                this.Logger.Trace("DoServiceCommandAsync called for '{0}' operation", operation);
                Task.Factory.StartNew(
                    () =>
                    {
                        serviceAction(this.service);
                        if (asyncOp != null)
                        {
                            asyncOp(true, null);
                        }
                    });                
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, operation);
                
                if (handleError == true)
                {
                    this.OnServiceException(operation, ex);
                    this.ShowServiceError(operation, ex);
                }
                
                if (asyncOp != null)
                {
                    asyncOp(false, ex);             
                }                
            }            
        }

        /// <summary>
        /// Do service operations and trap errors
        /// </summary>
        /// <param name="serviceAction">The service actions to perform</param>
        /// <param name="operation">The name of the operation. Will be logged in case of error.</param>
        /// <param name="handleError">True -> Error is trapped and displayed. False : Error is logged and raised. Null: Error is logged and ignored.</param>
        /// <returns></returns>
        private bool DoServiceCommand(Action<bs.IPrintSetService> serviceAction, string operation, bool? handleError = true)
        {
            try
            {
                this.Logger.Trace("DoServiceCommand called for '{0}' operation", operation);
                serviceAction(this.service);
                return true;
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, operation);
                if (!handleError.HasValue)
                {
                    return false;
                }
                if (handleError.Value)
                {
                    this.OnServiceException(operation, ex);
                    this.ShowServiceError(operation, ex);
                }
                else
                {
                    throw;
                }

            }

            return false;
        }

        private void ChannelOnFaulted(object sender, EventArgs eventArgs)
        {
            this.Logger.Error("Service Communication channel faulted !");
            this.service = null;
            this.connected = false;
            this.InSession = false;
            //throw new NotImplementedException();
        }


        private bool StartServiceSession()
        {
            this.Logger.Info("StartServiceSession()");
            // get missing prints
            var missings = this.Prints.Prints.Where(x => x.IsMissing && x.NistPosition != -1).ToList();

            var list = new List<bs.MissingPrint>();

            if (missings.Count > 0)
            {
                list = new List<bs.MissingPrint>();
                for (int i = 0; i < missings.Count; i++)
                {
                    var miss = new bs.MissingPrint();
                    miss.Position = (bs.PrintPosition)missings[i].NistPosition;
                    miss.NistCode = missings[i].PhysicalPart.MissingCode;
                    miss.Date = missings[i].PhysicalPart.MissingDate;
                    list.Add(miss);
                }
            }

            try
            {                      
                this.service.StartSession(list, this.Prints.Rules.IsFlatCaptureMode);
                var detail = this.service.GetPrintSet();
                this.PrintSetId = detail.Id;
                this.InSession = true;

                this.OnSessionStartOrEnd(true);                

                return true;
            }
            catch (Exception ex)
            {
                this.InSession = false;
                this.Logger.Error(ex, "PrintSetSession Start Session error.");
                this.OnServiceException("Start Service Session", ex);
            }

            return false;
        }        

        public void Dispose()
        {
            if (this.serviceConnectionChecker != null)
            {
                this.serviceConnectionChecker.Dispose();
                this.serviceConnectionChecker = null;
            }

        }

        internal void OnPrintSequenceCheckCompleted(bs.PrintDetail printDetail)
        {
            this.Logger.Debug("Print info received from remote service : {0}", printDetail.Position);
            // avoid deadlock of communication
            Task.Factory.StartNew(
                () =>
                {
                    this.RefreshPrintFromDetail(printDetail);
                    if (printInProcessCount < 1)
                    {
                        var list = this.GetPrintSwapList();
                        this.ChangeMatchedIndexes(list);
                    }                    
                });
        }


        private static T CreateRelayedDuplexWebService<T>(object callback, string connectString) where T : class, IClientChannel
        {

            var binding = new NetTcpRelayBinding { }; // SendTimeout = TimeSpan.FromMinutes(5)

            var cs = new ServiceBusConnectionStringBuilder(connectString);
            var endPoint = new EndpointAddress(cs.Endpoints.First());
            var isSharedAccessKey = !string.IsNullOrEmpty(cs.SharedSecretIssuerName);
            

            //var endPoint = new EndpointAddress(servicebusAddress);
            LogManager.GetCurrentClassLogger().Debug("Service EndPoint Uri : {0}", endPoint.Uri.ToString());

            var cf = new DuplexChannelFactory<T>(callback, binding, endPoint);

            var tknProvider = isSharedAccessKey ?
                TokenProvider.CreateSharedAccessSignatureTokenProvider(cs.SharedSecretIssuerName, cs.SharedSecretIssuerSecret) :
                TokenProvider.CreateSharedSecretTokenProvider(cs.SharedAccessKeyName, cs.SharedAccessKey);

            // - ORIGINAL -
            //var tknProvider = isSharedAccessKey ?
            //    TokenProvider.CreateSharedSecretTokenProvider(cs.SharedSecretIssuerName, cs.SharedSecretIssuerSecret) :
            //    TokenProvider.CreateSharedAccessSignatureTokenProvider(cs.SharedAccessKey, cs.SharedAccessKeyName);   

            cf.Endpoint.Behaviors.Add(new TransportClientEndpointBehavior { TokenProvider = tknProvider });

            var channel = cf.CreateChannel();

            return channel;
        }

        private void ChangeMatchedIndexes(List<bs.PrintSwap> allSwaps)
        {
            if (allSwaps.Count == 0)
            {
                return;
            }

            if (this.swapped != null && this.swapped.Count == allSwaps.Count)
            {
                var identical = true;
                foreach (var printSwap in this.swapped)
                {
                    var swap = allSwaps.SingleOrDefault(x => x.Position == printSwap.Position);
                    if (swap == null || swap.ScannedPosition != printSwap.ScannedPosition)
                    {
                        identical = false;
                    }
                }

                if (identical)
                {
                    return;
                }
            }

            this.swapped = allSwaps;
            //bs.PrintSet details = null;

            this.DoServiceCommandAsync(x => x.PrintSwap(allSwaps), "Print Swap", false, this.Swapped);

            //if (!(this.DoServiceCommand(x => x.PrintSwap(allSwaps), "Print Swap") && this.DoServiceCommand(x => details = x.GetPrintSet(), "GetPrintSet")))
            //{
            //    this.swapped = null;
            //    return;
            //}

            //foreach (var printDetail in details.Prints)
            //{
            //    this.RefreshPrintFromDetail(printDetail);
            //}
        }

        private void Swapped(bool success, Exception ex)
        {
            if (!success)
            {
                this.swapped = null;
                return;
            }

            this.DoServiceCommandAsync(
                x =>
                {
                    var details = x.GetPrintSet();
                    foreach (var printDetail in details.Prints)
                    {
                        this.RefreshPrintFromDetail(printDetail);
                    }
                },
                "GetPrintSet");
        }

        private void RefreshPrintFromDetail(bs.PrintDetail printDetail)
        {

            if (printDetail.Position == bs.PrintPosition.Unknown)
            {
                this.Logger.Error("Received an update from an unknown position");
                return;
            }

            var position = !printDetail.IsEndorsement ? (int)printDetail.ScannedPosition : PrintList.EndorsementFingerIndex;

            var print = this.Prints.Prints.Single(x => x.NistPosition == position);
            print.TemplateErrors.Clear();            
            
            this.UpdateSegments(print, printDetail.Segments);            

            this.UpdateSequenceScore(print, printDetail);            

            print.SequenceAnalyzed = true;

            if (printDetail.WsqCreated.HasValue && !printDetail.WsqCreated.Value)
            {
                print.ProcessStatus = PrintProcessStatus.ServiceError;
                print.TemplateErrors.Add(TemplateError.WsqNotCreated);
            }
            else if (printDetail.IsSequenceCheckInError || printDetail.TemplateStatus != bs.TemplateStatus.Ok)
            {
                print.ProcessStatus = PrintProcessStatus.TemplateError;
                printDetail.AllScores = new List<bs.PrintSequenceCheck>();
                print.SequenceScore = 0;
                if (printDetail.TemplateStatus != bs.TemplateStatus.Ok)
                {
                    print.TemplateErrors.Add((TemplateError)printDetail.TemplateStatus);
                }
            }
            else
            {
                print.ProcessStatus = PrintProcessStatus.Success;
            }

            this.OnPrintModified(print);
        }

        private void UpdateSequenceScore(PrintInfo print, bs.PrintDetail printDetail)
        {
            print.SequenceScore = 0;

            if (printDetail.AllScores != null)
            {
                print.AllScores =
                    printDetail.AllScores.Select(
                        x => new SequenceCheckResult { Position = (int)x.Position, Score = x.Score }).ToList();
            }
            else
            {
                printDetail.AllScores = new List<bs.PrintSequenceCheck>();
            }

            print.SequenceSelfScore = printDetail.SelfMatch != null ? printDetail.SelfMatch.Score : 0;

            if (printDetail.BestMatch != null)
            {
                print.SequenceBestScore = printDetail.BestMatch.Score;
                print.SequenceBestScorePosition = (int)printDetail.BestMatch.Position;
            }
            else
            {
                print.SequenceBestScore = 0;
                print.SequenceBestScorePosition = 0;
            }

            if (printDetail.ScannedPosition != printDetail.Position)
            {
                var matchedPrint = this.Prints.Prints.Single(x => x.NistPosition == (int)printDetail.Position);
                print.MatchedKey = matchedPrint.Key;
                print.MatchedNistPosition = matchedPrint.NistPosition;
            }
            else
            {
                print.MatchedKey = string.Empty;
                print.MatchedNistPosition = 0;
            }


            var score = print.AllScores.SingleOrDefault(x => x.Position == (int)printDetail.Position);
            print.SequenceScore = score == null ? 0 : score.Score;
        }

        private void UpdateSegments(PrintInfo print, IEnumerable<bs.PrintSegment> serviceSegments)
        {
            if (print.HasSegments && serviceSegments != null)
            {
                foreach (var servicePrintSegment in serviceSegments)
                {
                    var seg = print.Segments.SingleOrDefault(x => x.Part.EndorsementIndex == (int)servicePrintSegment.Position);
                    if (seg == null)
                    {
                        continue;
                    }
                    seg.Position = new Rectangle(servicePrintSegment.Left, servicePrintSegment.Top, servicePrintSegment.Width, servicePrintSegment.Height);
                    seg.QualityScore = servicePrintSegment.QualityScore;
                }
            }

            var hand = HandnessFinder.ReturnHandness(print);
            if (hand != Hand.None && hand != print.Hand)
            {
                print.TemplateErrors.Add(TemplateError.WrongHand);
            }
        }

        public override List<PrintInfo> GetCapturedPrints()
        {
            throw new NotImplementedException();
        }
    }
}
