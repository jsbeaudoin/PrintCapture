using System.Collections.Generic;
using System.Linq;
using PrintsCapture.Prints;
using PrintsCapture.Prints.Enum;
using PrintsCapture.Prints.Language;
using PrintsCapture.Ui.Class;
using PrintsCapture.Ui.ViewModel;

namespace PrintsCapture.Ui.Service
{
    public class MissingReasonService
    {
        private readonly List<ReasonInfo> allowedReasons = new List<ReasonInfo>();

        public MissingReasonService(PrintInfo printInfo) : this(printInfo.HandPart) { }

        public MissingReasonService(HandPart handPart)
        {
            var isIcd178 = PrintCaptureApp.Instance.IcdVersion == @"178";
            var isFlatCapture = PrintCaptureApp.Instance.PrintList.Rules.IsFlatCaptureMode;

            var restrict178 = isIcd178 || isFlatCapture;

            // For flat capture or Criminal 178, FourSlaps and twothumbs can only be missing
            if ((handPart == HandPart.FourFlats || handPart == HandPart.TwoThumbs)
                && restrict178)
            {
                allowedReasons.Add(new ReasonInfo(@"MI", CommonText.HandPartMissingImage));
            }
            else
            {
                allowedReasons.Add(new ReasonInfo(@"XX", CommonText.HandPartAmputated));
                allowedReasons.Add(new ReasonInfo(@"UP", CommonText.HandPartBandaged));
                allowedReasons.Add(new ReasonInfo(@"PL", CommonText.HandPartPhysicalLimitation));

                if (!restrict178)
                {
                    allowedReasons.Add(new ReasonInfo(@"FR", CommonText.HandPartForeignReason));
                }
            }

            if (allowedReasons.Count > 0)
            {
                this.DefaultMissingReason = allowedReasons[0].Code;
            }
        }

        public bool IsAllowed(string code)
        {
            return allowedReasons.Any(x => x.Code == code);
        }

        public string DefaultMissingReason { get; private set; }

        public List<MissingReasonViewModel> GetList()
        {
            return
                this.allowedReasons.Select(x => new MissingReasonViewModel() { Code = x.Code, Label = x.Label })
                    .ToList();
        }

        private class ReasonInfo
        {
            public ReasonInfo(string code, string label)
            {
                this.Code = code;
                this.Label = label;
            }

            public string Code { get; private set; }
            public string Label { get; private set; }
        }
    }
}
