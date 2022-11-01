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
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Language;

    internal class CaptureOrder
    {
        private List<PrintInfo> captureList = new List<PrintInfo>();

        private readonly PrintList printList;

        private readonly ILivescanDevice device;

        private readonly PrintRules rules;

        public CaptureOrder(PrintList printList, ILivescanDevice device, bool resumeCaptureMode)
        {
            this.printList = printList;
            this.device = device;
            this.rules = this.printList.Rules;
            this.IsResumeCaptureMode = resumeCaptureMode;
        }

        public bool IsResumeCaptureMode { get; private set; }

        public List<PrintInfo> GetCaptureList()
        {            
            this.captureList = new List<PrintInfo>();            
            this.CreateCaptureList();

            return this.captureList;
        }

        //public List<PrintInfo> GetQuickFlatCaptureList(bool includeEndorsement)
        //{
        //    this.captureList = new List<PrintInfo>();

        //    if (rules.CaptureGroup == PrintCaptureGroup.OneFingerOnly)
        //    {
        //        this.AddToCaptureList(Hand.Right, HandScanKind.Flat, PrintResolution.Dpi500, HandPart.Thumb);
        //    }
        //    else
        //    {
        //        this.AddToCaptureList(Hand.Left, HandScanKind.Flat, device.FingerResolution, HandPart.FourFlats);
        //        this.AddToCaptureList(Hand.Right, HandScanKind.Flat, device.FingerResolution, HandPart.FourFlats);
        //        this.AddToCaptureList(Hand.None, HandScanKind.Flat, device.FingerResolution, HandPart.TwoThumbs);

        //        if (includeEndorsement)
        //        {
        //            this.AddToCaptureList(Hand.None, HandScanKind.Flat, device.FingerResolution, HandPart.Endorsement);
        //            this.SetEndorsement();
        //        }
        //    }

            

        //    return this.captureList;
        //}

        public List<PrintInfo> GetCaptureList(PrintInfo print, PrintResolution resolution)
        {
            this.captureList = new List<PrintInfo>();
            this.AddToCaptureList(print.Hand, print.ScanKind, resolution, new[] { print.HandPart });

            return this.captureList;
        }

        


        private void CreateCaptureList()
        {
            this.captureList.Clear();
            if (this.printList.Rules.OrderMode == CaptureOrderMode.Sq)
            {
                this.CreateSqCaptureList();
            }
            else
            {
                this.CreateStandardCaptureList();
            }
            
        }

        private void CreateSqCaptureList()
        {
            if (rules.CaptureGroup == PrintCaptureGroup.OneFingerOnly)
            {
                if (!device.Supports(DeviceScanKind.FlatSingleFinger))
                {
                    throw new ApplicationException(CommonText.ScannerNotSupportFlat);
                }

                this.AddToCaptureList(Hand.Right, HandScanKind.Flat, PrintResolution.Dpi500, HandPart.Thumb);
                return;
            }

            // 1 --> two flat thumbs
            if (rules.CaptureTwoThumbs && device.Supports(DeviceScanKind.FlatTwoFinger))
            {
                this.AddToCaptureList(Hand.None, HandScanKind.Flat, device.FingerResolution, HandPart.TwoThumbs);
            }
            else
            {
                this.AddToCaptureList(Hand.Left, HandScanKind.Flat, device.FingerResolution, HandPart.Thumb);
                this.AddToCaptureList(Hand.Right, HandScanKind.Flat, device.FingerResolution, HandPart.Thumb);
            }

            // 2 --> 4 right fingers flat
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

            // 3 to 7 --> Rolled fingers right
            if (rules.CaptureGroup != PrintCaptureGroup.FlatOnly)
            {
                if (!device.Supports(DeviceScanKind.RolledSingleFinger))
                {
                    throw new ApplicationException(CommonText.ScannerNotSupportRolled);
                }
                this.AddToCaptureList(Hand.Right, HandScanKind.Rolled, device.FingerResolution, HandPart.Thumb, HandPart.Index, HandPart.Middle, HandPart.Ring, HandPart.Little);
            }

            // 8, 9, 10 --> right upper palm, lower palm, hypothenar palm

            if (rules.CaptureGroup == PrintCaptureGroup.StandardAndPalm)
            {
                if (device.Supports(DeviceScanKind.FlatPartialPalm))
                {
                    this.AddToCaptureList(Hand.Right, HandScanKind.Flat, device.PalmResolution, HandPart.UpperPalm, HandPart.LowerPalm, HandPart.Hypothenar);
                }
            }

            // 11 --> 4 fingers left
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

            // 12-16 --> Left fingers rolled
            if (rules.CaptureGroup != PrintCaptureGroup.FlatOnly)
            {
                if (!device.Supports(DeviceScanKind.RolledSingleFinger))
                {
                    throw new ApplicationException(CommonText.ScannerNotSupportRolled);
                }
                this.AddToCaptureList(Hand.Left, HandScanKind.Rolled, device.FingerResolution, HandPart.Thumb, HandPart.Index, HandPart.Middle, HandPart.Ring, HandPart.Little);
            }

            // 17, 18, 19 --> Left Upper palm, Lower palm, hypothenar

            if (rules.CaptureGroup == PrintCaptureGroup.StandardAndPalm)
            {
                if (device.Supports(DeviceScanKind.FlatPartialPalm))
                {
                    this.AddToCaptureList(Hand.Left, HandScanKind.Flat, device.PalmResolution, HandPart.UpperPalm, HandPart.LowerPalm, HandPart.Hypothenar);
                }
            }

            // 20 --> Endorsement finger
            if (rules.IsEndorsementAllowed)
            {
                this.AddToCaptureList(Hand.None, HandScanKind.Flat, device.FingerResolution, HandPart.Endorsement);
                this.SetEndorsement();
            }
        }

        private void CreateStandardCaptureList()
        {
            if (rules.CaptureGroup == PrintCaptureGroup.OneFingerOnly)
            {
                if (!device.Supports(DeviceScanKind.FlatSingleFinger))
                {
                    throw new ApplicationException(CommonText.ScannerNotSupportFlat);
                }

                this.AddToCaptureList(Hand.Right, HandScanKind.Flat, PrintResolution.Dpi500, HandPart.Thumb);
                return;
            }

            if (rules.CaptureGroup == PrintCaptureGroup.StandardAndPalm)
            {
                if (device.Supports(DeviceScanKind.FlatPartialPalm))
                {
                    this.AddToCaptureList(Hand.Left, HandScanKind.Flat, device.PalmResolution, HandPart.UpperPalm, HandPart.LowerPalm, HandPart.Hypothenar);
                    this.AddToCaptureList(Hand.Right, HandScanKind.Flat, device.PalmResolution, HandPart.UpperPalm, HandPart.LowerPalm, HandPart.Hypothenar);
                }
                else if (device.Supports(DeviceScanKind.FlatCompletePalm))
                {
                    this.AddToCaptureList(Hand.Left, HandScanKind.Flat, device.PalmResolution, HandPart.CompletePalm, HandPart.Hypothenar);
                    this.AddToCaptureList(Hand.Right, HandScanKind.Flat, device.PalmResolution, HandPart.CompletePalm, HandPart.Hypothenar);
                }
                else
                {
                    if (device.Supports(DeviceScanKind.FlatSingleFinger))
                    {
                        throw new ApplicationException(CommonText.ScannerNotSupporPalm);
                    }
                }
            }

            if (!device.Supports(DeviceScanKind.FlatSingleFinger))
            {
                throw new ApplicationException(CommonText.ScannerNotSupportFlat);
            }

            if (device.Supports(DeviceScanKind.FlatFourFinger))
            {
                this.AddToCaptureList(Hand.Left, HandScanKind.Flat, device.FingerResolution, HandPart.FourFlats);
                this.AddToCaptureList(Hand.Right, HandScanKind.Flat, device.FingerResolution, HandPart.FourFlats);
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
                this.AddToCaptureList(
                    Hand.Right,
                    HandScanKind.Flat,
                    device.FingerResolution,
                    HandPart.Index,
                    HandPart.Middle,
                    HandPart.Ring,
                    HandPart.Little);
            }

            if (rules.CaptureTwoThumbs && device.Supports(DeviceScanKind.FlatTwoFinger))
            {
                this.AddToCaptureList(Hand.None, HandScanKind.Flat, device.FingerResolution, HandPart.TwoThumbs);
            }
            else
            {
                this.AddToCaptureList(Hand.Left, HandScanKind.Flat, device.FingerResolution, HandPart.Thumb);
                this.AddToCaptureList(Hand.Right, HandScanKind.Flat, device.FingerResolution, HandPart.Thumb);
            }

            if (rules.CaptureGroup != PrintCaptureGroup.FlatOnly)
            {
                if (!device.Supports(DeviceScanKind.RolledSingleFinger))
                {
                    throw new ApplicationException(CommonText.ScannerNotSupportRolled);
                }

                this.AddToCaptureList(Hand.Left, HandScanKind.Rolled, device.FingerResolution, HandPart.Index, HandPart.Middle, HandPart.Ring, HandPart.Little);
                this.AddToCaptureList(Hand.Right, HandScanKind.Rolled, device.FingerResolution, HandPart.Index, HandPart.Middle, HandPart.Ring, HandPart.Little);

                this.AddToCaptureList(Hand.Left, HandScanKind.Rolled, device.FingerResolution, HandPart.Thumb);
                this.AddToCaptureList(Hand.Right, HandScanKind.Rolled, device.FingerResolution, HandPart.Thumb);
            }

            if (rules.IsEndorsementAllowed)
            {
                this.AddToCaptureList(Hand.None, HandScanKind.Flat, device.FingerResolution, HandPart.Endorsement);
                this.SetEndorsement();
            }
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

    }
}
