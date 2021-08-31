using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PrintsCapture.Cardscan.ViewModel;
using PrintsCapture.Prints.Language;
using XL_ID.Utilities.Image;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace PrintsCapture.Cardscan
{
    public class PageScan
    {
        private static PageScan instance;

        private static Image displayedImage;       

        public static Image DisplayedImage
        {
            get
            {
                return displayedImage;
            }
            set
            {
                displayedImage = value;
                if (value == null)
                {
                    return;
                }

                value.SizeChanged += DisplayedImageSizeChanged;
            }
        }

        private static void DisplayedImageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            instance.wpfImageHeight = displayedImage.ActualHeight;
            instance.wpfImageWidth = displayedImage.ActualWidth;
        }

        
        private double wpfImageWidth;
        private double wpfImageHeight;

        public Bitmap originalImage;

        public static void CaptureCurrentZone()
        {
            if (instance.CurrentModel == null || instance.ViewModel.CurrentZone == null)
            {
                return;
            }
            var zone = instance.ViewModel.CurrentZone.LinkedZone;
            //if (zone.ZoneImage == null)
            //{
            //    instance.CaptureZone(zone); 
            //}            
            
            BitmapSource result = null;
            try
            {
                var cropped = instance.CaptureZone(zone);
                result = ImageUtilities.CreateBitmapSourceFromBitmap(
                cropped,
                Application.Current.Dispatcher, false);
            }
            catch (Exception)
            {
                // nothing
            }            

            instance.ViewModel.CurrentZone.DisplayedImage = result;
        }

        private int scanDpi;

        private ScanModel currentModel;

        private WriteableBitmap displayedBitmap;

        public PageScan(Bitmap scannedImage, int scanDpi, bool isFlat)
        {
            instance = this;
            this.scanDpi = scanDpi;
            this.ViewModel = new PageScanViewModel(isFlat);
            this.CurrentModel = new ScanModel { Name = CommonText.NoModel, IsReadOnly = true};
            this.ScanZones = new List<ScanPrintZone>();            

            this.ScannedImage = scannedImage;
            this.originalImage = scannedImage;

            //this.displayedBitmap = new WriteableBitmap(scannedImage.Width, scannedImage.Height, scannedImage.HorizontalResolution, scannedImage.VerticalResolution, PixelFormats.Rgb24, null);
            var colorList = new List<Color>();
            for (var x = 0; x < 256; x++)
            {
                colorList.Add(Color.FromRgb((byte)x, (byte)x, (byte)x));
            }
            var palette = new BitmapPalette(colorList);
            this.displayedBitmap = new WriteableBitmap(scannedImage.Width, scannedImage.Height, scannedImage.HorizontalResolution, scannedImage.VerticalResolution, PixelFormats.Indexed8, palette);
            this.WriteBitmap();

            this.ViewModel.PageImage = this.displayedBitmap; // this.ScannedImage.ToImageSource(false, false);
            
        }

        public Bitmap ScannedImage { get; private set; }        

        public ScanModel CurrentModel
        {
            get
            {
                return this.currentModel;
            }
            set
            {
                this.currentModel = value;
                this.ViewModel.CurrentModel = value;
            }
        }

        public List<ScanPrintZone> ScanZones { get; private set; }        

        public PageScanViewModel ViewModel { get; private set; }

        public Bitmap CaptureZone(ScanPrintZone zone)
        {
            //zone.ZoneImage = null;
            if (this.CurrentModel == null)
            {
                return null;
            }            

            var adjustDpiX = this.ScannedImage.Width / instance.wpfImageWidth;  // ActualWidth //bmp.HorizontalResolution / 96;
            var adjustDpiY = this.ScannedImage.Height / instance.wpfImageHeight; // ActualHeight // bmp.VerticalResolution / 96;                        

            var pixSize = zone.Size.ToPixels();
            var pixLoc = zone.Location.ToPixels();
            var left = pixLoc.Left; 
            var top = pixLoc.Top;
            var width = pixSize.Width; //var width = zone.Thumb.ActualWidth;
            var height = pixSize.Height; //var height = zone.Thumb.ActualHeight;

            var pxLeft = (float)(left * adjustDpiX);
            var pxTop = (float)(top * adjustDpiY);

            var pxWidth = (float)(width * adjustDpiX);
            var pxHeight = (float)(height * adjustDpiY);

            if (pxLeft < 1)
            {
                pxWidth = pxWidth + pxLeft;
                pxLeft = 0;
            }

            if (pxTop < 1)
            {
                pxHeight = pxHeight + pxTop;
                pxTop = 0;
            }

            if (pxLeft + 10 > this.ScannedImage.Width || pxTop + 10 > this.ScannedImage.Height)
            {
                return null;
            }

            if (pxLeft + pxWidth > this.ScannedImage.Width)
            {
                pxWidth = (int)(this.ScannedImage.Width - pxLeft);
            }

            if (pxTop + pxHeight > this.ScannedImage.Height)
            {
                pxHeight = (int)(this.ScannedImage.Height - pxTop);
            }

            // Cannot capture a 0 height or width rectangle !
            if (pxHeight < 1 || pxWidth < 1)
            {
                return null;
            }

            var cropped = this.ScannedImage.Clone(new RectangleF(pxLeft, pxTop, pxWidth, pxHeight), this.ScannedImage.PixelFormat);

            // Apply rotation
            if (zone.Rotation > 0)
            {
                var rotateType = RotateFlipType.Rotate90FlipNone;
                if (zone.Rotation == 180)
                {
                    rotateType = RotateFlipType.Rotate180FlipNone;
                }
                else if (zone.Rotation == 270)
                {
                    rotateType = RotateFlipType.Rotate270FlipNone;
                }

                cropped.RotateFlip(rotateType);
            }

            cropped.SetResolution(this.scanDpi, this.scanDpi);

            return cropped;
        }

        public void AdjustContrastBrightness(float brightness, float contrast)
        {            
            float gamma = 1.0f; // no change in gamma

            float adjustedBrightness = (float)brightness - 1.0F;
            // create matrix that will brighten and contrast the image
            float[][] ptsArray ={
                new float[] {contrast, 0, 0, 0, 0}, // scale red
                new float[] {0, contrast, 0, 0, 0}, // scale green
                new float[] {0, 0, contrast, 0, 0}, // scale blue
                new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
                new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

            var imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);

            var imgTemp = new Bitmap(this.originalImage.Width, this.originalImage.Height, PixelFormat.Format24bppRgb);
            imgTemp.SetResolution(this.originalImage.HorizontalResolution, this.originalImage.VerticalResolution);

            using (var g = Graphics.FromImage(imgTemp))
            {
                g.Clear(System.Drawing.Color.White);

                g.DrawImage(this.originalImage, new Rectangle(0, 0, this.originalImage.Width, this.originalImage.Height)
                    , 0, 0, this.originalImage.Width, this.originalImage.Height,
                    GraphicsUnit.Pixel, imageAttributes);
            }

            var gray = ImageUtilities.ConvertToIndexedFormat(
                imgTemp,
                ConvertBitmapFormat.Format8bppIndexed);            

            this.ScannedImage = gray;

            this.WriteBitmap();

            imgTemp.Dispose();
        }

        private void WriteBitmap()
        {
            var data = ImageUtilities.ConvertToByteArray(this.ScannedImage);

            this.displayedBitmap.WritePixels(
                new Int32Rect(0, 0, this.ScannedImage.Width, this.ScannedImage.Height),
                data,
                this.ScannedImage.Width,
                0);

        }
    }
}
