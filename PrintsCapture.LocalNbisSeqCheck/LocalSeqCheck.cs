using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Nbis;
using UniBIO.Services.Communication.TransactionWatcherService;

namespace PrintsCapture.LocalAwSeqCheck
{    
    using NLog;
    

    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Sequence;

    using UniBIO = UniBIO.Services.Communication.BiometricService;

    public class LocalSeqCheck : BaseSeqCheckService
    {       

        private class NbisIndexReference
        {
            public int NistIndex { get; set; }

            public int SequenceIndex { get; set; }            

        }

        private class CapturedPrint
        {            

            public int NistIndex { get; set; }

            public PrintInfo Print { get; set; }

            public Guid TemplateImageKey { get; set; }

            public bool IsSlap { get; set; }                      

            public bool IsUpdateRequired
            {
                get
                {
                    return this.Print.ImageKey != this.TemplateImageKey || !this.Print.SequenceAnalyzed;
                }
            }

            //public List<PrintSegment> Segments { get; private set; }

        }

        private readonly List<NbisIndexReference> capturedSlap = new List<NbisIndexReference>();        

        private List<CapturedPrint> capturedPrints;

        private NbisSeqChecker seqChecker;

        public LocalSeqCheck(CaptureKind captureProvenance, PrintList prints)
            : base(captureProvenance, prints)
        {
            throw new NotImplementedException("Local NBis Check NOT COMPLETED. CANNOT BE USED");
        }

        public override bool Connect()
        {            
            this.OnConnectionStateChanged(true);
            return true;
        }

        public void Test1(Bitmap test)
        {
            //var leftFingers = new[] { Finger.LeftIndex, Finger.LeftLittle, Finger.}

            //Nbis.Segmentation.FromBitmap()
        }

        public override bool StartSession()
        {
            this.Logger.Trace("LocalNbisSeqCheck - StartSession");
            if (this.InSession)
            {
                return true;
            }
            this.capturedPrints = new List<CapturedPrint>();

            this.InSession = true;

            this.Logger.Trace("LocalNbisSeqCheck - StartSession - Creating Objects");

            try
            {
                seqChecker = new NbisSeqChecker();
            }
            catch (Exception ex)
            {
                // if a dll is missing, it will throw an error !!
                this.Logger.Error(ex, "Cannot start Sequence session");                
                this.OnConnectionStateChanged(false);
                return false;
            }                       

            this.SetInitialMissingFingers();

            //this.SetinitialSpecialCases();           

            this.SetCaptureMode();

            this.OnSessionStartOrEnd(true);
            return true;
        }

       

        public override void AddPrint(PrintInfo print)
        {
            this.Logger.Trace("LocalNbisSeqCheck - AddPrint ({0])", PrintList.GetName(print));
            this.AddOrUpdateTemplate(print);
            this.SwapPrints();
        }

        public override void AddPrintRange(List<PrintInfo> printsToSequence)
        {
            this.Logger.Trace("LocalNbisSeqCheck - AddPrintRange");
            foreach (var printInfo in printsToSequence)
            {
                this.AddOrUpdateTemplate(printInfo);
            }
            this.SwapPrints();
        }

        public override string EndSession(bool accept)
        {
            this.Logger.Trace("LocalNbisSeqCheck - EndSession");
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
            this.Logger.Trace("LocalNbisSeqCheck - ResetSession");
            //this.specialCases.Clear();            

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
            this.Logger.Trace("LocalNbisSeqCheck - SetPrintOverride");
            // done in PrintInfo, so nothing needed for the service            
        }

        public override void SetSegmentOverride(PrintInfo print, PrintSegment segment)
        {
            this.Logger.Trace("LocalNbisSeqCheck - SetSegmentOverride");            
        }

        public override void SequencePositionRuleChanged()
        {
           this.Logger.Trace("LocalNbisSeqCheck - SequencePosistionRuleChanged");
           this.SwapPrints();
        }

        public override void OnDeviceChanged()
        {
            this.Logger.Trace("LocalNbisSeqCheck - OnDeviceChanged");
            // nothing
        }

        public override void Test()
        {
            this.AntiSequencing();
        }

        protected override List<PrintSetWarning> CheckSessionIntegrity()
        {
            this.Logger.Trace("LocalNbisSeqCheck - CheckSessionIntegrity");
            this.Logger.Trace("No integirty to check, service is notr async and not remote");
            return new List<PrintSetWarning>();
        }

        public override List<PrintInfo> GetCapturedPrints()
        {
            this.Logger.Trace("LocalNbisSeqCheck - GetCapturedPrints");
            var captured =  this.capturedPrints.Select(x => x.Print).ToList();

            //var missing = this.Prints.GetPrintListToVerify().Where(x => x.IsMissing && captured.All(y => y.NistPosition != x.NistPosition));
            //captured.AddRange(missing);                        
            
            return captured;
        }

        public List<PrintInfo> GetCapturedSegments()
        {
            this.Logger.Trace("LocalNbisSeqCheck - GetCapturedSegments");
            var slapList = this.capturedPrints.Where(x => x.Print.HasSegments).Select(y => y.Print).ToList();

            return slapList;
        }

        private void SetInitialMissingFingers()
        {
            // set missing fingers
            this.Logger.Trace("LocalNbisSeqCheck - StartSession - Setting Missings 1");
            var missings = this.Prints.PhysicalParts.Where(x => x.IsMissing && x.Kind != HandPartKind.Palm).Select(x => x.EndorsementIndex).ToList();

            seqChecker.SetMissing(missings);

            this.Logger.Trace("LocalNbisSeqCheck - StartSession - Setting Missings 2");
            var missingCaptured = this.Prints.GetPrintListToVerify().Where(x => x.IsMissing);
            foreach (var printInfo in missingCaptured)
            {
                var captured = new CapturedPrint
                {
                    Print = printInfo,
                    NistIndex = printInfo.NistPosition,                    
                    IsSlap = printInfo.IsSlap
                };
                this.capturedPrints.Add(captured);

            }
        }        

        private void SwapPrints()
        {
            this.Logger.Trace("LocalNbisSeqCheck - SwapPrints");
            var swapper = new PrintSwapper(this.Prints);

            var list = swapper.GetSwapList();

            if (list == null || list.Count == 0)
            {
                this.Logger.Trace("LocalNbisSeqCheck - SwapPrints - Nothing to swap");
                return;
            }

            this.Logger.Trace("LocalNbisSeqCheck - SwapPrints - {0} print(s) to swap", list.Count);

            foreach (var printSwap in list)
            {
                var swap = printSwap;

                var print = this.Prints.Prints.Single(x => x.NistPosition == (int)swap.ScannedPosition);                
                var newPrint= this.Prints.Prints.Single(x => x.NistPosition == (int)swap.Position);

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
            this.Logger.Trace("LocalNbisSeqCheck - GetPrint ({0])", PrintList.GetName(info));
            var result = this.capturedPrints.SingleOrDefault(x => x.Print == info);

            if (result == null)
            {
                result = new CapturedPrint
                {
                    NistIndex = info.NistPosition,                    
                    Print = info,
                    IsSlap = info.ScanKind == HandScanKind.Flat && info.HandPart != HandPart.Endorsement
                };
                this.capturedPrints.Add(result);
            }
                

            return result;
        }

        private void AddOrUpdateTemplate(PrintInfo info)
        {
            this.Logger.Trace("LocalNbisSeqCheck - AddOrUpdateTemplate ({0})", PrintList.GetName(info));
            var print = this.GetPrint(info);
            
            if (print.Print.IsEndorsement)
            {
                this.CheckEndorsement(info);
                return;
            }

            var segmentsMissing = print.Print.Segments == null
                ? new List<PrintSegment>()
                : print.Print.Segments.Where(x => !x.IsExpected && !x.Part.IsMissing).ToList();

            //print.IsSpecial = print.Print.Segments != null && print.Print.HandPart != HandPart.TwoThumbs && print.Print.Segments.Count(x => x.IsExpected && !x.Part.IsMissing) == 1;

            if (print.IsUpdateRequired)
            {
                this.Logger.Trace("LocalNbisSeqCheck - AddOrUpdateTemplate - Update required");
                // a segment ignored must be set missing before validation and restored as present after
                //foreach (var unexpSegment in segmentsMissing)
                //{
                //    seqChecker.SetFingerMissing((awSequenceCheck.AwareFingerType)unexpSegment.Part.EndorsementIndex, awSequenceCheck.AwareFingerMissingCode.AW_FNG_MISSING);                    
                //}

                if (!this.SetTemplate(print))
                {
                    this.OnPrintModified(info);
                    return;
                }
                
                info.QualityScore = seqChecker.GetQualityScore(print.NistIndex);
            }            

            if (!this.CheckSlap(print))
            {
                this.Logger.Warn("LocalNbisSeqCheck - AddOrUpdateTemplate - Slap verifcation failed");
                info.ProcessStatus = PrintProcessStatus.TemplateError;
                info.TemplateErrors.Add(TemplateError.WrongTemplateCount);
                this.OnPrintModified(info);
            }

            
            foreach (var unexpSegment in segmentsMissing)
            {
                //seqChecker.SetFingerMissing((awSequenceCheck.AwareFingerType)unexpSegment.Part.EndorsementIndex, awSequenceCheck.AwareFingerMissingCode.AW_FNG_PRESENT);                    
            }
            
            if (print.IsSlap)
            {
                this.Logger.Trace("LocalNbisSeqCheck - AddOrUpdateTemplate - Recalculating scores");
                this.RecalculateScores();                
            }
            else
            {
                this.Logger.Trace("LocalNbisSeqCheck - AddOrUpdateTemplates - Calculating scores");
                this.CalculateScores(print);
            }

            this.Logger.Trace("LocalNbisSeqCheck - AddOrUpdateTemplates - setting status");
            print.Print.ProcessStatus = PrintProcessStatus.Success;
            print.Print.SequenceAnalyzed = true;

            this.OnPrintModified(info);
        }

                

        private bool SetTemplate(CapturedPrint capturedPrint)
        {
            this.Logger.Trace("LocalNbisSeqCheck - SetTemplate");
            var refImage = capturedPrint.Print.ImageForProcessing;
            
           
            string errorMsg = string.Empty;            
                        
            try
            {
                seqChecker.AddPrint(capturedPrint.NistIndex, refImage);                
            }
            catch (Exception ex)
            {
                this.Logger.Error("LocalNbisSeqCheck - SetTemplate", ex.Message);

                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString() + " - SetTemplate Exception : " + ex.Message );

                capturedPrint.Print.ProcessStatus = PrintProcessStatus.ServiceError;                                                
            }
            capturedPrint.TemplateImageKey = capturedPrint.Print.ImageKey;

            if (capturedPrint.Print.ProcessStatus == PrintProcessStatus.ServiceError)
            {
                this.Logger.Error("Sequence check failed for '{0}', with error : {1}", capturedPrint.Print.NistPosition, errorMsg);
                capturedPrint.Print.ProcessStatus = PrintProcessStatus.ServiceError;
                this.OnPrintModified(capturedPrint.Print);
                return false;
            }

            return true;
        }

        

        private void CheckEndorsement(PrintInfo endorsement)
        {
            const int endorsementIndex = 16;
            this.seqChecker.AddPrint(endorsementIndex, endorsement.ImageForProcessing);
            endorsement.QualityScore = this.seqChecker.GetQualityScore(endorsementIndex);
            
            this.Logger.Trace("LocalNbisSeqCheck - CheckEndorsement - Getting reference print position");            
            
            try
            {
                endorsement.SequenceScore = this.seqChecker.GetMatchScore(endorsementIndex, endorsement.EndorsementFinger.EndorsementIndex);
            }
            catch (Exception)
            {
                this.Logger.Trace("LocalNbisSeqCheck - CheckEndorsement - Cannot Get reference match score");
                endorsement.SequenceScore = 0;
            }
            
            endorsement.SequenceSelfScore = endorsement.SequenceScore;
            endorsement.AllScores.Add(new SequenceCheckResult { Position = endorsementIndex, Score = endorsement.SequenceScore });

            this.OnPrintModified(endorsement);
        }

        

        private bool CheckSlap(CapturedPrint print)
        {
            this.Logger.Trace("LocalNbisSeqCheck - CheckSlap");            

            var captureInfo = this.GetCaptureInfo(print);
            
            captureInfo.DefineMissingAndSubPrints();
            
            // Clear captured slaps list
            var toremove = this.capturedSlap.ToList();
            foreach (var awareIndexReference in toremove)
            {
                this.capturedSlap.Remove(awareIndexReference);
            }

            bool endProcess = false;

            // set minutia count
            print.Print.MinutiaCount = seqChecker.GetMinutiaCount(print.NistIndex);            
            

            // Set slaps into list and minutia count and 
            captureInfo.SetSlapsAndSegments(capturedSlap.Select(x => x.NistIndex).ToList(), ref endProcess);

            if (endProcess)
            {
                return true;
            }

            this.VerifyHandess(print);
            
            return captureInfo.ExpectedCount == captureInfo.PrintCount;
        }

        private PrintCaptureInfo GetCaptureInfo(CapturedPrint print)
        {
            throw new NotImplementedException();
        }

        private void VerifyHandess(CapturedPrint print)
        {
            // custom handness check :-)
            // Returns none for other than 4slaps.
            if (print.Print.HandPart == HandPart.FourFlats)
            {
                this.Logger.Trace("LocalNbisSeqCheck - CheckSlap - Handness validation");                

                var handess = HandnessFinder.ReturnHandness(print.Print);
                if (handess != Hand.None && handess != print.Print.Hand)
                {
                    print.Print.TemplateErrors.Add(TemplateError.WrongHand);
                }                                
            }
        }
       
        private void CalculateScores(CapturedPrint capturedPrint)
        {
            var print = capturedPrint.Print;
            var scores = this.GetScores(capturedPrint.NistIndex);

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

        private List<SequenceCheckResult> GetScores(int nistIndex, bool includeTwoThumbs = true)
        {
            var result = new List<SequenceCheckResult>();
            var slapList =
                this.capturedSlap.Where( x => x.NistIndex != 15 || includeTwoThumbs).ToList();

            

            return result;

            //return (from reference in this.capturedSlap.Where(x => x.Parent.Fingertype != awSequenceCheck.AwareFingerType.AW_PLAIN_LEFT_RIGHT_THUMBS || includeTwoThumbs) 
            //        where reference.Fingertype != fingerType
            //        let score = this.GetMatchScore(fingerType, reference.Fingertype)
            //        select new SequenceCheckResult { Position = reference.NistIndex, Score = score }).ToList();
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
            this.Logger.Trace("LocalNbisSeqCheck - Antisequencing");
                       
            
        }

        /// <summary>
        /// return true if print was modified, else false
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="scores"></param>
        /// <returns></returns>
        private bool AssignScoresToSegments(int reference, List<SequenceCheckResult> scores)
        {
            return false;
            
        }

        private void ResetSequenceScore(PrintInfo print)
        {
            this.Logger.Trace("LocalNbisSeqCheck - ResetSequenceScore");
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
            private readonly object seqChecker;
            private readonly CapturedPrint capturedPrint;

            private readonly PrintList printList;

            private List<int> SubPrints { get; set; }
            private List<PhysicalHandPart> MissingPrints { get; set; }
            private Logger Logger { get; set; }

            public PrintCaptureInfo(CapturedPrint print, PrintList list)
            {                
                
            }

            public int PrintCount { get; set; }
            public int ExpectedCount { get; set; }
            public int[] NistIndexes { get; set; }
            public int[] Fingers { get; set; }
            public bool NistIndexSwitch { get; set; }




            public void DefineMissingAndSubPrints()
            {
                
            }

            public void SetSlapsAndSegments(List<int> capturedSlaps, ref bool endProcess)
            {


                this.UpdateSegmentsAndMinutias();
            }

            private void SlapsForSpecialCase(List<int> capturedSlaps)
            {
                

                //capturedSlaps.Add(1);
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
                        

                    }
                    catch (Exception ex)
                    {
                        // this.capturedPrint.Print.Kind != HandPartKind.Palm && 
                        if ((this.printList.Rules.MinimumMinitiaCount == 0 || minutiaCount < this.printList.Rules.MinimumMinitiaCount))
                        {
                            qualityError = true;
                        }

                        
                        this.Logger.Warn(ex, "seqChecker failed for print '{1}' : {0}", ex.Message, 1);
                    }

                    var prn1 = prn;
                    var part = this.printList.PhysicalParts.Single(x => x.EndorsementIndex == 1);
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

