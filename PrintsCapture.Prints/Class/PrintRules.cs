namespace PrintsCapture.Prints
{
    using System.Threading;

    using PrintsCapture.Prints.Enum;

    public class PrintRules
    {
        public PrintRules()
        {
            this.Labels = new CustomLabels();
            this.IsSequenceEnabled = true;
            this.IsQualityEnabled = false;
            this.IsDataCompressed = true;
            this.QualityThreshold = 3;
            this.SequenceThreshold = 50;
            this.AntiSequencingThreshold = 25;
            this.IsSequenceChangingPosition = true;            
            this.CaptureGroup = PrintCaptureGroup.Standard14;
            this.CaptureGroupAllowed = PrintCaptureGroup.Standard14;
            this.CropTolerance = 50;
            this.MinimumMinitiaCount = 15;
            this.IsTemplateQualityVerified = false;
        }

        /// <summary>
        /// Threshold for anti sequencing
        /// </summary>
        public int AntiSequencingThreshold { get; set; }

        /// <summary>
        /// Determine how data is fetched
        /// </summary>
        public CaptureKind CaptureKind { get; set; }

        /// <summary>
        /// Print set to capture
        /// </summary>
        public PrintCaptureGroup CaptureGroup { get; set; }

        /// <summary>
        /// Possible Print set to capture
        /// </summary>
        public PrintCaptureGroup CaptureGroupAllowed { get; set; }

        /// <summary>
        /// Captures Flat prints only ?
        /// </summary>
        public bool IsFlatCaptureMode => this.CaptureGroup == PrintCaptureGroup.FlatOnly;

        /// <summary>
        /// Capture two thumbs simultaneously instead of one by one
        /// </summary>
        public bool CaptureTwoThumbs => this.IsSqMode || (this.CaptureGroup == PrintCaptureGroup.FlatOnly);

        public bool SplitCapturedThumbs => this.IsSqMode && !this.IsFlatCaptureMode;

        /// <summary>
        /// Minimum sequence score to be considered "Matched".1 = Doesn't match at all. 40 = some match. 100+ really good match.
        /// </summary>
        public int SequenceThreshold { get; set; }

        /// <summary>
        /// Maximum quality score to be considered valid. 0 Means ignore quality. 1 = best. 5 = worst.
        /// </summary>
        public int QualityThreshold { get; set; }

        /// <summary>
        /// When Sequence score of print does not equals or exceed Threshold, produce a warning instead of an error
        /// </summary>
        public bool IsSequenceEnabled { get; set; }

        /// <summary>
        /// When Quality of print is not below or equal to the threshold, produce a warning instead of an error
        /// </summary>
        public bool IsQualityEnabled { get; set; }

        /// <summary>
        /// When sequence determine that the print is not at the right place, allow it to change position
        /// </summary>
        public bool IsSequenceChangingPosition { get; set; }

        public bool IsOverrideFlatForbidden { get; set; }

        public bool IsOverrideRolledForbidden { get; set; }

        public bool IsDataCompressed { get; set; }
        
        public int CropTolerance { get; set; }
        public bool IsOverrideAlwaysShown { get; set; }

        public int MinimumMinitiaCount { get; set; }
        public bool IsEndorsementAllowed { get; set; }
        public int RetryNeededForOverride { get; set; }

        public bool IsTemplateQualityVerified { get; set; }

        public CaptureOrderMode OrderMode { get; set; } = CaptureOrderMode.Standard;

        public bool IsSqMode => this.OrderMode == CaptureOrderMode.Sq;

        public CustomLabels Labels { get; set; }
    }
}
