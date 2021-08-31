namespace PrintsCapture.Cardscan
{
    using System.Collections.Generic;

    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;

    public class PrintZoneInfo
    {
        private static List<PrintZoneInfo> allPrintZones;

        public string Id { get; set; }

        public string Label { get; set; }

        public ScanSize Size { get; set; }

        public Hand Hand { get; set; }

        public HandPart HandPart { get; set; }

        public HandScanKind ScanKind { get; set; }

        public PrintZoneGroup Group { get; set; }

        public int SortOrder { get; set; }

        public static List<PrintZoneInfo> GetPrintList(bool isFlat)
        {
            if (allPrintZones != null)
            {
                return allPrintZones;
            }

            allPrintZones = new List<PrintZoneInfo>();
            
            

            allPrintZones.Add(new PrintZoneInfo { Hand= Hand.Right, HandPart = HandPart.FourFlats, Id = "RightFourFlat", Size = new ScanSize(3.2, isFlat ? 3 : 2 ), ScanKind = HandScanKind.Flat, SortOrder = 1 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Left, HandPart = HandPart.FourFlats, Id = "LeftFourFlat", Size =  new ScanSize(3.2, isFlat ? 3 : 2 ), ScanKind = HandScanKind.Flat, SortOrder = 2 });

            
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.None, HandPart = HandPart.TwoThumbs, Id = "TwoThumbs", Size = new ScanSize(3.2, 3), ScanKind = HandScanKind.Flat, SortOrder = 3 });
            
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Right, HandPart = HandPart.Thumb, Id = "RightThumbFlat", Size = new ScanSize(1, 2), ScanKind = HandScanKind.Flat, SortOrder = 10 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Left, HandPart = HandPart.Thumb, Id = "LeftThumbFlat", Size = new ScanSize(1, 2), ScanKind = HandScanKind.Flat, SortOrder = 11 });
            
            

            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.None, HandPart = HandPart.Endorsement, Id = "EndorsementFingerFlat", Size = new ScanSize(1, 2), ScanKind = HandScanKind.Flat, SortOrder = 12 });

            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Right, HandPart = HandPart.Thumb, Id = "RightThumb", Size = new ScanSize(1.6, 1.5), ScanKind = HandScanKind.Rolled, SortOrder = 20 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Right, HandPart = HandPart.Index, Id = "RightIndex", Size = new ScanSize(1.6, 1.5), ScanKind = HandScanKind.Rolled, SortOrder = 21 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Right, HandPart = HandPart.Middle, Id = "RightMiddle", Size = new ScanSize(1.6, 1.5), ScanKind = HandScanKind.Rolled, SortOrder = 22 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Right, HandPart = HandPart.Ring, Id = "RightRing", Size = new ScanSize(1.6, 1.5), ScanKind = HandScanKind.Rolled, SortOrder = 23 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Right, HandPart = HandPart.Little, Id = "RightLittle", Size = new ScanSize(1.6, 1.5), ScanKind = HandScanKind.Rolled, SortOrder = 24 });

            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Left, HandPart = HandPart.Thumb, Id = "LeftThumb", Size = new ScanSize(1.6, 1.5), ScanKind = HandScanKind.Rolled, SortOrder = 25 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Left, HandPart = HandPart.Index, Id = "LeftIndex", Size = new ScanSize(1.6, 1.5), ScanKind = HandScanKind.Rolled, SortOrder = 26 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Left, HandPart = HandPart.Middle, Id = "LeftMiddle", Size = new ScanSize(1.6, 1.5), ScanKind = HandScanKind.Rolled, SortOrder = 27 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Left, HandPart = HandPart.Ring, Id = "LeftRing", Size = new ScanSize(1.6, 1.5), ScanKind = HandScanKind.Rolled, SortOrder = 28 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Left, HandPart = HandPart.Little, Id = "LeftLittle", Size = new ScanSize(1.6, 1.5), ScanKind = HandScanKind.Rolled, SortOrder = 29 });


            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Right, HandPart = HandPart.UpperPalm, Id = "RightUpperPalm", Size = new ScanSize(5.5, 5.5), ScanKind = HandScanKind.Flat, SortOrder = 40 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Right, HandPart = HandPart.LowerPalm, Id = "RightLowerPalm", Size = new ScanSize(5.5, 5.5), ScanKind = HandScanKind.Flat, SortOrder = 41 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Right, HandPart = HandPart.Hypothenar, Id = "RightHypothenar", Size = new ScanSize(1.8, 5.0), ScanKind = HandScanKind.Flat, SortOrder = 42 });

            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Left, HandPart = HandPart.UpperPalm, Id = "LeftUpperPalm", Size = new ScanSize(5.5, 5.5), ScanKind = HandScanKind.Flat, SortOrder = 50 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Left, HandPart = HandPart.LowerPalm, Id = "LeftLowerPalm", Size = new ScanSize(5.5, 5.5), ScanKind = HandScanKind.Flat, SortOrder = 51 });
            allPrintZones.Add(new PrintZoneInfo { Hand = Hand.Left, HandPart = HandPart.Hypothenar, Id = "LeftHypothenar", Size = new ScanSize(1.8, 5.0), ScanKind = HandScanKind.Flat, SortOrder = 52 });

            // update all labels !

            foreach (var zone in allPrintZones)
            {
                zone.Label = PrintList.GetName(zone.Hand, zone.HandPart);
            }

            return allPrintZones;
        }
    }
}