using System.Collections.ObjectModel;

namespace PrintsCapture.Prints
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    using Enum;
    using Extension;

    public enum PrintProcessStatus
    {
        Undefined,
        ToBeScanned,
        InProcess,
        Submited,
        Success,
        TemplateError,
        ServiceError

    }

    /// <summary>
    /// Action taken by the user for a given print.
    /// Value 1-31 are not flags, but override codes.
    /// </summary>
    [Flags]
    public enum WizardAction
    {
        Undefined = 0,        
        OverrideCodeMask = 127,

        Accept = 128,        

        /// <summary>
        /// Not used yet (For cardscan purpose, when edited)
        /// </summary>
        ValidateAgain = 256,
        ScanAgain = 512,

    }

    public class PrintInfo
    {

        public static string GetKey(Hand hand, HandPart part, HandScanKind scan)
        {
            return $"{hand}.{scan}.{part}";
        }

        private Bitmap image;

        public PrintInfo(PrintList printList, PhysicalHandPart physicalpart, HandScanKind scan, HandPartKind kind, PrintCaptureGroup groupsMembership, int position =-1)
        {
            this.PrintList = printList;
            this.PhysicalPart = physicalpart;
            this.ScanKind = scan;
            this.Kind = kind;
            this.GroupMembership = groupsMembership;
            this.Key = GetKey(physicalpart.Hand, physicalpart.HandPart, scan);
            this.NistPosition = position;

            this.Reset();
            
            //this.InternalPrints = new List<PrintInfo>();
        }
        
        public string Key { private set; get; }

        public string MatchedKey { get; set; }

        public PrintCaptureGroup GroupMembership { get; }

        public PrintList PrintList { get; private set; }

        public int NistPosition { get; set; }        

        public WizardAction UserAction { get; set; }

        /// <summary>
        /// Gets the hand part.
        /// </summary>
        public HandPart HandPart { get { return this.PhysicalPart.HandPart; } }

        /// <summary>
        /// Gets the hand.
        /// </summary>
        public Hand Hand { get { return this.PhysicalPart.Hand; } }

        /// <summary>
        /// Gets the kind.
        /// </summary>
        public HandPartKind Kind { get; private set; }

        /// <summary>
        /// Gets the scan kind.
        /// </summary>
        public HandScanKind ScanKind { get; private set; }

        public PhysicalHandPart PhysicalPart { get; private set; }
        

        /// <summary>
        /// Gets or sets the resolution.
        /// </summary>
        public PrintResolution Resolution { get; set; }

        /// <summary>
        /// Gets or sets the endorsement finger.
        /// </summary>
        public PhysicalHandPart EndorsementFinger { get; set; }

        /// <summary>
        /// RCMP override code
        /// </summary>
        public int OverrideCode { get; set; }

        /// <summary>
        /// Gets or sets the override user reason.
        /// </summary>
        public string OverrideUserReason { get; set; }

        /// <summary>
        /// Gets or sets the Sequence analyzed value. When false, sequence check has not been run yet.
        /// </summary>
        public bool SequenceAnalyzed { get; set; }

        /// <summary>
        /// Gets or sets the sequence score for self print.
        /// </summary>
        public int SequenceSelfScore { get; set; }

        /// <summary>
        /// Gets or sets the sequence best score.
        /// </summary>
        public int SequenceBestScore { get; set; }

        /// <summary>
        /// Gets or sets the sequence best score position.
        /// </summary>
        public int SequenceBestScorePosition { get; set; }

        public List<SequenceCheckResult> AllScores { get; set; }

        /// <summary>
        /// Gets or sets the sequence score.
        /// </summary>
        public int SequenceScore { get; set; }

        /// <summary>
        /// Gets or sets the minutia count
        /// </summary>
        public int MinutiaCount { get; set; }

        /// <summary>
        /// Gets or sets the quality score.
        /// </summary>
        public int QualityScore { get; set; }

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        public Bitmap Image
        {
            get
            {
                return this.image;
            }
            set
            {
                if (Equals(value, this.image))
                {
                    return;
                }

                this.image = value;
                this.ImageForProcessing = value; //value.DeepClone();                
                this.ImageKey = Guid.NewGuid();
            }
        }

        public Bitmap ImageForProcessing { get; private set; }

        public Bitmap OriginalImage { get; set; }

        /// <summary>
        /// Gets or sets the process status.
        /// </summary>
        public PrintProcessStatus ProcessStatus { get; set; }

        // Computed properties

        public bool IsMissing
        {
            get
            {
                return this.PhysicalPart.IsMissing;
            }            
        }

        public bool IsOverriden
        {
            get
            {
                return this.OverrideCode > 0;
            }
        }

        public bool IsSequenceCheckEnabled
        {
            get
            {
                return this.ScanKind != HandScanKind.Flat || this.IsEndorsement;
            }
        }

        public bool IsSlap
        {
            get
            {
                return this.ScanKind == HandScanKind.Flat && !this.IsEndorsement;
            }
        }

        public bool IsEndorsement
        {
            get
            {
                return this.HandPart == HandPart.Endorsement;
            }
        }

        public bool IsInError
        {
            get
            {
                return this.Status == PrintStatus.InError || this.Status == PrintStatus.InErrorSwapped;
            }
        }

        public bool IsReady
        {
            get
            {
                return this.Status == PrintStatus.Ok || this.Status == PrintStatus.Validated
                       || this.Status == PrintStatus.ValidatedSwapped;
            }
        }

        public bool HasSegments
        {
            get { return this.Segments != null && this.Segments.Count > 0; }
        }

        public PrintStatus Status { get; set; }

        public List<PrintError> FailedValidations { get; private set; }

        public List<TemplateError> TemplateErrors { get; private set; }        

        public ReadOnlyCollection<PrintSegment> Segments { get; internal set; }

        public int MatchedNistPosition { get; set; }

        public Guid ImageKey { get; set; }

        /// <summary>
        /// Informative purspose mostly. And for retry count. Reseted manually when an auto capture starts
        /// </summary>
        public int CaptureCount { get; set; }

        public bool IsAcceptedByUser { get; set; }

        public void Reset()
        {
            this.MatchedKey = string.Empty;
            this.MatchedNistPosition = 0;

            this.OverrideCode = 0;
            this.OverrideUserReason = null;
            
            this.Image = null;
            this.ImageKey = Guid.Empty;            

            this.QualityScore = 0;
            this.SequenceScore = 0;
            this.MinutiaCount = 0;

            this.AllScores = new List<SequenceCheckResult>();
            this.SequenceBestScore = 0;
            this.SequenceBestScorePosition = 0;
            this.SequenceSelfScore = 0;
            
            this.Status = PrintStatus.Ok;
            this.ProcessStatus = PrintProcessStatus.Undefined;            
            this.SequenceAnalyzed = false;
            this.FailedValidations = new List<PrintError>();
            this.TemplateErrors = new List<TemplateError>();
            this.IsAcceptedByUser = false;

            if (this.HasSegments)
            {
                foreach (var printSegment in this.Segments)
                {
                    printSegment.Reset();
                }
            }
        }

        
    }





    }
