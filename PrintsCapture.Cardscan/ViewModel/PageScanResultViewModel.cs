namespace PrintsCapture.Cardscan.ViewModel
{
    using System.Collections.Generic;

    public class PageScanResultViewModel
    {
        public PageScanResultViewModel()
        {
            this.Results = new List<PageScanZoneResultViewModel>();
        }

        public List<PageScanZoneResultViewModel> Results { get; private set; }
    }
}
