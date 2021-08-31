using System;
using System.Collections.Generic;

namespace PrintsCapture.Cardscan
{
   public static class FixedModels
    {

        public static ScanModel C216()
        {
            var sm = new ScanModel();
            sm.Name = "C216";
            sm.SettingFileName = "C216.xml";
            sm.CreationDateTime = new DateTime(2017,01,01);
            sm.IsStarred = true;
            sm.IsReadOnly = false;

            sm.Zones = new List<ScanPrintZone>();
            sm.Zones.Add(new ScanPrintZone("RightThumb", 0.344, 0.24, 1.528, 1.464) );
            sm.Zones.Add(new ScanPrintZone("RightIndex", 1.888, 0.24, 1.528, 1.464));
            sm.Zones.Add(new ScanPrintZone("RightMiddle", 3.396, 0.24, 1.528, 1.464));
            sm.Zones.Add(new ScanPrintZone("RightRing", 4.96, 0.24, 1.528, 1.464));
            sm.Zones.Add(new ScanPrintZone("RightLittle", 6.504, 0.24, 1.528, 1.464));


            sm.Zones.Add(new ScanPrintZone("LeftThumb", 0.344, 1.744, 1.528, 1.464));
            sm.Zones.Add(new ScanPrintZone("LeftIndex", 1.888, 1.744, 1.528, 1.464));
            sm.Zones.Add(new ScanPrintZone("LeftMiddle", 3.396, 1.744, 1.528, 1.464));
            sm.Zones.Add(new ScanPrintZone("LeftRing", 4.96, 1.7444, 1.528, 1.464));
            sm.Zones.Add(new ScanPrintZone("LeftLittle", 6.504, 1.7444, 1.528, 1.464));

            sm.Zones.Add(new ScanPrintZone("LeftFourFlat", 0.16, 3.24, 3.2, 2));
            sm.Zones.Add(new ScanPrintZone("LeftThumbFlat", 3.384, 3.24, 0.8, 2));
            sm.Zones.Add(new ScanPrintZone("RightThumbFlat", 4.232, 3.24, 0.8, 2));
            sm.Zones.Add(new ScanPrintZone("RightFourFlat", 5.048, 3.24, 3.2, 2));
            return sm;
        }

        public static ScanModel FD258()
        {
            var sm = new ScanModel();
            sm.Name = "FD258";
            sm.SettingFileName = "FD258.xml";
            sm.CreationDateTime = new DateTime(2017, 01, 01);
            sm.IsStarred = true;
            sm.IsReadOnly = false;

            sm.Zones = new List<ScanPrintZone>();
            sm.Zones.Add(new ScanPrintZone("RightThumb", 0, 0.26, 1.6, 1.5));
            sm.Zones.Add(new ScanPrintZone("RightIndex", 1.60, 0.26, 1.6, 1.5));
            sm.Zones.Add(new ScanPrintZone("RightMiddle", 3.24, 0.26, 1.6, 1.5));
            sm.Zones.Add(new ScanPrintZone("RightRing", 4.89, 0.26, 1.6, 1.5));
            sm.Zones.Add(new ScanPrintZone("RightLittle", 6.54, 0.26, 1.6, 1.5));


            sm.Zones.Add(new ScanPrintZone("LeftThumb", 0, 1.8, 1.6, 1.5));
            sm.Zones.Add(new ScanPrintZone("LeftIndex", 1.6, 1.8, 1.6, 1.5));
            sm.Zones.Add(new ScanPrintZone("LeftMiddle", 3.24, 1.8, 1.6, 1.54));
            sm.Zones.Add(new ScanPrintZone("LeftRing", 4.89, 1.8, 1.6, 1.5));
            sm.Zones.Add(new ScanPrintZone("LeftLittle", 6.54, 1.8, 1.6, 1.5));

            sm.Zones.Add(new ScanPrintZone("LeftFourFlat", 0, 3.325, 3.2, 2));
            sm.Zones.Add(new ScanPrintZone("LeftThumbFlat", 3.23, 3.325, 0.8, 2));
            sm.Zones.Add(new ScanPrintZone("RightThumbFlat", 4.03, 3.325, 0.8, 2));
            sm.Zones.Add(new ScanPrintZone("RightFourFlat", 4.86, 3.325, 3.2, 2));
            return sm;
        }
    }
}
