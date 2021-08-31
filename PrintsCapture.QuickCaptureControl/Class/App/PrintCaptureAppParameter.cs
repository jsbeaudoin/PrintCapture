namespace PrintsCapture.QuickCaptureControl.Class.App
{
    using System;

    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Sequence;

    public class PrintCaptureAppParameter
    {
        private PrintList printList;

        public PrintCaptureAppParameter()
        {
            this.CaptureModeAllowed = PrintCaptureGroup.FlatOnly | PrintCaptureGroup.Standard14;
            this.IsEndorsementAllowed = true;
        }

        public string Mode { get; set; }

        public string DescriptionLine1 { get; set; }

        public string DescriptionLine2 { get; set; }

        public string CultureName { get; set; }

        public PrintCaptureGroup CaptureModeAllowed { get; set; }

        public bool IsEndorsementAllowed { get; set; }

        public bool IsOptionAvailable { get; set; }

        public bool IsDebugAvailable { get; set; }

        public bool IsWizardMode { get; set; }

        public string IcdVersion { get; set; }

        public PrintList PrintList
        {
            get
            {
                if (this.printList == null)
                {
                    this.printList = new PrintList();
                    CaptureKind captMode;
                    if (Enum.TryParse(this.Mode, true, out captMode))
                    {
                        this.printList.Rules.CaptureKind = captMode;
                    }                    
                }
                
                return this.printList;
            }
        }

        public BaseSeqCheckService SeqCheckService { get; set; }

        public string SeqCheckServiceConnection { get; set; }
    }
}
