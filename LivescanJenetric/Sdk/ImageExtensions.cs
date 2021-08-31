using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TouchLab.NET;

namespace PrintsCapture.Device.LivescanJenetric.Sdk
{       
        /// <summary>
        /// Extends the functionality of an image.
        /// </summary>
        public static class ImageExtensions
        {
            /// <summary>
            /// Initializes the color palette.
            /// </summary>
            static ImageExtensions()
            {
                for (int index = 0; index < m_ColorPalette.Length; ++index)
                {
                    m_ColorPalette[index] = System.Drawing.Color.FromArgb(index, index, index);
                }
            }

            /// <summary>
            /// Converts the image to a bitmap source.
            /// </summary>
            /// <param name="image">
            /// The image.
            /// </param>
            /// <param name="freeze">
            /// Indicates whether to freeze the bitmap source.
            /// </param>
            /// <returns>
            /// The bitmap source.
            /// </returns>
            public static BitmapSource ToBitmapImage(this Image image, bool freeze)
            {
                using (System.Drawing.Bitmap bitmap = image.ToBitmap())
                {
                    return bitmap.ToBitmapImage(freeze);
                }
            }

            /// <summary>
            /// Converts the image to a bitmap.
            /// </summary>
            /// <param name="image">
            /// The image.
            /// </param>
            /// <returns>
            /// The bitmap.
            /// </returns>
            public static System.Drawing.Bitmap ToBitmap(this Image image)
            {
                System.Drawing.Imaging.PixelFormat format;

                switch (image.Format)
                {
                    case ImageFormat.GRAY8:
                        format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                        break;
                    case ImageFormat.ARGB32:
                        format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image.Width, image.Height, format);

                if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                {
                    // Reorganize the color palette to gray scale colors.
                    System.Drawing.Imaging.ColorPalette palette = bitmap.Palette;
                    System.Drawing.Color[] entries = palette.Entries;
                    Array.Copy(m_ColorPalette, entries, m_ColorPalette.Length);
                    bitmap.Palette = palette;
                }

                System.Drawing.Imaging.BitmapData data = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    bitmap.PixelFormat);

                Marshal.Copy(image.Data, 0, data.Scan0, image.Data.Length);

                bitmap.UnlockBits(data);

                return bitmap;
            }

            /// <summary>
            /// Converts the image to a bitmap source with highlighted segments.
            /// </summary>
            /// <param name="image">
            /// The image.
            /// </param>
            /// <param name="segmentation">
            /// The segmentation.
            /// </param>
            /// <param name="freeze">
            /// Indicates whether to freeze the bitmap source.
            /// </param>
            /// <returns>
            /// The bitmap source.
            /// </returns>
            public static BitmapSource ToBitmapImage(this Image image, Segmentation segmentation, bool freeze)
            {
                using (System.Drawing.Bitmap bitmap = image.ToBitmap(segmentation))
                {
                    return bitmap.ToBitmapImage(freeze);
                }
            }

            /// <summary>
            /// Converts the image to a bitmap with highlighted segments.
            /// </summary>
            /// <param name="image">
            /// The image.
            /// </param>
            /// <param name="segmentation">
            /// The segments.
            /// </param>
            /// <returns>
            /// The bitmap.
            /// </returns>
            public static System.Drawing.Bitmap ToBitmap(this Image image, Segmentation segmentation)
            {
                System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Brushes.Red, 2.5f);

                System.Drawing.Bitmap canvas
                    = new System.Drawing.Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(canvas))
                {
                    graphics.Clear(System.Drawing.Color.White);
                    using (System.Drawing.Bitmap background = image.ToBitmap())
                    {
                        graphics.DrawImage(background, 0.0f, 0.0f);
                    }
                    for (int index = 0; index < segmentation.Count; ++index)
                    {
                        Segment segment = segmentation[index];
                        graphics.DrawRectangle(
                            pen,
                            segment.OriginX,
                            segment.OriginY,
                            segment.Width,
                            segment.Height);
                    }
                }
                return canvas;
            }


            /// <summary>
            /// The color palette used by gray scale images.
            /// </summary>
            private static readonly System.Drawing.Color[] m_ColorPalette
                = new System.Drawing.Color[256];
        }

}
