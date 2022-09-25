namespace PrintsCapture.Prints.Extension
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using PrintsCapture.Prints.Enum;

    public static class PrintInfoExtension
    {
        private static List<PrintCaptureSize>  captureSizes;

        public static Size GetSize(HandPart part, HandPartKind partKind, HandScanKind scan, PrintResolution resolution, bool isFlatCapture)
        {
            if (captureSizes == null)
            {
                captureSizes = CreateList();
            }

            // HandPartKind.Unknown and HandPart.Unknown  are used as wild values in search list.

            if (partKind == HandPartKind.Unknown)
            {
                if (part == HandPart.CompletePalm || part == HandPart.LowerPalm || part == HandPart.UpperPalm
                    || part == HandPart.Hypothenar)
                {
                    partKind = HandPartKind.Palm;
                }
                else if (part == HandPart.TwoThumbs || part == HandPart.FourFlats || part == HandPart.Unknown)
                {
                    partKind = HandPartKind.Other;
                }
                else
                {
                    partKind = HandPartKind.Finger;
                }
            }

            var dpi = resolution.ToDpi();
            var sz = captureSizes.Where(x => x.Kind == HandPartKind.Unknown && x.Part == part && x.Scan == scan && x.Dpi == dpi && x.IsType14 == isFlatCapture).ToList();

            if (sz.Count < 1)
            {
                // search generic sizes
                var genericSz = captureSizes.Where(x => x.Kind == partKind && x.Part == HandPart.Unknown && x.Scan == scan && x.Dpi == dpi && x.IsType14 == isFlatCapture).ToList();

                sz = genericSz;
            }

            if (sz.Count != 1)
            {
                throw new ApplicationException("PrintInfoExtension - GetSize : Print has no defined Size !!");
            }

            return sz[0].Size;           
            
        }

        public static Size CaptureSize(this PrintInfo print, bool isFlatCapture)
        {
            return GetSize(print.HandPart, print.Kind, print.ScanKind, print.Resolution, isFlatCapture);
        }

        private static List<PrintCaptureSize> CreateList()
        {
            var sizes = new List<PrintCaptureSize>();

            // flats -- 500 dpi
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.TwoThumbs, HandScanKind.Flat, 500, new Size(1600, 1500), true)); 
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.FourFlats, HandScanKind.Flat, 500, new Size(1600, 1500), true));
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.Endorsement, HandScanKind.Flat, 500, new Size(500, 1000), true));

            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.TwoThumbs, HandScanKind.Flat, 500, new Size(1600, 1500)));
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.FourFlats, HandScanKind.Flat, 500, new Size(1600, 1000)));            
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.Endorsement, HandScanKind.Flat, 500, new Size(500, 1000)));
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.UpperPalm, HandScanKind.Flat, 500, new Size(2750, 2750)));
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.LowerPalm, HandScanKind.Flat, 500, new Size(2750, 2750)));
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.Hypothenar, HandScanKind.Flat, 500, new Size(900, 2500)));
            

            // flats -- 1000 dpi
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.TwoThumbs, HandScanKind.Flat, 1000, new Size(3200, 3000), true));
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.FourFlats, HandScanKind.Flat, 1000, new Size(3200, 3000), true));            
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.Endorsement, HandScanKind.Flat, 1000, new Size(1000, 2000), true));

            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.TwoThumbs, HandScanKind.Flat, 1000, new Size(3200, 3000)));
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.FourFlats, HandScanKind.Flat, 1000, new Size(3200, 2000)));            
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.Endorsement, HandScanKind.Flat, 1000, new Size(1000, 2000)));
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.UpperPalm, HandScanKind.Flat, 1000, new Size(5500, 5500)));
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.LowerPalm, HandScanKind.Flat, 1000, new Size(5500, 5500)));
            sizes.Add(new PrintCaptureSize(HandPartKind.Unknown, HandPart.Hypothenar, HandScanKind.Flat, 1000, new Size(1800, 5000)));
            

            // generic flats and rolled fingers
            sizes.Add(new PrintCaptureSize(HandPartKind.Finger, HandPart.Unknown, HandScanKind.Flat, 500, new Size(500, 1000)));
            sizes.Add(new PrintCaptureSize(HandPartKind.Finger, HandPart.Unknown, HandScanKind.Flat, 1000, new Size(1000, 2000)));
            sizes.Add(new PrintCaptureSize(HandPartKind.Finger, HandPart.Unknown, HandScanKind.Flat, 500, new Size(500, 1000), true));
            sizes.Add(new PrintCaptureSize(HandPartKind.Finger, HandPart.Unknown, HandScanKind.Flat, 1000, new Size(1000, 2000), true));

            sizes.Add(new PrintCaptureSize(HandPartKind.Finger, HandPart.Unknown, HandScanKind.Rolled, 500, new Size(800, 750)));
            sizes.Add(new PrintCaptureSize(HandPartKind.Finger, HandPart.Unknown, HandScanKind.Rolled, 1000, new Size(1600, 1500)));

            return sizes;
        }

        private class PrintCaptureSize
        {
            public HandPartKind Kind { get; private set; }

            public HandPart Part { get; private set; }

            public HandScanKind Scan { get; private set; }

            public int Dpi { get; private set; }

            public Size Size { get; private set; }

            public bool IsType14 { get; private set; }

            public PrintCaptureSize(HandPartKind kind, HandPart part, HandScanKind scan, int dpi, Size size, bool isType14 = false)
            {
                this.Kind = kind;
                this.Part = part;
                this.Scan = scan;
                this.Dpi = dpi;
                this.Size = size;
                this.IsType14 = isType14;
            }
        }
    }
}
