namespace PrintsCapture.LivescanWinForm
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    //using System.Windows.Media.Imaging;

    using PrintsCapture.Prints.Enum;

    public class ConditionImage
    {
        public HandPartStatus Status { get; private set; }

        //public BitmapImage Image { get; private set; }
        public Bitmap Image { get; private set; }

        //private ConditionImage(HandPartStatus status, BitmapImage image)
        private ConditionImage(HandPartStatus status, Bitmap image)
        {
            this.Status = status;
            this.Image = image;
        }

        private static List<ConditionImage> images;

        public static List<ConditionImage> GetConditionImages()
        {
            if (images == null)
            {
                images = new List<ConditionImage>();
                AddImage(HandPartStatus.Present, @"Part_Present.png");
                AddImage(HandPartStatus.Amputated, @"Part_Amputated.png");
                AddImage(HandPartStatus.Bandaged, @"Part_Bandaged.png");
                AddImage(HandPartStatus.PhysicalLimitation, @"Part_PhysicalLimitation.png");                
                AddImage(HandPartStatus.ForeignReason, @"Part_ForeignReason.png");                                
            }

            return images;
        }

        private static void AddImage(HandPartStatus status, string imageName)
        {
            const string FixedPathPart = "pack://application:,,,/PrintsCapture.Livescan;component/Images/";

            var imageUri = new Uri(FixedPathPart + imageName);
            var b = new Bitmap(imageUri);

            images.Add(new ConditionImage(status, b));
        }
    }


    
}
