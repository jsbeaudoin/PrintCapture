using System;
using System.Collections.Generic;
using System.Linq;

namespace PrintsCapture.LivescanWinForm
{
    using PrintsCapture.Device.Enum;
    using PrintsCapture.Livescan.SharedControls;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;

    public class HandPartQualityAndStateDisplay
    {
        //private readonly PrintList prints;

        public HandQualityControl Control { get; set; }

        private readonly Dictionary<Hand, Dictionary<HandPart, HandPartQualityDisplay>> handPartDisplays = new Dictionary<Hand, Dictionary<HandPart, HandPartQualityDisplay>>();

        public HandPartQualityAndStateDisplay(PrintList prints)
        {
            //this.prints = prints;            
            var leftParts = new Dictionary<HandPart, HandPartQualityDisplay>();
            var rightParts = new Dictionary<HandPart, HandPartQualityDisplay>();

            handPartDisplays.Add(Hand.Left, leftParts);
            handPartDisplays.Add(Hand.Right, rightParts);

            foreach (HandPart handPartValue in Enum.GetValues(typeof(HandPart)))
            {
                HandPart value = handPartValue;
                leftParts.Add(handPartValue, new HandPartQualityDisplay(prints.PhysicalParts.FirstOrDefault(x => x.Hand ==  Hand.Left && x.HandPart == value)));
                rightParts.Add(handPartValue, new HandPartQualityDisplay(prints.PhysicalParts.FirstOrDefault(x => x.Hand == Hand.Right && x.HandPart == value)));
            }

            //this.SetConditions();

        }

        //public void SetConditions()
        //{

        //    foreach (var pp in prints.PhysicalParts)
        //    {
        //        this.SetPhysicalPartCondition(pp);
        //    }

        //}

        public void ResetQuality(PrintQualityLevel quality = PrintQualityLevel.Unknown)
        {
            // reset qualities of all the parts
            foreach (var handPartDisplay in handPartDisplays)
            {
                foreach (var partDisplay in handPartDisplay.Value)
                {
                    var hand = handPartDisplay.Key;
                    if (partDisplay.Value.Part == null || partDisplay.Value.Part.IsMissing)
                    {
                        continue;
                    }

                    partDisplay.Value.SetQuality(quality);
                    var part = partDisplay.Key;
                    if (this.Control != null)
                    {
                        this.Control.SetPartBrushAndTip(hand, part, partDisplay.Value.PartBrush);
                    }
                    
                }
            }
        }

        public void SetQuality(Hand hand, HandPart part, PrintQualityLevel quality)
        {
            var d = this.GetDisplay(hand, part);
            if (d == null)
            {
                return;
            }

            d.SetQuality(quality);
            if (this.Control != null && ! d.Part.IsMissing)
            {
                this.Control.SetPartBrushAndTip(hand, part, d.PartBrush);
            }
            
        }

        //private void SetPhysicalPartCondition(PhysicalHandPart part)
        //{
        //    var display = this.GetDisplay(part.Hand, part.HandPart);
        //    if (display == null)
        //    {
        //        return;
        //    }

        //    var condition = this.prints.GetCondition(part.MissingCode);
            
        //    display.SetCondition(condition.Status, part.MissingDate);
        //    if (this.Control != null)
        //    {
        //        this.Control.SetPartBrushAndTip(part.Hand, part.HandPart, display.PartBrush);
        //    }
            
        //}

        private HandPartQualityDisplay GetDisplay(Hand hand, HandPart part)
        {
            if (!handPartDisplays.ContainsKey(hand))
            {
                return null;
            }

            var parts = handPartDisplays[hand];

            if (!parts.ContainsKey(part))
            {
                return null;
            }

            return parts[part];
        }

    }
}
