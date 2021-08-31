namespace PrintsCapture.Livescan
{
    using System.Windows.Media;

    using PrintsCapture.Device.Enum;
    using PrintsCapture.Prints;

    public class HandPartQualityDisplay 
    {
        

        private static Brush GetBrush(HandPartQualityDisplay instance)
        {            
            Brush result = Brushes.Green;

            if (instance.Part != null && !instance.Part.IsMissing)
            {
                switch (instance.Quality)
                {
                    case PrintQualityLevel.Bad:
                        result = Brushes.Red;
                        break;
                    case PrintQualityLevel.NotGoodEnough:
                        result = Brushes.Yellow;
                        break;
                    case PrintQualityLevel.Good:
                        result = Brushes.Green;
                        break;
                    case PrintQualityLevel.NotPresent:
                        result = Brushes.Black;
                        break;
                    case PrintQualityLevel.Unknown:
                        result = Brushes.Gray;
                        break;
                }
            }
            else
            {
                
            }

            return result;
        }



        private Brush partBrush;

        //private string missingDate;        

        public PhysicalHandPart Part { get; private set; }

        public PrintQualityLevel Quality { get; private set; }

        //public HandPartStatus Status { get; private set; }

        //public string MissingDate
        //{
        //    get
        //    {
        //        return this.missingDate;
        //    }
        //    private set
        //    {
        //        if (value == this.missingDate)
        //        {
        //            return;
        //        }
        //        this.missingDate = value;
                
        //    }
        //}

        public Brush PartBrush
        {
            get
            {
                return this.partBrush;
            }
            private set
            {
                if (Equals(value, this.partBrush))
                {
                    return;
                }
                this.partBrush = value;                
            }
        }

        public HandPartQualityDisplay(PhysicalHandPart physical)
        {
            this.Part = physical;            
            this.Quality = PrintQualityLevel.Unknown;            
        }        

        public void Reset()
        {
            this.Quality = PrintQualityLevel.Unknown;
            this.PartBrush = GetBrush(this);
        }        

        public void SetQuality(PrintQualityLevel quality)
        {
            if (this.Part == null || this.Part.IsMissing)
            {
                return;
            }

            this.Quality = quality;
            // refresh brush
            this.PartBrush = GetBrush(this);
        }
       
    }
}
