namespace PrintsCapture.Cardscan
{
    using System.Collections.Generic;

    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.ViewModel;

    public class EndorsementZoneInfo : PrintZoneInfo
    {
        public List<EndorsableFingerViewModel> EndorsableFingers { get; set; }

        public EndorsementZoneInfo(PrintZoneInfo baseZone, List<EndorsableFingerViewModel> endorsableFingers) : base()
        {
            this.EndorsableFingers = endorsableFingers;            
            this.Hand = Hand.None;
            this.HandPart = HandPart.Endorsement;
            this.ScanKind = HandScanKind.Flat;

            this.Group = baseZone.Group;
            this.Id = baseZone.Id;
            this.Label = baseZone.Label;
            this.Size = baseZone.Size;
            this.SortOrder = baseZone.SortOrder;
        }

        public int EndorsementIndex { get; set; }

    }
}
