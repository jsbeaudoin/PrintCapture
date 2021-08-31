namespace PrintsCapture.Cardscan.ViewModel
{
    using System.Drawing;

    using PrintsCapture.Prints.Enum;

    public class PageScanZoneResultViewModel
    {
        public Bitmap Image { get; set; }

        public PrintZoneInfo ZoneInfo { get; set; }        

        public PrintResolution Resolution { get; set; }
    }
}
