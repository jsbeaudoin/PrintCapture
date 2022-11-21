using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.Device.LivescanThales
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    using XL_ID.Utilities.Image;

    internal class IndexedImageEditor
    {
        private Bitmap image;

        private bool usePalette;

        private bool invertedImage;

        private int bmpStride;

        private int pixelFormatSize;

        private BitmapData bmpData;

        private Color baseColor;

        private Color[] palette;

        private int colorTolerance;

        private int paletteColorIndex;

        private byte[] bmpBytes;

        private Dictionary<Color, byte> colorDict;
       
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoCropper"/> class.
        /// </summary>
        /// <param name="baseImage">The base image.</param>        
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public IndexedImageEditor(Bitmap baseImage)
        {            
            this.baseColor = Color.White;
            this.colorTolerance = 30;
            this.image = baseImage;
        }

        public bool IsInEdit
        {
            get
            {
                return this.bmpData != null;
            }
        }

        public void Begin()
        {
            if (this.IsInEdit)
            {
                throw new ApplicationException("Cannot Begin - Already in progress");
            }
            this.LockData();
        }

        public void End()
        {
            if (!this.IsInEdit)
            {
                throw new ApplicationException("Cannot End - Not in progress");
            }

            this.UnlockData();
        }        

        public void DrawRectangle(Rectangle rect, Color color)
        {
            bool mustEnd = false;
            if (!this.IsInEdit)
            {
                mustEnd = true;
                this.Begin();
            }

            if (rect.X < 0 || rect.X >= this.image.Width || rect.Y < 0 || rect.Y >= this.image.Height ||
                rect.Right > this.image.Width || rect.Bottom > this.image.Height)
            {
                //throw new ApplicationException("Cannot draw rectangle outside of image bound");
                return;
            }

            

            //BitmapData bmpDat1 = this.image.LockBits(
            //   new Rectangle(0, 0, this.image.Width, this.image.Height), ImageLockMode.ReadWrite, this.image.PixelFormat);

            //try
            //{
            var scan0 = (long)this.bmpData.Scan0;
            var stride = this.bmpData.Stride;

                int x, y;
                IntPtr target;
                // fill horizontal lines
                for (x = rect.Left; x < rect.Right; x++)
                {
                    y = rect.Top;
                    target = (IntPtr)(scan0 + (this.bmpData.Stride * y) + (x * this.pixelFormatSize));
                    this.SetPixel(target, color);

                    y = rect.Bottom;
                    target = (IntPtr)(scan0 + (stride * y) + (x * this.pixelFormatSize));
                    this.SetPixel(target, color);
                }

                // fill vertical lines
                for (y = rect.Top; y < rect.Bottom; y++)
                {
                    x = rect.Left;
                    target = (IntPtr)(scan0 + (stride * y) + (x * this.pixelFormatSize));
                    this.SetPixel(target, color);

                    x = rect.Right;
                    target = (IntPtr)(scan0 + (stride * y) + (x * this.pixelFormatSize));
                    this.SetPixel(target, color);
                }

            //}
            
            //this.image.UnlockBits(bmpDat1);
            if (mustEnd)
            {
                this.End();
            }
            
        }

        public void DrawText(string text, int fontSize, int posX, int posY)
        {
            int sizeX = this.image.Width, sizeY = this.image.Height;            
            var bmp = new Bitmap(sizeX, sizeY);
            SizeF textSize;
            using (var g = Graphics.FromImage(bmp))
            {
                var font = new Font("Arial", fontSize);
                g.Clear(Color.White);

                textSize = g.MeasureString(text, font);

                g.DrawString(text, font, Brushes.Black, posX, posY);
                g.Flush();
            } 
            

            var maxX = Math.Min(this.image.Width, posX + textSize.Width);
            var maxY = Math.Min(this.image.Height, posY + textSize.Height);

            var list = new List<PointInfo>();
            for (int x = posX; x < maxX; x++)
            {
                for (int y = posY; y < maxY; y++)
                {
                    var col = bmp.GetPixel(x, y);
                    if (col.R != 255 && col.B != 255  && col.G != 255)
                    {
                        list.Add(new PointInfo{Color = Color.Black, X = x, Y = y});
                    }
                }
            }

            this.DrawPoints(list);            
            bmp.Dispose();
        }

        private void DrawPoints(IEnumerable<PointInfo> points)
        {
            bool mustEnd = false;
            if (!this.IsInEdit)
            {
                mustEnd = true;
                this.Begin();
            }

            var scan0 = (long)this.bmpData.Scan0;
            var stride = this.bmpData.Stride;

            // make sure no point is invalid
            foreach (var point in points)
            {
                if (point.X < 0 || point.X >= this.image.Width || point.Y < 0 || point.Y >= this.image.Height)
                {
                    throw new ApplicationException("Cannot draw points outside of image bound");
                }
                var target = (IntPtr)(scan0 + (stride * point.Y) + (point.X * this.pixelFormatSize));
                this.SetPixel(target, point.Color);
            }

            if (mustEnd)
            {
                this.End();
            }

        }

        /// <summary>
        /// Get the ratio of pixel considered not empty (White)
        /// </summary>
        /// <returns>Value between 0 and 100. Ratio of not empty pixel / Total image pixels</returns>
        public int GetAutoCropPixelRatio()
        {
            var pixelCount = this.GetAutoCropPixelCount();
            var totalCount = this.image.Width * this.image.Height;

            var ratio = (int)((float)(pixelCount / totalCount) * 100);

            return ratio;
        }

        /// <summary>
        /// Gets the count of pixel considered not empty (White)
        /// </summary>
        /// <returns>Pixel count not empty (White)</returns>
        public int GetAutoCropPixelCount()
        {
            bool mustEnd = false;
            if (!this.IsInEdit)
            {
                mustEnd = true;
                this.Begin();
            }

            var pixelCount = this.GetCropPixelCount();            

            if (mustEnd)
            {
                this.End();
            }

            return pixelCount;
        }

        /// <summary>
        /// Gets the auto crop rectangle.
        /// </summary>
        /// <returns></returns>
        public Rectangle GetAutoCropRectangle()
        {
            bool mustEnd = false;
            if (!this.IsInEdit)
            {
                mustEnd = true;
                this.Begin();
            }

            var croppedRect = this.GetCropRectangle();

            if (mustEnd)
            {
                this.End();
            }

            return croppedRect;
        }
        
        private Rectangle GetOffset(Size targetSize, Rectangle croppedRect, ImageVertAlignment vertAlign, ImageHorAlignment horAlign, out int offsetX, out int offsetY)
        {            
            switch (horAlign)
            {
                case ImageHorAlignment.Center:
                    offsetX = targetSize.Width / 2 - croppedRect.Width / 2;
                    break;
                case ImageHorAlignment.Right:
                    offsetX = targetSize.Width - croppedRect.Width;
                    break;
                default: // ImageHorAlignment.Left, none
                    offsetX = 0;
                    break;
            }

            switch (vertAlign)
            {
                case ImageVertAlignment.Middle:
                    offsetY = targetSize.Height / 2 - croppedRect.Height / 2;
                    break;
                case ImageVertAlignment.Bottom:
                    offsetY = targetSize.Height - croppedRect.Height;
                    break;
                default: // ImageVertAlignment.Top, None
                    offsetY = 0;
                    break;
            }                                    

            var targetRect = new Rectangle(offsetX, offsetY, croppedRect.Width, croppedRect.Height);

            return targetRect;
        }



        private void LockData()
        {
            this.usePalette = this.image.PixelFormat == PixelFormat.Format8bppIndexed
                              || this.image.PixelFormat == PixelFormat.Format1bppIndexed
                              || this.image.PixelFormat == PixelFormat.Format4bppIndexed;
            this.palette = this.image.Palette.Entries;

            this.bmpData = this.image.LockBits(
                new Rectangle(0, 0, this.image.Width, this.image.Height), ImageLockMode.ReadOnly, this.image.PixelFormat);
            this.bmpStride = Math.Abs(this.bmpData.Stride);
            this.invertedImage = this.bmpStride != this.bmpData.Stride;
            this.pixelFormatSize = Image.GetPixelFormatSize(this.image.PixelFormat) / 8;

            this.SetBmpBytes();
            
            this.colorDict = new Dictionary<Color, byte>();
        }

        private void UnlockData()
        {
            this.image.UnlockBits(this.bmpData);

            this.colorDict = null;
            this.bmpData = null;
            this.bmpBytes = null;
        }

        private void SetBmpBytes()
        {
            this.bmpBytes = new byte[this.bmpStride * this.image.Height];
            if (!this.invertedImage)
            {
                Marshal.Copy(this.bmpData.Scan0, this.bmpBytes, 0, this.bmpBytes.Length);
            }
            else
            {
                var height1 = this.image.Height - 1;
                var stride = Math.Abs(this.bmpData.Stride);
                var ptr0 = new IntPtr(this.bmpData.Scan0.ToInt32() - (stride * height1));

                for (var row = 0; row <= height1; row++)
                {
                    var rowAdr = new IntPtr(ptr0.ToInt32() + (height1 - row) * stride);
                    Marshal.Copy(rowAdr, this.bmpBytes, row * stride, stride);
                }
            }            
        }

        /// <summary>
        /// Gets the crop rectangle.
        /// </summary>
        /// <returns>Crop coordinates</returns>
        private Rectangle GetCropRectangle()
        {
            var colorToleranceLevel = (byte)((this.colorTolerance / 100.0) * byte.MaxValue);
            var colorRange = new ColorRange(this.baseColor, colorToleranceLevel);

            int bitmapWidth = this.image.Width;
            int bitmapHeight = this.image.Height;

            var left = -1;
            var right = -1;
            var top = -1;
            var bottom = -1;

            //var w = new System.Diagnostics.Stopwatch();
            //w.Start();

            // find top
            for (int y = 0; y < bitmapHeight && top == -1; y++)
            {
                for (int x = 0; x < bitmapWidth && top == -1; x++)
                {
                    if (!this.IsNear(x, y, colorRange))
                    {
                        top = y;
                    }
                }
            }

            if (top == -1) {return Rectangle.Empty;}

            // find left
            for (int x = 0; x < bitmapWidth && left == -1; x++)
            {
                for (int y = top; y < bitmapHeight && left == -1; y++)
                {
                    if (!this.IsNear(x, y, colorRange))
                    {
                        left = x;
                    }
                }
            }

            if (left == -1) { return Rectangle.Empty; }

            // find right
            for (int x = bitmapWidth - 1; x > left && right == -1; x--)
            {
                for (int y = top; y < bitmapHeight && right == -1; y++)
                {
                    if (!this.IsNear(x, y, colorRange))
                    {
                        right = x;
                    }
                }
            }

            if (right == -1) { return Rectangle.Empty; }

            // find bottom
            for (int y = bitmapHeight - 1; y > top && bottom == -1; y--)
            {
                for (int x = left; x <= right && bottom == -1; x++)
                {
                    if (!this.IsNear(x, y, colorRange))
                    {
                        bottom = y;
                    }
                }
            }

            if (bottom == -1) { return Rectangle.Empty; }

            //w.Stop();
            //Console.WriteLine("Crop Zone found in : " + w.ElapsedMilliseconds.ToString() + " ms");

            return new Rectangle(left, top, right - left + 1, bottom - top + 1);
        }

        private int GetCropPixelCount()
        {           
            var colorToleranceLevel = (byte)((this.colorTolerance / 100.0) * byte.MaxValue);
            var colorRange = new ColorRange(this.baseColor, colorToleranceLevel);

            int bitmapWidth = this.image.Width;
            int bitmapHeight = this.image.Height;

            var rectToSearch = this.GetCropRectangle();
            var result = 0;

            if (rectToSearch.IsEmpty)
            {
                return result;
            }

            for (int y = rectToSearch.Top; y < rectToSearch.Bottom; y++)
            {
                for (int x = rectToSearch.Left; x < rectToSearch.Right; x++)
                {
                    if (!this.IsNear(x, y, colorRange))
                    {
                        result++;
                    }
                }
            }            
            
            return result;
        
        }

        private void SetPixelToPaletteIndex(IntPtr pointer, byte paletteIndex)
        {
            var data = new byte[] { paletteIndex };
            Marshal.Copy(data, 0, pointer, data.Length);            
        }

        private void SetPixel(IntPtr pointer, Color color)
        {
            byte[] data;
            if (this.usePalette)
            {
                if (colorDict.ContainsKey(color))
                {
                    data = new[] { colorDict[color] };
                }
                else
                {
                    var targetPaletteIndex = (byte)this.GetPaletteColorIndex(color);
                    data = new[] { targetPaletteIndex };
                    colorDict.Add(color, targetPaletteIndex);
                }                
            }
            else if (this.pixelFormatSize == 3)
            {
                data = new[] { color.B, color.G, color.R };                
            }
            else if (this.pixelFormatSize == 4)
            {
                data = new[] { color.B, color.G, color.R, color.A };                  
            }
            else
            {
                throw new ApplicationException("Unknown Image Information");
            }

            Marshal.Copy(data, 0, pointer, data.Length);
        }

        private int GetPaletteColorIndex(Color color)
        {
            int colorValue = color.ToArgb();

            if (this.paletteColorIndex > 0 && this.palette[this.paletteColorIndex].ToArgb() == colorValue)
            {
                return this.paletteColorIndex;
            }

            int length = this.palette.Length;
            int lastColorValue = -2;
            int currentColorValue = -2;
            int indexToInsert = -1;

            for (int index = 0; index < length; index++)
            {
                lastColorValue = currentColorValue;
                currentColorValue = this.palette[index].ToArgb();
                if (currentColorValue == colorValue)
                {
                    this.paletteColorIndex = index;
                    return index;
                }                
            }

            return 1;

            // if palette entries have unused entries, use one for wanted color
            if (indexToInsert == -1 || lastColorValue != currentColorValue)
            {
                throw new ApplicationException("Image palette does not contain Color " + color);
            }
            else
            {
                // Modify the palette
                ColorPalette palette = this.image.Palette;
                Color[] entries = palette.Entries;

                entries[indexToInsert] = color;
                this.image.Palette = palette; // The crucial statement

                this.paletteColorIndex = indexToInsert;

                return this.paletteColorIndex;
            }
        }

        private void SetPaletteIndex(int arrayposition, int index)
        {
            var byteData = BitConverter.GetBytes(index);

            for (var x = 0; x < this.pixelFormatSize; x++)
            {
                this.bmpBytes[arrayposition + x] = byteData[x];
            }
        }

        private bool IsNear(int x, int y, ColorRange colorRange)
        {
            var adr = this.GetArrayPosition(x, y);
            var pixelColor = this.GetColor(adr);
            return colorRange.IsNear(pixelColor);

            // Simple method, but no garantee
            // return (this.bmpBytes[adr] > 240);

        }

        /// <summary>
        /// Gets the array position of the x y coordinates of an image.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="otherSize">Size to consider.</param>
        /// <returns></returns>
        private int GetArrayPosition(int x, int y, Size otherSize)
        {
            var stride = otherSize.Width % 4 + otherSize.Width;
            //if (invertedImage)
            //{
            //    var vert = stride * (otherSize.Height - 1 - y) * this.pixelFormatSize;
            //    var hor = (otherSize.Width - 1 - x) * this.pixelFormatSize;
            //    return vert + hor;
            //}
            //else
            //{
                var vert = y * stride * this.pixelFormatSize;
                var hor = x * this.pixelFormatSize;
                return vert + hor;
            //}
        }

        /// <summary>
        /// Gets the array position of the x y coordinates of an image.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        private int GetArrayPosition(int x, int y)
        {
            //if (this.invertedImage)
            //{
            //    var vert = this.bmpStride * (this.bmpData.Height - 1 - y);
            //    var hor = (this.bmpData.Width - 1 - x) * this.pixelFormatSize;
            //    return vert + hor;
            //}
            //else
            //{
                var vert = y * this.bmpStride;
                var hor = x * this.pixelFormatSize;
                return vert + hor;
            //}
        }

        private Color GetColor(int arrayIndex)
        {
            var pos = arrayIndex;
            if (this.usePalette)
            {
                var paletteIndex = 0;
                if (this.pixelFormatSize > 1)
                {
                    paletteIndex = GetPaletteIndex(arrayIndex);
                }
                else
                {
                    paletteIndex = this.bmpBytes[arrayIndex];
                }

                return this.palette[paletteIndex];
            }
            else if (this.pixelFormatSize == 3)
            {
                byte pixelB = this.bmpBytes[pos];
                byte pixelR = this.bmpBytes[pos + 2];
                byte pixelG = this.bmpBytes[pos + 1];
                return Color.FromArgb(pixelR, pixelG, pixelB);
            }
            else if (this.pixelFormatSize == 4)
            {
                byte alphaChannel = this.bmpBytes[pos + 3];
                byte pixelB = this.bmpBytes[pos + 0];
                byte pixelR = this.bmpBytes[pos + 2];
                byte pixelG = this.bmpBytes[pos + 1];
                return Color.FromArgb(alphaChannel, pixelR, pixelG, pixelB);
            }

            throw new ApplicationException("Unknown Image Information");
        }

        private int GetPaletteIndex(int arrayIndex)
        {
            var byteData = new byte[4];

            for (var x = 0; x < this.pixelFormatSize; x++)
            {
                byteData[x] = this.bmpBytes[arrayIndex + x];
            }

            return BitConverter.ToInt32(byteData, 0);
        }

        /// <summary>
        /// Class to have a Min and Max value for color components
        /// </summary>
        private class ByteRange
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ByteRange"/> class.
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="tolerance">The tolerance.</param>
            public ByteRange(byte value, byte tolerance)
            {
                if (value + tolerance > byte.MaxValue)
                {
                    this.Max = byte.MaxValue;
                }
                else
                {
                    this.Max = (byte)(value + tolerance);
                }

                if (value - tolerance < byte.MinValue)
                {
                    this.Min = byte.MinValue;
                }
                else
                {
                    this.Min = (byte)(value - tolerance);
                }
            }

            /// <summary>
            /// Gets the minimum.
            /// </summary>
            /// <value>
            /// The minimum.
            /// </value>
            public byte Min { get; private set; }

            /// <summary>
            /// Gets the maximum.
            /// </summary>
            /// <value>
            /// The maximum.
            /// </value>
            public byte Max { get; private set; }
        }

        /// <summary>
        /// Color Range manager
        /// </summary>
        private class ColorRange
        {
            /// <summary>
            /// The Range for the blue color component
            /// </summary>
            private ByteRange b;

            /// <summary>
            /// The Range for the red color component
            /// </summary>
            private ByteRange r;

            /// <summary>
            /// The Range for the green color component
            /// </summary>
            private ByteRange g;

            /// <summary>
            /// Initializes a new instance of the <see cref="ColorRange"/> class.
            /// </summary>
            /// <param name="color">The color.</param>
            /// <param name="tolerance">The tolerance.</param>
            public ColorRange(Color color, byte tolerance)
            {
                this.b = new ByteRange(color.B, tolerance);
                this.r = new ByteRange(color.R, tolerance);
                this.g = new ByteRange(color.G, tolerance);
            }

            /// <summary>
            /// Determines whether the specified color is near enough.
            /// </summary>
            /// <param name="color">The color.</param>
            /// <returns>Boolean to indicate if the color is considered near enough.</returns>
            public bool IsNear(Color color)
            {
                if (color.R >= this.r.Min && color.R <= this.r.Max && color.G >= this.g.Min && color.G <= this.g.Max
                    && color.B >= this.b.Min && color.B <= this.b.Max)
                {
                    return true;
                }

                return false;
            }
        }

        private class PointInfo
        {
            public int X { get; set; }
            public int Y { get; set; }
            public Color Color { get; set; }

        }
    }
}

