using System.Collections.Generic;

namespace PrintsCapture.Prints
{
    using System.Drawing;

    public class PrintSegment
    {
        public PrintSegment(PhysicalHandPart part)
        {
            this.Part = part;
            this.IsExpected = true;
            //this.SelfScore = new List<SequenceCheckResult>();
            this.SelfScore = new SequenceCheckResult();
        }

        public PhysicalHandPart Part { get; private set; }

        public string MissingCode
        {
            get { return this.Part.MissingCode; }
        }

        public string MissingDate
        {
            get { return this.Part.MissingDate; }
        }


        public Rectangle Position { get; set; }

        public int QualityScore { get; set; }
        
        public int OverrideCode { get; set; }

        public string OverrideText { get; set; }

        public bool IsExpected { get; set; }

        //public List<SequenceCheckResult> AllScores { get; set; }

        public SequenceCheckResult SelfScore { get; set; }

        public void Reset()
        {
            this.Position = new Rectangle();
            this.QualityScore = 0;            
            this.OverrideCode = 0;
            this.MinutiaCount = 0;
            this.OverrideText = string.Empty;
            //this.AllScores.Clear();
            this.SelfScore = new SequenceCheckResult();
        }

        public bool IsOverriden
        {
            get { return this.OverrideCode != 0; }
        }

        public bool IsMissing { get { return ! string.IsNullOrEmpty(this.MissingCode); } }

        public int MinutiaCount { get; set; }
    }
}
