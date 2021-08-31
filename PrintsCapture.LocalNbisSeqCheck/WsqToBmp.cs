
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace PrintsCapture.LocalAwSeqCheck
{
    /// <summary>
    /// Class to handle conversion between Bitmap and Wsq using NBIS
    /// </summary>
    public static class WsqToBmp
    {
        static WsqToBmp()
        {
            CompressionRatio = 15;
        }

        public static int CompressionRatio { get; set; }

        public static byte[] Decode(byte[] wsq)
        {
            var tmpBmp = Nbis.Wsq.FromWsqToBitmap(wsq);
            var mem = new MemoryStream();
            tmpBmp.Save(mem, ImageFormat.Bmp);

            return mem.ToArray();
        }

        public static Bitmap ToBmp(byte[] wsq)
        {
            return Nbis.Wsq.FromWsqToBitmap(wsq);
        }

        private static byte[] FromImageToBytes(Image img, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            return ToWsq((Bitmap)img);
        }

        private static float GetNbisBitRate()
        {
            if (CompressionRatio < 1 || CompressionRatio > 15)
            {
                CompressionRatio = 5;
            }

            var max = 4.0; // TEST. Real recommended max is -->  2.25; // ratio 5:1
            var min = 0.75; // ratio 15:1

            var variance = Math.Abs(max - min) / 14;

            return (float)(max - (variance * (CompressionRatio - 1)));

        }

        public static byte[] ToWsq(Bitmap bmp, string comment = "XL-ID")
        {
            var bmp2 = (Bitmap)bmp.Clone();
            return Nbis.Wsq.FromBitmapToWsq(bmp2, GetNbisBitRate(), 500, comment);
        }

        public static void pSetShowFilePropertiesDialog(Int64 file_properties_dialog)
        {
            return;
        }

        public static IntPtr pCreateBMPFromWSQByteArray([In] byte[] input_data_stream, [In] Int64 input_stream_length)
        {
            var bmp = Nbis.Wsq.FromWsqToBitmap(input_data_stream);
            return bmp.GetHbitmap();
        }
    }
}
