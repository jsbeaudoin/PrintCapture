namespace PrintsCapture.Cardscan
{
    public class ScanSize
    {
        public ScanSize()
        {
            
        }

        /// <summary>
        /// Scan size in inches
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public ScanSize(double width, double height)
        {
            this.Width = width;
            this.Height = height;
            this.Unit = ScanUnit.Inches;
        }

        public ScanSize(ScanUnit unit, double width, double height)
        {
            this.Unit = unit;
            this.Width = width;
            this.Height = height;
        }

        public double Height { get; set; }

        public double Width { get; set; }

        public ScanUnit Unit { get; set; }
    }

    public static class ScanSizeUtils
    {
        public static ScanSize ToInches(this ScanSize size)
        {
            if (size.Unit == ScanUnit.Inches)
            {                
                return size;
            }
            var width = ScanConstants.PixelToInches((int)(size.Width));
            var height = ScanConstants.PixelToInches((int)(size.Height));

            return new ScanSize(ScanUnit.Inches, width, height);
        }

        public static ScanSize ToPixels(this ScanSize size)
        {
            if (size.Unit == ScanUnit.Pixels)
            {
                return size;
            }
            var width = ScanConstants.InchesToPixels(size.Width);
            var height = ScanConstants.InchesToPixels(size.Height);

            return new ScanSize(ScanUnit.Pixels, width, height);
        }

        
    }
}
