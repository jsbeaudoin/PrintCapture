using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.Device.LivescanCrossmatchEssential.Plugin
{
    using Livescan.Scanners.DriverEssential.Sdk;

    using PrintsCapture.Device.Enum;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;

    internal class TftManager
    {
        private readonly int deviceHandle;

        private Dictionary<string, int> printToDisplayScreenIndex;
        private readonly List<PhysicalHandPart> handParts;
        private enumLScanDisplayObjectColor[] qualityColors = null;
        private bool supportsTft;

        public TftManager(int deviceHandle, List<PhysicalHandPart> handParts)
        {
            this.deviceHandle = deviceHandle;
            this.handParts = handParts;

            this.printToDisplayScreenIndex = new Dictionary<string, int>();

            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Left, HandPart.UpperPalm), 3);
            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Left, HandPart.Hypothenar), 1);
            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Left, HandPart.LowerPalm), 0);
            //this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Left, HandPart.CompletePalm), 2);

            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Left, HandPart.Thumb), 4);
            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Left, HandPart.Index), 5);
            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Left, HandPart.Middle), 6);
            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Left, HandPart.Ring), 7);
            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Left, HandPart.Little), 8);


            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Right, HandPart.UpperPalm), 12);
            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Right, HandPart.Hypothenar), 10);
            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Right, HandPart.LowerPalm), 9);
            //this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Right, HandPart.CompletePalm), 11);

            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Right, HandPart.Thumb), 13);
            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Right, HandPart.Index), 14);
            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Right, HandPart.Middle), 15);
            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Right, HandPart.Ring), 16);
            this.printToDisplayScreenIndex.Add(PhysicalHandPart.GetPhysicalKey(Hand.Right, HandPart.Little), 17);
            this.supportsTft = true;
            //(LSE_SDK.LSCAN_Controls_DisplayShowLogoScreen(
            //    this.deviceHandle,
            //    enumLScanDisplayLogoOption.LSCAN_DISPLAY_LOGO_OPTION_LEAVE_UNCHANGED,
            //    0) == LSE_ErrorCode.LSCAN_STATUS_OK);
            
    }

        


        //this.displayManage.InitForCapture(scan.Part, scan.Hand, scan.Kind);
        public void InitForCapture(HandPart part, Hand hand, HandScanKind scanKind)
        {
            if (!this.supportsTft)
            {
                return;
            }

            var leftButton = enumLScanDisplayCommonCtrl.LSCAN_DISPLAY_COMMON_CTRL_ERASE;

            var rightButton = enumLScanDisplayCommonCtrl.LSCAN_DISPLAY_COMMON_CTRL_ERASE;

            var topButton = scanKind == HandScanKind.Rolled
                ? enumLScanDisplayStatTop.LSCAN_DISPLAY_STAT_TOP_ROLL_HORIZONTAL
                : enumLScanDisplayStatTop.LSCAN_DISPLAY_STAT_TOP_CAPTURE_FLAT;

            var bottomButton = enumLScanDisplayStatBottom.LSCAN_DISPLAY_STAT_BOTTOM_ERASE;

            //this.displayManage.InitForCapture(scan.Part, scan.Hand, scan.Kind);

            var displayColors = new enumLScanDisplayObjectColor[18];


            for (int index = 0; index < displayColors.Length; index++)
            {
                displayColors[index] = enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_INACTIVE;
            }

            foreach (var handPart in this.handParts)
            {
                enumLScanDisplayObjectColor color = enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_INACTIVE;
                if (handPart.IsMissing)
                {
                    color = enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_MISSING;
                }
                else if (hand == handPart.Hand && part == handPart.HandPart)
                {
                    color = enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_CURRENT_SELECTION;
                }

                this.SetDisplayForHandPart(handPart.HandPart, handPart.Hand, color, displayColors);
            }

            LSE_SDK.LSCAN_Controls_DisplayShowCaptureProgressScreen(
                this.deviceHandle,
                leftButton,
                rightButton,
                topButton,
                bottomButton,
                displayColors[0],
                displayColors[1],
                displayColors[2],
                displayColors[3],
                displayColors[4],
                displayColors[5],
                displayColors[6],
                displayColors[7],
                displayColors[8],
                displayColors[9],
                displayColors[10],
                displayColors[11],
                displayColors[12],
                displayColors[13],
                displayColors[14],
                displayColors[15],
                displayColors[16],
                displayColors[17]);
        }


        public void DisplayWait()
        {
            if (!this.supportsTft)
            {
                return;
            }

            LSE_SDK.LSCAN_Controls_DisplayShowCaptureProgressScreen(
                this.deviceHandle,
                enumLScanDisplayCommonCtrl.LSCAN_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED,
                enumLScanDisplayCommonCtrl.LSCAN_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED,
                enumLScanDisplayStatTop.LSCAN_DISPLAY_STAT_TOP_ERASE,
                enumLScanDisplayStatBottom.LSCAN_DISPLAY_STAT_BOTTOM_HOURGLASS_ANIMATED,
                enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED, enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED,
                enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED, enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED,
                enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED, enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED,
                enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED, enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED,
                enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED, enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED,
                enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED, enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED,
                enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED, enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED,
                enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED, enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED,
                enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED, enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED
                );
        }

        public void DefineQuality(HandPart part, Hand hand, PrintQualityLevel quality)
        {
            if (!this.supportsTft)
            {
                return;
            }

            enumLScanDisplayObjectColor color = enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED;

            switch (quality)
            {
                case PrintQualityLevel.Bad:
                    color = enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_RESTRICTED;
                    break;
                case PrintQualityLevel.NotGoodEnough:
                    color = enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_ACTIVE;
                    break;
                case PrintQualityLevel.Good:
                    color = enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                    break;
                case PrintQualityLevel.NotPresent:
                    color = enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_ACTIVE;
                    break;

            }

            if (this.qualityColors == null)
            {
                this.qualityColors = new enumLScanDisplayObjectColor[18];

                for (int index = 0; index < this.qualityColors.Length; index++)
                {
                    this.qualityColors[index] = enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_LEAVE_UNCHANGED;
                }
            }

            this.SetDisplayForHandPart(part, hand, color, this.qualityColors);
        }

        public void DisplayQuality()
        {
            if (!this.supportsTft || this.qualityColors == null)
            {
                return;
            }            

            LSE_SDK.LSCAN_Controls_DisplayShowCaptureProgressScreen(
                this.deviceHandle,
                enumLScanDisplayCommonCtrl.LSCAN_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED,
                enumLScanDisplayCommonCtrl.LSCAN_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED,
                enumLScanDisplayStatTop.LSCAN_DISPLAY_STAT_TOP_LEAVE_UNCHANGED, 
                enumLScanDisplayStatBottom.LSCAN_DISPLAY_STAT_BOTTOM_LEAVE_UNCHANGED,
                this.qualityColors[0],
                this.qualityColors[1],
                this.qualityColors[2],
                this.qualityColors[3],
                this.qualityColors[4],
                this.qualityColors[5],
                this.qualityColors[6],
                this.qualityColors[7],
                this.qualityColors[8],
                this.qualityColors[9],
                this.qualityColors[10],
                this.qualityColors[11],
                this.qualityColors[12],
                this.qualityColors[13],
                this.qualityColors[14],
                this.qualityColors[15],
                this.qualityColors[16],
                this.qualityColors[17]);

            this.qualityColors = null;
        }

        /// <summary>
        /// Set status for prints when different from LSCAN_DISPLAY_OBJECT_INACTIVE
        /// </summary>
        /// <param name="part"></param>
        /// <param name="hand"></param>
        /// <param name="color"></param>
        /// <param name="list"></param>
        void SetDisplayForHandPart(HandPart part, Hand hand, enumLScanDisplayObjectColor color, enumLScanDisplayObjectColor[] list)
        {
            var processIndividual = true;

            if (part == HandPart.FourFlats || part == HandPart.UpperPalm || part == HandPart.CompletePalm)
            {
                this.SetDisplayForHandPart(HandPart.Index, hand, color, list);
                this.SetDisplayForHandPart(HandPart.Middle, hand, color, list);
                this.SetDisplayForHandPart(HandPart.Ring, hand, color, list);
                this.SetDisplayForHandPart(HandPart.Little, hand, color, list);
                processIndividual = part == HandPart.UpperPalm || part == HandPart.CompletePalm;
            }
            else if (part == HandPart.TwoThumbs)
            {
                processIndividual = false;
                this.SetDisplayForHandPart(HandPart.Thumb, Hand.Left, color, list);
                this.SetDisplayForHandPart(HandPart.Thumb, Hand.Right, color, list);
            }

            if (processIndividual)
            {
                var key = PhysicalHandPart.GetPhysicalKey(hand, part);
                if (!this.printToDisplayScreenIndex.ContainsKey(key))
                {
                    return;
                }
                var index = this.printToDisplayScreenIndex[key];
                if (color != enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_INACTIVE && list[index] != enumLScanDisplayObjectColor.LSCAN_DISPLAY_OBJECT_MISSING)
                {
                    list[index] = color;
                }
            }
        }

        

    }
}
