using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UniBIO.Services.Communication.TransactionWatcherService;

namespace PrintsCapture.LocalAwSeqCheck
{
    using Aware.AwSequence;

    using NLog;
    using NLog.Targets.Wrappers;

    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Sequence;

    using UniBIO = UniBIO.Services.Communication.BiometricService;

    public class LocalSeqCheck : BaseSeqCheckService, ISequencePrints
    {       

        private class AwareIndexReference
        {
            public int NistIndex { get; set; }

            public awSequenceCheck.AwareFingerType Fingertype { get; set; }

            public awSequenceCheck.AwareFingerType FingertypeSequence { get; set; }

            public AwareIndexReference Parent { get; set; }

        }

        private class CapturedPrint
        {            

            public awSequenceCheck.AwareFingerType Fingertype { get; set; }

            public PrintInfo Print { get; set; }

            public Guid TemplateImageKey { get; set; }

            public bool IsSlap { get; set; }

            public bool IsSpecial { get; set; }            

            public bool IsUpdateRequired
            {
                get
                {
                    return this.Print.ImageKey != this.TemplateImageKey || !this.Print.SequenceAnalyzed;
                }
            }

            //public List<PrintSegment> Segments { get; private set; }

        }
        
        private readonly List<AwareIndexReference> capturedSlap = new List<AwareIndexReference>();

        private readonly Dictionary<int, awSequenceCheck.AwareFingerType> specialCases = new Dictionary<int, awSequenceCheck.AwareFingerType>();

        private List<CapturedPrint> capturedPrints;

        private awSequenceCheck seqChecker;

        public LocalSeqCheck(CaptureKind captureProvenance, PrintList prints)
            : base(captureProvenance, prints)
        {
            
        }

        public override bool Connect()
        {            
            this.OnConnectionStateChanged(true);
            return true;
        }

        public override bool StartSession()
        {
            this.Logger.Trace("LocalAwSequence - StartSession");
            if (this.InSession)
            {
                return true;
            }
            this.capturedPrints = new List<CapturedPrint>();

            this.InSession = true;

            this.Logger.Trace("LocalAwSequence - StartSession - Creating Objects");

            try
            {
                seqChecker = new awSequenceCheck();
            }
            catch (Exception ex)
            {
                // if a dll is missing, it will throw an error !!
                this.Logger.Error(ex, "Cannot start Sequence session");                
                this.OnConnectionStateChanged(false);
                return false;
            }                       

            this.SetInitialMissingFingers();

            this.SetinitialSpecialCases();           

            this.SetCaptureMode();

            this.OnSessionStartOrEnd(true);
            return true;
        }

       

        public override void AddPrint(PrintInfo print)
        {
            this.Logger.Trace("LocalAwSequence - AddPrint ({0])", PrintList.GetName(print));
            this.AddOrUpdateTemplate(print);
            this.SwapPrints();
        }

        public override void AddPrintRange(List<PrintInfo> printsToSequence)
        {
            this.Logger.Trace("LocalAwSequence - AddPrintRange");
            foreach (var printInfo in printsToSequence)
            {
                this.AddOrUpdateTemplate(printInfo);
            }
            this.SwapPrints();
        }

        public override string EndSession(bool accept)
        {
            this.Logger.Trace("LocalAwSequence - EndSession");
            this.OnSessionStartOrEnd(false);
            this.InSession = false;
            if (this.seqChecker != null)
            {
                this.seqChecker.Dispose();
                this.seqChecker = null;
            }
            return "1";
        }

        public override bool ResetSession()
        {
            this.Logger.Trace("LocalAwSequence - ResetSession");
            this.specialCases.Clear();            

            this.OnSessionStartOrEnd(false);
            this.capturedPrints.Clear();
            this.capturedSlap.Clear();
            this.InSession = false;
            
            if (this.seqChecker != null)
            {
                this.seqChecker.Dispose();
                this.seqChecker = null;
            }

            return true; //this.StartSession();
        }

        public override void SetPrintOverride(PrintInfo print)
        {
            this.Logger.Trace("LocalAwSequence - SetPrintOverride");
            // done in PrintInfo, so nothing needed for the service            
        }

        public override void SetSegmentOverride(PrintInfo print, PrintSegment segment)
        {
            this.Logger.Trace("LocalAwSequence - SetSegmentOverride");            
        }

        public override void SequencePositionRuleChanged()
        {
           this.Logger.Trace("LocalAwSequence - SequencePosistionRuleChanged");
           this.SwapPrints();
        }

        public override void OnDeviceChanged()
        {
            this.Logger.Trace("LocalAwSequence - OnDeviceChanged");
            // nothing
        }

        public override void Test()
        {
            this.AntiSequencing();
        }

        protected override List<PrintSetWarning> CheckSessionIntegrity()
        {
            this.Logger.Trace("LocalAwSequence - CheckSessionIntegrity");
            this.Logger.Trace("No integirty to check, service is notr async and not remote");
            return new List<PrintSetWarning>();
        }

        public List<PrintInfo> GetCapturedPrints()
        {
            this.Logger.Trace("LocalAwSequence - GetCapturedPrints");
            var captured =  this.capturedPrints.Select(x => x.Print).ToList();

            //var missing = this.Prints.GetPrintListToVerify().Where(x => x.IsMissing && captured.All(y => y.NistPosition != x.NistPosition));
            //captured.AddRange(missing);                        
            
            return captured;
        }

        public List<PrintInfo> GetCapturedSegments()
        {
            this.Logger.Trace("LocalAwSequence - GetCapturedSegments");
            var slapList = this.capturedPrints.Where(x => x.Print.HasSegments).Select(y => y.Print).ToList();

            return slapList;
        }

        private void SetInitialMissingFingers()
        {
            // set missing fingers
            this.Logger.Trace("LocalAwSequence - StartSession - Setting Missings 1");
            var missings = this.Prints.PhysicalParts.Where(x => x.IsMissing && x.Kind != HandPartKind.Palm).ToList();
            foreach (var part in missings)
            {
                seqChecker.SetFingerMissing(
                    (awSequenceCheck.AwareFingerType)part.EndorsementIndex,
                    awSequenceCheck.AwareFingerMissingCode.AW_FNG_MISSING);
            }

            this.Logger.Trace("LocalAwSequence - StartSession - Setting Missings 2");
            var missingCaptured = this.Prints.GetPrintListToVerify().Where(x => x.IsMissing);
            foreach (var printInfo in missingCaptured)
            {
                var captured = new CapturedPrint
                {
                    Print = printInfo,
                    Fingertype = this.GetAwarePosition(printInfo.NistPosition),
                    IsSlap = printInfo.IsSlap
                };
                this.capturedPrints.Add(captured);

            }
        }

        private void SetinitialSpecialCases()
        {
            this.Logger.Trace("LocalAwSequence - StartSession - Add special cases to list");
            this.specialCases.Add(15, awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_RIGHT_THUMBS);
            this.specialCases.Add(99, awSequenceCheck.AwareFingerType.AW_LEFT_AUX_FINGER);

            this.specialCases.Add(26, awSequenceCheck.AwareFingerType.AW_UPPER_PALM_RIGHT);
            this.specialCases.Add(25, awSequenceCheck.AwareFingerType.AW_LOWER_PALM_RIGHT);
            this.specialCases.Add(22, awSequenceCheck.AwareFingerType.AW_PLAIN_PALM_WRITERS_RIGHT);

            this.specialCases.Add(28, awSequenceCheck.AwareFingerType.AW_UPPER_PALM_LEFT);
            this.specialCases.Add(27, awSequenceCheck.AwareFingerType.AW_LOWER_PALM_LEFT);
            this.specialCases.Add(24, awSequenceCheck.AwareFingerType.AW_PLAIN_PALM_WRITERS_LEFT);
        }

        private awSequenceCheck.AwareFingerType GetAwarePosition(int nistPosition)
        {
            this.Logger.Trace("LocalAwSequence - GetAwarePosition");
            return this.specialCases.ContainsKey(nistPosition) ?
                this.specialCases[nistPosition] :
                (awSequenceCheck.AwareFingerType)nistPosition;                        
        }

        private void SwapPrints()
        {
            this.Logger.Trace("LocalAwSequence - SwapPrints");
            var swapper = new PrintSwapper(this.Prints);

            var list = swapper.GetSwapList();

            if (list == null || list.Count == 0)
            {
                this.Logger.Trace("LocalAwSequence - SwapPrints - Nothing to swap");
                return;
            }

            this.Logger.Trace("LocalAwSequence - SwapPrints - {0} print(s) to swap", list.Count);

            foreach (var printSwap in list)
            {
                var swap = printSwap;

                var print = this.Prints.Prints.SingleOrDefault(x => x.NistPosition == (int)swap.ScannedPosition);                
                var newPrint= this.Prints.Prints.SingleOrDefault(x => x.NistPosition == (int)swap.Position);

                if (print != null )
                {
                    print.MatchedNistPosition = newPrint.NistPosition;
                    print.MatchedKey = newPrint.Key;
                    print.SequenceScore = print.AllScores.Single(x => x.Position == print.MatchedNistPosition).Score;
                    this.OnPrintModified(print);
                }
            }
        }
       
        private CapturedPrint GetPrint(PrintInfo info)
        {
            this.Logger.Trace("LocalAwSequence - GetPrint ({0])", PrintList.GetName(info));
            var result = this.capturedPrints.SingleOrDefault(x => x.Print == info);

            if (result == null)
            {
                result = new CapturedPrint
                {
                    Fingertype = this.GetAwarePosition(info.NistPosition),
                    Print = info,
                    IsSlap = info.ScanKind == HandScanKind.Flat && info.HandPart != HandPart.Endorsement
                };
                this.capturedPrints.Add(result);
            }
                

            return result;
        }

        private void AddOrUpdateTemplate(PrintInfo info)
        {
            this.Logger.Trace("LocalAwSequence - AddOrUpdateTemplate ({0})", PrintList.GetName(info));
            var print = this.GetPrint(info);
            
            if (print.Print.IsEndorsement)
            {
                this.CheckEndorsement(info);
                return;
            }

            var segmentsMissing = print.Print.Segments == null
                ? new List<PrintSegment>()
                : print.Print.Segments.Where(x => !x.IsExpected && !x.Part.IsMissing).ToList();

            print.IsSpecial = print.Print.Segments != null && print.Print.HandPart != HandPart.TwoThumbs && print.Print.Segments.Count(x => x.IsExpected && !x.Part.IsMissing) == 1;

            if (print.IsUpdateRequired)
            {
                this.Logger.Trace("LocalAwSequence - AddOrUpdateTemplate - Update required");
                // a segment ignored must be set missing before validation and restored as present after
                foreach (var unexpSegment in segmentsMissing)
                {
                    seqChecker.SetFingerMissing((awSequenceCheck.AwareFingerType)unexpSegment.Part.EndorsementIndex, awSequenceCheck.AwareFingerMissingCode.AW_FNG_MISSING);                    
                }

                if (!this.SetTemplate(print))
                {
                    this.OnPrintModified(info);
                    return;
                }
                
                info.QualityScore = this.seqChecker.GetQualityScore(print.Fingertype);
            }            

            if (!this.CheckSlap(print))
            {
                this.Logger.Warn("LocalAwSequence - AddOrUpdateTemplate - Slap verifcation failed");
                info.ProcessStatus = PrintProcessStatus.TemplateError;
                info.TemplateErrors.Add(TemplateError.WrongTemplateCount);
                this.OnPrintModified(info);
            }

            
            foreach (var unexpSegment in segmentsMissing)
            {
                seqChecker.SetFingerMissing((awSequenceCheck.AwareFingerType)unexpSegment.Part.EndorsementIndex, awSequenceCheck.AwareFingerMissingCode.AW_FNG_PRESENT);                    
            }
            
            if (print.IsSlap)
            {
                this.Logger.Trace("LocalAwSequence - AddOrUpdateTemplate - Recalculating scores");
                this.RecalculateScores();                
            }
            else
            {
                this.Logger.Trace("LocalAwSequence - AddOrUpdateTemplates - Calculating scores");
                this.CalculateScores(print);
            }

            this.Logger.Trace("LocalAwSequence - AddOrUpdateTemplates - setting status");
            print.Print.ProcessStatus = PrintProcessStatus.Success;
            print.Print.SequenceAnalyzed = true;

            this.OnPrintModified(info);
        }

        private int GetNumberSlapInt(awSequenceCheck.AwareFingerType fingertype)
        {
            int nbSlap = 1;
            try
            {
                nbSlap = seqChecker.GetNumberSlapDigits(fingertype);
            }
            catch (Exception ex)
            {
                var err = AwareSequenceError.ExceptionToError(ex);
                this.Logger.Warn(ex, "Sequence GetNumberSlapInt failed for print '{1}' : {0}", err, fingertype);
            }

            return nbSlap;
        }       

        private bool SetTemplate(CapturedPrint capturedPrint)
        {
            this.Logger.Trace("LocalAwSequence - SetTemplate");
            var refImage = capturedPrint.Print.ImageForProcessing;
            var imgData = XL_ID.Utilities.Image.ImageUtilities.ConvertToByteArray(refImage);
            var resolution = capturedPrint.Print.Resolution == PrintResolution.Dpi500
                ? awSequenceCheck.AwareImageResolution.AW_500PPI
                : awSequenceCheck.AwareImageResolution.AW_1000PPI;
            var awareError = awSequenceCheck.errorCode.AWSEQ_NO_ERRORS;
            
            string errorMsg;

            if (capturedPrint.IsSpecial)
            {
                SetSpecialType(capturedPrint);
            }
                        
            try
            {
                seqChecker.ClearFinger(capturedPrint.Fingertype);
                awareError = seqChecker.SetFingerRes(capturedPrint.Fingertype,
                    imgData,
                    refImage.Width,
                    refImage.Height,
                    resolution);                
                
                errorMsg = awareError.ToString();
            }
            catch (Exception ex)
            {
                var awareExError = AwareSequenceError.ExceptionToError(ex);
                if (awareExError == awSequenceCheck.errorCode.ERR_AWSEQ_INVALID_IMAGE && capturedPrint.IsSpecial)
                {
                    return false;
                }
                this.Logger.Error("LocalAwSequence - SetTemplate", awareExError);
                errorMsg = awareExError.ToString();
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString() + " - SetFingerRes Exception : " + ex.Message + " / " + errorMsg);

                capturedPrint.Print.ProcessStatus = PrintProcessStatus.ServiceError;                                                
            }
            capturedPrint.TemplateImageKey = capturedPrint.Print.ImageKey;

            if (capturedPrint.Print.ProcessStatus == PrintProcessStatus.ServiceError || awareError != awSequenceCheck.errorCode.AWSEQ_NO_ERRORS)
            {
                this.Logger.Error("Sequence check failed for '{0}', with error : {1}", capturedPrint.Print.NistPosition, errorMsg);
                capturedPrint.Print.ProcessStatus = PrintProcessStatus.ServiceError;
                this.OnPrintModified(capturedPrint.Print);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set the fingertype when 3 fingers out of the four flats are missing.
        /// </summary>
        /// <param name="capturedPrint"></param>
        private void SetSpecialType(CapturedPrint capturedPrint)
        {
            this.Logger.Trace("LocalAwSequence - SetSpecialType");
            if (capturedPrint.Print.HandPart != HandPart.FourFlats ||
                capturedPrint.Print.Segments == null )
            {
                return;
            }

            // find the non missing print
            var nonMissing = capturedPrint.Print.Segments.FirstOrDefault(x => !x.IsMissing && x.IsExpected);

            if (nonMissing == null)
            {
                return;
            }           

            if (capturedPrint.Print.Hand == Hand.Left)
            {
                var leftFingers = new Dictionary<HandPart, awSequenceCheck.AwareFingerType>();
                leftFingers.Add(HandPart.Index, awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_INDEX_FINGER);
                leftFingers.Add(HandPart.Middle, awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_MIDDLE_FINGER);
                leftFingers.Add(HandPart.Ring, awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_RING_FINGER);
                leftFingers.Add(HandPart.Little, awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_LITTLE_FINGER);

                capturedPrint.Fingertype = leftFingers[nonMissing.Part.HandPart];                
            }
            else
            {
                var rightFingers = new Dictionary<HandPart, awSequenceCheck.AwareFingerType>();
                rightFingers.Add(HandPart.Index, awSequenceCheck.AwareFingerType.AW_PLAIN_RIGHT_INDEX_FINGER);
                rightFingers.Add(HandPart.Middle, awSequenceCheck.AwareFingerType.AW_PLAIN_RIGHT_MIDDLE_FINGER);
                rightFingers.Add(HandPart.Ring, awSequenceCheck.AwareFingerType.AW_PLAIN_RIGHT_RING_FINGER);
                rightFingers.Add(HandPart.Little, awSequenceCheck.AwareFingerType.AW_PLAIN_RIGHT_LITTLE_FINGER);

                capturedPrint.Fingertype = rightFingers[nonMissing.Part.HandPart];
            }

            if (this.specialCases.ContainsKey(capturedPrint.Print.NistPosition))
            {
                this.specialCases.Remove(capturedPrint.Print.NistPosition);
            }

            this.specialCases.Add(capturedPrint.Print.NistPosition, capturedPrint.Fingertype);
        }

        private void CheckEndorsement(PrintInfo endorsement)
        {
            this.Logger.Trace("LocalAwSequence - CheckEndorsement");
            this.ResetSequenceScore(endorsement);
            endorsement.SequenceAnalyzed = true;
            endorsement.ProcessStatus = PrintProcessStatus.Success;

            var endorsementChecker = new awSequenceCheck();
           
            var fingerType = awSequenceCheck.AwareFingerType.AW_RIGHT_THUMB;
            var imgData = XL_ID.Utilities.Image.ImageUtilities.ConvertToByteArray(endorsement.ImageForProcessing);
            var resolution = endorsement.Resolution == PrintResolution.Dpi500
                ? awSequenceCheck.AwareImageResolution.AW_500PPI
                : awSequenceCheck.AwareImageResolution.AW_1000PPI;


            var awareError = awSequenceCheck.errorCode.AWSEQ_NO_ERRORS;
            try
            {
                
                awareError = endorsementChecker.SetFingerRes(fingerType,
                    imgData,
                    endorsement.ImageForProcessing.Width,
                    endorsement.ImageForProcessing.Height,
                    resolution);
                if (awareError != awSequenceCheck.errorCode.AWSEQ_NO_ERRORS)
                {
                    throw new ApplicationException("Aware SetFingerRes Error : " + awareError);
                }
            }
            catch (Exception ex)
            {
                var awareExError = AwareSequenceError.ExceptionToError(ex);
                if (awareExError == awSequenceCheck.errorCode.AWSEQ_NO_ERRORS)
                {
                    awareExError = awareError;
                }
                this.Logger.Error(ex, "LocalAwSequence - CheckEndorsement - SetFingerRes Error : {0}", awareExError);
                System.Diagnostics.Debug.WriteLine("{0} - SetFingerRes Exception : {1}", DateTime.Now.ToString(), awareExError);

                endorsement.ProcessStatus = PrintProcessStatus.TemplateError;
                this.OnPrintModified(endorsement);
                return;
            }
            
            var quality = endorsementChecker.GetQualityScore(fingerType);  // QualityFingerScore(print.Fingertype);
            endorsement.QualityScore = quality;

            awSequenceCheck.AwareFingerType reference;
            this.Logger.Trace("LocalAwSequence - CheckEndorsement - Getting reference print position");
            if (endorsement.EndorsementFinger.HandPart != HandPart.Thumb)
            {
                reference = endorsement.EndorsementFinger.Hand == Hand.Left
                    ? this.GetAwarePosition(14)  // Four flat left
                    : this.GetAwarePosition(13); // four flat right
                // GetAwarePosaition must be used because of special limitation when 3 fingers out of 4 are missing !
            }
            else
            {
                var hasTwoThumbs =
                    this.capturedPrints.Any(
                        x => x.Fingertype == awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_RIGHT_THUMBS);
                if (hasTwoThumbs)
                {
                    reference = awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_RIGHT_THUMBS; // endorsement.Hand == Hand.Left ? awSequenceCheck.AwareFingerType.AW_PLAIN_THUMBS_LEFT : awSequenceCheck.AwareFingerType.AW_PLAIN_THUMBS_RIGHT;
                }else
                {
                    reference = endorsement.EndorsementFinger.Hand == Hand.Left
                        ? awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_THUMB
                        : awSequenceCheck.AwareFingerType.AW_PLAIN_RIGHT_THUMB;
                }
            }

            this.Logger.Trace("LocalAwSequence - CheckEndorsement - Getting reference print data");
            var referenceSlapImage = this.capturedSlap.SingleOrDefault(x => x.NistIndex == endorsement.EndorsementFinger.EndorsementIndex);

            if (reference == awSequenceCheck.AwareFingerType.AW_INTERDIGITAL_PALM_LEFT
                || referenceSlapImage == null)
            {
                this.Logger.Trace("LocalAwSequence - CheckEndorsement - No reference data to validate");
                this.OnPrintModified(endorsement);
                return;
            }

            // load reference into data !
            this.Logger.Trace("LocalAwSequence - CheckEndorsement - Converting reference data");
            var capturedPrint = this.capturedPrints.Single(x => x.Fingertype == referenceSlapImage.Parent.Fingertype);

            // set missing fingers
            if (capturedPrint.Print.HasSegments)
            {
                var missings = capturedPrint.Print.Segments.Where(x => x.IsMissing).ToList();
                foreach (var missing in missings)
                {
                    var index = this.GetAwarePosition(missing.Part.EndorsementIndex);
                    endorsementChecker.SetFingerMissing(index, awSequenceCheck.AwareFingerMissingCode.AW_FNG_MISSING);
                }
            }

            imgData = XL_ID.Utilities.Image.ImageUtilities.ConvertToByteArray(capturedPrint.Print.ImageForProcessing);
            resolution = capturedPrint.Print.Resolution == PrintResolution.Dpi500
                ? awSequenceCheck.AwareImageResolution.AW_500PPI
                : awSequenceCheck.AwareImageResolution.AW_1000PPI;

            awareError = awSequenceCheck.errorCode.AWSEQ_NO_ERRORS;       
            
            try
            {                                               
                awareError = endorsementChecker.SetFingerRes(reference,
                    imgData,
                    capturedPrint.Print.ImageForProcessing.Width,
                    capturedPrint.Print.ImageForProcessing.Height,
                    resolution);
                if (awareError != awSequenceCheck.errorCode.AWSEQ_NO_ERRORS)
                {
                    throw new ApplicationException("Aware SetFingerRes Error : " + awareError);
                }
            }
            catch (Exception ex)
            {
                var awareExError = AwareSequenceError.ExceptionToError(ex);
                if (awareExError == awSequenceCheck.errorCode.AWSEQ_NO_ERRORS)
                {
                    awareExError = awareError;
                }

                this.Logger.Error(ex, "LocalAwSequence - CheckEndorsement - Cannot load reference print data: {0}", awareExError);
                System.Diagnostics.Debug.WriteLine("{0} - SetFingerRes Exception : {1}", DateTime.Now.ToString(), awareExError);
                endorsement.ProcessStatus = PrintProcessStatus.TemplateError;
                this.OnPrintModified(endorsement);
                return;
            }

            try
            {
                endorsement.SequenceScore = endorsementChecker.MatchFingers(referenceSlapImage.Fingertype, fingerType) / 1000;
            }
            catch (Exception)
            {
                this.Logger.Trace("LocalAwSequence - CheckEndorsement - Cannot Get reference match score");
                endorsement.SequenceScore = 0;
            }
            
            endorsement.SequenceSelfScore = endorsement.SequenceScore;
            endorsement.AllScores.Add(new SequenceCheckResult { Position = capturedPrint.Print.NistPosition, Score = endorsement.SequenceScore });

            this.OnPrintModified(endorsement);
        }

        

        private bool CheckSlap(CapturedPrint print)
        {
            this.Logger.Trace("LocalAwSequence - CheckSlap");            

            var captureInfo = this.GetCaptureInfo(print);
            
            captureInfo.DefineMissingAndSubPrints();
            
            // Clear captured slaps list
            var toremove = this.capturedSlap.Where(x => x.Parent.Fingertype == print.Fingertype).ToList();
            foreach (var awareIndexReference in toremove)
            {
                this.capturedSlap.Remove(awareIndexReference);
            }

            bool endProcess = false;

            // set minutia count
            try
            {
                print.Print.MinutiaCount = seqChecker.NumberMinutia(print.Fingertype);
            }
            catch (Exception ex)
            {
                var err = AwareSequenceError.ExceptionToError(ex);
                this.Logger.Warn(ex, "seqChecker failed for print '{1}' : {0}", err, print.Fingertype);
            }
            

            // Set slaps into list and minutia count and 
            captureInfo.SetSlapsAndSegments(capturedSlap, ref endProcess);

            if (endProcess)
            {
                return true;
            }

            this.VerifyHandess(print);
            
            return captureInfo.ExpectedCount == captureInfo.PrintCount;
        }

        private PrintCaptureInfo GetCaptureInfo(CapturedPrint print)
        {            
            switch (print.Fingertype)
            {
                case awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_FOUR_FINGERS:
                    return new PrintCaptureInfo(print, seqChecker, this.Prints)
                           {
                               ExpectedCount = 4,
                               PrintCount = this.GetNumberSlapInt(print.Fingertype),
                               NistIndexes = new[] { 10, 9, 8, 7 },
                               Fingers =
                                   new[]
                                   {
                                       awSequenceCheck.AwareFingerType.AW_LEFT_SLAP_FINGER_ONE,
                                       awSequenceCheck.AwareFingerType.AW_LEFT_SLAP_FINGER_TWO,
                                       awSequenceCheck.AwareFingerType.AW_LEFT_SLAP_FINGER_THREE,
                                       awSequenceCheck.AwareFingerType.AW_LEFT_SLAP_FINGER_FOUR
                                   },
                               NistIndexSwitch = false
                           };                   

                case awSequenceCheck.AwareFingerType.AW_UPPER_PALM_LEFT:
                    return new PrintCaptureInfo(print, seqChecker, this.Prints)
                           {
                               ExpectedCount = 4,
                               PrintCount = this.GetNumberSlapInt(print.Fingertype),
                               NistIndexes = new[] { 7, 8, 9, 10 },
                               Fingers =
                                   new[]
                                   {
                                       awSequenceCheck.AwareFingerType
                                           .AW_UPPER_PALM_LEFT_INDEX_FINGER,
                                       awSequenceCheck.AwareFingerType
                                           .AW_UPPER_PALM_LEFT_MIDDLE_FINGER,
                                       awSequenceCheck.AwareFingerType
                                           .AW_UPPER_PALM_LEFT_RING_FINGER,
                                       awSequenceCheck.AwareFingerType
                                           .AW_UPPER_PALM_LEFT_LITTLE_FINGER
                                   },
                               NistIndexSwitch = true
                           };                     

                case awSequenceCheck.AwareFingerType.AW_PLAIN_RIGHT_FOUR_FINGERS:
                    return new PrintCaptureInfo(print, seqChecker, this.Prints)
                           {
                               ExpectedCount = 4,
                               PrintCount = this.GetNumberSlapInt(print.Fingertype),
                               NistIndexes = new[] { 2, 3, 4, 5 },
                               Fingers =
                                   new[]
                                   {
                                       awSequenceCheck.AwareFingerType.AW_RIGHT_SLAP_FINGER_ONE,
                                       awSequenceCheck.AwareFingerType.AW_RIGHT_SLAP_FINGER_TWO,
                                       awSequenceCheck.AwareFingerType.AW_RIGHT_SLAP_FINGER_THREE,
                                       awSequenceCheck.AwareFingerType.AW_RIGHT_SLAP_FINGER_FOUR
                                   },
                               NistIndexSwitch = false
                           };

                case awSequenceCheck.AwareFingerType.AW_UPPER_PALM_RIGHT:
                    return new PrintCaptureInfo(print, seqChecker, this.Prints)
                           {
                               ExpectedCount = 4,
                               PrintCount = this.GetNumberSlapInt(print.Fingertype),
                               NistIndexes = new[] { 2, 3, 4, 5 },
                               Fingers =
                                   new[]
                                   {
                                       awSequenceCheck.AwareFingerType
                                           .AW_UPPER_PALM_RIGHT_INDEX_FINGER,
                                       awSequenceCheck.AwareFingerType
                                           .AW_UPPER_PALM_RIGHT_MIDDLE_FINGER,
                                       awSequenceCheck.AwareFingerType
                                           .AW_UPPER_PALM_RIGHT_RING_FINGER,
                                       awSequenceCheck.AwareFingerType
                                           .AW_UPPER_PALM_RIGHT_LITTLE_FINGER
                                   },
                               NistIndexSwitch = true
                           };                      

                case awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_RIGHT_THUMBS:
                    return new PrintCaptureInfo(print, seqChecker, this.Prints)
                           {
                               ExpectedCount = 2,
                               PrintCount = this.GetNumberSlapInt(print.Fingertype),
                               NistIndexes = new[] { 6, 1 },
                               Fingers =
                                   new[]
                                   {
                                       awSequenceCheck.AwareFingerType.AW_PLAIN_THUMBS_LEFT,
                                       awSequenceCheck.AwareFingerType.AW_PLAIN_THUMBS_RIGHT
                                   },
                               NistIndexSwitch = true
                           };                   

                case awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_THUMB:
                    return new PrintCaptureInfo(print, seqChecker, this.Prints)
                    {
                        ExpectedCount = 1,
                        PrintCount = 1,
                        NistIndexes = new[] { 6 },                        
                        NistIndexSwitch = false
                    };

                case awSequenceCheck.AwareFingerType.AW_PLAIN_RIGHT_THUMB:
                    return new PrintCaptureInfo(print, seqChecker, this.Prints)
                    {
                        ExpectedCount = 1,
                        PrintCount = 1,
                        NistIndexes = new[] { 1 },
                        NistIndexSwitch = false
                    };
                    
                default:
                    return new PrintCaptureInfo(print, seqChecker, this.Prints)
                    {
                        ExpectedCount = 1,
                        PrintCount = 1,                        
                    };                    
            }            
        }

        

        private void VerifyHandess(CapturedPrint print)
        {
            // custom handness check :-)
            // Returns none for other than 4slaps.
            if (print.Print.HandPart == HandPart.FourFlats && !print.IsSpecial)
            {
                this.Logger.Trace("LocalAwSequence - CheckSlap - Handness validation");
                var handess = HandnessFinder.ReturnHandness(print.Print);
                if (handess != Hand.None && handess != print.Print.Hand)
                {
                    print.Print.TemplateErrors.Add(TemplateError.WrongHand);
                }

                var awareHand = Hand.None;
                var awareConfidence = 0D;

                try
                {
                    awareHand = seqChecker.GetHandedness(print.Fingertype) == 0 ? Hand.Right : Hand.Left;
                    awareConfidence = seqChecker.GetHandednessConfidence(print.Fingertype);
                }
                catch (Exception ex)
                {
                    var err = AwareSequenceError.ExceptionToError(ex);
                    this.Logger.Warn(ex, "Sequence GetNumberSlapInt failed for print '{1}' : {0}", err, print.Fingertype);                    
                }
                
                if (awareHand != print.Print.Hand && awareConfidence > 0.75)
                {
                    print.Print.TemplateErrors.Add(TemplateError.WrongHand);
                }

                Console.WriteLine("Print Hand : {0}.  Self detect:{1}.  Aware:{2}, {3}%", print.Print.Hand, handess, awareHand, awareConfidence);
            }
        }
       
        private void CalculateScores(CapturedPrint capturedPrint)
        {
            var print = capturedPrint.Print;
            var scores = this.GetScores(capturedPrint.Fingertype);

            print.AllScores = scores;

            // find self score
            var self = scores.SingleOrDefault(x => x.Position == print.NistPosition);            
            print.SequenceSelfScore = self != null ? self.Score : 0;

            var best = scores.FirstOrDefault(y => y.Score == scores.Max(x => x.Score));
            if (best != null)
            {
                print.SequenceBestScore = best.Score;
                print.SequenceBestScorePosition = best.Position;
            }
            else
            {
                print.SequenceBestScore = 0;
                print.SequenceBestScorePosition = 0;
            }
            
            print.SequenceScore = print.SequenceSelfScore;                                
        }

        private List<SequenceCheckResult> GetScores(awSequenceCheck.AwareFingerType fingerType, bool includeTwoThumbs = true)
        {
            var result = new List<SequenceCheckResult>();
            var slapList =
                this.capturedSlap.Where(
                    x =>
                        x.Parent.Fingertype != awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_RIGHT_THUMBS
                        || includeTwoThumbs).ToList();

            slapList.RemoveAll(
                x =>
                    x.Parent.Fingertype == awSequenceCheck.AwareFingerType.AW_UPPER_PALM_LEFT
                    || x.Parent.Fingertype == awSequenceCheck.AwareFingerType.AW_UPPER_PALM_RIGHT);

            foreach (var slap in slapList)
            {
                var score = this.GetMatchScore(fingerType, slap.FingertypeSequence);
                result.Add(new SequenceCheckResult { Position = slap.NistIndex, Score = score });
            }

            return result;

            //return (from reference in this.capturedSlap.Where(x => x.Parent.Fingertype != awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_RIGHT_THUMBS || includeTwoThumbs) 
            //        where reference.Fingertype != fingerType
            //        let score = this.GetMatchScore(fingerType, reference.Fingertype)
            //        select new SequenceCheckResult { Position = reference.NistIndex, Score = score }).ToList();
        }

        // Aware scores are between 0 and 100 000 --> Return between 0 and 100. Handle an exception as a score of 0
        int GetMatchScore(awSequenceCheck.AwareFingerType fingerType1, awSequenceCheck.AwareFingerType fingerType2)
        {
            try
            {
                return this.seqChecker.MatchFingers(fingerType1, fingerType2) / 1000;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Match finger Error : {0}", ex);
                return 0;
            }
        }

        private void RecalculateScores()
        {
            var allRolled = this.capturedPrints.Where(x => !x.IsSlap && !x.Print.IsEndorsement);
            foreach (var capturedPrint in allRolled)
            {
                var origScore = capturedPrint.Print.AllScores;
                this.CalculateScores(capturedPrint);
                if (this.IsScoreChanged(origScore, capturedPrint.Print.AllScores))
                {
                    this.OnPrintModified(capturedPrint.Print);
                }
            }

            var endorsement = this.capturedPrints.SingleOrDefault(x => x.Print.IsEndorsement);
            if (endorsement != null)
            {
                this.CheckEndorsement(endorsement.Print);
            }

            this.AntiSequencing();
        }

        private void AntiSequencing()
        {
            this.Logger.Trace("LocalAwSequence - Antisequencing");
            var modifiedList = new List<awSequenceCheck.AwareFingerType>();
            var originalResult = new Dictionary<PrintInfo, List<SequenceCheckResult>>();
            foreach (var source in this.capturedPrints.Where(x => x.IsSlap))
            {
                originalResult.Add(source.Print, source.Print.AllScores);
                this.ResetSequenceScore(source.Print);                
            }

            foreach (var reference in this.capturedSlap)
            {
                // no anti sequencing for thumbs Or palms (Save fingers of upper palms)
                if (reference.Parent.Fingertype == awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_RIGHT_THUMBS || 
                    reference.Fingertype == awSequenceCheck.AwareFingerType.AW_LOWER_PALM_LEFT || reference.Fingertype == awSequenceCheck.AwareFingerType.AW_LOWER_PALM_RIGHT ||
                    reference.Fingertype == awSequenceCheck.AwareFingerType.AW_PLAIN_PALM_WRITERS_LEFT || reference.Fingertype == awSequenceCheck.AwareFingerType.AW_PLAIN_PALM_WRITERS_RIGHT)
                {
                    continue;
                }

                // Antisequencing check ignores two thumbs
                var scores = this.GetScores(reference.Fingertype, false);                

                if (reference.Parent != null && this.AssignScoresToSegments(reference, scores))
                {
                    modifiedList.Add(reference.Parent.Fingertype);                                                            
                }

                if (scores != null)
                {
                    // remove scores from same parent, since anti sequencing is about matching other prints, not the same one
                    AwareIndexReference reference1 = reference;
                    var children = this.capturedSlap.Where(x => x.Parent.Fingertype == reference1.Parent.Fingertype);
                    scores.RemoveAll(x => children.Any(y => y.NistIndex == x.Position));
                }

                if (scores != null && scores.Count > 0)
                {                    
                    var max = scores.Max(x => x.Score);
                    var maxResult = scores.First(x => x.Score == max);
                    AwareIndexReference reference1 = reference;
                    var slap = this.capturedPrints.SingleOrDefault(x => x.Fingertype == reference1.Parent.Fingertype);
                    if (slap != null)
                    {
                        var result = slap.Print.AllScores.SingleOrDefault(x => x.Position == maxResult.Position);
                        if (result == null)
                        {
                            result = new SequenceCheckResult { Position = maxResult.Position, Score = max };
                            slap.Print.AllScores.Add(result);
                        }
                        else
                        {
                            if (result.Score < maxResult.Score)
                            {
                                result.Score = maxResult.Score;
                            }
                        }
                        
                    }
                }                
            }

            foreach (var source in this.capturedPrints.Where(x => x.IsSlap))
            {
                if (source.Print.AllScores.Count > 0)
                {
                    var max = source.Print.AllScores.Max(x => x.Score);
                    var maxResult = source.Print.AllScores.First(x => x.Score == max);
                    source.Print.SequenceBestScore = max;
                    source.Print.SequenceBestScorePosition = maxResult.Position;
                    
                    if (originalResult.ContainsKey(source.Print))
                    {
                        var orig = originalResult[source.Print];
                        if (this.IsScoreChanged(orig, source.Print.AllScores))
                        {
                            modifiedList.Add(source.Fingertype);                            
                        }
                    }                          
                }
            }

            foreach (var fingerType in modifiedList.Distinct())
            {
                awSequenceCheck.AwareFingerType type = fingerType;
                var prn = this.capturedPrints.FirstOrDefault(x => x.Fingertype == type);
                if (prn != null)
                {
                    this.OnPrintModified(prn.Print);
                }
            }
        }

        /// <summary>
        /// return true if print was modified, else false
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="scores"></param>
        /// <returns></returns>
        private bool AssignScoresToSegments(AwareIndexReference reference, List<SequenceCheckResult> scores)
        {
            var capt = this.capturedPrints.SingleOrDefault(x => x.Fingertype == reference.Parent.Fingertype);

            var selfScore = scores.SingleOrDefault(x => x.Position == reference.NistIndex);

            if (capt != null && capt.Print.HasSegments)
            {
                // find segment
                var seg = capt.Print.Segments.SingleOrDefault(x => x.Part.EndorsementIndex == reference.NistIndex);
                if (seg != null)
                {
                    var old = seg.SelfScore;
                    seg.SelfScore = selfScore;

                    if (selfScore == old)
                    {
                        return false;
                    }

                    if (selfScore == null || old == null || old.Score != selfScore.Score || old.Position != selfScore.Position)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ResetSequenceScore(PrintInfo print)
        {
            this.Logger.Trace("LocalAwSequence - ResetSequenceScore");
            print.SequenceScore = 0;
            print.AllScores = new List<SequenceCheckResult>();
            print.SequenceSelfScore = 0;
            print.SequenceBestScorePosition = 0;
            print.SequenceBestScore = 0;            
        }


        private bool IsScoreChanged(List<SequenceCheckResult> scores1, List<SequenceCheckResult> scores2)
        {
            if (scores1.Count != scores2.Count)
            {
                return true;
            }

            foreach (var result in scores1)
            {
                SequenceCheckResult result1 = result;
                var matchScore2 = scores2.SingleOrDefault(x => x.Position == result1.Position);
                if (matchScore2 == null || (matchScore2.Score != result.Score))
                {
                    return true;
                }
                
            }

            return false;
        }
        // index Nist, indexAware (slap et autre) 

        private class PrintCaptureInfo
        {
            private readonly awSequenceCheck seqChecker;
            private readonly CapturedPrint capturedPrint;

            private readonly PrintList printList;
            
            private List<AwareIndexReference> SubPrints { get; set; }
            private List<PhysicalHandPart> MissingPrints { get; set; }
            private Logger Logger { get; set; }

            public PrintCaptureInfo(CapturedPrint print, awSequenceCheck seq, PrintList list)
            {
                this.SubPrints = new List<AwareIndexReference>();
                this.MissingPrints = new List<PhysicalHandPart>();
                this.capturedPrint = print;
                this.seqChecker = seq;
                this.printList = list;
                this.Logger = LogManager.GetCurrentClassLogger();
            }

            public int PrintCount { get; set; }
            public int ExpectedCount { get; set; }
            public int[] NistIndexes { get; set; }
            public awSequenceCheck.AwareFingerType[] Fingers { get; set; }
            public bool NistIndexSwitch { get; set; }

            
            

            public void DefineMissingAndSubPrints()
            {
                if (this.Fingers != null && this.NistIndexes != null)
                {
                    this.capturedPrint.Print.MinutiaCount = 0;
                    var fingerIndex = 0;
                    for (var index = 0; index < this.Fingers.Length; index++)
                    {
                        int index1 = index;
                        var phys = this.printList.PhysicalParts.SingleOrDefault(x => x.EndorsementIndex == this.NistIndexes[index1]);
                        var seg = phys == null ? null : this.capturedPrint.Print.Segments.Single(x => x.Part == phys);
                        if (phys == null)
                        {
                            continue;
                        }

                        if (!phys.IsMissing && seg.IsExpected)
                        {
                            this.SubPrints.Add(
                                new AwareIndexReference
                                {
                                    Fingertype = this.Fingers[fingerIndex],
                                    FingertypeSequence = this.Fingers[index],
                                    NistIndex = this.NistIndexes[index]
                                });
                            fingerIndex++;
                        }
                        else
                        {
                            if (phys.IsMissing || !seg.IsExpected)
                            {
                                this.MissingPrints.Add(phys);
                            }
                            this.ExpectedCount -= 1;
                            if (this.NistIndexSwitch)
                            {
                                // WHEN NIST INDEX DO NOT SWITCH when print is missing, follow the primary index
                                fingerIndex++;
                            }
                        }
                    }
                }                
            }

            public void SetSlapsAndSegments(List<AwareIndexReference> capturedSlaps, ref bool endProcess)
            {
                if (this.capturedPrint.IsSpecial && this.capturedPrint.Print.Segments != null)
                {
                    this.SlapsForSpecialCase(capturedSlaps);
                    return;
                }
                
                // Non slap are not processed / Already processed slaps too
                if (!this.capturedPrint.IsSlap || capturedSlaps.Any(x => x.Parent.NistIndex == this.capturedPrint.Print.NistPosition))
                {
                    return;
                }
                
                // No sequence check for lower palms and hypothenar
                if (this.capturedPrint.Print.HandPart == HandPart.LowerPalm || this.capturedPrint.Print.HandPart == HandPart.Hypothenar)
                {
                    endProcess = true;
                    return;
                }
                
                this.Logger.Trace("LocalAwSequence - CheckSlap - IsSlap and not a special case");
                if (capturedSlaps.Any(x => x.Parent.NistIndex == this.capturedPrint.Print.NistPosition))
                {
                    return;
                }

                var childNistCode = this.capturedPrint.Print.NistPosition;
                if (this.Fingers == null && this.NistIndexes != null)
                {
                    childNistCode = this.NistIndexes[0];
                }
                
                var slap = new AwareIndexReference
                           {
                               Fingertype = this.capturedPrint.Fingertype,
                               NistIndex = childNistCode,
                               FingertypeSequence = this.capturedPrint.Fingertype,
                               Parent =
                                   new AwareIndexReference
                                   {
                                       Fingertype = this.capturedPrint.Fingertype,
                                       NistIndex = this.capturedPrint.Print.NistPosition,
                                       FingertypeSequence = this.capturedPrint.Fingertype
                                   }
                           };

                if (this.SubPrints.Count == 0)
                {
                    capturedSlaps.Add(slap);
                }
                else
                {
                    foreach (var reference in this.SubPrints)
                    {
                        reference.Parent = slap;
                        capturedSlaps.Add(reference);
                    }
                }

                this.UpdateSegmentsAndMinutias();
            }

            private void SlapsForSpecialCase(List<AwareIndexReference> capturedSlaps)
            {
                this.Logger.Trace("LocalAwSequence - SlapsForSpecialCase");
                foreach (var segment in this.capturedPrint.Print.Segments)
                {
                    segment.Reset();
                }

                var loneSegment = this.capturedPrint.Print.Segments.First(x => x.IsExpected && !x.IsMissing);

                var quality = 5;
                var rect = new Rectangle();

                try
                {
                    rect = seqChecker.GetCentering(this.capturedPrint.Fingertype, awSequenceCheck.AwareCenteringMethod.AWSEQ_AUTO_CENTERING);
                    quality = seqChecker.GetQualityScore(this.capturedPrint.Fingertype);
                }
                catch (Exception ex)
                {
                    var err = AwareSequenceError.ExceptionToError(ex);
                    this.Logger.Warn(ex, "SetSlapsAndSegments/Sequence  failed for print '{1}' : {0}", err, this.capturedPrint.Fingertype);
                }

                loneSegment.Position = rect;
                loneSegment.QualityScore = quality;

                var awIndex = new AwareIndexReference
                              {
                                  Fingertype = this.capturedPrint.Fingertype,
                                  NistIndex = loneSegment.Part.EndorsementIndex,
                                  FingertypeSequence = this.capturedPrint.Fingertype,
                                  Parent =
                                      new AwareIndexReference
                                      {
                                          Fingertype = this.capturedPrint.Fingertype,
                                          NistIndex = this.capturedPrint.Print.NistPosition,
                                          FingertypeSequence = this.capturedPrint.Fingertype
                                      }
                              };

                capturedSlaps.Add(awIndex);
            }

            private void UpdateSegmentsAndMinutias()
            {
                // Update segments
                Console.WriteLine("Segments for print {0}", PrintList.GetName(this.capturedPrint.Print));

                if (this.capturedPrint.Print.HasSegments)
                {
                    foreach (var segment in this.capturedPrint.Print.Segments)
                    {
                        segment.Reset();
                    }
                }

                if (this.SubPrints.Count == 0)
                {                   
                    return;
                }

                bool qualityError = false;

                foreach (var prn in this.SubPrints)
                {                    
                    var rect = new Rectangle();
                    int quality = 5;
                    int minutiaCount = 0;

                    try
                    {
                        rect = seqChecker.GetCentering(
                            prn.Fingertype,
                            awSequenceCheck.AwareCenteringMethod.AWSEQ_AUTO_CENTERING);
                        quality = seqChecker.GetQualityScore(prn.FingertypeSequence);

                        minutiaCount = seqChecker.NumberMinutia(prn.FingertypeSequence);
                        Console.WriteLine("Slap {0}, found {1} minutias", prn.Fingertype, minutiaCount);
                        var deltaCount = seqChecker.NumberCoreDelta(prn.FingertypeSequence);
                        Console.WriteLine("Slap {0}, found {1} deltas", prn.Fingertype, deltaCount);                        

                        if (deltaCount == 0 && minutiaCount < 20)
                        {
                            qualityError = true;
                        }

                        for (var index = 0; index < deltaCount; index++)
                        {
                            var deltaInfo = seqChecker.GetCoreDeltaInfo(prn.FingertypeSequence, index);
                            Console.WriteLine(
                                "Core/Delta #{3} type '{0}' at ({1}, {2})",
                                deltaInfo.type,
                                deltaInfo.x,
                                deltaInfo.y,
                                index);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        // this.capturedPrint.Print.Kind != HandPartKind.Palm && 
                        if ((this.printList.Rules.MinimumMinitiaCount == 0 || minutiaCount < this.printList.Rules.MinimumMinitiaCount))
                        {
                            qualityError = true;
                        }
                        
                        var err = AwareSequenceError.ExceptionToError(ex);
                        this.Logger.Warn(ex, "seqChecker failed for print '{1}' : {0}", err, prn.Fingertype);
                    }                                                            

                    AwareIndexReference prn1 = prn;
                    var part = this.printList.PhysicalParts.Single(x => x.EndorsementIndex == prn1.NistIndex);
                    var segment = this.capturedPrint.Print.Segments.Single(x => x.Part == part);

                    if (rect.Height < 0 || rect.Width < 0 || rect.Height > this.capturedPrint.Print.Image.Height
                        || rect.Width > this.capturedPrint.Print.Image.Width)
                    {
                        rect = new Rectangle();
                        if (this.NistIndexSwitch)
                        {
                            this.PrintCount -= 1;
                        }
                    }

                    segment.MinutiaCount = minutiaCount;
                    this.capturedPrint.Print.MinutiaCount += segment.MinutiaCount;
                    segment.Position = rect;
                    segment.QualityScore = quality;
                }

                if (qualityError && this.capturedPrint.Print.Kind != HandPartKind.Palm && this.printList.Rules.IsTemplateQualityVerified)
                {
                    this.capturedPrint.Print.TemplateErrors.Add(TemplateError.QualityCheckFailed);
                }
            }

            
        }
    }
}

