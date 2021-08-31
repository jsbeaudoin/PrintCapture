namespace PrintsCapture.Prints
{
    using PrintsCapture.Prints.Enum;

    public class PhysicalHandPart
    {
        public static string GetPhysicalKey( Hand hand, HandPart handPart)
        {
            return hand.ToString() + "." + handPart.ToString();
        }

        public PhysicalHandPart(HandPart handPart, Hand hand, HandPartKind kind, int endorsementIndex = -1)
        {
            this.HandPart = handPart;
            this.Hand = hand;
            this.Kind = kind;
            this.EndorsementIndex = endorsementIndex;
        }

        public HandPart HandPart { get; private set; }

        public Hand Hand { get; private set; }

        public HandAndPart HandAndPart => (HandAndPart)((int)this.Hand | (int)this.HandPart);

        public HandPartKind Kind { get; private set; }

        public int EndorsementIndex { get; set; }

        /// <summary>
        /// RCMP missing code for the print
        /// </summary>
        public string MissingCode { get; set; }

        /// <summary>
        /// Gets or sets the missing date.
        /// </summary>
        public string MissingDate { get; set; }

        public bool IsMissing
        {
            get
            {
                return !string.IsNullOrEmpty(this.MissingCode);
            }
        }
    }
}
