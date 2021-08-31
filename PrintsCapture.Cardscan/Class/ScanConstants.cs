namespace PrintsCapture.Cardscan
{
    using System;
    using System.Windows.Media;

    public static class ScanConstants
    {
        static ScanConstants()
        {
            BrushOpacity = 0.4;
            LeftFingerBrush = Brushes.GreenYellow;
            RightFingerBrush = Brushes.MediumAquamarine;
            OtherZoneBrush = Brushes.Aqua;
            CurrentDpi = DefaultDpi;
        }

        public const int DefaultDpi = 96; // Dot per inch

        public const double MinInchHeight = 0.25; // 0.25 in

        public const double MinInchWidth = 0.25; // 0.25 in

        public static Brush LeftFingerBrush { get; set; }

        public static Brush RightFingerBrush { get; set; }

        public static Brush OtherZoneBrush { get; set; }

        public static Double BrushOpacity { get; set; }

        public static int CurrentDpi { get; set; }

        public static double PixelToInches(int pixels)
        {
            return Math.Round(pixels / (float)CurrentDpi, 6);
        }

        public static double InchesToPixels(double inches)
        {
            return Math.Round(inches * CurrentDpi, 6);
        }
    }
}
