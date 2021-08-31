namespace PrintsCapture.Cardscan
{
    public class ScanLocation
    {
        public ScanLocation()
        {

        }

        /// <summary>
        /// Scan size in inches
        /// </summary>
        
        public ScanLocation(double top, double left)
        {
            this.Top = top;
            this.Left = left;

            this.Unit = ScanUnit.Inches;
        }

        public ScanLocation(ScanUnit unit, double top, double left)
        {
            this.Unit = unit;
            this.Top = top;
            this.Left = left;
        }

        public double Top { get; set; }

        public double Left { get; set; }

        public ScanUnit Unit { get; set; }
    }

    public static class ScanLocationUtils
    {
        public static ScanLocation ToInches(this ScanLocation location)
        {
            if (location.Unit == ScanUnit.Inches)
            {
                return location;
            }
            var width = ScanConstants.PixelToInches((int)(location.Top));
            var height = ScanConstants.PixelToInches((int)(location.Left));

            return new ScanLocation(ScanUnit.Inches, width, height);
        }

        public static ScanLocation ToPixels(this ScanLocation location)
        {
            if (location.Unit == ScanUnit.Pixels)
            {
                return location;
            }
            var width = ScanConstants.InchesToPixels(location.Top);
            var height = ScanConstants.InchesToPixels(location.Left);

            return new ScanLocation(ScanUnit.Pixels, width, height);
        }


    }
}

