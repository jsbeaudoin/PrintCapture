using System.Collections.Generic;

namespace PrintsCapture.Livescan.ViewModel
{
    using PrintsCapture.Prints.ViewModel;

    public class EndorsementSelectViewModel
    {
        public List<EndorsableFingerViewModel> FingerList { get; set; }

        public EndorsableFingerViewModel SelectedFinger { get; set; }
    }
}
