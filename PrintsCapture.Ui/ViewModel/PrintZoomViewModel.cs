// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrintZoomViewModel.cs" company="Solutions XL-ID">
//   update text
// </copyright>
// <summary>
//   Defines the PrintZoomViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows;
using PrintsCapture.Prints.ViewModel;
using PrintsCapture.Ui.Class;

namespace PrintsCapture.Ui.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Media;

    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Extension;
    using PrintsCapture.Prints.Language;
    using PrintsCapture.Ui.Language;
    using PrintsCapture.Ui.Print;  

    using XL_ID.Utilities.XML;

    /// <summary>
    /// The print zoom view model.
    /// </summary>
    public class PrintZoomViewModel : INotifyPropertyChanged
    {       
        private int overrideCode;

        private string overrideText;        

        private List<OverrideReason> overrideReasons;                      

        private List<PrintAction> actions;
        private Visibility segmentOverrideVisibility;
        private SegmentInfoViewModel selectedSegment;
        
        private string overridePopupText;
        private string overrideLabel;
        private string overrideErrorMessage;


        public PrintZoomViewModel(PrintInfo print)
        {
            this.BrushForSelected = Brushes.Aqua;
            this.BrushForNotSelected = Brushes.LightSteelBlue;
            this.segmentOverrideVisibility = Visibility.Collapsed;            

            string name = string.Empty;          
            var lang = PrintCaptureApp.ApplicationCulture.TwoLetterISOLanguageName;
            var overrideReasonList = OverrideReason.GetBaseList().Where(x => x.Language == lang).ToList();

            var flatCaptureRestriction = !(print.IsSlap
                                         || print.PrintList.Rules.CaptureGroup != PrintCaptureGroup.FlatOnly
                                         || print.IsEndorsement);

            
            this.PrintSegments = print.PrintList.GetSegmentViewModel(print);
            this.CanOverrideSegment = print.PrintList.Rules.IsFlatCaptureMode;
            this.CanChangeExpectedSegment = true;
            this.Actions = new List<PrintAction>();
            if (print.PrintList.Rules.CaptureKind == CaptureKind.Livescan)
            {
                if (!flatCaptureRestriction)
                {
                    this.actions.Add(new PrintAction(Text.ScanAgain, PrintZoomAction.Scan));
                }                
            }
            else
            {
                if (print.Image != null)
                {
                    this.actions.Add(new PrintAction(Text.EditPrint, PrintZoomAction.Edit));
                }
                
            }                        

            if (print.ProcessStatus == PrintProcessStatus.ServiceError && !flatCaptureRestriction)
            {
                this.actions.Add(new PrintAction(Text.Resend, PrintZoomAction.Resend));
            }

            name = PrintList.GetName(print);

            if (print.PrintList.IsPrintSwapped(print))
            {
                var matchedPrint = print.PrintList.GetMatchedPrint(print);
                name = PrintList.GetName(matchedPrint);

                this.MatchingPrintPosition = string.Format(Text.PositionChangedFrom, PrintList.GetName(print));
            } 
            else if (print.SequenceBestScore > print.SequenceSelfScore && print.SequenceBestScore > print.PrintList.Rules.AntiSequencingThreshold && print.SequenceBestScorePosition > 0)
            {
                var matched =
                    print.PrintList.Prints.Single(x => x.NistPosition == print.SequenceBestScorePosition);
                var matchName = PrintList.GetName(matched);
                if (print.HasSegments)
                {
                    matchName = matched.Hand == Hand.Left ? CommonText.PrintLeftHand : CommonText.PrintRightHand;
                }
                this.MatchingPrintPosition = string.Format(Text.PrintMatchWarning, matchName, print.SequenceBestScore);
            }


            bool canOverridePrint = print.PrintList.Rules.IsOverrideAlwaysShown || print.Status == PrintStatus.InError || 
                print.Status == PrintStatus.InErrorSwapped || print.Status == PrintStatus.Overriden;

            if (print.ScanKind == HandScanKind.Flat && print.PrintList.Rules.IsOverrideFlatForbidden)
            {
                canOverridePrint = false;
            }
            else if (print.ScanKind == HandScanKind.Rolled && print.PrintList.Rules.IsOverrideRolledForbidden)
            {
                canOverridePrint = false;
            }
            else if (flatCaptureRestriction)
            {
                canOverridePrint = false;
            }

            if (print.PrintList.Rules.RetryNeededForOverride != 0 && 
                print.PrintList.Rules.CaptureKind == CaptureKind.Livescan &&
                print.CaptureCount < print.PrintList.Rules.RetryNeededForOverride )
            {
                CanOverridePrint = false;
            }

            if (print.HandPart == HandPart.Endorsement)
            {
                canOverridePrint = false;
                if (print.EndorsementFinger != null)
                {
                    name += " : " + PrintList.GetName(print.EndorsementFinger);
                }
            }

            if (print.Kind == HandPartKind.Palm)
            {
                canOverridePrint = false;
                this.CanChangeExpectedSegment = false;
            }

            this.PrintName = name;
            this.PrintImage = print.ImageForProcessing.ToImageSource(false, false); // full resolution print preview
            this.PrintStatusImage = print.PrintList.GetStatusImage(print);
            this.PrintStatusMessage = print.PrintList.GetStatusMessage(print);
            this.IsSequenceCheckEnabled = print.IsSequenceCheckEnabled;
            //this.MatchingPrintPosition = matchPositionText;
            this.OverrideReasons = overrideReasonList;
            this.IsOverriden = print.IsOverriden;
            this.OverrideCode = print.OverrideCode;
            this.OverrideText = print.OverrideUserReason;
            
            this.QualityScore = print.QualityScore;
            this.MinutiaCount = print.MinutiaCount;
            this.SequenceScore = print.SequenceScore;
            this.Print = print;
            this.CanOverridePrint = canOverridePrint;
            this.Resolution = print.Resolution.ToDpi();

            this.OverrideLabel = this.GetOverrideText(this.overrideCode, this.overrideText);

            if (print.HasSegments)
            {
                foreach (var printSegment in this.PrintSegments)
                {
                    printSegment.OverrideLabel = this.GetOverrideText(printSegment.OverrideCode, printSegment.OverrideText);                                                
                }
            }

            this.IsMatchingPrintDisplayed = !string.IsNullOrEmpty(this.MatchingPrintPosition);
        }

        //public bool CanOverride { get; private set; }

        public bool CanOverridePrint { get; private set; }

        public bool CanOverrideSegment { get; private set; }

        public bool CanChangeExpectedSegment { get; private set; }

        public PrintInfo Print { get; private set; }

        public List<PrintAction> Actions
        {
            get
            {
                return this.actions;
            }
            private set
            {
                if (Equals(value, this.actions))
                {
                    return;
                }
                this.actions = value;
                this.OnPropertyChanged("Actions");
            }
        }

        public string PrintName { get; private set; }

        public ImageSource PrintStatusImage { get; private set; }

        public ImageSource PrintImage { get; private set; }

        public bool IsSequenceCheckEnabled { get; private set; }

        /// <summary>
        /// Return value only
        /// </summary>
        public PrintZoomAction ActionSelected { get; set; }

        public int Resolution { get; private set; }

        public string MatchingPrintPosition { get; private set; }

        /// <summary>
        /// Set automatically when MatchingPrintPosition is set
        /// </summary>
        public bool IsMatchingPrintDisplayed { get; private set; }

        /// <summary>
        /// Gets or sets the override code.
        /// </summary>
        public int OverrideCode
        {
            get
            {
                return this.overrideCode;
            }
            set
            {
                if (value == this.overrideCode)
                {
                    return;
                }
                this.overrideCode = value;                

                this.OnPropertyChanged("OverrideCode");

                this.OverrideErrorMessage = string.Empty;
                //this.OnPropertyChanged("");
            }
        }

        public bool IsOverriden
        {
            get
            {
                return this.overrideCode > 0;
            }

            set
            {
                if (!value)
                {
                    this.OverrideCode = 0;
                    return;
                }
                
                if (this.OverrideCode == 0)
                {
                    this.overrideCode = 1;
                }

                this.OnPropertyChanged("IsOverriden");
                
            }
        }

        public bool HasPrintSegments
        {
            get { return this.PrintSegments != null && this.PrintSegments.Count > 0; }
        }

        /// <summary>
        /// Gets or sets the override text.
        /// </summary>
        
        public string OverrideText
        {
            get
            {
                return this.overrideText;
            }

            [DebuggerStepThrough]
            set
            {
                if (value == this.overrideText)
                {
                    return;
                }

                if (value != null)
                {
                    var text = value.ToUpper();

                    var match = Regex.Match(text, "[^A-Z -]");
                    if (match.Success)
                    {
                        throw new ApplicationException(Text.InvalidCharacters);
                    }

                    this.overrideText = text;
                }
                else
                {
                    this.overrideText = null;
                }

                this.OnPropertyChanged("OverrideText");

                //if (string.IsNullOrEmpty(text))
                //{
                //    return;
                //}

                //var overrideWithText = this.OverrideReasons.FirstOrDefault(x => x.HasUserText);
                //if (overrideWithText != null && overrideWithText.Code != this.OverrideCode)
                //{
                //    this.OverrideCode = overrideWithText.Code;
                //}
            }
        }        

        public List<OverrideReason> OverrideReasons
        {
            get
            {
                return this.overrideReasons;
            }
            private set
            {
                if (Equals(value, this.overrideReasons))
                {
                    return;
                }
                this.overrideReasons = value;
                this.OnPropertyChanged("OverrideText");
            }
        }

        public bool PopupForSegment { get; set; }        

        public string OverrideLabel
        {
            get { return overrideLabel; }
            set
            {
                if (value == this.overrideLabel)
                {
                    return;
                }
                overrideLabel = value;
                this.OnPropertyChanged("OverrideLabel");
            }
        }

        public string OverridePopupText
        {
            get { return overridePopupText; }

            [DebuggerStepThrough]
            set
            {
                if (this.overridePopupText == value)
                {
                    return;
                }

                try
                {
                    if (value != null)
                    {
                        var text = value.ToUpper();

                        var match = Regex.Match(text, "[^A-Z -]");
                        if (match.Success)
                        {
                            throw new ApplicationException(Text.InvalidCharacters);
                        }
                        overridePopupText = text;
                    }
                    else
                    {
                        overridePopupText = null;
                    }
                }
                catch (Exception ex)
                {
                    this.OverrideErrorMessage = ex.Message;
                    throw;
                }
                
                this.OnPropertyChanged("OverridePopupText");
                this.OverrideErrorMessage = string.Empty;
            }
        }

        public string OverrideErrorMessage
        {
            get { return overrideErrorMessage; }
            set
            {
                if (value == overrideErrorMessage)
                {
                    return;
                }

                overrideErrorMessage = value;
                this.OnPropertyChanged("OverrideErrorMessage");
            }
        }

        public int SequenceScore { get; private set; }

        public int QualityScore { get; private set; }

        public int MinutiaCount { get; private set; }

        public string PrintStatusMessage { get; set; }               

        public event PropertyChangedEventHandler PropertyChanged;               

        public Visibility SegmentOverrideVisibility
        {
            get { return segmentOverrideVisibility; }
            set
            {
                if (value == this.segmentOverrideVisibility)
                {
                    return;
                }
                this.segmentOverrideVisibility = value;
                this.OnPropertyChanged("SegmentOverrideVisibility");
            }
        }

        public Brush BrushForSelected { get; private set; }
        public Brush BrushForNotSelected { get; private set; }

        public List<SegmentInfoViewModel> PrintSegments { get; private set; }

        public SegmentInfoViewModel SelectedSegment
        {
            get { return selectedSegment; }
            set
            {
                if (value == this.selectedSegment)
                {
                    return;
                }
                selectedSegment = value;
                this.OnPropertyChanged("SelectedSegment");

                this.SegmentOverrideVisibility = value == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public string GetOverrideText(int code, string text)
        {
            var reason = this.OverrideReasons.SingleOrDefault(x => x.Code == code);
            return this.GetOverrideText(reason, text);
        }

        public string GetOverrideText(OverrideReason reason, string text)
        {
            if (reason == null || reason.Code == 0)
            {
                return Text.NoOverride;
            }
            
            if (reason.HasUserText)
            {
                return reason.Text + " : " + text;
            }

            return reason.Text;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}