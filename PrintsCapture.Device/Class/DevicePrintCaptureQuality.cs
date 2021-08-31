namespace PrintsCapture.Device
{
    using PrintsCapture.Device.Enum;
    using PrintsCapture.Prints;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PrintCaptureQuality
    {
        public PrintCaptureQuality(PhysicalHandPart part, PrintQualityLevel quality)
        {
            this.HandPart = part;
            this.Quality = quality;
        }

        public PhysicalHandPart HandPart { get; set; }

        public PrintQualityLevel Quality { get; set; }
    }

}