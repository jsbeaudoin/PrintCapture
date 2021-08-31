using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PrintsCapture.Device.LivescanJenetric.Sdk
{
    /// <summary>
    /// Extends the functionality of a bitmap.
    /// </summary>
    public static class BitmapExtensions
    {
        /// <summary>
        /// Converts the bitmap to a stream.
        /// </summary>
        /// <param name="bitmap">
        /// The bitmap.
        /// </param>
        /// <returns>
        /// The stream.
        /// </returns>
        public static Stream ToStream(this Bitmap bitmap)
        {
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// Converts the bitmap to a bitmap source.
        /// </summary>
        /// <param name="bitmap">
        /// The bitmap.
        /// </param>
        /// <param name="freeze">
        /// Indicating whether to freeze the bitmap source.
        /// </param>
        /// <returns>
        /// The bitmap source.
        /// </returns>
        public static BitmapSource ToBitmapImage(this Bitmap bitmap, bool freeze)
        {
            BitmapImage bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.None;
            bitmapImage.StreamSource = bitmap.ToStream();
            bitmapImage.EndInit();

            if (freeze)
            {
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }
    }
}
