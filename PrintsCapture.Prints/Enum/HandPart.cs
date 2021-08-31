namespace PrintsCapture.Prints.Enum
{
    public enum HandPart
    {
        Unknown = 0,
        Thumb = 1,
        Index = 2,
        Middle = 3,
        Ring = 4,
        Little = 5,
       
        UpperPalm = 6,
        LowerPalm = 7,
        Hypothenar = 8,

        Endorsement = 9,
        TwoThumbs = 10,
        FourFlats = 11,
        CompletePalm = 12,

    }

    public enum HandAndPart
    {
        LeftThumb = HandPart.Thumb | Hand.Left,
        LeftIndex = HandPart.Index | Hand.Left,
        LeftMiddle = HandPart.Middle | Hand.Left,
        LeftRing = HandPart.Ring | Hand.Left,
        LeftLittle = HandPart.Little | Hand.Left,


        RightThumb = HandPart.Thumb | Hand.Right,
        RightIndex = HandPart.Index | Hand.Right,
        RightMiddle = HandPart.Middle | Hand.Right,
        RightRing = HandPart.Ring | Hand.Right,
        RightLittle = HandPart.Little | Hand.Right,


        LeftUpperPalm = HandPart.UpperPalm | Hand.Left,
        LeftLowerPalm = HandPart.LowerPalm | Hand.Left,
        LeftHypothenar = HandPart.Hypothenar | Hand.Left,


        RightUpperPalm = HandPart.UpperPalm | Hand.Right,
        RightLowerPalm = HandPart.LowerPalm | Hand.Right,
        RightHypothenar = HandPart.Hypothenar | Hand.Right,
    }


    public static class HandAndPartExtension
    {
        public static Hand GetHand(this HandAndPart part)
        {
            return (Hand)(((int)part & (int)Hand.Left) | ((int)part & (int)Hand.Right));            
        }

        public static HandPart GetPart(this HandAndPart part)
        {            
            return (HandPart)((int)part - (int)GetHand(part));
        }


    }
}
