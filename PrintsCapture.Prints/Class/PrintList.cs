using System.Runtime.CompilerServices;

namespace PrintsCapture.Prints
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Extension;
    using PrintsCapture.Prints.Language;
    using PrintsCapture.Prints.ViewModel;

  

    /// <summary>
    /// The print list.
    /// </summary>
    public class PrintList
    {
        public const int EndorsementFingerIndex = 16;

        List<PhysicalHandPart> physicalHandPart;
        List<PrintInfo> prints;

        ReadOnlyCollection<PhysicalHandPart> readOnlyPhysicalHandPart;
        ReadOnlyCollection<PrintInfo> readOnlyPrints;

        private readonly ReadOnlyCollection<PrintCondition> printConditions;

        private readonly Dictionary<PrintStatus, string> statusMessages = new Dictionary<PrintStatus, string>();
        private readonly Dictionary<string, string> missingTexts = new Dictionary<string, string>();
        private readonly Dictionary<PrintError, string> validationMessages = new Dictionary<PrintError, string>();        
        private readonly Dictionary<string, ImageSourceLookup> bitmapLookups = new Dictionary<string, ImageSourceLookup>();

        private readonly Dictionary<PrintStatus, BitmapImage> statusImages = new Dictionary<PrintStatus, BitmapImage>();

        private BitmapImage blankPrint;

        public PrintList()
        {
            this.CreateList();
            // TODO : Handle missing only by the printCondition
            this.missingTexts.Add("XX", CommonText.HandPartAmputated);
            this.missingTexts.Add("PL", CommonText.HandPartPhysicalLimitation);
            this.missingTexts.Add("UP", CommonText.HandPartBandaged);
            this.missingTexts.Add("FR", CommonText.HandPartForeignReason);
            this.missingTexts.Add("MI", CommonText.HandPartMissingImage);

            var conditions = new List<PrintCondition>();
            conditions.Add(new PrintCondition("", HandPartStatus.Present, CommonText.HandPartPresent));
            conditions.Add(new PrintCondition("XX", HandPartStatus.Amputated, CommonText.HandPartAmputated));
            conditions.Add(new PrintCondition("PL", HandPartStatus.PhysicalLimitation, CommonText.HandPartPhysicalLimitation));
            conditions.Add(new PrintCondition("UP", HandPartStatus.Bandaged, CommonText.HandPartBandaged));
            conditions.Add(new PrintCondition("FR", HandPartStatus.ForeignReason, CommonText.HandPartForeignReason));
            conditions.Add(new PrintCondition("MI", HandPartStatus.Missing, CommonText.HandPartMissingImage));
            this.printConditions = new ReadOnlyCollection<PrintCondition>(conditions);

            this.statusMessages.Add(PrintStatus.InError, CommonText.PrintStatusInError);
            this.statusMessages.Add(PrintStatus.Missing, CommonText.PrintStatusMissing);
            this.statusMessages.Add(PrintStatus.Ok, CommonText.PrintsStatusOk);
            this.statusMessages.Add(PrintStatus.Overriden, CommonText.PrintStatusOverriden);
            this.statusMessages.Add(PrintStatus.PendingEvaluation, CommonText.PrintStatusPendingEvaluation);
            this.statusMessages.Add(PrintStatus.Submited, CommonText.PrintStatusSubmited);
            this.statusMessages.Add(PrintStatus.Unknown, CommonText.PrintStatusUnknown);
            this.statusMessages.Add(PrintStatus.Validated, CommonText.PrintsStatusOk);
            this.statusMessages.Add(PrintStatus.Empty, CommonText.NoPrint);
            this.statusMessages.Add(PrintStatus.ValidatedSwapped, CommonText.PrintStatusValidatedSwapped);
            this.statusMessages.Add(PrintStatus.InErrorSwapped, CommonText.PrintStatusInErrorSwapped);

            this.validationMessages.Add(PrintError.BadQuality, CommonText.PrintStatusQualityError);
            this.validationMessages.Add(PrintError.BadSegmentQuality, CommonText.PrintStatusSegmentQualityError);
            this.validationMessages.Add(PrintError.Double, CommonText.PrintStatusDouble);
            this.validationMessages.Add(PrintError.SequenceError, CommonText.PrintStatusSequenceError);
            this.validationMessages.Add(PrintError.ServiceError, CommonText.PrintStatusServiceError);
            this.validationMessages.Add(PrintError.TemplateError, CommonText.PrintStatusTemplateError);
            this.validationMessages.Add(PrintError.Undefined, CommonText.PrintStatusUnknownError);
            this.validationMessages.Add(PrintError.InvalidPrintSegment, CommonText.SegmentUndefinedPrintInvalid);

            
            //this.validationMessages.Add(PrintError.SwappedSequenceCheckError, CommonText.PrintStatusSequenceSwappedError);

            // Images must be created on the same thread they will be used ...
            Application.Current.Dispatcher.Invoke( new Action(() => {
                this.blankPrint = new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/blank-print.png"));

                this.statusImages.Add(PrintStatus.Empty, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/GreenButton.png")));
                this.statusImages.Add(PrintStatus.InError, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/StatusError.png")));
                this.statusImages.Add(PrintStatus.Missing, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/StatusMissing.png")));
                this.statusImages.Add(PrintStatus.Ok, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/GreenButton.png")));
                this.statusImages.Add(PrintStatus.Overriden, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/StatusModified.png")));
                this.statusImages.Add(PrintStatus.PendingEvaluation, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/OrangeButton.png")));
                this.statusImages.Add(PrintStatus.Submited, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/OrangeButton.png")));
                this.statusImages.Add(PrintStatus.Unknown, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/StatusUnknown.png")));
                this.statusImages.Add(PrintStatus.Validated, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/StatusOk.png")));
                this.statusImages.Add(PrintStatus.ValidatedSwapped, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/swappedGreenButton.png")));
                this.statusImages.Add(PrintStatus.InErrorSwapped, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/swappedRedButton.png")));
            }));
            
        }

        public PrintRules Rules { get; set; }

        public ReadOnlyCollection<PhysicalHandPart> PhysicalParts { get { return this.readOnlyPhysicalHandPart; } }

        public ReadOnlyCollection<PrintInfo> Prints { get { return this.readOnlyPrints; } }

        public ReadOnlyCollection<PrintCondition> Conditions { get { return this.printConditions; } }

        public PrintInfo GetPrint(Hand hand, HandPart part, HandScanKind scan)
        {
            return this.prints.SingleOrDefault(x => x.Hand == hand && x.HandPart == part && x.ScanKind == scan);
        }                

        /// <summary>
        /// Gets the viewModel. Update the underlying values from the value in the printInfo
        /// </summary>
        /// <param name="prn"></param>
        /// <returns></returns>
        public PrintElementViewModel GetViewModel(PrintInfo prn)
        {
            var vm = new PrintElementViewModel(prn);            
            
            this.SetPrintElementStatus(prn);
            vm.SequenceScore = prn.SequenceScore;
            vm.Name = GetName(prn);
            vm.ShortName = PrintList.GetShortName(prn);
            vm.SequenceAnalyzed = prn.SequenceAnalyzed;
            vm.QualityScore = prn.QualityScore;
            vm.MinutiaCount = prn.MinutiaCount;

            vm.MissingText = this.GetMissingText(prn.PhysicalPart.MissingCode, prn.PhysicalPart.MissingDate);                        

            vm.Image = this.GetImage(prn);            

            vm.StatusImage = this.statusImages[prn.Status];
            vm.StatusMessage = this.GetStatusMessage(prn);

            if (prn.HandPart == HandPart.Endorsement && prn.EndorsementFinger != null)
            {
                vm.EndorsementFingerName = GetName(prn.EndorsementFinger.Hand, prn.EndorsementFinger.HandPart);
            }
            else
            {
                vm.EndorsementFingerName = CommonText.NoPrint;
            }
            

            return vm;
        }

        /// <summary>
        /// Gets the viewModel.
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="part"></param>
        /// <param name="scan"></param>
        /// <returns></returns>
        public PrintElementViewModel GetViewModel(Hand hand, HandPart part, HandScanKind scan)
        {
            var prn = this.prints.SingleOrDefault(x => x.Hand == hand && x.HandPart == part && x.ScanKind == scan);

            return prn == null ? null : this.GetViewModel(prn);
        }

        public List<PrintElementViewModel> GetAllViewModels()
        {
            return this.Prints.Select(this.GetViewModel).ToList();
        }

        public List<SegmentInfoViewModel> GetSegmentViewModel(PrintInfo print)
        {
            if (!print.HasSegments)
            {
                return new List<SegmentInfoViewModel>();
            }
            var segments = print.Segments.Select(segment => new SegmentInfoViewModel(print, segment));

            if (print.Hand != Hand.Right)
            {
                segments = segments.OrderByDescending(x => x.Part.EndorsementIndex);
            }

            return segments.ToList();
        }

        /// <summary>
        /// Update the information on the print segments
        /// </summary>
        /// <param name="print"></param>
        /// <param name="printSegments"></param>
        /// <returns>0 - No change, 1 Override change, 2 Expected</returns>
        public List<SegmentChange> UpdatePrintSegments(PrintInfo print, List<SegmentInfoViewModel> printSegments)
        {
            var capturedPrint = this.prints.Single(x => x == print);
            var result = new List<SegmentChange>();
            //var change = 0;

            if (!print.HasSegments || printSegments == null)
            {
                return result;
            }

            foreach (var segment in printSegments)
            {
                var printSeg = capturedPrint.Segments.Single(x => x.Part == segment.Part);
                if (printSeg.OverrideCode != segment.OverrideCode || printSeg.OverrideText != segment.OverrideText)
                {
                    this.GetSegmentChange(result, printSeg).IsOverrideChanged = true;
                    printSeg.OverrideCode = segment.OverrideCode;
                    printSeg.OverrideText = segment.OverrideText;
                }

                if (printSeg.IsExpected != segment.IsExpected)
                {
                    this.GetSegmentChange(result, printSeg).IsExpectedChanged = true;
                    printSeg.IsExpected = segment.IsExpected;
                }                
            }

            if (result.Any(x => x.IsExpectedChanged))
            {
                print.SequenceAnalyzed = false;
            }

            return result;
        }

        public bool IsPrintSwapped(PrintInfo print)
        {
            return print.MatchedNistPosition > 0 && print.MatchedNistPosition != print.NistPosition
                   && this.Rules.IsSequenceChangingPosition;
        }

        public PrintInfo GetMatchedPrint(PrintInfo print)
        {
            var key = this.IsPrintSwapped(print) ? print.MatchedKey : print.Key;

            return this.prints.SingleOrDefault(x => x.Key == key);
        }

        public static string GetName(PhysicalHandPart print)
        {
            if (print == null)
            {
                return CommonText.NoPrint;
            }

            return GetName(print.Hand, print.HandPart);
        }


        public static string GetName(Hand hand, HandPart part)
        {            
            if (part == HandPart.Endorsement)
            {
                return CommonText.EndorsementFinger;
            }

            var handLabel = CommonText.ResourceManager.GetString("enumHand" + hand, CommonText.Culture);
            var partLabel = CommonText.ResourceManager.GetString("enumHandPart" + part, CommonText.Culture);            
            if (hand == Hand.None)
            {
                handLabel = string.Empty;
            }

            return string.Format(CommonText.PhysicalPartNamePattern, partLabel, handLabel);
        }

        public static string GetName(PrintInfo print)
        {
            if (print == null)
            {
                return null;
            }

            if (print.HandPart == HandPart.Endorsement)
            {
                return CommonText.EndorsementFinger;                
            }

            var hand = CommonText.ResourceManager.GetString("enumHand" + print.Hand, CommonText.Culture);
            var part = CommonText.ResourceManager.GetString("enumHandPart" + print.HandPart, CommonText.Culture);
            var scan = CommonText.ResourceManager.GetString("enumHandScanKind" + print.ScanKind, CommonText.Culture);
            if (print.Hand == Hand.None)
            {
                hand = string.Empty;
            }

            return string.Format(CommonText.PrintNamePattern, part, scan, hand);

        }

        public static string GetShortName(PrintInfo print)
        {
            if (print == null)
            {
                return null;
            }

            if (print.HandPart == HandPart.Endorsement)
            {
                return GetShortName(print.EndorsementFinger);                
            }

            return GetShortName(print.PhysicalPart);
        }


        public static string GetShortName(PrintList instance, int nistPosition)
        {
            var originalFinger = instance.PhysicalParts.FirstOrDefault(x => x.EndorsementIndex == nistPosition);

            if (originalFinger != null)
            {
                return GetShortName(originalFinger);
            }
            
            return " ? " + nistPosition.ToString() + " ? ";            
        }

        public static string GetShortName(PhysicalHandPart physical)
        {
            if (physical == null)
            {
                return null;
            }
            var shortname = GetName(physical);                      

            var side = (physical.Hand == Hand.Left ? CommonText.Left : CommonText.Right).ToLowerInvariant();
            //var hand = CommonText.ResourceManager.GetString("enumHand" + physical.Hand, CommonText.Culture);
            var part = CommonText.ResourceManager.GetString("enumHandPart" + physical.HandPart, CommonText.Culture)?.ToLowerInvariant();           

            //if (print.Hand == Hand.None)
            //{
            //    hand = string.Empty;
            //}

            if (physical.HandPart == HandPart.FourFlats)
            {
                shortname = string.Format(CommonText.ShortNamePatternExplicit, part, side);
            }
            else if (physical.HandPart == HandPart.TwoThumbs)
            {
                shortname = part;
            }
            else if (!string.IsNullOrEmpty(part))
            {
                shortname = string.Format(CommonText.ShortNamePattern, part, side);
            }            

            return shortname;

        }

        public static string GetInstruction(PrintInfo print)
        {
            if (print == null)
            {
                return null;
            }

            var physicalPrint = print.PhysicalPart;
            var scanMode = print.ScanKind;
            string prnName = string.Empty;
            string handName = string.Empty, partName = string.Empty, scanKindName = string.Empty;
            string side = string.Empty;

            if (print.IsEndorsement)
            {
                physicalPrint = print.EndorsementFinger;
                scanMode = HandScanKind.Flat;                
            }
            
            side = physicalPrint.Hand == Hand.Left ? CommonText.Left : CommonText.Right;
            handName = CommonText.ResourceManager.GetString("enumHand" + physicalPrint.Hand, CommonText.Culture);
            partName = CommonText.ResourceManager.GetString("enumHandPart" + physicalPrint.HandPart, CommonText.Culture);
            scanKindName = CommonText.ResourceManager.GetString("enumHandScanKind" + scanMode, CommonText.Culture);
            if (physicalPrint.Hand == Hand.None)
            {
                handName = string.Empty;
            }

            prnName = string.Format(CommonText.PrintNamePattern, partName, scanKindName, handName);
            
            
            var details = string.Empty;

            // TODO : Include instruction for palm prints !!

            if (print.IsSlap || print.IsEndorsement)
            {
                switch (print.HandPart)
                {
                    case HandPart.FourFlats:
                        details =
                            string.Format(CommonText.InstructionFourFlats, handName.ToLowerInvariant());
                        break;

                    case HandPart.TwoThumbs:
                        details = CommonText.InstructionTwoThumbs ;
                        break;
                    

                    default:
                        details = string.Format(CommonText.InstructionSingleFinger, side, partName);
                        break;

                }

                if (print.PrintList.Rules.CaptureGroup == PrintCaptureGroup.OneFingerOnly && !string.IsNullOrEmpty(print.PrintList.Rules.Labels.SingleFingerCapturePrompt))
                {
                    details = print.PrintList.Rules.Labels.SingleFingerCapturePrompt;
                }
            }
            else
            {
                details = string.Format(CommonText.InstructionRolledFinger, partName, side);
            }

            
            return details;

        }

        public List<EndorsableFingerViewModel> GetEndorsableFingers()
        {
            var endorsableFingers =
                this.PhysicalParts.Where(x => x.EndorsementIndex > 0 && !x.IsMissing)
                    .Select(y => new EndorsableFingerViewModel(y.EndorsementIndex, GetName(y))).ToList();

            return endorsableFingers;
        }

        private SegmentChange GetSegmentChange(List<SegmentChange> changes, PrintSegment segment)
        {
            var currentSeg = changes.FirstOrDefault(x => x.Segment == segment);
            if (currentSeg == null)
            {
                currentSeg = new SegmentChange { Segment = segment };
                changes.Add(currentSeg);
            }

            return currentSeg;
        }

        private void CreateList()
        {
            this.Rules = new PrintRules();

            this.FillPhysicalPartList();            

            this.FillPrintList();

            this.FillPrintsSegments();

            // create read only lists
            this.readOnlyPhysicalHandPart = this.physicalHandPart.AsReadOnly();
            this.readOnlyPrints = this.prints.AsReadOnly();
        }

        private void FillPrintsSegments()
        {

            var leftSlap = this.prints.Single(x => x.Hand == Hand.Left && x.HandPart == HandPart.FourFlats);
            this.AddSegments(leftSlap, this.physicalHandPart.Where(x => x.Hand == Hand.Left && x.Kind == HandPartKind.Finger && x.HandPart != HandPart.Thumb));

            var rightSlap = this.prints.Single(x => x.Hand == Hand.Right && x.HandPart == HandPart.FourFlats);
            this.AddSegments(rightSlap, this.physicalHandPart.Where(x => x.Hand == Hand.Right && x.Kind == HandPartKind.Finger && x.HandPart != HandPart.Thumb));

            var twoThumbs = this.prints.Single(x => x.Hand == Hand.None && x.HandPart == HandPart.TwoThumbs);
            this.AddSegments(twoThumbs, this.physicalHandPart.Where(x =>x.HandPart == HandPart.Thumb));

            var leftUpper = this.prints.Single(x => x.Hand == Hand.Left && x.HandPart == HandPart.UpperPalm);
            this.AddSegments(leftUpper, this.physicalHandPart.Where(x => x.Hand == Hand.Left && x.Kind == HandPartKind.Finger && x.HandPart != HandPart.Thumb));

            var rightUpper = this.prints.Single(x => x.Hand == Hand.Right && x.HandPart == HandPart.UpperPalm);
            this.AddSegments(rightUpper, this.physicalHandPart.Where(x => x.Hand == Hand.Right && x.Kind == HandPartKind.Finger && x.HandPart != HandPart.Thumb));
        }

        private void AddSegments(PrintInfo print, IEnumerable<PhysicalHandPart> parts)
        {
            var list = new List<PrintSegment>();

            foreach (var handPart in parts)
            {
                var seg = new PrintSegment(handPart);
                list.Add(seg);
            }

            print.Segments = list.AsReadOnly() ;
        }

        private void FillPhysicalPartList()
        {
            this.physicalHandPart = new List<PhysicalHandPart>();

            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.Thumb, Hand.Right, HandPartKind.Finger, 1));
            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.Index, Hand.Right, HandPartKind.Finger, 2));
            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.Middle, Hand.Right, HandPartKind.Finger, 3));
            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.Ring, Hand.Right, HandPartKind.Finger, 4));
            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.Little, Hand.Right, HandPartKind.Finger, 5));

            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.Thumb, Hand.Left, HandPartKind.Finger, 6));
            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.Index, Hand.Left, HandPartKind.Finger, 7));
            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.Middle, Hand.Left, HandPartKind.Finger, 8));
            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.Ring, Hand.Left, HandPartKind.Finger, 9));
            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.Little, Hand.Left, HandPartKind.Finger, 10));


            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.UpperPalm, Hand.Left, HandPartKind.Palm));
            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.LowerPalm, Hand.Left, HandPartKind.Palm));
            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.Hypothenar, Hand.Left, HandPartKind.Palm));

            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.UpperPalm, Hand.Right, HandPartKind.Palm));
            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.LowerPalm, Hand.Right, HandPartKind.Palm));
            this.physicalHandPart.Add(new PhysicalHandPart(HandPart.Hypothenar, Hand.Right, HandPartKind.Palm));
        }

        private void FillPrintList()
        {
            this.prints = new List<PrintInfo>();
            // define capture group membership to assign to print
            var flatOnly = PrintCaptureGroup.FlatOnly;
            var flatOrSq = PrintCaptureGroup.FlatOnly | (this.Rules.OrderMode == CaptureOrderMode.Sq ? PrintCaptureGroup.Standard14 : 0);
            var palmOnly = PrintCaptureGroup.StandardAndPalm;
            var flatRolled = flatOnly | PrintCaptureGroup.Standard14;
            var flatRolledPalm = flatRolled | palmOnly;
            var rolledPalm = PrintCaptureGroup.Standard14 | palmOnly;
            var none = PrintCaptureGroup.Unknown;

            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Thumb, Hand.Right), HandScanKind.Rolled, HandPartKind.Finger, rolledPalm, 1));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Index, Hand.Right), HandScanKind.Rolled, HandPartKind.Finger, rolledPalm, 2));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Middle, Hand.Right), HandScanKind.Rolled, HandPartKind.Finger, rolledPalm, 3));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Ring, Hand.Right), HandScanKind.Rolled, HandPartKind.Finger, rolledPalm, 4));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Little, Hand.Right), HandScanKind.Rolled, HandPartKind.Finger, rolledPalm, 5));

            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Thumb, Hand.Left), HandScanKind.Rolled, HandPartKind.Finger, rolledPalm, 6));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Index, Hand.Left), HandScanKind.Rolled, HandPartKind.Finger, rolledPalm, 7));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Middle, Hand.Left), HandScanKind.Rolled, HandPartKind.Finger, rolledPalm, 8));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Ring, Hand.Left), HandScanKind.Rolled, HandPartKind.Finger, rolledPalm, 9));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Little, Hand.Left), HandScanKind.Rolled, HandPartKind.Finger, rolledPalm, 10));

            this.prints.Add(new PrintInfo(this, this.Part(HandPart.FourFlats, Hand.Right), HandScanKind.Flat, HandPartKind.Other, flatRolledPalm, 13));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.FourFlats, Hand.Left), HandScanKind.Flat, HandPartKind.Other, flatRolledPalm, 14));

            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Index, Hand.Right), HandScanKind.Flat, HandPartKind.Finger, none));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Middle, Hand.Right), HandScanKind.Flat, HandPartKind.Finger, none));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Ring, Hand.Right), HandScanKind.Flat, HandPartKind.Finger, none));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Little, Hand.Right), HandScanKind.Flat, HandPartKind.Finger, none));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Thumb, Hand.Right), HandScanKind.Flat, HandPartKind.Finger, rolledPalm, 11));

            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Index, Hand.Left), HandScanKind.Flat, HandPartKind.Finger, none));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Middle, Hand.Left), HandScanKind.Flat, HandPartKind.Finger, none));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Ring, Hand.Left), HandScanKind.Flat, HandPartKind.Finger, none));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Little, Hand.Left), HandScanKind.Flat, HandPartKind.Finger, none));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Thumb, Hand.Left), HandScanKind.Flat, HandPartKind.Finger, rolledPalm, 12));

            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Endorsement, Hand.None), HandScanKind.Flat, HandPartKind.Other, none, EndorsementFingerIndex));

            this.prints.Add(new PrintInfo(this, this.Part(HandPart.TwoThumbs, Hand.None), HandScanKind.Flat, HandPartKind.Other, flatOrSq, 15));

            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Hypothenar, Hand.Right), HandScanKind.Flat, HandPartKind.Palm, palmOnly, 22));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.UpperPalm, Hand.Right), HandScanKind.Flat, HandPartKind.Palm, palmOnly, 26));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.LowerPalm, Hand.Right), HandScanKind.Flat, HandPartKind.Palm, palmOnly, 25));

            this.prints.Add(new PrintInfo(this, this.Part(HandPart.Hypothenar, Hand.Left), HandScanKind.Flat, HandPartKind.Palm, palmOnly, 24));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.UpperPalm, Hand.Left), HandScanKind.Flat, HandPartKind.Palm, palmOnly, 28));
            this.prints.Add(new PrintInfo(this, this.Part(HandPart.LowerPalm, Hand.Left), HandScanKind.Flat, HandPartKind.Palm, palmOnly, 27));
        }

        private PhysicalHandPart Part(HandPart part, Hand hand)
        {
            var result = this.physicalHandPart.SingleOrDefault(x => x.HandPart == part && x.Hand == hand);
            if (result == null)
            {
                // For special parts (Endorsement, FourFinger, TwoThumbs), a part is created but NOT added to the list
                result = new PhysicalHandPart(part, hand, HandPartKind.Other);
            }
            return result;
        }


        private void SetPrintElementStatus(PrintInfo print)
        {
            var mustValidateSegments = false;
            if (print == null)
            {
                return;
            }
            print.FailedValidations.Clear();            
            
            if (print.IsMissing)
            {
                print.Status = PrintStatus.Missing;                                
            }
            else if (print.ProcessStatus == PrintProcessStatus.InProcess)
            {
                print.Status = PrintStatus.PendingEvaluation;
            }            
            else if (print.ProcessStatus == PrintProcessStatus.TemplateError)
            {
                print.Status = PrintStatus.InError;
                print.FailedValidations.Add(PrintError.TemplateError);
                mustValidateSegments = true;
            }
            else if (print.ProcessStatus == PrintProcessStatus.ServiceError)
            {
                print.Status = PrintStatus.InError;
                print.FailedValidations.Add(PrintError.ServiceError);
                mustValidateSegments = true;
            }            
            else if (print.ProcessStatus == PrintProcessStatus.Submited)
            {
                print.Status = PrintStatus.Submited;
            }
            else if (print.Image == null)
            {
                print.Status = PrintStatus.Empty;
            }
            else
            {
                print.Status = PrintStatus.Validated;
                mustValidateSegments = true;

                // validate quality              
                if (this.Rules.QualityThreshold != 0 && (print.QualityScore > this.Rules.QualityThreshold || print.QualityScore == 0))
                {
                    if (this.Rules.IsQualityEnabled)
                    {
                        print.Status = PrintStatus.InError;
                    }
                    
                    print.FailedValidations.Add(PrintError.BadQuality);
                }

                // validate sequence for rolled                
                if (print.SequenceScore < this.Rules.SequenceThreshold  && print.IsSequenceCheckEnabled)
                {
                    //var changedSequence = print.NistPosition != print.MatchedNistPosition && print.MatchedNistPosition > 0;

                    if (this.Rules.IsSequenceEnabled)
                    {
                        print.Status = PrintStatus.InError;
                    }
                    
                    //print.FailedValidations.Add(changedSequence ? PrintError.SwappedSequenceCheckError : PrintError.SequenceError);
                    print.FailedValidations.Add(PrintError.SequenceError);
                }

                // Anti sequencing check
                // On Flat non sequenced slap, to make sure no two of them are the same (two thumbs...)
                if (!print.IsSequenceCheckEnabled && print.SequenceBestScore > this.Rules.SequenceThreshold && print.SequenceBestScore > this.Rules.AntiSequencingThreshold && print.SequenceBestScorePosition != print.NistPosition)
                {
                    print.Status = PrintStatus.InError;
                    print.FailedValidations.Add(PrintError.Double);
                }                
                
            }

            if (print.TemplateErrors.Count > 0)
            {
                print.Status = PrintStatus.InError;
            }

            bool invalidPrintSegments = false;
            // Validation for prints segments, ignore overide of print if a segment is in error (Flat Capture Only)!
            if (print.HasSegments && mustValidateSegments)
            {
                var segmentsErrors = new List<PrintError>();
                //bool segmentInError = false;
                
                foreach (var segment in print.Segments)
                {
                    if (segment.Part.IsMissing || !segment.IsExpected)
                    {
                        continue;
                    }

                    // check for undefined segments !
                    invalidPrintSegments = invalidPrintSegments || segment.Position.IsEmpty;

                    // Check segments quality
                    if (!segment.IsOverriden && this.Rules.QualityThreshold != 0 && (segment.QualityScore > this.Rules.QualityThreshold || segment.QualityScore == 0))
                    {
                        segmentsErrors.Add(PrintError.BadSegmentQuality);
                    }

                    // check segment match
                    if (print.Kind == HandPartKind.Palm && segment.SelfScore != null &&
                        segment.SelfScore.Score < this.Rules.AntiSequencingThreshold)
                    {
                        segmentsErrors.Add(PrintError.SequenceError);
                    }
                }

                if (!invalidPrintSegments && segmentsErrors.Count > 0)
                {
                    var distinctErrors = segmentsErrors.Distinct().ToList();
                    if (distinctErrors.Contains(PrintError.SequenceError) || (distinctErrors.Contains(PrintError.BadSegmentQuality) && this.Rules.IsQualityEnabled))
                    {
                        print.Status = PrintStatus.InError;
                    }

                    print.FailedValidations.AddRange(distinctErrors);
                }

                if ((print.IsOverriden || print.IsAcceptedByUser) && print.Status == PrintStatus.InError)
                {
                    print.Status = PrintStatus.Overriden;
                }
            }

            if ((print.IsOverriden || print.IsAcceptedByUser) && print.Status == PrintStatus.InError)
            {
                print.Status = PrintStatus.Overriden;
            }

            if (invalidPrintSegments && this.Rules.IsFlatCaptureMode)
            {
                print.Status = PrintStatus.InError;
                print.FailedValidations.Add(PrintError.InvalidPrintSegment);
            }

            //if (print.Kind == HandPartKind.Palm && print.Status == PrintStatus.InError)
            //{
            //    print.Status = PrintStatus.Validated;
            //}

            // handle swapped statuses > For Validated and InError statuses into 2
            if (print.NistPosition != print.MatchedNistPosition && print.MatchedNistPosition > 0 && this.Rules.IsSequenceChangingPosition)
            {
                switch (print.Status)
                {
                    case PrintStatus.InError:
                        print.Status = PrintStatus.InErrorSwapped;
                        break;
                    case PrintStatus.Validated:
                        print.Status = PrintStatus.ValidatedSwapped;
                        break;
                }
            }            
        }

        

        private string GetMissingText(string missingCode, string missingDate)
        {
            if (string.IsNullOrEmpty(missingCode))
            {
                return null;
            }
                        
            string text = string.Empty;

            if (this.missingTexts.ContainsKey(missingCode))
            {
                text = this.missingTexts[missingCode];
            }

            if (!string.IsNullOrEmpty(missingDate))
            {
                text += Environment.NewLine + missingDate;
            }

            return text;
        }

        /// <summary>
        /// Gets the image. Images are cached so they are not reconstructed when the status change or new ViewModels are created
        /// </summary>
        /// <param name="print"></param>
        /// <returns></returns>
        public ImageSource GetImage(PrintInfo print)
        {
            if (!print.IsMissing)
            {
                if (print.Image != null)
                {
                    if (this.bitmapLookups.ContainsKey(print.Key) && this.bitmapLookups[print.Key].ImageKey == print.ImageKey)
                    {
                        return this.bitmapLookups[print.Key].Image;
                    }

                    // get a thumbnail...
                    var thumb = XL_ID.Utilities.Image.ImageUtilities.ResizeImageProportionnally(print.Image, new System.Drawing.Size(400, 400), System.Drawing.Color.White);
                    var img = WriteableBitmapExtension.FromBitmap(thumb, true);
                    //print.ImageForProcessing.

                    //var img = (print.Kind == HandPartKind.Palm) ? print.ImageForProcessing.ToImageSource(false, true) : print.ImageForProcessing.ToImageSource(false, false);
                    ImageSourceLookup look;

                    if (this.bitmapLookups.ContainsKey(print.Key))
                    {
                        look = this.bitmapLookups[print.Key];
                    }
                    else
                    {
                        look = new ImageSourceLookup();
                        this.bitmapLookups.Add(print.Key, look);
                    }

                    look.Image = img;
                    look.ImageKey = print.ImageKey;
                    return img;                    
                    
                }
               
                return this.blankPrint;
            }

            return null;
        }

        public void RemoveBitmapLookup(string key)
        {
            if (this.bitmapLookups.ContainsKey(key))
            {
                this.bitmapLookups.Remove(key);
            }
        }

        public PrintCondition GetCondition(string code)
        {
            if (code == null)
            {
                code = string.Empty;
            }
            return this.printConditions.FirstOrDefault(x => x.Code == code);
        }

        public PrintCondition GetCondition(HandPartStatus status)
        {
            return this.printConditions.FirstOrDefault(x => x.Status == status);
        }

        public ImageSource GetStatusImage(PrintInfo print)
        {
            this.SetPrintElementStatus(print);
            return this.statusImages[print.Status];
        }

        public string GetStatusMessage(PrintInfo print)
        {
            var message = this.statusMessages[print.Status];

            if (print.Kind == HandPartKind.Palm)
            {
                return message;
            }

            if (print.FailedValidations.Count > 0)
            {
                message += print.FailedValidations.Aggregate(string.Empty, (current, error) => current + Environment.NewLine + (this.validationMessages[error]));
            }

            if (print.TemplateErrors.Count > 0)
            {
                message += print.TemplateErrors.Aggregate(string.Empty, (current,error) => 
                    current + Environment.NewLine + 
                    (CommonText.ResourceManager.GetString("TemplateError" + error, CommonText.Culture )));
            }

            return message;
        }

        public bool IsEndorsementEmpty()
        {
            var endorsement = this.prints.Single(x => x.HandPart == HandPart.Endorsement);

            return endorsement.Status == PrintStatus.Empty;
        }

        public bool CheckAllStatuses(bool displayMessage)
        {
            var printsToCheck = this.GetPrintListToVerify();
            var printsNotReady = printsToCheck.Where(
                    x =>
                        x.Status == PrintStatus.InError || x.Status == PrintStatus.InErrorSwapped
                        || x.Status == PrintStatus.PendingEvaluation || x.Status == PrintStatus.Submited
                        || x.Status == PrintStatus.Unknown || x.Status == PrintStatus.Empty).ToList();

            // TODO : Add a parameter for optional or mandatory palms maybe... but palms are always optional, so empty palms are not an error
            printsNotReady.RemoveAll(x => x.Status == PrintStatus.Empty && (x.Kind == HandPartKind.Palm));

            // Remove Wizard overridden prints
            printsNotReady.RemoveAll(x => (int) (x.UserAction & WizardAction.OverrideCodeMask) > 0);

            if (printsNotReady.Count == 0)
            {
                return true;
            }            

            if (!displayMessage)
            {
                return false;
            }

            // Create the message
            var msg = CommonText.PrintsNotReady + Environment.NewLine;

            foreach (var printInfo in printsNotReady)
            {
                msg += GetName(printInfo) + " : " + this.statusMessages[printInfo.Status]
                       + Environment.NewLine;
            }

            MessageBox.Show(msg, CommonText.ValidationTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);

            return false;
        }

        public bool IsPrintToBeVerified(PrintInfo pr)
        {
            var verify = false;
            if (pr.IsEndorsement)
            {
                verify = this.Rules.IsEndorsementAllowed;                
            }
            else
            {
                verify = (this.Rules.CaptureGroup & pr.GroupMembership) > 0;
            }

            if (verify)
            {
                this.SetPrintElementStatus(pr);
            }

            return verify;            
        }

        public IEnumerable<PrintInfo> GetPrintListToVerify()
        {
            if (this.Rules.CaptureGroup == PrintCaptureGroup.OneFingerOnly)
            {
                return this.prints.Where(
                                    x => x.Hand == Hand.Right && x.HandPart == HandPart.Thumb && x.ScanKind == HandScanKind.Flat)
                        .ToList();                
            }

            return this.prints.Where(this.IsPrintToBeVerified).ToList();
        }

        public IEnumerable<PrintInfo> GetMissingPrintList()
        {
            return this.prints.Where(x => this.IsPrintToBeVerified(x) && x.IsMissing);            
        }

        private class PrintOrder
        {
            public int TwoThumbsFlat = -1;

            public int RightThumbFlat = -1;
            public int LeftThumbFlat = -1;

            public int RightFourFingers = -1;
            public int RightThumbRolled = -1;
            public int RightIndexRolled = -1;
            public int RightMiddleRolled = -1;
            public int RightRingRolled = -1;
            public int RightLittleRolled = -1;

            public int LeftFourFingers = -1;
            public int LeftThumbRolled = -1;
            public int LeftIndexRolled = -1;
            public int LeftMiddleRolled = -1;
            public int LeftRingRolled = -1;
            public int LeftLittleRolled = -1;

            public int EndorsementFinger = -1;

            public int RightHypothenarPalm = -1;
            public int RightLowerPalm = -1;
            public int RightUpperPalm = -1;

            public int LeftHypothenarPalm = -1;
            public int LeftLowerPalm = -1;
            public int LeftUpperPalm = -1;

        }
    }

    
}


