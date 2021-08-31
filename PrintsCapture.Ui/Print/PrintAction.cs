namespace PrintsCapture.Ui.Print
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using PrintsCapture.Prints.Enum;

    public class PrintAction
    {
        private static Dictionary<PrintZoomAction, ImageSource> images = null;

        public PrintAction(string label, PrintZoomAction action)
        {
            this.Label = label;
            this.Key = action;
            this.Image = GetImage(action);
        }

        public string Label { get; set; }

        public PrintZoomAction Key { get; set; }

        public ImageSource Image { get; private set; }

        private static ImageSource GetImage(PrintZoomAction action)
        {
            if (images == null)
            {
                // fill
                images = new Dictionary<PrintZoomAction, ImageSource>();
                images.Add(PrintZoomAction.Edit,   new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/edit32x32.png")));
                images.Add(PrintZoomAction.Scan, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/LivescanCapture32x32.png")));
                images.Add(PrintZoomAction.Resend, new BitmapImage(new Uri("pack://application:,,,/PrintsCapture.Ui;component/Images/ReSend32x32.png")));
            }

            if (!images.ContainsKey(action))
            {
                return null;
            }

            return images[action];
        }
    }
}
