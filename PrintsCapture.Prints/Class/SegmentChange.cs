namespace PrintsCapture.Prints
{
    using System;
    using System.Windows.Controls.Primitives;

    public class SegmentChange
    {
        public PrintSegment Segment { get; set; }

        public bool IsOverrideChanged { get; set; }

        public bool IsExpectedChanged { get; set; }
    }
}
