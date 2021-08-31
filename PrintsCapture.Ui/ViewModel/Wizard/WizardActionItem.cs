using PrintsCapture.Prints;

namespace PrintsCapture.Ui.ViewModel.Wizard
{
    public class WizardActionItem
    {
        public string Label { get; set; }

        public WizardAction Action { get; set; }

        public OverrideReason Reason { get; set; }

    }
}
