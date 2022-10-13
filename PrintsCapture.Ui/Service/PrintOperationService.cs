using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PrintsCapture.Ui.Service;

namespace PrintsCapture.Ui.Class
{
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.ViewModel;
    using PrintsCapture.Ui.ViewModel;
    using PrintsCapture.Ui.View;

    public class PrintOperationService
    {

        const string DefaultFlatMissingReason = "MI";
        const string DefaultOtherMissingReason = "XX";

        public void ProcessZoomWindow(PrintElementViewModel vm)
        {
            var print = vm.LinkedPrint;

            if (!(print.IsSlap || print.IsEndorsement) && print.PrintList.Rules.IsFlatCaptureMode)
            {
                return;
            }

            var zoomVm = new PrintZoomViewModel(print);
            var zoomWin = new PrintZoomWindow();
            zoomWin.ViewModel = zoomVm;

            var result = zoomWin.ShowDialog();

            if (!(result.HasValue && result.Value))
            {
                return;
            }

            switch (zoomVm.ActionSelected)
            {
                case PrintZoomAction.None:
                    break;

                case PrintZoomAction.SavePrintChanges:

                    var segmentChange = print.PrintList.UpdatePrintSegments(print, zoomVm.PrintSegments);
                    if (print.OverrideCode != zoomVm.OverrideCode || print.OverrideUserReason != zoomVm.OverrideText)
                    {
                        print.OverrideCode = zoomVm.OverrideCode;
                        print.OverrideUserReason = zoomVm.OverrideText;
                        PrintCaptureApp.SequenceCheck.SetPrintOverride(print);
                    }

                    if (segmentChange.Any(x => x.IsExpectedChanged))
                    {
                        print.TemplateErrors.Clear();

                        print.ProcessStatus = PrintProcessStatus.InProcess;
                        PrintCaptureApp.SequenceCheck.AddPrint(print);
                    }
                    else
                    {
                        foreach (var change in segmentChange)
                        {
                            PrintCaptureApp.SequenceCheck.SetSegmentOverride(print, change.Segment);
                        }
                    }

                    break;

                case PrintZoomAction.Resend:
                    print.ProcessStatus = PrintProcessStatus.InProcess;
                    PrintCaptureApp.SequenceCheck.AddPrint(print);
                    break;

                case PrintZoomAction.Scan:
                    print.ProcessStatus = PrintProcessStatus.InProcess;
                    PrintCaptureDriver.Instance.CaptureSingle(print);
                    break;

                default:
                    PrintCaptureDriver.Instance.DoOperation(print, zoomVm.ActionSelected);
                    break;
            }

            PrintModificationDispatcher.PrintModified(print);
        }

        public void ProcessPresenceWindow(PrintElementViewModel vm)
        {
            var print = vm.LinkedPrint;
            //var isFlatCapture = print.PrintList.Rules.IsFlatCaptureMode;
            //var flatRestriction = print.PrintList.Rules.IsFlatCaptureMode && print.IsSlap;
            
            // Print not captured
            var name = print.PrintList.Rules.IsFlatCaptureMode ? PrintList.GetName(print.Hand, print.HandPart) : vm.Name;
            var presVm = new FingerPresenceViewModel(name, print.PhysicalPart.MissingCode, print.PhysicalPart.MissingDate, print);

            var presWin = new FingerPresenceWindow(presVm);

            var result = presWin.ShowDialog();

            if (!(result.HasValue && result.Value))
            {
                return;
            }

            if (string.IsNullOrEmpty(presVm.MissingCode))
            {
                presVm.MissingDate = string.Empty;
            }

            this.UpdateMissingInfo(print.PhysicalPart, presVm.MissingCode, presVm.MissingDate);                                    
        }

        public void UpdateMissingInfo(PhysicalHandPart physicalPart, string code, string date)
        {
            var printList = PrintCaptureApp.Instance.PrintList;

            physicalPart.MissingCode = code;
            physicalPart.MissingDate = date;

            var allModifiedPrints = printList.Prints.Where(x => x.PhysicalPart == physicalPart).ToList();

            foreach (var modifiedPrint in allModifiedPrints)
            {
                PrintModificationDispatcher.PrintModified(modifiedPrint);
            }

            if (! string.IsNullOrEmpty(code) && physicalPart.HandPart == HandPart.FourFlats && printList.Rules.CaptureGroup != PrintCaptureGroup.FlatOnly)
            {                
                this.UpdateFingersUnderFlat(printList.PhysicalParts.Where(
                    x => x.Hand == physicalPart.Hand && x.Kind == HandPartKind.Finger && x.HandPart != HandPart.Thumb).ToList(), code, date);
            }
            else if (! string.IsNullOrEmpty(code) && physicalPart.Kind == HandPartKind.Finger && physicalPart.HandPart != HandPart.Thumb)
            {                
                // if all fingers were set to missing, set the flat image to "Amputated"                                
                var four = printList.Prints.Single(x => x.Hand == physicalPart.Hand && x.HandPart == HandPart.FourFlats);
                
                var fingers = printList.PhysicalParts.Where(x => x.Kind == HandPartKind.Finger && x.Hand == physicalPart.Hand && x.HandPart != HandPart.Thumb).ToList();
                this.UpdateFlatFromFingers(fingers, four, date, code);

            }

            // if two thumbs are missing
            if ((physicalPart.HandPart == HandPart.Thumb || physicalPart.HandPart == HandPart.TwoThumbs)
                && printList.Rules.CaptureTwoThumbs)
            {
                var two = printList.Prints.Single(x => x.HandPart == HandPart.TwoThumbs);
                var thumbs = printList.PhysicalParts.Where(x => x.HandPart == HandPart.Thumb).ToList();
                this.UpdateFlatFromFingers(thumbs, two, date, code);
            }
        }

        private void UpdateFlatFromFingers(IEnumerable<PhysicalHandPart> allFingers, PrintInfo flat, string date, string flatCode)
        {
            if (!allFingers.All(x => x.IsMissing) || flat.IsMissing)
            {
                return;
            }

            var reasonValidator = new MissingReasonService(flat);
            if (!reasonValidator.IsAllowed(flatCode))
            {
                flatCode = reasonValidator.DefaultMissingReason;
            }

            flat.PhysicalPart.MissingCode = flatCode;
            flat.PhysicalPart.MissingDate = date;
            PrintModificationDispatcher.PrintModified(flat);
        }         

        private void UpdateFingersUnderFlat(IEnumerable<PhysicalHandPart> allFingers, string missingCode, string missingDate)
        {
            var reasonValidator = new MissingReasonService(HandPart.Index);
            if (!reasonValidator.IsAllowed(missingCode))
            {
                missingCode = reasonValidator.DefaultMissingReason;
            }

            foreach (var physicalHandPart in allFingers)
            {
                // change no rerason to a reason, or a reason to no reason, but will not change a reason already present for another reason.
                if (string.IsNullOrEmpty(physicalHandPart.MissingCode))
                {
                    physicalHandPart.MissingCode = missingCode;
                    physicalHandPart.MissingDate = missingDate;
                    PhysicalHandPart part = physicalHandPart;
                    var allModifiedPrints = PrintCaptureApp.Instance.PrintList.Prints.Where(x => x.PhysicalPart == part).ToList();

                    foreach (var modifiedPrint in allModifiedPrints)
                    {
                        PrintModificationDispatcher.PrintModified(modifiedPrint);
                    }
                }
                
            }
        }        

    }
}
