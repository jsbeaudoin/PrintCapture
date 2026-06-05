// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CaptureOrder.cs" company="Solutions XL-ID inc">
//   update text
// </copyright>
// <summary>
//   Defines the CaptureOrder type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PrintsCapture.Livescan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Extension;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Class.Custom;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Language;
    using XL_ID.Utilities.Log;

    internal class CaptureOrder
    {
        private List<PrintInfo> captureList = new List<PrintInfo>();

        private readonly PrintList printList;

        private readonly ILivescanDevice device;

        private readonly PrintRules rules;

        private CustomPrintOrderManager customManager;

        public CaptureOrder(PrintList printList, ILivescanDevice device, bool resumeCaptureMode)
        {
            this.printList = printList;
            this.device = device;
            this.rules = this.printList.Rules;
            this.IsResumeCaptureMode = resumeCaptureMode;
            this.customManager = new CustomPrintOrderManager(this.rules);
        }

        public bool IsResumeCaptureMode { get; private set; }

        public List<PrintInfo> GetCaptureList()
        {            
            this.captureList = new List<PrintInfo>();
            this.CreateCaptureList();

            return this.captureList;
        }

        public List<PrintInfo> GetCaptureList(PrintInfo print, PrintResolution resolution)
        {
            this.captureList = new List<PrintInfo>();
            this.AddToCaptureList(print.Hand, print.ScanKind, resolution, new[] { print.HandPart });

            return this.captureList;
        }

        private void CreateCaptureList()
        {
            this.captureList.Clear();
            var order = this.printList.Rules.OrderMode;
            
            if (customManager.CanBeUsed && customManager.IsCustomFileValid())
            {
                order = CaptureOrderMode.Custom;
            }

            switch (order) {
                case CaptureOrderMode.Standard:
                    this.CreateStandardCaptureList();
                    break;
                case CaptureOrderMode.Sq:
                    this.CreateSqCaptureList();
                    break;
                case CaptureOrderMode.Custom:
                    this.CreateCustomCaptureList();
                    break;
            }
        }

        private void CreateCustomCaptureList()
        {
            if (rules.CaptureGroup == PrintCaptureGroup.OneFingerOnly)
            {
                AddOneFingerScan();
                return;
            }

            Dictionary<String, Action> scanActions = new Dictionary<string, Action>();

            scanActions.Add("E", () => AddEndorsementScan());

            scanActions.Add("1", () => AddRolledScan(Hand.Right, HandPart.Thumb));
            scanActions.Add("2", () => AddRolledScan(Hand.Right, HandPart.Index));
            scanActions.Add("3", () => AddRolledScan(Hand.Right, HandPart.Middle));
            scanActions.Add("4", () => AddRolledScan(Hand.Right, HandPart.Ring));
            scanActions.Add("5", () => AddRolledScan(Hand.Right, HandPart.Little));

            scanActions.Add("6", () => AddRolledScan(Hand.Left, HandPart.Thumb));
            scanActions.Add("7", () => AddRolledScan(Hand.Left, HandPart.Index));
            scanActions.Add("8", () => AddRolledScan(Hand.Left, HandPart.Middle));
            scanActions.Add("9", () => AddRolledScan(Hand.Left, HandPart.Ring));
            scanActions.Add("10", () => AddRolledScan(Hand.Left, HandPart.Little));

            scanActions.Add("11", () => AddThumbFlatScan(Hand.Right));
            scanActions.Add("12", () => AddThumbFlatScan(Hand.Left));

            scanActions.Add("13", () => AddFourFlatRightScan());
            scanActions.Add("14", () => AddFourFlatLeftScan());
            
            // avoid 15 for the moment. as it will be repalced by individual scan of the 2 flat thumbs
            // Only Sq Mode is allowed to capture and split the 2 thumbs
            scanActions.Add("15", () => AddTwoFlatThumbsScan()); 

            scanActions.Add("22", () => AddPalmScan(Hand.Right, HandPart.Hypothenar));
            scanActions.Add("24", () => AddPalmScan(Hand.Left, HandPart.Hypothenar));
            scanActions.Add("25", () => AddPalmScan(Hand.Right, HandPart.LowerPalm));
            scanActions.Add("26", () => AddPalmScan(Hand.Right, HandPart.UpperPalm));
            scanActions.Add("27", () => AddPalmScan(Hand.Left, HandPart.LowerPalm));
            scanActions.Add("28", () => AddPalmScan(Hand.Left, HandPart.UpperPalm));

            try
            {
                foreach (var print in customManager.PrintList)
                {
                    scanActions[print.Key]();
                }
            } catch (Exception ex)
            {
                LogDispatcher.DoLog("Could not load Custom order file. Using standard list instead.", LogEventLevel.Warning, ex);
                // failed setting order !!! use standard order !!!
                this.captureList.Clear();
                this.CreateStandardCaptureList();
            }
            LogDispatcher.DoLog("Using custom print capture order file");

        }

        private void CreateSqCaptureList()
        {
            if (rules.CaptureGroup == PrintCaptureGroup.OneFingerOnly)
            {
                AddOneFingerScan();
                return;
            }

            // 1 --> two flat thumbs
            AddTwoFlatThumbsScan();

            // 2 --> 4 right fingers flat
            AddFourFlatRightScan();

            // 3 to 7 --> Rolled fingers right
            AddRolledScan(Hand.Right, HandPart.Thumb);
            AddRolledScan(Hand.Right, HandPart.Index);
            AddRolledScan(Hand.Right, HandPart.Middle);
            AddRolledScan(Hand.Right, HandPart.Ring);
            AddRolledScan(Hand.Right, HandPart.Little);

            // 8, 9, 10 --> right upper palm, lower palm, hypothenar palm
            AddPalmScan(Hand.Right, HandPart.UpperPalm);
            AddPalmScan(Hand.Right, HandPart.LowerPalm);
            AddPalmScan(Hand.Right, HandPart.Hypothenar);

            // 11 --> 4 fingers left
            AddFourFlatLeftScan();

            // 12-16 --> Left fingers rolled
            AddRolledScan(Hand.Left, HandPart.Thumb);
            AddRolledScan(Hand.Left, HandPart.Index);
            AddRolledScan(Hand.Left, HandPart.Middle);
            AddRolledScan(Hand.Left, HandPart.Ring);
            AddRolledScan(Hand.Left, HandPart.Little);

            // 17, 18, 19 --> Left Upper palm, Lower palm, hypothenar
            AddPalmScan(Hand.Left, HandPart.UpperPalm);
            AddPalmScan(Hand.Left, HandPart.LowerPalm);
            AddPalmScan(Hand.Left, HandPart.Hypothenar);

            // 20 --> Endorsement finger
            AddEndorsementScan();
        }

        private void CreateStandardCaptureList()
        {
            if (rules.CaptureGroup == PrintCaptureGroup.OneFingerOnly)
            {
                AddOneFingerScan();
                return;
            }

            AddPalmScan(Hand.Left, HandPart.UpperPalm);
            AddPalmScan(Hand.Left, HandPart.LowerPalm);
            AddPalmScan(Hand.Left, HandPart.Hypothenar);

            AddPalmScan(Hand.Right, HandPart.UpperPalm);
            AddPalmScan(Hand.Right, HandPart.LowerPalm);
            AddPalmScan(Hand.Right, HandPart.Hypothenar);

            AddFourFlatLeftScan();
            AddFourFlatRightScan();
            AddTwoFlatThumbsScan();

            AddRolledScan(Hand.Left, HandPart.Index);
            AddRolledScan(Hand.Left, HandPart.Middle);
            AddRolledScan(Hand.Left, HandPart.Ring);
            AddRolledScan(Hand.Left, HandPart.Little);

            AddRolledScan(Hand.Right, HandPart.Index);
            AddRolledScan(Hand.Right, HandPart.Middle);
            AddRolledScan(Hand.Right, HandPart.Ring);
            AddRolledScan(Hand.Right, HandPart.Little);

            AddRolledScan(Hand.Left, HandPart.Thumb);
            AddRolledScan(Hand.Right, HandPart.Thumb);


            AddEndorsementScan();
        }

        private void SetEndorsement()
        {
            var endorsement = this.captureList.FirstOrDefault(x => x.HandPart == HandPart.Endorsement);
            if (endorsement == null)
            {
                return;
            }

            var endorsable = this.printList.GetEndorsableFingers().First();
            if (endorsable == null)
            {
                return;
            }

            endorsement.EndorsementFinger =
                this.printList.PhysicalParts.Single(
                    x => x.EndorsementIndex == endorsable.Index);
        }

        private void AddToCaptureList(Hand hand, HandScanKind scanKind, PrintResolution resolution, params HandPart[] parts)
        {            
            var baseList = printList.Prints.Where(x => x.Hand == hand && x.ScanKind == scanKind).ToList();

            foreach (var handPart in parts)
            {
                HandPart part = handPart;
                var print = baseList.Single(x => x.HandPart == part);


                if (print.IsMissing || (this.IsResumeCaptureMode && print.Status == PrintStatus.Validated))
                {
                    if (print.UserAction != WizardAction.ScanAgain)
                    {
                        // skip missing prints && ok prints (on resume) !
                        continue;
                    }                    
                }

                if ((print.UserAction & WizardAction.OverrideCodeMask) > 0)
                {
                    // print overriden by wizard are not to be scanned again !
                    continue;
                }

                print.Resolution = resolution;
                
                this.captureList.Add(print);                
            }
        }

        #region "Finger Capture methods"


        private void AddOneFingerScan()
        {
            if (!device.Supports(DeviceScanKind.FlatSingleFinger))
            {
                throw new ApplicationException(CommonText.ScannerNotSupportFlat);
            }

            this.AddToCaptureList(Hand.Right, HandScanKind.Flat, PrintResolution.Dpi500, HandPart.Thumb);
        }

        private void AddTwoFlatThumbsScan()
        {
            if (rules.CaptureTwoThumbs && device.Supports(DeviceScanKind.FlatTwoFinger))
            {
                this.AddToCaptureList(Hand.None, HandScanKind.Flat, device.FingerResolution, HandPart.TwoThumbs);
            }
            else
            {
                this.AddToCaptureList(Hand.Left, HandScanKind.Flat, device.FingerResolution, HandPart.Thumb);
                this.AddToCaptureList(Hand.Right, HandScanKind.Flat, device.FingerResolution, HandPart.Thumb);
            }
        }

        private void AddFourFlatRightScan()
        {
            if (device.Supports(DeviceScanKind.FlatFourFinger))
            {
                this.AddToCaptureList(Hand.Right, HandScanKind.Flat, device.FingerResolution, HandPart.FourFlats);
            }
            else
            {
                this.AddToCaptureList(
                    Hand.Right,
                    HandScanKind.Flat,
                    device.FingerResolution,
                    HandPart.Index,
                    HandPart.Middle,
                    HandPart.Ring,
                    HandPart.Little);
            }
        }

        private void AddFourFlatLeftScan()
        {
            if (device.Supports(DeviceScanKind.FlatFourFinger))
            {
                this.AddToCaptureList(Hand.Left, HandScanKind.Flat, device.FingerResolution, HandPart.FourFlats);
            }
            else
            {
                this.AddToCaptureList(
                    Hand.Left,
                    HandScanKind.Flat,
                    device.FingerResolution,
                    HandPart.Index,
                    HandPart.Middle,
                    HandPart.Ring,
                    HandPart.Little);
            }
        }

        private void AddThumbFlatScan(Hand hand)
        {
            this.AddToCaptureList(hand, HandScanKind.Flat, device.FingerResolution, HandPart.Thumb);
        }

        private void AddPalmScan(Hand hand, HandPart palmPart)
        {
            if (rules.CaptureGroup == PrintCaptureGroup.StandardAndPalm)
            {
                if (device.Supports(DeviceScanKind.FlatPartialPalm))
                {
                    this.AddToCaptureList(hand, HandScanKind.Flat, device.PalmResolution, palmPart);
                }
                    
            }
        }

        private void AddRolledScan(Hand hand, HandPart rolledFinger)
        {
            if (rules.CaptureGroup == PrintCaptureGroup.FlatOnly)
            {
                return;
            }

            if (!device.Supports(DeviceScanKind.RolledSingleFinger))
            {
                throw new ApplicationException(CommonText.ScannerNotSupportRolled);
            }
            this.AddToCaptureList(hand, HandScanKind.Rolled, device.FingerResolution, rolledFinger);
        }

        private void AddEndorsementScan()
        {
            if (rules.IsEndorsementAllowed)
            {
                this.AddToCaptureList(Hand.None, HandScanKind.Flat, device.FingerResolution, HandPart.Endorsement);
                this.SetEndorsement();
            }
        }

            

    #endregion



}
}
