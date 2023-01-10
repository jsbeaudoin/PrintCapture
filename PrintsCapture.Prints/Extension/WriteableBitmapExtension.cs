using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PrintsCapture.Prints.Extension
{
    public static class WriteableBitmapExtension
    {
        public static WriteableBitmap FromBitmap(Bitmap img, bool writeData = false)
        {
            WriteableBitmap wbmp;

            if (img == null)
            {
                return null;
            }

            var pal =
                new BitmapPalette(
                    img.Palette.Entries.Select(x => System.Windows.Media.Color.FromArgb(255, x.R, x.G, x.B))
                        .ToList());
            wbmp = new WriteableBitmap(
                img.Width,
                img.Height,
                img.HorizontalResolution,
                img.VerticalResolution,
                PixelFormats.Indexed8,
                pal);

            if (writeData)
            {
                wbmp.WriteBitmap(img);
            }

            return wbmp;
        }

        // previous name : WriteIntoWpfBitmap
        public static void WriteBitmap(this WriteableBitmap wbmp, Bitmap img)
        {
            if (img == null)
            {
                img = new Bitmap(wbmp.PixelWidth, wbmp.PixelHeight);
                using (var g = Graphics.FromImage(img))
                {
                    g.Clear(System.Drawing.Color.White);
                }
            }

            BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadOnly, img.PixelFormat);

            try
            {
                var ptr0 = data.Scan0;
                var stride = data.Stride;
                if (stride < 0)
                {
                    stride = Math.Abs(stride);
                    ptr0 = new IntPtr(ptr0.ToInt32() - (stride * (img.Height - 1)));

                    var arraySize = stride * img.Height;
                    var pix = new byte[arraySize];
                    var height1 = img.Height - 1;

                    for (var row = 0; row <= height1; row++)
                    {
                        var rowAdr = new IntPtr(ptr0.ToInt32() + (height1 - row) * stride);
                        Marshal.Copy(rowAdr, pix, row * stride, stride);
                    }

                    // Marshal.Copy(ptr0, pix, 0, arraySize);
                    // Array.Reverse(pix);
                    wbmp.WritePixels(new Int32Rect(0, 0, img.Width, img.Height), pix, stride, 0);

                }
                else
                {
                    wbmp.WritePixels(
                    new Int32Rect(0, 0, img.Width, img.Height),
                    ptr0,
                    stride * img.Height,
                    stride);
                }

            }
            finally
            {
                img.UnlockBits(data);
            }
        }
    }
}
